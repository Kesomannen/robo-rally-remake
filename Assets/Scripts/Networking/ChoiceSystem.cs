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
        while (_current.AvailableChoices[randomChoice]);

        EndChoice(randomChoice);
    }
    
    public void StartChoice(IEnumerable<bool> availableChoices, int choices, float maxTime = DefaultTime) {
        if (IsActive) {
            Debug.LogError("Choice is already active");
            return;
        }
        
        var choiceArray = availableChoices.ToArray();
        if (choiceArray.Length != choices) {
            Debug.LogError("Unavailable choices array length does not match choices");
            return;
        }
        if (choiceArray.All(x => !x)) {
            Debug.LogError("All choices are unavailable");
            return;
        }
        
        OnTimeUp += RandomizeChoice;

        _current = new ChoiceData {
            AvailableChoices = choiceArray,
            Choices = (byte)choices,
            MaxTime = (byte)maxTime
        };
        StartChoiceServerRpc(_current);
    }
    
    public void EndChoice(int choice) {
        if (NetworkManager.Singleton == null) {
            IsActive = false;
            OnChoiceMade?.Invoke(choice);
            return;
        }
        
        if (!IsActive) {
            Debug.LogError("Choice is not active");
            return;
        }
        OnTimeUp -= RandomizeChoice;
        EndChoiceServerRpc((byte) choice);
    }
    
    static IEnumerator DoTimer(float time) {
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
        StartCoroutine(DoTimer(data.MaxTime));
        
        StartChoiceClientRpc(data);
    }

    [ClientRpc]
    void StartChoiceClientRpc(ChoiceData data) {
        if (IsServer) return;
        
        _current = data;
        IsActive = true;
        StartCoroutine(DoTimer(data.MaxTime));
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
        public byte MaxTime;
        public byte Choices;
        public bool[] AvailableChoices;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref MaxTime);
            serializer.SerializeValue(ref Choices);
            if (AvailableChoices is not null && AvailableChoices.Length > 0) {
                serializer.SerializeValue(ref AvailableChoices);
            }
        }
    }
}