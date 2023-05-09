namespace Cass.UI.Lobbies
{
    using System.Collections.Generic;
    using Unity.Services.Lobbies.Models;
    using UnityEngine;
    using UniRx;
    using System;

    [RequireComponent(typeof(RectTransform))]
    public class LobbiesController : MonoBehaviour
    {
        [SerializeField]
        private LobbyManager _lobbyManager = default;

        [SerializeField]
        private Lobby _lobbyPrefab = default;

        [SerializeField, Range(1, 10)]
        private float _lobbiesUpdateTime = 5;

        [SerializeField, Min(10)]
        private int _lobbiesCount = 10;

        private RectTransform _rectTransform = default;

        private List<Lobby> _lobbiesList = new List<Lobby>();

        private Action<bool> onConnectError = delegate { };

        private void Awake()=> 
            _lobbyManager.IsInited.Subscribe(Inited =>
            {
                if (Inited)
                {
                    Init();
                }
            }).AddTo(this);

        private void Init()
        {
            _rectTransform = GetComponent<RectTransform>();

            CreateLobbies();

            _lobbyManager.LobbiesList.Subscribe(lobbies => UpdateLobbies(lobbies)).AddTo(this);

            ObserveConnectToLobbies();

            UpdateLobbiesList();

            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, -(_rectTransform.sizeDelta.y / 2));
        }
        private void UpdateLobbiesList() => Observable.Timer(TimeSpan.FromSeconds(_lobbiesUpdateTime)).Repeat().Subscribe(_ => _lobbyManager.ForceUpdateLobbiesList(_lobbiesCount)).AddTo(this);

        private void ObserveConnectToLobbies() => Lobby.JoinId.Subscribe(lobbyCode =>
        {
            if (lobbyCode != null)
            {
                _lobbyManager.TryConnectToLobby(onConnectError, lobbyCode);
            }
        }).AddTo(this);

        private void CreateLobbies()
        {

            for (int i = 0; i < _lobbiesCount; i++)
            {
                Lobby lobby = Instantiate(_lobbyPrefab, _rectTransform);
                lobby.gameObject.SetActive(false);
                _lobbiesList.Add(lobby);
            }

            _lobbiesList.Reverse();
        }


        private void UpdateLobbies(QueryResponse lobbies)
        {
            if(lobbies == null)
            {

#if UNITY_EDITOR 

                Debug.LogWarning("No lobbies found");

#endif

                return;
            }

            if(lobbies.Results.Count < _lobbiesList.Count)
            {
                for (int i = lobbies.Results.Count; i < _lobbiesList.Count; i++)
                {
                    _lobbiesList[i].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < lobbies.Results.Count; i++)
            {
                if (_lobbiesList[i].gameObject.activeSelf) 
                {
                    _lobbiesList[i].SetName(lobbies.Results[i].Name);
                    _lobbiesList[i].SetLobbyId(lobbies.Results[i].Id);
                }
                else
                {
                    Lobby lobby = _lobbiesList[i];
                    lobby.SetName(lobbies.Results[i].Name);
                    lobby.SetLobbyId(lobbies.Results[i].Id);
                    lobby.gameObject.SetActive(true);
                }
            }
        }
    }
}
