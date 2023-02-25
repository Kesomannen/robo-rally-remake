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
        PlayerSystem.Instance.CreatePlayer(0, "Kesomannen", new LobbyPlayerData {
            RobotId = (byte)_robotToLoad.GetLookupId(),
        });
    }

    void Start() {
        if (NetworkManager.Singleton != null) return;

        for (var i = 0; i < _upgradesToLoad.Length; i++) {
            PlayerSystem.Players[0].AddUpgrade(_upgradesToLoad[i], i);
        }
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