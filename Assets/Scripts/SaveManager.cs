namespace Cass.Services
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Cass.LoadManager;
    using System.Threading.Tasks;
    using System;
    using System.Threading;
    using GooglePlayGames.BasicApi.SavedGame;
    using GooglePlayGames;

    public class SaveManager : MonoBehaviour, ILoadingCondition
    {
        public static SaveManager Instance = default;

        public int Order => 3;

        public string Name => typeof(SaveManager).Name;

        public bool IsInited => _isInited;

        private bool _isInited = false;

        public Task<Action> Initialization(CancellationToken token)
        { 
            if (Instance != null)
            {
                Destroy(this);

                return Task.FromResult<Action>(null);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

            return Task.FromResult<Action>(null);
        }

        public void Load()
        {
            if(Social.localUser.authenticated && PlayGamesPlatform.Instance.IsAuthenticated())
            {

            }
        }
    }
}
