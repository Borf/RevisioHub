using RevisioHub.Common.Models;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace RevisioHub.Web.Services;

public class ServiceLogService : IDictionary<int, string>, INotifyCollectionChanged
{
    private Dictionary<int, string> status = new();

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public string this[int key]
    {
        get => status[key];
        set
        {
            status[key] = value;
            if(CollectionChanged != null)
                CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public ICollection<int> Keys => status.Keys;
    public ICollection<string> Values => status.Values;
    public int Count => ((ICollection<KeyValuePair<int, string>>)status).Count;
    public bool IsReadOnly => ((ICollection<KeyValuePair<int, string>>)status).IsReadOnly;
    public void Add(int key, string value)
    {
        status.Add(key, value);
        if (CollectionChanged != null)
            CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Clear()
    {
        status.Clear();
        if (CollectionChanged != null)
            CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(KeyValuePair<int, string> item) => status.Contains(item);
    public bool ContainsKey(int key) => status.ContainsKey(key);
    public IEnumerator<KeyValuePair<int, string>> GetEnumerator() => status.GetEnumerator();
    public bool Remove(int key)
    {
        if (CollectionChanged != null)
            CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        return status.Remove(key);
    }

    IEnumerator IEnumerable.GetEnumerator() => status.GetEnumerator();
    public bool TryGetValue(int key, [MaybeNullWhen(false)] out string value) => status.TryGetValue(key, out value);

    public void CopyTo(KeyValuePair<int, string>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<int, string>>)status).CopyTo(array, arrayIndex);
    }

    public void Add(KeyValuePair<int, string> item)
    {
        ((ICollection<KeyValuePair<int, string>>)status).Add(item);
        if (CollectionChanged != null)
            CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Remove(KeyValuePair<int, string> item)
    {
        if (CollectionChanged != null)
            CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        return ((ICollection<KeyValuePair<int, string>>)status).Remove(item);
    }

    public string GetValueOrDefault(int key, string defaultValue) => status.GetValueOrDefault(key, defaultValue);


}

