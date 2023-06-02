using FishNet.Managing;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagesnShadows
{
    public class ServerManager : MonoBehaviour
    {
        [SerializeField] private GameObject _serverManager;

        private NetworkManager _networkManager;
        [SerializeField] private LocalConnectionState _serverState = LocalConnectionState.Stopped;

        // Start is called before the first frame update
        void Start()
        {
            _serverManager = GameObject.Find("ServerManager");

            if (_serverManager == null)
            {
                Debug.LogError("ServerManager not found, HUD will not function.");
                return;
            }
            if (Application.isBatchMode)
            {
                Destroy(_serverManager);
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
            if (Input.GetKeyDown(KeyCode.L))
            {
                OnJoin_Server();
            }
        }

        public void OnJoin_Server()
        {
            if (_networkManager == null)
                return;

            if (_serverState != LocalConnectionState.Stopped)
                _networkManager.ServerManager.StopConnection(true);
            else
                _networkManager.ServerManager.StartConnection();

            
        }


    }
}
