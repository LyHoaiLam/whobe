using System.Collections.Concurrent;

namespace whobe.Hubs;

public static class UserConnectionManager {
    private static readonly ConcurrentDictionary<string, HashSet<string>> Connections = new();

    public static void Add(string userId, string connectionId) {
        var connections = Connections.GetOrAdd(
            userId,
            _ => new HashSet<string>()
        );

        lock (connections) {
            connections.Add(connectionId);
        }
    }


    public static void Remove(string userId, string connectionId) {
        if (!Connections.TryGetValue(userId, out var connections)) {
            return;
        }

        lock (connections) {
            connections.Remove(connectionId);

            if (connections.Count == 0) {
                Connections.TryRemove(userId, out _);
            }
        }
    }


    public static bool IsOnline(string userId) {
        return Connections.TryGetValue(userId, out var connections) && connections.Count > 0;
    }


    public static string[] GetOnlineUsers() {
        return Connections.Keys.ToArray();
    }
}