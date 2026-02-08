using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using RelationshipService.Domain.Models;

namespace RelationshipWebSocketServer.Services
{
    public interface IClientConnectionManager
    {
        void AddClient(SocketClient client);
        void RemoveClient(string connectionId);
        SocketClient? GetClient(string connectionId);
        IEnumerable<SocketClient> GetClientsByProfileId(long profileId);
        void UpdateMatches(long profileId, List<MatchSyncItem> matches);
        void RemoveMatch(long profileId, long matchedProfileId);
        (bool IsMatched, long MatchId) GetMatchInfo(long profileId, long targetProfileId);
    }

    public class ClientConnectionManager : IClientConnectionManager
    {
        // Key: ConnectionId, Value: SocketClient details
        private readonly ConcurrentDictionary<string, SocketClient> _clients = new();

        // Key: ProfileId, Value: Set of ConnectionIds (to handle multi-tab/device)
        private readonly ConcurrentDictionary<long, ConcurrentHashSet<string>> _profileConnections = new();

        public void AddClient(SocketClient client)
        {
            _clients[client.ConnectionId] = client;
            _profileConnections.GetOrAdd(client.ProfileId, _ => new ConcurrentHashSet<string>()).Add(client.ConnectionId);
        }

        public void RemoveClient(string connectionId)
        {
            if (_clients.TryRemove(connectionId, out var client))
            {
                if (_profileConnections.TryGetValue(client.ProfileId, out var connections))
                {
                    connections.Remove(connectionId);
                    if (connections.IsEmpty)
                    {
                        _profileConnections.TryRemove(client.ProfileId, out _);
                    }
                }
            }
        }

        public SocketClient? GetClient(string connectionId)
        {
            return _clients.TryGetValue(connectionId, out var client) ? client : null;
        }

        public IEnumerable<SocketClient> GetClientsByProfileId(long profileId)
        {
            if (_profileConnections.TryGetValue(profileId, out var connections))
            {
                return connections.ToList().Select(id => _clients[id]).Where(c => c != null)!;
            }
            return Enumerable.Empty<SocketClient>();
        }

        public void UpdateMatches(long profileId, List<MatchSyncItem> matches)
        {
            // Update all active connections for this profile
            if (_profileConnections.TryGetValue(profileId, out var connections))
            {
                foreach (var connId in connections.ToList())
                {
                    if (_clients.TryGetValue(connId, out var client))
                    {
                        client.Matches = matches;
                    }
                }
            }
        }

        public void RemoveMatch(long profileId, long matchedProfileId)
        {
            if (_profileConnections.TryGetValue(profileId, out var connections))
            {
                foreach (var connId in connections.ToList())
                {
                    if (_clients.TryGetValue(connId, out var client))
                    {
                        client.Matches.RemoveAll(m => m.MatchedProfileId == matchedProfileId);
                    }
                }
            }
        }

        public (bool IsMatched, long MatchId) GetMatchInfo(long profileId, long targetProfileId)
        {
            if (_profileConnections.TryGetValue(profileId, out var connections))
            {
                var firstConnId = connections.FirstOrDefault();
                if (firstConnId != null && _clients.TryGetValue(firstConnId, out var client))
                {
                    var match = client.Matches.FirstOrDefault(m => m.MatchedProfileId == targetProfileId);
                    if (match != null)
                    {
                        return (true, match.MatchId);
                    }
                }
            }
            return (false, 0);
        }
    }

    // Small helper for concurrent set
    public class ConcurrentHashSet<T> : System.Collections.Generic.IEnumerable<T> where T : notnull
    {
        private readonly ConcurrentDictionary<T, byte> _dict = new();
        public bool Add(T item) => _dict.TryAdd(item, 0);
        public bool Remove(T item) => _dict.TryRemove(item, out _);
        public bool IsEmpty => _dict.IsEmpty;
        public List<T> ToList() => _dict.Keys.ToList();
        public System.Collections.Generic.IEnumerator<T> GetEnumerator() => _dict.Keys.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        public T? FirstOrDefault() => _dict.Keys.FirstOrDefault();
        public int Count => _dict.Count;
    }
}
