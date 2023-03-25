using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class CollectionOverlay : Overlay {
    [FormerlySerializedAs("_cardContainer")]
    [SerializeField] Transform _itemParent;
    [SerializeField] DynamicUITween _onEnableTween;

    readonly List<Transform> _objects = new();

    public void Init<T>(Container<T> prefab, IEnumerable<T> contents, bool shuffledView = false) {
        if (contents == null) return;
        
        var contentList = contents.Where(x => x != null).ToList();
        if (shuffledView) contentList.Shuffle();
        
        foreach (var container in contentList.Select(card => Instantiate(prefab, _itemParent).SetContent(card))) {
            _objects.Add(container.transform);
            container.gameObject.SetActive(false);
        }
        StartCoroutine(TweenHelper.DoUITween(_onEnableTween.ToTween(_objects.Select(c => c.gameObject))));
    }
}