import React, { useState, useEffect } from 'react';
import { Container } from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';
import { SelectOusideRoleSetting as SELECT_OUTSIDE_ROLE } from 'Components/Api/UserTypeApi';
import { IsOutsideActivate as ISOUTSIDE_ACTIVATE } from 'Components/Api/UserTypeApi';

import ReactTable from 'Components/ReactTable/ReactTable';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const OutsideRoleSetting = () => {
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const selectData = async () => {
        const response = await SELECT_OUTSIDE_ROLE();
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

    const IsOutsideActivate = async (item, value) => {
        var jsonObj = {
            UserTypeID: item,
            IsOutside: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await ISOUTSIDE_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        setSeverity('success');
        setResponseMessage(response.data);

        selectData();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserTypeID',
            srno: true
        });

        columns.push({
            label: 'Role',
            name: 'UserType'
        });

        columns.push({
            label: 'IsOutside',
            name: 'IsOutside',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                IsOutsideActivate(id, value);
            }
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
            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={50}
            />
        </CustomContainer>
    );
};
