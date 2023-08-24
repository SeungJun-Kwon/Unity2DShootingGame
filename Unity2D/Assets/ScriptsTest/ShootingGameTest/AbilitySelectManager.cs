using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySelectManager : MonoBehaviour
{
    public static AbilitySelectManager Instance;

    [SerializeField] AbilitySelectUI _abilityUIPrefab;
    [SerializeField] RectTransform _contentLayout;

    List<WeaponSO> _weaponList = new List<WeaponSO>();
    List<UpgradeSO> _upgradeList = new List<UpgradeSO>();

    Dictionary<int, bool> _check = new Dictionary<int, bool>();

    List<AbilitySelectUI> _abilityUIList = new List<AbilitySelectUI>();
    int _sellectCount = 3;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        _weaponList = Resources.LoadAll<WeaponSO>("ScriptableObject/Weapon").ToList();
        _upgradeList = Resources.LoadAll<UpgradeSO>("ScriptableObject/Upgrade").ToList();

        for (int i = 0; i < _sellectCount; i++)
            _abilityUIList.Add(Instantiate(_abilityUIPrefab, _contentLayout));
        foreach (var ui in _abilityUIList)
            ui.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.GameRound > 0)
        {
            foreach (var ui in _abilityUIList)
                ui.e_onClick.AddListener(FadeOutSelectUI);
            if (GameManager.Instance != null && PhotonNetwork.IsMasterClient)
                GameManager.Instance.photonView.RPC(nameof(GameManager.Instance.TimerCorRPC), RpcTarget.All, 10f);
            if (GameUIController.Instance != null)
                GameUIController.Instance._playerUI.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null && GameManager.Instance.GameRound > 0)
        {
            if(GameManager.Instance != null)
                GameManager.Instance.photonView.RPC(nameof(GameManager.Instance.TimerCorStopRPC), RpcTarget.All);
            if (GameUIController.Instance != null)
                GameUIController.Instance._playerUI.SetActive(true);
        }
    }

    public void SelectWeapon()
    {
        Dictionary<int, bool> _check = new Dictionary<int, bool>();
        for (int i = 0; i < _weaponList.Count; i++)
            _check[i] = false;

        for (int i = 0; i < _sellectCount;)
        {
            int rand = Random.Range(0, _weaponList.Count);
            if (_check[rand])
                continue;

            _abilityUIList[i].gameObject.SetActive(true);
            _abilityUIList[i].SetValue(_weaponList[rand]);
            _check[rand] = true;
            i++;
        }
    }

    public void SelectUpgrade(PlayerManager playerManager)
    {
        bool tryGetValue;

        for (int i = 0; i < _upgradeList.Count; i++)
            _check[i] = false;

        for (int i = 0; i < _sellectCount;)
        {
            int rand = Random.Range(0, _upgradeList.Count);
            if (_check[rand])
                continue;

            tryGetValue = playerManager._upgradeDic.TryGetValue(_upgradeList[rand], out int curLevel);

            if (curLevel < _upgradeList[rand]._increaseStat.Count - 1 || (curLevel == 0 && _upgradeList[rand]._increaseStat.Count == 1))
            {
                _abilityUIList[i].gameObject.SetActive(true);
                _abilityUIList[i].SetValue(_upgradeList[rand], tryGetValue == false ? 0 : curLevel + 1);
            }
            else
                _abilityUIList[i].gameObject.SetActive(false);

            // 플레이어 업그레이드가 최대 업그레이드 상태면 활성화 안 함
            _check[rand] = true;
            i++;
        }

        foreach(var ui in _abilityUIList)
            if (ui.gameObject.activeSelf)
                return;

        // 만약 활성화 된 오브젝트가 없다 = 업그레이드가 불가능
        GameManager.Instance.photonView.RPC(nameof(GameManager.SetPlayerUpgrade), RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, "null");
    }

    public void SelectRandom()
    {
        float count = 0f;
        int rand = Random.Range(0, _abilityUIList.Count);

        while(true)
        {
            if (!PhotonNetwork.InRoom || count > 1000000f)
            {
                GameManager.Instance.photonView.RPC(nameof(GameManager.Instance.GameFinish), RpcTarget.All);
                return;
            }

            count++;

            if(!_abilityUIList[rand].gameObject.activeSelf)
            {
                rand = Random.Range(0, _abilityUIList.Count);
                continue;
            }

            _abilityUIList[rand].e_onClick.Invoke();
            return;
        }
    }

    public void FadeOutSelectUI()
    {
        foreach (var ui in _abilityUIList)
        {
            if (ui.gameObject.activeSelf)
            {
                ui.e_onClick.RemoveAllListeners();
                ui.FadeOut();
            }
        }
    }
}
