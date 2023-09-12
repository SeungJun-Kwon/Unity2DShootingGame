using Firebase.Auth;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public UserInfo _info;

    public Dictionary<UpgradeSO, int> _upgradeDic = new Dictionary<UpgradeSO, int>();

    public delegate void ChangeWeapon();
    public event ChangeWeapon OnChangeWeapon;
    WeaponSO _curWeapon;
    public WeaponSO CurWeapon
    {
        get { return _curWeapon; }
        set
        {
            _curWeapon = value;
            if (value == null)
            {
                _damage = 0;
                return;
            }

            photonView.RPC("OnChangeWeaponRPC", RpcTarget.All);
            _damage = value._damage;
            _weaponName = value._name;
        }
    }
    public string _weaponName;

    [HideInInspector] public Player _player;

    public IncreaseStat _curStat;

    public int _hp, _damage;
    public float _moveSpeed, _attackSpeed;
    public bool _doubleJump;

    private float _curHp;
    public float CurHp
    {
        get { return _curHp; }
        set
        {
            _curHp = value;

            if (_curHp < 0)
                _curHp = 0;

            if (GameUIController.Instance != null && _player != null)
            {
                if ((bool)_player.CustomProperties["Player1"])
                    GameUIController.Instance._player1Hp.fillAmount = value / _hp;
                else
                    GameUIController.Instance._player2Hp.fillAmount = value / _hp;
            }
        }
    }

    private void Awake()
    {
        Init();
    }

    [PunRPC]
    public async void LoadUserData(string email)
    {
        _info = await FirebaseFirestoreManager.Instance.LoadUserInfoByEmail(email);
        print(_info.ToString());
    }

    void Init()
    {
        _player = null;
        _hp = 100;
        CurHp = _hp;
        _moveSpeed = 100f;
        _attackSpeed = 1f;
        _doubleJump = false;
        CurWeapon = null;
    }

    [PunRPC]
    public void SetPlayer(Player player) => _player = player;

    [PunRPC]
    public void SetWeapon(string weaponName)
    {
        WeaponSO weapon = WeaponSOManager.Instance._weaponDic[weaponName];
        CurWeapon = weapon;
    }

    [PunRPC]
    public void OnChangeWeaponRPC() => OnChangeWeapon.Invoke();

    [PunRPC]
    public void AddUpgrade(string upgradeName)
    {
        UpgradeSO upgrade = UpgradeSOManager.Instance._upgradeDic[upgradeName];

        if(_upgradeDic.TryGetValue(upgrade, out var count))
            _upgradeDic[upgrade] = count + 1;
        else
            _upgradeDic[upgrade] = 0;

        if (_upgradeDic[upgrade] >= upgrade._increaseStat.Count)
            return;

        _curStat += upgrade._increaseStat[_upgradeDic[upgrade]];
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

    private void OnApplicationQuit()
    {
        FirebaseFirestoreManager.Instance.UpdateUserInfo(FirebaseAuthManager.Instance._user, _info);
    }
}
