using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dragginz.AudioTool.Scripts.StepEditor.UI
{
    public class UiRegion : MonoBehaviour
    {
        [SerializeField] private Image imageRegion;
        [SerializeField] private TMP_Text labelKey;
        [SerializeField] private TMP_Text labelChord;
        [SerializeField] private Button buttonRegion;
        [SerializeField] private Image imageMuted;
        
        public delegate void ClickRegionEvent();
        public event ClickRegionEvent OnClickRegionEvent;

        public RectTransform rectTransform;
        
        public void Init(int width, Color color, string txtKey, string txtChord)
        {
            rectTransform = GetComponent<RectTransform>();
            var size = rectTransform.sizeDelta;
            size.x = width;
            rectTransform.sizeDelta = size;

            imageRegion.color = color;
            
            labelKey.text = txtKey;
            labelChord.text = txtChord;
            labelChord.enabled = width >= 140;
            
            imageMuted.enabled = false;
        }

        public void UpdateValues(string txtKey, string txtChord)
        {
            labelKey.text = txtKey;
            labelChord.text = txtChord;
        }
        
        public void UpdateInstrument(Color instrumentDefaultColor)
        {
            imageRegion.color = instrumentDefaultColor;
        }

        public void ShowMuted(bool muted)
        {
            imageMuted.enabled = muted;
        }
        
        private void Start()
        {
            buttonRegion.onClick.AddListener(OnClickRegion);
        }
        
        public void Remove()
        {
            Destroy(this.gameObject);
        }

        private void OnClickRegion()
        {
            //Debug.Log("click track, region: "+TrackPos+", "+RegionPos);
            OnClickRegionEvent?.Invoke();
        }
    }
}
