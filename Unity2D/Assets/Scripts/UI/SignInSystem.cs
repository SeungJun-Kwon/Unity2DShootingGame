using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SignInSystem : MonoBehaviour
{
    [SerializeField] TMP_InputField _idInput, _pwInput;
    [SerializeField] GameObject _signUpPanel;
    string _id, _pw;

    private void Start()
    {
        FirebaseAuthManager.Instance.Init();
        FirebaseFirestoreManager.Instance.Init();
    }

    bool Check()
    {
        _id = _idInput.text.Trim();
        _pw = _pwInput.text.Trim();

        if (_id == "" || _pw == "")
            return false;

        return true;
    }

    public async void SignIn()
    {
        UserInfo userInfo = null;
        bool result;

        if (!Check())
        {
            if (NoticeMessageController.Instance.gameObject.activeSelf)
                NoticeMessageController.Instance.gameObject.SetActive(false);
            NoticeMessageController.Instance.gameObject.SetActive(true);
            NoticeMessageController.Instance.SetText("입력되지 않은 칸이 있습니다.");

            return;
        }

        result = await FirebaseAuthManager.Instance.SignIn(_id, _pw);

        if (FirebaseAuthManager.Instance._user != null)
            userInfo = await FirebaseFirestoreManager.Instance.LoadUserInfoByEmail(FirebaseAuthManager.Instance._user.Email);

        if (NoticeMessageController.Instance.gameObject.activeSelf)
            NoticeMessageController.Instance.gameObject.SetActive(false);
        NoticeMessageController.Instance.gameObject.SetActive(true);

        if (!result || userInfo == null)
        {
            NoticeMessageController.Instance.SetText("로그인 실패");

            if (FirebaseAuthManager.Instance._user != null)
                FirebaseAuthManager.Instance.SignOut();
        }
        else
        {
            NoticeMessageController.Instance.SetText("로그인 성공!");

            Photon.Pun.PhotonNetwork.LocalPlayer.NickName = userInfo.Name;

            SceneManager.LoadScene("LobbyScene");
        }
    }
}