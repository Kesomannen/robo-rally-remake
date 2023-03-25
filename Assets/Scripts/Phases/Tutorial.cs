using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Tutorial : Singleton<Tutorial> {
    [SerializeField] GameObject _drawPile, _discardPile;
    [SerializeField] GameObject _timer;
    [SerializeField] GameObject _playerArray;
    [SerializeField] GameObject _logButton;
    [Space]
    [SerializeField] Optional<int> _manualStartLevel;
    // ReSharper disable once NotAccessedField.Local
    [SerializeField] [ReadOnly] int _level;
    [SerializeField] [ReadOnly] bool _setup;
    [SerializeField] TutorialLevelData[] _levelData;

    [Serializable]
    struct TutorialLevelData {
        [SerializeField] RobotData _robot;
        [SerializeField] MapData _mapData;
        [SerializeField] GameSettings _gameSettings;
        [Space]
        [SerializeField] bool _enablePileUI;
        [SerializeField] bool _enableTimer;
        [SerializeField] bool _enablePlayerArray;
        [SerializeField] bool _enableLogButton;
        
        public RobotData Robot => _robot;
        public MapData MapData => _mapData;
        public GameSettings GameSettings => _gameSettings;
        
        public bool EnablePileUI => _enablePileUI;
        public bool EnableTimer => _enableTimer;
        public bool EnablePlayerArray => _enablePlayerArray;
        public bool EnableLogButton => _enableLogButton;
    }

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
        GameSystem.Instance.Initialize(level.GameSettings, level.MapData, new [] {
            new GameSystem.PlayerData {
                Id = NetworkManager.Singleton.LocalClientId,
                RobotData = level.Robot,
                Name = LobbySystem.PlayerName
            }
        });
        
        _drawPile.SetActive(level.EnablePileUI);
        _discardPile.SetActive(level.EnablePileUI);
        _timer.SetActive(level.EnableTimer);
        _playerArray.SetActive(level.EnablePlayerArray);
        _logButton.SetActive(level.EnableLogButton);
        
        CheckpointSystem.PlayerWon += PlayerWon;
        
        _currentLevel++;
        _setup = true;
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
            
            if (_currentLevel >= _levelData.Length) {
                yield return NetworkSystem.Instance.ReturnToLobby();
            } else {
                yield return NetworkSystem.Instance.ReloadScene();
            }
        }
    }
}