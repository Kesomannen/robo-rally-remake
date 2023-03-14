using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Hover Unit")]
public class HoverUnit : UpgradeCardData {
    public override void OnAdd(Player player) {
        player.Model.Hovering = true;
    }
    
    public override void OnRemove(Player player) {
        player.Model.Hovering = false;
    }
}