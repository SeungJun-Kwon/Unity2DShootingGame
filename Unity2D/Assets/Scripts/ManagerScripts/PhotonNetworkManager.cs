using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks, IConnectionCallbacks
{
    public static PhotonNetworkManager Instance;

    public Dictionary<string, RoomInfo> _roomDic = new Dictionary<string, RoomInfo>();

    public UnityEvent e_OnJoinedLobby, e_OnJoinedRoom, e_OnLeftLobby, e_OnLeftRoom;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        DontDestroyOnLoad(this);

        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("������ ����");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("�κ� ����");

        e_OnJoinedLobby.Invoke();

        if(LobbyUIManager.Instance != null)
            LobbyUIManager.Instance.gameObject.SetActive(true);
    }

    public void JoinOrCreateRoom(string roomName, RoomData _roomData)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.CustomRoomProperties = new Hashtable() { { "Name", _roomData._roomName }, { "Password", _roomData._roomPw }, { "MapIndex", _roomData._mapIndex }, };

        // �κ񿡼� Room�� �ƴ϶� RoomInfo�� �����ؾ� �ϴµ� RoomInfo������ CustomProperty�� ���ų� ������ �� ����.
        // �׷��� �ش� �ɼ��� �߰��� �κ񿡼� �� �� �ִ� Ŀ���� ������Ƽ�� Ű�� �����Ѵ�.
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "Name", "Password", "MapIndex", };
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, null);
    }

    public override void OnJoinedRoom()
    {
        e_OnJoinedRoom.Invoke();

        if(RoomUIManager.Instance != null)
            RoomUIManager.Instance.gameObject.SetActive(true);

        if(LobbyUIManager.Instance != null)
            LobbyUIManager.Instance.gameObject.SetActive(false);
    }

    public override void OnLeftRoom()
    {
        if(SceneManager.GetActiveScene().name != "LobbyScene")
            PhotonNetwork.LoadLevel("LobbyScene");

        if (RoomUIManager.Instance != null)
            RoomUIManager.Instance.gameObject.SetActive(false);

        if (LobbyUIManager.Instance != null)
            LobbyUIManager.Instance.gameObject.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(var room in roomList)
        {
            if (room.RemovedFromList || !room.IsOpen)
            {
                _roomDic.Remove(room.Name);
            }
            else
            {
                if(_roomDic.TryGetValue(room.Name, out var value))
                    _roomDic[room.Name] = room;
                else
                    _roomDic.Add(room.Name, room);
            }
        }

        if (LobbyUIManager.Instance != null)
            LobbyUIManager.Instance.RenewalRoomList();
    }
}
