using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Stat
{
    public int _hp;
    public int _damage;
    public float _moveSpeed;
    public float _attackSpeed;
    public int _multiShot;
    public int _multiJump;

    public static Stat operator +(Stat stat1, Stat stat2)
    {
        if (stat1._hp < stat2._hp)
            stat1._hp = stat2._hp;
        if (stat1._damage < stat2._damage)
            stat1._damage = stat2._damage;
        stat1._moveSpeed += stat2._moveSpeed;
        stat1._attackSpeed += stat2._attackSpeed;
        if (stat1._multiShot < stat2._multiShot)
            stat1._multiShot = stat2._multiShot;
        if (stat1._multiJump < stat2._multiJump)
            stat1._multiJump = stat2._multiJump;

        return stat1;
    }

    public override string ToString()
    {
        return $"Damage : {_damage}, AttackSpeed : {_attackSpeed}, MultiShot : {_multiShot}, DoubleJump : {_multiJump}";
    }
}

[CreateAssetMenu(fileName = "Upgrade", menuName = "Upgrade/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    public string _name;
    public string _desc;
    public Sprite _icon;

    public List<Stat> _increaseStat;
}
