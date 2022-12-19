using System.Collections;
using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class ChoiceSystem : NetworkSingleton<ChoiceSystem> {
    public static bool IsActive { get; private set; }
    public static readonly ObservableField<float> TimeLeft = new();

    static ChoiceData _current;
    
    const float DefaultTime = 10f;
    
    public static event Action OnTimeUp;
    public static event Action<int> OnChoiceMade;

    void RandomizeChoice() {
        int randomChoice;
        do { randomChoice = Random.Range(0, _current.Choices); }
        while (!_current.UnavailableChoices[randomChoice]);

        StartCoroutine(EndChoice(randomChoice));
    }

    public IEnumerator StartChoice(IEnumerable<bool> unavailableChoices, int choices, float maxTime = DefaultTime) {
        if (IsActive) {
            Debug.LogError("Choice is already active");
            yield break;
        }
        
        var choiceArray = unavailableChoices.ToArray();
        if (choiceArray.Length != choices) {
            Debug.LogError("Unavailable choices array length does not match choices");
            yield break;
        }
        if (choiceArray.All(x => !x)) {
            Debug.LogError("All choices are unavailable");
            yield break;
        }
        
        OnTimeUp += RandomizeChoice;
        StartChoiceServerRpc(new ChoiceData {
            UnavailableChoices = choiceArray,
            Choices = (byte) choices,
            MaxTime = (short) maxTime
        });
        yield return new WaitUntil(() => IsActive);
    }
    
    public IEnumerator EndChoice(int choice) {
        if (!IsActive) {
            Debug.LogError("Choice is not active");
            yield break;
        }
        OnTimeUp -= RandomizeChoice;
        EndChoiceServerRpc((byte) choice);
        yield return new WaitUntil(() => !IsActive);
    }
    
    static IEnumerator StartTimer(float time) {
        TimeLeft.Value = time;
        while (TimeLeft.Value > 0) {
            yield return CoroutineUtils.Wait(1);
            TimeLeft.Value--;
        }
        OnTimeUp?.Invoke();
    }
    
    [ServerRpc(RequireOwnership = false)]
    void StartChoiceServerRpc(ChoiceData data) {
        _current = data;
        IsActive = true;
        StartCoroutine(StartTimer(data.MaxTime));
        
        StartChoiceClientRpc(data);
    }

    [ClientRpc]
    void StartChoiceClientRpc(ChoiceData data) {
        if (IsServer) return;
        
        _current = data;
        IsActive = true;
        StartCoroutine(StartTimer(data.MaxTime));
    }
    
    [ServerRpc(RequireOwnership = false)]
    void EndChoiceServerRpc(byte choice) {
        IsActive = false;
        OnChoiceMade?.Invoke(choice);
        EndChoiceClientRpc(choice);
    }
    
    [ClientRpc]
    void EndChoiceClientRpc(byte choice) {
        if (IsServer) return;
        IsActive = false;
        OnChoiceMade?.Invoke(choice);
    }

    struct ChoiceData : INetworkSerializable {
        public short MaxTime;
        public byte Choices;
        public bool[] UnavailableChoices;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref MaxTime);
            serializer.SerializeValue(ref Choices);
            if (UnavailableChoices is not null && UnavailableChoices.Length > 0) {
                serializer.SerializeValue(ref UnavailableChoices);
            }
        }
    }
}