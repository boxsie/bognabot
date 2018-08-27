namespace Bognabot.Bitmex
{
    public enum BitmexSubject
    {
        // Public
        Announcement,// Site announcements
        Chat,        // Trollbox chat
        Connected,   // Statistics of connected users/bots
        Funding,     // Updates of swap funding rates. Sent every funding interval (usually 8hrs)
        Instrument,  // Instrument updates including turnover and bid/ask
        Insurance,   // Daily Insurance Fund updates
        Liquidation, // Liquidation orders as they're entered into the book
        OrderBookL2, // Full level 2 orderBook
        OrderBook10, // Top 10 levels using traditional full book push
        PublicNotifications, // System-wide notifications (used for short-lived messages)
        Quote,       // Top level of the book
        QuoteBin1M,  // 1-minute quote bins
        QuoteBin5M,  // 5-minute quote bins
        QuoteBin1H,  // 1-hour quote bins
        QuoteBin1D,  // 1-day quote bins
        Settlement,  // Settlements
        Trade,       // Live trades
        TradeBin1M,  // 1-minute trade bins
        TradeBin5M,  // 5-minute trade bins
        TradeBin1H,  // 1-hour trade bins
        TradeBin1D,  // 1-day trade bins

        // Requires auth
        Affiliate,   // Affiliate status, such as total referred users & payout %
        Execution,   // Individual executions; can be multiple per order
        Order,       // Live updates on your orders
        Margin,      // Updates on your current account balance and margin requirements
        Position,    // Updates on your positions
        PrivateNotifications, // Individual notifications - currently not used
        Transact,    // Deposit/Withdrawal updates
        Wallet       // Bitcoin address balance data, including total deposits & withdrawals
    }
}