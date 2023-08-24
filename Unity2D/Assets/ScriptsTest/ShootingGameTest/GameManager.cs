using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public List<PlayerController> _players = new List<PlayerController>();

    Dictionary<PlayerController, string> _playerWeaponDic = new Dictionary<PlayerController, string>();
    Dictionary<PlayerController, int> _playerPointDic = new Dictionary<PlayerController, int>();

    int _gamePoint = 5;
    public int GamePoint
    {
        get { return _gamePoint; }
        set
        {
            _gamePoint = value;
        }
    }

    int _gameRound = 1;
    public int GameRound
    {
        get { return _gameRound; }
        set
        {
            _gameRound = value;

            foreach (var p in _players)
            {
                p.Available = false;

                if (p.FindState(State.DIE))
                {
                    if (p.photonView.Owner.NickName == PhotonNetwork.LocalPlayer.NickName)
                        SelectUpgrade(p._playerManager);
                }
                else
                {
                    if ((bool)p._playerManager._player.CustomProperties["Player1"])
                        GameUIController.Instance._player1Point[_playerPointDic[p]].color = Color.red;
                    else
                        GameUIController.Instance._player2Point[_playerPointDic[p]].color = Color.red;

                    _playerPointDic[p]++;

                    p.photonView.RPC(nameof(p.SetPlayerColor), RpcTarget.All, 1f, 1f, 1f, 0f);

                    if (_playerPointDic[p] == _gamePoint && PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC(nameof(GameFinish), RpcTarget.All);
                        return;
                    }
                }
            }
        }
    }

    public CinemachineVirtualCamera _cmvcam;
    CinemachineBasicMultiChannelPerlin _cmvcamNoise;

    [SerializeField] Transform _player1StartPos, _player2StartPos;

    MapData _curMap;

    float _timer = 10f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        if (_cmvcam == null)
            _cmvcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (_cmvcam != null)
            _cmvcamNoise = _cmvcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if(_player1StartPos == null)
            GameObject.Find("Player1StartPos").TryGetComponent(out _player1StartPos);
        if(_player2StartPos == null)
            GameObject.Find("Player2StartPos").TryGetComponent(out _player2StartPos);
    }

    private void Start()
    {
        int count = 0;
        for (; count < 1000000; count++)
        {
            if (PhotonNetwork.InRoom)
                break;
        }
        if (count >= 1000000)
            Debug.Log("���� ����");

        _curMap = MapDataManager.Instance._mapList[(int)PhotonNetwork.CurrentRoom.CustomProperties["MapIndex"]];

        PhotonNetwork.Instantiate("Prefabs/Player", Vector3.zero, Quaternion.identity).TryGetComponent(out PlayerController player);

        if (player != null)
            photonView.RPC(nameof(AddPlayer), RpcTarget.All, player.photonView.ViewID);

        _gameRound = 1;
    }

    [PunRPC]
    public void AddPlayer(int viewId)
    {
        PhotonView playerView = PhotonView.Find(viewId);

        if (playerView != null && playerView.gameObject.TryGetComponent(out PlayerController playerController))
        {
            _players.Add(playerController);

            Player[] players = PhotonNetwork.CurrentRoom.Players.Values.ToArray();

            foreach (var p in players)
            {
                if (playerController.photonView.Owner.NickName == p.NickName)
                {
                    if(playerController.photonView.IsMine)
                        playerController._playerManager.photonView.RPC(nameof(playerController._playerManager.SetPlayer), RpcTarget.All, p);

                    if ((bool)p.CustomProperties["Player1"])
                    {
                        playerController.transform.position = _player1StartPos.position;
                        GameUIController.Instance._player1Name.text = p.NickName;
                    }
                    else
                    {
                        playerController.transform.position = _player2StartPos.position;
                        GameUIController.Instance._player2Name.text = p.NickName;
                    }

                    break;
                }
            }

            playerController.gameObject.SetActive(false);
        }

        if(_players.Count == PhotonNetwork.CurrentRoom.PlayerCount)
            photonView.RPC(nameof(SelectWeapon), RpcTarget.All);
    }

    [PunRPC]
    public void SelectWeapon()
    {
        AbilitySelectManager.Instance.gameObject.SetActive(true);
        AbilitySelectManager.Instance.SelectWeapon();
    }

    [PunRPC]
    public void SelectUpgrade(PlayerManager playerManager)
    {
        AbilitySelectManager.Instance.gameObject.SetActive(true);
        AbilitySelectManager.Instance.SelectUpgrade(playerManager);
    }

    [PunRPC]
    public void SetPlayerWeapon(string playerName, string weaponName)
    {
        foreach (var p in _players)
        {
            if (p.photonView.Owner.NickName == playerName)
            {
                _playerWeaponDic[p] = weaponName;
                _playerPointDic[p] = 0;
                StopCoroutine(nameof(TimerCor));
                break;
            }
        }

        if (_playerWeaponDic.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            AbilitySelectManager.Instance.gameObject.SetActive(false);

            for (int i = 0; i < _playerWeaponDic.Count; i++)
            {
                _players[i].gameObject.SetActive(true);
                _players[i]._playerManager.photonView.RPC("SetWeapon", RpcTarget.All, _playerWeaponDic[_players[i]]);
            }

            if(PhotonNetwork.IsMasterClient)
                photonView.RPC(nameof(RoundStart), RpcTarget.All);
        }
    }

    [PunRPC]
    public void SetPlayerUpgrade(string playerName, string upgradeName)
    {
        foreach (var p in _players)
        {
            if (p.photonView.Owner.NickName == playerName)
            {
                if(upgradeName != "null" && PhotonNetwork.IsMasterClient)
                    p._playerManager.photonView.RPC(nameof(p._playerManager.AddUpgrade), RpcTarget.All, upgradeName);

                AbilitySelectManager.Instance.gameObject.SetActive(false);

                if (PhotonNetwork.IsMasterClient)
                    photonView.RPC(nameof(RoundStart), RpcTarget.All);

                return;
            }
        }
    }

    [PunRPC]
    void StopAllCoroutinesRPC() => StopAllCoroutines();

    [PunRPC]
    public void ShakeCMVCameraRPC(float intensity, float time) => StartCoroutine(ShakeCMVCamera(intensity, time));

    public IEnumerator ShakeCMVCamera(float intensity, float time)
    {
        _cmvcamNoise.m_AmplitudeGain = intensity;

        while(time > 0f)
        {
            time -= Time.deltaTime;

            yield return null;
        }

        _cmvcamNoise.m_AmplitudeGain = 0f;
    }

    [PunRPC]
    public void RoundStart()
    {
        _cmvcam.m_Lens.OrthographicSize = _curMap._lensOrthoSize;

        foreach (var p in _players)
        {
            p._playerManager.CurHp = p._playerManager._hp;
            p.Available = true;
            p.photonView.RPC(nameof(p.ExitState), RpcTarget.All, State.DIE);
            p.photonView.RPC(nameof(p.EnterState), RpcTarget.All, State.IDLE);
            if ((bool)p._playerManager._player.CustomProperties["Player1"])
                p.transform.position = _player1StartPos.position;
            else
                p.transform.position = _player2StartPos.position;
        }
    }

    [PunRPC]
    public void RoundFinish() => GameRound += 1;

    [PunRPC]
    public void GameFinish()
    {
        if(PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    void SetTimer(float value, float standardTime)
    {
        _timer = value;

        if (GameUIController.Instance != null)
            GameUIController.Instance._timerFill.fillAmount = (standardTime - value) / standardTime;
    }

    [PunRPC]
    public void TimerCorRPC(float time)
    {
        GameUIController.Instance._timerImage.gameObject.SetActive(true);
        GameUIController.Instance._timerFill.fillAmount = 0f;
        StartCoroutine(TimerCor(time));
    }

    [PunRPC]
    public void TimerCorStopRPC()
    {
        GameUIController.Instance._timerImage.gameObject.SetActive(false);
        StopCoroutine(nameof(TimerCor));
    }

    IEnumerator TimerCor(float time)
    {
        _timer = time;

        while (_timer > 0f)
        {
            yield return new WaitForSeconds(1f);

            if (PhotonNetwork.IsMasterClient)
                photonView.RPC(nameof(SetTimer), RpcTarget.All, _timer - 1, time);

            if (_timer <= 5f)
            {

            }
        }

        // ���� ����
        if (AbilitySelectManager.Instance.gameObject.activeSelf)
            AbilitySelectManager.Instance.SelectRandom();

        GameUIController.Instance._timerImage.gameObject.SetActive(false);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        photonView.RPC(nameof(GameFinish), RpcTarget.All);
    }
}