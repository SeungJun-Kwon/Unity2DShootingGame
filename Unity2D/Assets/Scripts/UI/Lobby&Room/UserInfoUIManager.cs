using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserInfoUIManager : MonoBehaviour
{
    public static UserInfoUIManager Instance;

    [Header("UserInfo")]

    [SerializeField] TMP_Text _userName;
    [SerializeField] Image _userProfile;
    [SerializeField] TMP_Text _userWin;
    [SerializeField] TMP_Text _userLose;
    [SerializeField] TMP_InputField _userIntro;

    [SerializeField] Button _saveButton;
    [SerializeField] Button _closeButton;

    [Header("SetUserProfile")]
    [SerializeField] ProfileImageUI _profileImage;
    [SerializeField] GameObject _setUserProfileUI;
    [SerializeField] RectTransform _contentPanel;

    [HideInInspector] public ProfileImageUI _selectedProfile;

    UserInfo _curInfo;

    Button _profileImageButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        _userProfile.gameObject.TryGetComponent(out _profileImageButton);

        for(int i = 0; i < ProfileSpriteManager.Instance._profileSprites.Count; i++)
        {
            Instantiate(_profileImage, _contentPanel).SetSprite(ProfileSpriteManager.Instance._profileSprites[i]);
        }

        _setUserProfileUI.SetActive(false);

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
        _profileImageButton.enabled = isMine;

        _curInfo = await FirebaseFirestoreManager.Instance.LoadUserInfoByNickname(nickName);

        if(_curInfo == null)
        {
            Debug.Log("유저 정보 로드 실패");
            gameObject.SetActive(false);
            return;
        }

        _userProfile.sprite = ProfileSpriteManager.Instance.GetSprite(_curInfo.Profile);
        _userName.text = _curInfo.Name;
        _userWin.text = _curInfo.Win.ToString();
        _userLose.text = _curInfo.Lose.ToString();
        _userIntro.text = _curInfo.Intro;
    }

    public void SaveUserInfo()
    {
        _curInfo.Intro = _userIntro.text;

        FirebaseFirestoreManager.Instance.UpdateUserInfo(FirebaseAuthManager.Instance._user, _curInfo);

        gameObject.SetActive(false);
    }

    public void OpenSetUserProfileUI()
    {
        _selectedProfile = null;
        _setUserProfileUI.SetActive(true);
    }

    public void CloseSetUserProfileUI()
    {
        _selectedProfile = null;
        _setUserProfileUI.SetActive(false);
    }

    public void SaveUserProfile()
    {
        if (_selectedProfile != null) {
            _curInfo.Profile = _selectedProfile._image.sprite.name;
            _userProfile.sprite = _selectedProfile._image.sprite;
        }

        if(RoomUIManager.Instance.gameObject.activeSelf)
            RoomUIManager.Instance.photonView.RPC(nameof(RoomUIManager.Instance.SetPlayerProfile), Photon.Pun.RpcTarget.All, Photon.Pun.PhotonNetwork.LocalPlayer, _curInfo.Profile);

        _setUserProfileUI.SetActive(false);
    }
}
