import CryptoJS from 'crypto-js';

let actualKey = '*Finovative@key*';
let key = CryptoJS.enc.Utf8.parse(actualKey);
let iv = CryptoJS.enc.Utf8.parse(actualKey);

export const SecurityServices = {
    encryptFun,
    decryptFun
};

function encryptFun(jsonStr) {
    let encryptedString = CryptoJS.AES.encrypt(jsonStr, key, {
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });

    return encryptedString;
}

function decryptFun(resp) {
    let decryptedResponse = CryptoJS.AES.decrypt(resp, key, {
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });
    return decryptedResponse.toString(CryptoJS.enc.Utf8);
}
