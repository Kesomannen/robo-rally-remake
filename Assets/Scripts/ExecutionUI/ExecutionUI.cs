﻿using System;
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
        ExecutionPhase.AfterRegister += AfterRegister;
        ExecutionPhase.OnPlayersOrdered += OnPlayersOrdered;
    }

    void OnDisable() {
        ExecutionPhase.OnNewSubPhase -= OnNewSubPhase;
        ExecutionPhase.BeforeRegister -= BeforeRegister;
        ExecutionPhase.AfterRegister -= AfterRegister;
        ExecutionPhase.OnPlayersOrdered -= OnPlayersOrdered;
    }
    
    void BeforeRegister(ProgramCardData card, int index, Player player) {
        player.Model.Highlight(true);
        _playerSlices[player].Show(index);
    }
    
    void AfterRegister(ProgramCardData card, int index, Player player) {
        player.Model.Highlight(false);
        _playerSlices[player].Hide(index);
    }
    
    void OnNewSubPhase(ExecutionSubPhase subPhase) {
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