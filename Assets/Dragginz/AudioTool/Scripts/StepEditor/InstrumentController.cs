using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Dragginz.AudioTool.Scripts.Includes;
using Dragginz.AudioTool.Scripts.ScriptableObjects;

namespace Dragginz.AudioTool.Scripts.StepEditor
{
    public class InstrumentController
    {
        private ScriptableObjectInstrument _soInstrument;

        private List<Globals.PianoRoll> _pianoRoll;
        private int _curPianoRollIndex;
        
        private List<AudioClip> _audioClips;

        private List<int> _curIntervals;
        private List<SingleNote> _singleNotes;
    
        private int _numCurIntervals;
        private int _numSingleNotes;

        private double _startDspTime;
        public double LoopDspTime;
        
        public GameObject GoParent { get; }

        // Current state vars
        public Globals.InstrumentSettings Settings { get; private set; }

        public InstrumentController(ScriptableObjectInstrument soInstrument, GameObject go)
        {
            _soInstrument = soInstrument;
            
            GoParent = go;

            LoadAudioClipsAndSettings();
            CreateAudioSourceComponents();
        }

        private void LoadAudioClipsAndSettings()
        {
            _singleNotes = new List<SingleNote>();
            _numSingleNotes = 0;

            _curIntervals = new List<int>();
            _numCurIntervals = 0;

            _pianoRoll = new List<Globals.PianoRoll>();
            
            // set loop sample targets
            var beatsPerSecond  = _soInstrument.bpm / 60.0f;
            var timeInSecs = _soInstrument.beats / beatsPerSecond;
            var samplesTotal = timeInSecs * _soInstrument.sampleRate;
            var samplesPerBeat = samplesTotal / _soInstrument.beats;
        
            Settings = new Globals.InstrumentSettings
            {
                Instrument = _soInstrument,
                Octave = _soInstrument.octaves[0],
                //Mute = false,
                Solo = false,
                HighOctave = _soInstrument.highOctave,
                RootNoteOnly = _soInstrument.rootNoteOnly,
                Volume = _soInstrument.defaultVolume,
                Pan = _soInstrument.defaultPan,
                CanLoop = _soInstrument.type == InstrumentType.Looper,
                //BeatsPerSecond = beatsPerSecond,
                SampleLoopStart = (_soInstrument.beatLoopStart-1) * samplesPerBeat,
                SampleLoopEnd = (_soInstrument.beatLoopEnd-1) * samplesPerBeat,
                TimeLoopEnd = (_soInstrument.beatLoopEnd-1) / beatsPerSecond
            };

            _audioClips = new List<AudioClip>();
            
            foreach (var fileName in _soInstrument.noteFileNames)
            {
                var path = Path.Combine(_soInstrument.loadDirName, fileName);
                var audioClip = Resources.Load(path, typeof(AudioClip)) as AudioClip;
                if (audioClip != null)
                {
                    audioClip.LoadAudioData();
                    _audioClips.Add(audioClip);
                }
            }
        }

        private void CreateAudioSourceComponents()
        {
            var numClips = _audioClips.Count;
            for (var i = 0; i < numClips; ++i)
            {
                var audioSource = GoParent.AddComponent(typeof(AudioSource)) as AudioSource;
                if (audioSource != null)
                {
                    audioSource.playOnAwake = false;
                    audioSource.enabled = false;
                    var singleNote = new SingleNote();
                    singleNote.Init(audioSource);
                    _singleNotes.Add(singleNote);
                }
            }

            _numSingleNotes = _singleNotes.Count;
        }
        
        // Public methods
        
        public void UpdateParentName(int trackPos)
        {
            GoParent.name = trackPos + "-" + _soInstrument.name;
            GoParent.transform.SetSiblingIndex(trackPos-1);
        }
        
        public void UpdateInstrument(ScriptableObjectInstrument instrument)
        {
            Remove();
            
            _soInstrument = instrument;
            
            LoadAudioClipsAndSettings();
            CreateAudioSourceComponents();
        }
        
        public void PlayIntervals(int key, List<int> intervals, Globals.InstrumentSettings playbackSettings,
            int numInstrumentsSoloed, double startDspTime, double curDspTime)
        {
            StopAllSources();
        
            _curIntervals = intervals;
            _numCurIntervals = _curIntervals.Count;
        
            // add missing audio sources if needed
            /*while (_numCurIntervals > _numSingleNotes) {
                var audioSource = GoParent.AddComponent(typeof(AudioSource)) as AudioSource;
                if (audioSource != null)
                {
                    audioSource.playOnAwake = false;
                    var singleNote = new SingleNote();
                    singleNote.Init(audioSource);
                    _singleNotes.Add(singleNote);
                }

                _numSingleNotes = _singleNotes.Count;
            }*/

            for (var i = 0; i < _numCurIntervals; ++i)
            {
                var index = (playbackSettings.Octave * 12) + key + _curIntervals[i];
                if (index >= _audioClips.Count)
                {
                    _singleNotes[i].AudioSource.enabled = false;
                    //Debug.Log("index out of bounds - cannot play interval "+_curIntervals[i]);
                }
                else
                {
                    var clip = _audioClips[index];
                    _singleNotes[i].SetNote(key, _curIntervals[i]);
                    _singleNotes[i].AudioSource.enabled = true;
                    _singleNotes[i].AudioSource.loop = false;
                    _singleNotes[i].AudioSource.clip = clip;
                    _singleNotes[i].PlayNote(playbackSettings, numInstrumentsSoloed);
                    //Debug.Log("clip: "+clip.name+", length: "+clip.length+"s, samples: "+clip.samples);
                }
            }

            LoopDspTime = curDspTime + Settings.TimeLoopEnd;
            //Debug.Log("PlayIntervals: "+LoopDspTime);
        
            // disable remaining audio sources if needed
            //for (var i = _numCurIntervals; i < _numSingleNotes; ++i) {
            //    _singleNotes[i].AudioSource.enabled = false;
            //}
        }

        public void StartPlayback(List<Globals.PianoRoll> regionPianoRoll, Globals.InstrumentSettings playbackSettings,
            int numInstrumentsSoloed, double startDspTime, double curDspTime)
        {
            StopAllSources();
            
            _pianoRoll = regionPianoRoll;
            _startDspTime = startDspTime;

            _curPianoRollIndex = 0;

            if (_curPianoRollIndex >= _pianoRoll.Count) return;
            
            if (!(curDspTime >= _startDspTime + _pianoRoll[_curPianoRollIndex].PosTime)) return;

            if (_pianoRoll[_curPianoRollIndex].Notes.Count <= 0) return;

            //Debug.Log("start playback region "+curDspTime);
            foreach (var note in _pianoRoll[_curPianoRollIndex].Notes)
            {
                if (note.Index >= _singleNotes.Count) continue;
                
                var clip = _audioClips[note.Index];
                _singleNotes[note.Index].SetNote(0, 0); // not really needed
                _singleNotes[note.Index].AudioSource.enabled = true;
                _singleNotes[note.Index].AudioSource.loop = false;
                _singleNotes[note.Index].AudioSource.clip = clip;
                _singleNotes[note.Index].PlayNote(playbackSettings, numInstrumentsSoloed);
            }

            _curPianoRollIndex++;
        }
        
        public void UpdatePlayback(double curDspTime, Globals.InstrumentSettings playbackSettings, int numInstrumentsSoloed)
        {
            if (_curPianoRollIndex >= _pianoRoll.Count) return;

            if (!(curDspTime >= _startDspTime + _pianoRoll[_curPianoRollIndex].PosTime)) return;
            
            if (_pianoRoll[_curPianoRollIndex].Notes.Count <= 0) return;
            
            foreach (var note in _pianoRoll[_curPianoRollIndex].Notes)
            {
                if (note.Index >= _singleNotes.Count) continue;
                
                var clip = _audioClips[note.Index];
                _singleNotes[note.Index].SetNote(0, 0); // not really needed
                _singleNotes[note.Index].AudioSource.enabled = true;
                _singleNotes[note.Index].AudioSource.loop = false;
                _singleNotes[note.Index].AudioSource.clip = clip;
                _singleNotes[note.Index].PlayNote(playbackSettings, numInstrumentsSoloed);
            }

            _curPianoRollIndex++;
        }
        
        public void LoopBack(double curDspTime)
        {
            foreach (var singleNote in _singleNotes)
            {
                singleNote.AudioSource.timeSamples = (int)Settings.SampleLoopStart;
            }
        
            LoopDspTime = curDspTime + Settings.TimeLoopEnd;
        }

        public void SkipToEndBeat(double curDspTime)
        {
            foreach (var singleNote in _singleNotes)
            {
                singleNote.AudioSource.timeSamples = (int)Settings.SampleLoopEnd;
            }
            
            LoopDspTime = curDspTime + Settings.TimeLoopEnd;
        }
        
        public void Mute(bool mute)
        {
            foreach (var singleNote in _singleNotes) {
                singleNote.AudioSource.mute = mute;
            }
        }
        
        /*public void MuteIntervals(bool mute, int numInstrumentsSoloed)
        {
            var instrumentSettings = Settings;
            instrumentSettings.Mute = mute;
            Settings = instrumentSettings;
        
            foreach (var singleNote in _singleNotes) {
                singleNote.PlayNote(Settings, numInstrumentsSoloed);
            }
        }

        // Mute when soloing instruments, but don't set Mute flag!
        public void ForceMuteIntervals(bool mute, int numInstrumentsSoloed)
        {
            // do not unmute if mute flag is set to true!
            if (!mute && Settings.Mute) return;
        
            // do not force mute if solo flag is set to true!
            if (mute && Settings.Solo) return;
        
            foreach (var singleNote in _singleNotes) {
                singleNote.PlayNote(Settings, numInstrumentsSoloed);
            }
        }
    
        public void SoloIntervals(bool solo, int numInstrumentsSoloed)
        {
            if (Settings.Mute) return;
        
            var instrumentSettings = Settings;
            instrumentSettings.Solo = solo;
            Settings = instrumentSettings;
        
            foreach (var singleNote in _singleNotes) {
                singleNote.PlayNote(Settings, numInstrumentsSoloed);
            }
        }

        public void SetHighOctave(bool active, int numInstrumentsSoloed)
        {
            var instrumentSettings = Settings;
            instrumentSettings.HighOctave = active;
            Settings = instrumentSettings;
        
            foreach (var singleNote in _singleNotes) {
                singleNote.PlayNote(Settings, numInstrumentsSoloed);
            }
        }
    
        public void SetRootNoteOnly(bool active, int numInstrumentsSoloed)
        {
            var instrumentSettings = Settings;
            instrumentSettings.RootNoteOnly = active;
            Settings = instrumentSettings;
        
            foreach (var singleNote in _singleNotes) {
                singleNote.PlayNote(Settings, numInstrumentsSoloed);
            }
        }
    
        public void SetVolume(float volume, int numInstrumentsSoloed)
        {
            if (Settings.Mute) return;

            var instrumentSettings = Settings;
            instrumentSettings.Volume = volume;
            Settings = instrumentSettings;
        
            foreach (var singleNote in _singleNotes) {
                singleNote.PlayNote(Settings, numInstrumentsSoloed);
            }
        }*/
    
        public void StopAllSources()
        {
            for (var i = 0; i < _numSingleNotes; ++i) {
                _singleNotes[i].StopNote();
            }
        }

        public void Remove()
        {
            StopAllSources();

            if (_singleNotes != null)
            {
                for (var i = 0; i < _numSingleNotes; ++i) {
                    Object.Destroy(_singleNotes[i].AudioSource);
                }
                _singleNotes.Clear();
            }
            
            _audioClips?.Clear();
        }
    }
}