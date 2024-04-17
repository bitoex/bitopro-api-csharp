using Bitopro;
using System.Text.Json.Nodes;

namespace BitoProApiCSharp
{
    internal class Program
    {
        const string LOG_CONFIG = "LogConfig.xml";
        private static string _orderId = "";
        private const string SYMBOL = "BTC_USDT";
        private const string ACCOUNT = "";
        private const string API_KEY = "";
        private const string API_SECRET = "";

        static async Task Main(string[] args)
        {
            var logPath = $"../../../{LOG_CONFIG}";
            if (File.Exists(logPath))
            {
                File.Copy(logPath, LOG_CONFIG, true);
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(LOG_CONFIG));
            }

            //await RestfulTest();
            WebsocketTest();

            while (true)
                Thread.Sleep(10);
        }

        private static async Task RestfulTest()
        {
            JsonObject response;
            var bitopro = new BitoproRestApi(API_KEY, API_SECRET);

            #region "Open restful test"
            // [GET] list of currencies
            response = await bitopro.GetCurrencies();
            if (response != null)
                Utils.Logger.Info($"List of currencies: {response}\r\n");

            // [GET] limitations and fees
            response = await bitopro.GetLimitationsAndFees();
            if (response != null)
                Utils.Logger.Info($"Limitations And Fees: {response}\r\n");

            // [GET] order book
            response = await bitopro.GetOrderBook(SYMBOL);
            if (response != null)
                Utils.Logger.Info($"Order book: {response}\r\n");

            // [GET] tickers
            response = await bitopro.GetTicker(SYMBOL);
            if (response != null)
                Utils.Logger.Info($"Tickers: {response}\r\n");

            // [GET] trades
            response = await bitopro.GetTrades(SYMBOL);
            if (response != null)
                Utils.Logger.Info($"Trades: {response}\r\n");

            // [GET] candlestick
            response = await bitopro.GetCandlestick(new DateTime(2022, 4, 23, 17, 50, 15), new DateTime(2023, 3, 9, 17, 50, 15), SYMBOL);
            if (response != null)
                Utils.Logger.Info($"Candlestick: {response}\r\n");

            // [GET] trading pairs
            response = await bitopro.GetTradingPairs();
            if (response != null)
                Utils.Logger.Info($"Trading pairs: {response}\r\n");
            #endregion

            #region "Auth restful test"
            // [GET] account balance
            response = await bitopro.GetAccountBalance();
            if (response != null)
                Utils.Logger.Info($"Account balance: {response}\r\n");

            // [POST] Create a limit order
            response = await bitopro.CreateAnOrder(OrderAction.BUY, 0.0001, 10500, OrderType.Limit, SYMBOL);
            if (response != null)
            {
                if (response["orderId"] != null)
                    _orderId = (string)response["orderId"];
                Utils.Logger.Info($"Limit order created: {response}\r\n");
            }

            // [GET] Get an order
            response = await bitopro.GetAnOrder(SYMBOL, _orderId);
            if (response != null)
                Utils.Logger.Info($"Get an order: {response}\r\n");

            // [DELETE] Cancel the order
            response = await bitopro.CancelAnOrder(_orderId, SYMBOL);
            if (response != null)
                Utils.Logger.Info($"Order cancelled: {response}\r\n");

            // [GET] Get all orders
            response = await bitopro.GetAllOrders(SYMBOL);
            if (response != null)
                Utils.Logger.Info($"All orders: {response}\r\n");

            // [GET] Get all trades
            response = await bitopro.GetTradesList(SYMBOL);
            if (response != null)
                Utils.Logger.Info($"All trades: {response}\r\n");

            // [POST] Create batch order, create 10 orders at once
            var batchOrders = new Dictionary<string, List<object>>();
            batchOrders.Add(SYMBOL, new List<object>());
            batchOrders[SYMBOL].Add(new { pair = SYMBOL, action = Enum.GetName(typeof(OrderAction), OrderAction.BUY), amount = $"{0.0001}", price = $"{10500}", timestamp = Utils.GenerateTimeStamp(DateTime.Now), type = OrderType.Limit.ToString() });
            batchOrders[SYMBOL].Add(new { pair = SYMBOL, action = Enum.GetName(typeof(OrderAction), OrderAction.BUY), amount = $"{0.0001}", price = $"{10500}", timestamp = Utils.GenerateTimeStamp(DateTime.Now), type = OrderType.Limit.ToString() });
            batchOrders[SYMBOL].Add(new { pair = SYMBOL, action = Enum.GetName(typeof(OrderAction), OrderAction.BUY), amount = $"{0.0001}", price = $"{10500}", timestamp = Utils.GenerateTimeStamp(DateTime.Now), type = OrderType.Limit.ToString() });

            response = await bitopro.CreateBatchOrder(batchOrders[SYMBOL]);
            if (response != null)
            {
                if (response["data"] != null)
                {
                    JsonArray jsonArray = (JsonArray)response["data"];
                    batchOrders[SYMBOL].Clear();
                    foreach (var item in jsonArray)
                        batchOrders[SYMBOL].Add((string)item["orderId"]);
                }
                Utils.Logger.Info($"Batch orders created: {response}\r\n");
            }

            // [PUT] Cancel multiple orders
            response = await bitopro.CancelMultipleOrders(batchOrders);
            if (response != null)
                Utils.Logger.Info($"Cancel multiple orders: {response}\r\n");

            // [DELETE] Cancel all order
            response = await bitopro.CancelAllOrders(SYMBOL);
            if (response != null)
                Utils.Logger.Info($"Cancel all orders: {response}\r\n");

            // [GET] Deposit history
            response = await bitopro.GetDepositHistory("USDT");
            if (response != null)
                Utils.Logger.Info($"Deposit history: {response}\r\n");

            // [GET] Withdraw history
            response = await bitopro.GetWithdrawHistory("USDT");
            if (response != null)
                Utils.Logger.Info($"Withdraw history: {response}\r\n");

            // [GET] GET Withdraw
            response = await bitopro.GetWithdraw("USDT", "");
            if (response != null)
                Utils.Logger.Info($"Withdraw infomation: {response}\r\n");

            // [POST] Withdraw
            response = await bitopro.Withdraw("USDT", WithdrawProtocol.ERC20, "", 0.1, "");
            if (response != null)
                Utils.Logger.Info($"Withdraw: {response}\r\n");
            #endregion
        }

        private static void WebsocketTest()
        {
            #region Open websocket test
            var symbolsLimit = new Dictionary<string, int> { { "eth_btc", 5 }, { "BTC_TWD", 1 }, { "ETH_TWD", 20 }, { "BITO_ETH", 1 } };
            var symbols = new List<string> { "BTC_TWD", "ETH_TWD", "BITO_ETH" };

            // [PUBLIC] GET Order book
            var bitoproWsOrderBook = new BitoproOrderBookWebsocket(symbolsLimit);
            bitoproWsOrderBook.On_Receive_Message += Receive_WsMessage;
            bitoproWsOrderBook.InitWebsocket();

            // [PUBLIC] GET Ticker
            var bitoproWsTicker = new BitoproTickerWebsocket(symbols);
            bitoproWsTicker.On_Receive_Message += Receive_WsMessage;
            bitoproWsTicker.InitWebsocket();

            // [PUBLIC] GET Trade
            var bitoproWsTrades = new BitoproTradesWebsocket(symbols);
            bitoproWsTrades.On_Receive_Message += Receive_WsMessage;
            bitoproWsTrades.InitWebsocket();

            #endregion

            #region Auth websocket test
            // [Private] GET account balance
            var bitoproWsUserBalance = new BitoproUserBlanceWebsocket(ACCOUNT, API_KEY, API_SECRET);
            bitoproWsUserBalance.On_Receive_Message += Receive_WsMessage;
            bitoproWsUserBalance.InitWebsocket();

            // [Private] GET user order
            var bitoproWsUserOrder = new BitoproUserOrdersWebsocket(ACCOUNT, API_KEY, API_SECRET);
            bitoproWsUserOrder.On_Receive_Message += Receive_WsMessage;
            bitoproWsUserOrder.InitWebsocket();

            //// [Private] GET user trade
            var bitoproWsUserTrade = new BitoproUserTradeWebsocket(ACCOUNT, API_KEY, API_SECRET);
            bitoproWsUserTrade.On_Receive_Message += Receive_WsMessage;
            bitoproWsUserTrade.InitWebsocket();

            #endregion
        }

        static void Receive_WsMessage(string message)
        {
            Utils.Logger.Info(message);
        }
    }
}