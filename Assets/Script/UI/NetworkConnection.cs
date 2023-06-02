using FishNet.Managing;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MagesnShadows.UI
{
    public class NetworkConnection : MonoBehaviour
    {
        #region Types.
        /// <summary>
        /// Ways the HUD will automatically start a connection.
        /// </summary>
        private enum AutoStartType
        {
            Disabled,
            Host,
            Server,
            Client
        }
        #endregion

        #region Serialized.
        /// <summary>
        /// What connections to automatically start on play.
        /// </summary>
        [Tooltip("What connections to automatically start on play.")]
        [SerializeField]
        private AutoStartType _autoStartType = AutoStartType.Disabled;
        /// <summary>
        /// Color when socket is stopped.
        /// </summary>
        [Tooltip("Color when socket is stopped.")]
        [SerializeField]
        private Color _stoppedColor;
        /// <summary>
        /// Color when socket is changing.
        /// </summary>
        [Tooltip("Color when socket is changing.")]
        [SerializeField]
        private Color _changingColor;
        /// <summary>
        /// Color when socket is started.
        /// </summary>
        [Tooltip("Color when socket is started.")]
        [SerializeField]
        private Color _startedColor;
        [Header("Indicators")]
        /// <summary>
        /// Indicator for server state.
        /// </summary>
        [Tooltip("Indicator for server state.")]
        [SerializeField]
        private Image _serverIndicator;
        /// <summary>
        /// Indicator for client state.
        /// </summary>
        [Tooltip("Indicator for client state.")]
        [SerializeField]
        private Image _clientIndicator;
        #endregion

        #region Private.
        /// <summary>
        /// Found NetworkManager.
        /// </summary>
        private NetworkManager _networkManager;
        /// <summary>
        /// Current state of client socket.
        /// </summary>
        private LocalConnectionState _clientState = LocalConnectionState.Stopped;
        /// <summary>
        /// Current state of server socket.
        /// </summary>
        private LocalConnectionState _serverState = LocalConnectionState.Stopped;
        #endregion



        // Start is called before the first frame update
        void Start()
        {
#if !ENABLE_INPUT_SYSTEM
            EventSystem systems = FindObjectOfType<EventSystem>();
            if (systems == null)
                gameObject.AddComponent<EventSystem>();
            BaseInputModule inputModule = FindObjectOfType<BaseInputModule>();
            if (inputModule == null)
                gameObject.AddComponent<StandaloneInputModule>();
#else
        _serverIndicator.transform.parent.gameObject.SetActive(false);
        _clientIndicator.transform.parent.gameObject.SetActive(false);
#endif

            _networkManager = FindObjectOfType<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager not found, HUD will not function.");
                return;
            }
            else
            {
                UpdateColor(LocalConnectionState.Stopped, ref _serverIndicator);
                UpdateColor(LocalConnectionState.Stopped, ref _clientIndicator);
                _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
                _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
            }

            if (_autoStartType == AutoStartType.Host || _autoStartType == AutoStartType.Server)
                OnClick_Server();
            if (!Application.isBatchMode && (_autoStartType == AutoStartType.Host || _autoStartType == AutoStartType.Client))
                OnClick_Client();
        }

        private void OnDestroy()
        {
            if (_networkManager == null)
                return;

            _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        }

        /// <summary>
        /// Updates img color baased on state.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="img"></param>
        private void UpdateColor(LocalConnectionState state, ref Image img)
        {
            Color c;
            if (state == LocalConnectionState.Started)
                c = _startedColor;
            else if (state == LocalConnectionState.Stopped)
                c = _stoppedColor;
            else
                c = _changingColor;

            img.color = c;
        }


        private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
        {
            _clientState = obj.ConnectionState;
            UpdateColor(obj.ConnectionState, ref _clientIndicator);
        }


        private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
        {
            _serverState = obj.ConnectionState;
            UpdateColor(obj.ConnectionState, ref _serverIndicator);
        }


        public void OnClick_Server()
        {
            if (_networkManager == null)
                return;

            if (_serverState != LocalConnectionState.Stopped)
                _networkManager.ServerManager.StopConnection(true);
            else
                _networkManager.ServerManager.StartConnection();
        }


        public void OnClick_Client()
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
