using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

namespace MagesnShadows.PlayerSystems
{
    public class CameraController : NetworkBehaviour
    {
        public float mouseSensitivity = 100f;
        public Transform playerBody;
        [SerializeField] float xRotation = 0f;

        // Start is called before the first frame update
        void Start()
        {

            playerBody = GetComponentInParent<Transform>().root.gameObject.transform;

            if (Application.isEditor == true)
            {
                //Cursor.lockState = CursorLockMode.None;
                //Cursor.visible = false;
            }
            this.gameObject.SetActive(true);

        }
        private void Awake()
        {
            
        }

        //public override void OnStartClient()
        //{
        //    base.OnStartClient();
        //    if (!base.IsOwner)
        //    {
        //        gameObject.SetActive(false);
        //    }
        //}

        // Update is called once per frame
        void Update()
        {
            //if (!base.IsOwner) return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -80f, 50f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Cursor.lockState = CursorLockMode.Confined;
            }

        }
    }
}
