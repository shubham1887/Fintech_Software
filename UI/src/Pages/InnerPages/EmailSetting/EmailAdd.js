import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { LOGOUT } from 'ConstComponents';
import { Add as EMAIL_ADD } from 'Components/Api/EmailSettingApi';

import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

import { Container, Grid, Paper, Button } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';

import { SelectByID as EMAIL_SELECT_BY_ID } from 'Components/Api/EmailSettingApi';
import { Select as EMAIL_SELECT } from 'Components/Api/EmailSettingApi';
import { Update as EMAIL_UPDATE } from 'Components/Api/EmailSettingApi';
import { ActivateEmail as EMAIL_ACTIVATE } from 'Components/Api/EmailSettingApi';
import { ActivateSSL as EMAIL_ACTIVATE_SSL } from 'Components/Api/EmailSettingApi';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const EmailAdd = () => {
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);

    const [FormObj, setFormObj] = useState({
        EmailSettingMasterID: 0,
        Email: '',
        Password: '',
        HostName: '',
        Port: 0
    });

    const FormValidationSchema = Yup.object({
        Email: Yup.string().required(PLEASE_PROVIDE_VALID + 'email ID'),
        Password: Yup.string().required(PLEASE_PROVIDE_VALID + 'password'),
        HostName: Yup.string().required(PLEASE_PROVIDE_VALID + 'host name'),
        Port: Yup.number()
            .moreThan(-1, PLEASE_PROVIDE_VALID + 'port number')
            .required(PLEASE_PROVIDE_VALID + 'port number')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);

        var response =
            values.EmailSettingMasterID === 0
                ? await EMAIL_ADD(requestObj)
                : await EMAIL_UPDATE(requestObj);

        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        setIsError(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setLoading(false);
            setSeverity('error');
            return;
        }
        resetForm(FormObj);
        setFormObj({
            EmailSettingMasterID: 0,
            Email: '',
            Password: '',
            HostName: '',
            Port: 0
        });
        selectData();
    };
    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        selectData();
    }, []);
    const selectData = async () => {
        const response = await EMAIL_SELECT();
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
    const Edit = async (primaryID) => {
        selectEmailByID(primaryID);
    };
    const activate = async (item, value) => {
        var jsonObj = {
            EmailSettingMasterID: item,
            IsActive: value
        };

        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await EMAIL_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setResponseMessage(response.data);

        if (!response.status) {
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
    const selectEmailByID = async (primaryID) => {
        let JSONObj = {
            EmailSettingMasterID: primaryID
        };
        setLoading(true);
        var request = JSON.stringify(JSONObj);

        const response = await EMAIL_SELECT_BY_ID(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);

        setFormObj({
            EmailSettingMasterID: data.EmailSettingMasterID,
            Email: data.Email,
            Password: data.Password,
            HostName: data.HostName,
            Port: data.Port
        });
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
            <Formik
                enableReinitialize={true}
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Email Setting
                                    </h2>
                                </Grid>
                            </Grid>

                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialTextField
                                                label="Email ID"
                                                name="Email"
                                                type="text"
                                                placeholder="Email ID"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialTextField
                                                label="Password"
                                                name="Password"
                                                type="password"
                                                placeholder="Password"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialTextField
                                                label="Host Name"
                                                name="HostName"
                                                type="text"
                                                placeholder="Host Name"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialNumberField
                                                label="Port"
                                                name="Port"
                                                type="text"
                                                length={7}
                                                placeholder="Port"
                                            />
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Save"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Save
                                        </Button>
                                        {loading && (
                                            <CircularProgress
                                                size={24}
                                                className={
                                                    classes.buttonProgress
                                                }
                                            />
                                        )}
                                    </div>
                                    <Button
                                        type="reset"
                                        variant="contained"
                                        color="secondary"
                                        title="Reset"
                                        className={
                                            classes.CustomDangerButtonStyle
                                        }
                                    >
                                        Reset
                                    </Button>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
