using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class CardCollectionDisplay : MonoBehaviour, IPointerClickHandler {
    [SerializeField] Pile _targetPile;
    [SerializeField] bool _shuffledView;
    [SerializeField] OverlayData<CollectionOverlay> _collectionOverlayData;
    [Space]
    [SerializeField] TMP_Text _cardCountText;

    CardCollection _collection;
    static Player Owner => PlayerManager.LocalPlayer;

    void Start() {
        _collection = Owner.GetCollection(_targetPile);

        Refresh();
        _collection.OnAdd += Refresh;
        _collection.OnRemove += Refresh;
    }

    void OnDestroy() {
        _collection.OnAdd -= Refresh;
        _collection.OnRemove -= Refresh;
    }

    void Refresh(ProgramCardData card = default, int index = default) {
        _cardCountText.text = _collection.Cards.Count.ToString();
    }

    public void OnPointerClick(PointerEventData e) {
        var obj = OverlaySystem.Instance.ShowOverlay(_collectionOverlayData);
        obj.Init(_collection.Cards, _shuffledView);
    }
}