using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Affectors/Zoom Bot", fileName = "ZoomBotAffector")]
public class ZoomBotAffector : ScriptableAffector<IPlayer> {
    [SerializeField] bool _relative;
    [SerializeField] Vector2Int _direction;
    [SerializeField] int _distance;
    [SerializeField] ProgramCardData _programCardPreview;
    
    readonly List<PlayerModel> _affectedPlayers = new();

    public override void Apply(IPlayer target) {
        var invocations
        if (invocations == null || !invocations.Any()) {
            ExecutionPhase.OnPhaseEnd += OnExecutionEnd;
        } 
        
        _affectedPlayers.Add(target.Owner.Model);
    }
    
    public override void Remove(IPlayer target) {
        _affectedPlayers.Remove(target.Owner.Model);
    }

    void OnExecutionEnd() {
        foreach (var model in _affectedPlayers) {
            for (var i = 0; i < _distance; i++) {
                model.Owner.RegisterPlay(_programCardPreview);
                TaskScheduler.PushRoutine(model.Move(_direction, _relative));
            }
        }
    }
}