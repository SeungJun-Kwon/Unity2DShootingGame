using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameFinishUIController : MonoBehaviour
{
    [SerializeField] Image _user1Image;
    [SerializeField] TMP_Text _user1Name;
    [SerializeField] TMP_Text _user1Score;
    [SerializeField] Image _user2Image;
    [SerializeField] TMP_Text _user2Name;
    [SerializeField] TMP_Text _user2Score;

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

    public async void EnableGameFinishUI(string user1, string user2, bool isDraw)
    {
        UserInfo userInfo1, userInfo2;

        GameUIController.Instance._playerUI.gameObject.SetActive(false);
        AbilitySelectManager.Instance.gameObject.SetActive(false);
        GameUIController.Instance._timerImage.gameObject.SetActive(false);

        userInfo1 = await FirebaseFirestoreManager.Instance.LoadUserInfoByNickname(user1);
        userInfo2 = await FirebaseFirestoreManager.Instance.LoadUserInfoByNickname(user2);

        if (!isDraw)
        {
            userInfo1.Win += 1;
            userInfo2.Lose += 1;
        }

        StartCoroutine(FadeInGameFinishUICor());

        if (userInfo1 != null)
        {
            _user1Image.sprite = ProfileSpriteManager.Instance.GetSprite(userInfo1.Profile);
            _user1Name.text = userInfo1.Name;
            _user1Score.text = $"W : {userInfo1.Win} / L : {userInfo1.Lose}";
        }

        if (userInfo2 != null)
        {
            _user2Image.sprite = ProfileSpriteManager.Instance.GetSprite(userInfo2.Profile);
            _user2Name.text = userInfo2.Name;
            _user2Score.text = $"W : {userInfo2.Win} / L : {userInfo2.Lose}";
        }

        if(userInfo1.Name == PhotonNetwork.LocalPlayer.NickName)
            FirebaseFirestoreManager.Instance.UpdateUserInfo(FirebaseAuthManager.Instance._user, userInfo1);
        else
            FirebaseFirestoreManager.Instance.UpdateUserInfo(FirebaseAuthManager.Instance._user, userInfo2);

        Invoke(nameof(OutRoom), 5f);
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

    public void OutRoom() => PhotonNetwork.LeaveRoom();
}
