using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerExecutionPanel : Container<Player>, IPointerClickHandler {
    [Header("References")]
    [SerializeField] PlayerExecutionRegister[] _registers;
    [SerializeField] Image _robotImage;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _energyText;
    [SerializeField] TMP_Text _checkpointText;
    [SerializeField] GameObject _rebootedOverlay;
    [SerializeField] OverlayData<PlayerOverlay> _onClickedOverlay;

    [Header("Prefabs")]
    [SerializeField] ProgramCard _programCardPrefab;
    [SerializeField] GameObject _energyPrefab;
    
    [Header("Tween")]
    [SerializeField] Transform _tweenStart;
    [SerializeField] float _tweenTime;
    [SerializeField] float _tweenDistance;
    [SerializeField] LeanTweenType _tweenType;
    [SerializeField] float _spawnDelay;
    
    [Header("Sounds")]
    [SerializeField] SoundEffect _gainCardSound;

    public IReadOnlyList<PlayerExecutionRegister> Registers => _registers;

    void Awake() {
        ExecutionPhase.PhaseStart += OnExecutionStart;
        ExecutionPhase.PhaseEnd += OnExecutionEnd;
        PlayerSystem.PlayerRemoved += OnPlayerRemoved;
    }

    void OnDestroy() {
        ExecutionPhase.PhaseStart -= OnExecutionStart;
        ExecutionPhase.PhaseEnd -= OnExecutionEnd;
        PlayerSystem.PlayerRemoved -= OnPlayerRemoved;
    }

    void Start() {
        UpdateRegisters();
    }

    void OnEnable() {
        _energyText.text = Content?.Energy.ToString();
        _checkpointText.text = Content?.CurrentCheckpoint.ToString();
    }

    void OnPlayerRemoved(Player player) {
        if (player == Content) {
            Destroy(gameObject);
        }
    }

    void OnExecutionStart() {
        Content.Program.RegisterChanged += OnRegisterChanged;
        UpdateRegisters();
    }

    void OnExecutionEnd() {
        Content.Program.RegisterChanged -= OnRegisterChanged;
    }

    void UpdateRegisters() {
        var isLocal = PlayerSystem.IsLocal(Content);
        for (var i = 0; i < _registers.Length; i++) {
            _registers[i].SetContent(Content.Program[i]);
            _registers[i].Visible = isLocal;
        }
    }

    protected override void Serialize(Player player) {
        _nameText.text = PlayerSystem.IsLocal(player) ? player + " (You)" : player.ToString();
        _energyText.text = player.Energy.ToString();
        _checkpointText.text = player.CurrentCheckpoint.ToString();
        _rebootedOverlay.SetActive(player.IsRebooted.Value);
        _robotImage.sprite = player.RobotData.Icon;

        player.Energy.ValueChanged += OnEnergyChanged;
        player.IsRebooted.ValueChanged += (_, next) => _rebootedOverlay.SetActive(next);
        player.CurrentCheckpoint.ValueChanged += (_, next) => _checkpointText.text = next.ToString();
        player.CardAffectorApplied += OnCardAffectorApplied;
    }
    
    void OnRegisterChanged(int index, ProgramCardData prev, ProgramCardData next) {
        _registers[index].SetContent(next);
    }

    void OnCardAffectorApplied(CardAffector affector) {
        if (!gameObject.activeInHierarchy) return;
        
        var objects = new Transform[affector.Cards.Count];
        for (var i = 0; i < affector.Cards.Count; i++) {
            var card = Instantiate(_programCardPrefab);
            card.SetContent(affector.Cards[i]);
            objects[i] = card.transform;
        }
        TaskScheduler.PushRoutine(DoAnimation(objects));
    }

    void OnEnergyChanged(int prev, int next) {
        if (!gameObject.activeInHierarchy) return;
        
        if (next > prev) {
            var objects = new Transform[next - prev];
            for (var i = 0; i < objects.Length; i++) {
                objects[i] = Instantiate(_energyPrefab).transform;
            }
            TaskScheduler.PushRoutine(DoAnimation(objects));
        }
        
        _energyText.text = next.ToString();
    }

    IEnumerator DoAnimation(IReadOnlyCollection<Transform> objects) {
        Debug.Log($"Doing animation for {objects.Count} object(s)");
        
        foreach (var obj in objects) {
            obj.SetParent(_tweenStart, true);
            obj.localPosition = Vector3.zero;
            obj.localScale = Vector3.zero;
        }

        var distance = _tweenDistance * CanvasUtils.CanvasScale.x;
        foreach (var obj in objects) {
            LeanTween
                .sequence()
                .append(LeanTween.moveLocalX(obj.gameObject, -distance, _tweenTime / 2).setEase(_tweenType))
                .append(LeanTween.moveLocalX(obj.gameObject, 0, _tweenTime / 2).setEase(_tweenType));
            LeanTween
                .sequence()
                .append(LeanTween.scale(obj.gameObject, Vector3.one, _tweenTime / 2).setEase(_tweenType))
                .append(LeanTween.scale(obj.gameObject, Vector3.zero, _tweenTime / 2).setEase(_tweenType));
            yield return CoroutineUtils.Wait(_spawnDelay);
            _gainCardSound.Play();
        }
        yield return CoroutineUtils.Wait(_tweenTime);
        foreach (var obj in objects) {
            Destroy(obj.gameObject);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        OverlaySystem.Instance.PushAndShowOverlay(_onClickedOverlay).Init(Content);
    }
}