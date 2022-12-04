# if UNITY_EDITOR
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
    }

    public void Continue() {
        ProgrammingPhase.Continue();
    }
}
# endif