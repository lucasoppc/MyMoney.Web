using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;

namespace MyMoney.Web.Services;

public class CacheService
{
    private Lazy<IJSObjectReference> _accessorJsRef = new();
    private readonly IJSRuntime _jsRuntime;
    private readonly IMemoryCache _memoryCache;
    private readonly Dictionary<string, List<Action<object?>>> _subscribers = new();


    public CacheService(IJSRuntime jsRuntime, IMemoryCache memoryCache)
    {
        _jsRuntime = jsRuntime;
        _memoryCache = memoryCache;
    }
    
    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new(await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/LocalStorageAccessor.js"));
        }
    }
    
    public async Task<T> GetValueAsync<T>(string key)
    {
        await WaitForReference();
        var result = await _accessorJsRef.Value.InvokeAsync<T>("get", key);

        return result;
    }

    public async Task SetValueAsync<T>(string key, T value)
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("set", key, value);
        NotifySubscribers(key, value);
    }

    public async Task Clear()
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("clear");
    }

    public async Task RemoveAsync(string key)
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("remove", key);
        NotifySubscribers(key, null);
    }
    
    public void Subscribe(string key, Action<object> callback)
    {
        if (!_subscribers.ContainsKey(key))
        {
            _subscribers[key] = new List<Action<object>>();
        }
        _subscribers[key].Add(callback);
    }

    public void Unsubscribe(string key, Action<object> callback)
    {
        if (_subscribers.ContainsKey(key))
        {
            _subscribers[key].Remove(callback);
            if (_subscribers[key].Count == 0)
            {
                _subscribers.Remove(key);
            }
        }
    }

    private void NotifySubscribers(string key, object? value)
    {
        if (_subscribers.ContainsKey(key))
        {
            foreach (var callback in _subscribers[key])
            {
                callback(value);
            }
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }
}