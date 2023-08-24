using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdle : IState
{
    PlayerController _playerController;
    public PlayerIdle(PlayerController playerController)
    {
        _playerController = playerController;
    }

    public void OnEnter()
    {
        _playerController.PlayAnimation(State.IDLE);
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {
        if (_playerController.photonView.IsMine && _playerController.Available)
        {
            if ((Input.GetAxisRaw("Horizontal") > 0f || Input.GetAxisRaw("Horizontal") < 0f))
            {
                _playerController.photonView.RPC("ExitState", RpcTarget.AllBuffered, State.IDLE);
                _playerController.photonView.RPC("EnterState", RpcTarget.AllBuffered, State.MOVE);
                //_unitController.ExitState(State.IDLE);
                //_unitController.EnterState(State.MOVE);
            }
        }
    }

    public void OnFixedUpdate()
    {

    }
}
