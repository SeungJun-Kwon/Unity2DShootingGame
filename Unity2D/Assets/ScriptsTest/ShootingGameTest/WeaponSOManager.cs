using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSOManager : MonoBehaviour
{
    public static WeaponSOManager Instance;

    public Dictionary<string, WeaponSO> _weaponDic = new Dictionary<string, WeaponSO>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if(Instance != this)
        {
            Destroy(this);
            return;
        }

        WeaponSO[] weaponSO_Arr = Resources.LoadAll<WeaponSO>("ScriptableObject/Weapon");

        foreach (var weapon in weaponSO_Arr)
            _weaponDic[weapon._name] = weapon;
    }
}
