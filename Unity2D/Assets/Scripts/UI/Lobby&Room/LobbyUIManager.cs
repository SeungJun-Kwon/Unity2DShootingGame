using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance;

    public RoomUI _roomPrefab;

    [Header("Room List")]
    public RectTransform _contentPanel;
    public List<RoomUI> _rooms = new List<RoomUI>();

    [Header("Room Info")]
    public TMP_Text _roomName;
    public TMP_Text _mapName;
    public TMP_InputField _roomPwInput;
    public Image _mapImage;

    RoomInfo _curRoom;
    public RoomInfo CurRoom
    {
        get { return _curRoom; }
        set
        {
            _curRoom = value;
            if (_curRoom == null)
            {
                Init();
                return;
            }

            PhotonHashtable hash = _curRoom.CustomProperties;
            _roomName.text = hash["Name"].ToString();

            int mapIndex = int.Parse(hash["MapIndex"].ToString());
            MapData map = MapDataManager.Instance._mapList[mapIndex];
            Color color = _mapImage.color;
            color.a = 255;
            _mapImage.color = color;
            _mapImage.sprite = map._sprite;
            _mapName.text = map._name;

            _roomPwInput.text = "";
            if (hash["Password"].ToString().Length == 0 || hash["Password"].ToString() == "")
                _roomPwInput.enabled = false;
            else
                _roomPwInput.enabled = true;
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        CurRoom = null;
    }

    public void Init()
    {
        _roomName.text = "";
        _mapName.text = "";
        Color color = _mapImage.color;
        color.a = 0;
        _mapImage.color = color;
        _roomPwInput.text = "";
        _roomPwInput.enabled = false;
    }

    public void RenewalRoomList()
    {
        Dictionary<string, RoomInfo> roomDic = PhotonNetworkManager.Instance._roomDic;
        List<RoomInfo> roomList = new List<RoomInfo>();
        RoomUI tmp;

        foreach (var room in roomDic)
        {
            if(room.Value.IsOpen || room.Value.PlayerCount != 2)
                roomList.Add(room.Value);
        }

        if(_rooms.Count == 0)
        {
            foreach(var room in roomList)
            {
                tmp = Instantiate(_roomPrefab, _contentPanel);
                tmp.SetRoomInfo(room);
                _rooms.Add(tmp);
            }
        }
        else if(_rooms.Count <= roomList.Count)
        {
            int count = 0;

            for (; count < _rooms.Count; count++)
            {
                _rooms[count].gameObject.SetActive(true);
                _rooms[count].SetRoomInfo(roomList[count]);
            }

            for(; count < roomList.Count; count++)
            {
                tmp = Instantiate(_roomPrefab, _contentPanel);
                tmp.SetRoomInfo(roomList[count]);
                _rooms.Add(tmp);
            }
        }
        else
        {
            int count = 0;

            for(; count < roomList.Count; count++)
            {
                _rooms[count].gameObject.SetActive(true);
                _rooms[count].SetRoomInfo(roomList[count]);
            }

            for (; count < _rooms.Count; count++)
                _rooms[count].gameObject.SetActive(false);
        }
    }

    public void JoinRoom()
    {
        if (CurRoom == null)
            return;
        else if (CurRoom.PlayerCount == 0 || CurRoom.PlayerCount >= 2 || !CurRoom.IsOpen || !CurRoom.IsVisible)
            return;

        string pw = CurRoom.CustomProperties["Password"].ToString();

        if (pw == null || pw == "")
        {
            PhotonNetwork.JoinRoom(CurRoom.Name);
        }
        else
        {
            if (pw != _roomPwInput.text)
                return;

            PhotonNetwork.JoinRoom(CurRoom.Name);
        }
    }

    public void OpenUserInfo()
    {
        UserInfoUIManager.Instance.gameObject.SetActive(true);
        UserInfoUIManager.Instance.LoadUserInfo(PhotonNetwork.LocalPlayer.NickName);
    }
}