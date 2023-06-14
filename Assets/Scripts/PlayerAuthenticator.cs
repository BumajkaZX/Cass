namespace Cass.Services
{
    using UnityEngine;
    using Cass.LoadManager;
    using System.Threading.Tasks;
    using System;
    using System.Threading;

#if UNITY_ANDROID

    using GooglePlayGames;
    using GooglePlayGames.BasicApi;

#endif

    using Log = Cass.Logger;
    using UnityEngine.SocialPlatforms;

    public class PlayerAuthenticator : MonoBehaviour, ILoadingCondition
    {

        public int Order => 2;

        public string Name => typeof(PlayerAuthenticator).Name;

        public bool IsInited => _isInited;

        private bool _isInited = false;

        public Task<Action> Initialization(CancellationToken token)
        {

#if UNITY_ANDROID

            if (ConnectionManager.Instance.IsConnected.Value)
            {

                PlayGamesPlatform.Activate();
                PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);

            }
            else
            {
                Log.Logger.Instance.Log("No internet");
            }
#endif

     
            return Task.FromResult<Action>(null);
        }

#if UNITY_ANDROID
        private void ProcessAuthentication(SignInStatus status)
        {
            if(status == SignInStatus.Success)
            {
                _isInited = true;
                Log.Logger.Instance.Log($"User {Social.localUser.userName}");
            }
            else
            {
                Log.Logger.Instance.Log("Sign in error");
            }
        }

#endif

    }
}
