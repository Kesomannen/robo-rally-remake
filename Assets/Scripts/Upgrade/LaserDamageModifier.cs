using UnityEngine;

[CreateAssetMenu(fileName = "LaserDamageModifier", menuName = "ScriptableObjects/Upgrades/LaserDamageModifier")]
public class LaserDamageModifier : PermanentUpgrade {
    [SerializeField] ProgramCardData[] _cards;
    
    public override void Apply(Player player) {
        player.LaserDamage.Cards.AddRange(_cards);
    }
    
    public override void Remove(Player player) {
        foreach (var card in _cards){
            player.LaserDamage.Cards.Remove(card);
        }
    }
}