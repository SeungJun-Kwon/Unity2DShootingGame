using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Security.Cryptography;
using System;
using UnityEngine.UI;
using TMPro;
using Cinemachine;
using System.Linq;

public enum State { IDLE, MOVE, JUMP, ATTACK, HIT, DIE, }
public interface IState
{
    void OnEnter();
    void OnExit();
    void OnUpdate();
    void OnFixedUpdate();
}

[RequireComponent(typeof(PlayerManager))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    bool _isGrounded = true;
    public bool IsGrounded
    {
        get { return _isGrounded; }
        set
        {
            _isGrounded = value;
            _animator.SetBool("IsGrounded", value);
            if (_isGrounded)
            {
                _playerManager._doubleJump = false;
                if (FindState(State.JUMP))
                    photonView.RPC(nameof(ExitState), RpcTarget.AllBuffered, State.JUMP);
            }
            else
                photonView.RPC(nameof(EnterState), RpcTarget.AllBuffered, State.JUMP);
        }
    }

    [HideInInspector] public Rigidbody2D _rigidbody;
    [HideInInspector] public CapsuleCollider2D _collider;
    [HideInInspector] public Animator _animator;
    [HideInInspector] public SpriteRenderer _spriteRenderer;
    [HideInInspector] public UserInfo _info;
    [HideInInspector] public PlayerManager _playerManager;

    [Header("Effect")]
    [SerializeField] GameObject _appearEffect;
    [SerializeField] GameObject _dyingEffect;

    [Header("Gun")]
    public Transform _gunPart;
    public SpriteRenderer _gunSprite;

    [Header("Unit UI")]
    public GameObject _unitGameUI;
    public TMP_Text _nameText;
    public Image _hpBar;
    [SerializeField] PlayerUIController _playerUIController;

    [Header("Value Setting")]
    public float _moveSpeed = 3f;
    public float _maxSpeed = 10f;
    public float _jumpForce = 15f;

    protected Dictionary<State, IState> _stateDic = new Dictionary<State, IState>();
    protected List<IState> _iStateArr = new List<IState>();
    protected List<State> _stateArr = new List<State>();

    protected Dictionary<string, GameObject> _subObjectDic = new();

    protected Vector3 _curPos;

    [HideInInspector] public Vector2 _gunLeftPos = new Vector2(-0.5f, -0.175f);
    [HideInInspector] public Vector2 _gunRightPos = new Vector2(0.5f, -0.175f);

    bool _available = false;
    public bool Available
    {
        get { return _available; }
        set
        {
            _available = value;

            if (value)
                photonView.RPC(nameof(SetPlayerColor), RpcTarget.All, 1f, 1f, 1f, 1f);
        }
    }

    protected virtual void Awake()
    {
        TryGetComponent(out _rigidbody);
        TryGetComponent(out _collider);
        TryGetComponent(out _animator);
        TryGetComponent(out _spriteRenderer);
        TryGetComponent(out _playerManager);
        _playerManager.OnChangeWeapon += SetGunSpriteRPC;

        StateInit();
        EnterState(State.IDLE);

        if (_appearEffect == null)
            _appearEffect = transform.Find("AppearEffect").gameObject;
        if (_dyingEffect == null)
            _dyingEffect = transform.Find("DyingEffect").gameObject;
        _subObjectDic[_appearEffect.transform.name] = _appearEffect;
        _subObjectDic[_dyingEffect.transform.name] = _dyingEffect;
        _appearEffect.SetActive(false);
        _dyingEffect.SetActive(false);

        if (_gunPart == null)
            transform.Find("GunPart").gameObject.TryGetComponent(out _gunPart);
        if (_gunSprite == null)
            _gunPart.Find("GunSprite").gameObject.TryGetComponent(out _gunSprite);
        _subObjectDic[_gunPart.transform.name] = _gunPart.gameObject;
        _subObjectDic[_gunSprite.transform.name] = _gunSprite.gameObject;
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    private void Start()
    {
        IsGrounded = true;

        //_nameText.text = _photonView.IsMine ? PhotonNetwork.NickName : _photonView.Owner.NickName;
        //_nameText.color = _photonView.IsMine ? Color.green : Color.red;

        if (photonView.IsMine)
        {
            //photonView.RPC("LoadUserData", RpcTarget.AllBuffered);
            //_playerManager.photonView.RPC("LoadUserData", RpcTarget.AllBuffered, FirebaseAuthManager.Instance._user.Email);
            gameObject.layer = LayerMask.NameToLayer("Player");
            GameUIController.Instance._playerController = this;
            _playerUIController._itsMe.gameObject.SetActive(true);
            photonView.RPC(nameof(SetTransformName), RpcTarget.All, PhotonNetwork.NickName);
        }
        else
            gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    protected virtual void Update()
    {
        if (photonView.IsMine && Available)
        {
            for (int i = 0; i < _iStateArr.Count; i++)
                _iStateArr[i].OnUpdate();

            if (Input.GetKeyDown(KeyCode.Tab))
                photonView.RPC("Hit", RpcTarget.All, 50);

            if (Input.GetButtonDown("Jump"))
            {
                // 1. 땅에 있는 상태
                // 2. 공중이라면 더블 점프 업그레이드가 존재하며 더블 점프를 사용하지 않았을 때
                if (IsGrounded)
                    photonView.RPC(nameof(EnterState), RpcTarget.All, State.JUMP);
                else if (!IsGrounded && !_playerManager._doubleJump && _playerManager._curStat._doubleJump)
                    photonView.RPC(nameof(ForceEnterState), RpcTarget.All, State.JUMP);
            }

            if (Input.GetButtonDown("Attack") && !FindState(State.ATTACK))
                photonView.RPC(nameof(EnterState), RpcTarget.All, State.ATTACK);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!Available)
            return;

        if (photonView.IsMine)
        {
            for (int i = 0; i < _iStateArr.Count; i++)
                _iStateArr[i].OnFixedUpdate();

            if (_rigidbody.velocity.y < -0.01f || _rigidbody.velocity.y > 0.01f)
                IsGrounded = false;
            else if (_rigidbody.velocity.y >= -0.01f && _rigidbody.velocity.y <= 0.01f)
                IsGrounded = true;
        }
        else
        {
            if ((transform.position - _curPos).sqrMagnitude >= 100f)
                transform.position = _curPos;
            else
                transform.position = Vector3.Lerp(transform.position, _curPos, Time.deltaTime * 10f);
        }
    }

    public void Init()
    {
        //GameObject unitGameUI = Resources.Load("Prefabs/UI/UnitGameUI") as GameObject;
        //_unitGameUI = Instantiate(unitGameUI, transform);
        //_unitGameUI.transform.Find("Name").gameObject.TryGetComponent(out _nameText);
        //_unitGameUI.transform.Find("HpBar").Find("Fill").gameObject.TryGetComponent(out _hpBar);        
        //GameObject.Find("CMvcam").TryGetComponent(out CinemachineVirtualCamera cm);
        //cm.Follow = transform;
        //cm.LookAt = transform;
    }

    [PunRPC]
    public async void LoadUserData() => _info = await FirebaseFirestoreManager.Instance.LoadUserInfo(FirebaseAuthManager.Instance._user.Email);

    [PunRPC]
    public void SetTransformName(string name) => transform.name = name;

    public void SetGunSpriteRPC() => photonView.RPC(nameof(SetGunSprite), RpcTarget.All);

    [PunRPC]
    public void StateInit()
    {
        _stateDic.Add(State.IDLE, new PlayerIdle(this));
        _stateDic.Add(State.MOVE, new PlayerMove(this));
        _stateDic.Add(State.JUMP, new PlayerJump(this));
        _stateDic.Add(State.ATTACK, new PlayerAttack(this));
        _stateDic.Add(State.HIT, new PlayerHit(this));
        _stateDic.Add(State.DIE, new PlayerDie(this));
    }

    [PunRPC]
    public void SetGunSprite()
    {
        if (_playerManager.CurWeapon == null)
            _gunSprite.sprite = null;
        else
            _gunSprite.sprite = _playerManager.CurWeapon._icon;
    }

    public void InstantiateObject(string path, Vector3 pos, Quaternion rot)
    {
        GameObject go = Resources.Load(path) as GameObject;

        Instantiate(go, pos, rot);
    }

    public void DestroyObject(string name)
    {
        if (_subObjectDic.TryGetValue(name, out var go))
            Destroy(go);
    }

    public void SetActiveObject(string name, bool toggle)
    {
        if (_subObjectDic.TryGetValue(name, out var go))
            go.SetActive(toggle);
    }

    public virtual void Move(float moveX)
    {
        // AllBuffered로 하는 이유는 재접속시 동기화해주기 위해서
        // RpcTarget.All 이면, 호출되고 잊어버림.
        // RpcTarget.AllBuffered 이면, 호출되고 버퍼에 저장됨. 이후에 누가 들어오면 자동으로 순차적으로 실행된다.
        // 버퍼에 너무 많이 저장되면, 네트워크가 약한 클라이언트는 끊어질 수 있다고 한다.
        photonView.RPC("MoveXRPC", RpcTarget.AllBuffered, moveX);

        _rigidbody.AddForce(Vector2.right * moveX * _moveSpeed * (_playerManager._moveSpeed / 100f), ForceMode2D.Impulse);
        if (_rigidbody.velocity.x > _maxSpeed)
            _rigidbody.velocity = new Vector2(_maxSpeed, _rigidbody.velocity.y);
        else if (_rigidbody.velocity.x < -_maxSpeed)
            _rigidbody.velocity = new Vector2(-_maxSpeed, _rigidbody.velocity.y);
    }

    [PunRPC]
    public virtual void MoveXRPC(float moveX)
    {
        if (moveX < 0)
        {
            _spriteRenderer.flipX = true;
            _gunSprite.flipX = true;
            _gunPart.localPosition = _gunLeftPos;
        }
        else
        {
            _spriteRenderer.flipX = false;
            _gunSprite.flipX = false;
            _gunPart.localPosition = _gunRightPos;
        }
    }

    public virtual void Move(Transform target)
    {
        float moveX;
        if (target.position.x - transform.position.x > 0f)
        {
            moveX = 1f;
            Move(moveX);
        }
        else if (target.position.x - transform.position.x < 0f)
        {
            moveX = -1f;
            Move(moveX);
        }
    }

    public void Jump() => _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

    public virtual void Attack()
    {
        ExitState(State.MOVE);
        ExitState(State.IDLE);
        EnterState(State.ATTACK);
    }

    public virtual void PlayAnimation(State state)
    {
        switch (state)
        {
            case State.IDLE:
                _animator.SetBool("IsWalking", false);
                break;
            case State.MOVE:
                _animator.SetBool("IsWalking", true);
                break;
            case State.ATTACK:
                _animator.SetTrigger("Attack");
                break;
            case State.JUMP:
                _animator.SetTrigger("Jump");
                break;
            case State.DIE:
                _animator.SetTrigger("Die");
                break;
        }
    }

    [PunRPC]
    public void EnterState(State state)
    {
        if (!_stateArr.Contains(state))
        {
            _stateArr.Add(state);
            _stateDic.TryGetValue(state, out var iState);
            iState.OnEnter();
            _iStateArr.Add(iState);
        }
    }

    [PunRPC]
    public void ForceEnterState(State state)
    {
        if (!_stateArr.Contains(state))
            photonView.RPC(nameof(EnterState), RpcTarget.All, state);
        else
        {
            _stateDic.TryGetValue(state, out var iState);
            iState.OnEnter();
        }
    }

    [PunRPC]
    public void ExitState(State state)
    {
        if (_stateArr.Contains(state))
        {
            _stateArr.Remove(state);
            _stateDic.TryGetValue(state, out var iState);
            iState.OnExit();
            _iStateArr.Remove(iState);
        }
    }

    public bool FindState(State state)
    {
        return _stateArr.Contains(state);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            _curPos = (Vector3)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void Hit(int damage)
    {
        if (FindState(State.HIT) || !Available)
            return;

        _playerManager.CurHp -= damage;

        if (_playerManager.CurHp > 0)
            photonView.RPC(nameof(EnterState), RpcTarget.All, State.HIT);

        if (_playerManager.CurHp <= 0)
            Die();
    }

    [PunRPC]
    public void HitWithEffect(int damage, Vector2 hitPos)
    {
        if (FindState(State.HIT) || !Available)
            return;

        _playerManager.CurHp -= damage;

        PhotonNetwork.Instantiate("Prefabs/01_Test/HitEffect", hitPos, Quaternion.identity).TryGetComponent(out HitEffect hitEffect);
        hitEffect.photonView.RPC(nameof(hitEffect.SetWeapon), RpcTarget.All, _playerManager._weaponName);

        if (_playerManager.CurHp > 0)
            photonView.RPC(nameof(EnterState), RpcTarget.All, State.HIT);

        if (_playerManager.CurHp <= 0)
            Die();
    }

    [PunRPC]
    public void SetPlayerColor(float r, float g, float b, float a)
    {
        Color color = new Color(r, g, b, a);

        _spriteRenderer.color = color;
        _gunSprite.color = color;
    }

    [PunRPC]
    public void BlinkRPC() => StartCoroutine(Blink());

    IEnumerator Blink()
    {
        float count = 0f;
        WaitForSeconds wait = new WaitForSeconds(0.1f);

        while (count < 1f)
        {
            yield return wait;
            count += 0.1f;

            photonView.RPC(nameof(SetPlayerColor), RpcTarget.All, 1f, 1f, 1f, 0f);

            yield return wait;
            count += 0.1f;

            photonView.RPC(nameof(SetPlayerColor), RpcTarget.All, 1f, 1f, 1f, 1f);
        }

        photonView.RPC(nameof(SetPlayerColor), RpcTarget.All, 1f, 1f, 1f, 1f);
        photonView.RPC(nameof(ExitState), RpcTarget.All, State.HIT);
    }

    void Die()
    {
        for(int i = _stateArr.Count - 1; i >= 0; i--)
            photonView.RPC(nameof(ExitState), RpcTarget.AllBuffered, _stateArr[i]);

        photonView.RPC(nameof(EnterState), RpcTarget.AllBuffered, State.DIE);
    }

    [PunRPC]
    public void DyingEffectCorRPC() => StartCoroutine(DyingEffectCor());

    IEnumerator DyingEffectCor()
    {
        float t = 0.6f;

        while(t >= 0.1f)
        {
            photonView.RPC(nameof(SetPlayerColor), RpcTarget.All, 1f, 1f, 1f, 0.5f);

            yield return new WaitForSeconds(t);

            photonView.RPC(nameof(SetPlayerColor), RpcTarget.All, 1f, 1f, 1f, 1f);

            yield return new WaitForSeconds(t);

            t /= 2;
        }

        _dyingEffect.SetActive(true);
        photonView.RPC(nameof(SetPlayerColor), RpcTarget.All, 1f, 1f, 1f, 0f);

        yield return new WaitForSeconds(0.5f);

        if(photonView.IsMine)
            GameManager.Instance.photonView.RPC(nameof(GameManager.Instance.RoundFinish), RpcTarget.All);
    }

    public GameObject Instantiate(GameObject go, Transform parent = null)
    {
        GameObject result = null;

        if (parent == null)
            result = UnityEngine.Object.Instantiate(go);
        else
            result = UnityEngine.Object.Instantiate(go, parent);

        return result;
    }

    [PunRPC]
    public void SendChatRPC(string text)
    {
        _playerUIController.ShowChat(text);
    }
}