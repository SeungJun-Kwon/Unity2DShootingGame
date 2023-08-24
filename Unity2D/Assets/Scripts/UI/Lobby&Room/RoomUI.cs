using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomUI : MonoBehaviour
{
    public RoomData _roomData;

    [SerializeField] TMP_Text _roomName;
    [SerializeField] TMP_Text _roomPlayerCount;

    Button _button;

    private void Awake()
    {
        if(_roomName == null)
            transform.Find("NameText").gameObject.TryGetComponent(out _roomName);
        if (_roomPlayerCount == null)
            transform.Find("PlayerCount").gameObject.TryGetComponent(out _roomPlayerCount);

        TryGetComponent(out _button);
    }

    public void SetRoomInfo(RoomInfo room)
    {
        _roomName.text = (string)room.CustomProperties["Name"];
        _roomPlayerCount.text = $"{room.PlayerCount} / {room.MaxPlayers}";

        _button.onClick.AddListener(() => LobbyUIManager.Instance.CurRoom = room);
    }
}
