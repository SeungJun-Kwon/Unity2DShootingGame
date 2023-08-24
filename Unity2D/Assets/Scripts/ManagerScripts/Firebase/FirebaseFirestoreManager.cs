using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using Firebase;

public class FirebaseFirestoreManager
{
    private static FirebaseFirestoreManager instance;
    public static FirebaseFirestoreManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FirebaseFirestoreManager();
                instance.Init();
            }

            return instance;
        }
    }

    private FirebaseFirestore _userStore = null;

    string _userInfo = "userInfo";

    public void Init()
    {
        _userStore = FirebaseFirestore.DefaultInstance;
    }

    #region User
    public void CreateUser(string userEmail, UserInfo userInfo)
    {
        _userStore.Collection(_userInfo).Document(userEmail).GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to read data: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;

            // �ش� ID�� ����ϴ� ����ڰ� �̹� �����ϴ� ���
            if (snapshot.Exists)
            {
                Debug.LogWarning("UserInfo ID already exists.");
            }
            // �ش� ID�� ����ϴ� ����ڰ� �������� �ʴ� ���
            else
            {
                // ���ο� ����� ������ ���� �� ����
                _userStore.Collection(_userInfo).Document(userEmail).SetAsync(userInfo);

                Debug.Log("New user added.");
            }
        });
    }

    public void UpdateUserInfo(FirebaseUser user, UserInfo userInfo)
    {
        if (userInfo.Name == "")
        {
            Debug.Log("���� ���� ������Ʈ ����");
            return;
        }

        _userStore.Collection(_userInfo).Document(user.Email).GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Failed to save user : " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    string key = user.Email;
                    _userStore.Collection(_userInfo).Document(key).SetAsync(userInfo);
                    Debug.Log("User Data Updated");
                }
            }
        });
    }

    public async Task<UserInfo> LoadUserInfo(string email)
    {
        try
        {
            var result = await _userStore.Collection(_userInfo).Document(email).GetSnapshotAsync();
            if (result.Exists)
            {
                UserInfo userInfo = result.ConvertTo<UserInfo>();

                return userInfo;
            }
            else
                return null;
        }
        catch (FirestoreException e)
        {
            Debug.Log($"���� ������ �ε� ���� : {e.Message}");
            return null;
        }
    }
    #endregion
}