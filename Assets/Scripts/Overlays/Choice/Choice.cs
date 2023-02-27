using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public abstract class Choice<T> : Overlay {
    [SerializeField] TMP_Text _timerText;

    protected IReadOnlyList<T> Options;
    protected IReadOnlyList<bool> AvailableOptions;

    Action<T> _callback;

    protected abstract void OnInit();
    
    public void Init(IReadOnlyList<T> options, IReadOnlyList<bool> availableArray, Action<T> callback = null) {
        Options = options;
        AvailableOptions = availableArray;
        _callback = callback;
        
        ChoiceSystem.Instance.StartChoice(availableArray, options.Count);
        
        OnInit();
    }

    protected override void OnEnable() {
        base.OnEnable();
        ChoiceSystem.TimeLeft.OnValueChanged += OnTimeLeftChanged;
    }

    protected override void OnDisable() {
        base.OnDisable();
        ChoiceSystem.TimeLeft.OnValueChanged -= OnTimeLeftChanged;
    }

    public class ChoiceResult {
        public bool ChoiceMade;
        public int Index;
        public T Value;
    }
    
    public static IEnumerator Create(
        Player player,
        OverlayData<Choice<T>> overlayData,
        IReadOnlyList<T> options, 
        IReadOnlyList<bool> availableArray,
        ChoiceResult result,
        float maxTime = 10f,
        Action<T> callback = null)
    {
        if (PlayerSystem.IsLocal(player)) {
            OverlaySystem.Instance.PushAndShowOverlay(overlayData).Init(options, availableArray, callback);
            ChoiceSystem.Instance.StartChoice(availableArray, options.Count, maxTime);
        }
        
        ChoiceSystem.OnChoiceMade += OnChoiceMade;
        void OnChoiceMade(int index) {
            result.ChoiceMade = true;
            result.Index = index;
            result.Value = options[index];
            ChoiceSystem.OnChoiceMade -= OnChoiceMade;
        }

        yield return new WaitUntil(() => result.ChoiceMade);
    }

    void OnTimeLeftChanged(float _, float timeLeft) {
        _timerText.text = timeLeft.ToString(CultureInfo.InvariantCulture);
    }

    protected void OnOptionChoose(T choice) {
        ChoiceSystem.Instance.EndChoice(Options.IndexOf(choice));
        OverlaySystem.Instance.DestroyCurrentOverlay();
        _callback?.Invoke(choice);
    }
    
    protected override void OnOverlayClick() { }
}