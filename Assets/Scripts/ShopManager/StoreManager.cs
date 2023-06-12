namespace Cass.StoreManager 
{
    using Cass.Items;
    using Cass.LoadManager;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public class StoreManager : MonoBehaviour, ILoadingCondition
    {
        public static StoreManager Instance { get; private set; }

        public int Order => 5;

        public string Name => typeof(StoreManager).Name;

        public bool IsInited => _isInited;

        private IStore _store = default;

        private bool _isInited = false;

        public async Task<Action> Initialization(CancellationToken token)
        {
            Action act = null;

            Instance = this;

            DontDestroyOnLoad(this);

#if UNITY_ANDROID || UNITY_IOS

            _store = new MobileStore();

#else

#endif

           await _store.Init(token);

            _isInited = true;

            return act;
        }

        public void Purchase(PlayerItem item) => _store.Purchase(item);
    }
}
