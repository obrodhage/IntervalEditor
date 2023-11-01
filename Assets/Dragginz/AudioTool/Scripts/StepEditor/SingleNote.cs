using UnityEngine;
using Dragginz.AudioTool.Scripts.Includes;

namespace Dragginz.AudioTool.Scripts.StepEditor
{
    public class SingleNote
    {
        private int _key;
        private int _interval;
    
        public AudioSource AudioSource;

        public bool IsAudible { get; private set; }

        public void Init(AudioSource audioSource)
        {
            AudioSource = audioSource;
        }
    
        public void SetNote(int key, int interval)
        {
            _key = key;
            _interval = interval;
        }
    
        public void PlayNote(Globals.InstrumentSettings playbackSettings, int numInstrumentsSoloed)
        {
            if (AudioSource == null || !AudioSource.enabled) return;

            IsAudible =  !(_interval == 12 && !playbackSettings.HighOctave) && !(_interval != 0 && playbackSettings.RootNoteOnly); //!playbackSettings.Mute &&
            if (numInstrumentsSoloed > 0 && !playbackSettings.Solo) IsAudible = false;
        
            AudioSource.volume = IsAudible ? playbackSettings.Volume : 0;
            AudioSource.panStereo = Mathf.Lerp(-1f, 1f, playbackSettings.Pan);
            
            //if (!AudioSource.isPlaying) AudioSource.Play();
            AudioSource.timeSamples = 0;
            AudioSource.Play();
            
            //Debug.Log("Key "+Globals.Keys[_key]+", Note "+_interval+", isPlaying: "+AudioSource.isPlaying);
        }
        public void StopNote()
        {
            if (AudioSource != null) AudioSource.Stop();
        }
    }
}
