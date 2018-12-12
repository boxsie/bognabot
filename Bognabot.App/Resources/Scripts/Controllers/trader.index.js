import vue from 'vue';
import 'bootstrap';
import { OrdersHub } from '../Components/ordersHub';

import '../../Style/trader.scss';

export class TraderViewModel {
    constructor(options) {
        this.vm = new vue({
            el: '#trader',
            data: {
                ordersHub: new OrdersHub(),
                exchanges: options.exchanges,
                exchangePositions: [],
                orderTypes: options.orderTypes,
                orderModel: {
                    exchange: options.exchanges[0],
                    amount: 0,
                    instrument: 0,
                    orderType: options.orderTypes[0].value,
                    orderPrice: 0,
                    orderStopAmountType: '%',
                    orderStopAmount: 0,
                    orderProfitAmountType: '%',
                    orderProfitAmount: 0,
                    orderLimitSlipType: '%',
                    orderLimitSlipAmount: 0
                },
                showTradeAddStop: false,
                showTradeAddProfit: false
    },
            methods: {
                parseInstrument(index) {
                    return options.instruments[index];
                },
                buyOrder() {
                    if (this.orderModel.amount < 0) {
                        this.orderModel.amount = Math.abs(this.orderModel.amount);
                    }
                    this.ordersHub.placeOrder(this.orderModel);
                },
                sellOrder() {
                    if (this.orderModel.amount > 0) {
                        this.orderModel.amount = -Math.abs(this.orderModel.amount);
                    }
                    this.ordersHub.placeOrder(this.orderModel);
                }
            },
            created() {
                this.ordersHub.start(() => {
                    this.exchangePositions = this.ordersHub.positions;

                    for (let i = 0; i < this.exchangePositions.length; i++) {
                        const pos = this.exchangePositions[i];
                        const posI = i;

                        this.ordersHub.streamExchangePosition(pos.exchangeName, pos.instrument, (position) => {
                            this.$set(this.exchangePositions, posI, position);
                        });
                    }
                });
            }
        });
    }
}