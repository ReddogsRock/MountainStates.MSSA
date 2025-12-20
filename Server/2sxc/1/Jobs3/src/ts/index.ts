import { showAlert, showConfigWarnings } from './lib-2sxc-alerts';
import { disableInputs, enableInputs, getFormValues, sendForm, validateForm } from './lib-2sxc-forms';
import { PristineOptions } from './lib-2sxc-pristine-options';
// import { getRecaptchaToken, requiresRecaptcha } from './lib-2sxc-recaptcha';

// so it can be called from the HTML when content re-initializes dynamically
const winAny = (window as any)
winAny.appJobs3 ??= {}
winAny.appJobs3.init ??= init

const debug = false

function init({ domAttribute, currentCategory, validationOptions }: { domAttribute: string, currentCategory: string, validationOptions: PristineOptions }) {
  const appWrapper = document.querySelector(`[${domAttribute}]`)
  const categoryButtons = appWrapper.querySelectorAll(`[app-jobs3-category-button]`)
  const noReloadRequired = currentCategory == ""
  setPrimary(currentCategory, categoryButtons)

  categoryButtons.forEach((categoryButton: HTMLElement) =>
    categoryButton.addEventListener('click', ((e) => {
      const filter = categoryButton.getAttribute('app-jobs3-filter');

      if (noReloadRequired) {
        e.preventDefault()
        window.history.pushState(null, document.title, categoryButton.getAttribute('href'));
      }

      setPrimary(filter, categoryButtons)
      filterItems(filter, appWrapper)
    }))
  )

  const eventsWrapper = document.querySelector(`[${domAttribute}]`);
  if(!eventsWrapper) return

  const submitButton = eventsWrapper.querySelector('[app-jobs3-send]') as HTMLButtonElement;
  if(submitButton !== null) {
    submitButton.addEventListener('click', async (event: Event) => {
      event.preventDefault();
  
      const eventBtn = event.currentTarget as HTMLElement;
      
      var valid = validateForm(eventsWrapper, validationOptions)
      if (!valid) {
        showAlert(eventsWrapper, 'msgIncomplete')
        return
      }
      
      const formValues = await getFormValues(eventsWrapper)
  
      // if (requiresRecaptcha(eventsWrapper)) {
      //   let token = await getRecaptchaToken(eventsWrapper)
      //   if (!token) return showAlert(eventsWrapper, 'msgRecap')
    
      //   // set token for backend
      //   formValues.Recaptcha = token
      // }
  
  
      // imply that message is sending by ui modifications 
  
      disableInputs(eventsWrapper, true)
      showAlert(eventsWrapper, 'msgSending')
      
      //#region request handling
  
      let endpoint = (eventsWrapper as HTMLElement).dataset.webservice // (should be "Form/ProcessForm" or a custom override)
  
  
      sendForm(formValues, submitButton, endpoint)
        .then((result: any) => {
          // error
          if(!result.ok) {
            if(debug) console.log('error', result.status);
      
            showAlert(eventsWrapper, 'msgError')
            showConfigWarnings(eventsWrapper, 'app-jobs3-config-warning')
            enableInputs(eventsWrapper)
            return
          }
          
          // success
          if(debug) console.log('success', result.json())
          submitButton.setAttribute("disabled", "")
    
          showAlert(eventsWrapper, 'msgOk')
          showConfigWarnings(eventsWrapper, 'app-jobs3-config-warning')
          disableInputs(eventsWrapper, false)
    
        })
  
      //#endregion
    })   
  }
  
}

// match category button to filter and add/remove primary class
function setPrimary(filter: string, buttons: NodeListOf<Element>) {
  if (filter == "") filter = "nofilter"
  buttons.forEach((button) => {
    if (button.getAttribute('app-jobs3-filter') == filter) {
      button.classList.remove('btn-outline-primary')
      button.classList.add('btn-primary')
      return
    }
    button.classList.remove('btn-primary')
    button.classList.add('btn-outline-primary')
  })
}

// filter shown items by selected Filter
function filterItems(selectedFilter: string, appWrapper: Element) {
  appWrapper.querySelectorAll(`[app-jobs3-filterelem]`).forEach((jobElement: HTMLElement) => {
    if (selectedFilter === "nofilter" || [...JSON.parse(jobElement.getAttribute('app-jobs3-filterelem'))].find(filter => filter == selectedFilter)) {
      jobElement.style.display = "block"
      return
    }
    jobElement.style.display = "none"
  })
}