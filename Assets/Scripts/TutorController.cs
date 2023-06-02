using Cass.LoadManager;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Cass.FirstEntry;
using Unity.Netcode;
using Cinemachine;
using Cass.Character;
using UniRx;

public class TutorController : MonoBehaviour, ILoadingCondition
{
    public int Order => 5;

    public string Name => typeof(TutorController).Name;

    public bool IsInited => _isInited;

    [SerializeField]
    private CinemachineVirtualCamera _virtualCamera = default;

    private NetworkManager _networkManager = default;

    private bool _isInited = false;

    public Task<Action> Initialization(CancellationToken token)
    {
        _isInited = true;

        _networkManager = FindObjectOfType<NetworkManager>();

        return Task.FromResult<Action>(OnSceneStart);
    }

    private void OnSceneStart()
    {
        Debug.LogWarning("Entry start");
        FirstEntry firstEntry = new FirstEntry();

        if (firstEntry.IsFirstEntry())
        {
            FirstEntry();
        }
        else
        {
            TutorialStart();
        }
    }

    private void FirstEntry()
    {
    }

    private void TutorialStart()
    {
        _networkManager.StartHost();

        MainCharacterController.TargetTransform.Subscribe(target => 
        {
            _virtualCamera.Follow = target;
            _virtualCamera.LookAt = target;
        }).Dispose();
    }
}
