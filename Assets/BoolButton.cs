namespace Cass.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;

    [RequireComponent(typeof(Button))]
    public class BoolButton : MonoBehaviour
    {
        public ReactiveProperty<bool> CurrentState = new ReactiveProperty<bool>();

        [SerializeField]
        private Button _button = default;

        [SerializeField]
        private Transform _checkmarkTransform = default;

        [SerializeField]
        private bool _defaultState = false;

        private void Awake()
        {
            CurrentState.Value = _defaultState;
            _checkmarkTransform.gameObject.SetActive(_defaultState);
            _button.OnClickAsObservable().Subscribe(_ =>
            {
                CurrentState.Value = !CurrentState.Value;
                _checkmarkTransform.gameObject.SetActive(CurrentState.Value);
            }).AddTo(this);
        }
    }
}
