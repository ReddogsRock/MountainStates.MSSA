let Cookies = require('../../node_modules/js-cookie');
require('../../node_modules/micromodal');
declare let MicroModal: any;

const winAny = (window as any);
winAny.appPopupMessage3 ??= {};
winAny.appPopupMessage3.init ??= init;
winAny.appPopupMessage3.popUpShown = false; // parameter to prevent more than 1 popups showing simultaniously

function init({ domId, showAgainAfter }: { domId: string, showAgainAfter: string }) {
    MicroModal.init({ awaitCloseAnimation: true });

    if (!Cookies.get(`seenpopup-${domId}`) && winAny.appPopupMessage3.popUpShown === false && !document.body.classList.contains('role-admin')) {
        winAny.appPopupMessage3.popUpShown = true;
        setTimeout(() => {
            MicroModal.show(domId)
            console.log('set cookie')
            Cookies.set(`seenpopup-${domId}`, true, { expires: showAgainAfter })
        }, 5000)
    }
}
