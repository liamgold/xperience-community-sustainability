const path = require("path");

module.exports = {
  mode: "production",
  entry: path.resolve(__dirname, "src/resource-checker.js"),
  output: {
    path: path.resolve(__dirname, "../wwwroot/scripts"),
    filename: "resource-checker.js",
    library: {
      type: "module",
    },
  },
  experiments: {
    outputModule: true,
  },
  module: {
    rules: [
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: "babel-loader",
          options: {
            presets: [
              ["@babel/preset-env", {
                targets: "> .2% and last 3 versions, not op_mini all, not IE 11",
                modules: false,
              }]
            ],
          },
        },
      },
    ],
  },
  resolve: {
    extensions: [".js"],
  },
};
