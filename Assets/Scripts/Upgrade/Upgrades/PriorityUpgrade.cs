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
        ExecutionPhase.OnPlayerRegistersComplete += OnComplete;
        
        void OnComplete() {
            ExecutionPhase.OnPlayerRegistersComplete -= OnComplete;
            player.BonusPriority -= _bonusPriority;
        }
    }
}