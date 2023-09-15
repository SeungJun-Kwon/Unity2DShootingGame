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

        GameManager.Instance.photonView.RPC(nameof(GameManager.Instance.StopTimer), RpcTarget.MasterClient);
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
