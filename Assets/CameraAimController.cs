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
       
    }
}
