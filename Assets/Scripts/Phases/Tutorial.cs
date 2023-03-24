using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : Singleton<Tutorial> {
    [SerializeField] GameObject _drawPile, _discardPile;
    [SerializeField] GameObject _timer;
    [SerializeField] GameObject _playerArray;
    [Space]
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
        
        public RobotData Robot => _robot;
        public MapData MapData => _mapData;
        public GameSettings GameSettings => _gameSettings;
        
        public bool EnablePileUI => _enablePileUI;
        public bool EnableTimer => _enableTimer;
        public bool EnablePlayerArray => _enablePlayerArray;
    }

    static int _currentLevel;
    bool _setup;

    public void Initialize() {
        var level = _levelData[_currentLevel];
        GameSystem.Initialize(level.GameSettings, level.MapData, new [] {
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
                NetworkSystem.ReturnToLobby();
            } else {
                NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
            }   
        }
    }
}