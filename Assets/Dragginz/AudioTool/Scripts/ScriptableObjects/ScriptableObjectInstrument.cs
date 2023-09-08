using System.Collections.Generic;
using UnityEngine;

namespace Dragginz.AudioTool.Scripts.ScriptableObjects
{
    public enum InstrumentType
    {
        Looper,
        SingleNote
    }

    [CreateAssetMenu(fileName = "Instrument", menuName = "ScriptableObjects/Instrument", order = 1)]
    public class ScriptableObjectInstrument : ScriptableObject
    {
        public new string name;

        public uint sortOrder;

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

        public double beatLoopStart;
        public double beatLoopEnd;
    }
}