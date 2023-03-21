using System.Collections.Generic;

public class LobbySettings {
    public readonly LobbyProperty StartingEnergy, CardsPerTurn, StressTime, ShopCards, UpgradeSlots, AdvancedGame, BeginnerGame, GameSpeed;
    public IReadOnlyList<LobbyProperty> Properties => _properties;
    readonly LobbyProperty[] _properties;
    
    public LobbySettings() {
        StartingEnergy = new LobbyProperty(0, 10, 5);
        CardsPerTurn = new LobbyProperty(5, 10, 9);
        StressTime = new LobbyProperty(5, 60, 30, true);
        ShopCards = new LobbyProperty(3, 5, 5);
        UpgradeSlots = new LobbyProperty(2, 6, 6);
        GameSpeed = new LobbyProperty(1, 5, 3);
        
        AdvancedGame = new LobbyProperty(false);
        BeginnerGame = new LobbyProperty(false);
        
        _properties = new[] {
            StartingEnergy, CardsPerTurn, StressTime, ShopCards, UpgradeSlots, AdvancedGame, BeginnerGame, GameSpeed
        };
    }
}

public class LobbyProperty {
    public readonly bool CanToggle;
    public readonly bool HasValue;
    
    public readonly byte Min;
    public readonly byte Max;
    readonly byte _defaultValue;
    readonly bool _defaultState;
    
    public bool Enabled;
    public byte Value;

    public LobbyProperty(byte min, byte max, byte defaultValue, bool canToggle = false, bool enabled = true) {
        Min = min;
        Max = max;
        Value = defaultValue;
        
        CanToggle = canToggle;
        HasValue = true;

        _defaultValue = defaultValue;
        _defaultState = enabled;
        Enabled = enabled;
    }
    
    public LobbyProperty(bool defaultValue) {
        CanToggle = true;
        HasValue = false;
        
        _defaultState = defaultValue;
        Enabled = defaultValue;
    }

    public static implicit operator byte(LobbyProperty property) {
        return property.Value;
    }
}