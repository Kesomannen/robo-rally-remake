using UnityEngine;

public class SpawnBoard : MonoBehaviour, IBoard {
    public void Parent(Transform child) {
        child.SetParent(transform);
    }

    public void Respawn(IPlayer player) {
        var spawnPoint = player.Owner.Model.Spawn;
        TaskScheduler.PushRoutine(spawnPoint.RespawnRoutine(player.Object));
    }
}