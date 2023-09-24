using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoticeMessageController : MonoBehaviour
{
    public static NoticeMessageController Instance;

    [SerializeField] TMP_Text _text;

    CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        TryGetComponent(out _canvasGroup);

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        _canvasGroup.alpha = 1f;
        StartCoroutine(FadeOut());
    }

    private void OnDisable()
    {
        StopCoroutine(FadeOut());
    }

    public void SetText(string text) => _text.text = text;

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(0.5f);

        while(_canvasGroup.alpha > 0f)
        {
            yield return null;

            _canvasGroup.alpha -= Time.deltaTime;
        }

        gameObject.SetActive(false);
    }
}
