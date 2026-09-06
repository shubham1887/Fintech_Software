var LOCAL_API_URL = '';
var LOCAL_CLIENT_NAME = '';
var LOCAL_LOGO_WIDTH = '';
var LOCAL_GA_KEY = ''; //REACT_APP_GA_KEY
var LOCAL_RE_CAPTCHA_V2_KEY = ''; //REACT_APP_RE_CAPTCHA_V2_KEY

var LOCAL_CLIENT_ADDRESS = ''; //REACT_APP_CLIENT_ADDRESS
var LOCAL_CLIENT_MOBILENO = ''; //REACT_APP_CLIENT_MOBILENO
var LOCAL_CLIENT_EMAILID = ''; //REACT_APP_CLIENT_EMAILID
var LOCAL_SOUND_ON = ''; //REACT_APP_CLIENT_EMAILID

var LOCAL_LOGO_URL = '';
var LOCAL_OUTSIDE_SIGNUP = '';
var LOCAL_PLAY_STORE_URL = '';
var LOCAL_REFRESH_SECOND = '';
var LOCAL_SHOWPAYMODE = '';
var LOCAL_FETCHLIST = ''; // values are : All, Searchable
var LOCAL_TODAYSUMMARYONTOP = ''; // values are : Yes, No(default)
var LOCAL_PROVIDERSSUMMARYONTOP = '';
var LOCAL_IS_CAPTCHA = '';
var LOCAL_IS_RECHARGE_REPORT_COMPACT = '';
var LOCAL_SIGNUP_WITH_EKYC = '';

// values are : Yes, No(default)
const readJsonSync = async () => {
    const json = await readJson();

    LOCAL_API_URL = json.REACT_APP_API_URL;
    LOCAL_LOGO_URL = json.REACT_APP_API_URL + 'app-docs/logo.png';
    LOCAL_CLIENT_NAME = json.REACT_APP_CLIENT_NAME;
    LOCAL_LOGO_WIDTH = json.REACT_APP_LOGO_WIDTH;
    LOCAL_GA_KEY = json.REACT_APP_GA_KEY;
    LOCAL_RE_CAPTCHA_V2_KEY = json.REACT_APP_RE_CAPTCHA_V2_KEY;

    LOCAL_CLIENT_ADDRESS = json.REACT_APP_CLIENT_ADDRESS;
    LOCAL_CLIENT_MOBILENO = json.REACT_APP_CLIENT_MOBILENO;
    LOCAL_CLIENT_EMAILID = json.REACT_APP_LOCAL_CLIENT_EMAILID;

    LOCAL_OUTSIDE_SIGNUP = json.LOCAL_OUTSIDE_SIGNUP;
    LOCAL_PLAY_STORE_URL = json.REACT_APP_PLAY_STORE_URL;
    LOCAL_SOUND_ON = json.REACT_APP_SOUND_ON;
    LOCAL_REFRESH_SECOND = json.REACT_APP_REFRESH_SECOND;
    LOCAL_SHOWPAYMODE = json.REACT_APP_SHOWPAYMODE;

    LOCAL_FETCHLIST = json.REACT_APP_FETCHLIST;
    LOCAL_TODAYSUMMARYONTOP = json.REACT_APP_TODAYSUMMARYONTOP;
    LOCAL_PROVIDERSSUMMARYONTOP = json.REACT_APP_PROVIDERSSUMMARYONTOP;
    LOCAL_IS_CAPTCHA = json.REACT_APP_IS_CAPTCHA;
    LOCAL_IS_RECHARGE_REPORT_COMPACT = json.RECHARGE_REPORT_COMPACT;
    LOCAL_SIGNUP_WITH_EKYC = json.SIGNUP_WITH_EKYC;
};

const readJson = async () => {
    const response = await fetch(`${process.env.PUBLIC_URL}/config.json`);
    const json = await response.json();
    return json;
};

export const VARIANT = 'outlined';

export const API_URL = async () => {
    if (LOCAL_API_URL === undefined || LOCAL_API_URL === '') {
        await readJsonSync();
    }

    return LOCAL_API_URL;
};

export const LOGO_URL = async () => {
    if (LOCAL_LOGO_URL === undefined || LOCAL_LOGO_URL === '') {
        await readJsonSync();
    }

    return LOCAL_LOGO_URL;
};

export const CLIENT_NAME = async () => {
    if (LOCAL_CLIENT_NAME === undefined || LOCAL_CLIENT_NAME === '') {
        await readJsonSync();
    }

    return LOCAL_CLIENT_NAME;
};

export const LOGO_WIDTH = async () => {
    if (LOCAL_LOGO_WIDTH === undefined || LOCAL_LOGO_WIDTH === '') {
        await readJsonSync();
    }

    return LOCAL_LOGO_WIDTH;
};
export const REPORT_FONT_SIZE = () => {
    return localStorage.getItem('ReportFontSize') || '13px';
};
export const PRIMARY_COLOR = () => {
    return localStorage.getItem('PrimaryColor') || '#7467ef';
};

export const SECONDARY_COLOR = () => {
    return localStorage.getItem('SecondaryColor') || '#7467ef';
};
export const PRIMARY_HOVER_COLOR = () => {
    return localStorage.getItem('PrimaryHoverColor') || '#7467ef';
};

export const SECONDARY_HOVER_COLOR = () => {
    return localStorage.getItem('SecondaryHoverColor') || '#7467ef';
};

export const APP_BAR_COLOR = () => {
    return localStorage.getItem('AppBarColor') || '#7467ef';
};
export const MENU_FORE_COLOR = () => {
    return localStorage.getItem('ForeColor') || '#ffffff';
};

export const NAV_BAR_COLOR = () => {
    return localStorage.getItem('NavBarColor') || '#c4c4c4';
};

export const RESET_COLOR = () => {
    //return localStorage.getItem('ResetColor') || '#fff';
    return '#fff';
};

export const GA_KEY = async () => {
    if (LOCAL_GA_KEY === undefined || LOCAL_GA_KEY === '') {
        await readJsonSync();
    }

    return LOCAL_GA_KEY;
};

export const RE_CAPTCHA_V2_KEY = async () => {
    if (
        LOCAL_RE_CAPTCHA_V2_KEY === undefined ||
        LOCAL_RE_CAPTCHA_V2_KEY === ''
    ) {
        await readJsonSync();
    }

    return LOCAL_RE_CAPTCHA_V2_KEY;
};
export const IS_CAPTCHA = async () => {
    if (LOCAL_IS_CAPTCHA === undefined || LOCAL_IS_CAPTCHA === '') {
        await readJsonSync();
    }
    if (LOCAL_IS_CAPTCHA === undefined || LOCAL_IS_CAPTCHA === '') {
        LOCAL_IS_CAPTCHA = 'YES';
    }

    return LOCAL_IS_CAPTCHA;
};
export const IS_RECHARGE_REPORT_COMPACT = async () => {
    if (
        LOCAL_IS_RECHARGE_REPORT_COMPACT === undefined ||
        LOCAL_IS_RECHARGE_REPORT_COMPACT === ''
    ) {
        await readJsonSync();
    }
    if (
        LOCAL_IS_RECHARGE_REPORT_COMPACT === undefined ||
        LOCAL_IS_RECHARGE_REPORT_COMPACT === ''
    ) {
        LOCAL_IS_RECHARGE_REPORT_COMPACT = 'OFF';
    }

    return LOCAL_IS_RECHARGE_REPORT_COMPACT;
};

export const IS_SIGNUP_WITH_EKYC = async () => {
    if (
        LOCAL_SIGNUP_WITH_EKYC === undefined ||
        LOCAL_SIGNUP_WITH_EKYC === ''
    ) {
        await readJsonSync();
    }
    if (
        LOCAL_SIGNUP_WITH_EKYC === undefined ||
        LOCAL_SIGNUP_WITH_EKYC === ''
    ) {
        LOCAL_SIGNUP_WITH_EKYC = 'OFF';
    }

    return LOCAL_SIGNUP_WITH_EKYC;
};

export const CLIENT_ADDRESS = async () => {
    if (LOCAL_CLIENT_ADDRESS === undefined || LOCAL_CLIENT_ADDRESS === '') {
        await readJsonSync();
    }

    return LOCAL_CLIENT_ADDRESS;
};

export const CLIENT_MOBILENO = async () => {
    if (LOCAL_CLIENT_MOBILENO === undefined || LOCAL_CLIENT_MOBILENO === '') {
        await readJsonSync();
    }

    return LOCAL_CLIENT_MOBILENO;
};

export const CLIENT_EMAILID = async () => {
    if (LOCAL_CLIENT_EMAILID === undefined || LOCAL_CLIENT_EMAILID === '') {
        await readJsonSync();
    }

    return LOCAL_CLIENT_EMAILID;
};

export const CONSUMER_NUMBER_FONT_SIZE = async () => {
    return localStorage.getItem('ConsumerNumberFontSize') || '20';
};

export const CONTACT_US_HTML = async () => {
    return await fetch(`${process.env.PUBLIC_URL}/contactus.html`);
};

export const OUTSIDE_SIGNUP = async () => {
    if (LOCAL_OUTSIDE_SIGNUP === undefined || LOCAL_OUTSIDE_SIGNUP === true) {
        await readJsonSync();
    }
    return LOCAL_OUTSIDE_SIGNUP;
};

export const PLAY_STORE_URL = async () => {
    if (LOCAL_PLAY_STORE_URL === undefined || LOCAL_PLAY_STORE_URL === '') {
        await readJsonSync();
    }
    return LOCAL_PLAY_STORE_URL;
};

export const SOUND_ON = async () => {
    if (LOCAL_SOUND_ON === undefined || LOCAL_SOUND_ON === '') {
        await readJsonSync();
    }

    return LOCAL_SOUND_ON;
};

export const REFRESH_SECOND = async () => {
    if (LOCAL_REFRESH_SECOND === undefined) {
        LOCAL_REFRESH_SECOND = '40';
    }

    if (LOCAL_REFRESH_SECOND === '') {
        LOCAL_REFRESH_SECOND = '40';
    }

    return LOCAL_REFRESH_SECOND;
};

export const SHOW_PAYMODE = async () => {
    if (LOCAL_SHOWPAYMODE === undefined || LOCAL_SHOWPAYMODE === '') {
        await readJsonSync();
    }

    return LOCAL_SHOWPAYMODE;
};

export const FETCH_LIST = async () => {
    if (LOCAL_FETCHLIST === undefined || LOCAL_FETCHLIST === '') {
        await readJsonSync();
    }

    if (LOCAL_FETCHLIST === undefined || LOCAL_FETCHLIST === '') {
        LOCAL_FETCHLIST = 'Searchable';
    }

    return LOCAL_FETCHLIST;
};

export const TODAYSUMMARYONTOP = async () => {
    if (
        LOCAL_TODAYSUMMARYONTOP === undefined ||
        LOCAL_TODAYSUMMARYONTOP === ''
    ) {
        await readJsonSync();
    }

    if (
        LOCAL_TODAYSUMMARYONTOP === undefined ||
        LOCAL_TODAYSUMMARYONTOP === ''
    ) {
        LOCAL_TODAYSUMMARYONTOP = 'NO';
    }

    return LOCAL_TODAYSUMMARYONTOP;
};

export const PROVIDERSSUMMARYONTOP = async () => {
    if (
        LOCAL_PROVIDERSSUMMARYONTOP === undefined ||
        LOCAL_PROVIDERSSUMMARYONTOP === ''
    ) {
        await readJsonSync();
    }

    if (
        LOCAL_PROVIDERSSUMMARYONTOP === undefined ||
        LOCAL_PROVIDERSSUMMARYONTOP === ''
    ) {
        LOCAL_PROVIDERSSUMMARYONTOP = 'NO';
    }

    return LOCAL_PROVIDERSSUMMARYONTOP;
};
