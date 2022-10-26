using UnityEngine;

public class PlayerManager : Singleton<PlayerManager> {
    [SerializeField] PlayerModel _playerModelPrefab;
    [SerializeField] int _playerCount;
    [SerializeField] Transform[] _spawnPoints;

    public PlayerModel PlayerModelPrefab => _playerModelPrefab;

    public static int PlayerCount => instance._playerCount;
    public static Player[] Players { get; private set; }

    void Start() {
        Players = new Player[_playerCount];
        for (int i = 0; i < _playerCount; i++) {
            var pos = MapSystem.instance.GetGridPos(_spawnPoints[i].position);
            Players[i] = new Player(pos);
        }
    }
}