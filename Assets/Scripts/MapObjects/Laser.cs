using System;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MapObject, IOnEnterExitHandler {
    public event Action<Laser, MapObject> OnObstructed, OnUnobstructed;
    
    public void OnEnter(MapObject mapObject){
        OnObstructed?.Invoke(this, mapObject);
    }

    public void OnExit(MapObject mapObject) {
        OnUnobstructed?.Invoke(this, mapObject);
    }

    public static List<Laser> ShootLaser(Laser prefab, MapObject source, Vector2Int dir, int maxDistance = 20, bool ignoreSource = true) {
        var i = 0;
        Vector2Int pos;
        var lasers = new List<Laser>();
        
        do{
            pos = source.GridPos + dir * i;
            var onMap = MapSystem.Instance.TryGetBoard(pos, out var board);
            if (!onMap) break;

            if (i != 0 || !ignoreSource){
                var laser = MapSystem.Instance.CreateObject(prefab, pos, board, false);
                laser.transform.rotation = source.transform.rotation;
                lasers.Add(laser);
            }
            i++;

            if (i >= maxDistance) break;
        } while (Interaction.CanMove(pos, dir));
        return lasers;
    }
}