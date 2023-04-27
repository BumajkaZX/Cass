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

    [SerializeField]
    private bool _useRigidbody = true;

    private Rigidbody _rb = default;
    private void Awake()
    {
        if (_useRigidbody)
        {
            _rb = GetComponent<Rigidbody>();

            if(_rb == null)
            {
                Debug.LogError("Not attached rigidbody on " + gameObject.name);
                return;
            }

            Observable.EveryFixedUpdate().Subscribe(_ => _rb.MoveRotation(Quaternion.Euler((_rotationAxis * _rotationSpeed) + transform.rotation.eulerAngles))).AddTo(this);
        }
        else
        {
            Observable.EveryFixedUpdate().Subscribe(_ => transform.Rotate(_rotationAxis * _rotationSpeed)).AddTo(this);
        }
    }
}
