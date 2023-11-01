using System;
using System.Collections.Generic;
using Dragginz.AudioTool.Scripts.DataModels;
using Dragginz.AudioTool.Scripts.ScriptableObjects;
using Dragginz.AudioTool.Scripts.StepEditor.UI;
using UnityEngine;
using static Dragginz.AudioTool.Scripts.Includes.Globals;

namespace Dragginz.AudioTool.Scripts.StepEditor
{
    public class Region
    {
        private Action<int, int> _callbackRegionClick;

        public int startPosBeats;
        public int beats;
        public int trackPos;
        public string instrumentName;
        
        public InstrumentSettings playbackSettings;

        //public ArpeggiatorData arpeggiatorData;
        
        public float regionStartTime;
        public float regionEndTime;

        public bool isPlaying;
        
        public List<PianoRoll> PianoRoll { get; private set; }

        public UiRegion RegionUi { get; private set; }

        public void Init(int pos, int numBeats, Track track, float beatsPerSec)
        {
            startPosBeats = pos;
            beats = numBeats;
            trackPos = track.Position;
            instrumentName = track.Instrument.name;
            
            regionStartTime = startPosBeats / beatsPerSec;
            regionEndTime = regionStartTime + (beats / beatsPerSec);
            //Debug.Log("regionStartTime, regionEndTime: "+regionStartTime+", "+regionEndTime);
            
            SetDefaultPlaybackSettings(track.Instrument);

            //arpeggiatorData = new ArpeggiatorData();
        }
    
        public void SetChordData(int k, int i, int o)
        {
            playbackSettings.Key = k;
            playbackSettings.Interval = i;
            playbackSettings.Octave = o;
        }

        /*public void SetPatternData(int n)
        {
            playbackSettings.Note = n;
        }*/

        public void SetArpData(DataArpeggiator dataArp)
        {
            playbackSettings.arpData.octaves = dataArp.octaves;
            playbackSettings.arpData.start = dataArp.start;
            playbackSettings.arpData.end = dataArp.end;
            playbackSettings.arpData.type = dataArp.type;
        }
        
        public void UpdateLength(int length, float beatsPerSec)
        {
            beats = length;
            regionStartTime = startPosBeats / beatsPerSec;
            regionEndTime = regionStartTime + (beats / beatsPerSec);
        }

        public void UpdateStartPos(int pos, float beatsPerSec)
        {
            startPosBeats = pos;
            regionStartTime = startPosBeats / beatsPerSec;
            regionEndTime = regionStartTime + (beats / beatsPerSec);
        }
        
        public void CreatePianoRoll(List<List<int>> intervals)
        {
            PianoRoll?.Clear();
            
            if (playbackSettings.Type == (int)InstrumentType.Arpeggiator)
            {
                PianoRoll = CreateArpPianoRoll(intervals);
            }
            else if (playbackSettings.Type != (int)InstrumentType.Looper)
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

            var isOneShot = ChordNotesPerBar[playbackSettings.Note] == 0;
            
            var noteInterval = 60f / 120.0f * 4f / ChordNotesPerBar[playbackSettings.Note];
            var curPosTime = regionStartTime;
            
            var listInterval = intervals[playbackSettings.Interval];
            var numIntervalNotes = listInterval.Count;
            
            var intervalStartIndex = (playbackSettings.Octave * 12) + playbackSettings.Key;
            
            // play chord until region has reached end
            while (curPosTime < regionEndTime)
            {
                for (var i = 0; i < numIntervalNotes; ++i)
                {
                    if (!playbackSettings.HighOctave && listInterval[i] == 12) continue;
                    
                    var n = new Note
                    {
                        Index = intervalStartIndex + listInterval[i]
                    };
                    var p = new PianoRoll
                    {
                        PosTime = curPosTime,
                        Notes = new List<Note> {n}
                    };
                    pianoRoll.Add(p);

                    if (playbackSettings.RootNoteOnly && i == 0) break;
                }

                curPosTime += noteInterval;

                if (isOneShot) break;
            }

            return pianoRoll;
        }

        //
        // Arpeggiator
        //
        
        private List<PianoRoll> CreateArpPianoRoll(IReadOnlyList<List<int>> intervals) //, ScriptableObjectPattern soPattern)
        {
            var pianoRoll = new List<PianoRoll>();

            var noteInterval = 60f / 120.0f * 4f / ArpeggiatorNotesPerBar[playbackSettings.Note]; // ArpeggiatorNotesPerBar is set in Globals
            playbackSettings.arpData.noteInterval = noteInterval;
            
            playbackSettings.arpData.startTime = regionStartTime;
            playbackSettings.arpData.endTime = regionEndTime;
            
            var intervalStartIndex = (playbackSettings.Octave * 12) + playbackSettings.Key;

            var listIntervals = intervals[playbackSettings.Interval];
            var numIntervals = listIntervals.Count;

            // create list of all interval notes
            var listPattern = new List<int>();
            var numOctaves = playbackSettings.arpData.octaves + 1;
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

            //arpeggiatorData.soPattern = soPattern;
            playbackSettings.arpData.listPattern = listPattern;
            

            //Debug.Log("Track "+trackPos+", region "+startPosBeats+$" -> arpeggiator pattern: {string.Join(",", listPattern.ToArray())}");

            return playbackSettings.arpData.type switch
            {
                (int)ArpType.SimpleArpeggio => Arpeggiator.CreateArpSimple(playbackSettings.arpData),
                (int)ArpType.ThreeNoteSteps => Arpeggiator.CreateArpThreeNoteSteps(playbackSettings.arpData),
                (int)ArpType.RootClimb => Arpeggiator.CreateArpRootClimb(playbackSettings.arpData),
                _ => pianoRoll
            };
        }
        
        //
        
        private void SetDefaultPlaybackSettings(ScriptableObjectInstrument instrument, bool changeKeys = true)
        {
            playbackSettings = new InstrumentSettings()
            {
                Instrument = instrument,
                Key = (changeKeys ? 0 : playbackSettings.Key),
                Interval = (changeKeys ? 0 : playbackSettings.Interval),
                Octave = (changeKeys ? 0 : playbackSettings.Octave),
                Type = (instrument.type == InstrumentType.Looper) ? 3 : 0,
                Note = 0,
                arpData = new ArpeggiatorData(),
                //CanLoop = instrument.type == InstrumentType.Looper,
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
            trackPos = pos;
            if (RegionUi == null) return;
            
            var v2Pos = RegionUi.rectTransform.anchoredPosition;
            v2Pos.y = trackPos * PrefabTrackHeight * -1;
            RegionUi.rectTransform.anchoredPosition = v2Pos;
        }
        
        public void UpdateInstrument(ScriptableObjectInstrument instrument)
        {
            instrumentName = instrument.name;
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
            if (_callbackRegionClick != null) _callbackRegionClick.Invoke(trackPos, startPosBeats);
        }
    }
}
