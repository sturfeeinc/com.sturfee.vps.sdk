using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SturfeeVPS.UI
{

    public class LocalizedText : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_textMP;

        [SerializeField]
        private Text m_text;

        private void Start()
        {
            if (m_text == null)
            {
                m_text = GetComponentInChildren<Text>();
            }

            if (m_text != null)
            {
                m_text.text = SturfeeLocalizationProvider.Instance.GetString(m_text.text, null);
            }

            if (m_textMP == null)
            {
                m_textMP = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (m_textMP != null)
            {
                m_textMP.text = SturfeeLocalizationProvider.Instance.GetString(m_textMP.text, null);
            }
        }
    }
}