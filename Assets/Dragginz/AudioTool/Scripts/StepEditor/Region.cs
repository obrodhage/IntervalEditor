using System;
using System.Collections.Generic;
using Dragginz.AudioTool.Scripts.DataModels;
using Dragginz.AudioTool.Scripts.Includes;
using Dragginz.AudioTool.Scripts.ScriptableObjects;
using Dragginz.AudioTool.Scripts.StepEditor.UI;
using UnityEngine;
using static Dragginz.AudioTool.Scripts.Includes.Globals;

namespace Dragginz.AudioTool.Scripts.StepEditor
{
    public class Region
    {
        private Action<int, int> _callbackRegionClick;

        public int StartPosBeats;
        public int Beats;
        public int TrackPos;
        public string InstrumentName;
        
        public InstrumentSettings PlaybackSettings;

        public float RegionStartTime;
        public float RegionEndTime;

        public bool IsPlaying;
        
        public List<PianoRoll> PianoRoll { get; private set; }

        public UiRegion RegionUi { get; private set; }

        public void Init(int pos, int numBeats, Track track, float beatsPerSec)
        {
            StartPosBeats = pos;
            Beats = numBeats;
            TrackPos = track.Position;
            InstrumentName = track.Instrument.name;
            
            RegionStartTime = StartPosBeats / beatsPerSec;
            RegionEndTime = RegionStartTime + (Beats / beatsPerSec);
            //Debug.Log("regionStartTime, regionEndTime: "+regionStartTime+", "+regionEndTime);
            
            SetDefaultPlaybackSettings(track.Instrument);
        }
    
        public void SetChordData(int k, int i, int o)
        {
            PlaybackSettings.Key = k;
            PlaybackSettings.Interval = i;
            PlaybackSettings.Octave = o;
        }
        
        public void SetArpData(DataArpeggiator dataArp)
        {
            PlaybackSettings.ArpData.Octaves = dataArp.octaves;
            PlaybackSettings.ArpData.Start = dataArp.start;
            PlaybackSettings.ArpData.End = dataArp.end;
            PlaybackSettings.ArpData.Type = dataArp.type;
        }
        
        public void SetMelodyData(DataMelodyMaker dataMelody)
        {
            PlaybackSettings.MelodyData.Octaves = dataMelody.octaves;
            PlaybackSettings.MelodyData.Mode = dataMelody.mode;
            PlaybackSettings.MelodyData.End = dataMelody.end;
            PlaybackSettings.MelodyData.Type = dataMelody.type;
        }
        
        public void UpdateLength(int length, float beatsPerSec)
        {
            Beats = length;
            RegionStartTime = StartPosBeats / beatsPerSec;
            RegionEndTime = RegionStartTime + (Beats / beatsPerSec);
        }

        public void UpdateStartPos(int pos, float beatsPerSec)
        {
            StartPosBeats = pos;
            RegionStartTime = StartPosBeats / beatsPerSec;
            RegionEndTime = RegionStartTime + (Beats / beatsPerSec);
        }
        
        public void CreatePianoRoll(List<List<int>> intervals)
        {
            PianoRoll?.Clear();
            
            if (PlaybackSettings.Type == (int)InstrumentType.Arpeggiator)
            {
                PianoRoll = CreateArpPianoRoll(intervals);
            }
            else if (PlaybackSettings.Type == (int)InstrumentType.MelodyMaker)
            {
                PianoRoll = CreateMelodyPianoRoll(intervals);
            }
            else if (PlaybackSettings.Type != (int)InstrumentType.Looper)
            {
                PianoRoll = CreateChordPianoRoll(intervals);
            }
        }

        //
        // Chord
        //
        
        private List<PianoRoll> CreateChordPianoRoll(IReadOnlyList<List<int>> intervals)
        {
            var pianoRoll = new List<PianoRoll>();

            var isOneShot = ChordNotesPerBar[PlaybackSettings.Note] == 0;
            
            var noteInterval = 60f / 120.0f * 4f / ChordNotesPerBar[PlaybackSettings.Note];
            var curPosTime = RegionStartTime;
            
            var listInterval = intervals[PlaybackSettings.Interval];
            var numIntervalNotes = listInterval.Count;
            
            var intervalStartIndex = (PlaybackSettings.Octave * 12) + PlaybackSettings.Key;
            
            // play chord until region has reached end
            while (curPosTime < RegionEndTime)
            {
                for (var i = 0; i < numIntervalNotes; ++i)
                {
                    if (!PlaybackSettings.HighOctave && listInterval[i] == 12) continue;
                    
                    var n = new OneNote
                    {
                        Index = intervalStartIndex + listInterval[i]
                    };
                    var p = new PianoRoll
                    {
                        PosTime = curPosTime,
                        Notes = new List<OneNote> {n}
                    };
                    pianoRoll.Add(p);

                    if (PlaybackSettings.RootNoteOnly && i == 0) break;
                }

                curPosTime += noteInterval;

                if (isOneShot) break;
            }

            return pianoRoll;
        }

        //
        // Arpeggiator
        //
        
        private List<PianoRoll> CreateArpPianoRoll(IReadOnlyList<List<int>> intervals)
        {
            var pianoRoll = new List<PianoRoll>();

            var noteInterval = 60f / 120.0f * 4f / ArpeggiatorNotesPerBar[PlaybackSettings.Note]; // ArpeggiatorNotesPerBar is set in Globals
            PlaybackSettings.ArpData.NoteInterval = noteInterval;
            
            PlaybackSettings.ArpData.StartTime = RegionStartTime;
            PlaybackSettings.ArpData.EndTime = RegionEndTime;
            
            var intervalStartIndex = (PlaybackSettings.Octave * 12) + PlaybackSettings.Key;

            var listIntervals = intervals[PlaybackSettings.Interval];
            var numIntervals = listIntervals.Count;

            // create list of all interval notes
            var listPattern = new List<int>();
            var numOctaves = PlaybackSettings.ArpData.Octaves + 1;
            for (var octave = 0; octave < numOctaves; ++octave)
            {
                for (var i = 0; i < numIntervals; ++i)
                {
                    var index = intervalStartIndex + (octave * 12) + listIntervals[i];
                    
                    if (i == 0 && octave > 0) {
                        if (index == listPattern[^1]) continue; // avoid double notes when octave changes
                    }
                    
                    listPattern.Add(index);
                }
            }
            var s = "";
            foreach (var note in listPattern) s += note + ", ";
            Debug.Log("CreateArpPianoRoll: "+s);
            
            PlaybackSettings.ArpData.ListPattern = listPattern;
            
            //Debug.Log("Track "+trackPos+", region "+startPosBeats+$" -> arpeggiator pattern: {string.Join(",", listPattern.ToArray())}");

            return PlaybackSettings.ArpData.Type switch
            {
                (int)ArpType.SimpleArpeggio => Arpeggiator.CreateArpSimple(PlaybackSettings.ArpData),
                (int)ArpType.ThreeNoteSteps => Arpeggiator.CreateArpThreeNoteSteps(PlaybackSettings.ArpData),
                (int)ArpType.RootClimb => Arpeggiator.CreateArpRootClimb(PlaybackSettings.ArpData),
                _ => pianoRoll
            };
        }
        
        //
        // Melody Maker
        //
        
        private List<PianoRoll> CreateMelodyPianoRoll(IReadOnlyList<List<int>> intervals)
        {
            var pianoRoll = new List<PianoRoll>();

            var noteInterval = 60f / 120.0f * 4f / MelodyNotesPerBar[PlaybackSettings.Note]; // MelodyNotesPerBar is set in Globals
            PlaybackSettings.MelodyData.NoteInterval = noteInterval;
            
            PlaybackSettings.MelodyData.StartTime = RegionStartTime;
            PlaybackSettings.MelodyData.EndTime = RegionEndTime;
            
            var intervalStartIndex = (PlaybackSettings.Octave * 12) + PlaybackSettings.Key;

            var listIntervals = MajorScale;
            var numIntervals = listIntervals.Length;

            // create list of all interval notes
            var listPattern = new List<int>();
            var numOctaves = PlaybackSettings.MelodyData.Octaves + 1;
            for (var octave = 0; octave < numOctaves; ++octave)
            {
                for (var i = 0; i < numIntervals; ++i)
                {
                    var index = intervalStartIndex + (octave * 12) + listIntervals[i];
                    
                    if (i == 0 && octave > 0) {
                        if (index == listPattern[^1]) continue; // avoid double notes when octave changes
                    }
                    
                    listPattern.Add(index);
                }
            }
            var s = "";
            foreach (var note in listPattern) s += note + ", ";
            Debug.Log("CreateMelodyPianoRoll: "+s);
            
            PlaybackSettings.MelodyData.ListPattern = listPattern;
            
            //Debug.Log("Track "+trackPos+", region "+startPosBeats+$" -> arpeggiator pattern: {string.Join(",", listPattern.ToArray())}");

            return PlaybackSettings.MelodyData.Type switch
            {
                (int)MelodyType.MajorScale => MelodyMaker.CreateMajorScale(PlaybackSettings.MelodyData),
                _ => pianoRoll
            };
        }
        
        //
        
        private void SetDefaultPlaybackSettings(ScriptableObjectInstrument instrument, bool changeKeys = true)
        {
            PlaybackSettings = new InstrumentSettings()
            {
                Instrument = instrument,
                Key = (changeKeys ? 0 : PlaybackSettings.Key),
                Interval = (changeKeys ? 0 : PlaybackSettings.Interval),
                Octave = (changeKeys ? 0 : PlaybackSettings.Octave),
                Type = (instrument.type == InstrumentType.Looper) ? (int)InstrumentType.Looper : 0,
                Note = 0,
                ArpData = new ArpeggiatorData(),
                MelodyData = new MelodyMakerData(),
                HighOctave = instrument.highOctave,
                RootNoteOnly = instrument.rootNoteOnly,
                Volume = instrument.defaultVolume,
                Pan = instrument.defaultPan
            };
        }
        
        public void SetListener(UiRegion regionUi, Action<int, int> callbackRegionClick)
        {
            RegionUi = regionUi;
            _callbackRegionClick = callbackRegionClick;
            
            RegionUi.OnClickRegionEvent += OnRegionClick;
        }

        public void UpdateTrackPosition(int pos)
        {
            TrackPos = pos;
            if (RegionUi == null) return;
            
            var v2Pos = RegionUi.rectTransform.anchoredPosition;
            v2Pos.y = TrackPos * PrefabTrackHeight * -1;
            RegionUi.rectTransform.anchoredPosition = v2Pos;
        }
        
        public void UpdateInstrument(ScriptableObjectInstrument instrument)
        {
            InstrumentName = instrument.name;
            SetDefaultPlaybackSettings(instrument, false);

            if (RegionUi != null) RegionUi.UpdateInstrument(instrument.defaultColor);
        }

        public void Mute(bool muted)
        {
            if (RegionUi != null) RegionUi.ShowMuted(muted);
        }
        
        public void Remove()
        {
            if (RegionUi == null) return;
            
            RegionUi.OnClickRegionEvent -= OnRegionClick;
            _callbackRegionClick = null;

            RegionUi.Remove();
        }
        
        // EVENTS

        private void OnRegionClick()
        {
            if (_callbackRegionClick != null) _callbackRegionClick.Invoke(TrackPos, StartPosBeats);
        }
    }
}
