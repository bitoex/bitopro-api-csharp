using System.Text;
using System.Text.Json;
using WebSocket4Net;

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
            if (!string.IsNullOrEmpty(Account) && !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiSecret))
            {
                var parameter = new
                {
                    identity = Account,
                    nonce = Utils.GenerateTimeStamp(DateTime.Now)
                };
                var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(parameter)));
                var signature = Utils.GenerateSignature(ApiSecret, payload).ToLower();

                var customHeaders = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("X-BITOPRO-APIKEY", ApiKey),
                    new KeyValuePair<string, string>("X-BITOPRO-PAYLOAD", payload),
                    new KeyValuePair<string, string>("X-BITOPRO-SIGNATURE", signature)
                };

                WebSocket = new WebSocket(ConnectUrl, "", null, customHeaders, "", "", WebSocketVersion.None, null, System.Security.Authentication.SslProtocols.Tls12);
            }
            else
                WebSocket = new WebSocket(ConnectUrl);

            WebSocket.Opened += WebSocket_OnOpen;
            WebSocket.Closed += WebSocket_OnClose;
            WebSocket.MessageReceived += WebSocket_OnMessage;
            WebSocket.Error += WebSocket_OnError;
            WebSocket.Open();
        }

        protected void WebSocket_OnMessage(object? sender, MessageReceivedEventArgs e)
        {
            On_Receive_Message?.Invoke(e.Message);
        }

        protected void WebSocket_OnOpen(object? sender, EventArgs e)
        {
            Utils.Logger.Info($"{GetType().Name} connected server success");
        }

        protected void WebSocket_OnClose(object? sender, EventArgs e)
        {
            Utils.Logger.Info(e.ToString());
            Thread.Sleep(3000);
            Utils.Logger.Info($"{GetType().Name} closed connection, reconnecting...");

            InitWebsocket();
            WebSocket.Open();
        }

        protected void WebSocket_OnError(object? sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Utils.Logger.Error($"{GetType().Name}: {e.Exception.Message}");
        }
    }
}
