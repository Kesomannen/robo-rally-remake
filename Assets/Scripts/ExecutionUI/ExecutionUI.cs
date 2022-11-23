using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    [Header("References")]
    [SerializeField] PlayerSlice _playerSlicePrefab;
    [SerializeField] Transform _playerSliceParent;

    Dictionary<Player, PlayerSlice> _playerSlices;

    void OnEnable() {
        ExecutionPhase.OnNewSubPhase += OnNewSubPhase;
        ExecutionPhase.BeforeRegister += BeforeRegister;
    }

    void OnDisable() {
        ExecutionPhase.OnNewSubPhase -= OnNewSubPhase;
        ExecutionPhase.BeforeRegister -= BeforeRegister;
    }
    
    void BeforeRegister(ProgramCardData card, int index, Player player) {
        _playerSlices[player].Show(index);
    }
    
    void OnNewSubPhase(ExecutionSubPhase subPhase) {
        _subPhaseImage.sprite = GetSubPhaseSprite(subPhase);
        _subPhaseText.text = subPhase.ToString();
    }

    void Awake() {
        Setup();
    }

    void Setup() {
        _playerSlices = PlayerManager.Players
            .ToDictionary(p => 
                (PlayerSlice)Instantiate(_playerSlicePrefab, _playerSliceParent)
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