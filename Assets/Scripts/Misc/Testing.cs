using System.Collections;
using UnityEngine;

public class Testing : MonoBehaviour {
    [SerializeField] MapData _mapToLoad;
    [SerializeField] ProgramCardData _cardToLoad;

    void Awake() {
        MapSystem.Instance.LoadMap(_mapToLoad);
        PlayerManager.Instance.CreatePlayer(0, new LobbyPlayerData() { RobotId = 0 });
        PlayerManager.LocalPlayer.Hand.AddCard(_cardToLoad, CardPlacement.Top);
    }

    public void Continue() {
        ProgrammingPhase.Continue();
    }
}