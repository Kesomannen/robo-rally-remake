using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Testing : MonoBehaviour {
    [SerializeField] MapData _mapToLoad;
    [SerializeField] RobotData _robotToLoad;

    void Awake(){
        if (NetworkManager.Singleton != null) return;
        
        MapSystem.Instance.LoadMap(_mapToLoad);
        PlayerManager.Instance.CreatePlayer(0, new LobbyPlayerData() {
            RobotId = (byte) _robotToLoad.GetLookupId()
        });

        ProgrammingPhase.OnPhaseStarted += () => {
            LeanTween.delayedCall(2f, () => Scheduler.StartRoutine(ProgrammingPhase.StressRoutine()));
        };
    }

    public void Continue() {
        ProgrammingPhase.Continue();
    }
}