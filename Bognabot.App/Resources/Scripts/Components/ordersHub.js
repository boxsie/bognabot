import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

export class OrdersHub {
    constructor() {
        this.connection = new HubConnectionBuilder()
            .configureLogging(LogLevel.Information)
            .withUrl('/ordershub').build();

        this.positions = {};
    }

    start(onConnected) {
        this.connection.start().then(() => {
            this.connection.invoke('getAllPositions').then((positions) => {
                this.positions = positions;
                onConnected();
            });
        });
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

    placeOrder(orderModel) {
        const model = {
            exchange: orderModel.exchange,
            amount: orderModel.amount,
            instrument: orderModel.instrument,
            price: orderModel.orderPrice,
            side: orderModel.amount > 0 ? 0 : 1,
            orderType: orderModel.orderType 
        };

        this.connection.invoke('placeOrder', model).then((order) => {
            console.log(order);
        });
    }
};