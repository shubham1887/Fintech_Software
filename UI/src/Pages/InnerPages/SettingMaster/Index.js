import React, { useState, useEffect } from 'react';
import { Container, Typography, Grid } from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';
import { Select as SELECT_SETTING } from 'Components/Api/SettingMasterApi';
import { IsSecure as SETTINGMASTER_ISSECURE } from 'Components/Api/SettingMasterApi';
import { Update as SETTING_UPDATE } from 'Components/Api/SettingMasterApi';

import ReactTable from 'Components/ReactTable/ReactTable';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const SelectSettingMaster = () => {
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [TakeCount, setTakeCount] = useState(10);
    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const Select = async () => {
        const response = await SELECT_SETTING();
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

    const activate = async (item, value) => {
        var jsonObj = {
            SettingMasterID: item,
            IsSecure: value
        };

        var request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);
        const response = await SETTINGMASTER_ISSECURE(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        setSeverity('success');
        setResponseMessage(response.data);

        Select();
    };

    const UpdateClick = async (row) => {
        let jsonObj = {
            SettingMasterID: row[0],
            Key: row[1],
            Values: row[2]
        };

        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await SETTING_UPDATE(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        setSeverity('success');
        setResponseMessage(response.data);

        Select();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'SettingMasterID',
            srno: true
        });

        columns.push({
            label: 'Key',
            name: 'KeyText'
        });

        columns.push({
            label: 'Value',
            name: 'Value',
            textwrapwidth: 100,
            editableTextBoxOrPassword: true
        });
        columns.push({
            label: 'IsSecure',
            name: 'IsSecure',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                activate(id, value);
            }
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

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        Select();
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
            <Grid container spacing={2}>
                <Grid item xs={12} sm={12} md={7} lg={7}>
                    <Typography>
                        <span style={{ color: 'green' }}>
                            <b>[Secure means not displayed outside web]</b>
                        </span>
                    </Typography>
                </Grid>
            </Grid>
            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={TakeCount}
            />
        </CustomContainer>
    );
};
