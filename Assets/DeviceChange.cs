using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class DeviceChange : MonoBehaviour
{
    private PlayerInput _playerInput = default;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

}
