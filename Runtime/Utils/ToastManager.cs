using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SturfeeVPS.SDK
{
    public class ToastManager : MonoBehaviour {

        [SerializeField]
        private Canvas _displayCanvas;        
        [SerializeField]
        private Text _message;

        [SerializeField]
        private GameObject _error;
        [SerializeField]
        private Text _errorMessage;

        public static ToastManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Instantiate(Resources.Load<GameObject>("Prefabs/ToastManager")).GetComponent<ToastManager>();
                }

                return _instance;
            }
        }

        private static ToastManager _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(_instance);
            }

            _instance = this;
//    		DontDestroyOnLoad(gameObject);
        }              

		public void ShowToast(string toastMessage, float durationInSeconds = 5.0f)
		{
            _displayCanvas.gameObject.SetActive(true);

            _error.SetActive(false);
            _message.gameObject.SetActive(true);

            _message.text = toastMessage;

            StartCoroutine(HideToast(durationInSeconds));            
        }

        public void ShowErrorToast(string error, float durationInSeconds = 5.0f)
        {
            _displayCanvas.gameObject.SetActive(true);

            _error.SetActive(true);
            _message.gameObject.SetActive(false);
            
            _errorMessage.text = error;

            StartCoroutine(HideToast(durationInSeconds));
        }

        public void HideToast()
        {
            _displayCanvas.gameObject.SetActive(false);
        }

        private IEnumerator HideToast(float duration)
        {
            yield return new WaitForSeconds(duration);

            _displayCanvas.gameObject.SetActive(false);
        }             
    }
}