using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : IState
{
    PlayerController _playerController;
    public PlayerMove(PlayerController playerController)
    {
        _playerController = playerController;
    }

    float _moveX;

    public void OnEnter()
    {
        _playerController.PlayAnimation(State.MOVE);
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {
        if (_playerController.photonView.IsMine && _playerController.Available)
        {
            _moveX = Input.GetAxisRaw("Horizontal");

            if (_moveX == 0)
            {
                _playerController.photonView.RPC("ExitState", RpcTarget.AllBuffered, State.MOVE);
                _playerController.photonView.RPC("EnterState", RpcTarget.AllBuffered, State.IDLE);
                //_playerController.ExitState(State.MOVE);
                //_playerController.EnterState(State.IDLE);
            }
        }
    }

    public void OnFixedUpdate()
    {
        if (_playerController.photonView.IsMine && _moveX != 0)
            _playerController.Move(_moveX);
    }
}
