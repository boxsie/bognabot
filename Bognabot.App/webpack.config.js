var path = require('path');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

const webRoot = path.resolve(__dirname, 'wwwroot');
const baseScriptsPath = './Resources/Scripts/';

const filenameJs = 'js/[name].js';
const filenameCss = 'css/[name].css';
const filenameCssChunk = 'css/[id].css';

module.exports = {
    entry: {
        'layout': baseScriptsPath + 'layout.js',
        'signals.index': baseScriptsPath + 'Controllers/signals.index.js',
        'indicators.index': baseScriptsPath + 'Controllers/indicators.index.js'
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
                use: { loader: "babel-loader", options: { presets: ["es2015"] } }
            },
            {
                test: /\.css$/,
                use: [
                    { loader: MiniCssExtractPlugin.loader },
                    { loader: 'css-loader', options: {} },
                    { loader: 'postcss-loader' }
                ]
            },
            {
                test: /\.sass$|\.scss$/,
                use: [
                    { loader: MiniCssExtractPlugin.loader },
                    { loader: 'css-loader' },
                    { loader: 'postcss-loader', options: {
                        plugins: function () { // post css plugins, can be exported to postcss.config.js
                                return [
                                    require('precss'),
                                    require('autoprefixer')
                                ];
                            }
                        }
                    },
                    { loader: 'sass-loader' }
                ]
            }
        ]
    },
    resolve: {
        alias: {
            'vue': 'vue/dist/vue.js'
        }
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: filenameCss,
            chunkFilename: filenameCssChunk
        })
    ]
};