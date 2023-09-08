using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dragginz.AudioTool.Scripts.StepEditor.UI
{
    public class UiBarHeader : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelBar;
        
        public RectTransform rectTransform;
        
        public void Init(string txt)
        {
            labelBar.text = txt;
            
            rectTransform = GetComponent<RectTransform>();
        }
    }
}
