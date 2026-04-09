module.exports = {
    publicPath: './',
    outputDir: "../wwwroot",
    devServer: {
        port: process.env.VUE_APP_PORT,
        headers: {
            'Access-Control-Allow-Origin': '*'
        },
        proxy: {
            '/api': {
                target: process.env.VUE_APP_DEV_SERVER, // 你请求的第三方接口
                changeOrigin: true,
            },
        }
    },
    
    chainWebpack: config => {
        config.module
            .rule("i18n")
            .resourceQuery(/blockType=i18n/)
            .type('javascript/auto')
            .use("i18n")
            .loader("@kazupon/vue-i18n-loader");
        config.plugin('html')
            .tap(args => {
                args[0].title = '嘉腾WMS系统'
                return args
            });
    },
}