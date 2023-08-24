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

    [SerializeField] TMP_Text _messageText;

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

    [HideInInspector] public PlayerController _playerController = null;

    Canvas _canvas;

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

        _timerImage.gameObject.SetActive(false);
        _chatInput.gameObject.SetActive(false);

        TryGetComponent(out _canvas);
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
        _canvas.worldCamera = Camera.main;
        _canvas.sortingLayerName = "UI";
    }

    private void LateUpdate()
    {
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
    

    public void ShowMessage(string text, float size = 100)
    {
        _messageText.enabled = true;
        _messageText.text = text;
        _messageText.fontSize = size;
    }
}