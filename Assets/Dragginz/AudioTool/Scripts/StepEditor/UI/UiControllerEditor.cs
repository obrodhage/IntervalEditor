using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dragginz.AudioTool.Scripts.Includes;
using Dragginz.AudioTool.Scripts.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace Dragginz.AudioTool.Scripts.StepEditor.UI
{
    public class UiControllerEditor : MonoBehaviour
    {
        // UI Elements
        
        [SerializeField] private Button buttonDemo;
        [SerializeField] private Button buttonClear;
        [SerializeField] private Button buttonLoad;
        [SerializeField] private Button buttonSave;
        
        [SerializeField] private TMP_Dropdown dropDownInstruments;
        [SerializeField] private Button buttonAddTrack;
        [SerializeField] private Button buttonPlay;
        [SerializeField] private Button buttonStop;
        [SerializeField] private Button buttonLoop;
        
        [SerializeField] private Scrollbar scrollbarTracks;
        [SerializeField] private Scrollbar scrollbarBeats;
        [SerializeField] private GameObject goContentTracks;
        [SerializeField] private GameObject goContentBeat;
        [SerializeField] private GameObject goContentRegions;
        
        [SerializeField] private RectTransform rectTransformBarsContent;
        [SerializeField] private RectTransform rectTransformRegionsContent;
        
        [SerializeField] private RectTransform rectTransformRegions;
        
        public delegate void ButtonAddTrackEvent(int instrument);
        public event ButtonAddTrackEvent OnButtonAddTrackEvent;
        
        public delegate void ActionAddRegionEvent();
        public event ActionAddRegionEvent OnActionAddRegionEvent;

        public delegate void ButtonPlayEvent();
        public event ButtonPlayEvent OnButtonPlayEvent;
        
        public delegate void ButtonStopEvent();
        public event ButtonStopEvent OnButtonStopEvent;
    
        public delegate void ButtonLoopEvent();
        public event ButtonLoopEvent OnButtonLoopEvent;
        
        public delegate void ButtonDemoEvent();
        public event ButtonDemoEvent OnButtonDemoEvent;
        
        public delegate void ButtonClearEvent();
        public event ButtonClearEvent OnButtonClearEvent;

        public delegate void ButtonLoadEvent();
        public event ButtonLoadEvent OnButtonLoadEvent;
        
        public delegate void ButtonSaveEvent();
        public event ButtonSaveEvent OnButtonSaveEvent;
        
        
        public float regionContentOffset;

        private int _curInstrument;
        
        // Getters
        public GameObject GoContentTracks => goContentTracks;
        public GameObject GoContentBeat => goContentBeat;
        public GameObject GoContentRegions => goContentRegions;

        // MAIN EVENTS

        private void Awake()
        {
            AudioIsPlaying(false);
        }

        public void PopulateInstrumentsDropDown(List<ScriptableObjectInstrument> sortedListInstruments)
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (var instrument in sortedListInstruments) {
                optionData.Add(new TMP_Dropdown.OptionData(instrument.name));
            }
            dropDownInstruments.options = optionData;
        }

        public void Init()
        {
            scrollbarTracks.value = 1;
            scrollbarBeats.value = 0;
            
            _curInstrument = dropDownInstruments.value = 0;
            
            dropDownInstruments.onValueChanged.AddListener(delegate { OnDropDownInstrumentChanged(); });
            buttonAddTrack.onClick.AddListener(OnButtonAddTrackClick);
            buttonPlay.onClick.AddListener(OnButtonPlayClick);
            buttonStop.onClick.AddListener(OnButtonStopClick);
            buttonLoop.onClick.AddListener(OnButtonLoopClick);
            buttonDemo.onClick.AddListener(OnButtonDemoClick);
            buttonClear.onClick.AddListener(OnButtonClearClick);
            buttonLoad.onClick.AddListener(OnButtonLoadClick);
            buttonSave.onClick.AddListener(OnButtonSaveClick);
            
            scrollbarBeats.onValueChanged.AddListener(OnScrollbarBeatsChange);
        }

        private void OnDropDownInstrumentChanged() {
            _curInstrument = dropDownInstruments.value;
        }
        private void OnButtonAddTrackClick() {
            OnButtonAddTrackEvent?.Invoke(_curInstrument);
        }
        private void OnButtonPlayClick() {
            OnButtonPlayEvent?.Invoke();
        }
        private void OnButtonStopClick() {
            OnButtonStopEvent?.Invoke();
        }
        private void OnButtonLoopClick() {
            OnButtonLoopEvent?.Invoke();
        }
        private void OnButtonDemoClick() {
            OnButtonDemoEvent?.Invoke();
        }
        private void OnButtonClearClick() {
            OnButtonClearEvent?.Invoke();
        }
        private void OnButtonLoadClick() {
            OnButtonLoadEvent?.Invoke();
        }
        private void OnButtonSaveClick() {
            OnButtonSaveEvent?.Invoke();
        }
        
        private void OnScrollbarBeatsChange(float value)
        {
            regionContentOffset = rectTransformBarsContent.anchoredPosition.x;
            
            var anchoredPosition = rectTransformRegionsContent.anchoredPosition;
            anchoredPosition.x = regionContentOffset;
            rectTransformRegionsContent.anchoredPosition = anchoredPosition;
        }

        // Public Methods
        
        public void AudioIsPlaying(bool startPlayback)
        {
            buttonPlay.interactable = !startPlayback;
            buttonStop.interactable = !buttonPlay.interactable;

            buttonDemo.interactable = !startPlayback;
            buttonClear.interactable = !startPlayback;
        }

        public void AudioIsLooping(bool loop)
        {
            buttonLoop.targetGraphic.color = loop ? Color.white : new Color(1,1,1,0.5f);
        }
        
        public Globals.MouseRegionBeatPos GetRegionBeatPos()
        {
            var pos = new Globals.MouseRegionBeatPos();
            var mousePos = Input.mousePosition;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransformRegions, mousePos, null, out var localPos);
            var rect = rectTransformRegions.rect;
            localPos.x += rect.width / 2;
            localPos.x -= rectTransformRegionsContent.anchoredPosition.x; // add content offset
            localPos.y = Math.Abs(localPos.y - rect.height / 2);
            
            pos.trackPos = (int)(localPos.y / Globals.PrefabTrackHeight);
            pos.regionStartPos = (int)(localPos.x / Globals.PrefabBarBeatWidth - Globals.DefaultRegionBeats / 2);
            pos.numBeats = Globals.DefaultRegionBeats;
            pos.posIsValid = true;
            
            return pos;
        }
        
        public void OnButtonRegionsClick()
        {
            OnActionAddRegionEvent?.Invoke();
        }
    }
}
