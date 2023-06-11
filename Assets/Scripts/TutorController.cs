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

    [SerializeField]
    private CinemachineVirtualCamera _firstEntryCamera = default;

    [SerializeField]
    private Transform _playerSpawnPos = default;

    [SerializeField]
    private Transform _firstEntrySpawnPos = default;

    [SerializeField]
    private Transform _changeCharacterCanvas = default;

    [SerializeField]
    private Transform _inputCharacterCanvas = default;

    [SerializeField]
    private NetworkObject _playerNetworkObject = default;

    private NetworkManager _networkManager = default;

    private bool _isInited = false;

    public Task<Action> Initialization(CancellationToken token)
    {
        _isInited = true;

        _networkManager = FindObjectOfType<NetworkManager>();

        var player = Instantiate(_playerNetworkObject);

        FirstEntry firstEntry = new FirstEntry();

        player.transform.position = firstEntry.IsFirstEntry() ? _firstEntrySpawnPos.position : _playerSpawnPos.position;

        _networkManager.StartHost();

        MainCharacterController.TargetTransform.Where(_ => _ != null).Subscribe(target =>
        {
                _firstEntryCamera.Follow = target;
                _firstEntryCamera.LookAt = target;
                _virtualCamera.Follow = target;
                _virtualCamera.LookAt = target;
        }).AddTo(this);

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
        var playersPool = PlayersPool.Players;

        foreach(MainCharacterController player in playersPool)
        {
            player.EnableInput(false);
        }

        _virtualCamera.gameObject.SetActive(false);

        _firstEntryCamera.gameObject.SetActive(true);

        _changeCharacterCanvas.gameObject.SetActive(true);
        _inputCharacterCanvas.gameObject.SetActive(false);
    }

    private void TutorialStart()
    {
        _virtualCamera.gameObject.SetActive(true);

        _firstEntryCamera.gameObject.SetActive(false);

        _changeCharacterCanvas.gameObject.SetActive(false);
        _inputCharacterCanvas.gameObject.SetActive(true);
    }
}
