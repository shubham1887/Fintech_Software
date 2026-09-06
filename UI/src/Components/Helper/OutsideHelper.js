import { SecurityServices } from '../EncryptDecrypt/Crypto';
import { SERVER_DOWN_ERROR, UNABLE_TO_PROCESS_REQUEST_ERROR } from './Messages';
import { IsValidString, IsValidStatusCode } from './Validation';
import {
    RefreshJWTToken as REFRESH_TOKEN,
    logoutUser as LOGOUT_USER
} from 'Components/Api/UsersApi';
import axios from 'axios';
import { API_URL } from 'Config';

const getBaseUrl = async () => {
    const BASE_URL = await API_URL();
    return BASE_URL;
};

Date.prototype.addDays = function (days) {
    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
};

Number.prototype.countDecimals = function () {
    if (Math.floor(this.valueOf()) === this.valueOf()) return 0;
    return this.toString().split('.')[1].length || 0;
};

export var KEY_DATA = '';

const axiosInstance = axios.create({
    timeout: 5 * 60 * 1000
});

axiosInstance.interceptors.request.use(
    async (config) => {
        config.baseURL = await getBaseUrl();
        return config;
    },
    (error) => Promise.reject(error)
);

export const GetDefaultResponse = {
    status: false,
    data: ''
};

export const getAuthorizationHeader = () => {
    return localStorage.getItem('SecureToken');
};

export const OutsideGetFormData = (Request) => {
    var form = new FormData();
    form.append('Request', SecurityServices.encryptFun(Request));
    form.append(
        'KeyData',
        SecurityServices.encryptFun(getOutsideAuthorizationHeader())
    );
    return form;
};

export const getOutsideAuthorizationHeader = () => {
    return localStorage.getItem('OutsideToken');
};

export const OutsidePostAPI = async (request, url) => {
    let response = GetResponseClass();
    let form = OutsideGetFormData(request);

    try {
        const { status, data } = await axiosInstance.post(url, form, {
            headers: {
                Authorization: 'Bearer ' + getOutsideAuthorizationHeader()
            }
        });

        return ParseResponse(status, data, response);
    } catch (error) {
        return ParseErrorResponse(error, response);
    }
};

export const logout = async () => {
    window.location.href = '/SDKAccessDenied';
};

export const GetResponseClass = () => {
    return {
        isAuthorized: false,
        status: false,
        data: UNABLE_TO_PROCESS_REQUEST_ERROR
    };
};

export const ParseResponse = (status, data, response) => {
    if (!IsValidStatusCode(status)) {
        response.data = UNABLE_TO_PROCESS_REQUEST_ERROR;
        return response;
    }

    if (status !== 200) {
        response.data = UNABLE_TO_PROCESS_REQUEST_ERROR + status;
        return response;
    }
    if (status === 401) {
        response.data = UNABLE_TO_PROCESS_REQUEST_ERROR + status;

        response.isAuthorized = false;
        return response;
    }

    if (!IsValidString(data.body)) {
        response.data = UNABLE_TO_PROCESS_REQUEST_ERROR;
        return response;
    }

    let decryptedBody = SecurityServices.decryptFun(data.body);

    if (!IsValidString(decryptedBody)) {
        response.data = UNABLE_TO_PROCESS_REQUEST_ERROR;
        return response;
    }
    response.isAuthorized = true;
    response.status = data.status;
    response.data = decryptedBody;

    return response;
};

const UpdateJWTRefreshToken = async () => {
    let { status, data } = await REFRESH_TOKEN();

    if (!status) logout();

    data = JSON.parse(data);

    if (data.SecureToken === undefined) logout();

    if (data.SecureToken === null) logout();

    localStorage.setItem('RefreshToken', data.RefreshToken);
    localStorage.setItem('SecureToken', data.SecureToken);
    localStorage.setItem('UserID', data.UserID);
    localStorage.setItem('FirmName', data.FirmName);
    localStorage.setItem('MobileNumber', data.MobileNumber);
    localStorage.setItem('UserType', data.UserType);
    localStorage.setItem('UserTypeID', data.UserTypeID);
};

export const ParseErrorResponse = (error, returnResponse) => {
    if (error === undefined) {
        logout();
        return;
    }

    if (error.response === undefined) {
        logout();
        return;
    }

    let status = error.response.status;

    if (status === 401) {
        UpdateJWTRefreshToken();
        returnResponse.isAuthorized = true;
        return returnResponse;
    }

    if (error.message === 'Network Error') {
        returnResponse.data = SERVER_DOWN_ERROR;
        return returnResponse;
    }
    returnResponse.isAuthorized = false;
    returnResponse.data = 'Request failed with : ' + error.response.status;
    return returnResponse;
};
