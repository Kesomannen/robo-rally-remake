using UnityEngine;
using UnityEngine.EventSystems;

public class SkipButton : MonoBehaviour, IPointerClickHandler {
    void Awake() {
        ShopPhase.OnNewPlayer += OnNewPlayer;
    }

    void OnDestroy() {
        ShopPhase.OnNewPlayer -= OnNewPlayer;
    }

    public void OnPointerClick(PointerEventData e) {
        ShopPhase.Instance.MakeDecision(true, null, 0);
    }
    
    void OnNewPlayer(Player player) {
        gameObject.SetActive(PlayerSystem.IsLocal(player));
    }
}