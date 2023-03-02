using System.Collections;
using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Random = UnityEngine.Random;

public class ChoiceSystem : NetworkSingleton<ChoiceSystem> {
    [SerializeField] GameObject _timerPanel;
    [SerializeField] TMP_Text _timerText;
    [SerializeField] TMP_Text _messageText;
    
    static float _timeLeft;
    static readonly Queue<byte[]> _receivedResults = new();

    const float DefaultTime = 20f;

    public static IEnumerator DoChoice<T>(ChoiceData<T> data) {
        yield return Instance.DoChoiceInternal(data);
    }
    
    IEnumerator DoChoiceInternal<T>(ChoiceData<T> data) {
        data.AvailablePredicate ??= _ => true;
        if (data.Time <= 0) {
            data.Time = DefaultTime;
        }
        
        var isLocal = PlayerSystem.IsLocal(data.Player);
        Coroutine countdown = null;
        Choice<T> overlay = null;
        
        if (isLocal) {
            overlay = OverlaySystem.Instance.PushAndShowOverlay(data.Overlay);
            overlay.OnSubmit += SubmitResult;
            overlay.Init(data.Options, false, data.MinChoices, data.MaxChoices, data.AvailablePredicate);
            
            _timerPanel.SetActive(true);
            countdown = StartCoroutine(Countdown());
        } else {
            _messageText.gameObject.SetActive(true);
            _messageText.text = $"{data.Player} is {data.Message}";
        }
        yield return new WaitUntil(() => _receivedResults.Count > 0);
        
        var result = _receivedResults.Dequeue();
        for (var i = 0; i < data.OutputArray.Length; i++) {
            data.OutputArray[i] = data.Options[result[i]];
        }
        if (isLocal) {
            while (overlay != null && OverlaySystem.Instance.CurrentOverlay != overlay) {
                OverlaySystem.Instance.DestroyCurrentOverlay();
            }

            StopCoroutine(countdown);
            _timerPanel.SetActive(false);
        } else {
            _messageText.gameObject.SetActive(false);
        }

        IEnumerator Countdown() {
            _timeLeft = data.Time;
            while (_timeLeft > 0) {
                _timeLeft -= Time.deltaTime;
                _timerText.text = _timeLeft.ToString("F1", CultureInfo.InvariantCulture);
                yield return null;
            }
            var randomChoices = new int[Mathf.Min(data.MaxChoices, data.AvailableChoices.Count())];
            
            for (var i = 0; i < randomChoices.Length; i++) {
                int choice;
                do {
                    choice = Random.Range(0, data.Options.Count);
                } while (!data.AvailablePredicate(data.Options[choice]) || randomChoices.Contains(choice));
                randomChoices[i] = choice;
            }
            SubmitResult(randomChoices);
        }
    }

    void SubmitResult(IEnumerable<int> pickedChoices) {
        var result = pickedChoices.Select(x => (byte) x).ToArray();
        if (NetworkManager == null) {
            _receivedResults.Enqueue(result);
        } else {
            SubmitResultServerRpc(result);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void SubmitResultServerRpc(byte[] result) {
        _receivedResults.Enqueue(result);
        SubmitResultClientRpc(result);
    }
    
    [ClientRpc]
    void SubmitResultClientRpc(byte[] result) {
        if (IsServer) return;
        _receivedResults.Enqueue(result);
    }
}

public struct ChoiceData<T> {
    public OverlayData<Choice<T>> Overlay;
    public Player Player;
    public IReadOnlyList<T> Options;
    public string Message;
    public T[] OutputArray;
    public Func<T, bool> AvailablePredicate;
    public float Time;
    public int MinChoices;
    public int MaxChoices => OutputArray.Length;
    public IEnumerable<T> AvailableChoices => Options.Where(AvailablePredicate);
}