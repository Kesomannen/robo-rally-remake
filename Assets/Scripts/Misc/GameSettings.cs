using UnityEngine;

[CreateAssetMenu(fileName = "NewGameSettings", menuName = "ScriptableObjects/GameSettings")]
public class GameSettings : ScriptableSingleton<GameSettings> {
    [Header("General")]
    [SerializeField] int _maxPlayers;
    [SerializeField] int _minPlayers;

    public int MaxPlayers => _maxPlayers;
    public int MinPlayers => _minPlayers;
}