import React, { useEffect, useState } from 'react';
import { useHistory } from 'react-router';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';

import { Select as PROVIDER_SELECT } from 'Components/Api/ProviderApi';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import { Select as UrlType_SELECT } from 'Components/Api/UrlTypeApi';
import { SelectByID as PROVIDERDETAIL_SELECT_BY_ID } from 'Components/Api/ProviderDetailAPI';
import { Update as PROVIDER_DETAIL_UPDATE } from 'Components/Api/ProviderDetailAPI';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import { Container, Grid, MenuItem, Paper, Button } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';

import Box from '@material-ui/core/Box';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const ProviderDetailEdit = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [Services, setServices] = useState([]);
    const [Providers, setProviders] = useState([]);
    const [UrlTypes, setUrlType] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [HeaderJson, setHeaderJson] = useState('none');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [ProviderObj, SetProviderObject] = useState({
        ProviderName: '',
        ServiceName: '',
        ProviderID: 0,
        ServiceID: 0,
        UrlType: '0',
        ProviderURL: '',
        MethodType: '0',
        HeaderJson: '0',
        IsPendingProvider: false
    });
    useEffect(() => {
        setLoadingOpen(true);
        selectServices();
        selectProviders();
        selectUrlType();
        selectProviderDetails();
        setLoadingOpen(false);
    }, []);

    const OnChange = (name, value, setFieldValue) => {
        if (value === 'POST') {
            setHeaderJson('inline');
        }
        if (value === 'GET') {
            setHeaderJson('none');
        }
    };

    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);
        setServices(data);
    };
    const selectProviders = async () => {
        const response = await PROVIDER_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);
        setProviders(data);
    };
    const selectUrlType = async () => {
        const response = await UrlType_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }
        var data = JSON.parse(response.data);
        setUrlType(data);
    };

    const selectProviderDetails = async () => {
        let URL = window.location.pathname;
        var res = URL.split('/');

        let primaryID = res[res.length - 1];

        let JSONObj = {
            ProviderDetailID: primaryID
        };

        var request = JSON.stringify(JSONObj);

        const response = await PROVIDERDETAIL_SELECT_BY_ID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        var data = JSON.parse(response.data);

        if (data.MethodType === 'POST') {
            setHeaderJson('inline');
        }
        if (data.MethodType === 'GET') {
            setHeaderJson('none');
        }

        SetProviderObject({
            ...ProviderObj,
            ProviderID: data.ProviderID,
            ProviderDetailID: data.ProviderDetailID,
            ProviderName: data.ProviderName,
            ServiceID: data.ServiceID,
            ServiceName: data.ServiceName,
            UrlType: data.UrlType,
            ProviderURL: data.ProviderURL,
            MethodType: data.MethodType,
            HeaderJson: data.HeaderJson,
            IsPendingProvider: data.IsPendingProvider
        });
    };

    const ProviderDetailUpdate = async (
        values,
        { setSubmitting, resetForm }
    ) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }

        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        setIsError(false);
        setSubmitting(false);

        var response = await PROVIDER_DETAIL_UPDATE(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }

        setIsError(true);
        setSubmitting(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }

        resetForm(ProviderObj);

        setTimeout(() => {
            history.push('/Providers');
        }, MESSAGE_CLOSE_AFTER);
    };
    const FormValidationSchema = Yup.object({
        ProviderID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'providerID')
            .required(PLEASE_SELECT_VALID + 'providerID'),
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'ServiceID')
            .required(PLEASE_SELECT_VALID + 'serviceID'),
        UrlType: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'UrlType')
            .required(PLEASE_SELECT_VALID + 'UrlType'),
        ProviderURL: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'Provider URL'
        ),
        MethodType: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'MethodType')
            .required(PLEASE_SELECT_VALID + 'MethodType')
    });

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
                initialValues={ProviderObj}
                validationSchema={FormValidationSchema}
                onSubmit={ProviderDetailUpdate}
                resetForm={ProviderObj}
                render={({ values, setFieldValue, handleChange }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Provider Edit
                            </h2>
                            <Grid tem xs={12} sm={12} md={12} lg={12}>
                                <h3 className={classes.Provider_Format}>
                                    Note : use Amount=[Amount], Number=[Number],
                                    Operator=[Operator], RequestID=[RequestID],
                                    Type=[Type] for URL parameters
                                </h3>
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
                                            <MaterialSelect
                                                label="Provider"
                                                name="ProviderID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Providers
                                                </MenuItem>
                                                {Providers.map((Item) => (
                                                    <MenuItem
                                                        key={Item.ProviderID}
                                                        value={Item.ProviderID}
                                                    >
                                                        {Item.ProviderName}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialSelect
                                                label="Service"
                                                name="ServiceID"
                                                title="Service"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Service
                                                </MenuItem>
                                                {Services.map((Item) => (
                                                    <MenuItem
                                                        key={Item.ServiceID}
                                                        value={Item.ServiceID}
                                                    >
                                                        {Item.ServiceName}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="UrlType"
                                                name="UrlType"
                                                title="UrlType"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select UrlType
                                                </MenuItem>
                                                {UrlTypes.map((Item) => (
                                                    <MenuItem
                                                        key={Item.UrlType}
                                                        value={Item.UrlType}
                                                    >
                                                        {Item.UrlType}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={10}
                                            lg={10}
                                        >
                                            <MaterialTextField
                                                label="Provider URL"
                                                name="ProviderURL"
                                                type="text"
                                                placeholder="Provider URL"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialSelect
                                                label="MethodType"
                                                name="MethodType"
                                                title="MethodType"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnChange(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Type
                                                </MenuItem>

                                                <MenuItem key="GET" value="GET">
                                                    GET
                                                </MenuItem>
                                                <MenuItem
                                                    key="POST"
                                                    value="POST"
                                                >
                                                    POST
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <Box display={HeaderJson}>
                                                <MaterialTextField
                                                    label="HeaderJson"
                                                    name="HeaderJson"
                                                    type="text"
                                                    placeholder="HeaderJson"
                                                />
                                            </Box>
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
                                            disabled={loadingOpen}
                                        >
                                            Update
                                        </Button>
                                        {loadingOpen && (
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
            />
        </CustomContainer>
    );
};
