using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserInfoUIManager : MonoBehaviour
{
    public static UserInfoUIManager Instance;

    [SerializeField] TMP_Text _userName;
    [SerializeField] Image _userProfile;
    [SerializeField] TMP_Text _userWin;
    [SerializeField] TMP_Text _userLose;
    [SerializeField] TMP_InputField _userIntro;

    [SerializeField] Button _saveButton;
    [SerializeField] Button _closeButton;

    UserInfo _curInfo;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        _userName.text = "";
        _userProfile.sprite = null;
        _userWin.text = "";
        _userLose.text = "";
        _userIntro.text = "";

        _curInfo = null;
    }

    public async void LoadUserInfo(string nickName, bool isMine = true)
    {
        _saveButton.gameObject.SetActive(isMine);
        _userIntro.readOnly = !isMine;

        _curInfo = await FirebaseFirestoreManager.Instance.LoadUserInfoByNickname(nickName);

        if(_curInfo == null)
        {
            Debug.Log("유저 정보 로드 실패");
            gameObject.SetActive(false);
            return;
        }

        _userName.text = _curInfo.Name;
        _userWin.text = _curInfo.Win.ToString();
        _userLose.text = _curInfo.Lose.ToString();
        _userIntro.text = _curInfo.Intro;
    }

    public void SaveUserInfo()
    {
        _curInfo.Intro = _userIntro.text;

        FirebaseFirestoreManager.Instance.UpdateUserInfo(FirebaseAuthManager.Instance._user, _curInfo);
    }
}
