using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct IncreaseStat
{
    public int _damage;
    public float _attackSpeed;
    public int _multiShot;
    public bool _doubleJump;

    public static IncreaseStat operator +(IncreaseStat stat1, IncreaseStat stat2)
    {
        if (stat1._damage < stat2._damage)
            stat1._damage = stat2._damage;
        if (stat1._attackSpeed < stat2._attackSpeed)
            stat1._attackSpeed = stat2._attackSpeed;
        if (stat1._multiShot < stat2._multiShot)
            stat1._multiShot = stat2._multiShot;
        if (!stat1._doubleJump && stat2._doubleJump)
            stat1._doubleJump = stat2._doubleJump;

        return stat1;
    }

    public override string ToString()
    {
        return $"Damage : {_damage}, AttackSpeed : {_attackSpeed}, MultiShot : {_multiShot}";
    }
}

[CreateAssetMenu(fileName = "Upgrade", menuName = "Upgrade/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    public string _name;
    public string _desc;
    public Sprite _icon;

    public List<IncreaseStat> _increaseStat;
}
