using UnityEngine;
using UniRx;
using UniRx.Triggers;

[RequireComponent(typeof(Rigidbody))]
public class SawThrowerJumpModule : MonoBehaviour
{
    [SerializeField]
    private Collider _trigger = default;

    [SerializeField]
    private LayerMask _playerLayer = default;

    [SerializeField]
    private float _jumpPower = default;

    private Rigidbody _rb = default;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        _trigger.OnTriggerEnterAsObservable().Subscribe(trigger => 
        {
            if((1 << trigger.gameObject.layer & _playerLayer.value) == 0)
            {
                return;
            }

            _rb.AddForce(Vector3.up * _jumpPower * _rb.mass, ForceMode.Impulse); 
        }).AddTo(this);
    }
}
