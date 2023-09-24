using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class FirebaseAuthManager
{
    private static FirebaseAuthManager instance = null;
    public static FirebaseAuthManager Instance
    {
        get
        {
            if(instance == null)
                instance = new FirebaseAuthManager();

            return instance;
        }
    }

    public FirebaseUser _user = null;     // ������ �Ϸ�� ���� ����

    FirebaseAuth _auth;     // �α���, ȸ������ � ���

    public void Init()
    {
        _auth = FirebaseAuth.DefaultInstance;
    }

    public bool IsEmailValid(string email)
    {
        // �̸��� �ּ� ������ �����մϴ�.
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        // �̸��� �ּҰ� ���ϰ� ��ġ�ϴ��� Ȯ���մϴ�.
        return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
    }

    public async Task<bool> SignUp(string email, string pw, UserInfo userInfo)
    {
        FirebaseUser user = null;

        bool result = false;

        // User�� ã�Ƽ� �ش� �̸����̳� �г����� ���� ���� ��� ���� ����
        var findUser = await FirebaseFirestoreManager.Instance.FindUser(email, userInfo.Name);

        if (findUser || !IsEmailValid(email))
            return false;

        await _auth.CreateUserWithEmailAndPasswordAsync(email, pw).ContinueWith(async task => 
        {
            if (task.IsCanceled)
            {
                Debug.Log("ȸ������ ���");
                return;
            }
            else if (task.IsFaulted)
            {
                // ���� ���� => �̸����� ������, ��й�ȣ�� �ʹ� ����, �̹� ���Ե� �̸��� ���
                Debug.Log("ȸ������ ���� : " + task.Exception);
                return;
            }

            user = task.Result;

            await FirebaseFirestoreManager.Instance.CreateUser(email, userInfo);

            Debug.Log("ȸ������ �Ϸ�");

            result = true;
        });

        await Task.Delay(1500);

        return result;
    }

    public async Task<bool> SignIn(string email, string pw)
    {
        bool result = false;

        await _auth.SignInWithEmailAndPasswordAsync(email, pw).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.Log("�α��� ���");
                return;
            }
            else if (task.IsFaulted)
            {
                // ���� ���� => �̸����� ������, ��й�ȣ�� �ʹ� ����, �̹� ���Ե� �̸��� ���
                Debug.Log("�α��� ���� : " + task.Exception);
                return;
            }

            _user = task.Result;

            Debug.Log("�α��� ����");

            result = true;
        });

        await Task.Delay(1500);

        return result;
    }

    public void SignOut()
    {
        _auth.SignOut();
        _user = null;
    }
}
