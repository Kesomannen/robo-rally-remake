using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerOverlay : Overlay {
    [Header("References")]
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _robotNameText;
    [Space]
    [SerializeField] TMP_Text _energyText;
    [SerializeField] TMP_Text _checkpointText;
    [SerializeField] TMP_Text _drawPile, _hand, _discardPile;
    [Space]
    [SerializeField] Image _iconImage;

    [Header("Animation")]
    [SerializeField] StaticUITween _tween;

    Player _player;
    
    public void Init(Player player){
        _player = player;
        Refresh();
        StartCoroutine(TweenHelper.DoUITween(_tween));
    }

    void Refresh(){
        _nameText.text = _player.ToString();
        _energyText.text = _player.Energy.ToString();
        _iconImage.sprite = _player.RobotData.Icon;
        _robotNameText.text = _player.RobotData.Name;
        _checkpointText.text = _player.CurrentCheckpoint.ToString();
        _drawPile.text = Count(_player.DrawPile);
        _hand.text = Count(_player.Hand);
        _discardPile.text = Count(_player.DiscardPile);
        
        string Count(CardCollection collection){
            return collection.Cards.Count.ToString();
        }
    }
}