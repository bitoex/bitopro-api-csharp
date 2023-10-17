namespace Bitopro
{
    public enum CandlestickResolutin
    {
        _1m,
        _5m,
        _15m,
        _30m,
        _1h,
        _3h,
        _6h,
        _12h,
        _1d,
        _1w,
        _1M
    }

    public enum ApiMethod
    {
        POST,
        GET,
        PUT,
        DELETE
    }

    public enum OrderAction
    {
        BUY, SELL
    }

    public enum OrderType
    {
        Limit,
        Market
    }

    public enum TimeInForce
    {
        GTC,
        POST_ONLY
    }

    public enum StatusKind
    {
        ALL,
        OPEN,
        DONE,
    }

    public enum OrderStatus
    {
        NotTriggered = -1,
        InProgress = 0,
        InProgressPartialDeal = 1,
        Completed = 2,
        CompletedPartialDeal = 3,
        Cancelled = 4,
        PostOnlyCancelled = 6
    }

    public enum CurrencyStatus
    {
        CANCELLED,
        WAIT_PROCESS
    }

    public enum WithdrawProtocol
    {
        MAIN,
        ERC20,
        OMNI,
        TRX,
        BSC
    }
}
