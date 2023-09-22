using System;

namespace Dragginz.AudioTool.Scripts.DataModels
{
    [Serializable]
    public class DataComposition
    {
        public string title;
        public int bpm;

        public DataTrack[] tracks;
    }
}
