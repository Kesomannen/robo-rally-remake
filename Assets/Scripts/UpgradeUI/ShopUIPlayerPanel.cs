public class ShopUIPlayerPanel : PlayerPanel {
    protected override void Serialize(Player player) {
        base.Serialize(player);
        
        ShopPhase.OnPhaseStarted += OnPhaseStarted;
        ShopPhase.OnPlayerDecision += OnPlayerDecision;
        ShopPhase.OnNewPlayer += OnNewPlayer;
        
        OnPhaseStarted();
    }
    
    void OnPhaseStarted() {
        SetIndicatorState(IndicatorState.Waiting);
    }
    
    void OnPlayerDecision(Player player, bool skipped, UpgradeCardData card) {
        if (Content != player) return;
        SetIndicatorState(IndicatorState.Done);
    }
    
    void OnNewPlayer(Player player) {
        if (Content != player) return;
        SetIndicatorState(IndicatorState.InProgress);
    }
}