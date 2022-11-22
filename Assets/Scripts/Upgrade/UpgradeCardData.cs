using UnityEngine;

public abstract class UpgradeCardData : ScriptableObject {
    [Header("Upgrade Card")]
    [SerializeField] string _name;
    [SerializeField] string _description;
    [SerializeField] Sprite _icon;
    [SerializeField] int _cost;

    public string Name => _name;
    public string Description => _description;
    public Sprite Icon => _icon;
    public int Cost => _cost;
}