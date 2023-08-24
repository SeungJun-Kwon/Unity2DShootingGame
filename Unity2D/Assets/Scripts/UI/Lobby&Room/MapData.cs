using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "MapData")]
public class MapData : ScriptableObject
{
    public string _name;
    public string _sceneName;
    public float _lensOrthoSize;

    public Sprite _sprite;
}
