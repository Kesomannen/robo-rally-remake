using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Affectors/Sensor Bot")]
public class SensorBotAffector : ScriptableAffector<Player> {
    public override void Apply(Player target) {
        target.BonusPriority++;
    }
    
    public override void Remove(Player target) {
        target.BonusPriority--;
    }
}