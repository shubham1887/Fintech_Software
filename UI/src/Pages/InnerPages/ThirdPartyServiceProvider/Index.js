import React, { memo, useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Box,
    Button,
    Paper,
    CircularProgress
} from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import { LOGOUT } from 'ConstComponents';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import {
    Select as SELECT_THIRDPARTYSERVICE,
    SelectByID as SELECT_THIRDPARTYSERVICE_BY_ID,
    Insert as INSERT_THIRDPARTYSERVICE,
    Update as UPDATE_THIRDPARTYSERVICE
} from 'Components/Api/ThirdPartyServiceProviderAPI';
import { API_URL } from 'Config';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const Index = memo(() => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [IsUpdate, setIsUpdate] = useState(false);
    const [CustomContainerProps, setCustomContainerProps] = useState({
        loadingOpen: false,
        isError: false,
        severity: 'success',
        responseMessage: ''
    });
    const [callbackURL, setcallbackURL] = useState('');
    async function fetchcallbackURL() {
        setcallbackURL(await API_URL());
    }

    const [FormObj, setFormObj] = useState({
        ThirdPartyServiceID: 0,
        Name: '',
        Token: '',
        IsActive: false,
        DomainName: '',
        AppID: '',
        AllowedServices: ''
    });

    const FormValidationSchema = Yup.object({
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        DomainName: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' domain name'
        ),
        Token: Yup.string().required(PLEASE_PROVIDE_VALID + ' token'),
        AppID: Yup.string().required(PLEASE_PROVIDE_VALID + ' appid')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);

        setCustomContainerProps({
            isError: false,
            loading: true
        });

        var response = IsUpdate
            ? await UPDATE_THIRDPARTYSERVICE(requestObj)
            : await INSERT_THIRDPARTYSERVICE(requestObj);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setCustomContainerProps({
                isError: true,
                loading: false,
                severity: 'error',
                responseMessage: response.data
            });
            return;
        }
        setCustomContainerProps({
            isError: true,
            loading: false,
            severity: 'success',
            responseMessage: response.data
        });
        resetForm(FormObj);
        setIsUpdate(false);

        setFormObj({
            ThirdPartyServiceProviderID: 0,
            Name: '',
            Token: '',
            IsActive: false,
            DomainName: '',
            AppID: '',
            AllowedServices: ''
        });

        selectData();
    };

    useEffect(() => {
        createColumns();
        selectData();
        fetchcallbackURL();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ThirdPartyServiceProviderID',
            srno: true
        });

        columns.push({
            label: 'Name',
            name: 'Name'
        });

        columns.push({
            label: 'DomainName',
            name: 'DomainName'
        });

        columns.push({
            label: 'Token',
            name: 'Token'
        });

        columns.push({
            label: 'AppID',
            name: 'AppID'
        });

        columns.push({
            label: 'Allowed Services',
            name: 'AllowedServices'
        });

        columns.push({
            label: 'Active',
            name: 'IsActiveStr'
        });

        columns.push({
            label: 'Edit',
            name: 'ThirdPartyServiceProviderID',
            edit: true,
            callback: (ThirdPartyServiceProviderID) => {
                setIsUpdate(true);
                SelectByID(ThirdPartyServiceProviderID);
            }
        });

        setMainColumns(columns);
    };

    const selectData = async () => {
        setCustomContainerProps({
            isError: false,
            loading: true
        });

        const response = await SELECT_THIRDPARTYSERVICE();

        if (!response.isAuthorized) LOGOUT();

        setCustomContainerProps({
            isError: false,
            loading: false
        });

        if (!response.status) {
            setCustomContainerProps({
                isError: true,
                severity: 'error',
                responseMessage: response.data
            });
            return;
        }

        var data = JSON.parse(response.data);

        setItemList(data);
    };

    const SelectByID = async (ThirdPartyServiceProviderID) => {
        setCustomContainerProps({
            isError: false,
            loading: true
        });

        let jsonObj = {
            ThirdPartyServiceProviderID: ThirdPartyServiceProviderID
        };
        const response = await SELECT_THIRDPARTYSERVICE_BY_ID(
            JSON.stringify(jsonObj)
        );
        setCustomContainerProps({
            loading: false
        });
        if (!response.isAuthorized) LOGOUT();

        const { data, status } = response;
        if (!status) {
            setCustomContainerProps({
                ErrorMeeage: data,
                isError: true,
                severity: 'error'
            });
        }

        let d = JSON.parse(data);

        setFormObj({
            ThirdPartyServiceProviderID: d.ThirdPartyServiceProviderID,
            Name: d.Name,
            AppID: d.AppID,
            Token: d.Token,
            IsActive: d.IsActive,
            DomainName: d.DomainName,
            AllowedServices: d.AllowedServices
        });
    };
    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
    };

    return (
        <CustomContainer
            isLoading={CustomContainerProps.loading}
            isError={CustomContainerProps.isError}
            alertMessage={CustomContainerProps.responseMessage}
            severity={CustomContainerProps.severity}
            onCloseCallback={(alertState) => {
                setCustomContainerProps({
                    isError: alertState
                });
            }}
        >
            <Grid container spacing={3}>
                <Grid item xs={6}>
                    <h2 className={classes.PaperHead_h2}>
                        Third Party Service Provider Settings
                    </h2>
                </Grid>
                <Grid item xs={6}></Grid>
            </Grid>
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
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <MaterialTextField
                                                label="Name"
                                                name="Name"
                                                type="text"
                                                readOnly={true}
                                                placeholder="Name"
                                            />
                                        </Grid>

                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <MaterialTextField
                                                label="Token / secret key"
                                                name="Token"
                                                type="text"
                                                placeholder="Token / secret key"
                                            />
                                        </Grid>
                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <MaterialTextField
                                                label="AppID"
                                                name="AppID"
                                                type="text"
                                                placeholder="AppID"
                                            />
                                        </Grid>
                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <MaterialTextField
                                                label="Domain Name"
                                                name="DomainName"
                                                type="text"
                                                placeholder="DomainName"
                                            />
                                        </Grid>
                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <MaterialTextField
                                                label="Allowed Services"
                                                name="AllowedServices"
                                                type="text"
                                                placeholder="AllowedServices"
                                            />
                                        </Grid>
                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <FormikMaterialSwitch
                                                label="IsActive"
                                                name="IsActive"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnChange(
                                                        name,
                                                        value,
                                                        formProps.setFieldValue
                                                    );
                                                }}
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
                                            disabled={
                                                CustomContainerProps.loading
                                            }
                                        >
                                            {IsUpdate ? 'Update' : 'Save'}
                                        </Button>
                                        {CustomContainerProps.loading && (
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
            </Formik>

            <ReactTable data={itemList} columns={mainColumns} />
            <Paper>
                <Grid item xs={12}>
                    <label>
                        In InstantPay Prime Web SDK Transaction Confirmation
                        Webhook, use "{callbackURL}
                        InstantPayValidationConfirmation/ConfirmTransaction"
                    </label>
                    <br></br>
                    <label>
                        In InstantPay Prime Web SDK Transaction Webhook, use "
                        {callbackURL}
                        InstantPayTransactionConfirmation/ConfirmTransaction"
                    </label>
                    <br></br>

                    <label>
                        In InstantPay Prime Web SDK Balance Webhook, use "
                        {callbackURL}
                        InstantPayBalance/GetBalance"
                    </label>
                    <br></br>
                </Grid>
            </Paper>
        </CustomContainer>
    );
});
