using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AbilitySelectUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent e_onClick;

    public TMP_Text _name;
    public TMP_Text _desc;
    public Image _icon;

    WeaponSO _weapon;
    public WeaponSO Weapon
    {
        get { return _weapon; }
        set
        {
            if(value == null)
            {
                Init();
                return;
            }

            _weapon = value;
            _name.text = _weapon._name;
            _desc.text = $"{_weapon._desc}\n\n" +
                $"데미지 : {_weapon._damage}\n" +
                $"연사력 : {_weapon._attackSpeed}%";

            if (_weapon._icon == null)
                _icon.color = new Color(255, 255, 255, 0);
            else
            {
                _icon.color = new Color(255, 255, 255, 255);
                _icon.sprite = _weapon._icon;
            }
        }
    }

    UpgradeSO _upgrade;
    public UpgradeSO Upgrade
    {
        get { return _upgrade; }
        set
        {
            if(value == null)
            {
                Init();
                return;
            }

            _upgrade = value;
            _name.text = _upgrade._name;
            _desc.text = _upgrade._desc;

            if (_upgrade._icon == null)
                _icon.color = new Color(255, 255, 255, 0);
            else
            {
                _icon.color = new Color(255, 255, 255, 255);
                _icon.sprite = _upgrade._icon;
            }
        }
    }

    CanvasGroup _canvasGroup;
    Button _button;
    RectTransform _rect;

    float _sizeChangingTime = 0.5f;

    private void Awake()
    {
        TryGetComponent(out _canvasGroup);
        TryGetComponent(out _button);
        TryGetComponent(out _rect);

        _button.onClick.AddListener(() => e_onClick.Invoke());
    }

    private void OnEnable()
    {
        _rect.localScale = Vector3.one;

        _button.interactable = false;

        StartCoroutine(CorFadeIn());
    }

    public void Init()
    {
        _name.text = "";
        _desc.text = "";
        _icon.color = new Color(255, 255, 255, 0);
    }

    public void SetValue(WeaponSO weapon)
    {
        Weapon = weapon;
        e_onClick.AddListener(() => GameManager.Instance.photonView.RPC(nameof(GameManager.Instance.SetPlayerWeapon), RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, Weapon._name));
    }

    public void SetValue(UpgradeSO upgrade, int curLevel = 0)
    {
        Upgrade = upgrade;
        if (curLevel == 0)
            _desc.text += $"\n\nNew!";
        else
            _desc.text += $"\n\n현재 레벨 : {curLevel}";
        e_onClick.AddListener(() => GameManager.Instance.photonView.RPC(nameof(GameManager.Instance.SetPlayerUpgrade), RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, Upgrade._name));
    }

    IEnumerator CorFadeIn()
    {
        float count = 0;

        while(count < 1f)
        {
            _canvasGroup.alpha = count;

            count += Time.fixedDeltaTime;

            yield return null;
        }

        _button.interactable = true;
    }

    public void FadeOut()
    {
        _button.interactable = false;

        StartCoroutine(CorFadeOut());
    }

    IEnumerator CorFadeOut()
    {
        float count = 1f;

        while (count > 0)
        {
            _canvasGroup.alpha = count;

            count -= Time.fixedDeltaTime;

            yield return null;
        }

        gameObject.SetActive(false);
    }

    IEnumerator CorSizeUp()
    {
        float duration = 0.5f;
        float t = 0f;

        Vector3 startScale = _rect.localScale;
        Vector3 targetScale = Vector3.one * 1.1f;

        while (t < _sizeChangingTime)
        {
            t += Time.deltaTime / duration;
            _rect.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        _rect.localScale = targetScale;
    }

    IEnumerator CorSizeDown()
    {
        float duration = 0.5f;
        float t = 0f;

        Vector3 startScale = _rect.localScale;
        Vector3 targetScale = Vector3.one;

        while (t < _sizeChangingTime)
        {
            t += Time.deltaTime / duration;
            _rect.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        _rect.localScale = targetScale;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        StopCoroutine(CorSizeDown());
        StartCoroutine(CorSizeUp());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopCoroutine(CorSizeUp());
        StartCoroutine(CorSizeDown());
    }
}
