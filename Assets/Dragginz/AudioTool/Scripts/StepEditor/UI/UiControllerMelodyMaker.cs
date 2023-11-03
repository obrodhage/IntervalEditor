using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dragginz.AudioTool.Scripts.StepEditor.UI
{
    public class UiControllerMelodyMaker : MonoBehaviour
    {
        // UI Elements
        
        [SerializeField] private TMP_Dropdown dropDownOctaves;
        [SerializeField] private TMP_Dropdown dropDownMode;
        [SerializeField] private TMP_Dropdown dropDownStart;
        [SerializeField] private TMP_Dropdown dropDownEnd;
        [SerializeField] private TMP_Dropdown dropDownTypes;
        
        // Delegates
        
        public delegate void DropDownMelodyMakerEvent(MelodyMakerData makerData);
        public event DropDownMelodyMakerEvent OnDropDownMelodyMakerEvent;
        
        //

        private bool _uiIsUpdating;
        
        private MelodyMakerData _curMakerData;
        
        // Getters

        // System Methods

        private void Awake()
        {
            _curMakerData = new MelodyMakerData();

            PopulateOctavesDropDown();
            PopulateModeDropDown();
            PopulateStartDropDown();
            PopulateEndDropDown();
            PopulateTypesDropDown();
            
            dropDownOctaves.onValueChanged.AddListener(delegate { OnDropDownOctaveChanged(); });
            dropDownMode.onValueChanged.AddListener(delegate { OnDropDownModeChanged(); });
            dropDownStart.onValueChanged.AddListener(delegate { OnDropDownStartChanged(); });
            dropDownEnd.onValueChanged.AddListener(delegate { OnDropDownEndChanged(); });
            dropDownTypes.onValueChanged.AddListener(delegate { OnDropDownTypeChanged(); });
        }
        
        // Private methods
        
        private void PopulateOctavesDropDown()
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (int i in Enum.GetValues(typeof(MelodyOctaves))) {
                var s = Enum.GetName(typeof(MelodyOctaves), i);
                optionData.Add(new TMP_Dropdown.OptionData(s));
            }
            dropDownOctaves.options = optionData;
        }

        private void PopulateModeDropDown()
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (int i in Enum.GetValues(typeof(MelodyMode))) {
                var s = Enum.GetName(typeof(MelodyMode), i);
                optionData.Add(new TMP_Dropdown.OptionData(s));
            }
            dropDownMode.options = optionData;
        }
        
        private void PopulateStartDropDown()
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (int i in Enum.GetValues(typeof(MelodyStart))) {
                var s = Enum.GetName(typeof(MelodyStart), i);
                optionData.Add(new TMP_Dropdown.OptionData(s));
            }
            dropDownStart.options = optionData;
        }
        
        private void PopulateEndDropDown()
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (int i in Enum.GetValues(typeof(MelodyEnd))) {
                var s = Enum.GetName(typeof(MelodyEnd), i);
                optionData.Add(new TMP_Dropdown.OptionData(s));
            }
            dropDownEnd.options = optionData;
        }

        private void PopulateTypesDropDown()
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (int i in Enum.GetValues(typeof(MelodyType))) {
                var s = Enum.GetName(typeof(MelodyType), i);
                optionData.Add(new TMP_Dropdown.OptionData(s));
            }
            dropDownTypes.options = optionData;
        }
        
        private void OnDropDownOctaveChanged()
        {
            if (_uiIsUpdating) return;
            _curMakerData.Octaves = dropDownOctaves.value;
            OnDropDownMelodyMakerEvent?.Invoke(_curMakerData);
        }
        
        private void OnDropDownModeChanged()
        {
            if (_uiIsUpdating) return;
            _curMakerData.Mode = dropDownMode.value;
            OnDropDownMelodyMakerEvent?.Invoke(_curMakerData);
        }

        private void OnDropDownStartChanged()
        {
            if (_uiIsUpdating) return;
            _curMakerData.Start = dropDownStart.value;
            OnDropDownMelodyMakerEvent?.Invoke(_curMakerData);
        }

        private void OnDropDownEndChanged()
        {
            if (_uiIsUpdating) return;
            _curMakerData.End = dropDownEnd.value;
            OnDropDownMelodyMakerEvent?.Invoke(_curMakerData);
        }

        private void OnDropDownTypeChanged()
        {
            if (_uiIsUpdating) return;
            _curMakerData.Type = dropDownTypes.value;
            OnDropDownMelodyMakerEvent?.Invoke(_curMakerData);
        }

        // Public Methods
        
        public void ShowMelodyMakerData(MelodyMakerData makerData)
        {
            _uiIsUpdating = true;

            _curMakerData = makerData;
            
            dropDownOctaves.value = _curMakerData.Octaves;
            dropDownMode.value = _curMakerData.Mode;
            dropDownStart.value = _curMakerData.Start;
            dropDownEnd.value = _curMakerData.End;
            dropDownTypes.value = _curMakerData.Type;
            
            _uiIsUpdating = false;
        }
    }
}
