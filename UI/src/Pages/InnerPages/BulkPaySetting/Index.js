import React, { useState, useEffect } from 'react';
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
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    Select as SELECT,
    Update as UPDATE,
    SelectByID as SELECT_BY_ID
} from 'Components/Api/BulkPaySettingAPI';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import ReactTable from 'Components/ReactTable/ReactTable';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { API_URL } from 'Config';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const BulkPaySetting = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [IsUpdate, setIsUpdate] = useState(false);
    const [callbackURL, setcallbackURL] = useState('');
    async function fetchcallbackURL() {
        setcallbackURL(await API_URL());
    }
    const [FormObj, setFormObj] = useState({
        BulkPaySettingID: 0,
        Name: '',
        Mode: '',
        MerchantID: '',
        Slat: '',
        Token: '',
        ActionURL: '',
        StatusCheckURL: '',
        MinimumAmount: 0,
        MaximumAmount: 0,
        IsActive: false,
        IsPendingForFailed: false
    });
    const FormValidationSchema = Yup.object({
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        MerchantID: Yup.string().required(PLEASE_PROVIDE_VALID + ' MerchantID'),
        Slat: Yup.string().required(PLEASE_PROVIDE_VALID + ' slat'),
        Token: Yup.string().required(PLEASE_PROVIDE_VALID + ' token'),
        ActionURL: Yup.string().required(PLEASE_PROVIDE_VALID + ' actionURL'),
        StatusCheckURL: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' statusCheckURL'
        ),
        MinimumAmount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'Minimum 1  amount')
            .required(PLEASE_PROVIDE_VALID + 'minimum amount'),
        MaximumAmount: Yup.number()
            .moreThan(9, PLEASE_PROVIDE_VALID + 'amount more than 9')
            .required(PLEASE_PROVIDE_VALID + 'maximum amount')
    });
    const selectData = async () => {
        const response = await SELECT();
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

    const SelectByID = async (BulkPaySettingID) => {
        setLoadingOpen(false);

        let jsonObj = {
            BulkPaySettingID: BulkPaySettingID
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await SELECT_BY_ID(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setIsError(true);
        if (!response.status) setSeverity('error');

        let d = JSON.parse(response.data);
        setFormObj({
            BulkPaySettingID: d.BulkPaySettingID,
            Name: d.Name,
            Mode: d.Mode,
            MerchantID: d.MerchantID,
            Slat: d.Slat,
            Token: d.Token,
            ActionURL: d.ActionURL,
            StatusCheckURL: d.StatusCheckURL,
            MinimumAmount: d.MinimumAmount,
            MaximumAmount: d.MaximumAmount,
            IsActive: d.IsActive,
            IsPendingForFailed: d.IsPendingForFailed
        });
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'BulkPaySettingID',
            srno: true
        });
        columns.push({
            label: 'Edit',
            name: 'BulkPaySettingID',
            edit: true,
            callback: (BulkPaySettingID) => {
                setIsUpdate(true);
                SelectByID(BulkPaySettingID);
            }
        });
        columns.push({
            label: 'Name',
            name: 'Name',
            actived: true
        });
        columns.push({
            label: 'MerchantID',
            name: 'MerchantID',
            displayAsPassword: true
        });
        columns.push({
            label: 'Slat',
            name: 'Slat',
            displayAsPassword: true
        });
        columns.push({
            label: 'Token',
            name: 'Token',
            displayAsPassword: true
        });

        columns.push({
            label: 'ActionURL',
            name: 'ActionURL',
            actived: true
        });
        columns.push({
            label: 'StatusCheck URL',
            name: 'StatusCheckURL',
            actived: true
        });
        columns.push({
            label: 'Min.Amount',
            name: 'MinimumAmount',
            actived: true
        });
        columns.push({
            label: 'Max.Amount',
            name: 'MaximumAmount',
            actived: true
        });
        columns.push({
            label: 'Mode',
            name: 'Mode',
            actived: true
        });

        columns.push({
            label: 'IsPendingForFailed',
            name: 'IsPendingForFailedStr'
        });

        columns.push({
            label: 'IsActive',
            name: 'IsActiveStr',
            actived: true
        });

        setMainColumns(columns);
    };
    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
    };
    useEffect(() => {
        setLoadingOpen(true);
        fetchcallbackURL();
        createColumns();
        selectData();
    }, []);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await UPDATE(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setIsError(true);

        setSubmitting(true);
        setSeverity('success');
        setResponseMessage(response.data);
        if (!response.status) {
            setSeverity('error');
            return;
        }

        resetForm(FormObj);
        setFormObj({
            Name: '',
            Mode: '',
            MerchantID: '',
            Slat: '',
            Token: '',
            ActionURL: '',
            StatusCheckURL: '',
            MinimumAmount: '',
            MaximumAmount: '',
            IsActive: false,
            IsPendingForFailed: false
        });

        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);
    };
    return (
        <CustomContainer
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
                            <h2 className={classes.PaperHead_h2}>
                                VendorPay Setting
                            </h2>

                            <Grid item xs={12}>
                                {IsUpdate ? (
                                    <Paper className={classes.PaperStyle}>
                                        <Grid container spacing={2}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="Name"
                                                    name="Name"
                                                    type="text"
                                                    placeholder="Name"
                                                    readOnly={true}
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
                                                    label="MerchantID"
                                                    name="MerchantID"
                                                    type="text"
                                                    placeholder="MerchantID"
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
                                                    label="Slat"
                                                    name="Slat"
                                                    type="text"
                                                    placeholder="Slat"
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
                                                    label="Token"
                                                    name="Token"
                                                    type="text"
                                                    placeholder="Token"
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
                                                    label="ActionURL"
                                                    name="ActionURL"
                                                    type="text"
                                                    placeholder="ActionURL"
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
                                                    label="StatusCheck URL"
                                                    name="StatusCheckURL"
                                                    type="text"
                                                    placeholder="Status Check URL"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialNumberField
                                                    label="Minimum Amount"
                                                    name="MinimumAmount"
                                                    type="text"
                                                    placeholder="Minimum Amount"
                                                    length={7}
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialNumberField
                                                    label="Maximum Amount"
                                                    name="MaximumAmount"
                                                    type="text"
                                                    placeholder="Maximum Amount"
                                                    length={7}
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
                                                    label="Mode"
                                                    name="Mode"
                                                    type="text"
                                                    placeholder="Mode"
                                                    length={5}
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={3}
                                                sm={3}
                                                md={3}
                                                lg={3}
                                            >
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

                                            <Grid
                                                item
                                                xs={3}
                                                sm={3}
                                                md={3}
                                                lg={3}
                                            >
                                                <FormikMaterialSwitch
                                                    label="IsPendingForFailed"
                                                    name="IsPendingForFailed"
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
                                ) : null}
                            </Grid>
                            {IsUpdate ? (
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
                            ) : null}
                        </Form>
                    );
                }}
            </Formik>
            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={100}
            />

            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>IServeU</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[ClientID]</b> = MerchantID,{' '}
                        <b>[ClientSecret]</b> = Token , <b>ActionURL = </b> :
                    </span>
                    <span className={classes.action_url}>
                        https://apigatewayprod.iserveu.tech ,
                    </span>
                    <span className={classes.normalTextColor}>
                        use <b>[callback URL as]</b> :
                    </span>
                    <span className={classes.callback_url}>
                        "{callbackURL}IServeUCallback/postResponse"
                    </span>
                </h3>
            </label>
            <br />

            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>YesBank</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[MerchantID]</b> = MerchantID,{' '}
                        <b>[MerchantKey]</b> = Token ,{' '}
                        <b>
                            {' '}
                            [XIBMClientSecret,XIBMClientID,PrivateKeyFilePath,AuthUserName,AuthPassword]
                        </b>{' '}
                        :
                    </span>
                    <span className={classes.action_url}>= ActionURL,</span>
                    <span className={classes.normalTextColor}>
                        use <b>[PrivateKeyPassword]</b> = StatusCheckURL,{' '}
                        <b>[Mode]</b> = Mode
                    </span>
                </h3>
            </label>

            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>JusPay</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[APIKey]</b> = Token, <b>[MerchantID]</b> =
                        MerchantID ,<b>[Mode]</b> = Mode
                    </span>
                </h3>
            </label>
            <br />
        </CustomContainer>
    );
};
