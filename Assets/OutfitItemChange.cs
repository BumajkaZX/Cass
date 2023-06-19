namespace Cass.Interactable
{
    using UnityEngine;
    using UniRx;
    using Cass.Items;
    using Cass.Character;
    using Cass.LoadManager;
    using System.Threading.Tasks;
    using System;
    using System.Threading;

    public class OutfitItemChange : MonoBehaviour, IInteractableObject, ILoadingCondition
    {
        public int Order => 100;

        public string Name => typeof(OutfitItemChange).Name + gameObject.GetInstanceID();

        public bool IsInited => _isInited;

        [SerializeField]
        private Transform _animationTrans = default;

        [SerializeField]
        private Outfit _outfitItem = default;

        private Vector3 _defaultScale = default;

        private MainCharacterController _charController = default;

        private CompositeDisposable _dis = new CompositeDisposable();

        private bool _isInited = false;

        public Task<Action> Initialization(CancellationToken token)
        {
            _defaultScale = _animationTrans.localScale;
            _charController = PlayersPool.Players.Find(_ => _.IsOwner);
            _isInited = true;

            return Task.FromResult<Action>(null);
        }

        public void OnObjectInteract()
        {
            //if (!_outfitItem.IsFree && !_outfitItem.IsBought)
            //{
            //    return;
            //}

            _charController.PlayerInfo.SetOutfit(_outfitItem);
            _charController.RebuildOutfit(_outfitItem, true, true);
            _animationTrans.localScale = _defaultScale;
            _dis.Clear();
        }

        public void StartInteraction(bool isStart)
        {
            if (_charController.PlayerInfo.IsPutOn(_outfitItem.ItemId))
            {
                _animationTrans.localScale = _defaultScale;
                _dis.Clear();
                return;
            }

            _charController.RebuildOutfit(_outfitItem, isStart, false);

            if (!isStart)
            {
                _animationTrans.localScale = _defaultScale;
                _dis.Clear();
                return;
            } 

            StartAnimation();
        }

        private void StartAnimation()
        {
            Vector3 defaultScale = _animationTrans.localScale;
            Vector3 endScale = _animationTrans.localScale / 2;
            float iterator = 0;
            Observable.EveryUpdate().Subscribe(_ =>
            {
                _animationTrans.localScale = Vector3.Lerp(defaultScale, endScale, iterator);

                if (iterator >= 1)
                {
                    (defaultScale, endScale) = (endScale, defaultScale);
                    iterator = 0;
                }

                iterator += Time.deltaTime;

            }).AddTo(_dis);
        }
    }
}
