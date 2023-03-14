using Unity.Netcode;

public class LobbySettings : INetworkSerializable {
    public readonly LobbyProperty StartingEnergy, CardsPerTurn, StressTime, ShopCards, UpgradeSlots, AdvancedGame, BeginnerGame;

    public LobbySettings() {
        StartingEnergy = new LobbyProperty(0, 10, 5);
        CardsPerTurn = new LobbyProperty(5, 10, 9);
        StressTime = new LobbyProperty(5, 60, 30, true);
        ShopCards = new LobbyProperty(3, 5, 5);
        UpgradeSlots = new LobbyProperty(2, 6, 6);
        
        AdvancedGame = new LobbyProperty(false);
        BeginnerGame = new LobbyProperty(false);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref StartingEnergy.Value);
        serializer.SerializeValue(ref CardsPerTurn.Value);
        serializer.SerializeValue(ref StressTime.Value);
        serializer.SerializeValue(ref ShopCards.Value);
        serializer.SerializeValue(ref UpgradeSlots.Value);
        serializer.SerializeValue(ref AdvancedGame.Value);
        serializer.SerializeValue(ref BeginnerGame.Value);
        
        serializer.SerializeValue(ref StartingEnergy.Enabled);
        serializer.SerializeValue(ref CardsPerTurn.Enabled);
        serializer.SerializeValue(ref StressTime.Enabled);
        serializer.SerializeValue(ref ShopCards.Enabled);
        serializer.SerializeValue(ref UpgradeSlots.Enabled);
        serializer.SerializeValue(ref AdvancedGame.Enabled);
        serializer.SerializeValue(ref BeginnerGame.Enabled);
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
    
    public void Reset() {
        Value = _defaultValue;
        Enabled = _defaultState;
    }
    
    public static implicit operator byte(LobbyProperty property) {
        return property.Value;
    }
}