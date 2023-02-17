# if UNITY_EDITOR
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Testing : MonoBehaviour {
    [SerializeField] MapData _mapToLoad;
    [SerializeField] RobotData _robotToLoad;
    [SerializeField] UpgradeCardData[] _upgradesToLoad;
    [SerializeField] PlayerExecutionPanels _panelsController;

    void Awake() {
        if (NetworkManager.Singleton != null) return;

        MapSystem.Instance.LoadMap(_mapToLoad);
        var player = PlayerSystem.Instance.CreatePlayer(0, new LobbyPlayerData {
            RobotId = (byte)_robotToLoad.GetLookupId()
        });
        for (var i = 0; i < _upgradesToLoad.Length; i++) {
            player.AddUpgrade(_upgradesToLoad[i], i);
        }
        _panelsController.CreatePanel(player);
        _panelsController.CreatePanel(player);
    }

    void Start() {
        if (NetworkManager.Singleton != null) return;

        PhaseSystem.StartPhaseSystem();
    }

    public void Continue() {
        ProgrammingPhase.Continue();
    }

    float _lastPress;
    
    void Update() {
        /*
        if (Time.time - _lastPress < 0.5f) return;
        
        if (InputSystem.GetDevice<Keyboard>().spaceKey.wasPressedThisFrame) {
            TaskScheduler.PushRoutine(_panelsController.Swap(1, 2));   
            _lastPress = Time.time;
        }
        */
    }
}
# endif