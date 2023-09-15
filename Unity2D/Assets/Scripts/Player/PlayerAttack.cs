using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerAttack : IState, IPunObservable
{
    PlayerController _playerController;

    float _attackDuration;

    BulletGO _bulletGO;
    Vector2 _bulletPos;
    Vector2 _dir;
    Vector2 _spriteSize = Vector2.zero;

    public PlayerAttack(PlayerController unit)
    {
        _playerController = unit;
    }

    float _count;

    public void OnEnter()
    {
        if (_playerController.photonView.IsMine)
        {
            _playerController.PlayAnimation(State.ATTACK);
            _count = 0;
            _bulletGO = null;
            _bulletPos = Vector2.zero;
            _dir = Vector2.zero;

            if (_spriteSize == Vector2.zero)
                _spriteSize = _playerController._playerManager.CurWeapon._bullet.bounds.size;

            _attackDuration = _playerController._playerManager._attackSpeed * (100f / (_playerController._playerManager.CurWeapon._attackSpeed + _playerController._playerManager._curStat._attackSpeed));

            if (_playerController._gunPart.localPosition == (Vector3)_playerController._gunRightPos)
                _dir = Vector2.right;
            else
                _dir = Vector2.left;

            _bulletPos = new Vector2(_playerController._gunPart.position.x, _playerController._gunPart.position.y);

            for (int i = 0; i <= _playerController._playerManager._curStat._multiShot; i++)
            {
                Vector2 multiPos;
                if (i % 2 == 0)
                    multiPos = new Vector2(_bulletPos.x, _bulletPos.y - (_spriteSize.y * 1.5f * (i / 2)));
                else
                    multiPos = new Vector2(_bulletPos.x, _bulletPos.y + (_spriteSize.y * 1.5f * ((i / 2) + i % 2)));

                PhotonNetwork.Instantiate("Prefabs/Players/Bullet", multiPos, Quaternion.identity).TryGetComponent(out _bulletGO);
                _bulletGO.photonView.RPC("SetWeapon", RpcTarget.All, _playerController._playerManager.CurWeapon._name, _dir);
                _bulletGO.Shoot();
                _bulletGO._upgradeDamage = _playerController._playerManager._curStat._damage;
            }

            if (_playerController._playerManager.CurWeapon._shootEffect != null || _playerController._playerManager.CurWeapon._shootEffectAnim != null)
            {
                PhotonNetwork.Instantiate("Prefabs/Effects/ShootEffect", _bulletPos, Quaternion.identity).TryGetComponent(out ShootEffect shootEffect);
                shootEffect.photonView.RPC("SetEffect", RpcTarget.All, _playerController._playerManager.CurWeapon._name, _dir == Vector2.right ? false : true);
            }

            GameManager.Instance.StartCoroutine(GameManager.Instance.ShakeCMVCamera(1f, .1f));
        }
    }

    public void OnExit()
    {
        
    }

    public void OnFixedUpdate()
    {

    }

    public void OnUpdate()
    {
        _count += Time.deltaTime;
        if (_count > _attackDuration)
        {
            _playerController.photonView.RPC("ExitState", RpcTarget.AllBuffered, State.ATTACK);
            _playerController.photonView.RPC("EnterState", RpcTarget.AllBuffered, State.IDLE);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_count);
            stream.SendNext(_attackDuration);
        }
        else if (stream.IsReading)
        {
            _count = (float)stream.ReceiveNext();
            _attackDuration = (float)stream.ReceiveNext();
        }
    }
}
