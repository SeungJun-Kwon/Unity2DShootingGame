using Firebase.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

[Serializable]
// Ŭ������ ���̾�� �����ϱ� ���� ����Ѵ�
[FirestoreData]
public class UserInfo
{
    // ������ ���̾�� �����ϱ� ���� ����Ѵ�
    // ���̾����� �ʵ��� ���� �������� �ٸ� ��� ������ �� ������ �߻��ϹǷ� ���̾����� �ʵ� ���� �Է����ش�
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