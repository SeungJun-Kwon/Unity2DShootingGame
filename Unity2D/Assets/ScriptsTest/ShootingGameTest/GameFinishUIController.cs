using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameFinishUIController : MonoBehaviour
{
    [SerializeField] Image _winnerImage;
    [SerializeField] TMP_Text _winnerName;
    [SerializeField] TMP_Text _winnerScore;
    [SerializeField] Image _loserImage;
    [SerializeField] TMP_Text _loserName;
    [SerializeField] TMP_Text _loserScore;

    CanvasGroup _gameFinishUICanvasGroup;

    private void Awake()
    {
        gameObject.TryGetComponent(out _gameFinishUICanvasGroup);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        _gameFinishUICanvasGroup.alpha = 0f;
    }

    public async void EnableGameFinishUI(string winnerName, string loserName)
    {
        UserInfo winner, loser;

        GameUIController.Instance._playerUI.gameObject.SetActive(false);
        AbilitySelectManager.Instance.gameObject.SetActive(false);
        GameUIController.Instance._timerImage.gameObject.SetActive(false);

        winner = await FirebaseFirestoreManager.Instance.LoadUserInfoByNickname(winnerName);
        loser = await FirebaseFirestoreManager.Instance.LoadUserInfoByNickname(loserName);

        StartCoroutine(FadeInGameFinishUICor());

        _winnerName.text = winner.Name;
        _winnerScore.text = $"W : {winner.Win + 1} / L : {winner.Lose}";

        _loserName.text = loser.Name;
        _loserScore.text = $"W : {loser.Win} / L : {loser.Lose + 1}";
    }

    IEnumerator FadeInGameFinishUICor()
    {
        float count = 0f;

        while (count < 1f)
        {
            count += Time.deltaTime;

            _gameFinishUICanvasGroup.alpha += count;

            yield return null;
        }

        _gameFinishUICanvasGroup.alpha = 1f;
    }
}
