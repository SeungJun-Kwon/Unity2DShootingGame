using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileImageUI : MonoBehaviour
{
    [SerializeField] Button _button;

    public Image _image;

    public void SetSprite(Sprite s) => _image.sprite = s;

    public void OnClick() => UserInfoUIManager.Instance._selectedProfile = this;
}
