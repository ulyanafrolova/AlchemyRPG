namespace AlchemyRPG;

/// <summary>
/// The Observer interface. 
/// All classes that want to react to events must implement this.
/// </summary>
public interface IEventListener<TEvent>
{
    void OnEvent(TEvent eventData);
}

public interface INoiseListener : IEventListener<NoiseData> { }
public interface IEnemyDeathListener : IEventListener<EnemyDeathData> { }