using UnityEngine;



namespace SturfeeVPS.SDK.Examples
{
    // Attach to the image you want to lock under your moving mask
    public class LockUIItem : MonoBehaviour
    {
        private Vector3 position;

        private void Awake()
        {
            position = this.gameObject.GetComponent<RectTransform>().position;
        }

        private void Update()
        {
            if (position != this.gameObject.GetComponent<RectTransform>().position)
            {
                this.gameObject.GetComponent<RectTransform>().position = position;
            }
        }
    } 
}