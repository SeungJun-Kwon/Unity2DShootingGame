using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class InGameObjectManager : MonoBehaviour
{
    public string _gameManagerPath;
    public GameObject _gameUIController;
    public GameObject _weaponSOManager;
    public GameObject _upgradeSOManager;

    GameObject _gameManager;

    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
            _gameManager = PhotonNetwork.Instantiate(_gameManagerPath, Vector3.zero, Quaternion.identity);

        Instantiate(_gameUIController, Vector3.zero, Quaternion.identity);
        Instantiate(_weaponSOManager, Vector3.zero, Quaternion.identity);
        Instantiate(_upgradeSOManager, Vector3.zero, Quaternion.identity);
    }
}
