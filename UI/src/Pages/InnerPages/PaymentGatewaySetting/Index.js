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
} from 'Components/Api/PaymentGatewaySettingAPI';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import ReactTable from 'Components/ReactTable/ReactTable';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { API_URL } from 'Config';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const PaymentGateWaySetting = () => {
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
    const IsPGSettlementHold =
        localStorage.getItem('IsPGSettlementHold') === 'true';

    const [FormObj, setFormObj] = useState({
        PaymentGatewaySettingID: 0,
        Name: '',
        MerchantID: '',
        Slat: '',
        Token: '',
        SuccessURL: '',
        FailURL: '',
        ActionURL: '',
        StatusCheckURL: '',
        MinimumAmount: 0,
        MaximumAmount: 0,
        SettlementHoldInMinutes: 0,
        IsActive: false,
        IsUPI: false,
        IsStaticQR: false
    });
    const FormValidationSchema = Yup.object({
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        MerchantID: Yup.string().required(PLEASE_PROVIDE_VALID + ' MerchantID'),
        Slat: Yup.string().required(PLEASE_PROVIDE_VALID + ' slat'),
        Token: Yup.string().required(PLEASE_PROVIDE_VALID + ' token'),
        SuccessURL: Yup.string().required(PLEASE_PROVIDE_VALID + ' successURL'),
        FailURL: Yup.string().required(PLEASE_PROVIDE_VALID + ' failURL'),
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

    const SelectByID = async (PaymentGatewaySettingID) => {
        setLoadingOpen(false);

        let jsonObj = {
            PaymentGatewaySettingID: PaymentGatewaySettingID
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await ISONOFF(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setIsError(true);
        if (!response.status) setSeverity('error');

        let d = JSON.parse(response.data);
        setFormObj({
            PaymentGatewaySettingID: d.PaymentGatewaySettingID,
            Name: d.Name,
            MerchantID: d.MerchantID,
            Slat: d.Slat,
            Token: d.Token,
            SuccessURL: d.SuccessURL,
            FailURL: d.FailURL,
            ActionURL: d.ActionURL,
            StatusCheckURL: d.StatusCheckURL,
            MinimumAmount: d.MinimumAmount,
            MaximumAmount: d.MaximumAmount,
            IsActive: d.IsActive,
            IsUPI: d.IsUPI,
            IsStaticQR: d.IsStaticQR,
            SettlementHoldInMinutes: d.SettlementHoldInMinutes
        });
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'PaymentGatewaySettingID',
            srno: true
        });
        columns.push({
            label: 'Edit',
            name: 'PaymentGatewaySettingID',
            edit: true,
            callback: (PaymentGatewaySettingID) => {
                setIsUpdate(true);
                SelectByID(PaymentGatewaySettingID);
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
            label: 'SuccessURL',
            name: 'SuccessURL',
            actived: true
        });
        columns.push({
            label: 'FailURL',
            name: 'FailURL',
            actived: true
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
        if (IsPGSettlementHold) {
            columns.push({
                label: 'SettlementHold InMinutes ',
                name: 'SettlementHoldInMinutes',
                actived: true
            });
        }
        columns.push({
            label: 'IsUPI',
            name: 'IsUPIStr',
            actived: true
        });
        columns.push({
            label: 'IsStaticQR',
            name: 'IsStaticQRStr',
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
    }, [IsPGSettlementHold]);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);
        //setIsError(false);
        setResponseMessage('');
        setLoadingOpen(true);
        var response = await UPDATE(requestObj);

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        resetForm(FormObj);
        setFormObj({
            Name: '',
            MerchantID: '',
            Slat: '',
            Token: '',
            SuccessURL: '',
            FailURL: '',
            ActionURL: '',
            StatusCheckURL: '',
            MinimumAmount: '',
            MaximumAmount: '',
            SettlementHoldInMinutes: '',
            IsActive: false
        });

        selectData();
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
                                Payment Gateway Setting
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
                                                    label="SuccessURL"
                                                    name="SuccessURL"
                                                    type="text"
                                                    placeholder="SuccessURL"
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
                                                    label="FailURL"
                                                    name="FailURL"
                                                    type="text"
                                                    placeholder="FailURL"
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
                                            {IsPGSettlementHold && (
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <MaterialNumberField
                                                        label="SettlementHold InMinutes"
                                                        name="SettlementHoldInMinutes"
                                                        type="text"
                                                        placeholder="SettlementHold InMinutes"
                                                        length={5}
                                                    />
                                                </Grid>
                                            )}

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
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
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <FormikMaterialSwitch
                                                    label="IsUPI"
                                                    name="IsUPI"
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
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <FormikMaterialSwitch
                                                    label="IsStaticQR"
                                                    name="IsStaticQR"
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

            <Grid container spacing={2}>
                <Grid item xs={6} sm={6} md={12} lg={12} xl={12}>
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong>For ICICI_QR </strong> : {''}
                            <span className={classes.normalTextColor}>
                                use <b>[callback url is]</b>
                            </span>
                            <span className={classes.callback_url}>
                                {callbackURL}
                                callback/post_icici_upi_collection_response
                            </span>
                        </h3>
                        <span className={classes.normalTextColor}>
                            <b>[Slat]</b> client private key password,
                            <b>[Token]</b> : admin vpa,
                            <b>[Success URL] : </b> ICICI public key,
                            <b>[Fail URL] : </b> client private key,
                            <b>[Action URL] : </b> if Static QR prefix given,{' '}
                            <b>[Status check URL]</b> : MCCode (5411 or 6010)
                        </span>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong>For FreeUPI</strong> :{''}
                            <span className={classes.normalTextColor}>
                                put your <b>[merchant VPA ]</b> in Token field,{' '}
                                <b>[ other param not needed]</b>{' '}
                            </span>
                        </h3>
                    </label>
                    <br />

                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For OpenBank</strong> :{''}
                            <span className={classes.normalTextColor}>
                                put your <b>[accesskey]</b> in Slat field and,{' '}
                                <b>[secretkey]</b> in Token field{' '}
                            </span>
                        </h3>
                    </label>
                    <br />
                    {/* <label>
                        <h3 className={classes.Provider_Format}>
                        <strong> For OpenUPI</strong> :{''}
                            <span className={classes.normalTextColor}>
                                put your <b>[mobile number]</b> in merchant ID,{' '}
                                <b>[token]</b>=token , <b>[slat]</b>= slat
                            </span>                          
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                        <strong> For OpenUPI</strong> :{''}
                            <span className={classes.normalTextColor}>
                                put your <b>[Action url]</b> 
                            </span>
                             <span className={classes.action_url}>
                         https://openUPI.com/callback/
                     </span>                           
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                        <strong> For OpenUPI</strong> :{''}
                            <span className={classes.normalTextColor}>
                                put your <b>[Status url]</b> use
                            </span>
                             <span className={classes.callback_url}>
                         https://openUPI.com/callback/
                        </span>
                        </h3>
                    </label>
                    <br /> */}
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> In Paytm, </strong> :{''}
                            <span className={classes.normalTextColor}>
                                use <b>[Token ]</b> as a
                                [CHANNEL_ID],[INDUSTRY_TYPE_ID],[CUST_ID],[WEBSITE]
                            </span>
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For Paytm</strong> :{''}
                            <span className={classes.normalTextColor}>
                                put your <b>[SuccessURL and FailURL]</b> use
                            </span>
                            <span className={classes.callback_url}>
                                {callbackURL} callback/postPaytm
                            </span>
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For Razorpay</strong> :{''}
                            <span className={classes.normalTextColor}>
                                For <b>[Razorpay SuccessURL and FailURL]</b> use
                            </span>
                            <span className={classes.callback_url}>
                                {callbackURL} callback/postRazorpay
                            </span>
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For Razorpay</strong> :{''}
                            <span className={classes.normalTextColor}>
                                <b>[web hook need to set in ]</b>{' '}
                                https://dashboard.razorpay.com/ -- setting --
                                webhook ,<b>[set 2 Events]</b> :
                                payment.authorized and payment.failed, and{' '}
                                <b>[aet url as]</b> use :
                            </span>
                            <span className={classes.callback_url}>
                                {callbackURL}
                                Callback/webHookRazorpay
                            </span>
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For IMoneyPay, </strong> :{''}
                            <span className={classes.normalTextColor}>
                                use <b>[MerchantID ]</b> =[Pay Id] and
                                [Slat]=[Salt]
                            </span>
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For PayGoal, </strong> :{''}
                            <span className={classes.normalTextColor}>
                                use <b>[MerchantID ]</b> =[Pay Id] and
                                <b>[Slat]</b>=[Salt] , <b>ActionURL =</b>
                            </span>
                            <span className={classes.action_url}>
                                [https://www.paygoal.in/order/v1/payment]
                            </span>
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For Easebuzz, </strong> :{''}
                            <span className={classes.normalTextColor}>
                                use <b>[MerchantID ]</b> =[Key] ,<b>[Slat]</b>
                                =[Salt] and
                                <b>SuccessURL =</b>
                            </span>
                            <span className={classes.callback_url}>
                                {callbackURL}
                                EasebuzzCallback/postSuccess
                            </span>
                            <span className={classes.normalTextColor}>
                                <b>FailURL =</b>
                            </span>
                            <span className={classes.callback_url}>
                                {callbackURL}
                                EasebuzzCallback/postSuccess
                            </span>
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For CashFree, </strong> :{''}
                            <span className={classes.normalTextColor}>
                                use <b>[MerchantID ]</b> = [AppID] ,
                                <b>[SecretKey]</b>=[Salt] ,and use{' '}
                                <b>SuccessURL =</b>
                            </span>
                            <span className={classes.callback_url}>
                                {callbackURL}
                                CashfreeCallback/postSuccess
                            </span>
                            <span className={classes.normalTextColor}>
                                <b>FailURL=</b>
                            </span>
                            <span className={classes.callback_url}>
                                {callbackURL}
                                CashfreeCallback/post_CashFree_WebhookResponse
                            </span>
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For UPIGateway, </strong> :{''}
                            <span className={classes.normalTextColor}>
                                use <b>[MerchantID ]</b> =Key
                            </span>
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For Paysrpint: UPI Collection : </strong> :
                            {''}
                            <span className={classes.normalTextColor}>
                                use <b>[MerchantID ]</b> =[PartnerID] ,
                                <b>[Token]</b> =[Authorisedkey],
                                <b>[ActionURL]</b> =[JWTKEY],<b>[Slat]</b>{' '}
                                =[AESKey, AESIV],
                                <b>[Mode]</b>=[SuccessURL],{' '}
                                <b>[statusCheckURL]</b> = JSON like
                                {`{'PartnerCurrentAccount':'XXX','IfscCode':'XXX','PartnerMobile':'XXX','PartnerAddress':'XXX','PartnerCity':'XXX','PartnerState':'XXX', 'PartnerPincode': 'XXX' }`}
                            </span>
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For YesBank:</strong> :{''}
                            <span className={classes.normalTextColor}>
                                use <b>[MerchantID ]</b> =[MerchantID] ,
                                <b>[Slat]</b> =[MerchantKey],<b>[Token]</b>
                                =[ClientID],
                                <b>[SuccessURL]</b>=[ClientSecret],
                                <b>[FailURL]</b> = [PrivateKeyPassword],
                                <b>[ActionURL]</b>
                                =[PrivateKeyFilePath,Mode(PROD/UAT)],
                                <b>[statusCheckURL]</b>=[MCCCode]
                            </span>
                        </h3>
                    </label>

                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For PhonePay:</strong> :{''}
                            <span className={classes.normalTextColor}>
                                use <b>[MerchantID ]</b> =[MerchantID] ,
                                <b>[Slat]</b> =[Salt key],<b>[Token]</b>
                                =[Salt index],
                                <b>[ActionURL]=</b>
                            </span>
                            <span className={classes.callback_url}>
                                [{callbackURL}
                                PhonePayCallback/PostPhonePay]
                            </span>
                        </h3>
                    </label>

                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For WorldLine:</strong> :{''}
                            <span className={classes.normalTextColor}>
                                use <b>[MerchantID ]</b> =[MerchantID] ,
                                <b>[Slat]</b> =[Salt],<b>[Token]</b>
                                =[ItemId],
                                <b>[ActionURL]=</b>
                            </span>
                            <span className={classes.callback_url}>
                                [{callbackURL}
                                WorldLineCallback/PostWorldLine],
                            </span>
                            <span className={classes.normalTextColor}>
                                <b>[SuccessURL ]</b> =[instrumentId] ,
                                <b>[FailURL]</b> =[bankCode]
                            </span>
                        </h3>
                    </label>

                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For UPIALL:</strong> :{''}
                            <span className={classes.normalTextColor}>
                                use <b>[MerchantID ]</b> =[MerchantID] ,
                                <b>[Token]</b>
                                =[SecretKey] and
                                <b> [callbackURL] as =</b>
                            </span>
                            <span className={classes.callback_url}>
                                "{callbackURL}
                                Callback/postupiall"
                            </span>
                        </h3>
                    </label>
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong> For PayU:</strong> :{''}
                            <span className={classes.normalTextColor}>
                                use
                                <b>[Slat]</b> =Merchant Salt(Version 1),
                                <b>[Token]</b>= Merchant Key,
                                <b>[SuccessURL]</b>
                                =[{callbackURL}PayUCallback/postPG],
                                <b>[FailURL]</b> = [{callbackURL}
                                PayUCallback/postPG],
                                <b>[ActionURL]</b>
                                =[https://secure.payu.in/_payment],
                                <b>[statusCheckURL]</b>
                                =[https://info.payu.in/merchant/postservice?form=2]
                            </span>
                        </h3>
                    </label>
                    <br />
                </Grid>
            </Grid>
        </CustomContainer>
    );
};
