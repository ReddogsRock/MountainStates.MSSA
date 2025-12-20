import { AccordionOptions } from './lib-2sxc-accordion-options';
declare let $2sxc: any;

/*
  This is a shared code used in various 2sxc apps. Make sure that they are in sync, so if you improve it, improve all 2sxc apps which use this. 
  ATM they are:
  - Accordion
  - FAQ
  - App School System

  The master with the newest / best version must always be the Accordion, the others should then get a fresh copy.
  Because this is shared, all parameters like DOM-IDs etc. must be provided in the Init-call that it can work across apps
*/ 

export function initAccordionBs5({ domId, options } : { domId: string, options: AccordionOptions }) {
  // get navHight for correct scrollposition
  var nav = (document.getElementsByTagName(options.tagStickyHeader)[0] as HTMLElement);
  var navHeight = (nav != null ? nav.offsetHeight : 0);

  let accordionWrapper = document.querySelector(`[${domId}]`);

  if (!accordionWrapper) {
    return;
  }

  // attach click to all accordions when loading
  var accordionOpeners = accordionWrapper.querySelectorAll(`.${options.accordionOpener}`);
  
  if ($2sxc.env.platform() !== 'oqtane') {
    accordionOpeners.forEach((elem: Element) => {
      elem.addEventListener('click', (event) => {
        event.preventDefault();
        const parentAttr = elem.getAttribute(options.attrParent);
        if (parentAttr) {
          history.pushState({}, "", parentAttr);
        }
      });
    });
  }

  // get hash from url and open specific item
  if(window.location.hash){
    const hash = window.location.hash;
    const targetButton = accordionWrapper.querySelector(`[${options.attrParent}="${hash}"]`);
    const targetContent = accordionWrapper.querySelector(hash);

    // if target element exists scroll to element and open it
    if(targetButton && targetContent){
      const elemOffsetX = targetContent.getBoundingClientRect().top + window.scrollY - navHeight;
      targetContent.classList.toggle(`${options.classIsExpanded}`);
      targetButton.classList.toggle(`${options.classInactive}`);

      // scroll to element which should open then
      window.scrollTo({
        top: elemOffsetX,
        left: 0,
        behavior: 'smooth'
      });
    }
  }
}