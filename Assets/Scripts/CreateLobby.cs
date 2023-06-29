namespace Cass.UI.Lobbies 
{
    using UnityEngine;
    using UniRx;
    using UnityEngine.UI;
    using TMPro;

    public class CreateLobby : MonoBehaviour
    {
        [SerializeField]
        private LobbyManager _lobbyManager = default;

        [SerializeField]
        private Button _createLobbyButton = default;

        [SerializeField]
        private TMP_InputField _lobbyName = default;

        [SerializeField]
        private bool _isPrivate = false;

        [SerializeField, Range(5, 20)]
        private int _maxSymbols = default;

        private void Awake()
        {
            _createLobbyButton.OnClickAsObservable().Subscribe(_ =>
            {
                string lobbyName = _lobbyName.text;
                if(_lobbyName.text.Length > _maxSymbols)
                {
                    lobbyName = lobbyName.Substring(0, _maxSymbols);
                }
                _lobbyManager.TryCreateLobby(lobbyName ,_isPrivate);

            }).AddTo(this);
        }
    }
}
