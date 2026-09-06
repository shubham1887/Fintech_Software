import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { LOGOUT } from 'ConstComponents';

import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import {
    SelectByID as SELECT_BY_ID,
    Update as SETTINGS_UPDATE
} from 'Components/Api/ReconciliationSettingsApi';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { Container, Grid, Paper, Button, MenuItem } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID,
    GET_PRIMARY_ID_FROM_URL
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const ReconciliationSettingsEdit = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [primaryID] = useState(GET_PRIMARY_ID_FROM_URL());
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [providerList, setProviderList] = useState([]);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);

        var response = await SETTINGS_UPDATE(requestObj);
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

        setTimeout(() => {
            history.push('/ReconciliationSettings');
        }, MESSAGE_CLOSE_AFTER);
    };
    const [FormObj, setFormObj] = useState({
        ReconciliationSettingID: primaryID,
        ProviderID: 0,
        ConsumerNumber: '',
        Amount: '',
        Status: '',
        WorksheetName: '',
        APIUserRequestID: ''
    });

    const FormValidationSchema = Yup.object({
        ProviderID: Yup.string().required(PLEASE_PROVIDE_VALID + 'provider'),
        ConsumerNumber: Yup.string().required('*'),
        Amount: Yup.string().required('*'),
        Status: Yup.string().required('*'),
        WorksheetName: Yup.string().required('*'),
        APIUserRequestID: Yup.string().required('*')
    });

    const selectProvider = async () => {
        const response = await PROVIDER_DROP_DOWN();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
    };

    const SelectByID = async () => {
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
            APIUserRequestID: data.APIUserRequestID
        });
    };
    useEffect(() => {
        selectProvider();
        SelectByID();
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
                        <Form>
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Reconciliation Settings
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <Box className={classes.flexReverse}>
                                        <Button
                                            variant="contained"
                                            color="primary"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            onClick={() => {
                                                history.push(
                                                    '/ReconciliationSettings'
                                                );
                                            }}
                                        >
                                            Settings
                                        </Button>
                                    </Box>
                                </Grid>
                            </Grid>

                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                name="ProviderID"
                                                label="Provider"
                                                title="Provider"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Provider
                                                </MenuItem>
                                                {providerList &&
                                                    providerList.map(
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
                                            md={3}
                                            lg={3}
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
                                            md={3}
                                            lg={3}
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
                                            md={3}
                                            lg={3}
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
                                            md={3}
                                            lg={3}
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
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="APIUserRequestID"
                                                name="APIUserRequestID"
                                                type="text"
                                                placeholder="APIUserRequestID"
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
                                            title="Update"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Update
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
        </CustomContainer>
    );
};
