namespace Cass.Items
{ 
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Cass.LoadManager;
    using UnityEngine;

    public class ItemsContainer : MonoBehaviour, ILoadingCondition
    {
        public static ItemsContainer Instance { get; private set; }

        public List<PlayerItem> Items => _items;

        public int Order => 3;

        public string Name => typeof(ItemsContainer).Name;

        public bool IsInited => _isInited;

        [SerializeField]
        private List<PlayerItem> _items = new List<PlayerItem>();

        private bool _isInited = false;

        public Task<Action> Initialization(CancellationToken token)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            _isInited = true;

            return Task.FromResult<Action>(null);
        }

    }
}
