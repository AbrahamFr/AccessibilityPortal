module.exports = [
  {
    context:
      /**
       *
       * @param {string} pathname
       */
      function(pathname) {
        if (pathname.startsWith("/Trend/rest")) {
          // dev time API proxying
          return true;
        }
        if (pathname.startsWith("/Trend")) {
          // local angular stuff, do not proxy
          return false;
        }
        // everything else proxies
        return true;
      },
    target: "http://qawebui.southcentralus.cloudapp.azure.com",
    secure: false,
    bypass: function(req) {
      console.log(req.originalUrl);
    }
  }
];
