using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum GameState { WIN, LOSE, DRAW }
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public List<PlayerController> _players = new List<PlayerController>();
    public PlayerController _me, _other;

    Dictionary<PlayerController, string> _playerWeaponDic = new Dictionary<PlayerController, string>();
    Dictionary<PlayerController, int> _playerPointDic = new Dictionary<PlayerController, int>();

    public UnityEvent e_TimerOver = new();

    int _gamePoint = 5;

    int _gameRound = 1;
    public int GameRound
    {
        get { return _gameRound; }
        set
        {
            _gameRound = value;

            _me.Available = false;
            _me.photonView.RPC(nameof(_me.SetPlayerColor), RpcTarget.All, 1f, 1f, 1f, 0f);

            _other.Available = false;
            _other.photonView.RPC(nameof(_other.SetPlayerColor), RpcTarget.All, 1f, 1f, 1f, 0f);

            switch (_curState)
            {
                case GameState.WIN:
                    if ((bool)_me._playerManager._player.CustomProperties["Player1"] && _playerPointDic[_me] < GameUIController.Instance._player1Point.Count)
                        GameUIController.Instance._player1Point[_playerPointDic[_me]].color = Color.red;
                    else if((bool)_me._playerManager._player.CustomProperties["Player2"] && _playerPointDic[_me] < GameUIController.Instance._player2Point.Count)
                        GameUIController.Instance._player2Point[_playerPointDic[_me]].color = Color.red;

                    if(_playerPointDic[_me] < _gamePoint)
                        _playerPointDic[_me]++;

                    if (_playerPointDic[_me] == _gamePoint)
                    {
                        if (PhotonNetwork.IsMasterClient)
                            photonView.RPC(nameof(GameFinish), RpcTarget.All);

                        break;
                    }

                    SelectUpgrade(_me._playerManager);
                    break;
                case GameState.LOSE:
                    if ((bool)_other._playerManager._player.CustomProperties["Player1"] && _playerPointDic[_other] < GameUIController.Instance._player1Point.Count)
                        GameUIController.Instance._player1Point[_playerPointDic[_other]].color = Color.red;
                    else if((bool)_other._playerManager._player.CustomProperties["Player2"] && _playerPointDic[_other] < GameUIController.Instance._player2Point.Count)
                        GameUIController.Instance._player2Point[_playerPointDic[_other]].color = Color.red;

                    if (_playerPointDic[_other] < _gamePoint)
                        _playerPointDic[_other]++;

                    if (_playerPointDic[_other] == _gamePoint)
                    {
                        if (PhotonNetwork.IsMasterClient)
                            photonView.RPC(nameof(GameFinish), RpcTarget.All);

                        break;
                    }

                    SelectUpgrade(_me._playerManager);
                    break;
                case GameState.DRAW:
                    if ((bool)_me._playerManager._player.CustomProperties["Player1"])
                    {
                        if(_playerPointDic[_me] < GameUIController.Instance._player1Point.Count)
                            GameUIController.Instance._player1Point[_playerPointDic[_me]].color = Color.red;

                        if(_playerPointDic[_other] < GameUIController.Instance._player2Point.Count)
                            GameUIController.Instance._player2Point[_playerPointDic[_other]].color = Color.red;
                    }
                    else
                    {
                        if(_playerPointDic[_other] < GameUIController.Instance._player1Point.Count)
                            GameUIController.Instance._player1Point[_playerPointDic[_other]].color = Color.red;

                        if(_playerPointDic[_me] < GameUIController.Instance._player2Point.Count)
                            GameUIController.Instance._player2Point[_playerPointDic[_me]].color = Color.red;
                    }

                    if (_playerPointDic[_me] < _gamePoint)
                        _playerPointDic[_me]++;

                    if (_playerPointDic[_other] < _gamePoint)
                        _playerPointDic[_other]++;

                    if (_playerPointDic[_me] == _gamePoint || _playerPointDic[_other] == _gamePoint)
                    {
                        if (PhotonNetwork.IsMasterClient)
                            photonView.RPC(nameof(GameFinish), RpcTarget.All);

                        break;
                    }

                    SelectUpgrade(_me._playerManager);
                    break;
            }
        }
    }

    public CinemachineVirtualCamera _cmvcam;
    CinemachineBasicMultiChannelPerlin _cmvcamNoise;

    public Transform _player1StartPos, _player2StartPos;

    MapData _curMap;
    public GameState _curState;

    float _timer = 0f;
    public float Timer
    {
        get { return _timer; }
    }

    float _upgradeTime = 10f;
    float _roundTime = 60f;

    int _upgradeCount = 0;

    [HideInInspector] public bool _isTimerRunning = false;

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

        // Test
        // PhotonNetwork.LocalPlayer.NickName = GeneratePassword(5);
    }

    private const string PASSWORD_CHARS = "0123456789abcdefghijklmnopqrstuvwxyz";

    public static string GeneratePassword(int length)
    {
        var sb = new System.Text.StringBuilder(length);
        var r = new System.Random();

        for (int i = 0; i < length; i++)
        {
            int pos = r.Next(PASSWORD_CHARS.Length);
            char c = PASSWORD_CHARS[pos];
            sb.Append(c);
        }

        return sb.ToString();
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

        PhotonNetwork.Instantiate("Prefabs/Players/Player", Vector3.zero, Quaternion.identity).TryGetComponent(out PlayerController player);

        if (player != null)
            photonView.RPC(nameof(AddPlayer), RpcTarget.AllBuffered, player.photonView.ViewID);

        _gameRound = 1;
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && _isTimerRunning)
            UpdateTimer();
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

            SelectWeapon();
        }
    }

    public void SelectWeapon()
    {
        AbilitySelectManager.Instance.gameObject.SetActive(true);
        AbilitySelectManager.Instance.SelectWeapon();

        if (PhotonNetwork.IsMasterClient)
        {
            StartTimer(_upgradeTime);
            e_TimerOver.AddListener(() => photonView.RPC(nameof(SelectRandomAbility), RpcTarget.All));
        }
    }

    public void SelectUpgrade(PlayerManager playerManager)
    {
        if (_curState == GameState.LOSE || _curState == GameState.DRAW)
        {
            AbilitySelectManager.Instance.gameObject.SetActive(true);
            AbilitySelectManager.Instance.SelectUpgrade(playerManager);
        }
        else
            GameUIController.Instance._waitPanel.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            StartTimer(_upgradeTime);
            Debug.Log("SelectRandomAbility 리스너 추가");
            e_TimerOver.AddListener(() => photonView.RPC(nameof(SelectRandomAbility), RpcTarget.All));
        }
    }

    [PunRPC]
    public void SelectRandomAbility()
    {
        if (!AbilitySelectManager.Instance.gameObject.activeSelf)
            return;

        AbilitySelectManager.Instance.SelectRandom();
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
                break;
            }
        }

        if (_playerWeaponDic.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            AbilitySelectManager.Instance.gameObject.SetActive(false);

            foreach (var p in _players)
            {
                p.gameObject.SetActive(true);
                p._playerManager.photonView.RPC(nameof(p._playerManager.SetWeapon), RpcTarget.All, _playerWeaponDic[p]);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                e_TimerOver.RemoveAllListeners();
                StopTimer();
                photonView.RPC(nameof(RoundStart), RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void SetPlayerUpgrade(string playerName, string upgradeName)
    {
        foreach (var p in _players)
        {
            if (p.photonView.Owner.NickName == playerName)
            {
                if (upgradeName != "null" && PhotonNetwork.IsMasterClient)
                {
                    p._playerManager.photonView.RPC(nameof(p._playerManager.AddUpgrade), RpcTarget.All, upgradeName);
                    _upgradeCount += 1;
                }

                if(AbilitySelectManager.Instance.gameObject.activeSelf)
                    AbilitySelectManager.Instance.gameObject.SetActive(false);

                if (GameUIController.Instance._waitPanel.activeSelf)
                    GameUIController.Instance._waitPanel.SetActive(false);

                if (PhotonNetwork.IsMasterClient)
                {
                    // 비겼을 때는 두 명이 업그레이드를 해야됨.
                    // 비기지 않았을 경우 패자만 업그레이드를 함
                    if ((_curState == GameState.DRAW && _upgradeCount == 2) || (_curState != GameState.DRAW && _upgradeCount == 1))
                    {
                        Debug.Log("리스너 지우고 StopTimer");
                        e_TimerOver.RemoveAllListeners();
                        StopTimer();
                        Debug.Log("RoundStart RPC");
                        photonView.RPC(nameof(RoundStart), RpcTarget.All);
                    }
                }
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
            p.Init();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            StartTimer(_roundTime);
            _upgradeCount = 0;
            e_TimerOver.AddListener(() => photonView.RPC(nameof(RoundFinishEffectCorRPC), RpcTarget.All));
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

            _players[i].Available = false;
        }

        // 둘 다 HP가 0이거나 둘 다 HP가 0 초과일 때 : 조건에 따라 판정
        if (hpZero >= _players.Count || hpNotZero >= _players.Count)
        {
            if (_me._playerManager.CurHp == _other._playerManager.CurHp)
            {
                GameUIController.Instance.EnableMessage("DRAW");
                _curState = GameState.DRAW;
            }
            else if(_me._playerManager.CurHp > _other._playerManager.CurHp)
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
        // 한 명만 Hp가 0
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

        e_TimerOver.RemoveAllListeners();

        if (PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(RoundFinish), RpcTarget.All);
    }

    [PunRPC]
    public void GameFinish()
    {
        GameUIController.Instance._gameFinishUI.gameObject.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            string[] winnerName = new string[2];
            string loserName = "";
            int winnerCount = 0;

            foreach(var p in _players)
            {
                if (_playerPointDic[p] == _gamePoint)
                {
                    winnerName[winnerCount] = p.photonView.Owner.NickName;
                    winnerCount++;
                }
                else
                    loserName = p.photonView.Owner.NickName;
            }

            if (winnerCount == 0)
                PhotonNetwork.LeaveRoom();
            else if (winnerCount == 1)
                GameUIController.Instance._gameFinishUI.EnableGameFinishUI(winnerName[0], loserName, false);
            else
                GameUIController.Instance._gameFinishUI.EnableGameFinishUI(winnerName[0], winnerName[1], true);
        }
    }

    [PunRPC]
    public void SetTimer(float time) => _timer = time;

    // 타이머 시작 메서드. MasterClient에서만 시작함
    public void StartTimer(float time)
    {
        if (_isTimerRunning || !PhotonNetwork.IsMasterClient)
            return;

        _timer = time;
        photonView.RPC(nameof(SetTimerActiveRPC), RpcTarget.All, true, _timer);
        photonView.RPC(nameof(SetTimer), RpcTarget.All, _timer);

        _isTimerRunning = true;
    }

    private void UpdateTimer()
    {
        if (!_isTimerRunning || !PhotonNetwork.IsMasterClient)
            return;

        _timer -= Time.deltaTime;
        photonView.RPC(nameof(SetTimer), RpcTarget.All, _timer);

        // 타이머 종료 시 이벤트 호출
        if (_timer <= 0f)
            StopTimer();
    }

    public void StopTimer()
    {
        if (!_isTimerRunning || !PhotonNetwork.IsMasterClient)
            return;

        _isTimerRunning = false;
        _timer = 0f;
        e_TimerOver.Invoke();
        photonView.RPC(nameof(SetTimerActiveRPC), RpcTarget.All, false, 0f);
        photonView.RPC(nameof(SetTimer), RpcTarget.All, _timer);
    }

    [PunRPC]
    public void SetTimerActiveRPC(bool active, float time) => GameUIController.Instance.SetTimerActive(active, time);

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (GameUIController.Instance._gameFinishUI.gameObject.activeSelf)
            return;

        _playerPointDic[_me] = _gamePoint;

        photonView.RPC(nameof(GameFinish), RpcTarget.All);
    }
}