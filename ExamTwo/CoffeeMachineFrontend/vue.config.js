module.exports = {

  lintOnSave: false,
  
  devServer: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000',  
        changeOrigin: true,               
        secure: false,                    
        pathRewrite: {
          '^/api': '/api'                 
        },
        logLevel: 'debug'                 
      }
    },
    
    port: 8080,                           
    open: true,                           
    hot: true,                            
    client: {
      overlay: {                          
        warnings: false,
        errors: true
      }
    }
  },

  productionSourceMap: false,             
  
  configureWebpack: {
    devtool: 'source-map',               
    performance: {
      hints: false                       
    }
  }
}