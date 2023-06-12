namespace Cass.Services
{

    using System;
    using UnityEngine;
    using UniRx;
    using System.Threading.Tasks;
    using System.Threading;
    using Cass.LoadManager;

    public class ConnectionManager : MonoBehaviour, ILoadingCondition
    {
        public static ConnectionManager Instance = default;

        public int Order => _order;

        public string Name => typeof(ConnectionManager).Name;

        public bool IsInited => _isInited;

        [HideInInspector]
        public ReactiveProperty<bool> IsConnected = new ReactiveProperty<bool>(false);

        [SerializeField, Min(1)]
        private int _pingTimeInSeconds = 5;

        [SerializeField, Min(0)]
        private int _order = 1;

        private bool _isInited = false;

        public async Task<Action> Initialization(CancellationToken token)
        {
            Action act = null;

            Instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR

            IsConnected.Value = true;

#else
      
            ConnectionObserver();

#endif

            while (!IsConnected.Value)
            {
                if (token.IsCancellationRequested)
                {
                    return act;
                }
                await Task.Yield();
            }

            _isInited = true;


            Debug.LogError("Connected");

            return act;
        }

        private void ConnectionObserver() =>
            Observable.Timer(TimeSpan.FromSeconds(_pingTimeInSeconds)).Repeat().Subscribe(_ =>
                {

                    if (Application.internetReachability != NetworkReachability.ReachableViaCarrierDataNetwork && IsConnected.Value)
                    {
                        IsConnected.Value = false;
                        Cass.Logger.Logger.Instance.Log("No connection");
                    }
                    else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork && !IsConnected.Value)
                    {
                        IsConnected.Value = true;
                    }
                }).AddTo(this);
    }
}
