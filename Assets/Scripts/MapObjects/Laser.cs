using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Laser : MapObject, IOnEnterExitHandler {
    [SerializeField] PassiveAnimation[] _inactivatedAnimations;
    [SerializeField] PassiveAnimation[] _activeAnimations;
    [SerializeField] Light2D[] _lights;
    [SerializeField] SpriteRenderer[] _renderers;
    [SerializeField] Color _activeColor, _inactiveColor;
    
    public event Action<Laser, MapObject> OnObstructed, OnUnobstructed;

    public void SetActiveVisual(bool active) {
        for (var i = 0; i < _lights.Length; i++) {
            _inactivatedAnimations[i].enabled = !active;
            _activeAnimations[i].enabled = active;
            _lights[i].enabled = active;
            _renderers[i].color = active ? _activeColor : _inactiveColor;
        }
    }
    
    public void OnEnter(MapObject mapObject) {
        OnObstructed?.Invoke(this, mapObject);
    }

    public void OnExit(MapObject mapObject) {
        OnUnobstructed?.Invoke(this, mapObject);
    }

    public static List<T> ShootLaser<T>(T prefab, MapObject source, Vector2Int dir, int maxDistance = 20, bool ignoreSource = true, IReadOnlyList<Type> ignoredTypes = null) where T : Laser {
        var i = 0;
        Vector2Int pos;
        var lasers = new List<T>();
        
        do {
            pos = source.GridPos + dir * i;
            var onMap = MapSystem.Instance.TryGetBoard(pos, out var board);
            if (!onMap) break;

            if (i > 0 || !ignoreSource) {
                var laser = MapSystem.Instance.CreateObject(prefab, pos, board, false);
                laser.transform.rotation = source.transform.rotation;
                lasers.Add(laser);
            }
            i++;

            if (i >= maxDistance) break;
        } while (Interaction.CanMove(pos, dir, ignoredTypes: ignoredTypes));
        return lasers;
    }
}