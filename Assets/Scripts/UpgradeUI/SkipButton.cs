using UnityEngine;
using UnityEngine.EventSystems;

public class SkipButton : MonoBehaviour, IPointerClickHandler {
    void Awake() {
        ShopPhase.NewPlayer += OnNewPlayer;
        ShopPhase.PlayerDecision += OnPlayerDecision;
    }
    
    void OnDestroy() {
        ShopPhase.NewPlayer -= OnNewPlayer;
        ShopPhase.PlayerDecision -= OnPlayerDecision;
    }

    void OnEnable() {
        OnNewPlayer(ShopPhase.Instance.CurrentPlayer);
    }
    
    void OnPlayerDecision(Player player, bool skipped, UpgradeCardData upgrade) {
        gameObject.SetActive(false);
    }
    
    void OnNewPlayer(Player player) {
        gameObject.SetActive(PlayerSystem.IsLocal(player));
    }

    public void OnPointerClick(PointerEventData e) {
        ShopPhase.Instance.MakeDecision(true, null, 0);
        gameObject.SetActive(false);
    }
}