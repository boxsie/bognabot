var path = require('path');

const webRoot = path.resolve(__dirname, 'wwwroot');
const baseScriptsPath = './Resources/Scripts/';
const filenameJs = 'js/[name].js';

module.exports = {
    entry: {
        'layout': baseScriptsPath + 'layout.js'
    },
    output: {
        path: webRoot,
        publicPath: '../',
        filename: filenameJs,
        library: 'bognabot'
    },
    module: {
        rules: [
            {
                test: /\.js$/,
                use: {
                    loader: 'babel-loader',
                    options: { presets: ['es2015'] }
                }
            },
            {
                test: /\.scss$/,
                use: [{ loader: 'style-loader' },{ loader: 'css-loader' }, { loader: 'sass-loader' }]
            }
        ]
    },
    resolve: {
        alias: {
            'vue': 'vue/dist/vue.js'
        }
    }
};