using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Affector")]
public class AffectorUpgrade : UpgradeCardData {
    [ShowIf("_type", ShowIfComparer.Equals, (int)UpgradeType.Permanent)]
    [SerializeField] ScriptableAffector<Player>[] _affectors;

    [ShowIf("_type", ShowIfComparer.Equals, (int)UpgradeType.Temporary)]
    [SerializeField] ScriptablePermanentAffector<Player>[] _permanentAffectors;

    public override bool CanUse(Player player) {
        return UpgradeUtils.IsProgramming(player);
    }

    public override void OnAdd(Player player) {
        if (Type != UpgradeType.Permanent) return;
        foreach (var affector in _affectors) {
            affector.Apply(player);
        }
    }
    
    public override void OnRemove(Player player) {
        if (Type != UpgradeType.Permanent) return;
        foreach (var affector in _affectors) {
            affector.Remove(player);
        }
    }
    
    public override void Use(Player player) {
        if (Type != UpgradeType.Temporary) return;
        foreach (var affector in _permanentAffectors) {
            affector.Apply(player);
        }
    }
}