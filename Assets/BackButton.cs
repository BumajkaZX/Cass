namespace Cass.CanvasController
{
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;

    [RequireComponent(typeof(Button))]
    public class BackButton : MonoBehaviour
    {
        [SerializeField]
        private Button _button = default;

        [SerializeField]
        private CanvasIdentificator _identificator = default;

        private void Awake() =>
            _button.OnClickAsObservable().Subscribe(_ =>
            {
                CanvasController.Instance.SetActiveCanvas(_identificator, true);
            }).AddTo(this);
    }
}
