namespace AlchemyRPG;

public interface IObserver<T>
{
    void OnNext(T data);
}

public class Subject<T>
{
    private readonly List<IObserver<T>> _observers = new();

    public void Subscribe(IObserver<T> observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void Unsubscribe(IObserver<T> observer)
    {
        _observers.Remove(observer);
    }

    public void Notify(T data)
    {
        foreach (var observer in _observers.ToList()) 
        {
            observer.OnNext(data);
        }
    }
}