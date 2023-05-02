using UnityEngine;
using UniRx.Triggers;
using UniRx;

/// <summary>
/// Enables kinematic on rigidbody if contacts trigger
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class KinematicEnabler : MonoBehaviour
{
    [SerializeField]
    private Collider _trigger = default;

    [SerializeField]
    private LayerMask _playerLayer = default;

    private Rigidbody _rb = default;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        _trigger.OnTriggerEnterAsObservable().Subscribe(trigger => 
        {
            if ((1 << trigger.gameObject.layer & _playerLayer.value) == 0)
            {
                return;
            }

            _rb.isKinematic = true;
        }).AddTo(this);

        _trigger.OnTriggerExitAsObservable().Subscribe(trigger => 
        {
            if ((1 << trigger.gameObject.layer & _playerLayer.value) == 0)
            {
                return;
            }

            _rb.isKinematic = false;
        }).AddTo(this);
    }
}
