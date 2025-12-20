const path = require('path');
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
    all: false,
    assets: true
  },
  cache: {
    type: 'filesystem',
    cacheDirectory: path.resolve(__dirname, '.temp_cache'),
    compression: 'gzip',
  },
  resolve: {
    extensions: ['.ts', '.tsx', '.js', '.scss', '.css']
  },
  plugins: [
    new MiniCssExtractPlugin({
      filename: 'styles.min.css',
    }),
  ],
  module: {
    rules: [{
        test: /\.(scss|css)$/,
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
      },
      {
        test: /\.ts$/,
        exclude: /node_modules/,
        use: {
          loader: 'ts-loader'
        }
      },
      {
        test: /\.(png|jpe?g|gif)$/,
        use: [{
          options: {
            name: '[name].[ext]',
            outputPath: 'images/'
          }
        }]
      }
    ],
  },
};