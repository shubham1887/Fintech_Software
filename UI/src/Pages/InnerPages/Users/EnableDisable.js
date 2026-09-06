import React, { useState, useEffect } from 'react';
import { Container } from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';

import {
    SelectUser as SELECT_BY_USERID,
    Activate as USER_ACTIVATE,
    LoginOTPActivate as USER_LOGIN_OTP_ACTIVATE,
    IsInvoiceActivate as ISINVOICE_ACTIVATE,
    CommissionHoldActivate as USERS_COMMISSION_HOLD_ACTIVATE
} from 'Components/Api/UsersApi';

import ReactTable from 'Components/ReactTable/ReactTable';

import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const EnableDisablePopUp = ({ UserID }) => {
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [UsersList, setUsersList] = useState([]);

    const IsInvoiceActivate = async (item, value) => {
        var jsonObj = {
            UserID: item,
            IsActive: value
        };

        setIsError(false);
        setLoadingOpen(true);
        var request = JSON.stringify(jsonObj);

        const response = await ISINVOICE_ACTIVATE(request);

        if (!response.isAuthorized) LOGOUT();
        setLoadingOpen(false);

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) setSeverity('error');

        selectByUserID(UserID);
    };

    const activate = async (item, value) => {
        var jsonObj = {
            UserID: item,
            IsActive: value
        };

        setIsError(false);
        setLoadingOpen(true);
        var request = JSON.stringify(jsonObj);

        const response = await USER_ACTIVATE(request);

        if (!response.isAuthorized) LOGOUT();
        setLoadingOpen(false);

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) setSeverity('error');

        selectByUserID(UserID);
    };

    const activateLoginOTP = async (item, value) => {
        var jsonObj = {
            UserID: item,
            IsLoginOTP: value
        };

        setIsError(false);
        setLoadingOpen(true);
        var request = JSON.stringify(jsonObj);

        const response = await USER_LOGIN_OTP_ACTIVATE(request);

        if (!response.isAuthorized) LOGOUT();
        setLoadingOpen(false);

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) setSeverity('error');

        selectByUserID(UserID);
    };
    const activateIsCommissionHold = async (item, value) => {
        var jsonObj = {
            UserID: item,
            IsCommissionHold: value
        };

        setIsError(false);
        setLoadingOpen(true);
        var request = JSON.stringify(jsonObj);

        const response = await USERS_COMMISSION_HOLD_ACTIVATE(request);

        if (!response.isAuthorized) LOGOUT();
        setLoadingOpen(false);

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) setSeverity('error');

        selectByUserID(UserID);
    };

    useEffect(() => {
        selectByUserID(UserID);
        createColumns();
    }, [UserID]);

    const selectByUserID = async (UserID) => {
        var jsonObj = {
            UserID: UserID
        };

        let requestObj = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setUsersList([]);
        const response = await SELECT_BY_USERID(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;
        var data = JSON.parse(response.data);

        let arr = [];
        arr.push(data);
        setUsersList(arr);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });

        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });
        columns.push({
            label: 'Active',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                activate(id, value);
            }
        });

        columns.push({
            label: 'Login OTP',
            name: 'IsLoginOTP',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                activateLoginOTP(id, value);
            }
        });

        columns.push({
            label: 'IsCommissionHold',
            name: 'IsCommissionHold',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                activateIsCommissionHold(id, value);
            }
        });

        columns.push({
            label: 'Invoice',
            name: 'IsInvoiceApplicable',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                IsInvoiceActivate(id, value);
            }
        });

        columns.push({
            label: 'Profile',
            name: 'ProfileImagePath',
            Image: true
        });

        setMainColumns(columns);
    };

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <ReactTable
                title={'User'}
                data={UsersList}
                columns={mainColumns}
                headerToolbar={false}
                footerToolbar={false}
            />
        </CustomContainer>
    );
};
