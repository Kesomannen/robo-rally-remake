using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Rail Gun")]
public class RailGun : UpgradeCardData {
    public override void OnAdd(Player player) {
        var list = player.Model.IgnoredObjectsForLaser;
        list.Add(typeof(Wall));
        list.Add(typeof(PlayerModel));
    }
    
    public override void OnRemove(Player player) {
        var list = player.Model.IgnoredObjectsForLaser;
        list.Remove(typeof(Wall));
        list.Remove(typeof(PlayerModel));
    }
}