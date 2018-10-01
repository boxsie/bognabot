export class TradeIndicator {
    constructor(connection, indicatorName, exchangeName, instrument, period) {
        this.connection = connection;
        this.indicatorName = indicatorName;
        this.exchangeName = exchangeName;
        this.instrument = instrument;
        this.period = period;

        this.current = 0;
    }

    getLatest() {
        var self = this;

        self.connection.stream('getIndicator', self.indicatorName, self.exchangeName, self.instrument, self.period)
            .subscribe({
                next: (item) => {
                    self.current = item;
                },
                complete: () => { },
                error: (err) => {
                     console.log(err);
                }
            });
    }
};