import vue from 'vue';
import 'bootstrap';
import { OrdersHub } from '../Components/ordersHub.js';

export class TraderViewModel {
    constructor(options) {
        this.vm = new vue({
            el: '#trader',
            data: {
                exchangePositions: [{
                    exchangeName: 'bitmex'
                }]
            },
            created() {
                this.ordersHub = new OrdersHub();

                this.ordersHub.start(() => {
                    this.ordersHub.streamExchangePosition('Bitmex', 'BTCUSD', (position) => {
                        console.log(position);
                        for (let i = 0; i < this.exchangePositions.length; i++) {
                            const ep = this.exchangePositions[i];
                            if (ep.exchangeName.toLowerCase() === 'bitmex') {
                                this.$set(this.exchangePositions, i, position);
                            }
                        }
                    });
                });
            }
        });
    }
}