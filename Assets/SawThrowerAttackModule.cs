using UnityEngine;
using UnityEngine.Pool;
using UniRx;
using UniRx.Triggers;
using FMODUnity;
using System;
using NaughtyAttributes;

/// <summary>
/// Launch saw on player 
/// </summary>
public class SawThrowerAttackModule : MonoBehaviour
{
    [SerializeField]
    private Transform _sawPrefab = default;

    [SerializeField]
    private float _throwForce = default;

    [SerializeField]
    private float _throwTime = default;

    [SerializeField]
    private float _sawLifetime = default;

    [SerializeField]
    private int _poolCount = default;

    [SerializeField]
    private Vector3 _sawPosition = default;

    [SerializeField]
    private Collider _activeTrigger = default;

    [SerializeField]
    private LayerMask _playerLayer = default;

    [SerializeField]
    private StudioEventEmitter _throwSound = default;

    private CompositeDisposable _disposable = new CompositeDisposable();

    private bool _canThrow = true;

    private ObjectPool<Transform> _sawPool = default;

    private CompositeDisposable _throwDisposable = new CompositeDisposable();

    private void Awake()
    {
        CreateSawPool();

        _activeTrigger.OnTriggerStayAsObservable().Subscribe(trigger =>
        {
            if ((1 << trigger.gameObject.layer & _playerLayer.value) == 0 || !_canThrow)
            {
                return;
            }

            ThrowSaw(trigger.transform.position);

        }).AddTo(_throwDisposable);
    }

    private void ThrowSaw(Vector3 posToThrow)
    {
        posToThrow = new Vector3(posToThrow.x, transform.position.y, posToThrow.z);

        Vector3 normalizePos = (posToThrow - transform.position).normalized;

        normalizePos = new Vector3(normalizePos.x, 0, normalizePos.z);

        _canThrow = false;

        _throwSound.Play();

        Rigidbody saw = _sawPool.Get().GetComponent<Rigidbody>();

        saw.AddForce(normalizePos * _throwForce, ForceMode.Impulse);

        Observable.Timer(TimeSpan.FromSeconds(_throwTime)).Subscribe(_ => 
        {
            _canThrow = true;
            _disposable.Clear();
        }).AddTo(_disposable);

        CompositeDisposable dis = new CompositeDisposable();

        Observable.Timer(TimeSpan.FromSeconds(_sawLifetime)).Subscribe(_ => 
        {
            saw.velocity = Vector3.zero;
            _sawPool.Release(saw.transform);
        }).AddTo(dis);
    }

    private void CreateSawPool() =>
        _sawPool = new ObjectPool<Transform>(() => Instantiate(_sawPrefab),
            saw =>
            {
                saw.position = transform.TransformPoint(_sawPosition);
                saw.gameObject.SetActive(true);
            },
            saw => saw.gameObject.SetActive(false),
            saw => Destroy(saw.gameObject),
            false,
            _poolCount
            );

    [Button("Disable")]
    private void DisableThrow()
    {
        _throwDisposable.Clear();
        _sawPool.Clear();
    }

    [Button("Enable")]
    private void EnableThrow() => Awake();

#if UNITY_EDITOR

    private void OnDrawGizmos() => Gizmos.DrawSphere(transform.TransformPoint(_sawPosition), 0.03f);

#endif
}
