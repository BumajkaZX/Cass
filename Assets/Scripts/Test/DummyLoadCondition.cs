namespace Cass.LoadManager
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public class DummyLoadCondition : MonoBehaviour, ILoadingCondition
    {
        public int Order => _order;
        public bool IsInited => _isInited;

        private bool _isInited = false;

        private int _order = 0;
        public string Name => typeof(DummyLoadCondition).Name;

        public async Task<Action> Initialization(CancellationToken token)
        {
            _isInited = true;
            Action action = OnSceneStart;
            await Task.CompletedTask;
            return action;
        }

        private void OnSceneStart()
        {
            Debug.Log("STARTTTTT");
        }
    }
}
