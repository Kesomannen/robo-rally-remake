using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class CardCollectionDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [SerializeField] Pile _targetPile;
    [SerializeField] TMP_Text _cardCountText;
    [SerializeField] Sprite _normalSprite, _highlightedSprite;
    [SerializeField] Image _image;
    [SerializeField] CollectionOverlay _overlayPrefab;
    [SerializeField] bool _shuffleCollection;
    [SerializeField] string _headerText, _subTitleText;

    CardCollection _collection;
    Player _owner => NetworkSystem.LocalPlayer;

    public void OnPointerEnter(PointerEventData e) {
        _image.sprite = _highlightedSprite;
    }

    public void OnPointerExit(PointerEventData e) {
        _image.sprite = _normalSprite;
    }

    public void OnPointerClick(PointerEventData e) {
        OverlaySystem.Instance.ShowOverlay(_overlayPrefab, _headerText, _subTitleText)
                              .Init(_collection.Cards, _shuffleCollection);
    }

    void OnEnable() {
        if (_collection == null) _collection = PlayerManager.Players[0].GetCollection(_targetPile);
        Refresh();
        _collection.OnAdd += Refresh;
        _collection.OnRemove += Refresh;
    }

    void Refresh(ProgramCardData card = null, int index = default) {
        _cardCountText.text = _collection.Cards.Count.ToString();
    }
}