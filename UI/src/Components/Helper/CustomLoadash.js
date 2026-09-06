import { isEqual } from 'lodash';

const getPlainJSON = (obj) => {
    var seen = [];

    if (obj === undefined) {
        return '';
    }
    if (obj === null) {
        return '';
    }

    try {
        if (typeof obj === 'number') {
            return obj;
        }

        if (
            typeof obj === 'function' ||
            obj instanceof Date ||
            obj instanceof RegExp ||
            obj instanceof String ||
            obj instanceof Number
        ) {
            return obj.toString();
        }

        return JSON.stringify(obj, function (key, val) {
            if (val != null && typeof val == 'object') {
                if (seen.indexOf(val) >= 0) {
                    return;
                }
                seen.push(val);
            }
            return val;
        });
    } catch (errro) {
        return obj;
    }
};

export const checkEqual = (pre, next) => {
    var obj1 = getPlainJSON(pre);

    var obj2 = getPlainJSON(next);

    return isEqual(obj1, obj2);
};
