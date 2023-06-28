#if UNITY_EDITOR

using ParrelSync;

#endif

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UniRx;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Services.Relay.Models;

public class LobbyManager : MonoBehaviour
{
    private const string JUST_FOR_WORK = "Random";

    public ReactiveProperty<Lobby> ConnectedLobby = new ReactiveProperty<Lobby>();

    public ReactiveProperty<QueryResponse> LobbiesList = new ReactiveProperty<QueryResponse>();

    public ReactiveProperty<bool> IsInited = new ReactiveProperty<bool>();

    [SerializeField, Range(2, 10)]
    private int _maxPlayers = 2;

    [SerializeField]
    private TMPro.TMP_Text _text = default;

    private UnityTransport _unityTransport = default;
    private string _lobbyKey = default;
    private string _playerId = default;

    private CompositeDisposable _dis = new CompositeDisposable();

    private void Awake() => _unityTransport = GetComponentInParent<UnityTransport>();

    private async void Start()
    {
        await Authenticate();

        IsInited.Value = true;
    }

    private async Task<QueryResponse> GetLobbiesList(int lobbiesCount)
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();

            options.Count = lobbiesCount;

            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    QueryFilter.FieldOptions.AvailableSlots,
                    "0",
                    QueryFilter.OpOptions.GT)
            };

            options.Order = new List<QueryOrder>()
            {
                new QueryOrder
                (
                    true,
                    QueryOrder.FieldOptions.Created
                )
            };

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            return lobbies;

        }
        catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
            return null;
        }
    }

    private async Task Authenticate()
    {
        InitializationOptions options = new InitializationOptions();

#if UNITY_EDITOR

        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");

        Debug.LogError(ClonesManager.IsClone() + ClonesManager.GetArgument());
#endif
        AuthenticationService.Instance.SignOut();

        await UnityServices.InitializeAsync(options);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        _playerId = AuthenticationService.Instance.PlayerId;

        Debug.LogError(_playerId);
    }
    private async Task<Lobby> QuickJoinLobby(string lobbyId = null)
    {
        try
        {
            if (ConnectedLobby.Value != null && ConnectedLobby.Value.HostId == _playerId)
            {
                await Lobbies.Instance.RemovePlayerAsync(ConnectedLobby.Value.Id, _playerId);
            }

            Lobby lobby = lobbyId == null ? await Lobbies.Instance.QuickJoinLobbyAsync() : await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);

            JoinAllocation joinAllocation = await Relay.Instance.JoinAllocationAsync(lobby.Data[JUST_FOR_WORK].Value);

            _unityTransport.SetClientRelayData(joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port, joinAllocation.AllocationIdBytes, joinAllocation.Key, joinAllocation.ConnectionData, joinAllocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();

            ConnectedLobby.Value = lobby;

            return lobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError("Can't join " + ex.Message);

            return null;
        }
    }
    private async Task<Lobby> CreateLobby(string lobbyName, bool publicOrPrivate)
    {
        try
        {
            if (ConnectedLobby.Value != null && ConnectedLobby.Value.HostId == _playerId)
            {
                await Lobbies.Instance.DeleteLobbyAsync(ConnectedLobby.Value.Id);
            }

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_maxPlayers);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> { { JUST_FOR_WORK, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            };

            _text.text = joinCode;

            options.IsPrivate = publicOrPrivate;

            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, _maxPlayers, options);

            _lobbyKey = lobby.Id;

            Observable.Timer(TimeSpan.FromSeconds(15)).Repeat().Subscribe(_ => Lobbies.Instance.SendHeartbeatPingAsync(_lobbyKey)).AddTo(this);

            _unityTransport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();

            ConnectedLobby.Value = lobby;

            return lobby;
        }
        catch(Exception ex)
        {
            Debug.LogError(ex.Message);
            return null;
        }
    }

    private async void OnDestroy()
    {
        try
        {
            if (ConnectedLobby.Value != null)
            {
                if (ConnectedLobby.Value.HostId == _playerId)
                {
                   await Lobbies.Instance.DeleteLobbyAsync(ConnectedLobby.Value.Id);
                }
                else
                {
                   await Lobbies.Instance.RemovePlayerAsync(ConnectedLobby.Value.Id, _playerId);
                }
            }
            _dis.Clear();
        }
        catch(Exception ex)
        {
            Debug.LogWarning($"Something happend with {gameObject.name} - {ex.Message}");
        }
    }

    /// <summary>
    /// Connect to lobby
    /// </summary>
    /// <param name="lobbyId"></param>
    public async void TryConnectToLobby(Action<bool> onConnectError, string lobbyId = null)
    {
        Lobby lobby = await QuickJoinLobby(lobbyId);
        if(lobby == null)
        {
            Debug.LogError("Can't connect to lobby");
            onConnectError.Invoke(true);
            return;
        }

        Debug.LogError("Connect to lobby");
        onConnectError.Invoke(false);
        ConnectedLobby.Value = lobby;
    }
    /// <summary>
    /// Create lobby
    /// </summary>
    /// <param name="lobbyName"></param>
    public async void TryCreateLobby(string lobbyName, bool publicOrPrivate)
    {
        Lobby lobby = await CreateLobby(lobbyName, publicOrPrivate);

        if(lobby == null)
        {
            //Service not available or internet
            Debug.LogError("Can't create lobby");
            return;
        }

        ConnectedLobby.Value = lobby;

        Debug.LogWarning("Lobby create");
    }        
    /// <summary>
    /// Force update lobbies list
    /// </summary>
    public async void ForceUpdateLobbiesList(int lobbiesCount) => LobbiesList.Value = await GetLobbiesList(lobbiesCount);
}
