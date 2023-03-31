using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class Tutorial : Singleton<Tutorial> {
    [FormerlySerializedAs("_tutorialText")]
    [SerializeField] TMP_Text _programmingText;
    [SerializeField] TMP_Text _executionText;
    [SerializeField] TMP_Text _shopText;
    [Space]
    [SerializeField] GameObject _drawPile, _discardPile;
    [SerializeField] GameObject _playerArray;
    [SerializeField] GameObject _logButton;
    [Space]
    [SerializeField] Optional<int> _manualStartLevel;
    // ReSharper disable once NotAccessedField.Local
    [SerializeField] [ReadOnly] int _level;
    [SerializeField] [ReadOnly] bool _setup;
    [SerializeField] TutorialLevelData[] _levelData;

    static int _currentLevel;

    protected override void Awake() {
        base.Awake();
        if (_manualStartLevel.Enabled) {
            _currentLevel = _manualStartLevel.Value;
        }
        _level = _currentLevel;
    }

    public void Initialize() {
        var level = _levelData[_currentLevel];
        
        var player = new GameSystem.PlayerData {
            Id = NetworkManager.Singleton.LocalClientId,
            RobotData = level.Robot,
            Name = LobbySystem.PlayerName
        };
        GameSystem.Initialize(level.GameSettings, level.MapData, new [] { player }, level.ShopCards);
        
        _drawPile.SetActive(level.EnablePileUI);
        _discardPile.SetActive(level.EnablePileUI);
        _playerArray.SetActive(level.EnablePlayerArray);
        _logButton.SetActive(level.EnableLogButton);

        RegisterText(level.ProgrammingText, _programmingText);
        RegisterText(level.ExecutionText, _executionText);
        RegisterText(level.ShopText, _shopText);

        CheckpointSystem.PlayerWon += PlayerWon;
        _setup = true;

        void RegisterText(Optional<string> text, TMP_Text ui) {
            ui.transform.parent.SetActive(text.Enabled);
            if (!text.Enabled) return;
            ui.text = text.Value;
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        
        if (!_setup) return;
        CheckpointSystem.PlayerWon -= PlayerWon;
    }
    
    void PlayerWon(Player player) {
        TaskScheduler.PushRoutine(Task());

        IEnumerator Task() {
            yield return CoroutineUtils.Wait(1f);

            if (_currentLevel >= _levelData.Length - 1) {
                yield return NetworkSystem.Instance.ReturnToLobby();
            } else {
                _currentLevel++;
                yield return NetworkSystem.Instance.ReloadScene();
            }
        }
    }
}