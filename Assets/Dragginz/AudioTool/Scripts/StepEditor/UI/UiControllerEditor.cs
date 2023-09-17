using System;
using System.Runtime.CompilerServices;
using Dragginz.AudioTool.Scripts.Includes;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace Dragginz.AudioTool.Scripts.StepEditor.UI
{
    public class UiControllerEditor : MonoBehaviour
    {
        // UI Elements
        
        [SerializeField] private Button buttonDemo;
        [SerializeField] private Button buttonClear;
        
        [SerializeField] private Button buttonAddTrack;
        [SerializeField] private Button buttonPlay;
        [SerializeField] private Button buttonStop;
        
        [SerializeField] private Scrollbar scrollbarTracks;
        [SerializeField] private Scrollbar scrollbarBeats;
        [SerializeField] private GameObject goContentTracks;
        [SerializeField] private GameObject goContentBeat;
        [SerializeField] private GameObject goContentRegions;
        
        [SerializeField] private RectTransform rectTransformBarsContent;
        [SerializeField] private RectTransform rectTransformRegionsContent;
        
        [SerializeField] private RectTransform rectTransformRegions;
        
        public delegate void ButtonAddTrackEvent();
        public event ButtonAddTrackEvent OnButtonAddTrackEvent;
        
        public delegate void ActionAddRegionEvent();
        public event ActionAddRegionEvent OnActionAddRegionEvent;

        public delegate void ButtonPlayEvent();
        public event ButtonPlayEvent OnButtonPlayEvent;
        
        public delegate void ButtonStopEvent();
        public event ButtonStopEvent OnButtonStopEvent;
    
        public delegate void ButtonDemoEvent();
        public event ButtonDemoEvent OnButtonDemoEvent;
        
        public delegate void ButtonClearEvent();
        public event ButtonClearEvent OnButtonClearEvent;
        
        // Getters
        public GameObject GoContentTracks => goContentTracks;
        public GameObject GoContentBeat => goContentBeat;
        public GameObject GoContentRegions => goContentRegions;

        // MAIN EVENTS

        private void Awake()
        {
            AudioIsPlaying(false);
        }

        public void Init()
        {
            scrollbarTracks.value = 1;
            scrollbarBeats.value = 0;
            
            buttonAddTrack.onClick.AddListener(OnButtonAddTrackClick);
            buttonPlay.onClick.AddListener(OnButtonPlayClick);
            buttonStop.onClick.AddListener(OnButtonStopClick);
            buttonDemo.onClick.AddListener(OnButtonDemoClick);
            buttonClear.onClick.AddListener(OnButtonClearClick);
            
            scrollbarBeats.onValueChanged.AddListener(OnScrollbarBeatsChange);
        }

        private void OnButtonAddTrackClick()
        {
            OnButtonAddTrackEvent?.Invoke();
        }
        
        private void OnButtonPlayClick()
        {
            OnButtonPlayEvent?.Invoke();
        }
        
        private void OnButtonStopClick()
        {
            OnButtonStopEvent?.Invoke();
        }
        
        private void OnButtonDemoClick()
        {
            OnButtonDemoEvent?.Invoke();
        }
        
        private void OnButtonClearClick()
        {
            OnButtonClearEvent?.Invoke();
        }

        private void OnScrollbarBeatsChange(float value)
        {
            var anchoredPosition = rectTransformRegionsContent.anchoredPosition;
            anchoredPosition.x = rectTransformBarsContent.anchoredPosition.x;
            rectTransformRegionsContent.anchoredPosition = anchoredPosition;
        }

        // Public Methods
        
        public void AudioIsPlaying(bool startPlayback)
        {
            buttonPlay.interactable = !startPlayback;
            buttonStop.interactable = !buttonPlay.interactable;
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
            //var regionBeatPos = GetRegionBeatPos();
            
            OnActionAddRegionEvent?.Invoke();
        }
    }
}
