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

        public int Order => 10;

        public string Name => typeof(CanvasController).Name;

        public bool IsInited => _isInited;

        [SerializeField]
        private List<IdentificatorHolder> _identificators = default;

        [SerializeField]
        private CanvasIdentificator _defaultCanvas = default;

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
            if (!isSolo)
            {
                foreach (IdentificatorHolder holder in _identificators)
                {
                    holder.gameObject.SetActive(false);
                }
            }

            _identificators.Find(_ => _.Identificator.Id == identificator.Id).gameObject.SetActive(true);

        }

        public void SetActiveCanvas(string id, bool isSolo)
        {
            if (!isSolo)
            {
                foreach (IdentificatorHolder holder in _identificators)
                {
                    holder.gameObject.SetActive(false);
                }
            }
            _identificators.Find(_ => _.Identificator.Id == id).gameObject.SetActive(true);
        }
    }
}
