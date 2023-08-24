using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerDie : IState
{
    PlayerController _playerController;

    public PlayerDie(PlayerController playerController)
    {
        _playerController = playerController;
    }

    public void OnEnter()
    {
        _playerController.Available = false;

        if (_playerController.photonView.IsMine)
            _playerController.photonView.RPC(nameof(_playerController.DyingEffectCorRPC), RpcTarget.All);
    }

    public void OnExit()
    {
        
    }

    public void OnFixedUpdate()
    {
        
    }

    public void OnUpdate()
    {
        
    }
}
