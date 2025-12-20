const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

module.exports = {
  entry: ['./src/ts/index.ts'],
  output: {
    path: path.resolve(__dirname, 'dist'),
    filename: 'scripts.min.js',
  },
  mode: 'production',
  devtool: 'source-map',
  watch: true,
  stats: {
    warnings: false,
    cachedModules: false,
    groupModulesByCacheStatus: false
  },
  cache: {
    type: 'filesystem',
    cacheDirectory: path.resolve(__dirname, '.temp_cache'),
    compression: 'gzip',
  },
  resolve: {
    extensions: ['.d.ts', '.ts', '.tsx', '.js', '.scss', '.css']
  },
  plugins: [
    new MiniCssExtractPlugin({
      filename: 'styles.min.css',
    }),
    new webpack.ProgressPlugin(),
  ],
  module: {
    rules: [{
        test: /\.scss$/,
        exclude: /node_modules/,
        use: [
          MiniCssExtractPlugin.loader,
          {
            loader: 'css-loader',
            options: {
              sourceMap: true
            }
          }, {
            loader: 'postcss-loader',
            options: {
              sourceMap: true,
              postcssOptions: {
                plugins: [
                  require('autoprefixer')
                ]
              }
            }
          }, {
            loader: 'sass-loader',
            options: {
              sourceMap: true
            }
          }
        ],
      }, {
        test: /\.ts$/,
        exclude: /node_modules|\.d\.ts$/,
        use: {
          loader: 'ts-loader'
        }
      }, {
        test: /\.d\.ts$/,
        loader: 'ignore-loader'
      }
    ],
  },
};

new webpack.ProgressPlugin((percentage, message) => {
  console.log(`${(percentage * 100).toFixed()}% ${message}`);
})