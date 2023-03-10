using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RobotData", menuName = "ScriptableObjects/RobotData", order = 0)]
public class RobotData : Lookup<RobotData>, ITooltipable {
    [Header("Metadata")]
    [SerializeField] string _name;
    [SerializeField] [TextArea] string _description;
    [SerializeField] Sprite _icon;
    [SerializeField] Sprite _sprite;
    [SerializeField] Color _color;
    
    [Header("Stats")]
    [SerializeField] ProgramCardData[] _startingDeck;
    [SerializeField] ScriptableCardAffector _laserDamage;
    [SerializeField] ScriptableCardAffector _pushDamage;
    [SerializeField] ScriptableAffector<IPlayer>[] _onSpawnAffectors;

    public string Name => _name;
    public string Header => _name;
    public string Description => _description;
    
    public Sprite Icon => _icon;
    public Sprite Sprite => _sprite;
    public Color Color => _color;
    public IEnumerable<ProgramCardData> StartingDeck => _startingDeck;

    public CardAffector GetLaserDamage() => _laserDamage.ToInstance();
    public CardAffector GetPushDamage() => _pushDamage.ToInstance();

    public void OnSpawn(Player player) {
        foreach (var affector in _onSpawnAffectors){
            affector.Apply(player);
        }
    }
}