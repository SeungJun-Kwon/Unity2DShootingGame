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
    // Return ���� ���� ����
    // 0 : ������ ���� ����
    // 1 : ������ �̸��� ����
    // 2 : ������ �г��� ����
    // 3 : ����
    public Task<int> CreateUser(string userEmail, UserInfo userInfo)
    {
        _userStore.Collection(_userInfo).Document(userEmail).GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to read data: " + task.Exception);
                Task.FromResult(0);
            }

            DocumentSnapshot snapshot = task.Result;

            // �ش� ID�� ����ϴ� ����ڰ� �̹� �����ϴ� ���
            if (snapshot.Exists)
            {
                Debug.LogWarning("UserInfo ID already exists.");
                Task.FromResult(1);
            }
            // �ش� ID�� ����ϴ� ����ڰ� �������� �ʴ� ���
            else
            {
                _userStore.Collection(_userInfo).WhereEqualTo("name", userInfo.Name).GetSnapshotAsync().ContinueWith(task =>
                {
                    if(task.IsFaulted)
                    {
                        Debug.LogError("Failed to read data: " + task.Exception);
                        Task.FromResult(0);
                    }

                    QuerySnapshot snapshot = task.Result;

                    // �ش� �г����� ����ϴ� ����ڰ� �̹� �����ϴ� ���
                    if(snapshot.Count > 0)
                    {
                        Debug.LogWarning("UserInfo Name already exists.");
                        Task.FromResult(2);
                    }
                    // �ش� �г����� ����ϴ� ����ڰ� �̹� �������� �ʴ� ���
                    else
                    {
                        // ���ο� ����� ������ ���� �� ����
                        _userStore.Collection(_userInfo).Document(userEmail).SetAsync(userInfo);
                        Task.FromResult(3);
                        Debug.Log("New user added.");
                    }

                    return Task.CompletedTask;
                });
            }
        });

        return Task.FromResult(3);
    }

    public async Task<bool> FindUser(string email, string nickName)
    {
        try
        {
            var result = await _userStore.Collection(_userInfo).Document(email).GetSnapshotAsync();
            if (result.Exists)
                return true;
            else
            {
                try
                {
                    var query = await _userStore.Collection(_userInfo).WhereEqualTo("name", nickName).GetSnapshotAsync();

                    if (query.Count > 0)
                        return true;
                    else
                        return false;
                }
                catch (FirestoreException e)
                {
                    Debug.Log($"���� ������ Ž�� ���� : {e.Message}");
                    return false;
                }
            }
        }
        catch (FirestoreException e)
        {
            Debug.Log($"���� ������ Ž�� ���� : {e.Message}");
            return false;
        }
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

    public async Task<UserInfo> LoadUserInfoByEmail(string email)
    {
        try
        {
            var result = await _userStore.Collection(_userInfo).Document(email).GetSnapshotAsync();
            if (result.Exists)
            {
                UserInfo userInfo = result.ConvertTo<UserInfo>();

                Debug.Log("���� ������ �ε� ����");

                return userInfo;
            }
            else
            {
                Debug.Log("���� ����");

                return null;
            }
        }
        catch (FirestoreException e)
        {
            Debug.Log($"���� ������ �ε� ���� : {e.Message}");
            return null;
        }
    }

    public async Task<UserInfo> LoadUserInfoByNickname(string nickname)
    {
        try
        {
            var query = await _userStore.Collection(_userInfo).WhereEqualTo("name", nickname).GetSnapshotAsync();

            if (query.Count > 0)
            {
                foreach(var result in query)
                {
                    UserInfo userInfo = result.ConvertTo<UserInfo>();

                    return userInfo;
                }

                return null;
            }
            else
            {
                return null;
            }
        }
        catch (FirestoreException e)
        {
            Debug.Log($"���� ������ �ε� ���� : {e.Message}");
            return null;
        }
    }

    #endregion
}