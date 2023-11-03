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
        private UiControllerTrackInfo _uiControllerTrackInfo;
        private UiControllerRegionInfo _uiControllerRegionInfo;

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
            
            _uiControllerTrackInfo = FindObjectOfType<UiControllerTrackInfo>();
            if (_uiControllerTrackInfo == null) {
                Debug.LogError("Couldn't find Component UiControllerTrackInfo!");
            }
            
            _uiControllerRegionInfo = FindObjectOfType<UiControllerRegionInfo>();
            if (_uiControllerRegionInfo == null) {
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
            
            _uiControllerTrackInfo.PopulateInstrumentsDropDown(_listInstrumentObjects);

            _uiControllerRegionInfo.PopulateLengthsDropDown();
            _uiControllerRegionInfo.PopulateKeysDropDown(Globals.Keys);
            _uiControllerRegionInfo.PopulateIntervalsDropDown(_listChordObjects);
            _uiControllerRegionInfo.PopulateArpeggiatorNotesDropDown(Globals.ArpeggiatorNotes);
            _uiControllerRegionInfo.PopulateChordNotesDropDown(Globals.ChordNotes);
            _uiControllerRegionInfo.PopulateMelodyNotesDropDown(Globals.MelodyNotes);
            
            CreateBarHeaders();
            
            _audioEngine.InitChordList(_listChordObjects);
            _audioEngine.InitInstrumentList(_listInstrumentObjects);
            
            _uiControllerEditor.Init();
            _uiControllerTrackInfo.Init();
            _uiControllerRegionInfo.Init();
            
            _uiControllerEditor.OnButtonDemoEvent += OnButtonDemoClick;
            _uiControllerEditor.OnButtonClearEvent += OnButtonClearClick;
            _uiControllerEditor.OnButtonLoadEvent += OnLoadComposition;
            _uiControllerEditor.OnButtonSaveEvent += OnSaveComposition;
            
            _uiControllerEditor.OnButtonAddTrackEvent += OnButtonAddTrackClick;
            _uiControllerEditor.OnButtonPlayEvent += OnButtonPlayClick;
            _uiControllerEditor.OnButtonStopEvent += OnButtonStopClick;
            _uiControllerEditor.OnButtonLoopEvent += OnButtonLoopClick;
            _uiControllerEditor.OnActionAddRegionEvent += OnActionAddRegion;
            
            _uiControllerTrackInfo.OnDropDownInstrumentEvent += TrackInfoInstrumentChange;
            _uiControllerTrackInfo.OnDropDownReverbFilterEvent += TrackInfoReverbFilterChange;
            _uiControllerTrackInfo.OnButtonMoveUpEvent += TrackMoveUpEvent;
            _uiControllerTrackInfo.OnButtonMoveDownEvent += TrackMoveDownEvent;
            _uiControllerTrackInfo.OnButtonDeleteEvent += TrackInfoDeleteEvent;
            
            _uiControllerRegionInfo.OnDropDownLengthEvent += RegionInfoLengthChange;
            _uiControllerRegionInfo.OnDropDownKeyEvent += RegionInfoKeyChange;
            _uiControllerRegionInfo.OnDropDownIntervalEvent += RegionInfoIntervalChange;
            _uiControllerRegionInfo.OnDropDownOctaveEvent += RegionInfoOctaveChange;
            _uiControllerRegionInfo.OnDropDownTypeEvent += RegionInfoTypeChange;
            _uiControllerRegionInfo.OnDropDownNoteEvent += RegionInfoNoteChange;
            _uiControllerRegionInfo.OnDropDownChordNoteEvent += RegionInfoChordNoteChange;
            _uiControllerRegionInfo.OnDropDownMelodyNoteEvent += RegionInfoMelodyNoteChange;
            _uiControllerRegionInfo.OnArpeggiatorUpdateEvent += RegionInfoArpeggiatorUpdate;
            _uiControllerRegionInfo.OnMelodyMakerUpdateEvent += RegionInfoMelodyMakerUpdate;
            _uiControllerRegionInfo.OnButtonRegionSizeEvent += OnRegionSizeChange;
            
            _uiControllerRegionInfo.OnSliderVolumeEvent += RegionInfoVolumeChange;
            _uiControllerRegionInfo.OnSliderPanEvent += RegionInfoPanChange;
            _uiControllerRegionInfo.OnToggleRootNoteOnlyEvent += RegionInfoRootNoteOnlyChange;
            _uiControllerRegionInfo.OnToggleHighOctaveEvent += RegionInfoHighOctaveChange;
            _uiControllerRegionInfo.OnButtonDeleteEvent += RegionInfoDeleteEvent;
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
                if (_uiControllerTrackInfo.IsVisible) _uiControllerTrackInfo.Hide();
                if (_uiControllerRegionInfo.IsVisible) _uiControllerRegionInfo.Hide();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                var playing = _audioEngine.StartPlayback();
                if (!playing) _audioEngine.StopPlayback();
                _uiControllerEditor.AudioIsPlaying(playing);
            }

            if (_uiControllerTrackInfo.IsVisible || _uiControllerRegionInfo.IsVisible)
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

            if (_mouseRegionBeatPos.TrackPos < 0) _mouseRegionBeatPos.TrackPos = 0;
            
            var track = _audioEngine.GetTrack(_mouseRegionBeatPos.TrackPos);
            if (track == null) {
                regionMarker.gameObject.SetActive(false);
                return;
            }
                
            _mouseRegionBeatPos = _audioEngine.ValidateRegionPosAndSize(track, _mouseRegionBeatPos);

            if (!_mouseRegionBeatPos.PosIsValid) {
                regionMarker.gameObject.SetActive(false);
                return;
            }
                
            var size = new Vector2Int {
                x = Globals.PrefabBarBeatWidth * _mouseRegionBeatPos.NumBeats,
                y = Globals.PrefabTrackHeight
            };
            regionMarker.sizeDelta = size;
                
            var pos = new Vector2Int {
                x = _mouseRegionBeatPos.RegionStartPos * Globals.PrefabBarBeatWidth,
                y = _mouseRegionBeatPos.TrackPos * Globals.PrefabTrackHeight * -1
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
                    
            var key = Globals.Keys[region.PlaybackSettings.Key];
            var chord = _listChordObjects[region.PlaybackSettings.Interval].name;
            if (region.PlaybackSettings.Type == (int) InstrumentType.MelodyMaker) {
                chord = Enum.GetName(typeof(MelodyMode), region.PlaybackSettings.MelodyData.Mode);
            }
            
            var prefabBarWidth = Globals.PrefabBarWidth / 4;
            var w = prefabBarWidth * region.Beats;
            uiRegion.Init(w, instrument.defaultColor, key, chord);
            
            pos.x = prefabBarWidth * region.StartPosBeats;
            pos.y = track.Position * Globals.PrefabTrackHeight * -1;
            uiRegion.rectTransform.anchoredPosition = pos;
        }

        public void UpdateRegionGameObjectPosAndSize(Region regionUpdate)
        {
            var uiRegion = regionUpdate.RegionUi;
            if (uiRegion == null) return;
            
            var pos = uiRegion.rectTransform.anchoredPosition;
            
            var prefabBarWidth = Globals.PrefabBarWidth / 4;
            var w = prefabBarWidth * regionUpdate.Beats;
            
            var size = uiRegion.rectTransform.sizeDelta;
            size.x = w;
            uiRegion.rectTransform.sizeDelta = size;

            pos.x = prefabBarWidth * regionUpdate.StartPosBeats;
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
            if (!_mouseRegionBeatPos.PosIsValid) return;
            
            //Debug.Log("trackPos, regionPos: "+trackPos+", "+regionPos);
            //_audioEngine.AddRegion(trackPos, regionPos);
            var track = _audioEngine.GetTrack(_mouseRegionBeatPos.TrackPos);
            if (track == null) return;
            
            _curRegionEdit = _audioEngine.CreateNewRegion(track, _mouseRegionBeatPos);
            if (_curRegionEdit != null)
            {
                OnButtonStopClick();
                ShowMessage("");
                
                CreateRegionGameObject(track, _curRegionEdit, track.Instrument);
                _uiControllerRegionInfo.ShowRegionInfo(_curRegionEdit);
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
                _uiControllerTrackInfo.ShowTrackInfo(_curTrackEdit); // refresh
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
                _uiControllerTrackInfo.ShowTrackInfo(_curTrackEdit); // refresh
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
            _uiControllerRegionInfo.ShowRegionInfoHeader(_curRegionEdit); // refresh
        }

        private void RegionInfoKeyChange(int key)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.PlaybackSettings.Key = key;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }
        
        private void RegionInfoIntervalChange(int interval)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.PlaybackSettings.Interval = interval;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }

        private void RegionInfoOctaveChange(int octave)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.PlaybackSettings.Octave = octave;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }

        private void RegionInfoTypeChange(int type)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.PlaybackSettings.Type = type;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
            _uiControllerRegionInfo.UpdateGroupVisibility(_curRegionEdit.PlaybackSettings);
        }

        private void RegionInfoArpeggiatorUpdate(ArpeggiatorData data)
        {
            if (_curRegionEdit == null) return;
            
            _audioEngine.UpdateRegionArpeggiatorData(_curRegionEdit, data);
        }
        
        private void RegionInfoMelodyMakerUpdate(MelodyMakerData data)
        {
            if (_curRegionEdit == null) return;
            
            _audioEngine.UpdateRegionMelodyMakerData(_curRegionEdit, data);
        }
        
        private void RegionInfoNoteChange(int note)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.PlaybackSettings.Note = note;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }
        
        private void RegionInfoChordNoteChange(int note)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.PlaybackSettings.Note = note;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }
        
        private void RegionInfoMelodyNoteChange(int note)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.PlaybackSettings.Note = note;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }

        private void RegionInfoVolumeChange(float value)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.PlaybackSettings.Volume = value;
            if (_curRegionEdit.IsPlaying) {
                _audioEngine.SetLiveVolume(_curRegionEdit);
            }
        }
        
        private void RegionInfoPanChange(float value)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.PlaybackSettings.Pan = value;
            if (_curRegionEdit.IsPlaying) {
                _audioEngine.SetLivePan(_curRegionEdit);
            }
        }
        
        private void RegionInfoRootNoteOnlyChange(bool value)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.PlaybackSettings.RootNoteOnly = value;
            _audioEngine.UpdateRegionNew(_curRegionEdit);
        }
        
        private void RegionInfoHighOctaveChange(bool value)
        {
            if (_curRegionEdit == null) return;
            
            _curRegionEdit.PlaybackSettings.HighOctave = value;
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
                    _uiControllerTrackInfo.ShowTrackInfo(_curTrackEdit);
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
            if (_curRegionEdit != null) _uiControllerRegionInfo.ShowRegionInfo(_curRegionEdit);
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
