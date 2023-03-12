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

    public event Action<ProgramExecution> OnExecutionStart;
    public event Action<ProgramExecution> OnExecutionEnd; 

    public ProgramExecution(ProgramCardData card, Player player, int register) {
        Card = card;
        Player = player;
        Register = register;
    }

    public IEnumerator Execute() {
        Started = true;
        IsExecuting = true;
        Player.OnExecute(this);
        OnExecutionStart?.Invoke(this);
        
        yield return CoroutineUtils.Wait(ExecutionDelay);
        yield return Card.ExecuteRoutine(Player, Register);
        
        IsExecuting = false;
        OnExecutionEnd?.Invoke(this);
        Debug.Log($"Execution of {Card.name} ended");
    }
    
    public IEnumerator WaitUntilEnd() {
        yield return new WaitUntil(() => !IsExecuting);
    }
    
    public IEnumerator WaitUntilStart() {
        yield return new WaitUntil(() => Started);
    }
}