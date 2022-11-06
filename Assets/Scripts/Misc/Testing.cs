using UnityEngine;

public class Testing : MonoBehaviour {
    [SerializeField] MapData _mapToLoad;

    void Awake() {
        PlayerManager.Instance.CreatePlayer(0, new PlayerData() { RobotId = 0 });    
    }

    void Start() {
        MapSystem.Instance.LoadMap(_mapToLoad);
    }
}