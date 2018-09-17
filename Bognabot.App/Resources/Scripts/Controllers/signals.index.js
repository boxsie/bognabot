import vue from 'vue';
import 'bootstrap';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

export class SignalsViewModel {
    constructor(options) {
        this.vm = new vue({
            el: '#signals',
            data: {

            },
            created() {
                this.connection = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Information)
                    .withUrl('/layouthub').build();

                this.connection.start()
                    .then(() => {
                        this.connection.on('updateTimestampUtc',
                            (timestamp) => {
                                this.utcMoment = moment(timestamp);
                            });

                        this.connection.invoke('getLatestTimestampUtc').then((timestamp) => {
                            this.utcMoment = moment(timestamp);
                        });
                    });

                setInterval(() => {
                    this.utcMoment = moment(this.utcMoment.add(1, 's'));
                }, 1000);
            }
        });
    }
}