import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

export class OrdersHub {
    constructor() {
        this.connection = new HubConnectionBuilder()
            .configureLogging(LogLevel.Information)
            .withUrl('/ordershub').build();
    }

    start(onConnected) {
        this.connection.start().then(onConnected);
    }

    streamExchangePosition(exchange, instrument, callback) {
        this.connection.stream('streamPosition', exchange, instrument)
            .subscribe({
                next: (item) => {
                    callback(item);
                },
                complete: () => { },
                error: (err) => {
                     console.log(err);
                }
            });
    }
};