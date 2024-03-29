﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SetupSystem : NetworkSingleton<SetupSystem> {
    [SerializeField] PlayerModel _playerModelPrefab;
    [SerializeField] TMP_Text _statusText;
    [SerializeField] Image _statusImage;
    [SerializeField] GameObject _uiParent;
    [SerializeField] SoundEffect _playerPlacementSound;

    List<RebootToken> _tokens;
    RebootToken _playerDecision;
    bool _playerMadeDecision;

    protected override void Awake() {
        base.Awake();
        MapSystem.MapLoaded += OnMapLoaded;
    }

    public override void OnDestroy() {
        base.OnDestroy();
        MapSystem.MapLoaded -= OnMapLoaded;
    }

    void OnMapLoaded() {
        _tokens = MapSystem.GetByType<RebootToken>().Where(token => token.IsSpawnPoint).ToList();
    }

    public IEnumerator ChooseSpawnPoints() {
        Debug.Log("Choosing spawn points");
        if (_tokens.Count == 1) {
            PlayerSystem.Players[0].CreateModel(Instance._playerModelPrefab, _tokens[0]);
            yield break;
        }
        
        UIMap.Instance.SetActive(true);
        UIMap.Instance.ZoomToFullscreen();
        UIMap.Instance.CanFocus = false;
        UIMap.Instance.IsCallingHover = false;
        
        Instance._uiParent.SetActive(true);
        
        foreach (var player in PlayerSystem.Players) {
            if (_tokens.Count > 1) {
                var isLocal = PlayerSystem.IsLocal(player);
                if (isLocal) {
                    foreach (var token in _tokens) {
                        token.GetComponent<Highlight>().enabled = true;
                        token.GetComponent<PhysicsTooltipTrigger>().enabled = false;
                    }
                    RebootToken.RebootTokenClicked += RebootTokenClicked;
                
                    Instance._statusText.text = "Choose a spawn point";
                } else {
                    Instance._statusText.text = $"{player} is choosing a spawn point";
                }
                Instance._statusImage.sprite = player.RobotData.Icon;
            
                yield return new WaitUntil(() => _playerMadeDecision);
            
                if (isLocal) {
                    foreach (var token in _tokens) {
                        token.GetComponent<Highlight>().enabled = false;
                        token.GetComponent<PhysicsTooltipTrigger>().enabled = true;
                    }
                    RebootToken.RebootTokenClicked -= RebootTokenClicked;
                }
            } else {
                _playerDecision = _tokens[0];
            }

            player.CreateModel(Instance._playerModelPrefab, _playerDecision);
            _tokens.Remove(_playerDecision);
            
            Instance._playerPlacementSound.Play();
            
            _playerDecision = null;
            _playerMadeDecision = false;
        }
        
        Instance._uiParent.SetActive(false);
        
        UIMap.Instance.IsCallingHover = true;
        
        void RebootTokenClicked(RebootToken token) {
            if (!_tokens.Contains(token)) return;
            
            var tokenIndex = _tokens.IndexOf(token);
            Instance.SendPlayerDecisionServerRpc((byte) tokenIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendPlayerDecisionServerRpc(byte tokenIndex) {
        _playerDecision = _tokens[tokenIndex];
        _playerMadeDecision = true;
        SendPlayerDecisionClientRpc(tokenIndex);
    }
    
    [ClientRpc]
    void SendPlayerDecisionClientRpc(byte tokenIndex) {
        if (IsServer) return;
        _playerDecision = _tokens[tokenIndex];
        _playerMadeDecision = true;
    }
}