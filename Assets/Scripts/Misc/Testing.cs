# if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Testing : MonoBehaviour {
    [SerializeField] MapData _mapToLoad;
    [SerializeField] RobotData _robotToLoad;
    [SerializeField] UpgradeCardData[] _upgradesToLoad;

    void Awake() {
        if (NetworkManager.Singleton != null) return;

        MapSystem.Instance.LoadMap(_mapToLoad);
        PlayerSystem.Instance.CreatePlayer(0, new LobbyPlayerData {
            RobotId = (byte)_robotToLoad.GetLookupId(),
            Name = "Test Player"
        }, true);
    }

    void Start() {
        if (NetworkManager.Singleton != null) return;

        for (var i = 0; i < _upgradesToLoad.Length; i++) {
            PlayerSystem.Players[0].ReplaceUpgradeAt(_upgradesToLoad[i], i);
        }
        PhaseSystem.StartPhaseSystem();
    }

    public void Continue() => ProgrammingPhase.Continue();
}
# endif