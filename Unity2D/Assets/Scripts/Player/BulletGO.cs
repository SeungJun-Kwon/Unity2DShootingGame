using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Realtime;
using Photon.Pun;

public class BulletGO : MonoBehaviourPunCallbacks, IPunObservable
{
    public Vector2 _dir;
    public float _power = 0f;
    public int _upgradeDamage = 0;

    protected SpriteRenderer _spriteRenderer;
    protected Animator _animator;
    protected BoxCollider2D _collider;
    protected Rigidbody2D _rb;

    protected Vector3 _curPos;
    protected WeaponSO _curWeapon;

    Vector2 rayOrigin;

    public string _hitEffectPath = "Prefabs/Effects/HitEffect";

    private void Awake()
    {
        TryGetComponent(out _spriteRenderer);
        TryGetComponent(out _animator);
        TryGetComponent(out _collider);
        TryGetComponent(out _rb);
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    private void Start()
    {
        _collider.isTrigger = true;
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public virtual void Shoot()
    {
        if(photonView.IsMine)
        {
            if(_curWeapon != null)
            {
                _rb.AddForce(_dir * _power, ForceMode2D.Impulse);
            }
        }
    }

    [PunRPC]
    public virtual void SetWeapon(string weaponName, Vector2 dir)
    {
        _curWeapon = WeaponSOManager.Instance._weaponDic[weaponName];

        if (_curWeapon._bullet != null)
        {
            _spriteRenderer.sprite = _curWeapon._bullet;
            _collider.size = _spriteRenderer.sprite.bounds.size;
            _collider.offset = new Vector2(_spriteRenderer.sprite.bounds.size.x / 2, 0f);
        }

        if (_curWeapon._bulletAnim != null)
            _animator.runtimeAnimatorController = _curWeapon._bulletAnim;

        _dir = dir;

        if (_curWeapon._type == WeaponType.Normal)
        {
            transform.localScale = dir == Vector2.right ? Vector3.one : -Vector3.one;
            _rb.isKinematic = false;
            _power = 10f;

            Invoke(nameof(InvokeDestroy), 5f);
        }
        else if (_curWeapon._type == WeaponType.Laser)
        {
            transform.localScale = dir == Vector2.right ? new Vector3(3f, 1f, 1f) : new Vector3(-3f, 1f, 1f);
            _rb.isKinematic = true;
            _power = 0f;

            Invoke(nameof(InvokeDestroy), _curWeapon._bulletAnim.animationClips[0].length);
        }

        if (photonView.IsMine)
        {

        }
    }

    public void InvokeDestroy()
    {
        if(photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (photonView.IsMine)
            ConflictHandling(collision);
    }

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if (photonView.IsMine)
    //        ConflictHandling(collision);
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(photonView.IsMine)
            ConflictHandling(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (photonView.IsMine)
            ConflictHandling(collision);
    }

    void ConflictHandling(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (_curWeapon._type == WeaponType.Normal && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);

            Vector2 hitPos = collision.ClosestPoint(transform.position);
            float distanceX = _dir == Vector2.right ? collision.transform.position.x - transform.position.x : transform.position.x - collision.transform.position.x;

            if (_curWeapon._type == WeaponType.Laser)
                hitPos = new Vector2(hitPos.x + distanceX, hitPos.y);

            PhotonNetwork.Instantiate(_hitEffectPath, hitPos, Quaternion.identity).TryGetComponent(out HitEffect hitEffect);
            hitEffect.photonView.RPC(nameof(hitEffect.SetWeapon), RpcTarget.All, _curWeapon._name);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (!collision.gameObject.TryGetComponent(out PlayerController playerController) || !playerController.Available)
                return;

            playerController.photonView.RPC(nameof(playerController.Hit), RpcTarget.All, _curWeapon._damage + Mathf.RoundToInt(_curWeapon._damage * (_upgradeDamage / 100f)));

            Vector2 hitPos = collision.ClosestPoint(transform.position);
            float distanceX = _dir == Vector2.right ? collision.transform.position.x - transform.position.x : transform.position.x - collision.transform.position.x;

            if (_curWeapon._type == WeaponType.Laser)
                hitPos = new Vector2(hitPos.x + distanceX, hitPos.y);

            PhotonNetwork.Instantiate(_hitEffectPath, hitPos, Quaternion.identity).TryGetComponent(out HitEffect hitEffect);
            hitEffect.photonView.RPC(nameof(hitEffect.SetWeapon), RpcTarget.All, _curWeapon._name);

            if (_curWeapon._type == WeaponType.Normal && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }

    void ConflictHandling(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if(_curWeapon._type == WeaponType.Normal && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);

            PhotonNetwork.Instantiate(_hitEffectPath, collision.GetContact(0).point, Quaternion.identity).TryGetComponent(out HitEffect hitEffect);
            hitEffect.photonView.RPC(nameof(hitEffect.SetWeapon), RpcTarget.All, _curWeapon._name);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (!collision.gameObject.TryGetComponent(out PlayerController playerController) || !playerController.Available)
                return;

            playerController.photonView.RPC(nameof(playerController.Hit), RpcTarget.All, _curWeapon._damage + Mathf.RoundToInt(_curWeapon._damage * (_upgradeDamage / 100f)));

            PhotonNetwork.Instantiate(_hitEffectPath, collision.GetContact(0).point, Quaternion.identity).TryGetComponent(out HitEffect hitEffect);
            hitEffect.photonView.RPC(nameof(hitEffect.SetWeapon), RpcTarget.All, _curWeapon._name);

            if (_curWeapon._type == WeaponType.Normal && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else if(stream.IsReading)
        {
            _curPos = (Vector3)stream.ReceiveNext();
        }
    }
}
