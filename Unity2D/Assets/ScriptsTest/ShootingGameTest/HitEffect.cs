using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class HitEffect : MonoBehaviourPunCallbacks
{
    public ParticleSystem _particleSystem;
    public Renderer _renderer;

    WeaponSO _curWeapon;

    private void Awake()
    {
        TryGetComponent(out _particleSystem);
        _particleSystem.TryGetComponent(out _renderer);
    }

    [PunRPC]
    public void SetWeapon(string weaponName)
    {
        if (photonView.IsMine)
        {
            if (weaponName == "")
            {
                PhotonNetwork.Destroy(gameObject);
                return;
            }

            _curWeapon = WeaponSOManager.Instance._weaponDic[weaponName];
            _renderer.material = _curWeapon._hitEffectMat;
        }
    }
}
