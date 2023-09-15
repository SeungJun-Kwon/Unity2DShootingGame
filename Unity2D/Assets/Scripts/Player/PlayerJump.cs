using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : IState, IPunObservable
{
    PlayerController _playerController;

    public PlayerJump(PlayerController playerController)
    {
        _playerController = playerController;
    }

    public void OnEnter()
    {
        _playerController.Jump();
        _playerController.PlayAnimation(State.JUMP);
        _playerController._playerManager._jumpLeft--;
    }

    public void OnExit()
    {
        _playerController.InstantiateObject("Prefabs/Effects/LandingEffect", _playerController.transform.position, Quaternion.identity);
        _playerController._playerManager._jumpLeft = _playerController._playerManager.MaxJump;
        _playerController.PlayAnimation(State.JUMP);
    }

    public void OnFixedUpdate()
    {
        if (_playerController.IsGrounded())
            _playerController.photonView.RPC(nameof(_playerController.ExitState), RpcTarget.All, State.JUMP);
    }

    public void OnUpdate()
    {

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

}
