using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDataManager : MonoBehaviour
{
    public static MapDataManager Instance;

    public List<MapData> _mapList = new List<MapData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != this)
            Destroy(this);

        MapData[] mapResources = Resources.LoadAll<MapData>("ScriptableObject/MapData");
        foreach (var map in mapResources)
            _mapList.Add(map);
    }
}
