using UnityEngine;
using UnityEngine.EventSystems;

public class SkipButton : MonoBehaviour, IPointerClickHandler {
    void Awake() {
        ShopPhase.NewPlayer += OnNewPlayer;
    }

    void OnEnable() {
        OnNewPlayer(ShopPhase.CurrentPlayer);
    }

    void OnDestroy() {
        ShopPhase.NewPlayer -= OnNewPlayer;
    }

    public void OnPointerClick(PointerEventData e) {
        ShopPhase.Instance.MakeDecision(true, null, 0);
    }
    
    void OnNewPlayer(Player player) {
        gameObject.SetActive(PlayerSystem.IsLocal(player));
    }
}