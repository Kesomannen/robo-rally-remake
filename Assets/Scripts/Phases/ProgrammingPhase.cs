using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class ProgrammingPhase : NetworkSingleton<ProgrammingPhase> {
    public static bool IsStressed { get; private set; }
    public static bool LocalPlayerLockedIn { get; private set; }
    
    public static readonly ObservableField<int> StressTimer = new();
    
    static int _playersLockedIn;

    public static event Action OnPhaseStarted, OnStressStarted;
    public static event Action<Player> OnPlayerLockedIn;

    public static IEnumerator DoPhase() {
        yield return UIManager.Instance.ChangeState(UIState.Programming);

        IsStressed = false;
        PlayerRegisterUI.Locked = false;
        LocalPlayerLockedIn = false;
        StressTimer.Value = GameSettings.Instance.StressTime;
        
        OnPhaseStarted?.Invoke();
        
        foreach (var player in PlayerSystem.Players) {
            player.DrawCardsUpTo(player.CardsPerTurn);
        }

        _playersLockedIn = 0;
        yield return new WaitUntil(() => _playersLockedIn >= PlayerSystem.Players.Count);
    }

    [ServerRpc(RequireOwnership = false)]
    public void LockRegisterServerRpc(byte playerIndex, byte[] registerCardIds) {
        LockPlayerRegister(playerIndex, registerCardIds);
        LockRegisterClientRpc(playerIndex, registerCardIds);
        _playersLockedIn++;
    }

    [ClientRpc]
    void LockRegisterClientRpc(byte playerIndex, byte[] registerCardIds) {
        if (IsServer) return;
        LockPlayerRegister(playerIndex, registerCardIds);
        _playersLockedIn++;
    }

     void LockPlayerRegister(byte playerIndex, IEnumerable<byte> registerCardIds) {
         var player = PlayerSystem.Players[playerIndex];
         
         OnPlayerLockedIn?.Invoke(player);
         
         if (PlayerSystem.IsLocal(player)) {
             PlayerRegisterUI.Locked = true;
             LocalPlayerLockedIn = true;
         } else {
             var cards = registerCardIds.Select(c => ProgramCardData.GetById(c)).ToArray();
             for (var i = 0; i < cards.Length; i++) {
                 player.Program.SetCard(i, cards[i]);
                 Debug.Log($"Register {i} of player {playerIndex} is now {cards[i]}");
             }
             
             if (!IsStressed) {
                 StartCoroutine(StressRoutine());   
             }
         }

         if (IsStressed) return;
         OnStressStarted?.Invoke();
     }

    IEnumerator StressRoutine() {
        IsStressed = true;

        while (!LocalPlayerLockedIn) {
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
        // This is only supposed to be called on the local client
        var player = PlayerSystem.LocalPlayer;
        
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
        _playersLockedIn = PlayerSystem.Players.Count;
    }
}