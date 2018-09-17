import vue from 'vue';
import 'bootstrap';

import '../Style/layout.scss';
import '../Style/signals.scss';

export class LayoutViewModel {
    constructor(options) {
        this.vm = new vue({
            el: '#main',
            data: {
                
            }
        });
    }
}