const WebpackNotifierPlugin = require('webpack-notifier');
const path = require('path');

module.exports = {
  module : {
    rules: [
      {
        test   : /\.scss$/,
        loader : 'postcss-loader',
        options: {
          ident  : 'postcss',
          plugins: () => [
            require('postcss-short'),
            require('postcss-preset-env'),
            require('autoprefixer')
          ]
        }
      }
    ]
  },
  plugins: [
    new WebpackNotifierPlugin({
      alwaysNotify: true,
      title       : 'Compliance Sheriff - Angular App',
      contentImage: path.join(__dirname, 'image.png')
    }),
  ]
};