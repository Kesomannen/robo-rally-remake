﻿using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "InfectiveProgram", menuName = "ScriptableObjects/Programs/Infective")]
public class InfectiveProgram : SpamProgram {
    [SerializeField] int _infectionRange;
    [SerializeField] CardAffector _cardAffector;
    
    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        foreach (var plr in PlayerManager.Players){
            if (plr == player) continue;
            if (Vector2Int.Distance(plr.Model.GridPos, player.Model.GridPos) >= _infectionRange){
                _cardAffector.Apply(plr);
            }
        }
        yield return base.ExecuteRoutine(player, positionInRegister);
    }
}