using System;

namespace Dragginz.AudioTool.Scripts.DataModels
{
    [Serializable]
    public class DataRegion
    {
        public int pos;
        public int beats;

        public int key;
        public int interval;
        public int octave;
        public int type;
        public int note;
        public int highOctave;
        public int rootNoteOnly;
        public int vol;
        public int pan;

        public DataArpeggiator dataArp;
        public DataMelodyMaker dataMelody;
    }
}
