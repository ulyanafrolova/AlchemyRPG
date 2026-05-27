using System.Collections.Generic;

namespace AlchemyRPG;

/// <summary>
/// Defines a provider for receiving push-based notifications.
/// Implements the Observer side of the Observer design pattern.
/// </summary>
/// <typeparam name="T">The type of the event data being observed.</typeparam>
public interface IObserver<T>
{
    /// <summary>
    /// Provides the observer with new data.
    /// </summary>
    /// <param name="data">The current notification information.</param>
    void OnNext(T data);
}

/// <summary>
/// Defines a provider that pushes notifications to subscribed observers.
/// Implements the Subject side of the Observer design pattern.
/// </summary>
/// <typeparam name="T">The type of the event data being broadcasted.</typeparam>
public interface ISubject<T>
{
    /// <summary>
    /// Subscribes an observer to receive notifications.
    /// </summary>
    void Subscribe(IObserver<T> observer);

    /// <summary>
    /// Unsubscribes an observer, stopping further notifications.
    /// </summary>
    void Unsubscribe(IObserver<T> observer);

    /// <summary>
    /// Broadcasts the event data to all currently subscribed observers.
    /// </summary>
    void Notify(T data);
}

/// <summary>
/// A thread-safe, generic implementation of the Observer pattern's Subject.
/// Utilizes a Copy-On-Write (COW) strategy to allow lock-free, concurrent notifications 
/// while safely handling dynamic subscriptions in a multi-threaded server environment.
/// </summary>
/// <typeparam name="T">The specific event data type payload.</typeparam>
public class Subject<T> : ISubject<T>
{
    /// <summary>
    /// A volatile reference to an immutable list of observers. 
    /// Ensures that reads during the Notify phase do not require a lock.
    /// </summary>
    private volatile IReadOnlyList<IObserver<T>> _observers = new List<IObserver<T>>();
    
    /// <summary>
    /// A dedicated synchronization root for safely modifying the observer list.
    /// </summary>
    private readonly object _writeLock = new();

    /// <summary>
    /// Safely subscribes an observer using a Copy-On-Write mechanism.
    /// </summary>
    /// <param name="observer">The observer to attach.</param>
    public void Subscribe(IObserver<T> observer)
    {
        lock (_writeLock)
        {
            var updated = new List<IObserver<T>>(_observers);
            if (!updated.Contains(observer))
                updated.Add(observer);
            
            // Atomic pointer swap
            _observers = updated;
        }
    }

    /// <summary>
    /// Safely unsubscribes an observer using a Copy-On-Write mechanism.
    /// </summary>
    /// <param name="observer">The observer to detach.</param>
    public void Unsubscribe(IObserver<T> observer)
    {
        lock (_writeLock)
        {
            var updated = new List<IObserver<T>>(_observers);
            updated.Remove(observer);
            
            // Atomic pointer swap
            _observers = updated;
        }
    }

    /// <summary>
    /// Broadcasts data to all observers lock-free. Captures a snapshot of the active observers 
    /// at the moment of invocation to prevent exceptions if the list is modified mid-broadcast.
    /// Iterates backward to safely handle structural evaluations.
    /// </summary>
    /// <param name="data">The event payload to distribute.</param>
    public void Notify(T data)
    {
        var snapshot = _observers;
        for (int i = snapshot.Count - 1; i >= 0; i--)
        {
            snapshot[i].OnNext(data);
        }
    }
}