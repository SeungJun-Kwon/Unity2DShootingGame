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

    public FirebaseUser _user = null;     // 인증이 완료된 유저 정보

    FirebaseAuth _auth;     // 로그인, 회원가입 등에 사용

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
                Debug.Log("회원가입 취소");
                return;
            }
            else if(task.IsFaulted)
            {
                // 실패 이유 => 이메일이 비정상, 비밀번호가 너무 간단, 이미 가입된 이메일 등등
                Debug.Log("회원가입 실패 : " + task.Exception);
                return;
            }

            user = task.Result;
            FirebaseFirestoreManager.Instance.CreateUser(email, userInfo);
            Debug.Log("회원가입 완료");
        });

        if (user != null)
            result = true;
        else
            result = false;
    }

    // 해당 작업이 완전히 끝난 후 이 함수를 호출한 곳에서 다음 줄을 실행하기 위해 비동기로 작업이 끝날 때 까지 기다리도록 함
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
                    Debug.Log("이메일");
                    return false;
                case 2:
                    Debug.Log("닉네임");
                    return false;
                case 3:
                    var result = await _auth.CreateUserWithEmailAndPasswordAsync(email, pw);
                    return true;
            }

            return false;
        }
        catch(FirestoreException e)
        {
            Debug.Log($"회원가입 실패 : {e.Message}");
            return false;
        }
    }

    public void SignIn(string email, string pw, ref bool result)
    {
        _auth.SignInWithEmailAndPasswordAsync(email, pw).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.Log("로그인 취소");
                return;
            }
            else if (task.IsFaulted)
            {
                // 실패 이유 => 이메일이 비정상, 비밀번호가 너무 간단, 이미 가입된 이메일 등등
                Debug.Log("로그인 실패 : " + task.Exception);
                return;
            }

            _user = task.Result;
            Debug.Log("로그인 완료");
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
                Debug.Log("유저 로드 실패");
                return false;
            }
            return true;
        }
        catch(FirebaseException e)
        {
            Debug.Log($"로그인 실패 : {e.Message}");
            return false;
        }
    }

    public void SignOut()
    {
        _auth.SignOut();
        _user = null;
    }
}
