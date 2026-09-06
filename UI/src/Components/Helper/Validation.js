export const IsValidString = (inputString) => {
    if (inputString === undefined) return false;

    if (inputString === null) return false;

    let input = String(inputString);

    return input.trim().length > 0;
};

export const IsValidMobileNumber = (inputString) => {
    if (!IsValidString(inputString)) return false;

    return inputString.length === 10;
};

export const IsValidPassword = (inputString) => {
    if (!IsValidString(inputString)) return false;

    return inputString.length > 3;
};

export const IsValidInteger = (inputString) => {
    if (inputString === undefined) return false;

    if (inputString === null) return false;

    return inputString.length > 3;
};

export const IsValidStatusCode = (input) => {
    if (input === undefined) return false;

    return input !== null;
};

export const EqualsIgnoreCase = (firstString, secondString) => {
    if (!IsValidString(firstString)) return false;

    if (!IsValidString(secondString)) return false;

    return firstString.toUpperCase() === secondString.toUpperCase();
};
