import React, { useState, useEffect } from 'react';
import { Container, Grid } from '@material-ui/core';
import { useHistory } from 'react-router';
import Box from '@material-ui/core/Box';
import Button from '@material-ui/core/Button';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';

import { Select as EMAIL_SELECT } from 'Components/Api/EmailSettingApi';
import { ActivateEmail as EMAIL_ACTIVATE } from 'Components/Api/EmailSettingApi';
import { ActivateSSL as EMAIL_ACTIVATE_SSL } from 'Components/Api/EmailSettingApi';

const CustomContainer = HOCLoader(Container);

export const EmailSetting = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const selectData = async () => {
        setLoadingOpen(true);
        const { isAuthorized, status, data } = await EMAIL_SELECT();
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);

            return;
        }

        var d = JSON.parse(data);

        setItemList(d);
    };

    const activate = async (item, value) => {
        var jsonObj = {
            EmailSettingMasterID: item,
            IsActive: value
        };

        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const { isAuthorized, status, data } = await EMAIL_ACTIVATE(request);
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        setResponseMessage(data);

        if (!status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        setSeverity('success');
        selectData();
    };

    const activateSSL = async (item, value) => {
        var jsonObj = {
            EmailSettingMasterID: item,
            IsSSL: value
        };

        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await EMAIL_ACTIVATE_SSL(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };

    const Edit = async (request) => {
        history.push('/EmailSetting/Edit/' + request);
    };

    const Add = async (request) => {
        history.push('/EmailSetting/Add/' + request);
    };

    const createColumns = () => {
        let primaryKeyIndex = 0;
        var columns = [];
        columns.push({
            label: '#',
            name: 'EmailSettingMasterID',
            srno: true
        });
        columns.push({
            label: 'Email',
            name: 'Email'
        });
        columns.push({
            label: 'HostName',
            name: 'HostName'
        });
        columns.push({
            label: 'Port',
            name: 'Port'
        });
        columns.push({
            label: 'SSL',
            name: 'IsSSL',
            activate: true,
            primaryKeyIndex,
            callback: (id, value) => {
                activateSSL(id, value);
            }
        });

        columns.push({
            label: 'Active',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex,
            callback: (id, value) => {
                activate(id, value);
            }
        });

        columns.push({
            label: 'Edit',
            name: 'EmailSettingMasterID',
            edit: true,
            callback: (EmailSettingMasterID) => {
                Edit(EmailSettingMasterID);
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
            <Grid className={classes.footerGrid}>
                <Box className={classes.flexReverse}>
                    <Button
                        variant="contained"
                        color="primary"
                        className={classes.CustomPrimaryButtonStyle}
                        onClick={() => {
                            Add(0);
                            history.push('/EmailSetting/Add');
                        }}
                    >
                        New Email
                    </Button>
                </Box>
            </Grid>

            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
