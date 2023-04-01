using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class WinScreen : MonoBehaviour {
    [FormerlySerializedAs("_winnerContainerPrefab")]
    [SerializeField] WinScreenPanel _winnerPanelPrefab;
    [SerializeField] Transform _winnerParent;
    [FormerlySerializedAs("_loserContainerPrefab")] 
    [SerializeField] WinScreenPanel _loserPanelPrefab;
    [Space]
    [SerializeField] Transform _loserParent;
    [SerializeField] GameObject _previewButton;
    [SerializeField] DynamicUITween _tween;
    [Space]
    [SerializeField] SoundEffect _winSoundEffect;

    void Awake() {
        gameObject.SetActive(false);
        
        CheckpointSystem.PlayerWon += OnPlayerWon;
    }
    
    void OnDestroy() {
        CheckpointSystem.PlayerWon -= OnPlayerWon;
    }
    
    void OnPlayerWon(Player winner) {
        if (NetworkSystem.CurrentGameType == NetworkSystem.GameType.Tutorial) return;
        TaskScheduler.PushRoutine(Show());

        IEnumerator Show() {
            var objects = new List<GameObject>();
            
            objects.AddRange(CreatePanel(_winnerPanelPrefab, _winnerParent, winner));
            foreach (var loser in PlayerSystem.Players.Drop(winner).OrderByDescending(player => player.CurrentCheckpoint.Value)) {
                objects.AddRange(CreatePanel(_loserPanelPrefab, _loserParent, loser));   
            }

            gameObject.SetActive(true);
            _previewButton.SetActive(true);
            GameSystem.Instance.StopPhaseSystem();
            
            _winSoundEffect.Play();
            StartCoroutine(TweenHelper.DoUITween(_tween, objects));

            yield return new WaitUntil(() => !enabled);

            IEnumerable<GameObject> CreatePanel(WinScreenPanel prefab, Transform parent, Player player) {
                return Instantiate(prefab, parent).SetContent(player).Objects.Select(obj => obj.gameObject);
            }
        }
    }

    public void ReturnToMenu() {
        StartCoroutine(NetworkSystem.Instance.ReturnToLobby());
    }
}