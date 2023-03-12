using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Affectors/Zoom Bot", fileName = "ZoomBotAffector")]
public class ZoomBotAffector : ScriptableAffector<Player> {
    [SerializeField] ProgramCardData _programCard;
    
    readonly List<Player> _affectedPlayers = new();

    public override void Apply(Player target) {
        var invocations = ExecutionPhase.GetPhaseEndInvocations();
        if (invocations == null || !invocations.Contains(OnExecutionEnd)) {
            ExecutionPhase.OnPhaseEnd += OnExecutionEnd;
        } 
        
        _affectedPlayers.Add(target);
    }
    
    public override void Remove(Player target) {
        _affectedPlayers.Remove(target);
    }

    void OnExecutionEnd() {
        for (var i = 0; i < _affectedPlayers.Count; i++) {
            var player = _affectedPlayers[i];
            if (player == null) {
                _affectedPlayers.RemoveAt(i);
                i--;
                continue;
            }

            const int register = ExecutionPhase.RegisterCount - 1;
            TaskScheduler.PushRoutine(new ProgramExecution(_programCard, player, register).Execute());
        }
    }
}