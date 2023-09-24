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

    public bool IsEmailValid(string email)
    {
        // 이메일 주소 패턴을 정의합니다.
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        // 이메일 주소가 패턴과 일치하는지 확인합니다.
        return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
    }

    public async Task<bool> SignUp(string email, string pw, UserInfo userInfo)
    {
        FirebaseUser user = null;

        bool result = false;

        // User를 찾아서 해당 이메일이나 닉네임을 갖고 있을 경우 가입 실패
        var findUser = await FirebaseFirestoreManager.Instance.FindUser(email, userInfo.Name);

        if (findUser || !IsEmailValid(email))
            return false;

        await _auth.CreateUserWithEmailAndPasswordAsync(email, pw).ContinueWith(async task => 
        {
            if (task.IsCanceled)
            {
                Debug.Log("회원가입 취소");
                return;
            }
            else if (task.IsFaulted)
            {
                // 실패 이유 => 이메일이 비정상, 비밀번호가 너무 간단, 이미 가입된 이메일 등등
                Debug.Log("회원가입 실패 : " + task.Exception);
                return;
            }

            user = task.Result;

            await FirebaseFirestoreManager.Instance.CreateUser(email, userInfo);

            Debug.Log("회원가입 완료");

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

            Debug.Log("로그인 성공");

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
