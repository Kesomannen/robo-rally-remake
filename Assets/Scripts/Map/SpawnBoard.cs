using UnityEngine;

public class SpawnBoard : MonoBehaviour, IBoard {
    public void Parent(Transform child) {
        child.SetParent(transform);
    }

    public void Respawn(IPlayer player) {
        var spawnPoint = PlayerManager.GetSpawnPoint(player.Owner);
        Scheduler.Push(spawnPoint.RespawnRoutine(player.Object), $"SpawnBoard Respawn {player}");
    }
}