using System;
using System.Collections.Generic;
using Dragginz.AudioTool.Scripts.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dragginz.AudioTool.Scripts.StepEditor.UI
{
    public class UiControllerTrackInfo : MonoBehaviour
    {
        // UI Elements
        
        [SerializeField] private GameObject goTrackInfoPanel;
        [SerializeField] private Button buttonClose;
        [SerializeField] private TMP_Text labelTrackInfo;
        
        [SerializeField] private TMP_Dropdown dropDownInstruments;
        
        [SerializeField] private TMP_Dropdown dropDownReverbFilter;
        
        [SerializeField] private Button buttonMoveUp;
        [SerializeField] private Button buttonMoveDown;
        [SerializeField] private Button buttonDelete;
        
        // Delegates
        
        public delegate void DropDownInstrumentEvent(int instrument);
        public event DropDownInstrumentEvent OnDropDownInstrumentEvent;
        
        public delegate void DropDownReverbFilterEvent(int value);
        public event DropDownReverbFilterEvent OnDropDownReverbFilterEvent;

        public delegate void ButtonMoveUpEvent();
        public event ButtonMoveUpEvent OnButtonMoveUpEvent;
        
        public delegate void ButtonMoveDownEvent();
        public event ButtonMoveDownEvent OnButtonMoveDownEvent;
        
        public delegate void ButtonDeleteEvent();
        public event ButtonDeleteEvent OnButtonDeleteEvent;
        
        //

        private bool _uiIsUpdating;
        
        // Getters

        public bool IsVisible => goTrackInfoPanel.activeSelf;
        
        // System methods

        private void Awake()
        {
            goTrackInfoPanel.SetActive(false);
        }

        // Public methods
        
        public void Init()
        {
            PopulateAudioFilterDropDowns();
            
            buttonClose.onClick.AddListener(Hide);
            
            dropDownInstruments.onValueChanged.AddListener(delegate { OnDropDownInstrumentChanged(); });
            dropDownReverbFilter.onValueChanged.AddListener(delegate { OnDropDownReverbFilterChanged(); });
            
            buttonMoveUp.onClick.AddListener(MoveTrackUp);
            buttonMoveDown.onClick.AddListener(MoveTrackDown);

            buttonDelete.onClick.AddListener(DeleteTrack);
        }

        private void PopulateAudioFilterDropDowns()
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (int i in Enum.GetValues(typeof(AudioReverbPreset))) {
                var s = Enum.GetName(typeof(AudioReverbPreset), i);
                optionData.Add(new TMP_Dropdown.OptionData(s));
            }
            optionData.RemoveAt(optionData.Count-1); // remove user preset
            dropDownReverbFilter.options = optionData;
            dropDownReverbFilter.value = 0;
        }
        
        public void PopulateInstrumentsDropDown(List<ScriptableObjectInstrument> sortedListInstruments)
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (var instrument in sortedListInstruments) {
                optionData.Add(new TMP_Dropdown.OptionData(instrument.name));
            }
            dropDownInstruments.options = optionData;
        }
        
        public void ShowTrackInfo(Track track)
        {
            _uiIsUpdating = true;
            
            goTrackInfoPanel.SetActive(true);

            var header = "Track " + (track.Position+1) + " - " + track.Instrument.name;
            labelTrackInfo.text = header;

            dropDownInstruments.value = (int)track.Instrument.sortOrder;

            dropDownReverbFilter.value = track.ReverbFilter;
            
            _uiIsUpdating = false;
        }

        // Ui events
        
        public void Hide()
        {
            goTrackInfoPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        private void OnDropDownReverbFilterChanged() {
            if (!_uiIsUpdating) OnDropDownReverbFilterEvent?.Invoke(dropDownReverbFilter.value);
        }
        
        private void MoveTrackUp() {
            OnButtonMoveUpEvent?.Invoke();
        }
        private void MoveTrackDown() {
            OnButtonMoveDownEvent?.Invoke();
        }
        
        private void DeleteTrack()
        {
            goTrackInfoPanel.SetActive(false);
            
            OnButtonDeleteEvent?.Invoke();
        }

        private void OnDropDownInstrumentChanged()
        {
            if (!_uiIsUpdating) OnDropDownInstrumentEvent?.Invoke(dropDownInstruments.value);

            Hide(); // close info screen
        }
    }
}
