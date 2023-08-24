using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] TMP_Text _chatText;
    [SerializeField] RectTransform _chatRect;

    public Image _itsMe;

    private void Awake()
    {
        _chatRect.gameObject.SetActive(false);
        _itsMe.gameObject.SetActive(false);
    }

    public void ShowChat(string txt)
    {
        if (!_chatRect.gameObject.activeSelf)
            _chatRect.gameObject.SetActive(true);
        else
            CancelInvoke(nameof(SetActiveFalse));

        _chatText.text = txt;

        StartCoroutine(EnableEffect());

        Invoke(nameof(SetActiveFalse), 3f);
    }

    IEnumerator EnableEffect()
    {
        _chatRect.localScale = new Vector3(0.5f, 0.5f);

        while(_chatRect.localScale.x < 1.1f)
        {
            _chatRect.localScale += new Vector3(Time.deltaTime * 2f, Time.deltaTime * 2f);

            yield return null;
        }

        while(_chatRect.localScale.x > 1f)
        {
            _chatRect.localScale -= new Vector3(Time.deltaTime, Time.deltaTime);

            yield return null;
        }

        _chatRect.localScale = Vector3.one;
    }

    public void SetActiveFalse() => _chatRect.gameObject.SetActive(false);
}
