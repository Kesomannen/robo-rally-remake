public class PlayerUIPlayerPanel : PlayerPanel {
    bool _programLockedIn;

    protected override void Serialize(Player player) {
        base.Serialize(player);
        
        Content.OnUpgradeUsed += OnUpgradeUsed;

        ProgrammingPhase.OnPhaseStarted += OnPhaseStarted;
        ProgrammingPhase.OnPlayerLockedIn += OnPlayerProgramDone;
        ProgrammingPhase.OnStressStarted += OnStressStarted;

        OnPhaseStarted();
    }
    
    void OnUpgradeUsed(UpgradeCardData upgrade) {
        
    }

    void OnPlayerDecision(Player player, bool skipped, UpgradeCardData card) {
        if (Content != player) return;
        SetIndicatorState(IndicatorState.Done);
    }
    
    void OnNewPlayer(Player player) {
        if (Content != player) return;
        SetIndicatorState(IndicatorState.InProgress);
    }

    void OnPlayerProgramDone(Player player) {
        if (Content != player) return;
        _programLockedIn = true;
        SetIndicatorState(IndicatorState.Done);
    }

    void OnPhaseStarted() {
        _programLockedIn = false;
        SetIndicatorState(IndicatorState.Waiting);
    }
    
    void OnStressStarted(Player player) {
        if (_programLockedIn || player == Content) return;
        SetIndicatorState(IndicatorState.InProgress);
    }
}