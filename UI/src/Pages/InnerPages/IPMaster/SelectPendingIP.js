import React, { useState, useEffect } from 'react';
import { Container, Grid, Paper, MenuItem, Button } from '@material-ui/core';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import {
    SelectPendingIP as SELECT_PENDING_IP,
    Delete as DELETE_IP
} from 'Components/Api/IPAddAPI';
import {
    UpdateStatus as UPDATE_STATUS,
    Select as SELECT
} from 'Components/Api/IPAddAPI';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import CircularProgress from '@material-ui/core/CircularProgress';

import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import Box from '@material-ui/core/Box';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';

import { Formik, Form } from 'formik';
import { PrimaryButton } from 'Components/Inputes/CustomButton';

const CustomContainer = HOCLoader(Container);

export const PendingIP = () => {
    const classes = commonStyle();

    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const FormObj = {
        Status: 0
    };

    const statusList = [
        {
            ID: 'APPROVE',
            Type: 'APPROVE'
        },
        {
            ID: 'REJECT',
            Type: 'REJECT'
        }
    ];

    useEffect(() => {
        select();
        createColumns();
    }, []);

    const select = async () => {
        var inputObj = {
            Status: 'PROCESS'
        };
        setLoadingOpen(true);
        setItemList([]);
        var requestObj = JSON.stringify(inputObj);

        const response = await SELECT_PENDING_IP(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);
        setItemList(data);
    };

    const onUpdateClick = async (row) => {
        let requestObj = {
            IPMasterID: row[0],
            Status: row[4]
        };

        var jsonReq = JSON.stringify(requestObj);
        setLoadingOpen(true);
        const response = await UPDATE_STATUS(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        select();
    };

    const onDeleteClick = (item) => {
        var RequestObj = { IpMasterID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (requestObj) => {
        setLoadingOpen(true);
        var reqObj = JSON.stringify(requestObj);
        var response = await DELETE_IP(reqObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }

        window.location.reload();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'IPMasterID',
            srno: true
        });

        columns.push({
            label: 'Name',
            name: 'FirmName'
        });

        columns.push({
            label: 'IPAddress',
            name: 'IPAddress'
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        columns.push({
            label: 'Action',
            name: 'Status',
            dropdown: true,
            list: statusList
        });

        columns.push({
            label: 'Update',
            name: 'IPMasterID',
            save: true,
            buttonTitle: 'Update',
            callback: (row) => {
                onUpdateClick(row);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'IPMasterID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });

        setMainColumns(columns);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            Status: values.Status
        };
        setLoadingOpen(true);
        setItemList([]);
        var requestObj = JSON.stringify(inputObj);

        const response = await SELECT_PENDING_IP(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);
        setItemList(data);
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
            <Formik
                enableReinitialize={true}
                initialValues={FormObj}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {({ values, errors, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>Pending IP</h2>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Status"
                                                name="Status"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Type
                                                </MenuItem>
                                                <MenuItem
                                                    key="PROCESS"
                                                    value="PROCESS"
                                                >
                                                    PROCESS
                                                </MenuItem>

                                                <MenuItem
                                                    key="APPROVE"
                                                    value="APPROVE"
                                                >
                                                    APPROVE
                                                </MenuItem>
                                                <MenuItem
                                                    key="REJECT"
                                                    value="REJECT"
                                                >
                                                    REJECT
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="submit"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                fullWidth
                                                disabled={loadingOpen}
                                            >
                                                Search
                                            </PrimaryButton>
                                            {loadingOpen && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        ></Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                            <Grid className={classes.footerGrid}></Grid>
                        </Form>
                    );
                }}
            </Formik>

            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this IP."
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Delete(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
            <ReactTable
                data={itemList}
                rowsPerPage={100}
                columns={mainColumns}
            />
        </CustomContainer>
    );
};
