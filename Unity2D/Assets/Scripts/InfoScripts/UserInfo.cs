using Firebase.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

[Serializable]
// 클래스를 파이어스토어에 매핑하기 위해 사용한다
[FirestoreData]
public class UserInfo
{
    // 변수를 파이어스토어에 매핑하기 위해 사용한다
    // 파이어스토어의 필드명과 실제 변수명이 다를 경우 매핑할 때 에러가 발생하므로 파이어스토어의 필드 명을 입력해준다
    [FirestoreProperty("name")]
    public string Name { get; set; }

    [FirestoreProperty("profile")]
    public string Profile { get; set; }

    [FirestoreProperty("intro")]
    public string Intro { get; set; }

    [FirestoreProperty("win")]
    public int Win { get; set; }

    [FirestoreProperty("lose")]
    public int Lose { get; set; }

    public UserInfo()
    {
        Name = "";
        Profile = "";
        Intro = "";
        Win = 0;
        Lose = 0;
    }

    public UserInfo(string name) : this()
    {
        Name = name;
        Profile = "E1";
    }

    public override string ToString()
    {
        return $"Name: {Name}, Lv: {Profile}, Win: {Win}, Lose: {Lose}\nIntro: {Intro}";
    }
}