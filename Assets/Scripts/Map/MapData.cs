using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/Map/Basic", order = 0)]
public class MapData : Lookup<MapData> {
    [SerializeField] string _name;
    [SerializeField] MapDifficulty _difficulty;
    [SerializeField] MapLength _length;
    [SerializeField] Grid _prefab;
    [SerializeField] Vector2Int _start, _end;

    public string Name => _name;
    public MapDifficulty Difficulty => _difficulty;
    public MapLength Length => _length;
    public Grid Prefab => _prefab;
    public Vector2Int Start => _start;
    public Vector2Int End => _end;

    public virtual void OnLoad() { }
}

public enum MapDifficulty {
    Beginner,
    Intermediate,
    Advanced,
    RobotsMustDie,
}

public enum MapLength {
    Short,
    Medium,
    Long,
}