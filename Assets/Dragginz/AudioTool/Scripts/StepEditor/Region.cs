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

        public ArpeggiatorData arpeggiatorData;
        
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

            arpeggiatorData = new ArpeggiatorData();
        }
    
        public void SetChordData(int k, int i, int o)
        {
            playbackSettings.Key = k;
            playbackSettings.Interval = i;
            playbackSettings.Octave = o;
        }

        public void SetPatternData(int p, int n)
        {
            //playbackSettings.Pattern = p;
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
        
        public void CreatePianoRoll(List<List<int>> intervals, List<ScriptableObjectPattern> soPatterns)
        {
            PianoRoll?.Clear();
            
            if (playbackSettings.Type == (int)InstrumentType.Arpeggiator)
            {
                PianoRoll = CreateArpPianoRoll(intervals); //, soPatterns[playbackSettings.Pattern]);
            }
            else
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
            arpeggiatorData.noteInterval = noteInterval;
            
            arpeggiatorData.startTime = regionStartTime;
            arpeggiatorData.endTime = regionEndTime;
            
            var intervalStartIndex = (playbackSettings.Octave * 12) + playbackSettings.Key;

            var listIntervals = intervals[playbackSettings.Interval];
            var numIntervals = listIntervals.Count;

            // create list of all interval notes
            var listPattern = new List<int>();
            var numOctaves = arpeggiatorData.octave + 1;
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
            arpeggiatorData.listPattern = listPattern;
            

            Debug.Log("Track "+trackPos+", region "+startPosBeats+$" -> arpeggiator pattern: {string.Join(",", listPattern.ToArray())}");

            return arpeggiatorData.type switch
            {
                (int)ArpType.SimpleArpeggio => Arpeggiator.CreateArpSimple(arpeggiatorData),
                (int)ArpType.ThreeNoteSteps => Arpeggiator.CreateArpThreeNoteSteps(arpeggiatorData),
                (int)ArpType.RootClimb => pianoRoll,
                _ => pianoRoll
            };
        }
        
        /*private List<PianoRoll> CreateArpSimple(ScriptableObjectPattern pattern, float noteInterval, IReadOnlyList<int> listPattern)
        {
            var pianoRoll = new List<PianoRoll>();

            var curPosTime = regionStartTime;
            var numPatternNotes = listPattern.Count;

            var patternDirection = pattern.startDirection == PatternStartDirection.Forward ? 1 : -1;
            var patternIndex = pattern.start == PatternStart.Beginning ? 0 : numPatternNotes - 1;

            var sPattern = "";
            
            // play pattern until region has reached end
            while (curPosTime < regionEndTime)
            {
                var n = new Note {
                    Index = listPattern[patternIndex]
                };
                var p = new PianoRoll {
                    PosTime = curPosTime,
                    Notes = new List<Note> {n}
                };
                pianoRoll.Add(p);

                sPattern += patternIndex + ",";
                
                curPosTime += noteInterval;
                if (curPosTime >= regionEndTime) break;
                
                patternIndex += patternDirection;
                if (patternIndex < 0)
                {
                    if (pattern.end == PatternEnd.Reset) {
                        patternIndex = pattern.start == PatternStart.Beginning ? 0 : numPatternNotes - 1;
                    }
                    else if (pattern.end == PatternEnd.Reverse) {
                        patternIndex = 1;
                        patternDirection *= -1;
                    }
                }
                else if (patternIndex >= numPatternNotes)
                {
                    if (pattern.end == PatternEnd.Reset) {
                        patternIndex = pattern.start == PatternStart.Beginning ? 0 : numPatternNotes - 1;
                    }
                    else if (pattern.end == PatternEnd.Reverse) {
                        patternIndex = numPatternNotes - 2;
                        patternDirection *= -1;
                    }
                }
            }
            
            //var s = "";
            //foreach (var note in pianoRoll) s += note.Notes[0].Index + ", ";
            //Debug.Log("simple arp: "+s);
            
            //Debug.Log("simple arp: "+sPattern);
            
            return pianoRoll;
        }
        
        private List<PianoRoll> CreateArpThreeNoteSteps(ScriptableObjectPattern pattern, float noteInterval, IReadOnlyList<int> listPattern)
        {
            var pianoRoll = new List<PianoRoll>();

            var curPosTime = regionStartTime;
            var numPatternNotes = listPattern.Count;

            var patternDirection = pattern.startDirection == PatternStartDirection.Forward ? 1 : -1;
            var patternIndex = pattern.start == PatternStart.Beginning ? 0 : numPatternNotes - 1;
            var noteCount = 0;
            
            // play pattern until region has reached end
            while (curPosTime < regionEndTime)
            {
                var n = new Note {
                    Index = listPattern[patternIndex]
                };
                var p = new PianoRoll {
                    PosTime = curPosTime,
                    Notes = new List<Note> {n}
                };
                pianoRoll.Add(p);

                curPosTime += noteInterval;
                if (curPosTime >= regionEndTime) break;

                noteCount++;
                if (noteCount == 3)
                {
                    if (patternDirection < 0)
                    {
                        if (patternIndex == 0)
                        {
                            if (pattern.end == PatternEnd.Reset) {
                                noteCount = 0;
                                patternIndex = pattern.start == PatternStart.Beginning ? 0 : numPatternNotes - 1;
                            }
                            else if (pattern.end == PatternEnd.Reverse) {
                                noteCount = 1;
                                patternIndex = 1;
                                patternDirection *= -1;
                            }
                        } 
                        else
                        {
                            noteCount = 0;
                            patternIndex -= patternDirection;
                        }
                    } 
                    else 
                    {
                        if (patternIndex == (numPatternNotes - 1))
                        {
                            if (pattern.end == PatternEnd.Reset) {
                                noteCount = 0;
                                patternIndex = pattern.start == PatternStart.Beginning ? 0 : numPatternNotes - 1;
                            }
                            else if (pattern.end == PatternEnd.Reverse) {
                                noteCount = 1;
                                patternIndex = numPatternNotes - 2;
                                patternDirection *= -1;
                            }
                        }
                        else
                        {
                            noteCount = 0;
                            patternIndex -= patternDirection;
                        }
                    }
                }
                else
                {
                    patternIndex += patternDirection;
                }
            }

            var s = "";
            foreach (var note in pianoRoll) s += note.Notes[0].Index + ", ";
            Debug.Log("three note step arp: "+s);
            
            return pianoRoll;
        }*/
        
        //
        
        private void SetDefaultPlaybackSettings(ScriptableObjectInstrument instrument, bool changeKeys = true)
        {
            playbackSettings = new InstrumentSettings()
            {
                Instrument = instrument,
                Key = (changeKeys ? 0 : playbackSettings.Key),
                Interval = (changeKeys ? 0 : playbackSettings.Interval),
                Octave = (changeKeys ? 0 : playbackSettings.Octave),
                Type = (instrument.type == InstrumentType.Looper) ? 2 : 0,
                //Pattern = 0,
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
