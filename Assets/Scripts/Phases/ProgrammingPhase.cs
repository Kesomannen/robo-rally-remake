using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class ProgrammingPhase : NetworkSingleton<ProgrammingPhase> {
    public static bool IsStressed { get; private set; }
    public static IEnumerable<Player> PlayersLockedIn => _playersLockedIn;
    static bool LocalPlayerLockedIn { get; set; }
    
    public static readonly ObservableField<int> StressTimer = new();

    static readonly List<Player> _playersLockedIn = new();

    public static event Action PhaseStarted, StressStarted;
    public static event Action<Player> PlayerLockedIn;

    public static IEnumerator DoPhase() {
        yield return UIManager.Instance.ChangeState(UIState.Programming);

        IsStressed = false;
        PlayerUIRegister.Locked = false;
        LocalPlayerLockedIn = false;
        StressTimer.Value = LobbySystem.LobbySettings.StressTime.Value;
        
        PhaseStarted?.Invoke();
        
        foreach (var player in PlayerSystem.Players) {
            player.DrawCardsUpTo(player.CardsPerTurn);
        }
        
        yield return new WaitUntil(() => _playersLockedIn.Count >= PlayerSystem.Players.Count);
        _playersLockedIn.Clear();

        if (LobbySystem.LobbySettings.AdvancedGame.Enabled) yield break;
        foreach (var player in PlayerSystem.Players) {
            player.DiscardHand();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LockRegisterServerRpc(byte playerIndex, byte[] registerCardIds) {
        LockPlayerRegister(playerIndex, registerCardIds);
        LockRegisterClientRpc(playerIndex, registerCardIds);
    }

    [ClientRpc]
    void LockRegisterClientRpc(byte playerIndex, byte[] registerCardIds) {
        if (IsServer) return;
        LockPlayerRegister(playerIndex, registerCardIds);
    }

     void LockPlayerRegister(byte playerIndex, IEnumerable<byte> registerCardIds) {
         var player = PlayerSystem.Players[playerIndex];
         var stressEnabled = LobbySystem.LobbySettings.StressTime.Enabled;
         
         _playersLockedIn.Add(player);
         PlayerLockedIn?.Invoke(player);
         
         if (PlayerSystem.IsLocal(player)) {
             PlayerUIRegister.Locked = true;
             LocalPlayerLockedIn = true;
         } else {
             var cards = registerCardIds.Select(c => ProgramCardData.GetById(c)).ToArray();
             for (var i = 0; i < cards.Length; i++) {
                 player.Program.SetRegister(i, cards[i]);
                 Debug.Log($"Register {i} of player {playerIndex} is now {cards[i]}");
             }
             
             if (!IsStressed && stressEnabled) {
                 StartCoroutine(StressRoutine());
             }
         }

         if (IsStressed || !stressEnabled) return;
         StressStarted?.Invoke();
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
            player.Program.SetRegister(index, player.DiscardTopCardsUntil(c => c.CanPlace(player, index)));
        }

        // Lock registers
        player.SerializeRegisters(out var playerIndex, out var registerCardIds);
        LockRegisterServerRpc(playerIndex, registerCardIds);
    }

    public static void Continue() {
        _playersLockedIn.Add(PlayerSystem.LocalPlayer);
    }
}