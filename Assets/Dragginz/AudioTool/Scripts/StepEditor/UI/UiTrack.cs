using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dragginz.AudioTool.Scripts.StepEditor.UI
{
    public class UiTrack : MonoBehaviour
    {
        [SerializeField] private Image imageTrackId;
        [SerializeField] private TMP_Text labelTrackId;
        [SerializeField] private TMP_Text labelInstrument;
        [SerializeField] private Button buttonTrackInfo;
        [SerializeField] private Button buttonMute;
        [SerializeField] private Button buttonDelete;
        [SerializeField] private Image imageMuted;
        [SerializeField] private TMP_Text labelReverb
            ;
        public delegate void ClickTrackInfoEvent();
        public event ClickTrackInfoEvent OnClickTrackInfoEvent;

        public delegate void ClickMuteEvent();
        public event ClickMuteEvent OnClickMuteEvent;

        public delegate void ClickDeleteEvent();
        public event ClickDeleteEvent OnClickDeleteEvent;
        
        private void Start()
        {
            buttonTrackInfo.onClick.AddListener(OnClickTrackInfo);
            buttonMute.onClick.AddListener(OnClickMute);
            buttonDelete.onClick.AddListener(OnClickDelete);
        }
        
        public void Init(string txtId, string txtName, Color color)
        {
            imageTrackId.color = color;
            labelTrackId.text = txtId;
            labelInstrument.text = txtName;

            labelReverb.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
            
            imageMuted.enabled = false;
        }

        public void UpdateTrackId(int pos)
        {
            labelTrackId.text = (pos + 1).ToString();
            gameObject.name = (pos + 1) + "-" + labelInstrument.text;
            transform.SetSiblingIndex(pos);
        }
        
        public void UpdateInstrument(string instrumentName, Color color)
        {
            imageTrackId.color = color;
            labelInstrument.text = instrumentName;
        }
        
        public void ShowMuted(bool muted, string instrumentName)
        {
            imageMuted.enabled = muted;
            labelInstrument.text = instrumentName;
        }
        
        public void SetReverbFilter(bool active)
        {
            labelReverb.color = active ? new Color(.1f, 1f, .1f, 1) : new Color(0.5f, 0.5f, 0.5f, 0.25f);
        }

        public void Remove()
        {
            Destroy(gameObject);
        }
        
        private void OnClickTrackInfo()
        {
            OnClickTrackInfoEvent?.Invoke();
        }

        private void OnClickMute()
        {
            OnClickMuteEvent?.Invoke();
        }
    
        private void OnClickDelete()
        {
            OnClickDeleteEvent?.Invoke();
        }
    }
}
