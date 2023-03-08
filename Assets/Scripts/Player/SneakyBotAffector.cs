using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Affectors/Sneaky Bot")]
public class SneakyBotAffector : ScriptableAffector<IPlayer> {
    public override void Apply(IPlayer target) {
        target.Owner.Model.IgnoredObjectsForMoving.Add(typeof(Wall));
    }
    
    public override void Remove(IPlayer target) {
        target.Owner.Model.IgnoredObjectsForMoving.Remove(typeof(Wall));
    }
}