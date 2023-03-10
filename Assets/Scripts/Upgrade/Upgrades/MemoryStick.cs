using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Memory Stick")]
public class MemoryStick : UpgradeCardData {
    public override void OnAdd(Player player) {
        player.CardsPerTurn++;
    }
    
    public override void OnRemove(Player player) {
        player.CardsPerTurn--;
    }
}