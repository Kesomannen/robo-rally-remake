using UnityEngine;
using System;

public class NetworkSystem : Singleton<NetworkSystem> {
    public static Player LocalPlayer => PlayerManager.Players[0];
}