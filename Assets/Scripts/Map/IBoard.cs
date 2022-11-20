using UnityEngine;

public interface IBoard {
    void Respawn(IPlayer playerModel);
    void Parent(Transform child);
}