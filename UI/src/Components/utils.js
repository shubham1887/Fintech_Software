import { differenceInSeconds } from 'date-fns';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectDMTChargeAndCommission as SELECT_DMT_CHARGE } from 'Components/Api/SlabDetailApi';

export function debounce(func, wait, immediate) {
    var timeout;
    return function () {
        var context = this,
            args = arguments;
        clearTimeout(timeout);
        timeout = setTimeout(function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        }, wait);
        if (immediate && !timeout) func.apply(context, args);
    };
}

export function isMobile() {
    if (window) return window.matchMedia(`(max-width: 767px)`).matches;

    return false;
}

export function isMdScreen() {
    if (window) return window.matchMedia(`(max-width: 1199px)`).matches;

    return false;
}

function currentYPosition() {
    if (!window) return;

    // Firefox, Chrome, Opera, Safari
    if (window.pageYOffset) return window.pageYOffset;
    // Internet Explorer 6 - standards mode
    if (document.documentElement && document.documentElement.scrollTop)
        return document.documentElement.scrollTop;
    // Internet Explorer 6, 7 and 8
    if (document.body.scrollTop) return document.body.scrollTop;
    return 0;
}

export function scrollToTop() {
    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;
}

function elmYPosition(elm) {
    var y = elm.offsetTop;
    var node = elm;
    while (node.offsetParent && node.offsetParent !== document.body) {
        node = node.offsetParent;
        y += node.offsetTop;
    }
    return y;
}

export function scrollTo(scrollableElement, elmID) {
    var elm = document.getElementById(elmID);
    if (!elmID || !elm) return;

    var startY = currentYPosition();
    var stopY = elmYPosition(elm);
    var distance = stopY > startY ? stopY - startY : startY - stopY;
    if (distance < 100) {
        scrollTo(0, stopY);
        return;
    }
    var speed = Math.round(distance / 50);
    if (speed >= 20) speed = 20;
    var step = Math.round(distance / 25);
    var leapY = stopY > startY ? startY + step : startY - step;
    var timer = 0;
    if (stopY > startY) {
        for (var i = startY; i < stopY; i += step) {
            setTimeout(
                (function (leapY) {
                    return () => {
                        scrollableElement.scrollTo(0, leapY);
                    };
                })(leapY),
                timer * speed
            );
            leapY += step;
            if (leapY > stopY) leapY = stopY;
            timer++;
        }
        return;
    }
    for (let i = startY; i > stopY; i -= step) {
        setTimeout(
            (function (leapY) {
                return () => {
                    scrollableElement.scrollTo(0, leapY);
                };
            })(leapY),
            timer * speed
        );
        leapY -= step;
        if (leapY < stopY) leapY = stopY;
        timer++;
    }
    return false;
}

export function getTimeDifference(date) {
    let difference = differenceInSeconds(new Date(), date);

    if (difference < 60) return `${Math.floor(difference)} seconds`;
    else if (difference < 3600) return `${Math.floor(difference / 60)} minutes`;
    else if (difference < 86400)
        return `${Math.floor(difference / 3660)} hours`;
    else if (difference < 86400 * 30)
        return `${Math.floor(difference / 86400)} days`;
    else if (difference < 86400 * 30 * 12)
        return `${Math.floor(difference / 86400 / 30)} months`;
    else return `${(difference / 86400 / 30 / 12).toFixed(1)} years`;
}

export function generateRandomId() {
    let tempId = Math.random().toString();
    let uid = tempId.substr(2, tempId.length - 1);
    return uid;
}

export function getQueryParam(prop) {
    var params = {};
    var search = decodeURIComponent(
        window.location.href.slice(window.location.href.indexOf('?') + 1)
    );
    var definitions = search.split('&');
    definitions.forEach(function (val, key) {
        var parts = val.split('=', 2);
        params[parts[0]] = parts[1];
    });
    return prop && prop in params ? params[prop] : params;
}

export function classList(classes) {
    return Object.entries(classes)
        .filter((entry) => entry[1])
        .map((entry) => entry[0])
        .join(' ');
}

export function indianFormatNumber(number) {
    return number.toLocaleString('en-IN', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

export function indianFormatNumberWithoutDecimal(number) {
    return number.toLocaleString('en-IN');
}

export function getClassName(value) {
    const classes = commonStyle();

    if (
        value === 'SUCCESS' ||
        value === 'COMPLETED' ||
        value === 'APPROVE' ||
        value === 'CREDIT' ||
        value === 'CLOSED' ||
        // value === 'REFUND' ||
        value === 'CASH COLLECTION DECREASED'
    )
        return classes.success;
    else if (
             value === 'FAIL' ||
             value === 'FAILURE' ||
             value === 'REJECT' ||
             value === 'DEBIT' ||
             value === 'REFUND' ||
             value === 'CASH COLLECTION INCREASED'
         )
             return classes.fail;
         else if (
             value === 'PROCESS' ||
             value === 'PENDING' ||
             value === 'CASH COLLECTION REVERSE'
         )
             return classes.process;
         else if (value === 'OPEN') return classes.Open;
}

export function titleCase(str) {
    str = str.trim();
    return str
        .toLowerCase()
        .split(' ')
        .map(function (word) {
            return word.replace(word[0], word[0].toUpperCase());
        })
        .join(' ');
}

export const SELECT_DMT_CHARGE_AND_COMM = async (requestObj) => {
    try {
        var request = JSON.stringify(requestObj);

        const response = await SELECT_DMT_CHARGE(request);

        if (!response.isAuthorized) {
            return { Commission: 0, Charge: 0, Total: 0 };
        }
        var data = JSON.parse(response.data);
        return {
            Commission: data.Commission,
            Charge: data.Charge,
            Total: data.Total
        };
    } catch (error) {
        return { Commission: 0, Charge: 0, Total: 0 };
    }
};
