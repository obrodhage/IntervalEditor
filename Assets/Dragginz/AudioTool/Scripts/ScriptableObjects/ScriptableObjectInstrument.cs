using System.Collections.Generic;
using UnityEngine;

namespace Dragginz.AudioTool.Scripts.ScriptableObjects
{
    public enum InstrumentType
    {
        Chord,
        Arpeggiator,
        Looper
    }

    [CreateAssetMenu(fileName = "Instrument", menuName = "ScriptableObjects/Instrument", order = 1)]
    public class ScriptableObjectInstrument : ScriptableObject
    {
        public int uniqueId;
        
        public new string name;

        public int sortOrder;

        public InstrumentType type;

        public string loadDirName;

        public List<string> noteFileNames;

        public List<int> octaves;
        
        public Color defaultColor;

        public bool highOctave;
        public bool rootNoteOnly;
        public float defaultVolume;
        public float defaultPan;

        public uint bpm;
        public uint beats;
        public uint sampleRate;

        public bool canLoop;
        public double beatLoopStart;
        public double beatLoopEnd;
    }
}