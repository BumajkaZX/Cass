namespace Cass.UI.Lobbies
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;

    public class Lobby : MonoBehaviour
    {
        /// <summary>
        /// Join code
        /// </summary>
        public static ReactiveProperty<string> JoinId = new ReactiveProperty<string>();

        [SerializeField]
        private TMP_Text _lobbyName = default;

        [SerializeField]
        private Image _lobbyImage = default;

        [SerializeField]
        private Button _joinButton = default;

        [SerializeField]
        private string _lobbyId = default;

        private void Awake() => _joinButton.OnClickAsObservable().Subscribe(_ => JoinId.SetValueAndForceNotify(_lobbyId)).AddTo(this);
        /// <summary>
        /// Set lobby name
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name) => _lobbyName.text = name;

        /// <summary>
        /// Set lobby image
        /// </summary>
        /// <param name="image"></param>
        public void SetImage(Sprite image) => _lobbyImage.sprite = image;
        /// <summary>
        /// Set lobby code
        /// </summary>
        /// <param name="id"></param>
        public void SetLobbyId(string id) => _lobbyId = id;
    }
}
