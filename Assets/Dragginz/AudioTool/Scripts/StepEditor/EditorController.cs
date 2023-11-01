using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dragginz.AudioTool.Scripts.DataModels;
using UnityEngine;
using Dragginz.AudioTool.Scripts.Includes;
using Dragginz.AudioTool.Scripts.ScriptableObjects;
using Dragginz.AudioTool.Scripts.StepEditor.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Dragginz.AudioTool.Scripts.StepEditor
{
    public class EditorController : MonoBehaviour
    {
        [SerializeField] private TMP_Text textVersion;
        [SerializeField] private TMP_Text textMessage;
        [SerializeField] private GameObject prefabTrackInfo;
        [SerializeField] private GameObject prefabBarHeader;
        [SerializeField] private GameObject prefabRegion;
        [SerializeField] private GameObject posMarker;
        [SerializeField] private RectTransform regionMarker;
        [SerializeField] private GameObject viewportRegions;
        [SerializeField] private AudioSource audioError;
        
        private UiControllerEditor _uiControllerEditor;
        private UiControllerTrackInfo uiControllerTrackInfo;
        private UiControllerRegionInfo uiControllerRegionInfo;

        private AudioEngine _audioEngine;

        private List<ScriptableObjectChord> _listChordObjects;
        private List<ScriptableObjectInstrument> _listInstrumentObjects;
        
        private RectTransform _rectTransformPosMarker;
        private Vector2 _posMarkerPos;
        private const float PosMarkerStartX = 180.0f;

        private float _timer;
        private float _lastRegionMarkerUpdate;
        private Globals.MouseRegionBeatPos _mouseRegionBeatPos;
        
        private Track _curTrackEdit;
        private Region _curRegionEdit;
        
        private void Awake()
        {
            textVersion.text = Globals.Version;
            textMessage.text = "";
            
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
            _mouseRegionBeatPos = new Globals.MouseRegionBeatPos();
                
            LoadIntervalsFromScriptableObjects();
            LoadInstrumentsFromScriptableObjects();
            
            _uiControllerEditor.PopulateInstrumentsDropDown(_listInstrumentObjects);
            
            uiControllerTrackInfo.PopulateInstrumentsDropDown(_listInstrumentObjects);

            uiControllerRegionInfo.PopulateLengthsDropDown();
            uiControllerRegionInfo.PopulateKeysDropDown(Globals.Keys);
            uiControllerRegionInfo.PopulateIntervalsDropDown(_listChordObjects);
            uiControllerRegionInfo.PopulateArpeggiatorNotesDropDown(Globals.ArpeggiatorNotes);
            uiControllerRegionInfo.PopulateChordNotesDropDown(Globals.ChordNotes);
            
            CreateBarHeaders();
            
            _audioEngine.InitChordList(_listChordObjects);
            _audioEngine.InitInstrumentList(_listInstrumentObjects);
            
            _uiControllerEditor.Init();
            uiControllerTrackInfo.Init();
            uiControllerRegionInfo.Init();
            
            _uiControllerEditor.OnButtonDemoEvent += OnButtonDemoClick;
            _uiControllerEditor.OnButtonClearEvent += OnButtonClearClick;
            _uiControllerEditor.OnButtonLoadEvent += OnLoadComposition;
            _uiControllerEditor.OnButtonSaveEvent += OnSaveComposition;
            
            _uiControllerEditor.OnButtonAddTrackEvent += OnButtonAddTrackClick;
            _uiControllerEditor.OnButtonPlayEvent += OnButtonPlayClick;
            _uiControllerEditor.OnButtonStopEvent += OnButtonStopClick;
            _uiControllerEditor.OnButtonLoopEvent += OnButtonLoopClick;
            _uiControllerEditor.OnActionAddRegionEvent += OnActionAddRegion;
            
            uiControllerTrackInfo.OnDropDownInstrumentEvent += TrackInfoInstrumentChange;
            uiControllerTrackInfo.OnDropDownReverbFilterEvent += TrackInfoReverbFilterChange;
            uiControllerTrackInfo.OnButtonMoveUpEvent += TrackMoveUpEvent;
            uiControllerTrackInfo.OnButtonMoveDownEvent += TrackMoveDownEvent;
            uiControllerTrackInfo.OnButtonDeleteEvent += TrackInfoDeleteEvent;
            
            uiControllerRegionInfo.OnDropDownLengthEvent += RegionInfoLengthChange;
            uiControllerRegionInfo.OnDropDownKeyEvent += RegionInfoKeyChange;
            uiControllerRegionInfo.OnDropDownIntervalEvent += RegionInfoIntervalChange;
            uiControllerRegionInfo.OnDropDownOctaveEvent += RegionInfoOctaveChange;
            uiControllerRegionInfo.OnDropDownTypeEvent += RegionInfoTypeChange;
            uiControllerRegionInfo.OnDropDownNoteEvent += RegionInfoNoteChange;
            uiControllerRegionInfo.OnDropDownChordNoteEvent += RegionInfoChordNoteChange;
            uiControllerRegionInfo.OnArpeggiatorUpdateEvent += RegionInfoArpeggiatorUpdateChange;
            uiControllerRegionInfo.OnButtonRegionSizeEvent += OnRegionSizeChange;
            
            uiControllerRegionInfo.OnSliderVolumeEvent += RegionInfoVolumeChange;
            uiControllerRegionInfo.OnSliderPanEvent += RegionInfoPanChange;
            uiControllerRegionInfo.OnToggleRootNoteOnlyEvent += RegionInfoRootNoteOnlyChange;
            uiControllerRegionInfo.OnToggleHighOctaveEvent += RegionInfoHighOctaveChange;
            uiControllerRegionInfo.OnButtonDeleteEvent += RegionInfoDeleteEvent;
        }

        private void Update()
        {
            _timer = Time.realtimeSinceStartup;
            
            if (_audioEngine.IsPlaying)
            {
                _posMarkerPos.x = PosMarkerStartX + _uiControllerEditor.regionContentOffset + (_audioEngine.curBeat * Globals.PrefabBarBeatWidth);
                _rectTransformPosMarker.anchoredPosition = _posMarkerPos;
            }

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

            if (uiControllerTrackInfo.IsVisible || uiControllerRegionInfo.IsVisible)
            {
                if (regionMarker.gameObject.activeSelf) regionMarker.gameObject.SetActive(false);
                return;
            }
            
            if (_lastRegionMarkerUpdate < _timer)
            {
                _lastRegionMarkerUpdate = _timer + 0.1f;

                var hideMarker = true;
                
                // Check if the mouse is over the regions viewport
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    var pointerEventData = new PointerEventData(EventSystem.current) {
                        position = Input.mousePosition
                    };

                    var raycastResults = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointerEventData, raycastResults);

                    if (raycastResults.Count > 0)
                    {
                        if (raycastResults[0].gameObject == viewportRegions)
                        {
                            UpdateRegionMarker();
                            hideMarker = false;
                        }
                    }
                }
                
                if (hideMarker && regionMarker.gameObject.activeSelf) regionMarker.gameObject.SetActive(false);
            }
        }

        private void UpdateRegionMarker()
        {
            _mouseRegionBeatPos = _uiControllerEditor.GetRegionBeatPos();

            if (_mouseRegionBeatPos.trackPos < 0) _mouseRegionBeatPos.trackPos = 0;
            
            var track = _audioEngine.GetTrack(_mouseRegionBeatPos.trackPos);
            if (track == null) {
                regionMarker.gameObject.SetActive(false);
                return;
            }
                
            _mouseRegionBeatPos = _audioEngine.ValidateRegionPosAndSize(track, _mouseRegionBeatPos);

            if (!_mouseRegionBeatPos.posIsValid) {
                regionMarker.gameObject.SetActive(false);
                return;
            }
                
            var size = new Vector2Int {
                x = Globals.PrefabBarBeatWidth * _mouseRegionBeatPos.numBeats,
                y = Globals.PrefabTrackHeight
            };
            regionMarker.sizeDelta = size;
                
            var pos = new Vector2Int {
                x = _mouseRegionBeatPos.regionStartPos * Globals.PrefabBarBeatWidth,
                y = _mouseRegionBeatPos.trackPos * Globals.PrefabTrackHeight * -1
            };
            regionMarker.anchoredPosition = pos;
                
            if (!regionMarker.gameObject.activeSelf) regionMarker.gameObject.SetActive(true);
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
        
        /*private void LoadPatternsFromScriptableObjects()
        {
            var soPatterns = Resources.LoadAll("ScriptableObjects/Patterns", typeof(ScriptableObjectPattern));
            if (soPatterns == null) return;
        
            var listPatterns = soPatterns.Select(instrument => instrument as ScriptableObjectPattern).ToList();
            _listPatternObjects = listPatterns.OrderBy(o=>o.sortOrder).ToList();
        }*/

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

        public void UpdateRegionGameObjectPosAndSize(Region regionUpdate)
        {
            var uiRegion = regionUpdate.RegionUi;
            if (uiRegion == null) return;
            
            var pos = uiRegion.rectTransform.anchoredPosition;
            
            var prefabBarWidth = Globals.PrefabBarWidth / 4;
            var w = prefabBarWidth * regionUpdate.beats;
            
            var size = uiRegion.rectTransform.sizeDelta;
            size.x = w;
            uiRegion.rectTransform.sizeDelta = size;

            pos.x = prefabBarWidth * regionUpdate.startPosBeats;
            uiRegion.rectTransform.anchoredPosition = pos;
        }
        
        // UI EVENTS
        
        private void OnButtonAddTrackClick(int instrument)
        {
            var soInstrument = _listInstrumentObjects[instrument];
            
            var track = new Track();
            track.Init(_audioEngine.Tracks.Count, soInstrument);
            
            CreateTrackGameObject(track);
            
            _audioEngine.AddTrack(track);
        }

        private void OnActionAddRegion()
        {
            if (!_mouseRegionBeatPos.posIsValid) return;
            
            //Debug.Log("trackPos, regionPos: "+trackPos+", "+regionPos);
            //_audioEngine.AddRegion(trackPos, regionPos);
            var track = _audioEngine.GetTrack(_mouseRegionBeatPos.trackPos);
            if (track == null) return;
            
            _curRegionEdit = _audioEngine.CreateNewRegion(track, _mouseRegionBeatPos);
            if (_curRegionEdit != null)
            {
                OnButtonStopClick();
                ShowMessage("");
                
                CreateRegionGameObject(track, _curRegionEdit, track.Instrument);
                uiControllerRegionInfo.ShowRegionInfo(_curRegionEdit);
            }
        }
        
        //
        
        private void OnButtonPlayClick()
        {
            ShowMessage("");
            _uiControllerEditor.AudioIsPlaying(_audioEngine.StartPlayback());
        }
        
        private void OnButtonStopClick()
        {
            ShowMessage("");
            _audioEngine.StopPlayback();
            _uiControllerEditor.AudioIsPlaying(false);
        }
        
        private void OnButtonLoopClick()
        {
            _audioEngine.ToggleLoop();
            _uiControllerEditor.AudioIsLooping(_audioEngine.loop);
        }
        
        //
        
        private void TrackInfoInstrumentChange(int instrument)
        {
            if (_curTrackEdit == null) return;
            
            _audioEngine.UpdateTrackInstrument(_curTrackEdit, _listInstrumentObjects[instrument]);
        }
        
        private void TrackInfoReverbFilterChange(int value)
        {
            _curTrackEdit?.UpdateReverbFilter(value);
        }
        
        private void TrackMoveUpEvent()
        {
            if (_curTrackEdit == null) return;

            if (_audioEngine.MoveTrack(_curTrackEdit.Position, -1))
            {
                uiControllerTrackInfo.ShowTrackInfo(_curTrackEdit); // refresh
            }
            else
            {
                ErrorMessage();
            }
        }
        private void TrackMoveDownEvent()
        {
            if (_curTrackEdit == null) return;
            
            if (_audioEngine.MoveTrack(_curTrackEdit.Position, 1))
            {
                uiControllerTrackInfo.ShowTrackInfo(_curTrackEdit); // refresh
            }
            else
            {
                ErrorMessage();
            }
        }

        private void TrackInfoDeleteEvent()
        {
            if (_curTrackEdit != null) {
                _audioEngine.DeleteTrack(_curTrackEdit);
            }
        }
        
        //
        
        private void RegionInfoLengthChange(int length)
        {
            if (_curRegionEdit == null) return;
            
            _audioEngine.UpdateRegionLength(_curRegionEdit, length+1);
            uiControllerRegionInfo.ShowRegionInfoHeader(_curRegionEdit); // refresh
        }

        private void RegionInfoKeyChange(int key)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Key = key;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }
        
        private void RegionInfoIntervalChange(int interval)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Interval = interval;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }

        private void RegionInfoOctaveChange(int octave)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Octave = octave;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }

        private void RegionInfoTypeChange(int type)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Type = type;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
            uiControllerRegionInfo.UpdateGroupVisibility(_curRegionEdit.playbackSettings);
        }

        private void RegionInfoArpeggiatorUpdateChange(ArpeggiatorData data)
        {
            if (_curRegionEdit == null) return;
            
            _audioEngine.UpdateRegionArpeggiatorData(_curRegionEdit, data);
        }
        
        private void RegionInfoNoteChange(int note)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Note = note;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }
        
        private void RegionInfoChordNoteChange(int note)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Note = note;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }
        
        private void RegionInfoVolumeChange(float value)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Volume = value;
            if (_curRegionEdit.isPlaying) {
                _audioEngine.SetLiveVolume(_curRegionEdit);
            }
        }
        
        private void RegionInfoPanChange(float value)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.Pan = value;
            if (_curRegionEdit.isPlaying) {
                _audioEngine.SetLivePan(_curRegionEdit);
            }
        }
        
        private void RegionInfoRootNoteOnlyChange(bool value)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.RootNoteOnly = value;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }
        
        private void RegionInfoHighOctaveChange(bool value)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.playbackSettings.HighOctave = value;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }

        private void RegionInfoDeleteEvent()
        {
            if (_curRegionEdit != null) {
                _audioEngine.DeleteRegion(_curRegionEdit);
            }
        }
        
        private void OnRegionSizeChange(Globals.RegionSizeControls action)
        {
            if (_curRegionEdit == null) return;
            
            var success = _audioEngine.ChangeRegionSizePos(_curRegionEdit, action);
            if (!success) ErrorMessage();
        }

        // Load & Save
        
        private void OnButtonDemoClick()
        {
            OnButtonStopClick();
            
            try
            {
                var demo = Resources.Load("Data/demo");
                var composition = JsonUtility.FromJson<DataComposition>(demo.ToString());
                
                _audioEngine.CreateDemoProject(composition);
                
                ShowMessage("loading data from: Resources/Data/demo.json");
            }
            catch (Exception e)
            {
                ErrorMessage(e.ToString());
            }
        }

        private void OnButtonClearClick()
        {
            ShowMessage("");
            OnButtonStopClick();
            
            _audioEngine.ClearProject();
        }

        private void OnSaveComposition()
        {
            OnButtonStopClick();

            try
            {
                var composition = _audioEngine.GetSaveData();
                var json = JsonUtility.ToJson(composition);

                var filePath = Application.persistentDataPath + "/composition.json";
                File.WriteAllText(filePath, json);
                
                ShowMessage("saving data to: " + filePath);
            }
            catch (Exception e)
            {
                ErrorMessage(e.ToString());
            }
        }
        
        private void OnLoadComposition()
        {
            OnButtonStopClick();

            var filePath= Application.persistentDataPath + "/composition.json";

            try
            {
                var json = File.ReadAllText(filePath);
                var composition = JsonUtility.FromJson<DataComposition>(json);
                
                _audioEngine.CreateDemoProject(composition);
                
                ShowMessage("loading data from: " + filePath);
            }
            catch (Exception e)
            {
                ErrorMessage(e.ToString());
            }
        }
        
        // Callback methods
        
        private void CallbackTrackClick(int trackPos, uint clickAction)
        {
            //Debug.Log("CallbackTrackClick - track, action: "+trackPos+", "+clickAction);

            OnButtonStopClick();
            ShowMessage("");
            
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
            ShowMessage("");
            
            _curRegionEdit = _audioEngine.GetRegion(trackPos, regionStartPos);
            if (_curRegionEdit != null) uiControllerRegionInfo.ShowRegionInfo(_curRegionEdit);
        }

        // 
        
        private void ErrorMessage(string msg = null)
        {
            if (audioError != null) audioError.Play();

            if (!string.IsNullOrEmpty(msg)) ShowMessage(msg);
        }

        private void ShowMessage(string msg)
        {
            textMessage.text = msg;
            if (!string.IsNullOrEmpty(msg)) Debug.Log(msg);
        }
    }
}
