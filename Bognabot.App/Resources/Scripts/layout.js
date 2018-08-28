import vue from 'vue';
import 'bootstrap';

import '../Style/layout.scss';

export class LayoutViewModel {
    constructor(options) {
        this.vm = new vue({
            el: '#main',
            data: {
                
            }
        });
    }
}