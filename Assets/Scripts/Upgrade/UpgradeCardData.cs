using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeCardData", menuName = "ScriptableObjects/UpgradeData")]
public class UpgradeCardData : Lookup<UpgradeCardData>, IAffector<IPlayer> {
    [SerializeField] string _name;
    [TextArea(minLines: 1, maxLines: 10)]
    [SerializeField] string _description;
    [SerializeField] Sprite _icon;
    
    [SerializeField] int _cost;
    [SerializeField] int _useCost;
    [SerializeField] UpgradeType _type;
    [SerializeField] UseContext _useContext = UseContext.None;

    [SerializeField] ScriptableAffector<IPlayer>[] _temporaryAffectors;
    [SerializeField] ScriptablePermanentAffector<IPlayer>[] _permanentAffectors;
    
    [SerializeField] UpgradeTooltipData[] _tooltips;

    public string Name => _name;
    public string Description => _description.Replace("\r", "");
    public Sprite Icon => _icon;
    public int Cost => _cost;
    public int UseCost => _useCost;
    public UpgradeType Type => _type;
    public IReadOnlyList<UpgradeTooltipData> Tooltips => _tooltips;

    public bool CanUse(IPlayer player) {
        if (_type == UpgradeType.Permanent
            || _type == UpgradeType.Action && player.Owner.Energy.Value < UseCost) return false;
        if (player.Owner.IsRebooted.Value) return false;

        return PhaseSystem.Current.Value switch {
            Phase.Programming => CanUseIn(ProgrammingPhase.LocalPlayerLockedIn ? UseContext.AfterLockIn : UseContext.DuringProgramming),
            Phase.Execution => ExecutionPhase.CurrentSubPhase == ExecutionSubPhase.Registers 
                ? CanUseIn(PlayerSystem.IsLocal(ExecutionPhase.CurrentPlayer) ? UseContext.OwnRegister : UseContext.OtherRegisters) 
                : CanUseIn(UseContext.BoardElements),
            Phase.Shop => false,
            _ => throw new Exception("Unknown phase")
        };

        bool CanUseIn(UseContext context) => _useContext.HasFlag(context);
    }

    public void OnBuy(IPlayer player) {
        if (_type == UpgradeType.Permanent) {
            Apply(player);
        }
    }
    
    public void Apply(IPlayer player) {
        var affectors = _type switch {
            UpgradeType.Temporary => _permanentAffectors,
            UpgradeType.Permanent => _temporaryAffectors,
            UpgradeType.Action => _permanentAffectors,
            _ => throw new Exception("Unknown upgrade type")
        };

        foreach (var affector in affectors) {
            affector.Apply(player);
        }

        if (_type == UpgradeType.Action) {
            player.Owner.Energy.Value -= _useCost;
        }
    }
    
    public void Remove(IPlayer player) {
        if (_type != UpgradeType.Permanent) return;
        foreach (var affector in _temporaryAffectors){
            affector.Remove(player);
        }
    }

    public override string ToString() {
        return $"{Name} ({Cost})";
    }
}

[Flags]
public enum UseContext {
    None = 0,
    ProgrammingPhase = DuringProgramming | AfterLockIn,
    ExecutionPhase = OwnRegister | OtherRegisters | BoardElements,
    DuringProgramming = 1,
    AfterLockIn = 2,
    OwnRegister = 4,
    OtherRegisters = 8,
    BoardElements = 16
}

public enum UpgradeType {
    Temporary,
    Permanent,
    Action,
}