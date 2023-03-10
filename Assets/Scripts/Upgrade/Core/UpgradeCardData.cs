using System.Collections.Generic;
using UnityEngine;

public abstract class UpgradeCardData : Lookup<UpgradeCardData> {
    [SerializeField] string _name;
    [TextArea(minLines: 2, maxLines: 10)]
    [SerializeField] string _description;
    [SerializeField] Sprite _icon;
    [Space]
    [SerializeField] UpgradeType _type;
    [SerializeField] [Min(0)] int _cost;
    [SerializeField] [Min(0)] int _useCost;
    [Space]
    [SerializeField] UpgradeTooltipData[] _tooltips;

    public string Name => _name;
    public string Description => _description.Replace("\r", "");
    public Sprite Icon => _icon;
    
    public UpgradeType Type => _type;
    public int Cost => _cost;
    public int UseCost => _useCost;
    
    public IReadOnlyList<UpgradeTooltipData> Tooltips => _tooltips;

    public virtual bool CanUse(Player player) => false;
    public virtual void OnAdd(Player player) { }
    public virtual void OnRemove(Player player) { }
    public virtual void Use(Player player) { }

    public override string ToString() => $"{Name} ({Cost})";
}

public enum UpgradeType {
    Temporary,
    Permanent,
    Action
}