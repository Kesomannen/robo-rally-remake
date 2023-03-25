using System.Collections;
using TMPro;
using UnityEngine;

public class WinScreen : MonoBehaviour {
    [SerializeField] TMP_Text _winText;
    
    void Awake() {
        gameObject.SetActive(false);
        
        CheckpointSystem.PlayerWon += OnPlayerWon;
    }
    
    void OnDestroy() {
        CheckpointSystem.PlayerWon -= OnPlayerWon;
    }
    
    void OnPlayerWon(Player player) {
        if (NetworkSystem.CurrentGameType == NetworkSystem.GameType.Tutorial) return;
        TaskScheduler.PushRoutine(Show());

        IEnumerator Show() {
            _winText.text = $"{player} won!";
            gameObject.SetActive(true);
            GameSystem.Instance.StopPhaseSystem();

            yield return new WaitUntil(() => !enabled);
        }
    }

    public void ReturnToMenu() {
        StartCoroutine(NetworkSystem.Instance.ReturnToLobby());
    }
}