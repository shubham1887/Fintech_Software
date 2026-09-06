import React, { useState, useEffect, memo } from 'react';
import { Container } from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';
import HOCLoader from 'Components/Loader/HOCLoader';
import { LOGOUT } from 'ConstComponents';
import {
    Select as SELECT_SETTING,
    Update as SETTING_UPDATE
} from 'Components/Api/CompanyAPI';

const CustomContainer = HOCLoader(Container);
const Company = memo(() => {
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const Select = async () => {
        setLoadingOpen(true);

        const { isAuthorized, status, data } = await SELECT_SETTING();
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);

            return;
        }

        setItemList(JSON.parse(data));
    };

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        Select();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'CompanyMasterID',
            srno: true
        });

        columns.push({
            label: 'Name',
            name: 'Name'
        });

        columns.push({
            label: 'Value',
            name: 'Value',
            textwrapwidth: 100,
            editable: true
        });

        columns.push({
            label: 'Update',
            name: 'Update',
            save: true,
            buttonTitle: 'Update',
            callback: (row) => {
                UpdateClick(row);
            }
        });

        setMainColumns(columns);
    };

    const UpdateClick = async (row) => {
        let jsonObj = {
            CompanyMasterID: row[0],
            Name: row[1],
            Value: row[2]
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const { isAuthorized, status, data } = await SETTING_UPDATE(request);

        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(data);
        setIsError(true);
        if (!status) {
            setSeverity('error');
            return;
        }

        Select();
    };

    const CompanyTableData = (
        <ReactTable data={itemList} columns={mainColumns} />
    );

    const handleContainerCloseCallback = (alertState) => {
        setIsError(alertState);
    };

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={handleContainerCloseCallback}
        >
            {CompanyTableData}
        </CustomContainer>
    );
});

export default Company;
