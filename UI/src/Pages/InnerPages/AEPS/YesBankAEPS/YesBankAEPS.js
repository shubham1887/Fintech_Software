import React, { useEffect, useState, useContext, useRef } from 'react';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { TransactionConfirmation } from 'Components/Alert/TransactionConfirmation';
import { TransactionResponse } from 'Components/Alert/TransactionResponse';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { useLocation } from 'react-router-dom';
import { FormControlLabel, Checkbox, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@material-ui/core'

import {
    CashWithdrawal as CASH_WITHDRAWAL,
    BalanceEnquiry as BALANCE_ENQUIRY,
    MiniStatement as MINI_STATEMENT
} from 'Components/Api/YesBankAEPS';
import { SelectBank as BANK_SELECT } from 'Components/Api/NPCIBankAPI';

import { SelectAEPSForReceipt as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import Box from '@material-ui/core/Box';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';

import {
    Container,
    Grid,
    Paper,
    Button
} from '@material-ui/core';
import { AEPS_CODE } from 'Components/Helper/Messages';

import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import {
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import {
    AEPS_BALANCEENQUIRY,
    AEPS_MINISTATEMENT,
    AEPS_CASHWITHDRAW
} from 'Components/Helper/OperatorCode';
import { BalanceContext } from 'Components/BalanceContext';
import usePosition from 'Components/Posistion';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import HOCLoader from 'Components/Loader/HOCLoader';
import InputLabel from 'Components/Inputes/InputLabel';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import { GetFingerPrint } from 'Components/FingerPrintProcessing';
import { FailTransactionResponse } from 'Components/Alert/FailTransactionResponse';
import { AEPSPrint } from 'Components/Print/AEPSPrint';
import { useHistory } from 'react-router';
import { YESBANK } from 'Components/Helper/AEPSProviderName';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { Last10AEPSReport } from 'Pages/InnerPages/Report/Last10AEPSReport';
import { DailyAuthenticationSelect as SELECT_LAST_2FA } from 'Components/Api/DailyAuthonticationApi';

import { DailyAuthentication as DAILY_AUTHENTICATION } from 'Components/Api/YesBankAEPS';

const CustomContainer = HOCLoader(Container);

export const YesBankAEPS = () => {
    const history = useHistory();
    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }
    const [RefreshRandom, setRefreshRandom] = useState(0);
    let serviceCode = AEPS_CODE;

    const formRef = useRef();
    const { DashboardBalanceUpdate } = useContext(BalanceContext);

    const classes = commonStyle();

    const [Operator, setOperator] = useState([]);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [shouldDisplayAmount, setShouldDisplayAmount] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [isPrintOpen, setIsPrintOpen] = useState(false);
    const [isPin] = useState(localStorage.getItem('IsPin'));
    const [InWords, setInWords] = useState('');
    const [bankList, setBankList] = useState([]);
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [ResponseAlert, setResponseAlert] = useState({});
    const [status, setStatus] = useState('PROCESS');
    const [isAlertErrorOpen, setIsAlertErrorOpen] = useState(false);
    const [Message, setMessage] = useState('');
    const [printData, setPrintData] = useState(null);
    const { latitude, longitude, error } = usePosition();
    const [LastTwoFA, setLastTwoFA] = useState('');
    const [isVerifyAlertOpen, setVerifyAlertOpen] = useState(false);
    const [IsProceedBtn, setIsProceedBtn] = useState(true);
    const [IsMerchantBtn, setIsMerchantBtn] = useState(false);
    const [IsActiveMerchantverify, setIsActiveMerchantverify] = useState(false);
    const RetailerAadharNumber = localStorage.getItem('AadharNo');
    const [openAgentTC, setOpenAgentTC] = useState(false);
    const [openAadhaarConsent, setOpenAadhaarConsent] = useState(false);

    const [selectedOperatorID, setSelectedOperatorID] = useState('');

    const location = useLocation();
    const { IconName } = location.state || {};

    const [iconName, setIconName] = useState('');

    const handleOpenAgentTC = () => setOpenAgentTC(true);
    const handleCloseAgentTC = () => setOpenAgentTC(false);
    const handleOpenAadhaarConsent = () => setOpenAadhaarConsent(true);
    const handleCloseAadhaarConsent = () => setOpenAadhaarConsent(false);


    const [bankDefault, setBankDefault] = useState({
        IIN: '',
        BankName: 'Select Bank Name'
    });
    const [formObj, setFormObj] = useState({
        ServiceID: serviceCode,
        IIN: '',
        Type: '',
        BankName: '',
        OperatorID: selectedOperatorID,
        AadhaarNumber: '',
        ConsumerNumber: '',
        Amount: '',
        InWords: '',
        OperatorName: IconName,
        OperatorImagePath: '',
        Pin: '',
        IsPin: isPin,
        Bank: bankDefault,
        Latitude: latitude,
        Longitude: longitude,
        iconName: IconName,
        agreeToTerms: false,
        agreeToConsent: false
    });
    const reset = () => {
        setInWords('');
        setBankDefault({
            IIN: '',
            BankName: 'Select Bank Name'
        });
        setFormObj({
            ServiceID: serviceCode,
            OperatorID: '',
            AadhaarNumber: '',
            ConsumerNumber: '',
            Amount: '',
            InWords: '',
            OperatorName: '',
            OperatorImagePath: '',
            Pin: '',
            IsPin: isPin,
            Type: '',
            iconName: formObj.iconName,
            agreeToTerms: false,
            agreeToConsent: false
        });
    };

    const validationSchema = Yup.object({

        AadhaarNumber: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'aadhaar number/vid'),

        ConsumerNumber: Yup.string()
            .length(10, 'Must be 10 digit')
            .required(PLEASE_PROVIDE_VALID + 'number'),

        Pin: Yup.number().when(['IsPin'], {
            is: (isPin) => isPin === 'true',
            then: Yup.number().required('Please enter pin')
        }),

        Amount: Yup.number().when('iconName', {
            is: (iconName) => iconName === 'AEPS Cash Withdrawal',
            then: Yup.number()
                .required(PLEASE_PROVIDE_VALID + 'amount')
                .min(50, 'Amount must be at least 50')
                .test(
                    'is-multiple-of-50',
                    'Amount must be a multiple of 50',
                    (value) => value % 50 === 0
                ),
            otherwise: Yup.number().notRequired(),
        }),
        agreeToTerms: Yup.boolean().oneOf([true], 'You must accept the agent terms and conditions'),
        agreeToConsent: Yup.boolean().oneOf([true], 'You must accept the Customer Aadhaar Consent clause T & C'),

    });

    const selectBank = async () => {
        setLoadingOpen(true);
        const response = await BANK_SELECT();
        setLoadingOpen(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);

            setStatus('FAILURE');
            setMessage(response.data);
            setIsAlertErrorOpen(true);
        }

        var data = JSON.parse(response.data);
        let objWithSelectAll = [bankDefault, ...data];

        setBankList(objWithSelectAll);
    };


    const selectlast2fa = async (values) => {
        setLoadingOpen(true);
        var request = {
            ProviderName: YESBANK
        };
        var requestObj = JSON.stringify(request);

        const response = await SELECT_LAST_2FA(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);
        setLastTwoFA(data.DOC);
        setIsActiveMerchantverify(data.IsMerchantVerify);
        if (data.Is2FA) {
            history.push('/YBAEPSAuthentication', { IconName: IconName });
            return;
        }
    };

    const submitHandle = async (values, { resetForm }) => {

        if (loadingOpen) return;
        if (values.IsMerchantVerify) {
            setVerifyAlertOpen(true);
            setConfirmAlertRequest(values);
            return;
        } else {
            let op = Operator.filter(
                (o) => o.OperatorID === selectedOperatorID
            );
            values.OperatorName = IconName;

            //setIsAlertOpen(true);
            Proceed(values);
            setConfirmAlertRequest(values);
        }
    };

    const Authentication = async (values) => {
        setIsError(false);
        setResponseMessage('');
        setLoadingOpen(true);

        const { status, xmldata } = await GetFingerPrint('');

        if (status === false) {
            alert(xmldata);
            return;
        }
        let fingerPrintXML = xmldata;

        var request = {
            FingerPrint: fingerPrintXML,
            adhaarnumber: RetailerAadharNumber,
            Type: 'TXN_AUTH',
            IIN: values.IIN,
            Latitude: latitude,
            Longitude: longitude
        };

        var requestObj = JSON.stringify(request);

        var response = await DAILY_AUTHENTICATION(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        var data = JSON.parse(response.data);

        if (!response.status) {
            var data = JSON.parse(response.data);
            setIsError(true);
            setSeverity('error');

            setStatus('FAILURE');
            setMessage(data.Message);
            setIsAlertErrorOpen(true);
            return;
        }

        setIsMerchantBtn(false);
        setIsProceedBtn(true);
        setSeverity('success');
        setResponseMessage(data.Message);
        setIsError(true);
    };



    const Proceed = async (values) => {
        setLoadingOpen(true);

        //setFingerPrint('');
        setResponseMessage('');
        setIsError(false);

        const { status, xmldata } = await GetFingerPrint('');

        if (status === false) {
            alert(xmldata);
            return;
        }
        let fingerPrintXML = xmldata;

        let response = '';

        if (selectedOperatorID === AEPS_BALANCEENQUIRY) {
            var requestObj = {
                AadhaarNumber: values.AadhaarNumber,
                ConsumerNumber: values.ConsumerNumber,
                IIN: values.IIN,
                BankName: values.BankName,
                FingerPrint: fingerPrintXML,
                Latitude: latitude,
                Longitude: longitude
            };
            var request = JSON.stringify(requestObj);

            response = await BALANCE_ENQUIRY(request);
        } else if (selectedOperatorID === AEPS_MINISTATEMENT) {
            var requestObjMS = {
                AadhaarNumber: values.AadhaarNumber,
                ConsumerNumber: values.ConsumerNumber,
                IIN: values.IIN,
                BankName: values.BankName,
                FingerPrint: fingerPrintXML,
                Latitude: latitude,
                Longitude: longitude
            };
            var requestMS = JSON.stringify(requestObjMS);

            response = await MINI_STATEMENT(requestMS);
        } else {
            var requestObjCW = {
                AadhaarNumber: values.AadhaarNumber,
                ConsumerNumber: values.ConsumerNumber,
                IIN: values.IIN,
                BankName: values.BankName,
                FingerPrint: fingerPrintXML,
                Amount: values.Amount,
                Latitude: latitude,
                Longitude: longitude
            };
            var requestCW = JSON.stringify(requestObjCW);

            response = await CASH_WITHDRAWAL(requestCW);
        }

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        await DashboardBalanceUpdate();

        setRefreshRandom(Math.random(0));

        var data = JSON.parse(response.data);

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(data.Message);
            setIsError(true);
            //reset();

            setStatus('FAILURE');
            setMessage(data.Message);
            setIsAlertErrorOpen(true);

            return;
        }
        setSeverity('success');
        setResponseMessage(
            data.Message
        );
        setStatus(data.Status);
        //setResponseAlert(data);
        setResponseAlert({ ...data, IconName });
        setIsResponseAlertOpen(true);
        setIsError(true);
        //reset();
        if (data.Status === 'FAILURE') {
            setSeverity('error');
            setResponseMessage(data.Message);
            setIsError(true);

            setStatus('FAILURE');
            setMessage(data.Message);
            setIsAlertErrorOpen(true);
            //reset();
            return;
        }
        reset();
    };

    const Print = (VendorID) => {
        SelectPrintData(VendorID);
    };

    const SelectPrintData = async (TransactionMasterID) => {
        var req = {
            ID: TransactionMasterID
        };
        var request = JSON.stringify(req);
        setLoadingOpen(true);
        const { isAuthorized, status, data } = await TRANSACTION_RECEIPT(
            request
        );
        setLoadingOpen(false);
        if (!isAuthorized) LOGOUT();
        var d = JSON.parse(data);
        setPrintData(d);
        setIsPrintOpen(true);
    };

    const OnChange = (value, setFieldValue) => {
        if (value === undefined || value <= 0) return;
        setInWords(NumberToWord(value));
        setFieldValue('InWords', NumberToWord(value));
    };

    const handleOperatorChange = (selectedOperatorID) => {
        setIsProceedBtn(true);
        setIsMerchantBtn(false);

        if (selectedOperatorID === AEPS_BALANCEENQUIRY) {
            setShouldDisplayAmount(false);
            return;
        }
        if (selectedOperatorID === AEPS_MINISTATEMENT) {
            setShouldDisplayAmount(false);
            return;
        }
        if (selectedOperatorID === AEPS_CASHWITHDRAW && IsActiveMerchantverify === true) {
            setIsProceedBtn(false);
            setIsMerchantBtn(true);
        }
        setShouldDisplayAmount(true);
    };

    const selectOperator = async (ServiceID) => {
        if (
            ServiceID === null ||
            ServiceID === undefined ||
            ServiceID === 0 ||
            ServiceID === '0'
        )
            return;

        var requestObj = {
            ServiceID: ServiceID
        };

        var request = JSON.stringify(requestObj);

        const response = await OPERATOR_SELECT_BY_SERVICEID(request);

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) setIsError(true);

        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);
            setOperator(data);
        }
    };

    useEffect(() => {
        fetchConsumerNumberFontSize();
        selectBank();
        selectlast2fa();
        selectOperator(serviceCode);
    }, []);

    useEffect(() => {
        if (IconName) {
            let operatorID = '';
            switch (IconName) {
                case 'AEPS Cash Withdrawal':
                    operatorID = AEPS_CASHWITHDRAW;
                    break;
                case 'AEPS Balance Enquiry':
                    operatorID = AEPS_BALANCEENQUIRY;
                    break;
                case 'AEPS Mini Statement':
                    operatorID = AEPS_MINISTATEMENT;
                    break;
                default:
                    operatorID = '';
            }
            setSelectedOperatorID(operatorID);

            setIconName(IconName);
        }
    }, [IconName]);

    useEffect(() => {
        if (selectedOperatorID) {
            handleOperatorChange(selectedOperatorID);
        }
    }, [selectedOperatorID]);


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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={submitHandle}
                resetForm={formObj}
            >
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <Grid container margin="normal" spacing={2}>
                                <Grid item xs={12} sm={12} md={4} lg={4}>
                                    <Grid xs={12} sm={12} md={4} lg={4}>
                                        {' '}
                                        <Grid xs={12} sm={12} md={4} lg={4}>
                                            <h2
                                                className={classes.PaperHead_h2}
                                            >
                                                AEPS
                                            </h2>
                                        </Grid>
                                    </Grid>
                                    <Grid
                                        xs={12}
                                        sm={12}
                                        md={12}
                                        lg={12}
                                        className={classes.twofa}
                                    >
                                        <Box>
                                            <span className={classes.lasttwofa}>
                                                <span
                                                    className={
                                                        classes.labellasttwofa
                                                    }
                                                >
                                                    Last 2FA :
                                                </span>
                                                {LastTwoFA}
                                            </span>

                                            <Button
                                                variant="contained"
                                                color="primary"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={() => {
                                                    history.push(
                                                        '/YBAEPSAuthentication'
                                                    );
                                                }}
                                            >
                                                2FA
                                            </Button>
                                        </Box>
                                    </Grid>

                                    <Grid item xs={12} sm={12}>
                                        <Paper className={classes.PaperStyle}>
                                            <Grid container spacing={1}>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <MaterialTextField
                                                        label="Transaction Type*"
                                                        name="iconName"
                                                        type="text"
                                                        readOnly={true}
                                                        placeholder="Name"
                                                    />
                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <Field
                                                        component={
                                                            AutoCompleteTextView
                                                        }
                                                        defaultValue={
                                                            bankDefault
                                                        }
                                                        SubFieldName="BankName"
                                                        FieldName="Bank"
                                                        name="Bank"
                                                        onSearchCallback={(
                                                            text
                                                        ) => { }}
                                                        onCustomCallback={(
                                                            name,
                                                            value
                                                        ) => {
                                                            setFieldValue(
                                                                'IIN',
                                                                value.IIN
                                                            );

                                                            setFieldValue(
                                                                'BankName',
                                                                value.BankName
                                                            );

                                                            setBankDefault({
                                                                IIN: value.IIN,
                                                                BankName:
                                                                    value.BankName
                                                            });


                                                        }}
                                                        options={bankList}
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.BankName}
                                                        getOptionSelected={(
                                                            option,
                                                            value
                                                        ) =>
                                                            option.BankName ===
                                                            value.BankName
                                                        }
                                                        textFieldProps={{
                                                            label: 'Bank Name*'
                                                        }}
                                                    />
                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <MaterialNumberField
                                                        font_size="25"
                                                        label="Aadhaar Number/VID*"
                                                        name="AadhaarNumber"
                                                        icon="mobile"
                                                        placeholder="Enter Aadhaar Number/VID"
                                                        title="Aadhaar Number"
                                                        length="16"
                                                        typed_length={1}
                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <MaterialNumberField
                                                        font_size="25"
                                                        label="Mobile Number*"
                                                        name="ConsumerNumber"
                                                        icon="mobile"
                                                        placeholder="Enter Mobile Number"
                                                        title="Mobile Number"
                                                        length="10"
                                                        typed_length={1}
                                                    />
                                                </Grid>

                                                {shouldDisplayAmount ? (
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={12}
                                                        lg={12}
                                                    >
                                                        <MaterialCurrency
                                                            font_size="25"
                                                            name="Amount"
                                                            label="Amount (₹)*"
                                                            placeholder="₹ Amount"
                                                            autoComplete="new-password"
                                                            size="normal"
                                                            max={10000}
                                                            title="Amount"
                                                            onCustomCallback={(value) => {
                                                                OnChange(value, setFieldValue);
                                                            }}
                                                        />
                                                        <InputLabel
                                                            color="green"
                                                            value={InWords}
                                                        />
                                                    </Grid>


                                                ) : null}
                                                {isPin === 'true' ? (
                                                    <Grid item xs>
                                                        <MaterialNumberField
                                                            label="Pin"
                                                            type="password"
                                                            name="Pin"
                                                            placeholder="Pin"
                                                            title="Pin"
                                                            length={4}
                                                        />
                                                    </Grid>
                                                ) : null}

                                                <FormControlLabel
                                                    control={
                                                        <Field
                                                            as={Checkbox}
                                                            name="agreeToTerms"
                                                            color="primary"
                                                        />
                                                    }
                                                    label={
                                                        <span>
                                                            By clicking on proceed, you agree to our
                                                            <span style={{ color: 'blue', cursor: 'pointer' }} onClick={handleOpenAgentTC}> Agent T & C </span>

                                                        </span>
                                                    }
                                                />
                                                <ErrorMessage
                                                    style={{ color: 'red' }}
                                                    name="agreeToTerms"
                                                    component="div"
                                                    className="field-error-red"
                                                />

                                                <FormControlLabel
                                                    control={
                                                        <Field
                                                            style={{ marginTop: '-21px' }}
                                                            as={Checkbox}
                                                            name="agreeToConsent"
                                                            color="primary"
                                                        />
                                                    }
                                                    label={
                                                        <span>
                                                            By clicking on proceed, you agree to our

                                                            <span style={{ color: 'blue', cursor: 'pointer' }} onClick={handleOpenAadhaarConsent}> Customer Aadhaar Consent clause T& C</span>
                                                        </span>
                                                    }
                                                />
                                                <ErrorMessage
                                                    style={{ color: 'red' }}
                                                    name="agreeToConsent"
                                                    component="div"
                                                    className="field-error-red"
                                                />
                                            </Grid>
                                        </Paper>
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
                                                    disabled={loadingOpen}
                                                    title="Reset"
                                                    onClick={() => {
                                                        reset();
                                                    }}
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={6}
                                                lg={6}
                                            >
                                                {IsProceedBtn && (
                                                    <PrimaryButton
                                                        title="Proceed"
                                                        onClick={(e) => {
                                                            setFieldValue(
                                                                'IsProceed',
                                                                true
                                                            );
                                                            setFieldValue(
                                                                'IsMerchantVerify',
                                                                false
                                                            );
                                                        }}
                                                        disabled={loadingOpen}

                                                    />
                                                )}
                                                {IsMerchantBtn && (
                                                    <PrimaryButton
                                                        title="Merchant Verify"
                                                        onClick={(e) => {
                                                            setFieldValue(
                                                                'IsProceed',
                                                                false
                                                            );
                                                            setFieldValue(
                                                                'IsMerchantVerify',
                                                                true
                                                            );
                                                        }}
                                                        disabled={loadingOpen}

                                                    />
                                                )}
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                </Grid>
                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={8}
                                    lg={8}
                                    className="lastrecharges"
                                >
                                    <Last10AEPSReport
                                        ServiceID={serviceCode}
                                        refreshRandom={RefreshRandom}
                                    />
                                </Grid>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            <Dialog open={openAgentTC} onClose={handleCloseAgentTC}>
                <DialogTitle>Agent Terms & Conditions</DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        <p>1 I hereby confirm that the Customer has been explained (in a language known to the customer) about the nature of information, sharing of such information upon authentication, aspects of the transaction and the terms and conditions applicable to the transaction. The Customer has understood and has authorized the Bank to fetch applicable data from UIDAI.
                        </p>
                        <p> 2 I hereby confirm that the Customer has provided all the information voluntarily to the Bank and are true, correct and complete."
                        </p>
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseAgentTC} color="primary">
                        Close
                    </Button>
                </DialogActions>
            </Dialog>
            <Dialog open={openAadhaarConsent} onClose={handleCloseAadhaarConsent}>
                <DialogTitle>Customer Aadhaar Consent clause Terms & Conditions</DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        <p>
                            1. I, the Customer hereby authorizes the Bank store my AADHAAR number and fetch, authenticate and store all such necessary details retrieved or to be retrieved from UIDAI through AADHAAR Number and IRIS/biometric authentication for the purpose of this transaction. In case of any discrepancies, the Bank reserves the sole right to block my account/ relation and transaction without any further notice or intimation.
                        </p>
                        <p>2. Customer hereby states and undertake that it has no objection, if the information disclosed to the Bank is authenticated vide Aadhaar based system. Customer hereby voluntarily agree and confirm to provide or disclose (as required under the Aadhaar Act 2016 and Regulations framed there under) my identification information (Aadhaar number, biometric information & demographic information) for Aadhaar based authentication system and/or such similar authentication mechanism as provided or stipulated by the Government, from time to time, for the purpose of availing banking service, including operations of account or any other facility relating to banking operations.
                        </p>
                        <p> 3. The Customer hereby agree and confirm that the Bank will not oblige to process any request made by me, if:
                        </p>
                        <ol>
                            <li> Aadhaar number provided by me is incorrect.</li>
                            <li> Details in account does not match with details available with UIDAI.</li>
                        </ol>
                        <p>
                            4. The Customer hereby agree and confirm that the Aadhaar details may be updated for all my banking service, including but not limited to the operation of bank account, for the purposes of authenticate.
                        </p>
                        <p>5. The Customer hereby agree and confirm that my account may be used by receiving any government payments across schemes that the Customer is eligible and/or any other payment using the Aadhaar based information. The Customer hereby acknowledge and agree that NPCI may map the Customer's Account in the Aadhaar mapper of NPCI.
                        </p>
                        <p>6. The Customer hereby confirms that the Aadhaar detail provided by the Customer is true, correct and complete in all aspect.
                        </p>
                        <p>
                            7. The Customer hereby acknowledges that the Bank will not have any liability or responsibility, if the details provided by the Customer is/or subsequently become false, incorrect or incomplete, either in part or as whole.
                        </p>
                        <p>8. Before agreeing to the  above mentioned terms and conditions, the Customer has been explained (in a language known to the Customer) about the nature of information that may be shared upon authentication and have understood the nature and implication of the same, as stipulated under the applicable law. The Customer declares all the information has been voluntarily furnished by them to the Bank, without any duress or coercion."
                        </p>
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseAadhaarConsent} color="primary">
                        Close
                    </Button>
                </DialogActions>
            </Dialog>

            {isAlertOpen ? (
                <TransactionConfirmation
                    isOpen={isAlertOpen}
                    title="AEPS"
                    message="Are you sure to do transaction?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Proceed(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
            {isResponseAlertOpen ? (
                <TransactionResponse
                    isOpen={isResponseAlertOpen}
                    title={status}
                    ServiceID={AEPS_CODE}
                    request={ResponseAlert}
                    OkButtonClick={(request) => {
                        setIsResponseAlertOpen(false);
                    }}
                    PrintButtonClick={() => {
                        Print(ResponseAlert.VendorID);
                        setIsResponseAlertOpen(false);
                    }}
                />
            ) : null}

            {isVerifyAlertOpen && (
                <ConfirmAlert
                    isOpen={isVerifyAlertOpen}
                    title="Merchant Authentication"
                    message="Are you sure to do merchant authentication?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setVerifyAlertOpen(false);
                        Authentication(request);
                    }}
                    negativeButtonClick={() => {
                        setVerifyAlertOpen(false);
                    }}
                />
            )}

            {isPrintOpen ? (
                <AEPSPrint
                    isOpen={isPrintOpen}
                    title="AEPS Print"
                    request={printData}
                    //request={ResponseAlert}
                    positiveButtonClick={() => {
                        setIsPrintOpen(false);
                        reset();
                    }}
                    negativeButtonClick={() => {
                        setIsPrintOpen(false);
                        reset();
                    }}
                />
            ) : null}

            {isAlertErrorOpen ? (
                <FailTransactionResponse
                    isOpen={isAlertErrorOpen}
                    title={status}
                    Message={Message}
                    OkButtonClick={(request) => {
                        setIsAlertErrorOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
