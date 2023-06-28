namespace Cass.CanvasController
{
    using Cass.LoadManager;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public class CanvasController : MonoBehaviour, ILoadingCondition
    {
        public static CanvasController Instance = default;

        public CanvasIdentificator CurrentWindow => _currentWindow;

        public int Order => 10;

        public string Name => typeof(CanvasController).Name;

        public bool IsInited => _isInited;

        [SerializeField]
        private List<IdentificatorHolder> _identificators = default;

        [SerializeField]
        private CanvasIdentificator _defaultCanvas = default;

        private CanvasIdentificator _currentWindow = default;

        private bool _isInited = false;

        public Task<Action> Initialization(CancellationToken token)
        {
            _identificators = new List<IdentificatorHolder>(FindObjectsOfType<IdentificatorHolder>(true));

            SetActiveCanvas(_defaultCanvas, _defaultCanvas.IsSolo);

            DontDestroyOnLoad(this);
            Instance = this;

            _isInited = true;

            return Task.FromResult<Action>(null);
        }

        public void SetActiveCanvas(CanvasIdentificator identificator, bool isSolo)
        {
            if (isSolo)
            {
                foreach (IdentificatorHolder holder in _identificators)
                {
                    holder.gameObject.SetActive(false);
                }
            }

            var wHolder = _identificators.Find(_ => _.Identificator.Id == identificator.Id);

            wHolder.gameObject.SetActive(true);

            _currentWindow = wHolder.Identificator;
        }

        public void SetActiveCanvas(string id, bool isSolo)
        {
            if (isSolo)
            {
                foreach (IdentificatorHolder holder in _identificators)
                {
                    holder.gameObject.SetActive(false);
                }
            }

            var wHolder = _identificators.Find(_ => _.Identificator.Id == id);

            wHolder.gameObject.SetActive(true);

            _currentWindow = wHolder.Identificator;
        }
    }
}
