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

    [FirestoreProperty("lv")]
    public int Lv { get; set; }

    [FirestoreProperty("win")]
    public int Win { get; set; }

    [FirestoreProperty("lose")]
    public int Lose { get; set; }

    public UserInfo()
    {
        Name = "";
        Lv = 0;
        Win = 0;
        Lose = 0;
    }

    public UserInfo(string name) : this()
    {
        Name = name;
    }

    public UserInfo(string name, int win, int lose) : this(name)
    {
        Win = win;
        Lose = lose;
    }

    public void ModifyValue(string fieldName, float value)
    {
        var field = this.GetType().GetField(fieldName);
        if (field != null)
        {
            Type fieldType = field.FieldType;
            if (fieldType == typeof(float))
            {
                float currentValue = (float)field.GetValue(this);
                field.SetValue(this, currentValue + value);
            }
            else if (fieldType == typeof(int))
            {
                int currentValue = (int)field.GetValue(this);
                field.SetValue(this, currentValue + (int)value);
            }
        }
    }

    public override string ToString()
    {
        return $"Name: {Name}, Lv: {Lv}, Win: {Win}, Lose: {Lose}";
    }
}