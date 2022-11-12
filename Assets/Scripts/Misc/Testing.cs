using UnityEngine;

public class Testing : MonoBehaviour {
    [SerializeField] MapData _mapToLoad;

    void Awake() {
        MapSystem.Instance.LoadMap(_mapToLoad);
        PlayerManager.Instance.CreatePlayer(0, new LobbyPlayerData() { RobotId = 0 });
    }

    public void Continue() {
        ProgrammingPhase.Continue();
    }
}