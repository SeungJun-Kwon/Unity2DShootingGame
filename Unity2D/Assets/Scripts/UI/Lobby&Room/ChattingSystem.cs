using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChattingSystem : MonoBehaviourPunCallbacks
{
    public RectTransform _chatView;
    public TMP_InputField _chatInput;

    public ChatText _chatPrefab;

    private new void OnEnable()
    {
        base.OnEnable();

        for (int i = 0; i < _chatView.childCount; i++)
            Destroy(_chatView.GetChild(i).gameObject);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            EnterChat();
        }
    }

    [PunRPC]
    public void ChatInput(string nickName, string chat)
    {
        ChatText chatText = Instantiate<ChatText>(_chatPrefab, _chatView);
        chatText.SetChat($"{nickName} : {chat}");
    }

    [PunRPC]
    public void NoticeInput(string text)
    {
        ChatText chatText = Instantiate<ChatText>(_chatPrefab, _chatView);
        chatText.SetChat(text, true);
    }

    public void EnterChat()
    {
        string chat = _chatInput.text;
        chat.Replace(" ", "");
        if (chat != "")
            photonView.RPC("ChatInput", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, chat);

        _chatInput.text = "";
    }
}
