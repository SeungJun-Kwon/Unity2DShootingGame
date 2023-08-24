using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { Normal, Laser }

[CreateAssetMenu(fileName = "Weapon", menuName = "Weapon/Weapon")]
public class WeaponSO : ScriptableObject
{
    public string _name;
    public WeaponType _type;
    public string _desc;
    public Sprite _icon;

    public Sprite _bullet;
    public RuntimeAnimatorController _bulletAnim;

    public Sprite _shootEffect;
    public RuntimeAnimatorController _shootEffectAnim;

    public Material _hitEffectMat;

    public int _damage;
    public float _attackSpeed;
}
