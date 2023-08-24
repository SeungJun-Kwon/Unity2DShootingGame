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

        if(!_playerController.IsGrounded)
            _playerController._playerManager._doubleJump = true;
    }

    public void OnExit()
    {
        _playerController.InstantiateObject("Prefabs/01_Test/LandingEffect", _playerController.transform.position, Quaternion.identity);
    }

    public void OnFixedUpdate()
    {

    }

    public void OnUpdate()
    {

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

}
