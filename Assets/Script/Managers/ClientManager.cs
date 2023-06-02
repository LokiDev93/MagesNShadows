using FishNet.Managing;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagesnShadows
{
    public class ClientManager : MonoBehaviour
    {
        [SerializeField] private GameObject _clientManager;

        private NetworkManager _networkManager;
        [SerializeField] private LocalConnectionState _clientState = LocalConnectionState.Stopped;



        // Start is called before the first frame update
        void Start()
        {
            _clientManager = GameObject.Find("ClientManager");
            if (_clientManager == null)
            {
                Debug.LogError("ClientManager not found, HUD will not function.");
                return;
            }
            if (Application.isBatchMode)
            {
                
                Destroy(_clientManager);
                Application.Quit();
            }
            _networkManager = FindObjectOfType<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager not found, HUD will not function.");
                return;
            }


        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                OnJoin_Client();
            }
        }


        public void OnJoin_Client()
        {
            if (_networkManager == null)
                return;

            if (_clientState != LocalConnectionState.Stopped)
                _networkManager.ClientManager.StopConnection();
            else
                _networkManager.ClientManager.StartConnection();

            
        }
    }
}
