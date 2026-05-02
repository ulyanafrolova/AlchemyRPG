namespace AlchemyRPG;

public static class WeaponExtensions
{
    public static void TriggerPickUpEffects(this IWeapon weapon, GameState state)
    {
        GameLogger.Instance.Log(LogType.Loot, $"{state.Player.Name} picked up {weapon.Name} (Noise generated: {weapon.NoiseRange})");
        state.Log = $"Picked up weapon: {weapon.Name}";
        if (weapon.NoiseRange > 0)
        {
            var acousticMap = AcousticSystem.CalculateAcousticDistances(state.Map, state.Player.X, state.Player.Y, weapon.NoiseRange);
            var noiseData = new NoiseData(state.Player.X, state.Player.Y, acousticMap);
            state.NoiseEvents.Notify(noiseData);
        }
    }
}