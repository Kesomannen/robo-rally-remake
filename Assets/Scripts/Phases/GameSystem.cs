using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameSystem : Singleton<GameSystem> {
    bool _isPlaying;
    
    public static GameSettings Settings { get; private set; }
    public static readonly ObservableField<Phase> CurrentPhase = new();
    
    public static event Action<GameSettings> Initialized; 

    [Serializable]
    public struct PlayerData {
        [SerializeField] RobotData _robotData;
        [SerializeField] string _name;

        public RobotData RobotData {
            get => _robotData;
            set => _robotData = value;
        }
        
        public string Name {
            get => _name;
            set => _name = value;
        }
        
        public ulong Id { get; set; }
    }

    protected override void Awake() {
        base.Awake();
        Settings = null;
    }

    public static void Initialize(GameSettings gameSettings, MapData map, IEnumerable<PlayerData> players, IEnumerable<UpgradeCardData> shopCards) {
        Settings = gameSettings;
        
        MapSystem.Instance.LoadMap(map);
        if (!gameSettings.EnergyEnabled) {
            var spaces = MapSystem.GetByType<EnergySpace>().ToArray();
            // ReSharper disable once ForCanBeConvertedToForeach
            // Collection was modified; enumeration operation may not execute.
            for (var i = 0; i < spaces.Length; i++) {
                var energySpace = spaces[i];
                MapSystem.DestroyObject(energySpace);
            }
        }
        
        // Create players       
        foreach (var playerData in players) {
            PlayerSystem.Instance.CreatePlayer(playerData.Id, playerData.RobotData, playerData.Name);
        }
        
        // Initialize systems
        ShopPhase.Instance.Initialize(shopCards);
        
        Initialized?.Invoke(gameSettings);
    }
    
    public void StartPhaseSystem() {
        StartCoroutine(PhaseRoutine());
    }
    
    public void StopPhaseSystem() {
        Debug.Log("Stopping phase system...");
        _isPlaying = false;
    }

    IEnumerator PhaseRoutine() {
        _isPlaying = true;

        yield return DoPhaseRoutine(SetupSystem.Instance.ChooseSpawnPoints(), Phase.Setup);   

        while (_isPlaying) {
            if (Settings.EnergyEnabled) {
                yield return DoPhaseRoutine(ShopPhase.Instance.DoPhase(), Phase.Shop);   
            }
            yield return DoPhaseRoutine(ProgrammingPhase.Instance.DoPhase(), Phase.Programming);
            yield return DoPhaseRoutine(ExecutionPhase.Instance.DoPhase(), Phase.Execution);
        }
        
        IEnumerator DoPhaseRoutine(IEnumerator routine, Phase phase) {
             yield return TaskScheduler.WaitUntilClear();
            yield return NetworkUtils.Instance.SyncPlayers();
            
            CurrentPhase.Value = phase;
            yield return routine;
        }
    }
}

public enum Phase {
    Programming,
    Execution,
    Shop,
    Setup
}