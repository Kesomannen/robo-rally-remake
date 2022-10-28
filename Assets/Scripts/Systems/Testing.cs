using System.Collections;
using UnityEngine;

public class Testing : MonoBehaviour {
    [SerializeField] ProgramCardData _card;

    IEnumerator Start() {
        yield return new WaitForSeconds(2);
        Scheduler.AddItem(_card.Execute(PlayerManager.Players[0], 0));
    }
}