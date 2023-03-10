using System;
using System.Collections;
using UnityEngine;

public class ProgramExecution {
    public readonly ProgramCardData Card;
    public readonly Player Player;
    public readonly int Register;
    
    public bool IsExecuting { get; private set; }
    public bool Started { get; private set; }

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
        OnExecutionStart?.Invoke(this);
        
        yield return Card.ExecuteRoutine(Player, Register);
        
        IsExecuting = false;
        OnExecutionEnd?.Invoke(this);
    }
    
    public IEnumerator WaitUntilEnd() {
        yield return new WaitUntil(() => !IsExecuting);
    }
    
    public IEnumerator WaitUntilStart() {
        yield return new WaitUntil(() => Started);
    }
}