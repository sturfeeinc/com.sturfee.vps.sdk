using Newtonsoft.Json;
using SturfeeVPS.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    public delegate void SocketOpenDelegate();
    public delegate void SocketCloseDelegate(string reason);
    public delegate void SocketErrorDelegate(string error);
    public delegate void SocketReceiveDelegate();

    /// <summary>
    /// Network interface for VPS localization services
    /// </summary>
    public class LocalizationService
    {        
        public event SocketOpenDelegate OnSocketOpen;
        public event SocketCloseDelegate OnSocketClose;
        public event SocketErrorDelegate OnSocketError;
        public event SocketReceiveDelegate OnSocketReceive;

        private WebSocketSharp.WebSocket _socket;
        private Queue<byte[]> _messages = new Queue<byte[]> ();

        public bool IsConnected { get; private set; }

        public async Task Connect(string url, string token, double latitude, double longitude, string language = "en-US")
        {
            SturfeeDebug.Log($" [LocalizationService] :: Opening socket connection at {url}");
            var headers = new Dictionary<string, string>
            {
                {"Authorization", "Bearer " + token},
                {"latitude", latitude.ToString()},
                {"longitude", longitude.ToString()},
                {"Accept-Language", language}
            };
            SturfeeDebug.Log($" [LocalizationService] :: headers = > {JsonConvert.SerializeObject(headers)}");

            _socket = new WebSocketSharp.WebSocket(url);
            _socket.CustomHeaders = headers;
            _socket.OnOpen += OnOpen;
            _socket.OnMessage += OnMessage;
            _socket.OnError += OnError;
            _socket.OnClose += OnClose;

            _socket.ConnectAsync();

            while(!IsConnected) await Task.Yield ();
        }

        public void Send(string message, Action<bool> success)
        {
            _socket.SendAsync(message, success);
        }

        public void Send(byte[] buffer, Action<bool> success)
        {
            _socket.SendAsync(buffer, success);
        }

        public byte[] Recv()
        {
            if (_messages.Count == 0)
                return null;
            return _messages.Dequeue();
        }

        public string RecvString()
        {
            byte[] retval = Recv();
            if (retval == null)
                return null;
            return Encoding.UTF8.GetString(retval);
        }

        public void Disconnect(WebSocketSharp.CloseStatusCode closeCode = WebSocketSharp.CloseStatusCode.Normal)
        {
            SturfeeDebug.Log($" [LocalizationService] :: Disconneting socket connection with code {closeCode}");
            _socket?.Close(closeCode);
        }

        private void OnOpen(object sender, System.EventArgs e)
        {
            SturfeeDebug.Log($" [LocalizationService] :: Socket connection open");
            IsConnected = true;

            Dispatcher.RunOnMainThread(() => OnSocketOpen?.Invoke());            
        }

        private void OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            SturfeeDebug.Log($" [LocalizationService] :: Socket connection recieved a message..");
            _messages.Enqueue(e.RawData);

            Dispatcher.RunOnMainThread(() => OnSocketReceive?.Invoke());            
        }

        private void OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Debug.LogException(e.Exception);
            SturfeeDebug.Log($" [LocalizationService] :: Socket connection errorred out. Reason => {e.Message}");
            
            Dispatcher.RunOnMainThread(() => OnSocketError?.Invoke(e.Message));
        }

        private void OnClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
            SturfeeDebug.Log($" [LocalizationService] :: Socket connection closed. Reason => {e.Reason}");
            IsConnected = false;
            Debug.Log(e.Code);
            if (e.Code != (ushort)WebSocketSharp.CloseStatusCode.Normal)
            {
                Dispatcher.RunOnMainThread(() => OnSocketClose?.Invoke(e.Reason));
            }
        }

    }
}
