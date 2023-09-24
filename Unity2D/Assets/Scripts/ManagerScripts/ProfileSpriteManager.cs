using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileSpriteManager
{
    static ProfileSpriteManager _instance;
    public static ProfileSpriteManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ProfileSpriteManager();
                _instance.Init();
            }

            return _instance;
        }
    }

    public List<Sprite> _profileSprites = new();

    void Init()
    {
        var list = Resources.LoadAll<Sprite>("Sprites/ProfileSprites");

        foreach (var s in list)
            _profileSprites.Add(s);
    }

    public Sprite GetSprite(string name)
    {
        foreach(var s in _profileSprites)
        {
            if (name == s.name)
                return s;
        }

        return null;
    }
}
