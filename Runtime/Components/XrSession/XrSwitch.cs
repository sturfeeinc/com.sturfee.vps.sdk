using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SturfeeVPS.SDK
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class XrSwitch : MonoBehaviour
    {
        private TMP_Dropdown _dropdown;

        private void Start()
        {
            _dropdown = GetComponent<TMP_Dropdown>();

            var sturfeeXrSession = FindObjectOfType<SturfeeXrSession>();
            if (sturfeeXrSession != null)
            {
                int numOfSets = sturfeeXrSession.ProviderSets.Length;

                if(numOfSets <= 1)
                {
                    _dropdown.gameObject.SetActive(false);
                }

                List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();                
                for(int i =0; i < numOfSets; i++)
                {
                    TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData
                    {

                        text = string.IsNullOrEmpty(sturfeeXrSession.ProviderSets[i].DisplayName) ? sturfeeXrSession.ProviderSets[i].Name : sturfeeXrSession.ProviderSets[i].DisplayName
                    };
                    dropdownOptions.Add(optionData);
                }
                _dropdown.AddOptions(dropdownOptions);
                _dropdown.value = sturfeeXrSession.StartSet;
                _dropdown.onValueChanged.AddListener(OnDropDownChange);
            }
        }

        private void OnDestroy()
        {
            if (_dropdown != null)
            {
                _dropdown.onValueChanged.RemoveListener(OnDropDownChange);
            }
        }

        public void SetValue(int val)
        {
            _dropdown.value = val;
            _dropdown.onValueChanged?.Invoke(val);
        }

        private void OnDropDownChange(int value)
        {
            var sturfeeXrSession = FindObjectOfType<SturfeeXrSession>();
            if (sturfeeXrSession != null)
            {
                sturfeeXrSession.SwitchProviderSet(value);
            }
        }
    }
}
