namespace Cass.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;
    using UnityEngine.UI;

    public class LobbyUiController : MonoBehaviour
    {
        [SerializeField]
        private LobbyManager _lobbyManager = default;

        [SerializeField]
        private BoolButton _privatePublic = default;

        [SerializeField]
        private Button _createButton = default;

        [SerializeField]
        private Button _connectButton = default;

        [SerializeField]
        private TMPro.TMP_InputField _inputField = default;

        [SerializeField]
        private TMPro.TMP_InputField _lobbyName = default;

        [SerializeField]
        private TMPro.TMP_Text _privatePublicText = default;

        private void Awake()
        {
            _privatePublic.CurrentState.Subscribe(_ => _privatePublicText.text = _ ? "Public" : "Private").AddTo(this);

            _createButton.OnClickAsObservable().Subscribe(_ => _lobbyManager.TryCreateLobby(_lobbyName.text, _privatePublic.CurrentState.Value)).AddTo(this);

            _connectButton.OnClickAsObservable().Subscribe(_ => _lobbyManager.TryConnectToLobby(ConnectionCallback, _inputField.text == string.Empty ? null : _inputField.text)).AddTo(this);
        }

        private void ConnectionCallback(bool isConnect)
        {
            if (!isConnect)
            {
                Debug.LogError("Can't connect");
            }
        }
    }
}