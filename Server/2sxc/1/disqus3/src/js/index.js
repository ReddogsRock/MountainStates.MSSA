let winAny = window;
winAny.appDisqus3 ??= {};
winAny.appDisqus3.initPro = initPro;

function initPro({ domainKey }) {
  let disqusObserver = new IntersectionObserver((entries) => {
    // start loading Disqus now
    if(entries[0].isIntersecting) {
      // enter disqus config here

      addDisqusScript()
      // once executed, stop observing
      disqusObserver.disconnect()
    }
  }, { threshold: [0] });

  disqusObserver.observe(document.querySelector("#disqus_thread"))

  function addDisqusScript() {
    let disqusScript = document.createElement('script')
    disqusScript.src = `https://${domainKey}/embed.js`
    disqusScript.setAttribute('data-timestamp', +new Date());
    (document.head || document.body).appendChild(disqusScript)
  }
}