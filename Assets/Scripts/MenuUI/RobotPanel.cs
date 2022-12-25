using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RobotPanel : Container<RobotData>, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [SerializeField] Image _iconImage;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _descriptionText;
    [SerializeField] Selectable _selectable;
    [SerializeField] bool _interactable;
    [Space]
    [SerializeField] float _hoverScale;
    [SerializeField] float _tweenDuration;
    [SerializeField] LeanTweenType _tweenType;

    public bool Interactable {
        get => _interactable;
        set {
            _interactable = value;
            _selectable.interactable = value;
        }
    }

    Color _defaultColor;

    void Start() {
        _defaultColor = _iconImage.color;
        _scaleTargets = new[] {
            _iconImage.gameObject, _nameText.gameObject, _descriptionText.gameObject
        };
        _scaleTweenIds = new int[_scaleTargets.Length];
    }

    public void OnPointerEnter(PointerEventData e) {
        if (!Interactable) return;
        ScaleElementsTo(_hoverScale);
        FadeIconTo(Color.white);
    }

    public void OnPointerExit(PointerEventData e) {
        if (!Interactable) return;
        ScaleElementsTo(1);
        FadeIconTo(_defaultColor);
    }
    
    public void OnPointerClick(PointerEventData e) {
        if (!Interactable) return;
        LobbySystem.Instance.UpdatePlayerData(robotId: (byte) Content.GetLookupId());
    }
    
    int[] _scaleTweenIds;
    GameObject[] _scaleTargets;

    void ScaleElementsTo(float scale) {
        foreach (var id in _scaleTweenIds) {
            LeanTween.cancel(id);
        }

        for (var i = 0; i < _scaleTargets.Length; i++) {
            var target = _scaleTargets[i];
            _scaleTweenIds[i] = LeanTween
                .scale(target, Vector3.one * scale, _tweenDuration)
                .setEase(_tweenType)
                .uniqueId;
        }
    }

    int _fadeTweenId;

    void FadeIconTo(Color color) {
        LeanTween.cancel(_fadeTweenId);
        _fadeTweenId = LeanTween
            .value(_iconImage.gameObject, _iconImage.color, color, _tweenDuration)
            .setEase(_tweenType)
            .setOnUpdate(c => _iconImage.color = c)
            .uniqueId;
    }

    protected override void Serialize(RobotData data) {
        _iconImage.sprite = data.Icon;
        _nameText.text = data.Name;
        _descriptionText.text = data.Description;
    }
}