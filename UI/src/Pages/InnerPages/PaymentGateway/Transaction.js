import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { APP_BAR_COLOR, CLIENT_NAME, LOGO_URL, CLIENT_ADDRESS } from 'Config';
import * as Yup from 'yup';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { useHistory } from 'react-router';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    Init as PYMENTGATEWAY_INIT,
    ActivePaymentGatewayV2 as PAYMENTGATEWAY_ACTIVEV2
} from 'Components/Api/PaymentGatewayAPI';
import { SelectStaticQR as STATICQR } from 'Components/Api/PaymentGatewayAPI';
import { SelectForPaymentGateway as WALLET_SELECT_GATEWAY } from 'Components/Api/WalletApi';

import { SelectUPICommissionByUserID as PG_COMMI_INFO } from 'Components/Api/SlabDetailApi';
import { GetDetails as SELECT_MERCHANT_PS } from 'Components/Api/UPICollection';
import { GetDetailsRazorPay as SELECT_MERCHANT_RP } from 'Components/Api/UPICollection';

import ReactTable from 'Components/ReactTable/ReactTable';
import Divider from '@material-ui/core/Divider';
import {
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import ResetIcon from 'Components/SvgButtons/ResetIcon';
import { SelectLast10PaymentGateway as TRANSACTION_LAST10 } from 'Components/Api/PaymentGatewayAPI';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import PayIcon from 'Components/SvgButtons/PayIcon';
import { TransactionConfirmation } from 'Components/Alert/TransactionConfirmation';
import { SelectSlabByUserIDWithService as SELECT_COMMITION_STRUCTURE } from 'Components/Api/SlabDetailApi';
import { PAYMENT_GATEWAY_CODE } from 'Components/Helper/Messages';
import { PRIMARY_COLOR, SECONDARY_COLOR } from 'Config';

const CustomContainer = HOCLoader(Container);

export const GatewayTransaction = () => {
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isOpen, setIsOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const history = useHistory();
    const [wallets, setWallets] = useState([]);
    const [IsTypeDropdown, setTypeDropdown] = useState(false);
    const [TakeCount, setTakeCount] = useState(10);
    const [currentPage, setCurrentPage] = useState(0);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [imageQRCode, setimageQRCode] = useState('');
    const [isStaticQR, setisStaticQR] = useState(false);
    const [IsCommissionShow, setIsCommissionShow] = useState(false);
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [Commissiondata, setCommissiondata] = useState([]);
    const [CommissionmainColumns, setCommissionmainColumns] = useState([]);
    const [currentUserID] = useState(localStorage.getItem('UserID'));

    const SelectStaticQR = async () => {
        setLoadingOpen(true);
        const response = await STATICQR();

        if (response.data === 'Invalid data') {
            setLoadingOpen(false);
            history.push('/VirtualAccountCreate');
            return;
        }
        if (!response.isAuthorized) {
            setLoadingOpen(false);
            LOGOUT();
            return;
        }

        if (!response.status) {
            setLoadingOpen(false);
            return;
        }

        let data = JSON.parse(response.data);
        if (
            data.Name === 'ICICI_QR' ||
            data.Name === 'Paysprint' ||
            data.Name === 'RazorpayQR' ||
            data.Name === 'EasebuzzStaticQR'
        ) {
            setimageQRCode(data.ICICIQR);
            if (data.ICICIQR.length >= 10) {
                setisStaticQR(true);
            }
        }
        setLoadingOpen(false);
    };



    const selectWallets = async () => {
        var request = {
            ID: 0
        };
        var json = JSON.stringify(request);
        const response = await WALLET_SELECT_GATEWAY(json);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setWallets(data);
    };
    const SelectLast10 = async () => {
        createColumns();

        setTransactions([]);

        setLoadingOpen(true);
        progress();
        var response = await TRANSACTION_LAST10();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setTransactions(data);
    };

    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let inputObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            SearchText: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            UserID: currentUserID,
            ServiceID: PAYMENT_GATEWAY_CODE,
            IsExcelDownload: false
        };
        var requestObj = JSON.stringify(inputObj);

        setIsError(false);

        var response = await SELECT_COMMITION_STRUCTURE(requestObj);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        var data = JSON.parse(response.data);
        setCommissiondata(data.list);

        setTotalCount(data.Count);
    };

    const SelectCommission = async () => {
        createColumnsComm();
        setCommissiondata([]);
        callSelectData(TakeCount, SkipCount, '', '', '');
    };
    const [completed, setCompleted] = useState(0);
    const progress = () => {
        setCompleted((oldCompleted) => {
            if (oldCompleted === 100) return 0;

            const diff = Math.random() * 10;
            return Math.min(oldCompleted + diff, 100);
        });
    };

    const selectData = async () => {
        setLoadingOpen(true);
        const response = await SELECT_MERCHANT_PS();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);

        return data.Message;
    };

    const SelectDataRazorQR = async () => {
        setLoadingOpen(true);
        const response = await SELECT_MERCHANT_RP();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);

        return data.Message;
    };

    const selectActivePaymentGatewayV2 = async () => {
        const response = await PAYMENTGATEWAY_ACTIVEV2();

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        let data = JSON.parse(response.data);

        if (data.length <= 0) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage('No gateway active');
            return;
        }

        if (data[0].Name === 'ICICI_QR') setIsCommissionShow(true);
        if (data[0].Name === 'EasebuzzStaticQR') setisStaticQR(true);

        if (data.length <= 1) {
            setTypeDropdown(false);

            if (data[0].Name === 'Paysprint') {
                const message = await selectData();
                if (message === 'Merchant not exists') {
                    history.push('/PSCreateVPA');
                    return;
                }
                return;
            } else {
                history.push('/PaymentGatewayTransaction');
                return;
            }
        }
        setTypeDropdown(true);
        history.push('/PaymentGatewayTransaction');
        return;
    };

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        createColumnsComm();
        selectWallets();
        SelectStaticQR();
        SelectLast10();
        SelectCommission();
        selectActivePaymentGatewayV2();
    }, []);

    const formObject = {
        ServiceID: PAYMENT_GATEWAY_CODE,
        WalletID: 0,
        Amount: ''
    };

    const FormValidationSchema = Yup.object({
        WalletID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'wallet')
            .required(PLEASE_SELECT_VALID + 'wallet'),

        Amount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'amount')
            .required(PLEASE_PROVIDE_VALID + 'amount')
    });

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'PaymentGatewayID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'DisplayDOC'
        });
        columns.push({
            label: 'Name',
            name: 'UserName'
        });
        columns.push({
            label: 'OrderID',
            name: 'OrderID'
        });
        columns.push({
            label: 'Wallet',
            name: 'WalletName'
        });
        columns.push({
            label: 'Op Bal',
            name: 'OpeningBalance',
            textwrapwidth: 60,
            numeric: true
        });

        columns.push({
            label: 'Amount',
            name: 'Amount',
            textwrapwidth: 60,
            numeric: true
        });

        columns.push({
            label: 'Cl Bal',
            name: 'ClosingBalance',
            textwrapwidth: 60,
            numeric: true
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        columns.push({
            label: 'Ref No',
            name: 'TransactionID'
        });
        columns.push({
            label: 'PayMode',
            name: 'PayMode'
        });

        setMainColumns(columns);
    };

    const createColumnsComm = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'SlabDetailID',
            srno: true
        });

        columns.push({
            label: 'Type',
            name: 'Type'
        });
        columns.push({
            label: 'Amount',
            name: 'AmountString'
        });

        columns.push({
            label: 'Retailer',
            name: 'RetailerCommType'
        });

        setCommissionmainColumns(columns);
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(false);
        if (IsCommissionShow) {
            var request = {
                Amount: values.Amount
            };
            setLoadingOpen(true);
            var json = JSON.stringify(request);
            const response = await PG_COMMI_INFO(json);
            setLoadingOpen(false);
            if (!response.isAuthorized) LOGOUT();
            if (!response.status) {
                setSeverity('error');
                setResponseMessage(response.data);
                setIsError(true);
                return;
            }
            const { CommissionCategory, Commission } = JSON.parse(
                response.data
            );
            values.IsCommissionShow = IsCommissionShow;
            values.CommissionCategory = CommissionCategory;
            values.Commission = Commission;
        }
        let op = wallets.filter((o) => o.WalletID === values.WalletID);
        values.Name = op[0].Name;
        setIsAlertOpen(true);
        setConfirmAlertRequest(values);
    };
    const Transaction = async (values) => {
        setLoadingOpen(true);
        setIsError(false);
        setIsCommissionShow(false);

        var requestObj = JSON.stringify(values);

        setIsError(false);
        setLoadingOpen(true);
        setResponseMessage('');

        var response = await PYMENTGATEWAY_INIT(requestObj);

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }

        setIsError(true);
        setSeverity('success');

        let data = JSON.parse(response.data);
        if (!response.status) {
            if (data.Message === 'Invalid OutletID') {
                setSeverity('error');
                setResponseMessage(data.Message);

                history.push('/IPRegistrationOTP');
                return;
            }
            setResponseMessage(data.Message);
            setSeverity('error');
            return;
        }

        const gatewayName = data.PaymentGatewayName.toLowerCase();


        if (gatewayName === 'IPG'.toLowerCase()) {
            let param = {
                ResponseSuccessURL: data.SuccessURL,
                ResponseFailURL: data.FailURL,
                FormActionURL: data.ActionURL,
                Hash: data.Hash,
                TxnDatetime: data.OrderID,
                StoreName: data.MerchantID,
                Amount: data.Amount,
                Currency: data.Currency
            };

            history.push('/IPGTransaction', param);
            return;
        } else if (gatewayName === 'Paytm'.toLowerCase()) {
            let param = {
                CallBackURL: data.SuccessURL,
                FormActionURL: data.ActionURL,
                CheckSum: data.Hash,
                OrderID: data.OrderID,
                MerchantID: data.MerchantID,
                MerchantKey: data.Key,
                Amount: data.Amount,
                Name: data.Name,
                Email: data.Email,
                ContactNumber: data.ContactNumber,
                ChannelID: data.ChannelID,
                IndustryTypeID: data.IndustryTypeID,
                Website: data.Website,
                CustID: data.CustID
            };

            history.push('/PTransaction', param);
            return;
        } else if (gatewayName === 'Easebuzz'.toLowerCase()) {
            let param = {
                SuccessURL: data.SuccessURL,
                FormActionURL: data.ActionURL,
                Hash: data.Hash,
                OrderID: data.OrderID,
                MerchantID: data.MerchantID,
                MerchantKey: data.Key,
                Amount: data.Amount,
                Name: data.Name,
                Email: data.Email,
                ContactNumber: data.ContactNumber,
                ChannelID: data.ChannelID
            };

            history.push('/EBTransaction', param);
            return;
        } else if (gatewayName === 'CashFree'.toLowerCase()) {
            let param = {
                payment_session_id: data.payment_session_id,
                SuccessURL: data.SuccessURL,
                FailURL: data.FailURL,
                FormActionURL: data.ActionURL,
                Hash: data.Hash,
                OrderID: data.OrderID,
                MerchantID: data.MerchantID,
                Amount: data.Amount,
                Name: data.Name,
                Email: data.Email,
                ContactNumber: data.ContactNumber
            };

            history.push('/CFTransaction', param);
            return;
        } else if (gatewayName === 'IMoneyPay'.toLowerCase()) {
            let param = {
                CallBackURL: data.SuccessURL,
                FormActionURL: data.ActionURL,
                CheckSum: data.Hash,
                OrderID: data.OrderID,
                MerchantID: data.MerchantID,
                MerchantKey: data.Key,
                Amount: data.Amount,
                Name: data.Name,
                Email: data.Email,
                ContactNumber: data.ContactNumber,
                Currency: data.Currency
            };

            history.push('/IMoneyPayPG', param);
            return;
        } else if (gatewayName === 'PayGoal'.toLowerCase()) {
            let param = {
                CheckSum: data.Hash,
                MerchantID: data.MerchantID,
                OrderID: data.OrderID,
                CallBackURL: data.SuccessURL,
                FormActionURL: data.ActionURL,
                Amount: data.Amount,
                Name: data.Name,
                Email: data.Email,
                ContactNumber: data.ContactNumber,
                Currency: data.Currency,

                MerchantKey: data.Key
            };

            history.push('/PayGoalPG', param);
            return;
        } else if (gatewayName === 'InstantPayUPIGateway'.toLowerCase()) {
            let param = {
                QRImage: data.Hash
            };
            history.push('/QRCodeGenerate', param);
            return;
        } else if (
            gatewayName === 'ICICI_QR'.toLowerCase() ||
            gatewayName === 'YesBank'.toLowerCase()
        ) {
            let param = {
                QRImage: data.Hash,
                OrderID: data.OrderID
            };
            history.push('/QRCodeGenerate', param);
            return;
        } else if (gatewayName === 'FreeUPI'.toLowerCase()) {
            let param = {
                QRImage: data.Hash,
                OrderID: data.OrderID
            };
            history.push('/QRCodeGenerate', param);
            return;
        } else if (gatewayName === 'OpenUPI'.toLowerCase()) {
            let param = {
                QRImage: data.Hash,
                OrderID: data.OrderID
            };
            history.push('/QRCodeGenerate', param);
            return;
        } else if (gatewayName === 'UPIGateway'.toLowerCase()) {
            UPIGatewayPaymentHandler(data.RedirectURL);
            return;
        } else if (gatewayName === 'UPIALL'.toLowerCase()) {
            UPIALLPaymentHandler(data.RedirectURL);
            return;
        } else if (gatewayName === 'OpenBank'.toLowerCase()) {
            var param_OB = {
                Hash: data.Hash,
                Amount: data.Amount,
                OrderID: data.OrderID,
                TxnToken: data.TxnToken,
                MerchantID: data.MerchantID,
                LogoURL: data.LogoURL
            };
            history.push('/OBPGTransaction', param_OB);
            return;
        } else if (gatewayName === 'RazorpayQR'.toLowerCase()) {
            var param_QR = {
                SuccessURL: data.SuccessURL,
                OrderID: data.OrderID
            };
            history.push('/RazorpayQRTransaction', param_QR);
            return;
        } else if (gatewayName === 'Razorpay'.toLowerCase()) {
            let paramRazor = {
                ResponseSuccessURL: data.SuccessURL,
                FormActionURL: data.ActionURL,
                OrderID: data.OrderID,
                Key: data.MerchantID,
                Amount: data.Amount,
                Currency: data.Currency,
                Name: data.Name,
                Email: data.Email,
                ContactNumber: data.ContactNumber,
                PaymentMode: data.Hash
            };
            razorPaymentHandler(paramRazor);
            return;
        } else if (gatewayName === 'WorldLine'.toLowerCase()) {
            let param = {
                Token: data.Hash,
                RedirectURL: data.RedirectURL,
                CallBackURL: data.CallBackURL,
                MerchantID: data.MerchantID,
                Currency: data.Currency,
                ConsumerID: data.CustID,
                ConsumerMobileNo: data.ContactNumber,
                ConsumerEmailId: data.Email,
                Amount: data.Amount,
                OrderID: data.OrderID,
                Commission: 0,
                Name: data.Name,
                Key: data.Key,
                InstrumentId: data.ChannelID,
                BankCode: data.SuccessURL
            };

            worldLinePaymentHandler(param);
            return;
        } else if (gatewayName === 'Paysprint'.toLowerCase()) {
            let param = {
                QRImage: data.Hash,
                OrderID: data.OrderID
            };
            history.push('/QRCodeGenerate', param);
            return;
        } else if (gatewayName === 'PhonePay'.toLowerCase()) {
            PhonePayPGHandler(data.RedirectURL);
            return;
        } else if (gatewayName === 'PayU'.toLowerCase()) {
            let param = {
                Hash: data.Hash,
                OrderID: data.OrderID,
                Key: data.Key,
                Amount: data.Amount,
                SuccessURL: data.SuccessURL,
                FailURL: data.FailURL,
                ActionURL: data.ActionURL,
                Name: data.Name,
                Email: data.Email,
                ContactNumber: data.ContactNumber,
                Currency: data.Currency
            };

            history.push('/PayUTransaction', param);
            return;
        } else {
            setResponseMessage('No gateway found');
            setSeverity('error');
        }
    };

    const UPIGatewayPaymentHandler = (RedirectURL) => {
        window.open(RedirectURL);
    };
    const PhonePayPGHandler = (RedirectURL) => {
        window.open(RedirectURL);
    };

    const UPIALLPaymentHandler = (RedirectURL) => {
        window.open(RedirectURL);
    };

    const razorPaymentHandler = (param) => {
        let options = {};
        let arr = param.PaymentMode.split(',');

        if (arr.length <= 0 || param.PaymentMode.length <= 2) {
            options = {
                key: param.Key,
                amount: param.Amount,
                currency: param.Currency,
                name: CLIENT_NAME(),
                description: 'Transaction',
                image: LOGO_URL(),
                order_id: param.OrderID,
                callback_url: param.ResponseSuccessURL,
                prefill: {
                    name: param.Name,
                    email: param.Email,
                    contact: param.ContactNumber
                },
                notes: {
                    address: CLIENT_ADDRESS()
                },
                theme: {
                    color: APP_BAR_COLOR()
                }
            };
        } else {
            let paymentmodeArray = [];
            arr.map((item) => {
                paymentmodeArray.push({ method: item });
            });

            let config = {
                display: {
                    blocks: {
                        banks: {
                            name: 'All payment methods',
                            instruments: paymentmodeArray
                        }
                    },
                    sequence: ['block.banks'],
                    preferences: {
                        show_default_blocks: false
                    }
                }
            };

            options = {
                key: param.Key,
                amount: param.Amount,
                currency: param.Currency,
                name: CLIENT_NAME(),
                description: 'Transaction',
                image: LOGO_URL(),
                order_id: param.OrderID,
                callback_url: param.ResponseSuccessURL,
                prefill: {
                    name: param.Name,
                    email: param.Email,
                    contact: param.ContactNumber
                },
                notes: {
                    address: CLIENT_ADDRESS()
                },
                theme: {
                    color: APP_BAR_COLOR()
                },
                config: config
            };
        }

        const rzp1 = new window.Razorpay(options);

        rzp1.open();
    };

    const worldLinePaymentHandler = (param) => {
        const reqJson = {
            features: {
                enableAbortResponse: true,
                enableExpressPay: true,
                enableInstrumentDeRegistration: true,
                enableMerTxnDetails: true,
                enableNewWindowFlow: true,
                showLoader: true,
                showPGResponseMsg: true,
                showDownloadReceipt: false,
                payDetailsAtMerchantEnd: false,
                payWithSavedInstrument: false,
                hideSavedInstruments: false,
                separateCardMode: false
            },
            consumerData: {
                deviceId: 'WEBSH2',
                token: param.Token,
                returnUrl: param.RedirectURL,
                responseHandler: param.CallBackURL,
                paymentMode: 'all',
                merchantLogoUrl: LOGO_URL(),
                merchantId: param.MerchantID,
                currency: param.Currency,
                consumerId: param.ConsumerID,
                consumerMobileNo: param.ConsumerMobileNo,
                consumerEmailId: param.ConsumerEmailId,
                txnId: param.OrderID, // Unique merchant transaction ID
                items: [
                    {
                        itemId: param.Key,
                        amount: param.Amount,
                        comAmt: param.Commission
                    }
                ],
                customStyle: {}
            }
        };

        if (window.$ && window.$.pnCheckout) {
            window.$.pnCheckout(reqJson);
            if (reqJson.features.enableNewWindowFlow) {
                window.pnCheckoutShared.openNewWindow();
            }
        } else {
            console.log('The script did not load correctly');
        }
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
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {() => {
                    return (
                        <Grid
                            item
                            container
                            margin="normal"
                            xs={12}
                            md={12}
                            sm={12}
                            lg={12}
                            spacing={2}
                        >
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <Form>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <h2
                                                className={classes.PaperHead_h2}
                                            >
                                                Add Money
                                            </h2>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            {isStaticQR ? (
                                                <PrimaryButton
                                                    variant="contained"
                                                    color="primary"
                                                    title="Static QR"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    onClick={() => {
                                                        setIsOpen(true);
                                                    }}
                                                >
                                                    Static QR
                                                </PrimaryButton>
                                            ) : null}
                                        </Grid>
                                    </Grid>
                                    <Grid
                                        item
                                        xs={12}
                                        style={{ marginTop: '10px' }}
                                    >
                                        <Paper className={classes.PaperStyle}>
                                            <Grid container spacing={2}>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <MaterialSelect
                                                        label="Wallet"
                                                        name="WalletID"
                                                        title="Select Wallet"
                                                    >
                                                        <MenuItem
                                                            key="0"
                                                            value="0"
                                                        >
                                                            Select Wallet
                                                        </MenuItem>
                                                        {wallets.map((item) => (
                                                            <MenuItem
                                                                key={
                                                                    item.WalletID
                                                                }
                                                                value={
                                                                    item.WalletID
                                                                }
                                                            >
                                                                {item.Name}
                                                            </MenuItem>
                                                        ))}
                                                    </MaterialSelect>
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <MaterialNumberField
                                                        label="Amount"
                                                        name="Amount"
                                                        type="text"
                                                        length={6}
                                                        placeholder="Amount"
                                                    />
                                                </Grid>
                                                {IsTypeDropdown ? (
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={12}
                                                        lg={12}
                                                    >
                                                        <MaterialSelect
                                                            label="Type"
                                                            name="Type"
                                                            title="Type"
                                                        >
                                                            <MenuItem
                                                                key="UPI"
                                                                value="UPI"
                                                            >
                                                                UPI
                                                            </MenuItem>
                                                            <MenuItem
                                                                key="PG"
                                                                value="PG"
                                                            >
                                                                PG
                                                            </MenuItem>
                                                        </MaterialSelect>
                                                    </Grid>
                                                ) : null}
                                                {IsTypeDropdown ? (
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={12}
                                                        lg={12}
                                                    ></Grid>
                                                ) : null}
                                            </Grid>
                                            <Divider
                                                variant="middle"
                                                className="rechdevider"
                                            />

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                                spacing={2}
                                                container
                                                className={classes.footerGrid}
                                            >
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={6}
                                                    lg={6}
                                                >
                                                    <SecondaryButton
                                                        size="large"
                                                        type="reset"
                                                        title="Reset"
                                                        endIcon={<ResetIcon />}
                                                    ></SecondaryButton>
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={6}
                                                    lg={6}
                                                >
                                                    <PrimaryButton
                                                        type="submit"
                                                        title="Add Money"
                                                        endIcon={<PayIcon />}
                                                        disabled={loadingOpen}
                                                    ></PrimaryButton>
                                                </Grid>
                                            </Grid>
                                        </Paper>
                                    </Grid>
                                </Form>
                            </Grid>
                            <Grid
                                item
                                xs={12}
                                sm={12}
                                md={8}
                                lg={8}
                                className="lastrecharges"
                            >
                                <h2 className={classes.PaperHead_h2}>
                                    Last 10 Payment Gateway Transaction
                                </h2>
                                <ReactTable
                                    data={transactions}
                                    columns={mainColumns}
                                    headerToolbar={false}
                                    footerToolbar={false}
                                />
                            </Grid>
                        </Grid>
                    );
                }}
            </Formik>

            {totalCount >= 1 ? (
                <Grid item xs={12} sm={12} md={12} lg={12}>
                    <h2 className={classes.PaperHead_h2}>
                        Commission Structure
                    </h2>
                    <ReactTable
                        data={Commissiondata}
                        columns={CommissionmainColumns}
                        count={totalCount}
                        rowsPerPage={TakeCount}
                        getCallback={({ currentPage, RowsPerPage }) => {
                            setCurrentPage(currentPage);
                            setTakeCount(RowsPerPage);
                            setSkipCount(RowsPerPage * currentPage);
                            callSelectData(
                                RowsPerPage,
                                RowsPerPage * currentPage,
                                '',
                                '',
                                ''
                            );
                        }}
                        headerToolbar={false}
                        footerToolbar={false}
                    />
                </Grid>
            ) : null}
            {isAlertOpen ? (
                <TransactionConfirmation
                    isOpen={isAlertOpen}
                    title="Add Money"
                    message="Are you sure to add money?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Transaction(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
