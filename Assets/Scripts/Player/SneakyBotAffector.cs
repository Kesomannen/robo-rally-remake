using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Affectors/Sneaky Bot")]
public class SneakyBotAffector : ScriptableAffector<Player> {
    public override void Apply(Player target) {
        target.Model.IgnoredObjectsForMoving.Add(typeof(Wall));
    }
    
    public override void Remove(Player target) {
        target.Model.IgnoredObjectsForMoving.Remove(typeof(Wall));
    }
}