using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObjects/Tutorial")]
public class TutorialLevelData : ScriptableObject {
    [FormerlySerializedAs("_tutorialText")] 
    [SerializeField] [TextArea(minLines: 2, maxLines: 20)] string _programmingText;
    [SerializeField] [TextArea(minLines: 2, maxLines: 20)] string _executionText;
    [SerializeField] [TextArea(minLines: 2, maxLines: 20)] string _shopText;
    [Space]
    [SerializeField] RobotData _robot;
    [SerializeField] MapData _mapData;
    [SerializeField] GameSettings _gameSettings;
    [SerializeField] UpgradeCardData[] _shopCards;
    [Space]
    [SerializeField] bool _enablePileUI;
    [SerializeField] bool _enablePlayerArray;
    [SerializeField] bool _enableLogButton;
        
    public Optional<string> ProgrammingText => new(_programmingText, !string.IsNullOrEmpty(_programmingText));
    public Optional<string> ExecutionText => new(_executionText, !string.IsNullOrEmpty(_executionText));
    public Optional<string> ShopText => new(_shopText, !string.IsNullOrEmpty(_shopText));
    
    public RobotData Robot => _robot;
    public MapData MapData => _mapData;
    public GameSettings GameSettings => _gameSettings;
    public IEnumerable<UpgradeCardData> ShopCards => _shopCards;
        
    public bool EnablePileUI => _enablePileUI;
    public bool EnablePlayerArray => _enablePlayerArray;
    public bool EnableLogButton => _enableLogButton;
}