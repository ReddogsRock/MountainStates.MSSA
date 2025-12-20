import { CommandNames } from '@2sic.com/2sxc-typings';

/** Loader function for our Hotspot Admin */
export function activateAdmin() {
  // Hotspot image where marker should be added
  let hotSpotImage = document.querySelectorAll('.hotspot3-js-image');
  hotSpotImage.forEach((hsElem: HTMLElement, index) => {
    // add click listener only once
    if(!hsElem.classList.contains('added-listener')) {
      hsElem.addEventListener('click', () => {  
        debugger;
        // if clicked element is hotspot - do nothing
        if(hsElem.classList.contains('hotspot3-marker')) {
          return false;
        }

        // get all needed information to create new hotspot
        const e = (event as MouseEvent);
        // Find the tag with class 'hotspots' because it contains more positioning data
        const tagWithData = hsElem.closest(".hotspots") as HTMLElement;

        const moduleId = parseInt(tagWithData.dataset.moduleId);
        const entityId = parseInt(tagWithData.dataset.entityId);
        const guid = tagWithData.dataset.guid;

        // get image dimensions
        const bounds = hsElem.getBoundingClientRect();

        // get hotspot icon offset
        const iconOffsetX = tagWithData.dataset.iconoffsetX;
        const iconOffsetY = tagWithData.dataset.iconoffsetY;

        // get exact position where hotspot marker should be added
        const x = e.clientX - bounds.left - parseInt(iconOffsetX);
        const y = e.clientY - bounds.top - parseInt(iconOffsetY);
        const xPercent = x / bounds.width * 100;
        const yPercent = y / bounds.height * 100;
        
        // open 2sxc mask with prefilled hotpot coordinates
        $2sxc(moduleId).cms.run({
          action: CommandNames.new,
          params: {
            entityId: entityId,
            parent: guid,
            fields: 'Hotspots',
            prefill: {
              X: Math.round(xPercent * 100) / 100,
              Y: Math.round(yPercent * 100) / 100 
            }
          }
        });
      });

      // added so click listener is only added once
      hsElem.classList.add('added-listener');
    }
  })
}