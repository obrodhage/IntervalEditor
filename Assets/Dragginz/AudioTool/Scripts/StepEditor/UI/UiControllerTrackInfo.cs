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
        
        [SerializeField] private Toggle toggleReverb;
        
        [SerializeField] private Button buttonMoveUp;
        [SerializeField] private Button buttonMoveDown;
        [SerializeField] private Button buttonDelete;
        
        // Delegates
        
        public delegate void DropDownInstrumentEvent(int instrument);
        public event DropDownInstrumentEvent OnDropDownInstrumentEvent;
        
        public delegate void ToggleReverbFilterEvent(bool value);
        public event ToggleReverbFilterEvent OnToggleReverbFilterEvent;

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
            buttonClose.onClick.AddListener(Hide);
            
            dropDownInstruments.onValueChanged.AddListener(delegate { OnDropDownInstrumentChanged(); });
            toggleReverb.onValueChanged.AddListener(delegate { OnToggleReverbFilterChanged(toggleReverb.isOn); });
            
            buttonMoveUp.onClick.AddListener(MoveTrackUp);
            buttonMoveDown.onClick.AddListener(MoveTrackDown);

            buttonDelete.onClick.AddListener(DeleteTrack);
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

            toggleReverb.isOn = track.reverbFilter;
            
            _uiIsUpdating = false;
        }

        // Ui events
        
        public void Hide()
        {
            goTrackInfoPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        private void OnToggleReverbFilterChanged(bool value) {
            if (!_uiIsUpdating) OnToggleReverbFilterEvent?.Invoke(value);
        }
        
        private void MoveTrackUp()
        {
            //goTrackInfoPanel.SetActive(false);
            
            OnButtonMoveUpEvent?.Invoke();
        }
        private void MoveTrackDown()
        {
            //goTrackInfoPanel.SetActive(false);
            
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
