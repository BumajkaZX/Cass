namespace Cass.LoadManager
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;
    using UnityEngine.UI;

    public class LoadView : MonoBehaviour
    {
        [SerializeField]
        private Transform _loadImageTransform = default;

        [SerializeField]
        private float _rotationSpeed = 2f;

        [SerializeField]
        private Button _button = default;

        private CompositeDisposable _loadDis = new CompositeDisposable();

        private MainCharacterInput _input = default;

        private void Awake()
        {
            var manager = LoadManager.Instance;

            _input = new MainCharacterInput();

            _input.UI.Enable();

            manager.LoadComplete.Where(_ => _).Subscribe(_ => 
            {
                _button.OnClickAsObservable().Subscribe(_ => 
                {
                    manager.IsAvailableLoad.Value = true;
                    _input.UI.Disable();
                    _loadDis.Clear();
                }).AddTo(_loadDis);
            }).AddTo(_loadDis);

            RotateImage();

        }

        private void RotateImage() => Observable.EveryUpdate().Subscribe(_ => _loadImageTransform.Rotate(Vector3.forward, _rotationSpeed * Time.deltaTime)).AddTo(_loadDis);

        private void OnDestroy()
        {
            _loadDis.Clear();
        }
    }
}
