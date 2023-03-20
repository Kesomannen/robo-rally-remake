using System;
using System.Collections;
using UnityEngine;

public class ProgramExecution {
    public ProgramCardData Card;
    public readonly Player Player;
    public readonly int Register;

    bool IsExecuting { get; set; }
    bool Started { get; set; }
    
    const float ExecutionDelay = 0.5f;

    public event Action<ProgramExecution> ExecutionStart;
    public event Action<ProgramExecution> ExecutionEnd; 

    public ProgramExecution(ProgramCardData card, Player player, int register) {
        Card = card;
        Player = player;
        Register = register;
    }

    public IEnumerator Execute() {
        if (Card == null) yield break;
        
        Started = true;
        IsExecuting = true;
        Player.OnExecute(this);
        ExecutionStart?.Invoke(this);
        
        yield return CoroutineUtils.Wait(ExecutionDelay);
        yield return Card.ExecuteRoutine(Player, Register);
        
        IsExecuting = false;
        ExecutionEnd?.Invoke(this);
        Debug.Log($"Execution of {Card.name} ended");
    }
    
    public IEnumerator WaitUntilEnd() {
        yield return new WaitUntil(() => !IsExecuting);
    }
    
    public IEnumerator WaitUntilStart() {
        yield return new WaitUntil(() => Started);
    }
}