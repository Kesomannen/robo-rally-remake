using System;
using UnityEngine;

[Serializable]
public struct UpgradeTooltipData {
    [SerializeField] UpgradeTooltipType _type;
    [SerializeField] string _header, _description;
    [SerializeField] ProgramCardData _programCard;
    [SerializeField] UpgradeCardData _upgradeCard;
    [SerializeField] UpgradeMechanic _mechanic;

    public UpgradeTooltipType Type => _type;
    public string Header => _header;
    public string Description => _description;
    public ProgramCardData ProgramCard => _programCard;
    public UpgradeCardData UpgradeCard => _upgradeCard;
    public UpgradeMechanic Mechanic => _mechanic;

    public static (string Header, string Description) GetMechanic(UpgradeMechanic mechanic) {
        return mechanic switch {
            UpgradeMechanic.Deal => ("Deal", "Deal cards to the top of opponent's discard pile."),
            UpgradeMechanic.Reboot => ("Reboot", "Robots reboot when they fall off the map or in a pit."),
            UpgradeMechanic.Energy => ("Energy", "Energy is used to buy upgrades in the shop."),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public enum UpgradeTooltipType {
    Text,
    ProgramCard,
    UpgradeCard,
    Mechanic,
}

public enum UpgradeMechanic {
    Deal,
    Reboot,
    Energy,
}