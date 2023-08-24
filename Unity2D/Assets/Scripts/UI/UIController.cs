using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public Text _roomName;
    public PlayerUI _playerUI;
    public RectTransform _rect;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if(Instance != this)
            Destroy(gameObject);

        TryGetComponent(out _rect);
    }

    private void Update()
    {

    }

    private void OnEnable()
    {
        
    }
}
