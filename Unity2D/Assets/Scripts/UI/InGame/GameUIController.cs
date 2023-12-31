using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance;

    [SerializeField] AbilitySelectManager _abilitySelectManager;

    [Space(5)]
    public GameObject _waitPanel;

    [Space(5)]

    [Header("Player UI")]
    public GameObject _playerUI;

    public Image _player1Hp;
    public TMP_Text _player1Name;
    public List<Image> _player1Point = new();

    public Image _player2Hp;
    public TMP_Text _player2Name;
    public List<Image> _player2Point = new();

    [Header("Timer")]
    public Image _timerImage;
    public Image _timerFill;

    [Header("Chatting")]
    [SerializeField] TMP_InputField _chatInput;

    [Header("Message")]
    public TMP_Text _messageText;
    Animator _animMessage;

    [Header("GameFinishUI")]
    public GameFinishUIController _gameFinishUI;

    [HideInInspector] public PlayerController _playerController = null;

    Canvas _canvas;

    bool _isTimerActive = false;
    float _time = 0f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        if (_timerImage == null)
            transform.Find("TimerImage").gameObject.TryGetComponent(out _timerImage);
        if (_timerFill == null)
            _timerImage.transform.GetChild(0).gameObject.TryGetComponent(out _timerFill);

        _messageText.gameObject.TryGetComponent(out _animMessage);
        _messageText.gameObject.SetActive(false);
        _timerImage.gameObject.SetActive(false);
        _chatInput.gameObject.SetActive(false);

        TryGetComponent(out _canvas);
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
        _canvas.worldCamera = Camera.main;
        _canvas.sortingLayerName = "UI";
    }

    private void LateUpdate()
    {
        if(_isTimerActive)
        {
            _timerFill.fillAmount = (_time - GameManager.Instance.Timer) / _time;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_playerController == null)
            {
                foreach (var p in GameManager.Instance._players)
                {
                    if (p.photonView.Owner.NickName == PhotonNetwork.LocalPlayer.NickName)
                        _playerController = p;
                }
            }

            if (!_chatInput.gameObject.activeSelf)
            {
                _chatInput.gameObject.SetActive(true);
                _chatInput.ActivateInputField();

                _playerController.Available = false;
            }
            else
            {
                string txt = _chatInput.text.Trim();
                _chatInput.text = "";
                _chatInput.gameObject.SetActive(false);

                _playerController.Available = true;

                if (string.IsNullOrWhiteSpace(txt))
                    return;
                else
                    _playerController.photonView.RPC(nameof(_playerController.SendChatRPC), RpcTarget.All, txt);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && _chatInput.gameObject.activeSelf)
        {
            if (_playerController == null)
            {
                foreach (var p in GameManager.Instance._players)
                {
                    if (p.photonView.Owner.NickName == PhotonNetwork.LocalPlayer.NickName)
                        _playerController = p;
                }
            }

            _chatInput.text = "";
            _chatInput.gameObject.SetActive(false);

            _playerController.Available = true;
        }
    }
    
    public void EnableMessage(string text)
    {
        _messageText.gameObject.SetActive(true);
        _messageText.text = text;
    }

    public void DisableMessage() => _animMessage.SetTrigger("Disable");

    public void SetTimerActive(bool active, float time)
    {
        _timerImage.gameObject.SetActive(active);
        _isTimerActive = active;
        _time = time;
    }
}