using System.Collections.Generic;
using static Dragginz.AudioTool.Scripts.Includes.Globals;

namespace Dragginz.AudioTool.Scripts.StepEditor
{
    public enum MelodyOctaves
    {
        One,
        Two,
        Three
    }

    public enum MelodyMode
    {
        Ionian,
        Dorian,
        Phrygian,
        Lydian,
        Mixolydian,
        Aeolian,
        Locrian
    }
    
    public enum MelodyStart
    {
        Beginning,
        End
    }
    
    public enum MelodyEnd
    {
        Reverse,
        Reset
    }
    
    public enum MelodyType
    {
        SimpleScale,
        ThreeNoteSteps,
        RootClimb
    }
    
    public struct MelodyMakerData
    {
        public List<int> ListPattern;
        
        public float NoteInterval;
        public float StartTime;
        public float EndTime;
        
        public int Octaves;
        public int Mode;
        public int Start;
        public int End;
        public int Type;
    }

    public static class MelodyMaker 
    {
        public static List<PianoRoll> CreateSimpleScale(MelodyMakerData data)
        {
            var pianoRoll = new List<PianoRoll>();

            var curPosTime = data.StartTime;
            var numPatternNotes = data.ListPattern.Count;

            var patternIndex = data.Start == (int)MelodyStart.Beginning ? 0 : numPatternNotes - 1;
            var patternDirection = patternIndex == 0 ? 1 : -1;
            
            //var sPattern = "";
            
            // play pattern until region has reached end
            while (curPosTime < data.EndTime)
            {
                var n = new OneNote {
                    Index = data.ListPattern[patternIndex]
                };
                var p = new PianoRoll {
                    PosTime = curPosTime,
                    Notes = new List<OneNote> {n}
                };
                pianoRoll.Add(p);
                
                curPosTime += data.NoteInterval;
                if (curPosTime >= data.EndTime) break;
                
                patternIndex += patternDirection;
                if (patternIndex < 0)
                {
                    if (data.End == (int)MelodyEnd.Reset) {
                        patternIndex = data.Start == (int)MelodyStart.Beginning ? 0 : numPatternNotes - 1;
                    }
                    else if (data.End == (int)MelodyEnd.Reverse) {
                        patternIndex = 0;
                        patternDirection *= -1;
                    }
                }
                else if (patternIndex >= numPatternNotes)
                {
                    if (data.End == (int)MelodyEnd.Reset) {
                        patternIndex = data.Start == (int)MelodyStart.Beginning ? 0 : numPatternNotes - 1;
                    }
                    else if (data.End == (int)MelodyEnd.Reverse) {
                        patternIndex = numPatternNotes - 1;
                        patternDirection *= -1;
                    }
                }
            }
            
            //var s = "";
            //foreach (var note in pianoRoll) s += note.Notes[0].Index + ", ";
            //Debug.Log("major scale: "+s);
            
            //Debug.Log("major scale: "+sPattern);
            
            return pianoRoll;
        }
        
        public static List<PianoRoll> CreateThreeNoteSteps(MelodyMakerData data)
        {
            var pianoRoll = new List<PianoRoll>();

            var curPosTime = data.StartTime;
            var numPatternNotes = data.ListPattern.Count;

            var patternIndex = data.Start == (int)MelodyStart.Beginning ? 0 : numPatternNotes - 1;
            var patternDirection = patternIndex == 0 ? 1 : -1;
            var noteCount = 0;
            
            // play pattern until region has reached end
            while (curPosTime < data.EndTime)
            {
                var n = new OneNote {
                    Index = data.ListPattern[patternIndex]
                };
                var p = new PianoRoll {
                    PosTime = curPosTime,
                    Notes = new List<OneNote> {n}
                };
                pianoRoll.Add(p);

                curPosTime += data.NoteInterval;
                if (curPosTime >= data.EndTime) break;

                noteCount++;
                if (noteCount == 3)
                {
                    if (patternDirection < 0)
                    {
                        if (patternIndex == 0)
                        {
                            if (data.End == (int)MelodyEnd.Reset) {
                                noteCount = 0;
                                patternIndex = data.Start == (int)MelodyStart.Beginning ? 0 : numPatternNotes - 1;
                            }
                            else if (data.End == (int)MelodyEnd.Reverse) {
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
                            if (data.End == (int)MelodyEnd.Reset) {
                                noteCount = 0;
                                patternIndex = data.Start == (int)MelodyStart.Beginning ? 0 : numPatternNotes - 1;
                            }
                            else if (data.End == (int)MelodyEnd.Reverse) {
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

            //var s = "";
            //foreach (var note in pianoRoll) s += note.Notes[0].Index + ", ";
            //Debug.Log("three note step melody: "+s);
            
            return pianoRoll;
        }

        public static List<PianoRoll> CreateRootClimb(MelodyMakerData data)
        {
            var pianoRoll = new List<PianoRoll>();

            var curPosTime = data.StartTime;
            var numPatternNotes = data.ListPattern.Count;

            var patternIndex = data.Start == (int) MelodyStart.Beginning ? 0 : numPatternNotes - 1;
            var patternDirection = patternIndex == 0 ? 1 : -1;


            var patternIndex2 = patternIndex + patternDirection;

            // play pattern until region has reached end
            while (curPosTime < data.EndTime)
            {
                var n = new OneNote
                {
                    Index = data.ListPattern[patternIndex]
                };
                var p = new PianoRoll
                {
                    PosTime = curPosTime,
                    Notes = new List<OneNote> {n}
                };
                pianoRoll.Add(p);

                curPosTime += data.NoteInterval;
                if (curPosTime >= data.EndTime) break;

                n = new OneNote
                {
                    Index = data.ListPattern[patternIndex2]
                };
                p = new PianoRoll
                {
                    PosTime = curPosTime,
                    Notes = new List<OneNote> {n}
                };
                pianoRoll.Add(p);

                curPosTime += data.NoteInterval;
                if (curPosTime >= data.EndTime) break;

                patternIndex2 += patternDirection;

                if (data.Start == (int) MelodyStart.Beginning)
                {
                    if (patternIndex2 == patternIndex)
                    {
                        patternIndex2 = patternIndex + 2;
                        patternDirection *= -1;
                    }
                    else if (patternIndex2 >= numPatternNotes)
                    {
                        if (data.End == (int) MelodyEnd.Reset)
                        {
                            patternIndex2 = patternIndex + 1;
                            patternDirection = 1;
                        }
                        else if (data.End == (int) MelodyEnd.Reverse)
                        {
                            patternIndex2 = numPatternNotes - 2;
                            patternDirection = -1;
                        }
                    }
                }
                else if (data.Start == (int) MelodyStart.End)
                {
                    if (patternIndex2 == patternIndex)
                    {
                        patternIndex2 = patternIndex - 2;
                        patternDirection *= -1;
                    }
                    else if (patternIndex2 < 0)
                    {
                        if (data.End == (int) MelodyEnd.Reset)
                        {
                            patternIndex2 = patternIndex - 1;
                            patternDirection = -1;
                        }
                        else if (data.End == (int) MelodyEnd.Reverse)
                        {
                            patternIndex2 = 1;
                            patternDirection = 1;
                        }
                    }
                }
            }
            
            //var s = "";
            //foreach (var note in pianoRoll) s += note.Notes[0].Index + ", ";
            //Debug.Log("root climb melody: "+s);
            
            return pianoRoll;
        }
    }
}
