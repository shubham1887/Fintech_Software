import React, { useState, useEffect } from 'react';

import { Container } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';

import HOCLoader from 'Components/Loader/HOCLoader';
import { HeaderSummary } from 'Pages/InnerPages/Users/BalanceHeaderSummary';

import { SelectDownlineReport as SELECT_DOWNLINE_REPORT } from 'Components/Api/UsersApi';

const CustomContainer = HOCLoader(Container);

export const DownlinePopup = ({ UserID }) => {
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [openFilter, setOpenFilter] = useState(true);
    const [mainColumns, setMainColumns] = useState([]);
    const [headerSummary, setHeaderSummary] = useState([]);
    const [data, setData] = useState([]);
    const openHandler = (value) => {
        setOpenFilter(value);
    };

    var IsCashCollection = localStorage.getItem('IsCashCollection');

    const SelectByParentID = async (UserID) => {
        var inputObj = {
            UserID: UserID
        };
        setLoading(true);
        setData([]);

        var requestJSON = JSON.stringify(inputObj);

        const { isAuthorized, status, data } = await SELECT_DOWNLINE_REPORT(
            requestJSON
        );
        setLoading(false);
        if (!isAuthorized) LOGOUT();
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);
            return;
        }

        var d = JSON.parse(data);
        setData(d.data);
        setHeaderSummary(d.headerlist);
    };

    useEffect(() => {
        SelectByParentID(UserID);
        createColumns();
    }, [UserID]);
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
            label: 'Role',
            name: 'Role'
        });
        columns.push({
            label: 'Balance',
            name: 'Balance',
            Wallets: true
        });
        if (IsCashCollection === 'true') {
            columns.push({
                label: 'Outstanding',
                name: 'Outstanding',
                numeric: true
            });
        }
        setMainColumns(columns);
    };
    return (
        <CustomContainer
            isLoading={loading}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <ReactTable data={data} columns={mainColumns} rowsPerPage={4} />
            <HeaderSummary headerSummary={headerSummary} />
        </CustomContainer>
    );
};
