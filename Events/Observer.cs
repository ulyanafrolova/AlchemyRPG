namespace AlchemyRPG;

public interface IObserver<T>
{
    void OnNext(T data);
}

public interface ISubject<T>
{
    void Subscribe(IObserver<T> observer);
    void Unsubscribe(IObserver<T> observer);
    void Notify(T data);
}

public class Subject<T> : ISubject<T>
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
        for (int i = _observers.Count - 1; i >= 0; i--)
        {
            _observers[i].OnNext(data);
        }
    }
}