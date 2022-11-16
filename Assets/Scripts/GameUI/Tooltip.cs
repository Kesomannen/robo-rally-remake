﻿using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Tooltip : Singleton<Tooltip> {
    [SerializeField] Image _background;
    [SerializeField] TextMeshProUGUI _headerText, _descriptionText;
    [SerializeField] LayoutElement _layoutElement;
    [SerializeField] int _characterWrapLimit;
    [SerializeField] float _fadeInSpeed;
    [SerializeField] float _popupDuration;

    bool _currentState;
    ITooltipable _currentItem;
    RectTransform _rect;
    Coroutine _fadeIn;
    int _popupId;

    public static event Action<ITooltipable> OnTooltip, OnTooltipEnd;

    protected override void Awake() {
        base.Awake();
        _rect = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    public void Show(ITooltipable item) {
        if (_currentState) return;
        _currentState = true;

        _popupId = LeanTween.delayedCall(_popupDuration, () => {
            OnTooltip?.Invoke(item);
            _currentItem = item;

            gameObject.SetActive(true);
            UpdateText(item.Header, _headerText);
            UpdateText(item.Description, _descriptionText);
            UpdatePosition();

            _layoutElement.enabled = _headerText.textInfo.characterCount > _characterWrapLimit ||
                                     _descriptionText.textInfo.characterCount > _characterWrapLimit;


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

        static void UpdateText(String str, TextMeshProUGUI target) {
            if (String.IsNullOrEmpty(str)) {
                target.gameObject.SetActive(false);
            } else {
                target.gameObject.SetActive(true);
                target.text = str;
            }
        }
    }

    public void Hide() {
        if (!_currentState) return;
        _currentState = false;

        OnTooltipEnd?.Invoke(_currentItem);
        _currentItem = null;

        gameObject.SetActive(false);

        LeanTween.cancel(_popupId);
        if (_fadeIn != null) StopCoroutine(_fadeIn);
    }

    void Update() {
        UpdatePosition();
    }

    void UpdatePosition() {
        var screenPos = Mouse.current.position.ReadValue();

        var pivotX = screenPos.x >= Screen.width / 2 ? 1 : 0;
        var pivotY = screenPos.y - _rect.sizeDelta.y < 0 ? 0 : 1;

        _rect.pivot = new Vector2(pivotX, pivotY);
        transform.position = screenPos;
    }
}