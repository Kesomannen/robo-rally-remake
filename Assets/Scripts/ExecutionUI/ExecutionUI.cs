using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ExecutionUI : MonoBehaviour {
    [Header("Sub Phases")]
    [SerializeField] Image _subPhaseImage;
    [SerializeField] TMP_Text _subPhaseText;
    [Space]
    [SerializeField] Sprite _registersSprite;
    [SerializeField] Sprite _conveyorSprite;
    [SerializeField] Sprite _pushPanelSprite;
    [SerializeField] Sprite _gearSprite;
    [SerializeField] Sprite _boardLaserSprite;
    [SerializeField] Sprite _playerLaserSprite;
    [SerializeField] Sprite _energySpaceSprite;
    [SerializeField] Sprite _checkpointSprite;

    [FormerlySerializedAs("_playerSlicePrefab")]
    [Header("References")]
    [SerializeField] ExecutionPlayerPanel _executionPlayerPanelPrefab;
    [SerializeField] Transform _playerSliceParent;

    Dictionary<Player, ExecutionPlayerPanel> _playerSlices;
    PlayerModel _currentPlayer;

    void OnEnable() {
        ExecutionPhase.OnNewSubPhase += OnNewSubPhase;
        ExecutionPhase.OnPlayerRegister += OnPlayerRegister;
        ExecutionPhase.OnPlayersOrdered += OnPlayersOrdered;
    }

    void OnDisable() {
        ExecutionPhase.OnNewSubPhase -= OnNewSubPhase;
        ExecutionPhase.OnPlayerRegister -= OnPlayerRegister;
        ExecutionPhase.OnPlayersOrdered -= OnPlayersOrdered;
    }
    
    void OnPlayerRegister(ProgramCardData card, int index, Player player) {
        if (_currentPlayer != null) {
            _currentPlayer.Highlight(false);
            _playerSlices[player].Hide(index);
        }
        player.Model.Highlight(true);
        _playerSlices[player].Show(index);
        _currentPlayer = player.Model;
    }

    void OnNewSubPhase(ExecutionSubPhase subPhase) {
        if ((int)subPhase == 1) {
            _currentPlayer.Highlight(false);
            _playerSlices[_currentPlayer.Owner].Hide(ExecutionPhase.CurrentRegister);
        }
        _subPhaseImage.sprite = GetSubPhaseSprite(subPhase);
        _subPhaseText.text = subPhase.ToString();
    }
    
    void OnPlayersOrdered(IReadOnlyList<Player> players) {
        for (var i = 0; i < players.Count; i++){
            var player = players[i];
            _playerSlices[player].transform.SetSiblingIndex(i + 1);
        }
    }

    void Start() {
        Setup();
        gameObject.SetActive(false);
    }

    void Setup() {
        _playerSlices = PlayerSystem.Players
            .ToDictionary(p => 
                (ExecutionPlayerPanel)Instantiate(_executionPlayerPanelPrefab, _playerSliceParent)
                    .SetContent(p));
    }

    Sprite GetSubPhaseSprite(ExecutionSubPhase phase) {
        return phase switch {
            ExecutionSubPhase.Registers => _registersSprite,
            ExecutionSubPhase.Conveyor => _conveyorSprite,
            ExecutionSubPhase.PushPanel => _pushPanelSprite,
            ExecutionSubPhase.Gear => _gearSprite,
            ExecutionSubPhase.BoardLaser => _boardLaserSprite,
            ExecutionSubPhase.PlayerLaser => _playerLaserSprite,
            ExecutionSubPhase.EnergySpace => _energySpaceSprite,
            ExecutionSubPhase.Checkpoint => _checkpointSprite,
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null)
        };
    }
}