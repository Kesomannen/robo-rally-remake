using UnityEngine;

[CreateAssetMenu(fileName = "RobotData", menuName = "ScriptableObjects/Robots/Generic Robot", order = 0)]
public class RobotData : Lookup<RobotData> {
    [SerializeField] string _name;
    [SerializeField] Sprite _icon;
    [SerializeField] Sprite _sprite;
    [SerializeField] ProgramCardData[] _startingDeck;
    [SerializeField] Damage _laserDamage;

    public string Name => _name;
    public Sprite Icon => _icon;
    public Sprite Sprite => _sprite;
    public ProgramCardData[] StartingDeck => _startingDeck;
    public Damage LaserDamage => _laserDamage;
}