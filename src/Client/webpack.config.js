const webpackMerge = require("webpack-merge");
const path = require("path");

const baseWebpackConfig = require("@kentico/xperience-webpack-config");

module.exports = (opts, argv) => {
  const baseConfig = (webpackConfigEnv, argv) => {
    return baseWebpackConfig({
      // Sets the organizationName and projectName
      // The JS module is registered on the backend using these values
      orgName: "sustainability",
      projectName: "web-admin",
      webpackConfigEnv: webpackConfigEnv,
      argv: argv,
    });
  };

  const projectConfig = {
    module: {
      rules: [
        {
          test: /\.(js|ts)x?$/,
          exclude: [/node_modules/],
          loader: "babel-loader",
        },
      ],
    },
    output: {
      clean: true,
    },
    // Webpack server configuration. Required when running the boilerplate in 'Proxy' mode.
    devServer: {
      port: 3009,
    },
    resolve: {
      alias: {
        "@": path.resolve(__dirname),
      },
      extensions: [".js", ".jsx", ".ts", ".tsx"],
    },
  };

  return webpackMerge.merge(projectConfig, baseConfig(opts, argv));
};
