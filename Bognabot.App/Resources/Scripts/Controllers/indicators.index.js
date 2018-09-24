import vue from 'vue';
import 'bootstrap';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

export class IndicatorsViewModel {
    constructor(options) {
        this.vm = new vue({
            el: '#indicators',
            data: {

            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/bitmexbtcusdhub').build();

                this.connection.start()
                    .then(() => {
                        
                    });
            }
        });
    }
}

