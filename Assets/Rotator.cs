using UniRx;
using UnityEngine;

/// <summary>
/// Rotate object
/// </summary>
public class Rotator : MonoBehaviour
{
    [SerializeField]
    private Vector3 _rotationAxis = default;

    [SerializeField]
    private float _rotationSpeed = default;
    private void Awake() =>
        Observable.EveryFixedUpdate().Subscribe(_ => transform.Rotate(_rotationAxis * _rotationSpeed)).AddTo(this); 
}
