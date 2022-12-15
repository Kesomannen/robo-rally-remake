using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class ProgrammingPhase : NetworkSingleton<ProgrammingPhase> {
    public static bool IsStressed { get; private set; }
    public static bool LocalPlayerSubmitted { get; private set; }
    
    public static readonly ObservableField<int> StressTimer = new(0);
    
    static int _playersReady;

    public static event Action OnPhaseStarted;

    public static IEnumerator DoPhase() {
        UIManager.Instance.ChangeState(UIState.Hand);

        IsStressed = false;
        RegisterUI.Locked = false;
        LocalPlayerSubmitted = false;
        StressTimer.Value = GameSettings.Instance.StressTime;
        
        OnPhaseStarted?.Invoke();
        
        foreach (var player in PlayerManager.Players) {
            player.DrawCardsUpTo(player.CardsPerTurn);
        }

        _playersReady = 0;
        yield return new WaitUntil(() => _playersReady >= PlayerManager.Players.Count);
        
        yield return TaskScheduler.WaitUntilClear();
    }

    [ServerRpc(RequireOwnership = false)]
    public void LockRegisterServerRpc(byte playerIndex, byte[] registerCardIds) {
        LockPlayerRegister(playerIndex, registerCardIds);
        LockRegisterClientRpc(playerIndex, registerCardIds);
        _playersReady++;
    }

    [ClientRpc]
    void LockRegisterClientRpc(byte playerIndex, byte[] registerCardIds) {
        if (IsServer) return;
        LockPlayerRegister(playerIndex, registerCardIds);
        _playersReady++;
    }

     void LockPlayerRegister(byte playerIndex, IEnumerable<byte> registerCardIds) {
        if (PlayerManager.IsLocal(PlayerManager.Players[playerIndex])){
            RegisterUI.Locked = true;
            LocalPlayerSubmitted = true;
            return;
        }

        Debug.Log($"Locking register for player {playerIndex}");

        var player = PlayerManager.Players[playerIndex];
        var cards = registerCardIds.Select(id => ProgramCardData.GetById(id)).ToArray();
        for (var i = 0; i < cards.Length; i++) {
            player.Program.SetCard(i, cards[i]);
            Debug.Log($"Register {i} of player {playerIndex} is now {cards[i]}");
        }

        if (LocalPlayerSubmitted || IsStressed) return;
        
        StartCoroutine(StressRoutine());
    }

    IEnumerator StressRoutine(){
        IsStressed = true;
        while (!LocalPlayerSubmitted) {
            StressTimer.Value--;
            if (StressTimer.Value <= 0) {
                FillRegisters();
                IsStressed = false;
                yield break;
            }
            yield return CoroutineUtils.Wait(1);
        }
    }

    void FillRegisters() {
        // this is only supposed to be called on the local client
        var player = PlayerManager.LocalPlayer;
        
        player.DiscardHand();
        player.DiscardProgram();

        // Fill empty registers
        for (var i = 0; i < player.Program.Cards.Count; i++){
            if (player.Program[i] != null) continue;
            
            var index = i;
            player.Program.SetCard(index, player.DiscardTopCardsUntil(c => c.CanPlace(player, index)));
        }

        // Lock registers
        player.SerializeRegisters(out var playerIndex, out var registerCardIds);
        LockRegisterServerRpc(playerIndex, registerCardIds);
    }

    public static void Continue() {
        _playersReady = PlayerManager.Players.Count;
    }
}