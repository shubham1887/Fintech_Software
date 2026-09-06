import React, { useState, useRef, useEffect } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Button,
    Box,
    CircularProgress,
    Paper
} from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { LOGOUT } from 'ConstComponents';
import { VendorPay as VENDORPAY_TRANSACTION } from 'Components/Api/TransactionAPI';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectForReceipt as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { PayoutPrint } from 'Components/Print/PayoutPrint';
import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { VENDOR_PAY } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    BeneAdd as BENE_ADD,
    VerifyOTP,
    DeleteBene,
    SelectBeneficiary
} from 'Components/Api/PayoutBeneficiaryAPI';
import AsyncOperatorAutoComplete from 'Components/Inputes/AsyncOperatorAutoComplete';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';

import { Last10RechargeReport } from 'Pages/InnerPages/Report/Last10RechargeReport';
import { TransactionResponse } from 'Components/Alert/TransactionResponse';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import { OTPModal } from 'Components/Modal/OTPModal';
import { Select as BANK_SELECT } from 'Components/Api/DMTBankAPI';
import { useHistory } from 'react-router';
import { TransactionConfirmation } from 'Components/Alert/TransactionConfirmation';
import { PayoutTransactionConfirmation } from 'Components/Alert/PayoutTransactionConfirmation';
import usePosition from 'Components/Posistion';

const CustomContainer = HOCLoader(Container);

var PayoutOperators = [];

export const SmartPayTransaction = () => {
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [confirmAlertMessage, setConfirmAlertMessage] = useState('');
    const [serviceCode, setserviceCode] = useState(VENDOR_PAY);
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [ResponseAlert, setResponseAlert] = useState({});
    const [status, setStatus] = useState('PROCESS');
    const [Operator, setOperator] = useState([]);
    const [isDeleteAlertOpen, setIsDeleteAlertOpen] = useState(false);

    const [isPrintOpen, setIsPrintOpen] = useState(false);
    const [bankList, setBankList] = useState([]);
    const [printData, setPrintData] = useState(null);
    const formRef = useRef();
    const [isReadOnly, setReadOnly] = useState(false);
    const [openOTP, setOpenOTP] = useState(false);
    const [modalErrorMessage, setModalErrorMessage] = useState('');
    const [BeneficiaryID, setBeneficiaryID] = useState(0);
    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');

    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [currentPage, setCurrentPage] = useState(0);

    const [isAddOnly, setIsAddOnly] = useState(false);
    const { latitude, longitude, error } = usePosition();

    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }
    const history = useHistory();

    const [OperatorDefault, setOperatorDefault] = useState({
        OperatorID: 0,
        OperatorName: 'Select Operator'
    });

    const [bankDefault, setBankDefault] = useState({
        Name: 'Select Bank',
        DefaultIFSC: ''
    });

    const [FormObj, setFormObj] = useState({
        ServiceID: VENDOR_PAY,
        Name: '',
        Bank: bankDefault,
        AccountNo: '',
        ConfirmAccountNo: '',
        IFSC: '',
        isAddOnly: false,
        IsNameVerified: false,
        Latitude: latitude,
        Longitude: longitude
    });

    const FormValidationSchema = Yup.object({
        Operator: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator')
            .shape({
                OperatorID: Yup.number()
                    .notOneOf([0], PLEASE_SELECT_VALID + 'operator')
                    .nullable(PLEASE_SELECT_VALID + 'operator')
                    .required(PLEASE_SELECT_VALID + 'operator')
            }),

        Bank: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'bank')
            .required(PLEASE_SELECT_VALID + 'bank')

            .shape({
                Name: Yup.string()
                    .nullable(PLEASE_SELECT_VALID + 'bank')
                    // .matches(/^('Select Bank')/, PLEASE_SELECT_VALID + 'bank')
                    .required(PLEASE_SELECT_VALID + 'bank')
            }),

        isAddOnly: Yup.boolean(),

        Name: Yup.string().when(['isAddOnly'], {
            is: (isAddOnly) => isAddOnly,
            then: Yup.string().required(PLEASE_PROVIDE_VALID + 'name')
        }),

        AccountNo: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'account no')
            .min(7, PLEASE_PROVIDE_VALID + 'account no')
            .max(20, PLEASE_PROVIDE_VALID + 'account no'),

        ConfirmAccountNo: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'confirm account no')
            .min(7, PLEASE_PROVIDE_VALID + 'account no')
            .max(20, PLEASE_PROVIDE_VALID + 'account no')
            .oneOf([Yup.ref('AccountNo'), null], "AccountNo don't match"),

        IFSC: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'IFSC')
            .length(11, PLEASE_PROVIDE_VALID + 'IFSC')
    });

    useEffect(() => {
        setLoading(true);
        selectBank();
        fetchConsumerNumberFontSize();
        callSelectData(TakeCount, SkipCount, '', '', '');
    }, [PayoutOperators]);

    const selectBank = async () => {
        const response = await BANK_SELECT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        let objWithSelectAll = [bankDefault, ...data];

        setBankList(objWithSelectAll);
    };

    useEffect(() => {
        setLoading(true);
        selectOperator(serviceCode);
    }, [serviceCode]);

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

        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) setIsError(true);

        if (response.data === 'Requested operators not found') {
            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);

        const optionsProviders = data.map(function (row) {
            return { ID: row.OperatorID, Type: row.OperatorName };
        });

        PayoutOperators = optionsProviders;
        createColumns();

        setOperator(data);
    };

    const Print = (VendorID) => {
        SelectPrintData(VendorID);
    };

    const SelectPrintData = async (TransactionMasterID) => {
        var req = {
            ID: TransactionMasterID
        };
        var request = JSON.stringify(req);
        setLoading(true);
        const { isAuthorized, status, data } = await TRANSACTION_RECEIPT(
            request
        );
        setLoading(false);
        if (!isAuthorized) LOGOUT();
        var d = JSON.parse(data);
        setPrintData(d);
        setIsPrintOpen(true);
    };

    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            IsExcelDownload: false,
            UserID: 0,
            Status: 'APPROVE'
        };

        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);

        //  createColumns();
        setTransactions([]);

        const response = await SelectBeneficiary(requestJSON);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        if (data.length < 0) {
            setSeverity('error');
            setIsError(true);
            setResponseMessage('No transactions found');
            return;
        }
        setTransactions(data.list);
    };

    const OnOperatorChange = async (name, value, setFieldValue) => {
        setOperatorDefault({
            OperatorID: value.OperatorID,
            OperatorName: value.OperatorName
        });

        setFieldValue(name, value);
    };

    const submitHandle = async (values) => {
        setResponseMessage('');
        setModalErrorMessage('');
        setIsError(false);

        if (values.isAddOnly === false) {
            setIsAddOnly(false);

            let jsonObj = {
                ServiceID: VENDOR_PAY,
                Name: values.Name,
                BankName: values.Bank.Name,
                BeneName: values.Name,
                AccountNo: values.AccountNo,
                IFSC: values.IFSC,
                IsNameVerified: values.IsNameVerified,
                OperatorID: values.Operator.OperatorID,
                BeneficiaryID: 0,
                Amount: 1,
                IsVerify: true,
                PaymentType: values.Operator.OperatorName,
                Latitude: latitude,
                Longitude: longitude
            };

            let msg = `Please confirm SmartPay details - \n\nAccountNo : ${
                values.AccountNo
            }\nIFSC : ${values.IFSC}\nAmount : ${'1'}`;

            TransferConfirmation(jsonObj, msg);

            return;
        }

        var request = {
            Name: values.Name,
            BeneName: values.Name,
            BankName: values.Bank.Name,
            AccountNo: values.AccountNo,
            IFSC: values.IFSC,
            IsNameVerified: values.IsNameVerified
        };

        var requestObj = JSON.stringify(request);

        setLoading(true);
        setIsError(false);

        var response = await BENE_ADD(requestObj);

        if (!response.isAuthorized) LOGOUT();
        setLoading(false);

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);

        setResponseMessage(data.Message);
        setSeverity('success');

        if (data.IsOtpVerify) {
            setBeneficiaryID(data.BeneficiaryID);
            setOpenOTP(true);
            return;
        } else {
            setSeverity('success');
            setResponseMessage(data.Message);
            setIsError(true);
            return;
        }
    };

    const VerifyBeneOTP = async (otpObj) => {
        setOpenOTP(false);
        setModalErrorMessage('');
        const { OTP } = otpObj;
        let RequestObj = {
            OTP: OTP,
            BeneficiaryID: BeneficiaryID
        };

        var response = await VerifyOTP(JSON.stringify(RequestObj));
        setLoading(false);

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setOpenOTP(true);
            setModalErrorMessage(response.data);
            return;
        }
        setOpenOTP(false);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        reset('', false, '', '', 0, '', '');
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ServiceID',
            srno: true
        });
        columns.push({
            label: 'BeneName',
            name: 'Name'
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        columns.push({
            label: 'AccountNo',
            name: 'AccountNo'
        });

        columns.push({
            label: 'Operator',
            name: 'OperatorID',
            dropdown: true,
            list: PayoutOperators
        });

        columns.push({
            label: 'Amount',
            name: 'Amount',
            length: 6,
            editableWithNumberOnly: true
        });

        columns.push({
            label: 'Transfer',
            name: 'BeneficiaryID',
            buttonTitle: 'Transfer',
            statusIndex: 2,
            buttonWithTitle: true,
            callback: (value) => {
                PayoutConfirmation(value);
            }
        });
        columns.push({
            label: 'IFSC',
            name: 'IFSC'
        });
        columns.push({
            label: 'Delete',
            name: 'BeneficiaryID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });
        setMainColumns(columns);
    };

    const onDeleteClick = (item) => {
        var RequestObj = { BeneficiaryID: item };
        setIsDeleteAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (requestObj) => {
        setLoading(true);
        var reqObj = JSON.stringify(requestObj);
        var response = await DeleteBene(reqObj);
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }
        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const TransferValidation = Yup.object({
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator'),

        AccountNo: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'account no')
            .min(7, PLEASE_PROVIDE_VALID + 'account no')
            .max(20, PLEASE_PROVIDE_VALID + 'account no'),

        Amount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'amount')
            .min(1, PLEASE_PROVIDE_VALID + 'amount')
            .max(500000, PLEASE_PROVIDE_VALID + 'amount')
    });

    const PayoutConfirmation = async (values) => {
        let jsonObj = {
            ServiceID: VENDOR_PAY,
            OperatorID: values[4],
            AccountNo: values[3],
            IFSC: values[7],
            Amount: values[5],
            BeneficiaryID: values[6],
            IsVerify: false,
            PaymentType: '',
            Latitude: latitude,
            Longitude: longitude
        };

        let msg = `Please confirm SmartPay details - \n\nAccountNo : ${values[3]}\nIFSC : ${values[7]}\nAmount : ${values[5]}`;

        TransferConfirmation(jsonObj, msg);
    };

    const TransferConfirmation = async (jsonObj, msg) => {
        setConfirmAlertRequest(jsonObj);

        setIsError(false);
        var errorValidation = false;

        await TransferValidation.validate(jsonObj).catch(function (err) {
            errorValidation = true;
            setSeverity('error');
            setResponseMessage(err.message);
            setIsError(true);
            callSelectData(TakeCount, SkipCount, '', '', '');
            return;
        });

        if (errorValidation === true) return;

        setConfirmAlertMessage(msg);
        setIsAlertOpen(true);
    };

    const Smartpay = async () => {
        setResponseMessage('');
        setIsError(false);

        var requestObj = JSON.stringify(confirmAlertRequest);
        setLoading(true);
        var response = await VENDORPAY_TRANSACTION(requestObj);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            callSelectData(TakeCount, SkipCount, '', '', '');

            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }

        let dataSuccess = JSON.parse(response.data);

        setSeverity('success');
        setResponseMessage(dataSuccess.Message);
        setIsError(true);
        setStatus(dataSuccess.Status);
        setResponseAlert(dataSuccess);
        setIsResponseAlertOpen(true);

        if (dataSuccess.Status === 'SUCCESS') {
            setReadOnly(true);
        }

        reset(
            dataSuccess.BeneficiaryName,
            confirmAlertRequest.IsVerify,
            confirmAlertRequest.AccountNo,
            confirmAlertRequest.PaymentType,
            confirmAlertRequest.OperatorID,
            confirmAlertRequest.IFSC,
            confirmAlertRequest.BankName,
            confirmAlertRequest.latitude,
            confirmAlertRequest.longitude
        );

        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const reset = (
        BeneficiaryName,
        IsVerify,
        AccountNo,
        PaymentType,
        OperatorID,
        IFSC,
        BankName
    ) => {
        if (IsVerify === true) {
            setBankDefault({
                Name: BankName,
                DefaultIFSC: IFSC
            });

            setOperatorDefault({
                OperatorID: OperatorID,
                OperatorName: PaymentType
            });

            setFormObj({
                ...FormObj,
                AccountNo: AccountNo,
                ConfirmAccountNo: AccountNo,
                Name: BeneficiaryName,
                Operator: OperatorDefault,
                IsNameVerified: true,
                IFSC: IFSC,
                Bank: bankDefault
            });
        }
        if (IsVerify === false) {
            setOperatorDefault({
                OperatorID: 0,
                OperatorName: 'Select Operator'
            });

            setBankDefault({
                Name: 'Select Bank',
                DefaultIFSC: ''
            });

            setFormObj({
                ServiceID: VENDOR_PAY,
                Name: '',
                Bank: bankDefault,
                AccountNo: '',
                ConfirmAccountNo: '',
                IFSC: '',
                isAddOnly: false,
                IsNameVerified: false,
                Operator: OperatorDefault
            });
        }
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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submitHandle}
                resetForm={FormObj}
            >
                {({ values, setFieldValue, handleSubmit }) => {
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
                                    <h2 className={classes.PaperHead_h2}>
                                        Beneficiary Add
                                    </h2>
                                    <Grid item xs={12}>
                                        <Paper className={classes.PaperStyle}>
                                            <Grid container spacing={2}>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <Field
                                                        component={
                                                            AsyncOperatorAutoComplete
                                                        }
                                                        defaultValue={
                                                            OperatorDefault
                                                        }
                                                        ImagePath={'Path'}
                                                        SubFieldName="OperatorID"
                                                        FieldName="Operator"
                                                        name="Operator"
                                                        options={Operator}
                                                        getOptionLabel={(
                                                            option
                                                        ) =>
                                                            option.OperatorName
                                                        }
                                                        getOptionSelected={(
                                                            option,
                                                            value
                                                        ) =>
                                                            option.OperatorName ===
                                                            value.OperatorName
                                                        }
                                                        textFieldProps={{
                                                            label:
                                                                'Select Operator'
                                                        }}
                                                        onCustomCallback={(
                                                            name,
                                                            value
                                                        ) => {
                                                            OnOperatorChange(
                                                                name,
                                                                value,
                                                                setFieldValue
                                                            );
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
                                                    <Field
                                                        component={
                                                            AutoCompleteTextView
                                                        }
                                                        defaultValue={
                                                            bankDefault
                                                        }
                                                        FieldName="Bank"
                                                        name="Bank"
                                                        onCustomCallback={(
                                                            name,
                                                            value
                                                        ) => {
                                                            setFieldValue(
                                                                'IFSC',
                                                                value.DefaultIFSC
                                                            );
                                                            setBankDefault({
                                                                Name:
                                                                    value.Name,
                                                                DefaultIFSC:
                                                                    value.DefaultIFSC
                                                            });
                                                        }}
                                                        options={bankList}
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.Name}
                                                        getOptionSelected={(
                                                            option,
                                                            value
                                                        ) =>
                                                            option.Name ===
                                                            value.Name
                                                        }
                                                        textFieldProps={{
                                                            label: 'Bank'
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
                                                    <MaterialTextField
                                                        label="IFSC"
                                                        name="IFSC"
                                                        type="text"
                                                        length={11}
                                                        placeholder="IFSC"
                                                        readOnly={isReadOnly}
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
                                                        font_size={
                                                            consumerNumberFontSize
                                                        }
                                                        label="Account No."
                                                        name="AccountNo"
                                                        autoComplete="new-password"
                                                        type="text"
                                                        length={20}
                                                        placeholder="Account No."
                                                        typed_length={1}
                                                        readOnly={isReadOnly}
                                                    />
                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={9}
                                                    lg={9}
                                                >
                                                    <MaterialNumberField
                                                        font_size={
                                                            consumerNumberFontSize
                                                        }
                                                        label="Confirm Account No."
                                                        name="ConfirmAccountNo"
                                                        type="text"
                                                        length={20}
                                                        placeholder="Confirm Account No."
                                                        typed_length={1}
                                                        readOnly={isReadOnly}
                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <div
                                                        className={
                                                            classes.SubmitWrapper
                                                        }
                                                    >
                                                        <Button
                                                            type="submit"
                                                            title="Verify"
                                                            className={
                                                                classes.CustomPrimaryButtonStyle
                                                            }
                                                            onClick={(e) => {
                                                                setFieldValue(
                                                                    'isAddOnly',
                                                                    false
                                                                );
                                                                handleSubmit(e);
                                                            }}
                                                            disabled={loading}
                                                        >
                                                            Verify
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
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <MaterialTextField
                                                        font_size={
                                                            consumerNumberFontSize
                                                        }
                                                        label="Bene Name"
                                                        name="Name"
                                                        type="text"
                                                        placeholder="Bene Name"
                                                        readOnly={isReadOnly}
                                                    />
                                                </Grid>
                                            </Grid>
                                        </Paper>
                                    </Grid>

                                    <Grid className={classes.footerGrid}>
                                        <Box className={classes.flexReverse}>
                                            <div
                                                className={
                                                    classes.SubmitWrapper
                                                }
                                                style={{
                                                    marginLeft: '10px'
                                                }}
                                            >
                                                <Button
                                                    type="submit"
                                                    title="Add"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'isAddOnly',
                                                            true
                                                        );
                                                        handleSubmit(e);
                                                    }}
                                                    disabled={loading}
                                                >
                                                    Add
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
                                                title="Reset"
                                                className={
                                                    classes.CustomDangerButtonStyle
                                                }
                                                onClick={() => {
                                                    reset(
                                                        '',
                                                        false,
                                                        '',
                                                        0,
                                                        '',
                                                        ''
                                                    );
                                                }}
                                            >
                                                Reset
                                            </Button>
                                        </Box>
                                    </Grid>
                                </Form>
                            </Grid>

                            <Grid item sx={12} sm={12} md={8} lg={8}>
                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={12}
                                    lg={12}
                                    style={{
                                        display: 'flex',
                                        direction: 'rtl'
                                    }}
                                >
                                    <h2 className={classes.PaperHead_h2}>
                                        <Button
                                            variant="contained"
                                            color="primary"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            onClick={() => {
                                                history.push(
                                                    '/PayoutBeneficiaryReport'
                                                );
                                            }}
                                        >
                                            All Beneficiary
                                        </Button>
                                    </h2>
                                </Grid>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <ReactTable
                                        data={transactions}
                                        columns={mainColumns}
                                        count={totalCount}
                                        rowsPerPage={TakeCount}
                                        serverSide={true}
                                        getCallback={({
                                            currentPage,
                                            RowsPerPage
                                        }) => {
                                            setCurrentPage(currentPage);
                                            setTakeCount(RowsPerPage);
                                            setSkipCount(
                                                RowsPerPage * currentPage
                                            );
                                            callSelectData(
                                                RowsPerPage,
                                                RowsPerPage * currentPage,
                                                '',
                                                '',
                                                ''
                                            );
                                        }}
                                        getSearchCallback={({ SearchText }) => {
                                            callSelectData(
                                                TakeCount,
                                                currentPage,
                                                SearchText,
                                                '',
                                                ''
                                            );
                                        }}
                                        getSortCallback={({
                                            column,
                                            direction
                                        }) => {
                                            callSelectData(
                                                TakeCount,
                                                currentPage,
                                                SearchText,
                                                column,
                                                direction
                                            );
                                        }}
                                    />
                                </Grid>
                            </Grid>
                        </Grid>
                    );
                }}
            </Formik>

            <Last10RechargeReport ServiceID={serviceCode} refreshRandom={1} />

            {isAlertOpen ? (
                <PayoutTransactionConfirmation
                    isOpen={isAlertOpen}
                    title="Vendorpay Confirmation"
                    message="Are you sure to do this transaction?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Smartpay(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                        callSelectData();
                    }}
                />
            ) : null}

            {isResponseAlertOpen ? (
                <TransactionResponse
                    isOpen={isResponseAlertOpen}
                    isPrint={true}
                    title={status}
                    ServiceID={serviceCode}
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

            {isPrintOpen ? (
                <PayoutPrint
                    isOpen={isPrintOpen}
                    title="Vendorpay Print"
                    request={printData}
                    positiveButtonClick={() => {
                        setIsPrintOpen(false);
                    }}
                    negativeButtonClick={() => {
                        setIsPrintOpen(false);
                    }}
                />
            ) : null}

            {openOTP ? (
                <OTPModal
                    isOpen={openOTP}
                    title="OTP"
                    error={modalErrorMessage}
                    submitCallback={(value) => {
                        VerifyBeneOTP(value);
                    }}
                    resendCallback={(value) => {}}
                    onDismissCallback={(value) => {
                        setOpenOTP(value);
                    }}
                />
            ) : null}

            {isDeleteAlertOpen ? (
                <ConfirmAlert
                    isOpen={isDeleteAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this beneficiary."
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsDeleteAlertOpen(false);
                        Delete(request);
                    }}
                    negativeButtonClick={() => {
                        setIsDeleteAlertOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
