import ReactGA from 'react-ga4';
import { GA_KEY } from 'Config';
export const initializeReactGA = async () => {
    let ga_ID = await GA_KEY();

    if (ga_ID === undefined || ga_ID === null) {
        return;
    }

    ReactGA.initialize(ga_ID);

    ReactGA.send({
        hitType: window.location.hostname,
        page: window.location.pathname + window.location.search
    });
};

/** Check in real time -> event
 * Event - Add custom tracking event.
 * @param {string} category
 * @param {string} action
 * @param {string} label
 */
export const Event = (category, action, label) => {
    ReactGA.event({
        category: category,
        action: action,
        label: label
    });
};
