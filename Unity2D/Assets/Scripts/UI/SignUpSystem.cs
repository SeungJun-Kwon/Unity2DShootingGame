using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpSystem : MonoBehaviour
{
    [SerializeField] TMP_InputField _nameInput, _idInput, _pwInput;
    string _name, _id, _pw;

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
            if(_result)
                gameObject.SetActive(false);
        }
    }

    bool Check()
    {
        _name = _nameInput.text.Trim();
        _id = _idInput.text.Trim();
        _pw = _pwInput.text.Trim();

        if (_name == "" || _id == "" || _pw == "")
            return false;

        return true;
    }

    public async void SignUp()
    {
        if (!Check())
        {
            print("입력되지 않은 칸이 있습니다.");
            return;
        }

        UserInfo userInfo = new UserInfo(_name);
        
        Result = await FirebaseAuthManager.Instance.SignUp(_id, _pw, userInfo);
    }
}