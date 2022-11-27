using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerPropertyAffector", menuName = "ScriptableObjects/Affectors/Player Property")]
public class PlayerPropertyAffector : ScriptableAffector<IPlayer> {
    [SerializeField] int _value;
    [SerializeField] Target _target;

    public override void Apply(IPlayer target) {
        var player = target.Owner;
        switch (_target){
            case Target.CardsPerTurn: player.CardsPerTurn += _value; break;
            case Target.Energy: player.Energy.Value += _value; break;
            case Target.Priority: player.BonusPriority += _value; break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
    
    public override void Remove(IPlayer target) {
        var player = target.Owner;
        switch (_target){
            case Target.CardsPerTurn: player.CardsPerTurn -= _value; break;
            case Target.Energy: player.Energy.Value -= _value; break;
            case Target.Priority: player.BonusPriority -= _value; break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    enum Target {
        CardsPerTurn,
        Energy,
        Priority,
    }
}