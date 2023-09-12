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

    public void SignUp(string email, string pw, UserInfo userInfo, ref bool result)
    {
        FirebaseUser user = null;
        _auth.CreateUserWithEmailAndPasswordAsync(email, pw).ContinueWith(task => 
        {
            if(task.IsCanceled)
            {
                Debug.Log("ȸ������ ���");
                return;
            }
            else if(task.IsFaulted)
            {
                // ���� ���� => �̸����� ������, ��й�ȣ�� �ʹ� ����, �̹� ���Ե� �̸��� ���
                Debug.Log("ȸ������ ���� : " + task.Exception);
                return;
            }

            user = task.Result;
            FirebaseFirestoreManager.Instance.CreateUser(email, userInfo);
            Debug.Log("ȸ������ �Ϸ�");
        });

        if (user != null)
            result = true;
        else
            result = false;
    }

    // �ش� �۾��� ������ ���� �� �� �Լ��� ȣ���� ������ ���� ���� �����ϱ� ���� �񵿱�� �۾��� ���� �� ���� ��ٸ����� ��
    public async Task<bool> SignUp(string email, string pw, UserInfo userInfo)
    {
        try
        {
            var createResult = await FirebaseFirestoreManager.Instance.CreateUser(email, userInfo);

            switch(createResult)
            {
                case 0:
                    return false;
                case 1:
                    Debug.Log("�̸���");
                    return false;
                case 2:
                    Debug.Log("�г���");
                    return false;
                case 3:
                    var result = await _auth.CreateUserWithEmailAndPasswordAsync(email, pw);
                    return true;
            }

            return false;
        }
        catch(FirestoreException e)
        {
            Debug.Log($"ȸ������ ���� : {e.Message}");
            return false;
        }
    }

    public void SignIn(string email, string pw, ref bool result)
    {
        _auth.SignInWithEmailAndPasswordAsync(email, pw).ContinueWith(task =>
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
            Debug.Log("�α��� �Ϸ�");
        });

        if (_user != null)
            result = true;
        else
            result = false;
    }

    public async Task<bool> SignIn(string email, string pw)
    {
        try
        {
            var result = await _auth.SignInWithEmailAndPasswordAsync(email, pw);
            _user = result;
            var userInfo = await FirebaseFirestoreManager.Instance.LoadUserInfoByEmail(_user.Email);
            if (userInfo != null)
                PhotonNetwork.LocalPlayer.NickName = userInfo.Name;
            else
            {
                Debug.Log("���� �ε� ����");
                return false;
            }
            return true;
        }
        catch(FirebaseException e)
        {
            Debug.Log($"�α��� ���� : {e.Message}");
            return false;
        }
    }

    public void SignOut()
    {
        _auth.SignOut();
        _user = null;
    }
}
