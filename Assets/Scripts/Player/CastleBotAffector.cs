using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Affectors/Castle Bot")]
public class CastleBotAffector : ScriptableAffector<IPlayer> {
    public override void Apply(IPlayer target) {
        target.Owner.Model.Movable = false;
    }
    
    public override void Remove(IPlayer target) {
        target.Owner.Model.Movable = true;
    }
}