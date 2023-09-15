using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Runtime.CompilerServices;

public class RoomUIManager : MonoBehaviourPunCallbacks
{
    public static RoomUIManager Instance;

    public TMP_Text _roomName;

    [Header("Player 1")]
    public Image _player1Image;
    public Image _player1Frame;
    public TMP_Text _player1Name;
    public TMP_Text _player1Host;

    [Header("Player 2")]
    public Image _player2Image;
    public Image _player2Frame;
    public TMP_Text _player2Name;
    public TMP_Text _player2Host;

    Player _player1, _player2;

    [Header("Map")]
    public Image _mapImage;
    public TMP_Text _mapName;

    int _mapIndex;
    public int MapIndex
    {
        get { return _mapIndex; }
        set
        {
            _mapIndex = value;
            if(_mapIndex < 0 || _mapIndex >= MapDataManager.Instance._mapList.Count)
                _mapIndex = 0;

            try
            {
                CurMap = MapDataManager.Instance._mapList[_mapIndex];
            }
            catch (Exception)
            {
                CurMap = null;    
            }
        }
    }

    MapData _curMap;
    public MapData CurMap
    {
        get { return _curMap; }
        set
        {
            _curMap = value;

            if (value == null)
            {
                photonView.RPC(nameof(LeaveRoom), RpcTarget.All);
                return;
            }

            _mapImage.sprite = CurMap._sprite;
            _mapName.text = CurMap._name;
        }
    }

    [Header("Buttons")]
    [SerializeField] Button _startButton;
    [SerializeField] Button _readyButton;
    [SerializeField] Button _exitButton;
    [SerializeField] TMP_Text _readyButtonText;

    ChattingSystem _chattingSystem;

    bool _isHost = false;
    bool IsHost
    {
        get { return _isHost; }
        set
        {
            _isHost = value;

            if (_isHost)
            {
                _startButton.gameObject.SetActive(true);
                _readyButton.gameObject.SetActive(false);
            }
            else
            {
                _startButton.gameObject.SetActive(false);
                _readyButton.gameObject.SetActive(true);
            }
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        gameObject.SetActive(false);

        if (_startButton == null)
            transform.Find("StartButton").gameObject.TryGetComponent(out _startButton);
        _startButton.onClick.AddListener(StartGame);

        if(_readyButton == null)
            transform.Find("ReadyButton").gameObject.TryGetComponent(out _readyButton);
        if (_readyButtonText == null)
            _readyButton.transform.Find("Text (TMP)").gameObject.TryGetComponent(out _readyButtonText);
        _readyButton.onClick.AddListener(Ready);

        if (_exitButton == null)
            transform.Find("ExitButton").gameObject.TryGetComponent(out _exitButton);
    }

    public override void OnEnable()
    {
        base.OnEnable();

        if(!PhotonNetwork.InRoom)
        {
            gameObject.SetActive(false);
            LobbyUIManager.Instance.gameObject.SetActive(true);
            LobbyUIManager.Instance.CurRoom = null;
            return;
        }

        _player1Frame.gameObject.SetActive(false);
        _player1Host.gameObject.SetActive(false);
        _player2Frame.gameObject.SetActive(false);
        _player2Host.gameObject.SetActive(false);

        SetButtonInteractable(true);
        _startButton.gameObject.SetActive(false);
        _readyButton.gameObject.SetActive(false);

        _roomName.text = PhotonNetwork.CurrentRoom.CustomProperties["Name"].ToString();
        MapIndex = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties["MapIndex"].ToString());

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            photonView.RPC("SetPlayer1RPC", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
            photonView.RPC("SetPlayerHost", RpcTarget.AllBuffered, _player1, true);
            IsHost = true;
        }
        else
            IsHost = false;
    }

    private void Start()
    {
        transform.Find("ChattingSystem").gameObject.TryGetComponent(out _chattingSystem);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (_player1 == null)
            photonView.RPC("SetPlayer1RPC", RpcTarget.AllBuffered, newPlayer);
        else
            photonView.RPC("SetPlayer2RPC", RpcTarget.AllBuffered, newPlayer);

        photonView.RPC("SetPlayerHost", RpcTarget.All, newPlayer, false);

        _chattingSystem.photonView.RPC("NoticeInput", RpcTarget.All, $"{newPlayer.NickName}´ÔÀÌ ÀÔÀåÇÏ¼Ì½À´Ï´Ù.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Player nullPlayer = null;

        photonView.RPC("SetPlayerHost", RpcTarget.All, otherPlayer, false);

        if (_player1 == otherPlayer)
        {
            photonView.RPC("SetPlayer1RPC", RpcTarget.All, nullPlayer);
            photonView.RPC("SetPlayerHost", RpcTarget.AllBuffered, _player2, true);
        }
        else
        {
            photonView.RPC("SetPlayer2RPC", RpcTarget.All, nullPlayer);
            photonView.RPC("SetPlayerHost", RpcTarget.AllBuffered, _player1, true);
        }

        IsHost = true;

        _chattingSystem.photonView.RPC("NoticeInput", RpcTarget.All, $"{otherPlayer.NickName}´ÔÀÌ ÅðÀåÇÏ¼Ì½À´Ï´Ù.");
    }

    [PunRPC]
    public void SetPlayer1RPC(Player player)
    {
        _player1Frame.gameObject.SetActive(false);
        _player1Host.gameObject.SetActive(false);

        if (player == null)
        {
            _player1Image.color = Color.white;
            _player1Name.text = "";
            _player1.CustomProperties["Player1"] = false;
            _player1.CustomProperties["Player2"] = false;
            _player1 = null;
        }
        else
        {
            _player1 = player;
            _player1Image.color = Color.red;
            _player1Name.text = _player1.NickName;
            _player1.CustomProperties["Player1"] = true;
            _player1.CustomProperties["Player2"] = false;
        }
    }

    [PunRPC]
    public void SetPlayer2RPC(Player player)
    {
        _player2Frame.gameObject.SetActive(false);
        _player2Host.gameObject.SetActive(false);

        if (player == null)
        {
            _player2Image.color = Color.white;
            _player2Name.text = "";
            _player2.CustomProperties["Player1"] = false;
            _player2.CustomProperties["Player2"] = false;
            _player2 = null;
        }
        else
        {
            _player2 = player;
            _player2Image.color = Color.blue;
            _player2Name.text = _player2.NickName;
            _player2.CustomProperties["Player1"] = false;
            _player2.CustomProperties["Player2"] = true;
        }
    }

    [PunRPC]
    public void SetPlayerHost(Player player, bool isHost)
    {
        if (player == null)
            return;

        player.CustomProperties["IsHost"] = isHost;
        player.CustomProperties["IsReady"] = isHost;

        if(player == _player1)
        {
            _player1Frame.gameObject.SetActive(isHost);
            _player1Host.gameObject.SetActive(isHost);
        }
        else
        {
            _player2Frame.gameObject.SetActive(isHost);
            _player2Host.gameObject.SetActive(isHost);
        }
    }

    public void Ready()
    {
        bool isReady = (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsReady"];
        if (isReady)
        {
            _readyButton.image.color = Color.white;
            _readyButtonText.text = "ÁØºñ";
        }
        else
        {
            _readyButton.image.color = Color.gray;
            _readyButtonText.text = "ÁØºñ ¿Ï·á";
        }

        photonView.RPC("SetPlayerReady", RpcTarget.All, PhotonNetwork.LocalPlayer, !isReady);
    }

    [PunRPC]
    public void SetPlayerReady(Player player, bool isReady)
    {
        player.CustomProperties["IsReady"] = isReady;

        if (player == _player1)
            _player1Frame.gameObject.SetActive(isReady);
        else
            _player2Frame.gameObject.SetActive(isReady);
    }

    public void StartGame()
    {
        if (_player1 == null || _player2 == null)
            return;

        bool player1IsReady = (bool)_player1.CustomProperties["IsReady"];
        bool player2IsReady = (bool)_player2.CustomProperties["IsReady"];

        if (!player1IsReady || !player2IsReady)
            return;

        photonView.RPC("SetButtonInteractable", RpcTarget.All, false);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        StartCoroutine(Count());
    }

    [PunRPC]
    void SetButtonInteractable(bool b)
    {
        _startButton.interactable = b;
        _readyButton.interactable = b;
        _exitButton.interactable = b;
    }

    IEnumerator Count()
    {
        int count = 5;

        while(count > 0)
        {
            _chattingSystem.photonView.RPC(nameof(_chattingSystem.NoticeInput), RpcTarget.All, count.ToString());
            count -= 1;

            yield return new WaitForSeconds(1f);
        }

        _chattingSystem.photonView.RPC(nameof(_chattingSystem.NoticeInput), RpcTarget.All, "½ÃÀÛ!");
        PhotonNetwork.LoadLevel(CurMap._sceneName);
    }

    [PunRPC]
    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public async void OpenUserInfo(int playerNum)
    {
        string nickName;

        if (playerNum == 1)
            nickName = _player1Name.text;
        else
            nickName = _player2Name.text;

        nickName = nickName.Trim();

        if (nickName == null || nickName == "")
            return;

        UserInfo info = await FirebaseFirestoreManager.Instance.LoadUserInfoByNickname(nickName);

        if (info == null)
            return;

        UserInfoUIManager.Instance.gameObject.SetActive(true);
        UserInfoUIManager.Instance.LoadUserInfo(nickName, PhotonNetwork.LocalPlayer.NickName == nickName ? true : false);
    }
}
