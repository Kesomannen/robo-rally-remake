using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Tooltip : Singleton<Tooltip> {
    [Header("Tooltip")]
    [SerializeField] int _characterWrapLimit;
    [SerializeField] float _fadeInSpeed;
    [SerializeField] float _popupDuration;

    [Header("References")]
    [SerializeField] Image _background;
    [SerializeField] TextMeshProUGUI _headerText, _descriptionText;
    [SerializeField] LayoutElement _layoutElement;

    bool _state;
    RectTransform _rect;
    ITooltipable _current;

    Coroutine _fadeIn;
    int _popupId;

    protected override void Awake() {
        base.Awake();
        _rect = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    public void Show(ITooltipable item) {
        if (_state) return;
        _state = true;
        _current = item;

        _popupId = LeanTween.delayedCall(_popupDuration, () => {
            UpdateText(item.Header, _headerText);
            UpdateText(item.Description, _descriptionText);
            UpdatePosition();

            gameObject.SetActive(true);
            
            _layoutElement.enabled = item.Header.Length > _characterWrapLimit ||
                                     item.Description.Length > _characterWrapLimit;
            
            _fadeIn = StartCoroutine(FadeIn());
        }).id;

        IEnumerator FadeIn() {
            var textColor = _headerText.color;
            var backgroundColor = _background.color;

            textColor.a = 0;
            backgroundColor.a = 0;

            while (textColor.a < 1) {
                textColor.a += Time.deltaTime * _fadeInSpeed;
                backgroundColor.a += Time.deltaTime * _fadeInSpeed;

                _headerText.color = textColor;
                _descriptionText.color = textColor;
                _background.color = backgroundColor;

                yield return null;
            }
        }

        static void UpdateText(string str, TMP_Text target) {
            if (string.IsNullOrEmpty(str)) {
                target.gameObject.SetActive(false);
            } else {
                target.gameObject.SetActive(true);
                target.text = str;
            }
        }
    }

    public void Hide(ITooltipable item) {
        if (!_state || _current != item) return;
        _state = false;

        gameObject.SetActive(false);

        LeanTween.cancel(_popupId);
        if (_fadeIn != null) StopCoroutine(_fadeIn);
    }

    void Update() {
        UpdatePosition();
    }

    void UpdatePosition() {
        var screenPos = Mouse.current.position.ReadValue();

        var pivotX = screenPos.x >= Screen.width / 2f ? 1 : 0;
        var pivotY = screenPos.y - _rect.sizeDelta.y < 0 ? 0 : 1;

        _rect.pivot = new Vector2(pivotX, pivotY);
        transform.position = screenPos;
    }
}