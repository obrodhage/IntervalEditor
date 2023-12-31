using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Dragginz.AudioTool.Scripts.StepEditor.UI
{
    public class UiControllerArpeggiator : MonoBehaviour
    {
        // UI Elements
        
        [SerializeField] private TMP_Dropdown dropDownOctaves;
        [SerializeField] private TMP_Dropdown dropDownStart;
        [SerializeField] private TMP_Dropdown dropDownEnd;
        [SerializeField] private TMP_Dropdown dropDownTypes;
        
        // Delegates
        
        public delegate void DropDownArpeggiatorEvent(ArpeggiatorData data);
        public event DropDownArpeggiatorEvent OnDropDownArpeggiatorEvent;
        
        //

        private bool _uiIsUpdating;
        
        private ArpeggiatorData _curData;
        
        // Getters

        // System Methods

        private void Awake()
        {
            _curData = new ArpeggiatorData();

            PopulateOctavesDropDown();
            PopulateStartDropDown();
            PopulateEndDropDown();
            PopulateTypesDropDown();
            
            dropDownOctaves.onValueChanged.AddListener(delegate { OnDropDownOctaveChanged(); });
            dropDownStart.onValueChanged.AddListener(delegate { OnDropDownStartChanged(); });
            dropDownEnd.onValueChanged.AddListener(delegate { OnDropDownEndChanged(); });
            dropDownTypes.onValueChanged.AddListener(delegate { OnDropDownTypeChanged(); });
        }
        
        // Private methods
        
        private void PopulateOctavesDropDown()
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (int i in Enum.GetValues(typeof(ArpOctaves))) {
                var s = Enum.GetName(typeof(ArpOctaves), i);
                optionData.Add(new TMP_Dropdown.OptionData(s));
            }
            dropDownOctaves.options = optionData;
        }

        private void PopulateStartDropDown()
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (int i in Enum.GetValues(typeof(ArpStart))) {
                var s = Enum.GetName(typeof(ArpStart), i);
                optionData.Add(new TMP_Dropdown.OptionData(s));
            }
            dropDownStart.options = optionData;
        }
        
        private void PopulateEndDropDown()
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (int i in Enum.GetValues(typeof(ArpEnd))) {
                var s = Enum.GetName(typeof(ArpEnd), i);
                optionData.Add(new TMP_Dropdown.OptionData(s));
            }
            dropDownEnd.options = optionData;
        }

        private void PopulateTypesDropDown()
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (int i in Enum.GetValues(typeof(ArpType))) {
                var s = Enum.GetName(typeof(ArpType), i);
                optionData.Add(new TMP_Dropdown.OptionData(s));
            }
            dropDownTypes.options = optionData;
        }
        
        private void OnDropDownOctaveChanged()
        {
            if (_uiIsUpdating) return;
            _curData.Octaves = dropDownOctaves.value;
            OnDropDownArpeggiatorEvent?.Invoke(_curData);
        }
        
        private void OnDropDownStartChanged()
        {
            if (_uiIsUpdating) return;
            _curData.Start = dropDownStart.value;
            OnDropDownArpeggiatorEvent?.Invoke(_curData);
        }

        private void OnDropDownEndChanged()
        {
            if (_uiIsUpdating) return;
            _curData.End = dropDownEnd.value;
            OnDropDownArpeggiatorEvent?.Invoke(_curData);
        }

        private void OnDropDownTypeChanged()
        {
            if (_uiIsUpdating) return;
            _curData.Type = dropDownTypes.value;
            OnDropDownArpeggiatorEvent?.Invoke(_curData);
        }

        // Public Methods
        
        public void ShowArpeggiatorData(ArpeggiatorData data)
        {
            _uiIsUpdating = true;

            _curData = data;
            
            dropDownOctaves.value = _curData.Octaves;
            dropDownStart.value = _curData.Start;
            dropDownEnd.value = _curData.End;
            dropDownTypes.value = _curData.Type;
            
            _uiIsUpdating = false;
        }
    }
}
