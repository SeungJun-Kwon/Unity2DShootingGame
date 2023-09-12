using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum GameState { WIN, LOSE, DRAW }
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public List<PlayerController> _players = new List<PlayerController>();
    public PlayerController _me, _other;

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

            _me.Available = false;
            _me.photonView.RPC(nameof(_me.SetPlayerColor), RpcTarget.All, 1f, 1f, 1f, 0f);

            //_other.Available = false;
            //_other.photonView.RPC(nameof(_other.SetPlayerColor), RpcTarget.All, 1f, 1f, 1f, 0f);

            switch (_curState)
            {
                case GameState.WIN:
                    if ((bool)_me._playerManager._player.CustomProperties["Player1"])
                        GameUIController.Instance._player1Point[_playerPointDic[_me]].color = Color.red;
                    else
                        GameUIController.Instance._player2Point[_playerPointDic[_me]].color = Color.red;

                    _playerPointDic[_me]++;

                    if (_playerPointDic[_me] == _gamePoint && PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC(nameof(GameFinish), RpcTarget.All);
                        return;
                    }
                    break;
                case GameState.LOSE:
                    //if ((bool)_other._playerManager._player.CustomProperties["Player1"])
                    //    GameUIController.Instance._player1Point[_playerPointDic[_other]].color = Color.red;
                    //else
                    //    GameUIController.Instance._player2Point[_playerPointDic[_other]].color = Color.red;

                    //_playerPointDic[_other]++;

                    //if (_playerPointDic[_other] == _gamePoint && PhotonNetwork.IsMasterClient)
                    //{
                    //    photonView.RPC(nameof(GameFinish), RpcTarget.All);
                    //    return;
                    //}

                    SelectUpgrade(_me._playerManager);
                    break;
                case GameState.DRAW:
                    if ((bool)_me._playerManager._player.CustomProperties["Player1"])
                    {
                        GameUIController.Instance._player1Point[_playerPointDic[_me]].color = Color.red;
                        //GameUIController.Instance._player2Point[_playerPointDic[_other]].color = Color.red;
                    }
                    else
                    {
                        //GameUIController.Instance._player1Point[_playerPointDic[_other]].color = Color.red;
                        GameUIController.Instance._player2Point[_playerPointDic[_me]].color = Color.red;
                    }

                    _playerPointDic[_me]++;
                    //_playerPointDic[_other]++;

                    //if ((_playerPointDic[_me] == _gamePoint || _playerPointDic[_other] == _gamePoint) && PhotonNetwork.IsMasterClient)
                    if (_playerPointDic[_me] == _gamePoint && PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC(nameof(GameFinish), RpcTarget.All);
                        return;
                    }

                    SelectUpgrade(_me._playerManager);
                    break;
            }
        }
    }

    public CinemachineVirtualCamera _cmvcam;
    CinemachineBasicMultiChannelPerlin _cmvcamNoise;

    [SerializeField] Transform _player1StartPos, _player2StartPos;

    MapData _curMap;
    GameState _curState;

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
            Debug.Log("연결 실패");

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

        if (_players.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            foreach(var p in _players)
            {
                if (p.photonView.Owner.NickName == PhotonNetwork.LocalPlayer.NickName)
                    _me = p;
                else
                    _other = p;
            }

            photonView.RPC(nameof(SelectWeapon), RpcTarget.All);
        }
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
        GameUIController.Instance._messageText.gameObject.SetActive(false);

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
    public void RoundFinishEffectCorRPC() => StartCoroutine(RoundFinishEffectCor());

    IEnumerator RoundFinishEffectCor()
    {
        int hpZero = 0;
        int hpNotZero = 0;

        for(int i = 0; i < _players.Count; i++)
        {
            if (_players[i]._playerManager.CurHp > 0)
                hpNotZero++;
            else
                hpZero++;
        }

        // 둘 다 HP가 0이거나 둘 다 HP가 0 초과일 때 : 비겼을 때
        if (hpZero >= _players.Count || hpNotZero >= _players.Count)
        {
            GameUIController.Instance.EnableMessage("DRAW");
            _curState = GameState.DRAW;
        }
        // 승자가 있을 때
        else
        {
            // 승자가 로컬 플레이어인지 아닌지
            if (_me._playerManager.CurHp > 0)
            {
                GameUIController.Instance.EnableMessage("WIN!");
                _curState = GameState.WIN;
            }
            else
            {
                GameUIController.Instance.EnableMessage("LOSE...");
                _curState = GameState.LOSE;
            }
        }

        yield return new WaitForSeconds(1.5f);

        GameUIController.Instance.DisableMessage();

        yield return new WaitForSeconds(2f);

        if (PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(RoundFinish), RpcTarget.All);
    }

    [PunRPC]
    public void GameFinish()
    {
        GameUIController.Instance._gameFinishUI.gameObject.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            string winnerName = "", loserName = "";

            foreach (var p in _players)
            {
                if (_playerPointDic[p] == _gamePoint)
                    winnerName = p.photonView.Owner.NickName;
                else
                    loserName = p.photonView.Owner.NickName;
            }

            GameUIController.Instance._gameFinishUI.EnableGameFinishUI(winnerName, loserName);
        }
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

        // 랜덤 선택
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