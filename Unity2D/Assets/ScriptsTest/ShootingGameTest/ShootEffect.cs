using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootEffect : MonoBehaviourPunCallbacks
{
    public SpriteRenderer _spriteRenderer;
    public Animator _animator;

    WeaponSO _curWeapon;

    private void Awake()
    {
        TryGetComponent(out _spriteRenderer);
        TryGetComponent(out _animator);
    }

    [PunRPC]
    public void SetEffect(string weaponName, bool flipX = false)
    {
        transform.localScale = flipX ? -Vector3.one : Vector3.one;

        _curWeapon = WeaponSOManager.Instance._weaponDic[weaponName];

        if (_curWeapon._shootEffect != null)
            _spriteRenderer.sprite = _curWeapon._shootEffect;
        if (_curWeapon._shootEffectAnim != null)
            _animator.runtimeAnimatorController = _curWeapon._shootEffectAnim;

        Invoke("InvokeDestroy", _curWeapon._shootEffectAnim.animationClips[0].length);
    }

    public void InvokeDestroy()
    {
        if(photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
