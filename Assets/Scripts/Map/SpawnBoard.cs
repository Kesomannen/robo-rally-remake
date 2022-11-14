using UnityEngine;

public class SpawnBoard : MonoBehaviour, IBoard {
    public void Parent(Transform child) {
        child.SetParent(transform);
    }

    public void Respawn(MapObject obj) {
        if (obj is PlayerModel player) {
            var spawnPoint = PlayerManager.GetSpawnPoint(player.Owner);
            Scheduler.Push(spawnPoint.RespawnRoutine(player), $"SpawnBoard Respawn {player}");
        } else {
            Debug.LogWarning($"Respawn not implemented for {obj.name}");
        }
    }
}