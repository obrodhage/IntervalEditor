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
        public bool highOctave;
        public bool rootNoteOnly;
        public float volume;
        public float pan;

        public DataArpeggiator dataArpeggiator;
    }
}
