using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomUIManager : MonoBehaviour
{
    public static CreateRoomUIManager Instance;

    [SerializeField] TMP_InputField _roomNameInput;
    [SerializeField] TMP_InputField _roomPwInput;
    [SerializeField] Image _roomMapImage;
    [SerializeField] TMP_Text _roomMapName;

    List<MapData> _mapList = new List<MapData>();

    int _mapIndex = 0;
    int MapIndex
    {
        get { return _mapIndex; }
        set
        {
            _mapIndex = value;
            if (_mapList.Count <= 0)
                return;

            if (_mapIndex < 0)
                _mapIndex = _mapList.Count - 1;
            else if (_mapIndex >= _mapList.Count)
                _mapIndex = 0;

            _roomMapImage.sprite = _mapList[_mapIndex]._sprite;
            _roomMapName.text = _mapList[_mapIndex]._name;
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
        if (_mapList.Count <= 0)
            _mapList = MapDataManager.Instance._mapList;
    }

    public void Init()
    {
        _roomNameInput.text = "";
        _roomPwInput.text = "";

        MapIndex = 0;

        transform.SetAsLastSibling();
    }

    public void GetRoomInfo(RoomInfo room)
    {
        _roomNameInput.text = room.CustomProperties["Name"].ToString();
        _roomPwInput.text = room.CustomProperties["Password"].ToString();
        MapIndex = int.Parse(room.CustomProperties["MapIndex"].ToString());
    }

    public void ChangeMap(int direction)
    {
        // 왼쪽으로 이동
        if (direction == 0)
            MapIndex -= 1;
        // 오른쪽으로 이동
        else if (direction == 1)
            MapIndex += 1;
    }

    public void CreateRoom()
    {
        RoomData roomData = new RoomData();
        roomData._roomName = _roomNameInput.text;
        roomData._roomPw = _roomPwInput.text.Replace(" ", "");
        roomData._mapIndex = MapIndex;

        PhotonNetworkManager.Instance.JoinOrCreateRoom(PhotonNetwork.LocalPlayer.UserId, roomData);
        gameObject.SetActive(false);
    }
}

public class RoomData
{
    public string _roomName;
    public string _roomPw;
    public int _mapIndex;
}