using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Affectors/Castle Bot")]
public class CastleBotAffector : ScriptableAffector<Player> {
    public override void Apply(Player target) {
        target.Owner.Model.Movable = false;
    }
    
    public override void Remove(Player target) {
        target.Owner.Model.Movable = true;
    }
}