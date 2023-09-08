using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Dragginz.AudioTool.Scripts.Includes;
using Dragginz.AudioTool.Scripts.ScriptableObjects;
using Dragginz.AudioTool.Scripts.StepEditor.UI;

namespace Dragginz.AudioTool.Scripts.StepEditor
{
    public class EditorController : MonoBehaviour
    {
        [SerializeField] private GameObject prefabTrackInfo;
        [SerializeField] private GameObject prefabBarHeader;
        [SerializeField] private GameObject prefabRegion;
        [SerializeField] private GameObject posMarker;

        private UiControllerEditor _uiControllerEditor;
        private UiControllerTrackInfo uiControllerTrackInfo;
        private UiControllerRegionInfo uiControllerRegionInfo;

        private AudioEngine _audioEngine;

        private List<ScriptableObjectChord> _listChordObjects;
        private List<ScriptableObjectInstrument> _listInstrumentObjects;
        private List<ScriptableObjectPattern> _listPatternObjects;
        
        private RectTransform _rectTransformPosMarker;
        private Vector2 _posMarkerPos;
        private const float PosMarkerStartX = 150.0f;

        private Track _curTrackEdit;
        private Region _curRegionEdit;
        
        private void Awake()
        {
            _uiControllerEditor = FindObjectOfType<UiControllerEditor>();
            if (_uiControllerEditor == null) {
                Debug.LogError("Couldn't find Component UiControllerEditor!");
            }
            
            uiControllerTrackInfo = FindObjectOfType<UiControllerTrackInfo>();
            if (uiControllerTrackInfo == null) {
                Debug.LogError("Couldn't find Component UiControllerTrackInfo!");
            }
            
            uiControllerRegionInfo = FindObjectOfType<UiControllerRegionInfo>();
            if (uiControllerRegionInfo == null) {
                Debug.LogError("Couldn't find Component UiControllerRegionInfo!");
            }
            
            _audioEngine = FindObjectOfType<AudioEngine>();
            if (_audioEngine == null) {
                Debug.LogError("Couldn't find Component AudioEngine!");
            }
            
            _rectTransformPosMarker = posMarker.GetComponent<RectTransform>();
            _posMarkerPos = _rectTransformPosMarker.anchoredPosition;
        }
        
        private void Start()
        {
            LoadIntervalsFromScriptableObjects();
            LoadInstrumentsFromScriptableObjects();
            LoadPatternsFromScriptableObjects();
            
            uiControllerTrackInfo.PopulateInstrumentsDropDown(_listInstrumentObjects);

            uiControllerRegionInfo.PopulateLengthsDropDown(Globals.RegionLengths);
            uiControllerRegionInfo.PopulateKeysDropDown(Globals.Keys);
            uiControllerRegionInfo.PopulateIntervalsDropDown(_listChordObjects);
            uiControllerRegionInfo.PopulatePatternsDropDown(_listPatternObjects);
            uiControllerRegionInfo.PopulateArpeggiatorNotesDropDown(Globals.ArpeggiatorNotes);
            uiControllerRegionInfo.PopulateChordNotesDropDown(Globals.ChordNotes);
            
            CreateBarHeaders();
            
            _audioEngine.InitChordList(_listChordObjects);
            _audioEngine.InitPatternList(_listPatternObjects);
            _audioEngine.InitInstrumentList(_listInstrumentObjects);
            
            _uiControllerEditor.Init();
            uiControllerTrackInfo.Init();
            uiControllerRegionInfo.Init();
            
            _uiControllerEditor.OnButtonDemoEvent += OnButtonDemoClick;
            _uiControllerEditor.OnButtonClearEvent += OnButtonClearClick;
            _uiControllerEditor.OnButtonAddTrackEvent += OnButtonAddTrackClick;
            _uiControllerEditor.OnButtonPlayEvent += OnButtonPlayClick;
            _uiControllerEditor.OnButtonStopEvent += OnButtonStopClick;
            _uiControllerEditor.OnActionAddRegionEvent += OnActionAddRegion;
            
            uiControllerTrackInfo.OnDropDownInstrumentEvent += TrackInfoInstrumentChange;
            uiControllerTrackInfo.OnButtonMoveUpEvent += TrackMoveUpEvent;
            uiControllerTrackInfo.OnButtonMoveDownEvent += TrackMoveDownEvent;
            uiControllerTrackInfo.OnButtonDeleteEvent += TrackInfoDeleteEvent;
            
            uiControllerRegionInfo.OnDropDownKeyEvent += RegionInfoKeyChange;
            uiControllerRegionInfo.OnDropDownIntervalEvent += RegionInfoIntervalChange;
            uiControllerRegionInfo.OnDropDownOctaveEvent += RegionInfoOctaveChange;
            uiControllerRegionInfo.OnDropDownTypeEvent += RegionInfoTypeChange;
            uiControllerRegionInfo.OnDropDownPatternEvent += RegionInfoPatternChange;
            uiControllerRegionInfo.OnDropDownNoteEvent += RegionInfoNoteChange;
            uiControllerRegionInfo.OnDropDownChordNoteEvent += RegionInfoChordNoteChange;
            
            uiControllerRegionInfo.OnSliderVolumeEvent += RegionInfoVolumeChange;
            uiControllerRegionInfo.OnSliderPanEvent += RegionInfoPanChange;
            uiControllerRegionInfo.OnToggleRootNoteOnlyEvent += RegionInfoRootNoteOnlyChange;
            uiControllerRegionInfo.OnToggleHighOctaveEvent += RegionInfoHighOctaveChange;
            uiControllerRegionInfo.OnButtonDeleteEvent += RegionInfoDeleteEvent;
        }

        private void Update()
        {
            _posMarkerPos.x = PosMarkerStartX + _audioEngine.curBeat * Globals.PrefabBarBeatWidth;
            _rectTransformPosMarker.anchoredPosition = _posMarkerPos;
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (uiControllerTrackInfo.IsVisible) uiControllerTrackInfo.Hide();
                if (uiControllerRegionInfo.IsVisible) uiControllerRegionInfo.Hide();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                var playing = _audioEngine.StartPlayback();
                if (!playing) _audioEngine.StopPlayback();
                _uiControllerEditor.AudioIsPlaying(playing);
            }
        }
        
        private void LoadIntervalsFromScriptableObjects()
        {
            var soChords = Resources.LoadAll("ScriptableObjects/Chords", typeof(ScriptableObjectChord));
            if (soChords == null) return;
        
            var listChords = soChords.Select(chord => chord as ScriptableObjectChord).ToList();
            _listChordObjects = listChords.OrderBy(o=>o.sortOrder).ToList();
        }
    
        private void LoadInstrumentsFromScriptableObjects()
        {
            var soInstruments = Resources.LoadAll("ScriptableObjects/Instruments", typeof(ScriptableObjectInstrument));
            if (soInstruments == null) return;
        
            var listInstruments = soInstruments.Select(instrument => instrument as ScriptableObjectInstrument).ToList();
            _listInstrumentObjects = listInstruments.OrderBy(o=>o.sortOrder).ToList();
        }
        
        private void LoadPatternsFromScriptableObjects()
        {
            var soPatterns = Resources.LoadAll("ScriptableObjects/Patterns", typeof(ScriptableObjectPattern));
            if (soPatterns == null) return;
        
            var listPatterns = soPatterns.Select(instrument => instrument as ScriptableObjectPattern).ToList();
            _listPatternObjects = listPatterns.OrderBy(o=>o.sortOrder).ToList();
        }

        private void CreateBarHeaders()
        {
            for (var i = 0; i < Globals.StepEditorBars; ++i)
            {
                var goBar = Instantiate(prefabBarHeader, _uiControllerEditor.GoContentBeat.transform);
                var uiBar = goBar.GetComponent<UiBarHeader>();
                if (uiBar != null) {
                    uiBar.Init((i+1).ToString());
                }
            }
        }

        public void CreateTrackGameObject(Track track)
        {
            var goTrack = Instantiate(prefabTrackInfo, _uiControllerEditor.GoContentTracks.transform);
            goTrack.name = (track.Position + 1) + "-" + track.Instrument.name;
            
            var uiTrack = goTrack.GetComponent<UiTrack>();
            if (uiTrack == null) return;
            
            uiTrack.Init((track.Position + 1).ToString(), track.Instrument.name, track.Instrument.defaultColor);
            track.SetListener(uiTrack, CallbackTrackClick);
        }

        public void CreateRegionGameObject(Track track, Region region, ScriptableObjectInstrument instrument)
        {
            var pos = Vector2.zero;
            
            var goRegion = Instantiate(prefabRegion, _uiControllerEditor.GoContentRegions.transform);
            goRegion.name = instrument.name;
            var uiRegion = goRegion.GetComponent<UiRegion>();
            if (uiRegion == null) return;
                    
            region.SetListener(uiRegion, CallbackRegionClick);
                    
            var key = Globals.Keys[region.playbackSettings.Key];
            var chord = _listChordObjects[region.playbackSettings.Interval].name;
            
            var prefabBarWidth = Globals.PrefabBarWidth / 4;
            var w = prefabBarWidth * region.beats;
            uiRegion.Init(w, instrument.defaultColor, key, chord);
            
            pos.x = prefabBarWidth * region.startPosBeats;
            pos.y = track.Position * Globals.PrefabTrackHeight * -1;
            uiRegion.rectTransform.anchoredPosition = pos;
        }
        
        // UI EVENTS

        private void OnButtonDemoClick()
        {
            _audioEngine.CreateDemoProject();
        }

        private void OnButtonClearClick()
        {
            _audioEngine.ClearProject();
        }

        private void OnButtonAddTrackClick()
        {
            var instrument = _listInstrumentObjects[0];
            
            var track = new Track();
            track.Init(_audioEngine.Tracks.Count, instrument);
            
            CreateTrackGameObject(track);
            
            _audioEngine.AddTrack(track);
        }

        private void OnActionAddRegion(int trackPos, int regionPos)
        {
            //Debug.Log("trackPos, regionPos: "+trackPos+", "+regionPos);
            //_audioEngine.AddRegion(trackPos, regionPos);
            var track = _audioEngine.GetTrack(trackPos);
            if (track == null) return;
            
            var newRegion = _audioEngine.CreateNewRegion(track, regionPos);
            if (newRegion != null)
            {
                CreateRegionGameObject(track, newRegion, track.Instrument);
            }
        }
        
        //
        
        private void OnButtonPlayClick()
        {
            _uiControllerEditor.AudioIsPlaying(_audioEngine.StartPlayback());
        }
        
        private void OnButtonStopClick()
        {
            _audioEngine.StopPlayback();
            _uiControllerEditor.AudioIsPlaying(false);
        }
        
        //
        
        private void TrackInfoInstrumentChange(int instrument)
        {
            if (_curTrackEdit == null) return;
            
            //_curTrackEdit.instrument = _listInstrumentObjects[instrument];
            _audioEngine.UpdateTrackInstrument(_curTrackEdit, _listInstrumentObjects[instrument]);
        }
        
        private void TrackMoveUpEvent()
        {
            if (_curTrackEdit != null) {
                _audioEngine.MoveTrack(_curTrackEdit.Position, -1);
                uiControllerTrackInfo.ShowTrackInfo(_curTrackEdit); // refresh
            }
        }
        private void TrackMoveDownEvent()
        {
            if (_curTrackEdit != null) {
                _audioEngine.MoveTrack(_curTrackEdit.Position, 1);
                uiControllerTrackInfo.ShowTrackInfo(_curTrackEdit); // refresh
            }
        }

        private void TrackInfoDeleteEvent()
        {
            if (_curTrackEdit != null) {
                _audioEngine.DeleteTrack(_curTrackEdit);
            }
        }
        
        //
        
        private void RegionInfoKeyChange(int key)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Key = key;
            _audioEngine.UpdateRegion(_curRegionEdit);
        }
        
        private void RegionInfoIntervalChange(int interval)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Interval = interval;
            _audioEngine.UpdateRegion(_curRegionEdit);
        }

        private void RegionInfoOctaveChange(int octave)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Octave = octave;
            _audioEngine.UpdateRegion(_curRegionEdit);
        }

        private void RegionInfoTypeChange(int type)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Type = type;
            _audioEngine.UpdateRegion(_curRegionEdit);
            uiControllerRegionInfo.updateGroupVisibility(_curRegionEdit.playbackSettings);
        }

        private void RegionInfoPatternChange(int pattern)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Pattern = pattern;
            _audioEngine.UpdateRegion(_curRegionEdit);
        }

        private void RegionInfoNoteChange(int note)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Note = note;
            _audioEngine.UpdateRegion(_curRegionEdit);
        }
        
        private void RegionInfoChordNoteChange(int note)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Note = note;
            _audioEngine.UpdateRegion(_curRegionEdit);
        }
        
        private void RegionInfoVolumeChange(float value)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Volume = value;
            _audioEngine.UpdateRegion(_curRegionEdit);
        }
        
        private void RegionInfoPanChange(float value)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Pan = value;
            _audioEngine.UpdateRegion(_curRegionEdit);
        }
        
        private void RegionInfoRootNoteOnlyChange(bool value)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.RootNoteOnly = value;
            _audioEngine.UpdateRegion(_curRegionEdit);
        }
        
        private void RegionInfoHighOctaveChange(bool value)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.HighOctave = value;
            _audioEngine.UpdateRegion(_curRegionEdit);
        }

        private void RegionInfoDeleteEvent()
        {
            if (_curRegionEdit != null)
            {
                _audioEngine.DeleteRegion(_curRegionEdit);
            }
        }
        
        // PUBLIC METHODS

        private void CallbackTrackClick(int trackPos, uint clickAction)
        {
            //Debug.Log("CallbackTrackClick - track, action: "+trackPos+", "+clickAction);

            OnButtonStopClick();
            
            _curTrackEdit = _audioEngine.GetTrack(trackPos);
            if (_curTrackEdit == null) return;

            switch (clickAction)
            {
                case Globals.TrackInfo:
                    uiControllerTrackInfo.ShowTrackInfo(_curTrackEdit);
                    break;
                case Globals.TrackDelete:
                    _audioEngine.DeleteTrack(_curTrackEdit);
                    break;
            }
        }
        private void CallbackRegionClick(int trackPos, int regionStartPos)
        {
            //Debug.Log("CallbackRegionClick - track, region: "+trackPos+", "+regionStartPos);

            OnButtonStopClick();
            
            _curRegionEdit = _audioEngine.GetRegion(trackPos, regionStartPos);
            if (_curRegionEdit != null) uiControllerRegionInfo.ShowRegionInfo(_curRegionEdit);
        }
    }
}