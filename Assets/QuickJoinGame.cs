namespace Cass.UI.Lobbies
{
    using System;
    using UniRx;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class QuickJoinGame : MonoBehaviour
    {
        [SerializeField]
        private LobbyManager _lobbyManager = default;

        private Button _button = default;

        private Action<bool> onConnectError = delegate { };

        private void Awake()
        {
            _button = GetComponent<Button>();

            onConnectError += OnConnectionError;

            _button.OnClickAsObservable().Subscribe(_ => 
            {
                _lobbyManager.TryConnectToLobby(onConnectError);
                _button.interactable = false;
            }).AddTo(this);
        }

        private void OnConnectionError(bool isError)
        {
            if (isError)
            {
                _button.interactable = true;
            }
            Debug.LogError("ERROR");
        }
    }
}
