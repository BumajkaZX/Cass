using Cass.Character;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayersPool
{
    public static List<MainCharacterController> Players => _players;

    private static List<MainCharacterController> _players = new List<MainCharacterController>();

    public static void AddPlayer(MainCharacterController player) => _players.Add(player);

    public static void RemovePlayer(MainCharacterController player) => _players.Remove(player);
}
