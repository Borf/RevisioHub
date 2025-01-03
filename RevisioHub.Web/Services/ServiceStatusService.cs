﻿using RevisioHub.Common.Models;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace RevisioHub.Web.Services;

public class ServiceStatusService : IDictionary<int, ServiceStatus>, INotifyCollectionChanged
{
    private Dictionary<int, ServiceStatus> status = new();

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public ServiceStatus this[int key]
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
    public ICollection<ServiceStatus> Values => status.Values;
    public int Count => ((ICollection<KeyValuePair<int, string>>)status).Count;
    public bool IsReadOnly => ((ICollection<KeyValuePair<int, string>>)status).IsReadOnly;
    public void Add(int key, ServiceStatus value)
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

    public bool Contains(KeyValuePair<int, ServiceStatus> item) => status.Contains(item);
    public bool ContainsKey(int key) => status.ContainsKey(key);
    public IEnumerator<KeyValuePair<int, ServiceStatus>> GetEnumerator() => status.GetEnumerator();
    public bool Remove(int key)
    {
        if (CollectionChanged != null)
            CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        return status.Remove(key);
    }

    IEnumerator IEnumerable.GetEnumerator() => status.GetEnumerator();
    public bool TryGetValue(int key, [MaybeNullWhen(false)] out ServiceStatus value) => status.TryGetValue(key, out value);

    public void CopyTo(KeyValuePair<int, ServiceStatus>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<int, ServiceStatus>>)status).CopyTo(array, arrayIndex);
    }

    public void Add(KeyValuePair<int, ServiceStatus> item)
    {
        ((ICollection<KeyValuePair<int, ServiceStatus>>)status).Add(item);
        if (CollectionChanged != null)
            CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Remove(KeyValuePair<int, ServiceStatus> item)
    {
        if (CollectionChanged != null)
            CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        return ((ICollection<KeyValuePair<int, ServiceStatus>>)status).Remove(item);
    }

    public ServiceStatus GetValueOrDefault(int key, ServiceStatus defaultValue) => status.GetValueOrDefault(key, defaultValue);


}

