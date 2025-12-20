
require('../scss/_style.scss');

import { activateAdmin } from './admin';
import { initFancybox } from './lib-2sxc-fancybox';
import { FancyboxOptions } from './lib-2sxc-fancybox-options';

const winAny = (window as any);
winAny.appHotspot3 ??= {};
winAny.appHotspot3.activateAdmin ??= activateAdmin;
winAny.appHotspot3.initFancybox ??= initFancyboxCustom;

function initFancyboxCustom({ attribute, options } : { attribute: string, options: FancyboxOptions }) {
  let customOptions = {
    ...options,
    on: {  
      done: (fancybox: any, slide: any) => {
        const img = document.querySelector(`${slide.src} img`);
        // if (img == null) return;
        const imgWidth = img?.getBoundingClientRect().width ?? 0;

        const fancyboxText: HTMLElement = document.querySelector(`${slide.src} .hotspot3-fancybox-text`);
        if (fancyboxText?.hasAttribute('style') == true)
          fancyboxText.style.maxWidth = `${Math.floor(imgWidth)}px`;

        const hotspotContent: HTMLElement = document.querySelector(`${slide.src}.hotspot3-fancybox-content`);
        if (hotspotContent)
          hotspotContent.style.opacity = "1";
      }
    }
  }
  initFancybox({ attribute, options: customOptions })
}