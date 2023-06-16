using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class ShopTransition : MonoBehaviour, IInteractableObject
{
    [SerializeField]
    private Transform _animationTrans = default;

    private Vector3 _defaultScale = default;

    private CompositeDisposable _dis = new CompositeDisposable();

    private void Awake()
    {
        _defaultScale = _animationTrans.localScale;
    }
    public void OnObjectInteract()
    {
        Debug.LogError("Interact");
    }

    public void StartInteractionAnim(bool isStart)
    {
        if (!isStart)
        {
            _animationTrans.localScale = _defaultScale;
            _dis.Clear();
            return;
        }

        Vector3 defaultScale = _animationTrans.localScale;
        Vector3 endScale = _animationTrans.localScale / 2;
        float iterator = 0;
        Observable.EveryUpdate().Subscribe(_ => 
        {
            _animationTrans.localScale = Vector3.Lerp(defaultScale, endScale, iterator);

            if(iterator >= 1)
            {
                (defaultScale, endScale) = (endScale, defaultScale);
                iterator = 0;
            }

            iterator += Time.deltaTime;

        }).AddTo(_dis);
    }
}
