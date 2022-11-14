using UnityEngine;

public interface IBoard {
    void Respawn(MapObject obj);
    void Parent(Transform child);
}