import React, { useState, useEffect } from 'react';
import { Container } from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';
import { SelectRequired as SELECT_KYC_MASTER } from 'Components/Api/KYCAPI';
import { IsRequiredOnOff as KYC_REQUIRED_ONOFF } from 'Components/Api/KYCAPI';
import ReactTable from 'Components/ReactTable/ReactTable';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const KYCSetting = () => {
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const selectData = async () => {
        const response = await SELECT_KYC_MASTER();
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);

        setItemList(data);
    };

    const activate = async (item, value) => {
        var jsonObj = {
            KycDocumentMasterID: item,
            IsRequired: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await KYC_REQUIRED_ONOFF(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'KycDocumentMasterID',
            srno: true
        });

        columns.push({
            label: 'Document Name',
            name: 'DocumentType'
        });

        columns.push({
            label: 'IsRequired',
            name: 'IsRequired',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                activate(id, value);
            }
        });

        columns.push({
            label: 'RegEx',
            name: 'RegularExpression'
        });

        columns.push({
            label: 'Remark',
            name: 'Remark'
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        selectData();
    }, []);

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
            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
