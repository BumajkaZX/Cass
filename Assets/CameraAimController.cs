using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Cass.LoadManager;
using System.Threading.Tasks;
using System;
using System.Threading;
using UniRx;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraAimController : MonoBehaviour, ILoadingCondition
{
    public int Order => 6;

    public string Name => typeof(CameraAimController).Name;

    public bool IsInited => _isInited;

    [SerializeField, Range(0, 0.2f)]
    private float _followPower = 1f;

    [SerializeField, Range(0, 0.2f)]
    private float _aimPower = 1f;

    private bool _isInited = false;

    private CinemachineVirtualCamera _virtualCamera = default;

    public Task<Action> Initialization(CancellationToken token)
    {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();

        _isInited = true;

        return Task.FromResult<Action>(OnSceneStart);
    }

    private void OnSceneStart()
    {
        var transposer = _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        var composer = _virtualCamera.GetCinemachineComponent<CinemachineComposer>();
        var defaultOffsetTransposer = transposer.m_FollowOffset;
        var defaultOffsetComposer = composer.m_TrackedObjectOffset;
        var target = _virtualCamera.Follow;

        Observable.EveryUpdate().Subscribe(_ =>
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            transposer.m_FollowOffset = defaultOffsetTransposer + new Vector3(0, 0, -target.position.z * _followPower);
            composer.m_TrackedObjectOffset = defaultOffsetComposer + new Vector3(0, target.position.z * _aimPower, 0);
        }).AddTo(this);

    }
}
