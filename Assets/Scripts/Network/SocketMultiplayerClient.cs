using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using SocketIOClient.Transport;

public class SocketMultiplayerClient : MonoBehaviour
{
    [Header("Connection")]
    [SerializeField] private string _serverUrl = "http://127.0.0.1:3001";
    [SerializeField] private string _playerName = "Player";
    [SerializeField] private bool _connectOnStart = true;

    public string CurrentRoomId { get; private set; }
    public bool IsConnected => _socket != null && _socket.Connected;

    private SocketIOUnity _socket;

    private void Start()
    {
        if (_connectOnStart)
        {
            Connect();
        }
    }

    public async void Connect()
    {
        if (_socket != null && _socket.Connected)
        {
            return;
        }

        var uri = new Uri(_serverUrl);
        _socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            EIO = EngineIO.V4,
            Transport = TransportProtocol.WebSocket,
            Query = new Dictionary<string, string>
            {
                { "platform", "unity" }
            }
        });

        _socket.JsonSerializer = new NewtonsoftJsonSerializer();

        _socket.OnConnected += (_, _) => Debug.Log("[Socket] Connected");
        _socket.OnDisconnected += (_, reason) => Debug.Log($"[Socket] Disconnected: {reason}");

        _socket.OnUnityThread("room:created", response =>
        {
            var payload = response.GetValue().GetRawText();
            var json = JObject.Parse(payload);
            CurrentRoomId = json["roomId"]?.ToString() ?? string.Empty;
            Debug.Log($"[Socket] Room created: {CurrentRoomId}");
        });

        _socket.OnUnityThread("room:joined", response =>
        {
            var payload = response.GetValue().GetRawText();
            var json = JObject.Parse(payload);
            CurrentRoomId = json["roomId"]?.ToString() ?? string.Empty;
            Debug.Log($"[Socket] Joined room: {CurrentRoomId}");
        });

        _socket.OnUnityThread("game:state", response =>
        {
            var payload = response.GetValue().GetRawText();
            Debug.Log($"[Socket] Game state: {payload}");
            // TODO: map payload to your local grill model and update UI.
        });

        _socket.OnUnityThread("room:error", response =>
        {
            var payload = response.GetValue().GetRawText();
            Debug.LogWarning($"[Socket] Error: {payload}");
        });

        await _socket.ConnectAsync();
    }

    public void CreateRoom()
    {
        if (!IsConnected)
        {
            Debug.LogWarning("[Socket] Not connected");
            return;
        }

        _socket.Emit("room:create", new { playerName = _playerName });
    }

    public void JoinRoom(string roomId)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("[Socket] Not connected");
            return;
        }

        if (string.IsNullOrWhiteSpace(roomId))
        {
            Debug.LogWarning("[Socket] roomId is required");
            return;
        }

        _socket.Emit("room:join", new { roomId, playerName = _playerName });
    }

    public void SendMove(int from, int to)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("[Socket] Not connected");
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentRoomId))
        {
            Debug.LogWarning("[Socket] Join or create a room before sending moves");
            return;
        }

        _socket.Emit("game:move", new
        {
            roomId = CurrentRoomId,
            from,
            to
        });
    }

    public async void SyncState()
    {
        if (!IsConnected || string.IsNullOrWhiteSpace(CurrentRoomId))
        {
            return;
        }

        await _socket.EmitAsync("game:sync", CurrentRoomId);
    }

    private async void OnApplicationQuit()
    {
        if (_socket != null && _socket.Connected)
        {
            await _socket.DisconnectAsync();
        }

        _socket?.Dispose();
        _socket = null;
    }
}
