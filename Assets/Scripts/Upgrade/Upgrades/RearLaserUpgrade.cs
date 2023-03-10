using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Rear Laser")]
public class RearLaserUpgrade : UpgradeCardData {
    public override void OnAdd(Player player) {
        player.Model.LaserDirections.Add(Vector2Int.left);
    }
    
    public override void OnRemove(Player player) {
        player.Model.LaserDirections.Remove(Vector2Int.left);
    }
}