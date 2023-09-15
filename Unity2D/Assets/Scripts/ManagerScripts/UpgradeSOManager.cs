using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSOManager : MonoBehaviour
{
    public static UpgradeSOManager Instance;

    public Dictionary<string, UpgradeSO> _upgradeDic = new Dictionary<string, UpgradeSO>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }

        UpgradeSO[] upgradeSO_Arr = Resources.LoadAll<UpgradeSO>("ScriptableObject/Upgrade");

        foreach (var upgrade in upgradeSO_Arr)
            _upgradeDic[upgrade._name] = upgrade;
    }
}
