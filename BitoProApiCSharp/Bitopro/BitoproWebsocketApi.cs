using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using WebSocketSharp;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace Bitopro
{
    public delegate void OnReceiveMessageEvent(string msg);

    public class BitoproOrderBookWebsocket : BitoproWebsocketApi
    {
        public BitoproOrderBookWebsocket(Dictionary<string, int> symbolLimit) : base("", "", "")
        {
            ConnectUrl = BitoproWebsocketEndpoint + "/v1/pub/order-books/";
            foreach (var item in symbolLimit)
                ConnectUrl += $"{item.Key.ToLower()}:{item.Value},";
            ConnectUrl = ConnectUrl.TrimEnd(',');
        }
    }

    public class BitoproTickerWebsocket : BitoproWebsocketApi
    {
        public BitoproTickerWebsocket(List<string> symbols) : base("", "", "")
        {
            ConnectUrl = BitoproWebsocketEndpoint + "/v1/pub/tickers/";
            foreach (var symbol in symbols)
                ConnectUrl += $"{symbol.ToLower()},";
            ConnectUrl = ConnectUrl.TrimEnd(',');
        }
    }

    public class BitoproTradesWebsocket : BitoproWebsocketApi
    {
        public BitoproTradesWebsocket(List<string> symbols) : base("", "", "")
        {
            ConnectUrl = BitoproWebsocketEndpoint + "/v1/pub/trades/";
            foreach (var symbol in symbols)
                ConnectUrl += $"{symbol.ToLower()},";
            ConnectUrl.TrimEnd(',');
        }
    }

    public class BitoproUserOrdersWebsocket : BitoproWebsocketApi
    {
        public BitoproUserOrdersWebsocket(string account, string apiKey, string apiSecret) : base(account, apiKey, apiSecret)
        {
            ConnectUrl = BitoproWebsocketEndpoint + "/v1/pub/auth/orders";
        }
    }

    public class BitoproUserBlanceWebsocket : BitoproWebsocketApi
    {
        public BitoproUserBlanceWebsocket(string account, string apiKey, string apiSecret) : base(account, apiKey, apiSecret)
        {
            ConnectUrl = BitoproWebsocketEndpoint + "/v1/pub/auth/account-balance";
        }
    }

    public class BitoproUserTradeWebsocket : BitoproWebsocketApi
    {
        public BitoproUserTradeWebsocket(string account, string apiKey, string apiSecret) : base(account, apiKey, apiSecret)
        {
            ConnectUrl = BitoproWebsocketEndpoint + "/v1/pub/auth/user-trades";
        }
    }

    public class BitoproWebsocketApi
    {
        protected string BitoproWebsocketEndpoint = "wss://stream.bitopro.com:443/ws";
        protected string Account;
        protected string ApiKey;
        protected string ApiSecret;

        protected string ConnectUrl;
        protected WebSocket WebSocket { get; set; }
        public OnReceiveMessageEvent On_Receive_Message;
        public BitoproWebsocketApi(string account, string apiKey, string apiSecret)
        {
            Account = account;
            ApiKey = apiKey;
            ApiSecret = apiSecret;
        }

        public void InitWebsocket()
        {
            WebSocket = new WebSocket(ConnectUrl);
            WebSocket.OnOpen += WebSocket_OnOpen;
            WebSocket.OnClose += WebSocket_OnClose;
            WebSocket.OnMessage += WebSocket_OnMessage;
            WebSocket.OnError += WebSocket_OnError;

            WebSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            if (!string.IsNullOrEmpty(Account) && !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiSecret))
            {
                var parameter = new
                {
                    identity = Account,
                    nonce = Utils.GenerateTimeStamp(DateTime.Now)
                };
                var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parameter)));
                var signature = Utils.GenerateSignature(ApiSecret, payload).ToLower();

                WebSocket.CustomHeaders = new Dictionary<string, string>() 
                {
                    {"X-BITOPRO-APIKEY", ApiKey },
                    {"X-BITOPRO-PAYLOAD", payload},
                    {"X-BITOPRO-SIGNATURE", signature}
                };
            }
            
            WebSocket.Connect();
        }

        protected void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            On_Receive_Message?.Invoke(e.Data);
        }

        protected void WebSocket_OnOpen(object sender, EventArgs e)
        {
            Utils.Logger.Info($"{GetType().Name} connected server success");
        }

        protected void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            Utils.Logger.Info(e.Reason);
            Thread.Sleep(3000);
            Utils.Logger.Info($"{GetType().Name} closed connection, reconnecting...");

            InitWebsocket();
            WebSocket.Connect();
        }

        protected void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            Utils.Logger.Error($"{GetType().Name}: {e.Message}");
        }
    }
}
