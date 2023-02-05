# if UNITY_EDITOR
using Unity.Netcode;
using UnityEngine;

public class Testing : MonoBehaviour {
    [SerializeField] MapData _mapToLoad;
    [SerializeField] RobotData _robotToLoad;
    [SerializeField] UpgradeCardData[] _upgradesToLoad;

    void Awake() {
        if (NetworkManager.Singleton != null) return;

        MapSystem.Instance.LoadMap(_mapToLoad);
        var player = PlayerSystem.Instance.CreatePlayer(0, new LobbyPlayerData {
            RobotId = (byte)_robotToLoad.GetLookupId()
        });
        for (var i = 0; i < _upgradesToLoad.Length; i++) {
            player.AddUpgrade(_upgradesToLoad[i], i);
        }
    }

    void Start() {
        if (NetworkManager.Singleton != null) return;
        
        PhaseSystem.StartPhaseSystem();
    }

    public void Continue() {
        ProgrammingPhase.Continue();
    }
}
# endif