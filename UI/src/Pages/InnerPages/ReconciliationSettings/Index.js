import React, { useState, useEffect } from 'react';

import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import {
    SelectByID as SELECT_BY_ID,
    Select as SETTINGS_ALL,
    Insert as SETTINGS_ADD,
    Update as SETTINGS_UPDATE,
    Delete as SETTINGS_DELETE
} from 'Components/Api/ReconciliationSettingsApi';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import {
    Container,
    Grid,
    Paper,
    MenuItem,
    CircularProgress
} from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import ReactTable from 'Components/ReactTable/ReactTable';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';

const CustomContainer = HOCLoader(Container);

export const ReconciliationSettings = () => {
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [providerList, setProviderList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [IsUpdate, setIsUpdate] = useState(false);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);

        var response = IsUpdate
            ? await SETTINGS_UPDATE(requestObj)
            : await SETTINGS_ADD(requestObj);
        if (!response.isAuthorized) LOGOUT();

        setIsError(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setLoading(false);
            setSeverity('error');
            resetForm(FormData);
            return;
        }
        setIsUpdate(false);
        resetForm(FormData);

        selectData();
    };
    var FormData = {
        ReconciliationSettingID: 0,
        ProviderID: 0,
        ConsumerNumber: '',
        Amount: '',
        Status: '',
        WorksheetName: '',
        APIUserRequestID: '',
        SuccessStatusAs: '',
        FailureStatusAs: ''
    };
    const [FormObj, setFormObj] = useState({
        ReconciliationSettingID: 0,
        ProviderID: 0,
        ConsumerNumber: '',
        Amount: '',
        Status: '',
        WorksheetName: '',
        SuccessStatusAs: '',
        FailureStatusAs: '',
        APIUserRequestID: ''
    });

    const FormValidationSchema = Yup.object({
        ProviderID: Yup.string().required(PLEASE_PROVIDE_VALID + 'provider'),
        ConsumerNumber: Yup.string().required('*'),
        Amount: Yup.string().required('*'),
        Status: Yup.string().required('*'),
        WorksheetName: Yup.string().required('*'),
        APIUserRequestID: Yup.string().required('*'),
        SuccessStatusAs: Yup.string().required('*'),
        FailureStatusAs: Yup.string().required('*')
    });

    const selectProvider = async () => {
        const response = await PROVIDER_DROP_DOWN();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ReconciliationSettingID',
            srno: true
        });

        columns.push({
            label: 'ProviderName',
            name: 'ProviderName'
        });
        columns.push({
            label: 'WorksheetName',
            name: 'WorksheetName'
        });
        columns.push({
            label: 'ConsumerNumber',
            name: 'ConsumerNumber'
        });

        columns.push({
            label: 'Amount',
            name: 'Amount'
        });
        columns.push({
            label: 'Status',
            name: 'Status'
        });

        columns.push({
            label: 'APIUserRequestID',
            name: 'APIUserRequestID'
        });

        columns.push({
            label: 'SuccessStatusAs',
            name: 'SuccessStatusAs'
        });
        columns.push({
            label: 'FailureStatusAs',
            name: 'FailureStatusAs'
        });
        columns.push({
            label: 'Edit',
            name: 'ReconciliationSettingID',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'ReconciliationSettingID',
            delete: true,
            callback: (value) => {
                DeleteReconcile(value);
            }
        });

        setMainColumns(columns);
    };
    const DeleteReconcile = async (request) => {
        DeleteReconciliation(request);
    };

    const DeleteReconciliation = async (primaryID) => {
        let jsonObj = {
            ReconciliationSettingID: primaryID
        };
        setLoading(true);
        const response = await SETTINGS_DELETE(JSON.stringify(jsonObj));
        setLoading(false);

        const { isAuthorized, status, data } = response;

        if (!isAuthorized) LOGOUT();

        setResponseMessage(data);
        setIsError(true);

        if (!status) {
            setSeverity('error');
            return;
        }
        setSeverity('success');

        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);
    };

    const Edit = async (request) => {
        setIsUpdate(true);
        SelectByID(request);
    };

    const selectData = async () => {
        const response = await SETTINGS_ALL();
        setLoading(false);

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

    const SelectByID = async (primaryID) => {
        let jsonObj = {
            ReconciliationSettingID: primaryID
        };

        const response = await SELECT_BY_ID(JSON.stringify(jsonObj));

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setFormObj({
            ReconciliationSettingID: data.ReconciliationSettingID,
            ProviderID: data.ProviderID,
            ConsumerNumber: data.ConsumerNumber,
            Amount: data.Amount,
            Status: data.Status,
            WorksheetName: data.WorksheetName,
            APIUserRequestID: data.APIUserRequestID,
            SuccessStatusAs: data.SuccessStatusAs,
            FailureStatusAs: data.FailureStatusAs
        });
    };

    useEffect(() => {
        selectProvider();
        createColumns();
        selectData();
    }, []);

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
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Reconciliation Settings
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
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                name="ProviderID"
                                                label="Provider"
                                                title="Provider"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Provider
                                                </MenuItem>
                                                {providerList.map(
                                                    (provider) => (
                                                        <MenuItem
                                                            key={
                                                                provider.ProviderID
                                                            }
                                                            value={
                                                                provider.ProviderID
                                                            }
                                                        >
                                                            {
                                                                provider.ProviderName
                                                            }
                                                        </MenuItem>
                                                    )
                                                )}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="WorksheetName"
                                                name="WorksheetName"
                                                type="text"
                                                placeholder="WorksheetName"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="Consumer Number"
                                                name="ConsumerNumber"
                                                type="text"
                                                placeholder="ConsumerNumber"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="Status"
                                                name="Status"
                                                type="text"
                                                placeholder="Status"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="Amount"
                                                name="Amount"
                                                type="text"
                                                placeholder="Amount"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="APIUserRequestID"
                                                name="APIUserRequestID"
                                                type="text"
                                                placeholder="APIUserRequestID"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="SuccessStatusAs"
                                                name="SuccessStatusAs"
                                                type="text"
                                                placeholder="SuccessStatusAs"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="FailureStatusAs"
                                                name="FailureStatusAs"
                                                type="text"
                                                placeholder="FailureStatusAs"
                                            />
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
                                                title={
                                                    IsUpdate ? 'Update' : 'Save'
                                                }
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loading}
                                            >
                                                {IsUpdate ? 'Update' : 'Save'}
                                            </PrimaryButton>
                                            {loading && (
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
                                            md={2}
                                            lg={2}
                                        >
                                            <SecondaryButton
                                                type="reset"
                                                variant="contained"
                                                color="secondary"
                                                title="Reset"
                                                className={
                                                    classes.CustomDangerButtonStyle
                                                }
                                            >
                                                Reset
                                            </SecondaryButton>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
