using UnityEngine;

public class NetworkSystem : Singleton<NetworkSystem> {
    public static Player LocalPlayer => PlayerManager.Players[0];
}