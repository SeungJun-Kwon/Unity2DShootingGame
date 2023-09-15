using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHit : IState
{
    PlayerController _playerController;

    public PlayerHit(PlayerController playerController)
    {
        _playerController = playerController;
    }

    public void OnEnter()
    {
        if(_playerController.photonView.IsMine)
            _playerController.photonView.RPC("BlinkRPC", RpcTarget.All);

        if ((Vector2)_playerController._gunPart.localPosition == _playerController._gunLeftPos)
            _playerController._rigidbody.AddForce(Vector2.right * 5f, ForceMode2D.Impulse);
        else
            _playerController._rigidbody.AddForce(Vector2.left * 5f, ForceMode2D.Impulse);

        GameManager.Instance.StartCoroutine(GameManager.Instance.ShakeCMVCamera(2f, .1f));
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
