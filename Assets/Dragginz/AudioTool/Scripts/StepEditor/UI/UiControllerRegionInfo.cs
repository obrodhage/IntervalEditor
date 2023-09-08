using System.Collections.Generic;
using Dragginz.AudioTool.Scripts.Includes;
using Dragginz.AudioTool.Scripts.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Toggle = UnityEngine.UI.Toggle;

namespace Dragginz.AudioTool.Scripts.StepEditor.UI
{
    public class UiControllerRegionInfo : MonoBehaviour
    {
        // UI Elements
        
        [SerializeField] private GameObject goRegionPanel;
        [SerializeField] private Button buttonClose;
        [SerializeField] private TMP_Text labelInstrument;
        
        [SerializeField] private TMP_Dropdown dropDownLength;
        [SerializeField] private TMP_Dropdown dropDownKeys;
        [SerializeField] private TMP_Dropdown dropDownIntervals;
        [SerializeField] private TMP_Dropdown dropDownOctaves;
        [SerializeField] private TMP_Dropdown dropDownTypes;
        
        [SerializeField] private GameObject groupArpeggiator;
        [SerializeField] private TMP_Dropdown dropDownPatterns;
        [SerializeField] private TMP_Dropdown dropDownNotes;
        
        [SerializeField] private GameObject groupChord;
        [SerializeField] private TMP_Dropdown dropDownChordNotes;
        
        [SerializeField] private Toggle toggleRootNoteOnly;
        [SerializeField] private Toggle toggleHighOctave;
        [SerializeField] private Slider sliderVolume;
        [SerializeField] private Slider sliderPan;
        
        [SerializeField] private Button buttonDelete;
        
        // Delegates
        
        public delegate void DropDownLengthEvent(int key);
        public event DropDownLengthEvent OnDropDownLengthEvent;
        
        public delegate void DropDownKeyEvent(int key);
        public event DropDownKeyEvent OnDropDownKeyEvent;
    
        public delegate void DropDownIntervalEvent(int interval);
        public event DropDownIntervalEvent OnDropDownIntervalEvent;
    
        public delegate void DropDownOctaveEvent(int interval);
        public event DropDownOctaveEvent OnDropDownOctaveEvent;
        
        public delegate void DropDownTypeEvent(int interval);
        public event DropDownTypeEvent OnDropDownTypeEvent;
        
        public delegate void DropDownPatternEvent(int key);
        public event DropDownPatternEvent OnDropDownPatternEvent;
    
        public delegate void DropDownNoteEvent(int interval);
        public event DropDownNoteEvent OnDropDownNoteEvent;
    
        public delegate void DropDownChordNoteEvent(int interval);
        public event DropDownChordNoteEvent OnDropDownChordNoteEvent;
        
        public delegate void ToggleHighOctaveEvent(bool value);
        public event ToggleHighOctaveEvent OnToggleHighOctaveEvent;
    
        public delegate void ToggleRootNoteOnlyEvent(bool value);
        public event ToggleRootNoteOnlyEvent OnToggleRootNoteOnlyEvent;
    
        public delegate void SliderVolumeEvent(float value);
        public event SliderVolumeEvent OnSliderVolumeEvent;
    
        public delegate void SliderPanEvent(float value);
        public event SliderPanEvent OnSliderPanEvent;
        
        public delegate void ButtonDeleteEvent();
        public event ButtonDeleteEvent OnButtonDeleteEvent;
        
        //

        private bool _uiIsUpdating;
        
        // Getters

        public bool IsVisible => isActiveAndEnabled;
        
        // System methods

        private void Awake()
        {
            goRegionPanel.SetActive(false);
        }

        // Public methods
        
        public void Init()
        {
            toggleHighOctave.isOn = true;
            toggleRootNoteOnly.isOn = false;
            
            buttonClose.onClick.AddListener(Hide);
            
            dropDownLength.onValueChanged.AddListener(delegate { OnDropDownLengthChanged(); });
            dropDownKeys.onValueChanged.AddListener(delegate { OnDropDownKeyChanged(); });
            dropDownIntervals.onValueChanged.AddListener(delegate { OnDropDownIntervalChanged(); });
            dropDownOctaves.onValueChanged.AddListener(delegate { OnDropDownOctaveChanged(); });
            dropDownTypes.onValueChanged.AddListener(delegate { OnDropDownTypeChanged(); });
            dropDownPatterns.onValueChanged.AddListener(delegate { OnDropDownPatternChanged(); });
            dropDownNotes.onValueChanged.AddListener(delegate { OnDropDownNoteChanged(); });
            dropDownChordNotes.onValueChanged.AddListener(delegate { OnDropDownChordNoteChanged(); });

            toggleHighOctave.onValueChanged.AddListener(delegate { OnToggleHighOctaveChanged(toggleHighOctave.isOn); });
            toggleRootNoteOnly.onValueChanged.AddListener(delegate { OnToggleRootNoteOnlyChanged(toggleRootNoteOnly.isOn); });
        
            sliderVolume.onValueChanged.AddListener(delegate { OnSliderVolumeChanged(sliderVolume.value); });
            sliderPan.onValueChanged.AddListener(delegate { OnSliderPanChanged(sliderVolume.value); });
            
            buttonDelete.onClick.AddListener(DeleteRegion);
        }

        public void PopulateLengthsDropDown(string[] keys)
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (var key in keys) {
                optionData.Add(new TMP_Dropdown.OptionData(key));
            }
            dropDownLength.options = optionData;
        }

        public void PopulateKeysDropDown(string[] keys)
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (var key in keys) {
                optionData.Add(new TMP_Dropdown.OptionData(key));
            }
            dropDownKeys.options = optionData;
        }
        
        public void PopulateIntervalsDropDown(List<ScriptableObjectChord> sortedListChords)
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (var chord in sortedListChords) {
                optionData.Add(new TMP_Dropdown.OptionData(chord.name));
            }
            dropDownIntervals.options = optionData;
        }
        
        public void PopulateOctavesDropDown(List<int> octaves)
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (var octave in octaves) {
                optionData.Add(new TMP_Dropdown.OptionData(octave.ToString()));
            }
            dropDownOctaves.options = optionData;
        }
        
        public void PopulatePatternsDropDown(List<ScriptableObjectPattern> sortedListPatterns)
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (var chord in sortedListPatterns) {
                optionData.Add(new TMP_Dropdown.OptionData(chord.name));
            }
            dropDownPatterns.options = optionData;
        }
        
        public void PopulateArpeggiatorNotesDropDown(string[] keys)
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (var key in keys) {
                optionData.Add(new TMP_Dropdown.OptionData(key));
            }
            dropDownNotes.options = optionData;
        }
        
        public void PopulateChordNotesDropDown(string[] keys)
        {
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (var key in keys) {
                optionData.Add(new TMP_Dropdown.OptionData(key));
            }
            dropDownChordNotes.options = optionData;
        }
        
        public void ShowRegionInfo(Region region)
        {
            _uiIsUpdating = true;
            
            goRegionPanel.SetActive(true);

            var settings = region.playbackSettings;

            ShowRegionInfoHeader(region);
            
            //" 4 Beats", " 8 Beats", "12 Beats", "16 Beats", "20 Beats", "24 Beats"
            dropDownLength.value = (region.beats / 4) - 1; // not the best solution
            
            dropDownKeys.value = settings.Key;
            dropDownIntervals.value = settings.Interval;

            PopulateOctavesDropDown(settings.Instrument.octaves);
            dropDownOctaves.value = settings.Octave;
            
            var optionData = new List<TMP_Dropdown.OptionData>();
            foreach (var key in Globals.Types) {
                optionData.Add(new TMP_Dropdown.OptionData(key));
            }
            if (!region.playbackSettings.CanLoop) optionData.RemoveAt(optionData.Count-1);
            dropDownTypes.options = optionData;
            dropDownTypes.value = settings.Type;

            if (settings.Type == Globals.RegionTypeArpeggiator) {
                dropDownPatterns.value = settings.Pattern;
                dropDownNotes.value = settings.Note;
            }
            if (settings.Type == Globals.RegionTypeChord) dropDownChordNotes.value = settings.Note;
            
            updateGroupVisibility(region.playbackSettings);
            //groupArpeggiator.SetActive(settings.Type == Globals.RegionTypeArpeggiator);
            //groupChord.SetActive(!groupArpeggiator.activeSelf);
            
            sliderVolume.value = settings.Volume;
            sliderPan.value = settings.Pan; //Mathf.InverseLerp(-1f, 1f, region.playbackSettings.Pan);
            
            toggleRootNoteOnly.isOn = settings.RootNoteOnly;
            toggleHighOctave.isOn = settings.HighOctave;
            //toggleRootNoteOnly.gameObject.SetActive(settings.Type != Globals.RegionTypeArpeggiator);
            //toggleHighOctave.gameObject.SetActive(settings.Type != Globals.RegionTypeArpeggiator);

            _uiIsUpdating = false;
        }

        public void ShowRegionInfoHeader(Region region)
        {
            var header = "Track "+ (region.trackPos+1) + " - " + region.instrumentName + " - Beat " + region.startPosBeats + "-" + (region.startPosBeats + region.beats - 1);
            labelInstrument.text = header;
        }
        
        public void updateGroupVisibility(Globals.InstrumentSettings settings)
        {
            groupArpeggiator.SetActive(settings.Type == Globals.RegionTypeArpeggiator);
            groupChord.SetActive(settings.Type == Globals.RegionTypeChord);
            toggleRootNoteOnly.gameObject.SetActive(settings.Type != Globals.RegionTypeArpeggiator);
            toggleHighOctave.gameObject.SetActive(settings.Type != Globals.RegionTypeArpeggiator);
        }
        
        // Ui events
        
        public void Hide()
        {
            goRegionPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        private void DeleteRegion()
        {
            goRegionPanel.SetActive(false);
            
            OnButtonDeleteEvent?.Invoke();
        }

        private void OnDropDownLengthChanged() {
            if (!_uiIsUpdating) OnDropDownLengthEvent?.Invoke(dropDownLength.value);
        }
        
        private void OnDropDownKeyChanged() {
            if (!_uiIsUpdating) OnDropDownKeyEvent?.Invoke(dropDownKeys.value);
        }
        
        private void OnDropDownIntervalChanged() {
            if (!_uiIsUpdating) OnDropDownIntervalEvent?.Invoke(dropDownIntervals.value);
        }
        
        private void OnDropDownOctaveChanged() {
            if (!_uiIsUpdating) OnDropDownOctaveEvent?.Invoke(dropDownOctaves.value);
        }
        
        private void OnDropDownTypeChanged() {
            if (!_uiIsUpdating) OnDropDownTypeEvent?.Invoke(dropDownTypes.value);
        }
        
        private void OnDropDownPatternChanged() {
            if (!_uiIsUpdating) OnDropDownPatternEvent?.Invoke(dropDownPatterns.value);
        }
        
        private void OnDropDownNoteChanged() {
            if (!_uiIsUpdating) OnDropDownNoteEvent?.Invoke(dropDownNotes.value);
        }

        private void OnDropDownChordNoteChanged() {
            if (!_uiIsUpdating) OnDropDownChordNoteEvent?.Invoke(dropDownChordNotes.value);
        }
        
        private void OnToggleHighOctaveChanged(bool value) {
            if (!_uiIsUpdating) OnToggleHighOctaveEvent?.Invoke(value);
        }
    
        private void OnToggleRootNoteOnlyChanged(bool value) {
            if (!_uiIsUpdating) OnToggleRootNoteOnlyEvent?.Invoke(value);
        }

        private void OnSliderVolumeChanged(float value) {
            if (!_uiIsUpdating) OnSliderVolumeEvent?.Invoke(value);
        }
        
        private void OnSliderPanChanged(float value) {
            if (!_uiIsUpdating) OnSliderPanEvent?.Invoke(value);
        }
    }
}
