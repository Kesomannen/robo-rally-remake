using System.Collections.Generic;

public class LobbySettings {
    public readonly LobbyProperty StartingEnergy, CardsPerTurn, StressTime, UpgradeSlots, AdvancedGame, BeginnerGame, GameSpeed;
    public IReadOnlyList<LobbyProperty> Properties => _properties;
    readonly LobbyProperty[] _properties;
    
    public LobbySettings() {
        StartingEnergy = new LobbyProperty(0, 10, 5);
        CardsPerTurn = new LobbyProperty(5, 10, 9);
        StressTime = new LobbyProperty(5, 60, 30, true);
        UpgradeSlots = new LobbyProperty(2, 6, 6);
        GameSpeed = new LobbyProperty(1, 5, 3);
        
        AdvancedGame = new LobbyProperty(false);
        BeginnerGame = new LobbyProperty(false);
        
        _properties = new[] {
            StartingEnergy, CardsPerTurn, StressTime, UpgradeSlots, AdvancedGame, BeginnerGame, GameSpeed
        };
    }
}

public class LobbyProperty {
    public readonly bool CanToggle;
    public readonly bool HasValue;
    
    public readonly byte Min;
    public readonly byte Max;

    public bool Enabled;
    public byte Value;

    public LobbyProperty(byte min, byte max, byte defaultValue, bool canToggle = false, bool enabled = true) {
        Min = min;
        Max = max;
        Value = defaultValue;
        
        CanToggle = canToggle;
        HasValue = true;

        Enabled = enabled;
    }
    
    public LobbyProperty(bool defaultValue) {
        CanToggle = true;
        HasValue = false;

        Enabled = defaultValue;
    }

    public static implicit operator byte(LobbyProperty property) {
        return property.Value;
    }
}