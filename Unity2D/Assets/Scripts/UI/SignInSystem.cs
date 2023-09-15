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

    bool _result;
    bool Result
    {
        get
        {
            return _result;
        }
        set
        {
            _result = value;
            if(_result == true)
            {
                SceneManager.LoadScene("LobbyScene");
            }
        }
    }

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
        if (!Check())
        {
            print("입력되지 않은 칸이 있습니다.");
            return;
        }

        Result = await FirebaseAuthManager.Instance.SignIn(_id, _pw);
    }
}
