using System.Collections.Generic;
using Dragginz.AudioTool.Scripts.ScriptableObjects;
using UnityEngine;
using static Dragginz.AudioTool.Scripts.Includes.Globals;

namespace Dragginz.AudioTool.Scripts.StepEditor
{
    public enum ArpOctaves
    {
        One,
        Two,
        Three
    }

    public enum ArpStart
    {
        Beginning,
        End
    }
    
    /*public enum ArpDirection
    {
        Forward,
        Back
    }*/
    
    public enum ArpEnd
    {
        Reverse,
        Reset
    }
    
    public enum ArpType
    {
        SimpleArpeggio,
        ThreeNoteSteps,
        RootClimb
    }
    
    public struct ArpeggiatorData
    {
        public List<int> listPattern;
        
        public float noteInterval;
        public float startTime;
        public float endTime;
        
        public int octave;
        public int start;
        public int direction;
        public int end;
        public int type;
    }

    public static class Arpeggiator 
    {
        public static List<PianoRoll> CreateArpSimple(ArpeggiatorData data)
        {
            var pianoRoll = new List<PianoRoll>();

            var curPosTime = data.startTime;
            var numPatternNotes = data.listPattern.Count;

            var patternIndex = data.start == (int)ArpStart.Beginning ? 0 : numPatternNotes - 1;
            var patternDirection = patternIndex == 0 ? 1 : -1;
            
            //var sPattern = "";
            
            // play pattern until region has reached end
            while (curPosTime < data.endTime)
            {
                var n = new Note {
                    Index = data.listPattern[patternIndex]
                };
                var p = new PianoRoll {
                    PosTime = curPosTime,
                    Notes = new List<Note> {n}
                };
                pianoRoll.Add(p);

                //sPattern += patternIndex + ",";
                
                curPosTime += data.noteInterval;
                if (curPosTime >= data.endTime) break;
                
                patternIndex += patternDirection;
                if (patternIndex < 0)
                {
                    if (data.end == (int)ArpEnd.Reset) {
                        patternIndex = data.start == (int)ArpStart.Beginning ? 0 : numPatternNotes - 1;
                    }
                    else if (data.end == (int)ArpEnd.Reverse) {
                        patternIndex = 1;
                        patternDirection *= -1;
                    }
                }
                else if (patternIndex >= numPatternNotes)
                {
                    if (data.end == (int)ArpEnd.Reset) {
                        patternIndex = data.start == (int)ArpStart.Beginning ? 0 : numPatternNotes - 1;
                    }
                    else if (data.end == (int)ArpEnd.Reverse) {
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
        
        public static List<PianoRoll> CreateArpThreeNoteSteps(ArpeggiatorData data)
        {
            var pianoRoll = new List<PianoRoll>();

            var curPosTime = data.startTime;
            var numPatternNotes = data.listPattern.Count;

            var patternIndex = data.start == (int)ArpStart.Beginning ? 0 : numPatternNotes - 1;
            var patternDirection = patternIndex == 0 ? 1 : -1;
            var noteCount = 0;
            
            // play pattern until region has reached end
            while (curPosTime < data.endTime)
            {
                var n = new Note {
                    Index = data.listPattern[patternIndex]
                };
                var p = new PianoRoll {
                    PosTime = curPosTime,
                    Notes = new List<Note> {n}
                };
                pianoRoll.Add(p);

                curPosTime += data.noteInterval;
                if (curPosTime >= data.endTime) break;

                noteCount++;
                if (noteCount == 3)
                {
                    if (patternDirection < 0)
                    {
                        if (patternIndex == 0)
                        {
                            if (data.end == (int)ArpEnd.Reset) {
                                noteCount = 0;
                                patternIndex = data.start == (int)ArpStart.Beginning ? 0 : numPatternNotes - 1;
                            }
                            else if (data.end == (int)ArpEnd.Reverse) {
                                noteCount = 0;
                                patternIndex = 0;
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
                            if (data.end == (int)ArpEnd.Reset) {
                                noteCount = 0;
                                patternIndex = data.start == (int)ArpStart.Beginning ? 0 : numPatternNotes - 1;
                            }
                            else if (data.end == (int)ArpEnd.Reverse) {
                                noteCount = 0;
                                patternIndex = numPatternNotes - 1;
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
        }
        
        public static List<PianoRoll> CreateArpRootClimb(ArpeggiatorData data)
        {
            var pianoRoll = new List<PianoRoll>();

            var curPosTime = data.startTime;
            var numPatternNotes = data.listPattern.Count;

            var patternIndex = data.start == (int)ArpStart.Beginning ? 0 : numPatternNotes - 1;
            var patternDirection = patternIndex == 0 ? 1 : -1;


            var patternIndex2 = patternIndex + patternDirection;

            // play pattern until region has reached end
            while (curPosTime < data.endTime)
            {
                var n = new Note {
                    Index = data.listPattern[patternIndex]
                };
                var p = new PianoRoll {
                    PosTime = curPosTime,
                    Notes = new List<Note> {n}
                };
                pianoRoll.Add(p);

                curPosTime += data.noteInterval;
                if (curPosTime >= data.endTime) break;
                
                n = new Note {
                    Index = data.listPattern[patternIndex2]
                };
                p = new PianoRoll {
                    PosTime = curPosTime,
                    Notes = new List<Note> {n}
                };
                pianoRoll.Add(p);
                
                curPosTime += data.noteInterval;
                if (curPosTime >= data.endTime) break;
                
                patternIndex2 += patternDirection;

                if (data.start == (int) ArpStart.Beginning)
                {
                    if (patternIndex2 == patternIndex)
                    {
                        patternIndex2 = patternIndex + 2;
                        patternDirection *= -1;
                    }
                    else if (patternIndex2 >= numPatternNotes)
                    {
                        if (data.end == (int)ArpEnd.Reset)
                        {
                            patternIndex2 = patternIndex + 1;
                            patternDirection = 1;
                        }
                        else if (data.end == (int)ArpEnd.Reverse) {
                            patternIndex2 = numPatternNotes - 2;
                            patternDirection = -1;
                        }
                    } 
                }
                else if (data.start == (int) ArpStart.End)
                {
                    if (patternIndex2 == patternIndex)
                    {
                        patternIndex2 = patternIndex - 2;
                        patternDirection *= -1;
                    }
                    else if (patternIndex2 < 0)
                    {
                        if (data.end == (int)ArpEnd.Reset)
                        {
                            patternIndex2 = patternIndex - 1;
                            patternDirection = -1;
                        }
                        else if (data.end == (int)ArpEnd.Reverse) {
                            patternIndex2 = 1;
                            patternDirection = 1;
                        }
                    } 
                }
            }

            var s = "";
            foreach (var note in pianoRoll) s += note.Notes[0].Index + ", ";
            Debug.Log("three note step arp: "+s);
            
            return pianoRoll;
        }
    }
}
