using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace Bitopro
{
    public class BitoproRestApi
    {
        private string _apiKey;
        private string _apiKeySecret;

        private HttpContent _httpContent;
        private object _headerPayload;
        private HttpClient _httpClient;
        public BitoproRestApi(string apiKey = "", string apiSecret = "")
        {
            _apiKey = apiKey;
            _apiKeySecret = apiSecret;
            _httpClient = new HttpClient { BaseAddress = new Uri("https://api.bitopro.com/v3") };
        }

        #region "OPEN"

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/public/get_currency_info.md#get-currency-info
        /// </summary>
        /// <returns>the list of currencies</returns>
        public async Task<dynamic> GetCurrencies()
        {
            return await SendRequestAsync("/provisioning/currencies", ApiMethod.GET, null, false);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/public/get_limitations_and_fees.md
        /// </summary>
        /// <returns>the limitations and fees</returns>
        public async Task<dynamic> GetLimitationsAndFees()
        {
            return await SendRequestAsync("/provisioning/limitations-and-fees", ApiMethod.GET, null, false);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/public/get_orderbook_data.md
        /// </summary>
        /// <param name="pair">the trading pair in format</param>
        /// <param name="limit">the limit for the response</param>
        /// <param name="scale">scale for the response. Valid scale values are different by pair</param>
        /// <returns>the full order book of the specific pair</returns>
        public async Task<dynamic> GetOrderBook(string pair = "", int limit = 5, int scale = 0)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "limit",$"{limit}"},
                { "scale",$"{scale}"}
            };
            return await SendRequestAsync($"/order-book/{pair}", ApiMethod.GET, parameters, false);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/public/get_ticker_data.md
        /// </summary>
        /// <param name="pair">the trading pair in format</param>
        /// <returns>the ticker is a high level overview of the state of the market.</returns>
        public async Task<dynamic> GetTicker(string pair = "")
        {
            return await SendRequestAsync($"/tickers/{pair}", ApiMethod.GET, null, false);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/public/get_trades_data.md
        /// </summary>
        /// <param name="pair">the trading pair in format</param>
        /// <returns>a list of the most recent trades for the given symbol</returns>
        public async Task<dynamic> GetTrades(string pair = "")
        {
            return await SendRequestAsync(
                $"/trades/{pair}", ApiMethod.GET, null, false);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/public/get_ohlc_data.md
        /// </summary>
        /// <param name="startDatetime">start time</param>
        /// <param name="endDateTime">end time</param>
        /// <param name="pair">the trading pair in format</param>
        /// <param name="resolution">the timeframe of the candlestick chart</param>
        /// <returns></returns>
        public async Task<dynamic> GetCandlestick(DateTime startDatetime, DateTime endDateTime, string pair = "", CandlestickResolutin resolution = CandlestickResolutin._1d)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "resolution",$"{resolution.ToString().Replace("_","")}"},
                { "from",$"{Utils.GenerateTimeStamp(startDatetime)/1000}"},
                { "to",$"{Utils.GenerateTimeStamp(endDateTime)/1000}"}
            };
            return await SendRequestAsync($"/trading-history/{pair}", ApiMethod.GET, parameters, false);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/public/get_trading_pair_info.md
        /// </summary>
        /// <returns>a list of pairs available for trade</returns>
        public async Task<dynamic> GetTradingPairs()
        {
            return await SendRequestAsync("/provisioning/trading-pairs", ApiMethod.GET, null, false);
        }

        #endregion

        #region "Auth"
        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/get_account_balance.md
        /// </summary>
        /// <returns>the account balance</returns>
        public async Task<dynamic> GetAccountBalance()
        {
            _headerPayload = new
            {
                identity = "",
                nonce = Utils.GenerateTimeStamp(DateTime.Now)
            };
            return await SendRequestAsync("/accounts/balance", ApiMethod.GET, null, true);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/cancel_an_order.md
        /// </summary>
        /// <param name="orderId">the trading pair in format</param>
        /// <param name="pair">the id of the order</param>
        /// <returns></returns>
        public async Task<dynamic> CancelAnOrder(string orderId, string pair)
        {
            _headerPayload = new
            {
                identity = "",
                nonce = Utils.GenerateTimeStamp(DateTime.Now)
            };
            return await SendRequestAsync($"/orders/{pair}/{orderId}", ApiMethod.DELETE, null, true);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/cancel_all_orders.md
        /// </summary>
        /// <param name="pair">the trading pair in format</param>
        /// <returns>cancel all your active orders of all pairs</returns>
        public async Task<dynamic> CancelAllOrders(string pair)
        {
            _headerPayload = new
            {
                identity = "",
                nonce = Utils.GenerateTimeStamp(DateTime.Now)
            };
            return await SendRequestAsync($"/orders/{pair}", ApiMethod.DELETE, null, true);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/cancel_batch_orders.md
        /// </summary>
        /// <param name="orders_request">send a json format request to cancel multiple orders at a time. example: {"BTC_USDT": ["12234566","12234567"],"ETH_USDT": ["44566712","24552212"]}</param>
        /// <returns>multiple orders will be canceled</returns>
        public async Task<dynamic> CancelMultipleOrders(Dictionary<string, List<object>> ordersRequest)
        {
            _headerPayload = ordersRequest;

            string json = JsonConvert.SerializeObject(ordersRequest);
            _httpContent = new StringContent(json, Encoding.Default, "application/json");

            return await SendRequestAsync("/orders/", ApiMethod.PUT, null, true);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/create_an_order.md
        /// </summary>
        /// <param name="action">the action type of the order</param>
        /// <param name="amount">the amount of the order for the trading pair, please follow the link to see the limitations</param>
        /// <param name="price">the price of the order for the trading pair</param>
        /// <param name="orderType">the order type</param>
        /// <param name="pair">the trading pair in format</param>
        /// <param name="stopPrice">the price to trigger stop limit order, only required when type is STOP_LIMIT</param>
        /// <param name="condition">the condition to match stop price, only required when type is STOP_LIMIT(>=, <=). Example: "<="</param>
        /// <param name="timeInForce">time in force condition for orders. If type is MARKET, this will always be GTC</param>
        /// <param name="clientId">this information help users distinguish their orders</param>
        /// <returns>a dict contains an order info</returns>
        public async Task<dynamic> CreateAnOrder(OrderAction actionData, double amountData, double priceData, OrderType orderType, string pair, double? stopPriceData = null, string conditionData = "", TimeInForce timeInForceData = TimeInForce.GTC, int? clientIdData = null)
        {
            _headerPayload = new
            {
                action = actionData.ToString(),
                amount = amountData.ToString(),
                price = priceData.ToString(),
                timestamp = Utils.GenerateTimeStamp(DateTime.Now),
                type = orderType.ToString(),
                timeInForce = timeInForceData.ToString(),
                condition = string.IsNullOrEmpty(conditionData) ? "" : conditionData,
                stopPrice = stopPriceData == null ? "" : stopPriceData.ToString(),
                clientId = clientIdData == null ? 0 : clientIdData,
            };

            string json = JsonConvert.SerializeObject(_headerPayload);
            _httpContent = new StringContent(json, Encoding.Default, "application/json");

            return await SendRequestAsync($"/orders/{pair}", ApiMethod.POST, null, true);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/create_batch_orders.md
        /// </summary>
        /// <param name="ordersRequest">multiple orders will be created
        ///example:
        ///[
        /// {
        ///     pair: "BTC_TWD",
        ///     action: "BUY",
        ///     type: "LIMIT",
        ///     price: "210000",
        ///     amount: "1",
        ///     timestamp: 1504262258000,
        ///     timeInForce: "GTC",
        /// }, 
        /// {
        ///     pair: "BTC_TWD",
        ///     action: "SELL",
        ///     type: "MARKET",
        ///     amount: "2",
        ///     timestamp: 1504262258000
        /// }
        ///]
        /// </param>
        /// <returns></returns>
        public async Task<dynamic> CreateBatchOrder(List<object> ordersRequest)
        {
            _headerPayload = ordersRequest;

            string json = JsonConvert.SerializeObject(ordersRequest);
            _httpContent = new StringContent(json, Encoding.Default, "application/json");

            return await SendRequestAsync("/orders/batch", ApiMethod.POST, null, true);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/get_orders_data.md
        /// </summary>
        /// <param name="pair">the trading pair in format</param>
        /// <param name="startDatetime">start time</param>
        /// <param name="endDatetime">end time</param>
        /// <param name="ignoreTimeLimit">this parameter can only be used with the query orders with OPEN status. If set to true, it will respond with all open orders without a time range limitation</param>
        /// <param name="statusKind">filter order based on status kind, OPEN, DONE, ALL</param>
        /// <param name="status">filter order base on specific status</param>
        /// <param name="limit">the number of records to retrieve</param>
        /// <param name="orderId">if specified, list starts with order with id >= orderId</param>
        /// <param name="clientId">this information help users distinguish their orders</param>
        /// <returns></returns>
        public async Task<dynamic> GetAllOrders(string pair, DateTime? startDatetime=null, DateTime? endDatetime = null, bool ignoreTimeLimit = false, StatusKind statusKind= StatusKind.ALL, OrderStatus? status=null, int limit = 100, string orderId = null, int? clientId = null)
        {
            var parameter = new Dictionary<string, object>()
            {
                { "ignoreTimeLimitEnable",ignoreTimeLimit},
                { "statusKind",$"{statusKind}"},
                { "limit",$"{limit}"},
            };

            if (startDatetime != null)
                parameter.Add("startTimestamp", Utils.GenerateTimeStamp(startDatetime.Value));
            if (endDatetime != null)
                parameter.Add("endTimestamp", Utils.GenerateTimeStamp(endDatetime.Value));
            if (status != null)
                parameter.Add("status", status);
            if (orderId != null)
                parameter.Add("orderId", orderId);
            if (clientId != null)
                parameter.Add("clientId", clientId);

            _headerPayload = new
            {
                identity = "",
                nonce = Utils.GenerateTimeStamp(DateTime.Now)
            };

            return await SendRequestAsync($"/orders/all/{pair}", ApiMethod.GET, parameter, true);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/get_an_order_data.md
        /// </summary>
        /// <param name="pair">the trading pair in format</param>
        /// <param name="orderId">the id of the order</param>
        /// <returns>an order infomation</returns>
        public async Task<dynamic> GetAnOrder(string pair, string orderId)
        {
            _headerPayload = new
            {
                identity = "",
                nonce = Utils.GenerateTimeStamp(DateTime.Now)
            };

            return await SendRequestAsync($"/orders/{pair}/{orderId}", ApiMethod.GET, null, true);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/get_trades_data.md
        /// </summary>
        /// <param name="pair">the trading pair in format</param>
        /// <param name="startDatetime">start time</param>
        /// <param name="endDatetime">end time</param>
        /// <param name="orderId">the id of the order</param>
        /// <param name="tradeId">the id of the first trade in the response</param>
        /// <param name="limit">the limit for the response</param>
        /// <returns>the all trades</returns>
        public async Task<dynamic> GetTradesList(string pair, DateTime? startDatetime=null, DateTime? endDatetime = null, string orderId=null, string tradeId = null, int limit = 100)
        {
            var parameter = new Dictionary<string, object>()
            {
                { "limit",$"{limit}"},
            };
            if (startDatetime != null)
                parameter.Add("startTimestamp", Utils.GenerateTimeStamp(startDatetime.Value));
            if (endDatetime != null)
                parameter.Add("endTimestamp", Utils.GenerateTimeStamp(endDatetime.Value));
            if (orderId != null)
                parameter.Add("orderId", orderId);
            if (tradeId != null)
                parameter.Add("tradeId", tradeId);

            _headerPayload = new
            {
                identity = "",
                nonce = Utils.GenerateTimeStamp(DateTime.Now)
            };

            return await SendRequestAsync($"/orders/trades/{pair}", ApiMethod.GET, parameter, true);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/get_deposit_invoices_data.md
        /// </summary>
        /// <param name="currency">the currency of the deposit to get</param>
        /// <param name="startDatetime">start time</param>
        /// <param name="endDatetime">end time</param>
        /// <param name="limit">the limit for the response</param>
        /// <param name="id">the id of the first data in the response</param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<dynamic> GetDepositHistory(string currency, DateTime? startDatetime = null, DateTime? endDatetime = null, int limitData = 100, string idData = "", CurrencyStatus status = CurrencyStatus.WAIT_PROCESS)
        {
            var parameter = new Dictionary<string, object>()
            {
                { "limit",$"{limitData}"},
                { "id",$"{idData}"},
                { "statuses",$"{status}"}
            };
            if (startDatetime != null)
                parameter.Add("startTimestamp", Utils.GenerateTimeStamp(startDatetime.Value));
            if (endDatetime != null)
                parameter.Add("endTimestamp", Utils.GenerateTimeStamp(endDatetime.Value));

            _headerPayload = new
            {
                identity = "",
                nonce = Utils.GenerateTimeStamp(DateTime.Now)
            };

            return await SendRequestAsync($"/wallet/depositHistory/{currency}", ApiMethod.GET, parameter, true);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/get_withdraw_invoices_data.md
        /// </summary>
        /// <param name="currency">the currency of the deposit to get</param>
        /// <param name="startDatetime">start time</param>
        /// <param name="endDatetime">end time</param>
        /// <param name="limit">the limit for the response</param>
        /// <param name="id">the id of the first data in the response</param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<dynamic> GetWithdrawHistory(string currency, DateTime? startDatetime = null, DateTime? endDatetime = null, int limitData = 100, string idData = "", CurrencyStatus status = CurrencyStatus.WAIT_PROCESS)
        {
            var parameter = new Dictionary<string, object>()
            {
                { "limit",$"{limitData}"},
                { "id",$"{idData}"},
                { "statuses",$"{status}"}
            };
            if (startDatetime != null)
                parameter.Add("startTimestamp", Utils.GenerateTimeStamp(startDatetime.Value));
            if (endDatetime != null)
                parameter.Add("endTimestamp", Utils.GenerateTimeStamp(endDatetime.Value));


            _headerPayload = new
            {
                identity = "",
                nonce = Utils.GenerateTimeStamp(DateTime.Now)
            };

            return await SendRequestAsync($"/wallet/withdrawHistory/{currency}", ApiMethod.GET, parameter, true);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/get_an_withdraw_invoice_data.md
        /// </summary>
        /// <param name="currency">the currency of the withdraw to get</param>
        /// <param name="serial">the serial of the withdraw</param>
        /// <returns>the withdraw information</returns>
        public async Task<dynamic> GetWithdraw(string currency, string serial)
        {
            _headerPayload = new
            {
                identity = "",
                nonce = Utils.GenerateTimeStamp(DateTime.Now)
            };

            return await SendRequestAsync($"/wallet/withdraw/{currency}/{serial}", ApiMethod.GET, null, true);
        }

        /// <summary>
        /// https://github.com/bitoex/bitopro-offical-api-docs/blob/master/api/v3/private/create_an_withdraw_invoice.md
        /// </summary>
        /// <param name="currency">the currency to withdraw</param>
        /// <param name="protocol">the protocol to send</param>
        /// <param name="address">the address or bank account to send fund</param>
        /// <param name="amount">the amount of fund to send</param>
        /// <param name="message">the message or note to be attached with withdraw</param>
        /// <returns></returns>
        public async Task<dynamic> Withdraw(string currency, WithdrawProtocol protocolData, string addressData, double amountData, string messageData)
        {
            _headerPayload = new
            {
                protocol = $"{protocolData}",
                address = addressData,
                amount = amountData,
                timestamp = Utils.GenerateTimeStamp(DateTime.Now),
                message = messageData
            };

            string json = JsonConvert.SerializeObject(_headerPayload);
            _httpContent = new StringContent(json, Encoding.Default, "application/json");

            return await SendRequestAsync($"/wallet/withdraw/{currency}", ApiMethod.POST, null, true);
        }
        #endregion

        private async Task<dynamic> SendRequestAsync(string endPoint, ApiMethod method, Dictionary<string, object> parameter, bool isSigned)
        {
            var finalEndpoint = FinalEndPoint(endPoint, parameter);

            if (isSigned && _headerPayload != null)
                ConfiguredHeader(_headerPayload);

            var request = new HttpRequestMessage(CreateHttpMethod(method), _httpClient.BaseAddress?.AbsoluteUri + finalEndpoint);
            if (_httpContent != null)
                request.Content = _httpContent;

            using (var response = await _httpClient.SendAsync(request))
            { 
                var responseContent = await response.Content.ReadAsStringAsync();
                _httpContent = null;

                if(!response.IsSuccessStatusCode)
                    Utils.Logger.Error(responseContent);
            
                return JsonConvert.DeserializeObject<dynamic>(responseContent);
            }
        }

        private void ConfiguredHeader(object parameter)
        {
            _httpClient.DefaultRequestHeaders.Clear();

            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parameter)));
            var signature = Utils.GenerateSignature(_apiKeySecret, payload).ToLower();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("X-BITOPRO-APIKEY", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-BITOPRO-PAYLOAD", payload);
            _httpClient.DefaultRequestHeaders.Add("X-BITOPRO-SIGNATURE", signature);
        }

        private string FinalEndPoint(string endPoint, Dictionary<string, object> parameter)
        {
            var finalEndpoint = endPoint + "?";
            var parameters = "";

            if (parameter != null)
            {
                foreach (var keyValue in parameter)
                    parameters += $"{keyValue.Key}={keyValue.Value}&";
            }

            if (!string.IsNullOrEmpty(parameters))
            {
                parameters = parameters.TrimEnd('&');
                finalEndpoint += parameters;
            }

            return finalEndpoint;
        }

        /// <summary>
        /// Gets an HttpMethod object based on a string.
        /// </summary>
        /// <param name="method">Name of the HttpMethod to create.</param>
        /// <returns>HttpMethod object.</returns>
        private HttpMethod CreateHttpMethod(ApiMethod method)
        {
            switch (method)
            {
                case ApiMethod.DELETE:
                    return HttpMethod.Delete;
                case ApiMethod.POST:
                    return HttpMethod.Post;
                case ApiMethod.PUT:
                    return HttpMethod.Put;
                case ApiMethod.GET:
                    return HttpMethod.Get;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
