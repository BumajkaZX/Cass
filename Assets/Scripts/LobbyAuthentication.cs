using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UniRx;

public class LobbyAuthentication : MonoBehaviour
{
    [HideInInspector]
    public ReactiveProperty<string> LobbyCode = new ReactiveProperty<string>();

    [SerializeField, Range(2, 10)]
    private int _maxPlayers = 2;

    private UnityTransport _unityTransport = default;

    private async void Awake()
    {
        _unityTransport = GetComponentInParent<UnityTransport>();

        await Authenticate();
    }

    private static async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync(); //TODO: Переделать под сервисы
    }

    /// <summary>
    /// Create game
    /// </summary>
    public async void CreateGame()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_maxPlayers);

        LobbyCode.Value = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        _unityTransport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

        NetworkManager.Singleton.StartHost();
    }

    /// <summary>
    /// Join game with key
    /// </summary>
    /// <param name="joinKey"></param>
    public async void JoinGame(string joinKey)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinKey);

        _unityTransport.SetClientRelayData(joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port, joinAllocation.AllocationIdBytes, joinAllocation.Key, joinAllocation.ConnectionData, joinAllocation.HostConnectionData);

        NetworkManager.Singleton.StartClient();
    }
}
