namespace Cass.Interactable
{
    using Cass.CanvasController;
    using UniRx;
    using UnityEngine;

    public class MultiplayerStartMenu : MonoBehaviour, IInteractableObject
    {
        [SerializeField]
        private Transform _animationTrans = default;

        [SerializeField]
        private CanvasIdentificator _lobbiesIdentificator = default;

        private Vector3 _defaultScale = default;

        private CompositeDisposable _dis = new CompositeDisposable();

        private void Awake() => _defaultScale = _animationTrans.localScale;

        public void OnObjectInteract()
        {
            if (CanvasController.Instance.CurrentWindow.Id == _lobbiesIdentificator.Id)
            {
                return;
            }

            CanvasController.Instance.SetActiveCanvas(_lobbiesIdentificator, true);

            _animationTrans.localScale = _defaultScale;
            _dis.Clear();
        }

        public void StartInteraction(bool isStart)
        {

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
