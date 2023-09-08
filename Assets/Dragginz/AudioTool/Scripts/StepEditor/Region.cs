using System;
using System.Collections.Generic;
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

        public int startPosBeats;
        public int beats;
        public int trackPos;
        public string instrumentName;
        
        public InstrumentSettings playbackSettings;

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
        }
    
        public void SetChordData(int k, int i, int o)
        {
            playbackSettings.Key = k;
            playbackSettings.Interval = i;
            playbackSettings.Octave = o;
        }

        public void SetPatternData(int p, int n)
        {
            playbackSettings.Pattern = p;
            playbackSettings.Note = n;
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
        
        public void CreatePianoRoll(List<List<int>> intervals, List<List<Vector2>> patterns)
        {
            PianoRoll?.Clear();

            if (playbackSettings.Type == RegionTypeChord)
            {
                PianoRoll = CreateChordPianoRoll(intervals);
            }
            else if (playbackSettings.Type == RegionTypeArpeggiator)
            {
                PianoRoll = CreateArpeggiatorPianoRoll(intervals, patterns);
            }
        }

        private List<PianoRoll> CreateChordPianoRoll(List<List<int>> intervals)
        {
            var pianoRoll = new List<PianoRoll>();

            var isOneShot = ChordNotesPerBar[playbackSettings.Note] == 0;
            
            var noteInterval = 60f / 120.0f * 4f / ChordNotesPerBar[playbackSettings.Note];
            var curPosTime = regionStartTime;
            
            var listInterval = intervals[playbackSettings.Interval];
            var numIntervalNotes = listInterval.Count;
            
            // play chord until region has reached end
            while (curPosTime < regionEndTime)
            {
                for (var i = 0; i < numIntervalNotes; ++i)
                {
                    if (!playbackSettings.HighOctave && listInterval[i] == 12) continue;
                    
                    var n = new Note
                    {
                        //       root octave                     Key                    interval index
                        Index = (playbackSettings.Octave * 12) + playbackSettings.Key + listInterval[i]
                    };
                    //Debug.Log("index: "+n.index);
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

        private List<PianoRoll> CreateArpeggiatorPianoRoll(List<List<int>> intervals, List<List<Vector2>> patterns)
        {
            var pianoRoll = new List<PianoRoll>();

            var noteInterval = 60f / 120.0f * 4f / ArpeggiatorNotesPerBar[playbackSettings.Note];
            var curPosTime = regionStartTime;
            
            var listInterval = intervals[playbackSettings.Interval];
            var listPattern = patterns[playbackSettings.Pattern];
            
            var numPatternNotes = listPattern.Count;
            
            // play pattern until region has reached end
            while (curPosTime < regionEndTime)
            {
                for (var i = 0; i < numPatternNotes; ++i)
                {
                    var n = new Note
                    {
                        //       root octave                     Key                                       interval index             octave index
                        Index = (playbackSettings.Octave * 12) + playbackSettings.Key + listInterval[(int) listPattern[i].y] + ((int) listPattern[i].x * 12)
                    };
                    //Debug.Log("index: "+n.index);
                    var p = new PianoRoll
                    {
                        PosTime = curPosTime,
                        Notes = new List<Note> {n}
                    };
                    pianoRoll.Add(p);

                    curPosTime += noteInterval;

                    if (curPosTime >= regionEndTime) break;
                }
            }
            
            return pianoRoll;
        }

        private void SetDefaultPlaybackSettings(ScriptableObjectInstrument instrument, bool changeKeys = true)
        {
            playbackSettings = new InstrumentSettings()
            {
                Instrument = instrument,
                Key = (changeKeys ? 0 : playbackSettings.Key),
                Interval = (changeKeys ? 0 : playbackSettings.Interval),
                Octave = (changeKeys ? 0 : playbackSettings.Octave),
                Type = (instrument.type == InstrumentType.Looper) ? 2 : 0,
                Pattern = 0,
                Note = 0,
                CanLoop = instrument.type == InstrumentType.Looper,
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
