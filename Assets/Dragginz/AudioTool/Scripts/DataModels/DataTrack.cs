using System;

namespace Dragginz.AudioTool.Scripts.DataModels
{
    [Serializable]
    public class DataTrack
    {
        public int pos;
        public int instrument;
        public int muted;
        public int reverbFilter;
        
        public DataRegion[] regions;
    }
}
