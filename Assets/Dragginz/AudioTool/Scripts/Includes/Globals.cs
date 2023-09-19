using System.Collections.Generic;
using Dragginz.AudioTool.Scripts.ScriptableObjects;
using Dragginz.AudioTool.Scripts.StepEditor;

namespace Dragginz.AudioTool.Scripts.Includes
{
    public static class Globals
    {
        public const string Version = "v0.9.8.23";
        
        public const uint TrackInfo = 0;
        public const uint TrackMute = 1;
        public const uint TrackDelete = 2;
        
        public const int DefaultRegionBeats = 8;
        
        public const int StepEditorBars = 32;
            
        public const int PrefabTrackHeight = 40;
        public const int PrefabBarWidth = 80;
        public const int PrefabBarBeatWidth = 20;
        
        public static readonly string[] RegionLengths = {
            " 4 Beats",
            " 8 Beats",
            "12 Beats",
            "16 Beats",
            "20 Beats",
            "24 Beats"
        };
        
        public static readonly string[] Keys = {
            "C",
            "C#",
            "D",
            "Eb",
            "E",
            "F",
            "F#",
            "G",
            "Ab",
            "A",
            "Bb",
            "B"
        };
        
        public static readonly string[] ChordNotes = {
            "One Shot",
            "Full Note",
            "1/2 Notes",
            "1/4 Notes",
            "8th Notes",
            "16th Notes",
            "32nd Notes"
        };
        
        public static readonly float[] ChordNotesPerBar = {
            0f,
            1f,
            2f,
            4f,
            8f,
            16f,
            32f
        };
        
        public static readonly string[] ArpeggiatorNotes = {
            "16th Notes",
            "8th Notes",
            "1/4 Notes",
            "1/2 Notes",
            "Full Note",
            "32nd Notes"
        };
        
        public static readonly float[] ArpeggiatorNotesPerBar = {
            16f,
            8f,
            4f,
            2f,
            1f,
            32f
        };
        
        public struct InstrumentSettings
        {
            public ScriptableObjectInstrument Instrument;

            public int Key;
            public int Interval;
            public int Octave;
            public int Type;
            //public int Pattern;
            public int Note;

            public ArpeggiatorData arpeggiatorData;
            
            public bool Solo;
            public bool HighOctave;
            public bool RootNoteOnly;
        
            public float Volume;
            public float Pan;
            
            public bool CanLoop;
        
            public double SampleLoopStart;
            public double SampleLoopEnd;
            
            public double TimeLoopEnd;
        }

        public struct Note
        {
            public int Index;
        }

        public struct PianoRoll
        {
            public float PosTime;
            public List<Note> Notes;
        }
        
        public struct MouseRegionBeatPos
        {
            public int trackPos;
            public int regionStartPos;
            public int numBeats;
            public bool posIsValid;
        }
    }
}