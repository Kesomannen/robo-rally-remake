public class ShopUIPlayerPanel : PlayerPanel {
    protected override void Serialize(Player player) {
        base.Serialize(player);
        
        ShopPhase.PhaseStarted += OnPhaseStarted;
        ShopPhase.PlayerDecision += OnPlayerDecision;
        ShopPhase.NewPlayer += OnNewPlayer;
        
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