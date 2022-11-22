using System;
using UnityEngine;

public abstract class UpgradeCardData : ScriptableObject, IContainable<UpgradeCardData> {
    [Header("Upgrade Card")]
    [SerializeField] string _name;
    [SerializeField] [TextArea] string _description;
    [Space]
    [SerializeField] Sprite _icon;
    [SerializeField] int _cost;

    public string Name => _name;
    public string Description => _description;
    public Sprite Icon => _icon;
    public int Cost => _cost;

    public abstract void Apply(Player player);
    public Container<UpgradeCardData> DefaultContainerPrefab => GameSettings.instance.UpgradeContainerPrefab;
}

[Flags]
public enum UseContext {
    None = 0,
    DuringProgramming = 1,
    AfterLockIn = 2,
    OwnRegister = 4,
    OtherRegisters = 8,
    BoardElements = 16,
    Shop = 32,
}