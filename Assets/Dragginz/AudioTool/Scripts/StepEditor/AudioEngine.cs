using System.Collections.Generic;
using UnityEngine;
using Dragginz.AudioTool.Scripts.Includes;
using Dragginz.AudioTool.Scripts.ScriptableObjects;

namespace Dragginz.AudioTool.Scripts.StepEditor
{
    public class AudioEngine : MonoBehaviour
    {
        private EditorController _editorController;
        
        private List<List<int>> _intervals;
        
        private List<ScriptableObjectChord> _listChords;
        //private List<ScriptableObjectPattern> _listPatterns;
        private List<ScriptableObjectInstrument> _listInstruments;
        
        private int _numInstrumentsMuted;
        private int _numInstrumentsSoloed;

        private float _curBpm;

        public float curBeat;

        private bool _isUpdatingInstrumentInfo;

        private double _startDspTime;
        private double _curDspTime;

        // Getters

        private float BeatsPerSec { get; set; }

        public List<Track> Tracks { get; private set; }

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            _curBpm = 120.0f;
            BeatsPerSec = _curBpm / 60.0f;

            Tracks = new List<Track>();
        }

        private void Start()
        {
            _editorController = FindObjectOfType<EditorController>();
            if (_editorController == null) {
                Debug.LogError("Couldn't find Component EditorController!");
            }
            
            _startDspTime = AudioSettings.dspTime;
        }
        
        public void InitChordList(List<ScriptableObjectChord> sortedListChords)
        {
            _listChords = sortedListChords;
            
            _intervals = new List<List<int>>();
        
            foreach (var chord in _listChords)
            {
                var intervals = new List<int>();
                foreach (var interval in chord.intervals) {
                    intervals.Add(interval);
                }
                _intervals.Add(intervals);
            }
        }

        /*public void InitPatternList(List<ScriptableObjectPattern> sortedListPatterns)
        {
            _listPatterns = sortedListPatterns;
        }*/

        public void InitInstrumentList(List<ScriptableObjectInstrument> listInstrumentObjects)
        {
            _listInstruments = listInstrumentObjects;
        }
        
        public void CreateDemoProject()
        {
            ClearProject();
            
            var tracks = CreateTracksAndRegions();
            CreateAudioForTracks(tracks);
        }

        public void ClearProject()
        {
            while (Tracks.Count > 0)
            {
                while (Tracks[0].Regions.Count > 0)
                {
                    Tracks[0].Regions[0].Remove();
                    Tracks[0].Regions[0] = null;
                    Tracks[0].Regions.RemoveAt(0);
                }
                
                Tracks[0].Remove();
                Tracks[0] = null;
                Tracks.RemoveAt(0);
            }
        }
        
        // check for loop events
        void Update()
        {
            if (!IsPlaying) return;
        
            _curDspTime = AudioSettings.dspTime;

            curBeat = (float) ((_curDspTime - _startDspTime) * BeatsPerSec);
            //Debug.Log("_curBeat: "+_curBeat);
            
            foreach (var track in Tracks)
            {
                track.UpdatePlayback(_startDspTime, _curDspTime, _numInstrumentsSoloed);
            }
        }
        
        public bool StartPlayback()
        {
            if (IsPlaying) return false;

            IsPlaying = true;
        
            _curDspTime = _startDspTime = AudioSettings.dspTime;
            foreach (var track in Tracks)
            {
                track.PrepareForPlayback();
                track.UpdatePlayback(_startDspTime, _curDspTime, _numInstrumentsSoloed);
            }

            return true;
        }

        public void StopPlayback()
        {
            if (!IsPlaying) return;

            IsPlaying = false;
        
            foreach (var track in Tracks)
            {
                track.StopPlayback();
            }
        }
        
        public Track GetTrack(int trackPos)
        {
            Track track = null;
            foreach (var t in Tracks)
            {
                if (t.Position != trackPos) continue;
                
                track = t;
                break;
    
            }

            return track;
        }
        
        public void UpdateTrackInstrument(Track updateTrack, ScriptableObjectInstrument instrument)
        {
            foreach (var t in Tracks)
            {
                if (t.Position != updateTrack.Position) continue;

                t.UpdateInstrument(instrument);
                
                foreach (var r in t.Regions)
                {
                    r.UpdateInstrument(instrument);
                    
                    var key = Globals.Keys[r.playbackSettings.Key];
                    var chord = _listChords[r.playbackSettings.Interval].name;
                    r.RegionUi.UpdateValues(key, chord);
                    
                    if (instrument.type != InstrumentType.Looper) {
                        r.CreatePianoRoll(_intervals);//, _listPatterns);
                    }
                }
                
                break;
            }
        }
        
        public Region GetRegion(int trackPos, int regionStartPos)
        {
            Region region = null;
            foreach (var t in Tracks)
            {
                if (t.Position != trackPos) continue;
                
                foreach (var r in t.Regions)
                {
                    if (r.startPosBeats == regionStartPos)
                    {
                        region = r;
                        break;
                    }
                }
            }

            return region;
        }
        
        public void UpdateRegion(Region updateRegion)
        {
            foreach (var t in Tracks)
            {
                if (t.Position != updateRegion.trackPos) continue;
                
                foreach (var r in t.Regions)
                {
                    if (r.startPosBeats == updateRegion.startPosBeats)
                    {
                        r.SetChordData(updateRegion.playbackSettings.Key, updateRegion.playbackSettings.Interval, updateRegion.playbackSettings.Octave);
                        r.SetPatternData(-1, updateRegion.playbackSettings.Note); //updateRegion.playbackSettings.Pattern
                        r.playbackSettings.Type = updateRegion.playbackSettings.Type;
                        r.playbackSettings.Volume = updateRegion.playbackSettings.Volume;
                        r.playbackSettings.Pan = updateRegion.playbackSettings.Pan;
                        r.playbackSettings.RootNoteOnly = updateRegion.playbackSettings.RootNoteOnly;
                        r.playbackSettings.HighOctave = updateRegion.playbackSettings.HighOctave;
                        var key = Globals.Keys[r.playbackSettings.Key];
                        var chord = _listChords[r.playbackSettings.Interval].name;
                        r.RegionUi.UpdateValues(key, chord);
                        if (t.Instrument.type != InstrumentType.Looper) {
                            r.CreatePianoRoll(_intervals); //, _listPatterns);
                        }
                        break;
                    }
                }
            }
        }

        public void UpdateRegionArpeggiatorData(Region updateRegion, ArpeggiatorData data)
        {
            foreach (var t in Tracks)
            {
                if (t.Position != updateRegion.trackPos) continue;

                foreach (var r in t.Regions)
                {
                    if (r.startPosBeats == updateRegion.startPosBeats)
                    {
                        r.playbackSettings.arpData.octave = data.octave;
                        r.playbackSettings.arpData.start = data.start;
                        r.playbackSettings.arpData.direction = data.direction;
                        r.playbackSettings.arpData.end = data.end;
                        r.playbackSettings.arpData.type = data.type;
                        
                        if (t.Instrument.type != InstrumentType.Looper) {
                            r.CreatePianoRoll(_intervals) ;//, _listPatterns);
                        }
                    }
                }
            }
        }

        public void UpdateRegionLength(Region updateRegion, int length)
        {
            foreach (var t in Tracks)
            {
                if (t.Position != updateRegion.trackPos) continue;

                var updateOtherRegions = false;
                var regionPos = 0;
                
                foreach (var r in t.Regions)
                {
                    if (updateOtherRegions)
                    {
                        if (r.startPosBeats < regionPos)
                        {
                            r.UpdateStartPos(regionPos, BeatsPerSec);
                            r.CreatePianoRoll(_intervals); //, _listPatterns);
                            _editorController.UpdateRegionGameObjectPosAndSize(r);
                        }

                        regionPos = r.startPosBeats + r.beats;
                        continue;
                    }
                    
                    if (r.startPosBeats != updateRegion.startPosBeats) continue;

                    r.UpdateLength(length, BeatsPerSec);
                    r.CreatePianoRoll(_intervals); //, _listPatterns);
                    _editorController.UpdateRegionGameObjectPosAndSize(r);
                    
                    regionPos = r.startPosBeats + r.beats;
                    updateOtherRegions = true;
                }
            }
        }

        public void DeleteRegion(Region deleteRegion)
        {
            foreach (var t in Tracks)
            {
                if (t.Position != deleteRegion.trackPos) continue;
                
                for (var i = 0; i < t.Regions.Count; ++i)
                {
                    if (t.Regions[i].startPosBeats == deleteRegion.startPosBeats)
                    {
                        t.Regions[i].Remove();
                        t.Regions[i] = null;
                        t.Regions.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public Globals.MouseRegionBeatPos ValidateRegionPosAndSize(Track track, Globals.MouseRegionBeatPos regionBeatPos)
        {
            foreach (var t in Tracks)
            {
                if (t.Position == track.Position)
                {
                    regionBeatPos = t.ValidateRegionPosAndSize(regionBeatPos);
                    break;
                };
            }

            return regionBeatPos;
        }
        
        public Region CreateNewRegion(Track track, Globals.MouseRegionBeatPos regionBeatPos)
        {
            Region region = null;
            foreach (var t in Tracks)
            {
                if (t.Position != track.Position) continue;

                region = new Region();
                region.Init(regionBeatPos.regionStartPos, regionBeatPos.numBeats, t, BeatsPerSec);
                region.CreatePianoRoll(_intervals); //, _listPatterns);
                
                t.AddRegion(region);

                break;
            }

            return region;
        }

        public void AddTrack(Track track)
        {
            track.Intervals = _intervals;
            Tracks.Add(track);
            
            var go = new GameObject
            {
                transform = {parent = transform},
                name = (track.Position+1) + "-" + track.Instrument.name
            };

            var instrumentController = new InstrumentController(track.Instrument, go);

            track.InstrumentController = instrumentController;
        }

        public void MoveTrack(int trackPos, int direction)
        {
            if (trackPos <= 0 && direction < 0) return;
            if (trackPos >= (Tracks.Count - 1) && direction > 0) return;
            
            // Debug.Log("moving track on pos "+trackPos+" "+(direction < 0 ? "up" : "down"));
            
            var trackRemoved = Tracks[trackPos];
            Tracks.RemoveAt(trackPos);
            if (direction < 0) Tracks.Insert(trackPos-1, trackRemoved);
            else Tracks.Insert(trackPos+1, trackRemoved);
            
            for (var t = 0; t < Tracks.Count; ++t) {
                Tracks[t].UpdatePosition(t);
                //Debug.Log("track "+t+" - instrument: "+Tracks[t].Instrument.name);
            }
        }

        public void DeleteTrack(Track deleteTrack)
        {
            var trackWasDeleted = false;
            
            for (var t = 0; t < Tracks.Count; ++t)
            {
                if (Tracks[t].Position != deleteTrack.Position) continue;

                // remove regions first
                if (Tracks[t].Regions.Count > 0)
                {
                    while (Tracks[t].Regions.Count > 0)
                    {
                        Tracks[t].Regions[0].Remove();
                        Tracks[t].Regions[0] = null;
                        Tracks[t].Regions.RemoveAt(0);
                    }
                }
                // then remove track itself
                else
                {
                    trackWasDeleted = true;
                    
                    Tracks[t].Remove();
                    Tracks[t] = null;
                    Tracks.RemoveAt(t);
                    break;
                }
            }

            if (trackWasDeleted)
            {
                for (var t = 0; t < Tracks.Count; ++t) {
                    Tracks[t].UpdatePosition(t);
                }
            }
        }

        // Demo Project
        
        private List<Track> CreateTracksAndRegions()
        {
            var tracks = new List<Track>();
            
            var pos = Vector2.zero;

            var instCount = 0;
            foreach (var instrument in _listInstruments)
            {
                //Debug.Log("create regions for track "+instrument.name);
                // create the default tracks
                var track = new Track();
                track.Init(instCount++, instrument);
                if (instCount == 1) // Pad
                {
                    var region1 = new Region();
                    region1.Init(0, Globals.DefaultRegionBeats, track, BeatsPerSec);
                    region1.SetChordData(0, 0, 0);
                    region1.playbackSettings.RootNoteOnly = true;
                    region1.playbackSettings.Type = (int)InstrumentType.Looper;
                    track.AddRegion(region1);
                    //
                    region1 = new Region();
                    region1.Init(28, Globals.DefaultRegionBeats, track, BeatsPerSec);
                    region1.SetChordData(0, 0, 0);
                    region1.playbackSettings.RootNoteOnly = true;
                    region1.playbackSettings.Type = (int)InstrumentType.Looper;
                    track.AddRegion(region1);
                }
                if (instCount is 1 or 2) // Cello
                {
                    var region2 = new Region();
                    region2.Init(8, Globals.DefaultRegionBeats, track, BeatsPerSec);
                    region2.SetChordData(5, 6, 0);
                    region2.playbackSettings.Type = (int)InstrumentType.Looper;
                    track.AddRegion(region2);
                }
                if (instCount is 1 or 2 or 3) // Viola
                {
                    var region3 = new Region();
                    region3.Init(16, Globals.DefaultRegionBeats / 2, track, BeatsPerSec);
                    region3.SetChordData(7, 10, 0);
                    region3.playbackSettings.Type = (int)InstrumentType.Looper;
                    track.AddRegion(region3);
                    //
                    region3 = new Region();
                    region3.Init(20, Globals.DefaultRegionBeats, track, BeatsPerSec);
                    region3.SetChordData(7, 0, 0);
                    region3.playbackSettings.Type = (int)InstrumentType.Looper;
                    track.AddRegion(region3);
                }
                if (instCount is 4) // Piano
                {
                    var region4 = new Region();
                    region4.Init(0, Globals.DefaultRegionBeats, track, BeatsPerSec);
                    region4.SetChordData(0, 0, 1);
                    region4.SetPatternData(0, 0);
                    region4.playbackSettings.RootNoteOnly = true;
                    region4.playbackSettings.Type = (int)InstrumentType.Chord;
                    track.AddRegion(region4);
                    //
                    region4 = new Region();
                    region4.Init(8, Globals.DefaultRegionBeats, track, BeatsPerSec);
                    region4.SetChordData(5, 6, 1);
                    region4.SetPatternData(0, 1);
                    region4.playbackSettings.Type = (int)InstrumentType.Chord;
                    track.AddRegion(region4);
                    //
                    region4 = new Region();
                    region4.Init(16, Globals.DefaultRegionBeats / 2, track, BeatsPerSec);
                    region4.SetChordData(7, 10, 2);
                    region4.SetPatternData(0, 2);
                    region4.playbackSettings.Type = (int)InstrumentType.Chord;
                    track.AddRegion(region4);
                    //
                    region4 = new Region();
                    region4.Init(20, Globals.DefaultRegionBeats, track, BeatsPerSec);
                    region4.SetChordData(7, 0, 2);
                    region4.SetPatternData(0, 2);
                    region4.playbackSettings.HighOctave = true;
                    region4.playbackSettings.Type = (int)InstrumentType.Chord;
                    track.AddRegion(region4);
                    //
                    region4 = new Region();
                    region4.Init(28, Globals.DefaultRegionBeats, track, BeatsPerSec);
                    region4.SetChordData(0, 0, 0);
                    region4.SetPatternData(0, 0);
                    region4.playbackSettings.HighOctave = true;
                    region4.playbackSettings.Type = (int)InstrumentType.Chord;
                    track.AddRegion(region4);
                }
                if (instCount is 5) // Kalimba
                {
                    var region5 = new Region();
                    region5.Init(16, Globals.DefaultRegionBeats / 2, track, BeatsPerSec);
                    region5.SetChordData(7, 10, 0);
                    region5.SetPatternData(0, 0);
                    region5.playbackSettings.Type = (int)InstrumentType.Arpeggiator;
                    track.AddRegion(region5);
                    //
                    region5 = new Region();
                    region5.Init(20, Globals.DefaultRegionBeats, track, BeatsPerSec);
                    region5.SetChordData(7, 0, 0);
                    region5.SetPatternData(1, 0);
                    region5.playbackSettings.Type = (int)InstrumentType.Arpeggiator;
                    track.AddRegion(region5);
                }
                tracks.Add(track);

                _editorController.CreateTrackGameObject(track);

                foreach (var region in track.Regions)
                {
                    _editorController.CreateRegionGameObject(track, region, instrument);
                }

                pos.y -= Globals.PrefabTrackHeight;
            }

            return tracks;
        }
        
        private void CreateAudioForTracks(List<Track> tracks)
        {
            Tracks = tracks;
        
            foreach (var track in Tracks)
            {
                track.Intervals = _intervals;
                
                var go = new GameObject
                {
                    transform = {parent = transform},
                    name = (track.Position+1) + "-" + track.Instrument.name
                };

                var instrumentController = new InstrumentController(track.Instrument, go);

                track.InstrumentController = instrumentController;
                
                if (track.Instrument.type == InstrumentType.Looper) continue;
                
                foreach (var region in track.Regions) {
                    region.CreatePianoRoll(_intervals); //, _listPatterns);
                }
            }
        }
    }
}
