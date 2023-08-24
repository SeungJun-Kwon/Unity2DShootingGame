using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatText : MonoBehaviour
{
    public TMP_Text _chat;

    private void Awake()
    {
        if (_chat == null)
            gameObject.TryGetComponent(out _chat);
    }

    public void SetChat(string chat, bool isNotice = false)
    {
        _chat.text = chat;

        if (isNotice)
            _chat.color = Color.yellow;
    }
}
