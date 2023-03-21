using System;
using System.Collections;
using UnityEngine;

public class ProgramExecution {
    public ProgramCardData CardOverride;
    public readonly Func<ProgramCardData> CardFunc;
    public readonly Player Player;
    public readonly int Register;

    bool IsExecuting { get; set; }
    bool Started { get; set; }
    
    public ProgramCardData CurrentCard => CardOverride ? CardOverride : CardFunc();
    
    const float ExecutionDelay = 0.5f;

    public event Action<ProgramExecution> ExecutionStart;
    public event Action<ProgramExecution> ExecutionEnd; 

    public ProgramExecution(Func<ProgramCardData> cardFunc, Player player, int register) {
        CardOverride = null;
        CardFunc = cardFunc;
        Player = player;
        Register = register;
    }

    public IEnumerator Execute() {
        if (CurrentCard == null) yield break;
        
        Started = true;
        IsExecuting = true;
        Player.OnExecute(this);
        ExecutionStart?.Invoke(this);
        
        yield return CoroutineUtils.Wait(ExecutionDelay);
        yield return CurrentCard.ExecuteRoutine(Player, Register);
        
        IsExecuting = false;
        ExecutionEnd?.Invoke(this);
    }
    
    public IEnumerator WaitUntilEnd() {
        yield return new WaitUntil(() => !IsExecuting);
    }
    
    public IEnumerator WaitUntilStart() {
        yield return new WaitUntil(() => Started);
    }
}