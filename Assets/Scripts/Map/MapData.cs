using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/Map")]
public class MapData : Lookup<MapData> {
    [Header("Map")]
    [SerializeField] string _name;
    [SerializeField] MapDifficulty _difficulty;
    [SerializeField] MapLength _length;
    [SerializeField] Sprite _thumbnail;

    [Header("References")]
    [SerializeField] GameObject _prefab;

    public string Name => _name;
    public MapDifficulty Difficulty => _difficulty;
    public MapLength Length => _length;
    public GameObject Prefab => _prefab;
    public Sprite Thumbnail {
        get => _thumbnail;
        set => _thumbnail = value;
    }

    public override string ToString() => _name;
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