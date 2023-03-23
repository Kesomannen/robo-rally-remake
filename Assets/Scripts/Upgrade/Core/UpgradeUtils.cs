using System.Linq;

public static class UpgradeUtils {
    public static bool IsProgramming(Player player) {
        return GameSystem.CurrentPhase.Value == Phase.Programming 
               && !ProgrammingPhase.Instance.PlayersLockedIn.Contains(player);
    }
}