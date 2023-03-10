using System.Linq;

public static class UpgradeUtils {
    public static bool IsProgramming(Player player) {
        return PhaseSystem.Current.Value == Phase.Programming 
               && !ProgrammingPhase.PlayersLockedIn.Contains(player);
    }
}