import React, { useState, useEffect, useRef } from 'react';
import { Container, Grid, MenuItem, Paper } from '@material-ui/core';

import { Formik, Form } from 'formik';
import { LOGOUT } from 'ConstComponents';
import { SelectByOfflineProductMasterID as SELECT_BY_OFFLINEPRODUCTMASTER_ID } from 'Components/Api/CategoryChannelMappingAPI';

import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const ViewChannel = ({ OfflineProductMasterID }) => {
    const classes = commonStyle();
    const formRef = useRef();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const selectData = async (OfflineProductMasterID) => {
        var request = {
            OfflineProductMasterID: OfflineProductMasterID
        };
        var requestJson = JSON.stringify(request);

        setLoadingOpen(true);
        const response = await SELECT_BY_OFFLINEPRODUCTMASTER_ID(requestJson);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        setItemList(data);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ChannelCategoryID',
            srno: true
        });
        columns.push({
            label: 'Category',
            name: 'Category'
        });

        columns.push({
            label: 'Channel',
            name: 'ChannelList',
            arraycheckboxImage: true,
            keyName: 'ChannelName'
        });
        setMainColumns(columns);
    };

    useEffect(() => {
        createColumns();
        selectData(OfflineProductMasterID);
    }, [OfflineProductMasterID]);

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
                rowsPerPage={10}
            />
        </CustomContainer>
    );
};
