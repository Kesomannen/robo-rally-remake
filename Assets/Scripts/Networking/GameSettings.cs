using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class GameSettings {
    [SerializeField] GameProperty _startingEnergy;
    [SerializeField] GameProperty _cardsPerTurn;
    [SerializeField] GameProperty _stressTime;
    [SerializeField] GameProperty _upgradeSlots;
    [SerializeField] GameProperty _advancedGame;
    [SerializeField] GameProperty _beginnerGame;
    [SerializeField] GameProperty _gameSpeed;

    public GameProperty StartingEnergy => _startingEnergy;
    public GameProperty CardsPerTurn => _cardsPerTurn;
    public GameProperty StressTime => _stressTime;
    public GameProperty UpgradeSlots => _upgradeSlots;
    public GameProperty AdvancedGame => _advancedGame;
    public GameProperty BeginnerGame => _beginnerGame;
    public GameProperty GameSpeed => _gameSpeed;

    public IReadOnlyList<GameProperty> Properties => _properties;
    readonly GameProperty[] _properties;

    public bool EnergyEnabled => !_beginnerGame.Enabled;
    
    public GameSettings() {
        _startingEnergy = new GameProperty(0, 10, 5);
        _cardsPerTurn = new GameProperty(5, 10, 9);
        _stressTime = new GameProperty(5, 60, 30, true);
        _upgradeSlots = new GameProperty(2, 6, 6);
        _gameSpeed = new GameProperty(1, 5, 3);
        
        _advancedGame = new GameProperty(false);
        _beginnerGame = new GameProperty(false);
        
        _properties = new[] {
            _startingEnergy, _cardsPerTurn, _stressTime, _upgradeSlots, _advancedGame, _beginnerGame, _gameSpeed
        };
    }
}

[Serializable]
public class GameProperty {
    [SerializeField] bool _enabled;
    [ShowIf("_enabled")]
    [SerializeField] byte _value;

    public readonly bool CanToggle;
    public readonly bool HasValue;
    
    public readonly byte Min;
    public readonly byte Max;

    public bool Enabled {
        get => _enabled;
        set => _enabled = value;
    }

    public byte Value {
        get => _value;
        set => _value = value;
    }

    public GameProperty(byte min, byte max, byte defaultValue, bool canToggle = false, bool enabled = true) {
        Min = min;
        Max = max;
        Value = defaultValue;
        
        CanToggle = canToggle;
        HasValue = true;

        Enabled = enabled;
    }
    
    public GameProperty(bool defaultValue) {
        CanToggle = true;
        HasValue = false;

        Enabled = defaultValue;
    }

    public static implicit operator byte(GameProperty property) {
        return property.Value;
    }
}