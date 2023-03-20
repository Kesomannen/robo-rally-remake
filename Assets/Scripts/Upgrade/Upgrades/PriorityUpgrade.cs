using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Priority")]
public class PriorityUpgrade : UpgradeCardData {
    [SerializeField] int _bonusPriority;
    
    public override void OnAdd(Player player) {
        UpgradeAwaiter.BeforePlayerOrdering.AddListener();
    }
    
    public override void OnRemove(Player player) {
        UpgradeAwaiter.BeforePlayerOrdering.RemoveListener();
    }

    public override bool CanUse(Player player) {
        return UpgradeAwaiter.BeforePlayerOrdering.Active;
    }

    public override void Use(Player player) {
        player.BonusPriority += _bonusPriority;
        ExecutionPhase.PlayerRegistersComplete += Complete;
        Log.Instance.RawMessage($"{Log.PlayerString(player)} gained priority for this register");
        
        void Complete() {
            ExecutionPhase.PlayerRegistersComplete -= Complete;
            player.BonusPriority -= _bonusPriority;
        }
    }
}