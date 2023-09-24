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
    // Return 값에 따른 상태
    // 0 : 오류로 인한 실패
    // 1 : 동일한 이메일 존재
    // 2 : 동일한 닉네임 존재
    // 3 : 성공
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

            // 해당 ID를 사용하는 사용자가 이미 존재하는 경우
            if (snapshot.Exists)
            {
                Debug.LogWarning("UserInfo ID already exists.");
                Task.FromResult(1);
            }
            // 해당 ID를 사용하는 사용자가 존재하지 않는 경우
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

                    // 해당 닉네임을 사용하는 사용자가 이미 존재하는 경우
                    if(snapshot.Count > 0)
                    {
                        Debug.LogWarning("UserInfo Name already exists.");
                        Task.FromResult(2);
                    }
                    // 해당 닉네임을 사용하는 사용자가 이미 존재하지 않는 경우
                    else
                    {
                        // 새로운 사용자 데이터 생성 및 저장
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
                    Debug.Log($"유저 데이터 탐색 실패 : {e.Message}");
                    return false;
                }
            }
        }
        catch (FirestoreException e)
        {
            Debug.Log($"유저 데이터 탐색 실패 : {e.Message}");
            return false;
        }
    }

    public void UpdateUserInfo(FirebaseUser user, UserInfo userInfo)
    {
        if (userInfo.Name == "")
        {
            Debug.Log("유저 정보 업데이트 실패");
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

                Debug.Log("유저 데이터 로드 성공");

                return userInfo;
            }
            else
            {
                Debug.Log("유저 없음");

                return null;
            }
        }
        catch (FirestoreException e)
        {
            Debug.Log($"유저 데이터 로드 실패 : {e.Message}");
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
            Debug.Log($"유저 데이터 로드 실패 : {e.Message}");
            return null;
        }
    }

    #endregion
}