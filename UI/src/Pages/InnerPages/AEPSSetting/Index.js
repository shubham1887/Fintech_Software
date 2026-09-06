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
    IsOnOff as ISONOFF
} from 'Components/Api/AEPSSettingAPI';
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

export const AEPSSetting = () => {
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
        AEPSSettingID: 0,
        Name: '',
        MerchantID: '',
        Sequence: 0,
        DisplayName: '',
        Slat: '',
        Token: '',
        Mode: '',
        PublicKey: '',
        MinimumAmount: 0,
        MaximumAmount: 0,
        IsActive: false
    });
    const FormValidationSchema = Yup.object({
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        MerchantID: Yup.string().required(PLEASE_PROVIDE_VALID + ' MerchantID'),
        Slat: Yup.string().required(PLEASE_PROVIDE_VALID + ' slat'),
        Token: Yup.string().required(PLEASE_PROVIDE_VALID + ' token'),
        MinimumAmount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'Minimum 1  amount')
            .required(PLEASE_PROVIDE_VALID + 'minimum amount'),
        MaximumAmount: Yup.number()
            .moreThan(9, PLEASE_PROVIDE_VALID + 'amount more than 9')
            .required(PLEASE_PROVIDE_VALID + 'maximum amount'),
        Mode: Yup.string().required(PLEASE_PROVIDE_VALID + 'Mode')
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

    const SelectByID = async (AEPSSettingID) => {
        setLoadingOpen(false);

        let jsonObj = {
            AEPSSettingID: AEPSSettingID
        };

        var request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);
        setResponseMessage('');
        const response = await ISONOFF(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setIsError(true);
        if (!response.status) setSeverity('error');

        let d = JSON.parse(response.data);
        setFormObj({
            AEPSSettingID: d.AEPSSettingID,
            Name: d.Name,
            DisplayName: d.DisplayName,
            Sequence: d.Sequence,
            MerchantID: d.MerchantID,
            Slat: d.Slat,
            Token: d.Token,
            PublicKey: d.PublicKey,
            MinimumAmount: d.MinimumAmount,
            MaximumAmount: d.MaximumAmount,
            Mode: d.Mode,
            IsActive: d.IsActive
        });
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'AEPSSettingID',
            srno: true
        });
        columns.push({
            label: 'Edit',
            name: 'AEPSSettingID',
            edit: true,
            callback: (AEPSSettingID) => {
                setIsUpdate(true);
                SelectByID(AEPSSettingID);
            }
        });

        columns.push({
            label: 'Name',
            name: 'Name',
            actived: true
        });
        columns.push({
            label: 'Sequence',
            name: 'Sequence',
            actived: true
        });

        columns.push({
            label: 'Display Name',
            name: 'DisplayName',
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
            label: 'Public Key',
            name: 'PublicKey',
            displayAsPassword: true
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
        createColumns();
        selectData();
        fetchcallbackURL();
    }, []);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setSubmitting(false);
        setLoadingOpen(true);
        setIsError(false);
        setResponseMessage('');

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
            MerchantID: '',
            Sequence: 0,
            DisplayName: '',
            Mode: '',
            Slat: '',
            Token: '',
            PublicKey: '',
            MinimumAmount: '',
            MaximumAmount: '',
            IsActive: false
        });

        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);
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
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                AEPS Setting
                            </h2>

                            <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
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
                                                <MaterialNumberField
                                                    label="Sequence"
                                                    name="Sequence"
                                                    type="text"
                                                    placeholder="Sequence"
                                                    length={1}
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
                                                    label="Display Name"
                                                    name="DisplayName"
                                                    type="text"
                                                    placeholder="Display Name"
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
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="Public Key"
                                                    name="PublicKey"
                                                    type="text"
                                                    placeholder="PublicKey"
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
            <Grid container>
                <Box className={classes.flexReverse}>
                    <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                        {/* <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                         <label>
                            <h3 className={classes.Provider_Format}>
                                <strong>In Alle</strong> :{' '}
                                <span className={classes.normalTextColor}>
                                    use <b>[MobileNumber]</b> = MerchantID
                                </span>
                            </h3>
                        </Grid> */}
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In MASTERMONEY DMT</strong> :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[MerchantID]</b> = MerchantID,{' '}
                                        <b>[Slat]</b> = Slat,
                                        <b> PublicKey as 0 </b> and <b>Token</b>{' '}
                                        =Password
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In InstantPay Prime Web SDK</strong>{' '}
                                    :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[MerchantID]</b> = APP_ID,{' '}
                                        <b>[Slat]</b> = ALLOWED_SERVICES,
                                        <b>
                                            {' '}
                                            PublicKey as DomainName{' '}
                                        </b> and <b>Token</b> =Token
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In InstantPay Prime Web SDK</strong>{' '}
                                    :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[MerchantID]</b> = APP_ID,{' '}
                                        <b>[Slat]</b> = ALLOWED_SERVICES like
                                        WAP,SAP,BAP ,
                                        <b> PublicKey as Domain URL </b>
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>
                                        In InstantPay Prime Web SDK Transaction
                                        Confirmation Webhook
                                    </strong>{' '}
                                    :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[callbackURL] : </b>
                                    </span>
                                    <span className={classes.callback_url}>
                                        {callbackURL}{' '}
                                        InstantPayValidationConfirmation
                                        /ConfirmTransaction"
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>
                                        In InstantPay Prime Web SDK Transaction
                                        Webhook
                                    </strong>{' '}
                                    :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[callbackURL] : </b>
                                    </span>
                                    <span className={classes.callback_url}>
                                        {callbackURL}{' '}
                                        InstantPayTransactionConfirmation
                                        /ConfirmTransaction"
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>
                                        In InstantPay Prime Web SDK Balance
                                        Webhook
                                    </strong>{' '}
                                    :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[callbackURL] : </b>
                                    </span>
                                    <span className={classes.callback_url}>
                                        {callbackURL}{' '}
                                        InstantPayBalance/GetBalance
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In Mahagram AEPS</strong> :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[callback check status as] : </b>
                                    </span>
                                    <span className={classes.callback_url}>
                                        {callbackURL}{' '}
                                        MahagramAEPSStatus/ConfirmTransaction
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In Mahagram AEPS</strong> :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[callback check status as] : </b>
                                    </span>
                                    <span className={classes.callback_url}>
                                        {callbackURL}{' '}
                                        MahagramAEPSStatus/UpdateTransaction
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In Paysprint AEPS</strong> :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[MerchantID]</b> = as PartnerID,{' '}
                                        <b>[PublicKey]</b> = as JWTKEY ,
                                        <b>[Token]</b> = as Authorize Key ,
                                        <b>slat contains(comma separated)</b> =
                                        as AESKey,
                                        <b> AESIV </b>and <b> PIPE </b>
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In Paysprint AEPS</strong> :{' '}
                                    <span className={classes.normalTextColor}>
                                        use{' '}
                                        <b>[callback user onboarding as] : </b>
                                    </span>
                                    <span className={classes.callback_url}>
                                        {callbackURL}{' '}
                                        PaysprintCallback/UserOnbording
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        {/* <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                         <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In Eko AEPS</strong>{' '}
                                    :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[MerchantID]</b> = as InitiatorID,{' '}
                                        <b>[PublicKey]</b> = PublicKey  ,
                                        <b>[Token] </b> = as Dedeveloperkey,
                                        <b>[Slat]</b> = as Authenticator key 
                                    </span>
                                </h3>
                            </label>
                        </Grid> */}
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In Fingpay AEPS</strong> :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[SuperMerchantID]</b> = as
                                        MerchantID, <b>[PublicKey]</b> =
                                        "[companyname]_fingpay_public_production_key.txt"
                                        ,<b>[UserName] </b> = as Slat,
                                        <b>[Password]</b> = as Token
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        {/* <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                        <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In ApiClub AEPS</strong> :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>Referer(enter your
                                Application Package ID or Website URL)</b> = as
                                        MerchantID, <b>[APIKey]</b> =
                                        = as Token
                                    </span>
                                </h3>
                            </label>
                        </Grid> */}
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In EzyPay AEPS</strong> :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[MPIN]</b> = as MerchantID,{' '}
                                        <b>[Auth Code]</b> = as Token
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In CredoPay AEPS</strong> :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[MerchantID]</b> = AEPS Key,{' '}
                                        <b>[Slat]</b> = as AEPS Secret Key
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                        <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                            <label>
                                <h3 className={classes.Provider_Format}>
                                    <strong>In InstantPay AEPS</strong> :{' '}
                                    <span className={classes.normalTextColor}>
                                        use <b>[MerchantID]</b> = as AuthCode,{' '}
                                        <b>[PublicKey]</b> = as EndpointIP ,
                                        <b>[Token]</b> = as EncryptionKey ,
                                        <b>slat contains(comma separated)</b> =
                                        as ClientSecret and UniqueClientID
                                    </span>
                                </h3>
                            </label>
                        </Grid>
                    </Grid>
                </Box>
            </Grid>
        </CustomContainer>
    );
};
