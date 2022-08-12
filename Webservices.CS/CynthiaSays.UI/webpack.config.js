const WebpackNotifierPlugin = require('webpack-notifier');
const path = require('path');

module.exports = {
  module : {
    rules: [
      {
        test   : /\.css$/,
        loader : 'postcss-loader',
        options: {
          ident  : 'postcss',
          plugins: () => [
            require('postcss-short'),
            require('postcss-preset-env'),
            require('postcss-size'),
            require('autoprefixer')
          ]
        }
      }
    ]
  },
  plugins: [
    new WebpackNotifierPlugin({
      alwaysNotify: true,
      title       : 'Cynthia Says',
      contentImage: path.join(__dirname, 'image.png')
    }),
  ]
};