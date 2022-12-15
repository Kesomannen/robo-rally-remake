using UnityEngine;

public interface IBoard {
    void Respawn(IPlayer player);
    void Parent(Transform child);
}