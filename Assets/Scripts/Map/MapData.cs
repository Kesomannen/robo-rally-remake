using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/Map")]
public class MapData : Lookup<MapData> {
    [Header("Map")]
    [SerializeField] string _name;
    [SerializeField] MapDifficulty _difficulty;
    [SerializeField] MapLength _length;

    [Header("References")]
    [SerializeField] GameObject _prefab;

    public string Name => _name;
    public MapDifficulty Difficulty => _difficulty;
    public MapLength Length => _length;
    public GameObject Prefab => _prefab;
    public Texture2D Thumbnail => null; 

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