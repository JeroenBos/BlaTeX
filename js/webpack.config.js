const CleanWebpackPlugin = require('clean-webpack-plugin');

const outputDir = '../../wwwroot/js'

module.exports = {
    mode: 'development',
    devtool: 'source-map',
    module: {
        rules: [
            {
                test: /\.css$/,
                loader: 'style-loader!css-loader'
            },
            // All output '.js' files will have any sourcemaps re-processed by 'source-map-loader'.
            {
                enforce: "pre",
                test: /\.js$/,
                loader: "source-map-loader"
            }
        ]
    },
    resolve: {
        extensions: ['.js'],
    },
    // Suppress fatal error: Cannot resolve module 'fs'
    // @relative https://github.com/pugjs/pug-loader/issues/8
    // @see https://github.com/webpack/docs/wiki/Configuration#node
    node: {
        fs: 'empty',
        child_process: 'empty',
        net: 'empty',
        tls: 'empty'
    },
    watchOptions: {
        aggregateTimeout: 0
    },

    plugins: [new CleanWebpackPlugin([outputDir])],
    name: 'blatex',
    entry: './index.js',
    output: { filename: outputDir + '/blatex.js' },
};