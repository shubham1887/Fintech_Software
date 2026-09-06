import React, { useState, useEffect, useRef, useContext } from 'react';
import { Formik, Form, Field } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { LOGOUT } from 'ConstComponents';
import {
    ViewBeneficiary as BENE_SELECT,
    AddBeneficiary as BENE_ADD,
    MoneyTransfer as DMT_TRANSFER
} from 'Components/Api/YesBankDMTAPI';
import { Select as BANK_SELECT } from 'Components/Api/DMTBankAPI';

import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    Container,
    Grid,
    Paper,
    Button,
    Hidden,
    Tab,
    Tabs
} from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';
import { KeyValueModal } from 'Components/Modal/KeyValueModal';
import { CheckBranch as CHECK_BRANCH } from 'Components/Api/BeneficiaryAPI';
import AutoSubmitIFSCForBeneAdd from 'Components/Helper/AutoSubmitIFSCForBeneAdd';
import SearchIcon from 'Components/SvgButtons/SearchIcon';
import IFSCThroughBankAddress from 'Components/IFSCThroughBankAddress';
import { VerifyPendingModel } from 'Components/Modal/VerifyPendingModel';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID,
    MONEY_TRANSFER_CODE,
    MONEY_TRANSFER_OPERATOR_TRANSFER_CODE,
    MONEY_TRANSFER_OPERATOR_VERIFY_CODE
} from 'Components/Helper/Messages';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';
import { useLocation } from 'react-router-dom';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { GetDMTTransactionStatus as TRANSACTION_STATUS } from 'Components/Api/TransactionAPI';
import { CustomModal } from 'Components/Modal/CustomModal';
import BeneficiaryVerify from './BeneficiaryVerify';
import { makeStyles } from '@material-ui/core/styles';
import { DMTV1TransactionConfirmation } from 'Components/Alert/DMTV1TransactionConfirmation';
import { TransactionResponse } from 'Components/Alert/TransactionResponse';
import { SelectDMTForReceiptForWeb as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { DMTTransactionPrint } from 'Components/Print/DMTPrint';
import { BalanceContext } from 'Components/BalanceContext';
import { DMTTransactionConfirmation } from 'Components/Alert/DMTTransactionConfirmation';
import { PayoutReceipt } from 'Components/Api/PayoutBeneficiaryAPI';
const CustomContainer = HOCLoader(Container);

var receivedID = 0;
const BeneficiaryAdd = () => {
    const commonTabStyle = makeStyles({
        selectedTab: {
            backgroundColor: '#ff0000',
            color: 'white'
        }
    });
    const { DashboardBalanceUpdate } = useContext(BalanceContext);
    const formRef = useRef();
    const classess = commonTabStyle();
    const classes = commonStyle();
    const history = useHistory();
    const location = useLocation();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [bankList, setBankList] = useState([]);
    const [severity, setSeverity] = useState('success');
    const [bankDefault, setBankDefault] = useState({
        Name: 'Select Bank',
        BankID: 0,
        DefaultIFSC: ''
    });
    const [isBeneVerifyOpen, setIsBeneVerifyOpen] = useState(false);
    const [isOpen, setIsOpen] = useState(false);
    const [isOpenModel, setIsOpenModel] = useState(false);
    const [BeneVerifyDetails, setBeneVerifyDetails] = useState([]);
    const [verifyStatus, setVerifyStatus] = useState('');
    const [isAddOnly, setIsAddOnly] = useState(false);
    const [isReadOnly, setReadOnly] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [confirmRequest, setConfirmRequest] = useState({});
    const [list, setList] = useState([]);
    const Mobile = location?.state?.Mobile;
    const SenderID = location?.state?.SenderID;
    const SenderName = location?.state?.SenderName;
    const ActiveDMTName = location?.state?.DMTName;
    const [Branch, setBranch] = useState('');
    const [IFSCAddress, setIFSCAddress] = useState('');
    const [isVerifyAlertOpen, setIsVerifyAlertOpen] = useState(false);
    const [openOTP, setOpenOTP] = useState(false);
    const [TransactionMasterID, setTransactionMasterID] = useState(0);
    const [
        confirmDefaultAlertRequest,
        setConfirmDefaultAlertRequest
    ] = useState({});
    const [tabIndex, setTabIndex] = useState('IFSC');
    const [name, setName] = useState('');
    const [accountno, setAccountNo] = useState('');
    const [ifsc, setIFSC] = useState('');
    const [senderID, setSenderID] = useState('');
    const [beneficiaryID, setBeneficiaryID] = useState('');
    const [mobile, setMobile] = useState('');
    const [isTransferAlertOpen, setIsTransferAlertOpen] = useState(false);
    const [
        isTransferConfirmationAlert,
        setIsTransferConfirmationAlert
    ] = useState(false);
    const [
        transferConfirmationRequest,
        setTransferConfirmationRequest
    ] = useState({});
    const [transferRequest, setTransferRequest] = useState({});
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [ResponseAlert, setResponseAlert] = useState({});
    const [serviceCode, setserviceCode] = useState(MONEY_TRANSFER_CODE);
    const [printData, setPrintData] = useState(null);
    const [isPrint, setIsPrint] = useState(false);
    const [TransactionID, setTransactionID] = useState(0);
    const [charge, setCharge] = useState(0);

    const [formObject, setFormObject] = useState({
        Mobile: Mobile,
        Name: '',
        SenderName: SenderName,
        Bank: bankDefault,
        AccountNo: '',
        ConfirmAccountNo: '',
        IFSC: '',
        SenderID: SenderID,
        isAddOnly: false,
        ActiveDMTName: ActiveDMTName
    });

    useEffect(() => {
        selectBank();
        selectBene();
        createColumns();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'BeneficiaryID',
            srno: true
        });
        columns.push({
            label: 'Beneficiary Name',
            name: 'Name'
        });
        columns.push({
            label: 'BankName',
            name: 'BankName',
            hide: true
        });
        columns.push({
            label: 'Account Number',
            name: 'AccountNo'
        });
        columns.push({
            label: 'IFSC',
            name: 'IFSC'
        });
        setMainColumns(columns);
    };

    const selectBene = async () => {
        setLoading(true);
        setIsError(false);

        if (Mobile === undefined) return;

        if (Mobile === '') return;

        var beneReqObj = JSON.stringify({
            Mobile: Mobile,
            DMTName: ActiveDMTName
        });

        var response = await BENE_SELECT(beneReqObj);

        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setIsError(true);
            setResponseMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);
        var filteredData = data.filter((bene) => bene.Status !== '0');
        setList(filteredData);
    };

    const selectBank = async () => {
        setLoading(true);
        var ReqObj = JSON.stringify({
            DMTName: ActiveDMTName
        });

        const response = await BANK_SELECT(ReqObj);

        setLoading(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        let objWithSelectAll = [bankDefault, ...data];

        setBankList(objWithSelectAll);
    };

    const Verify = async (values) => {
        setConfirmRequest({});
        setResponseMessage('');

        let jsonObj = {
            Mobile: values.Mobile,
            SenderMobile: values.Mobile,
            Name: values.Name,
            SenderName: SenderName,
            BankName: values.Bank.Name,
            BankID: values.Bank.BankID,
            AccountNo: values.AccountNo,
            IFSC: values.IFSC,
            BeneficiaryID: values.Mobile,
            SenderID: values.Mobile,
            Amount: 1,
            PaymentType: 'IMPS',
            RechargePin: '',
            Through: 'WEB',
            IsVerify: true,
            DMTName: ActiveDMTName
        };

        setIsError(false);
        setLoading(true);

        var apirequest = JSON.stringify(jsonObj);

        var response = await DMT_TRANSFER(apirequest);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);

        if (!response.status) {
            var dataError = JSON.parse(response.data);
            setSeverity('error');
            setResponseMessage(dataError.Message);

            setIsError(true);
            return;
        }

        setSeverity('success');
        var data = JSON.parse(response.data);
        setResponseMessage(data.Message);
        setIsError(true);
        setVerifyStatus(data.Status);

        setFormObject({
            ...formObject,
            Mobile: formRef.current.values.Mobile,
            Name: data.BeneficiaryName,
            IFSC: formRef.current.values.IFSC,
            AccountNo: formRef.current.values.AccountNo,
            ConfirmAccountNo: formRef.current.values.ConfirmAccountNo
        });

        if (data.Status === 'PROCESS' || data.Status === 'FAILURE') {
            setTransactionID(data.VendorID);
            setIsOpen(true);
            setConfirmRequest(jsonObj);
        }

        if (data.Status === 'SUCCESS') {
            createKeyValuePair(data);
            setIsBeneVerifyOpen(true);
        }
    };

    const ReVerify = async (jsonObj) => {
        setResponseMessage('');
        setIsError(false);
        setLoading(true);
        var apirequest = JSON.stringify(jsonObj);
        var response = await DMT_TRANSFER(apirequest);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);

        if (!response.status) {
            var dataError = JSON.parse(response.data);
            setSeverity('error');
            setResponseMessage(dataError.Message);

            setIsError(true);
            return;
        }

        setSeverity('success');
        var data = JSON.parse(response.data);

        setResponseMessage(data.Message);
        setIsError(true);
        setVerifyStatus(data.Status);

        setFormObject({
            Mobile: data.Mobile,
            Name: data.Name
        });

        if (data.Status === 'SUCCESS') {
            createKeyValuePair(data);
            setIsBeneVerifyOpen(true);
        }
    };

    const StatusCheck = async (TransactionMasterID) => {
        var inputObj = {
            ID: TransactionMasterID
        };
        var requestObj = JSON.stringify(inputObj);
        setIsError(false);
        setLoading(true);

        var response = await TRANSACTION_STATUS(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        receivedID = 0;
        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }

        var data = JSON.parse(response.data);

        setResponseMessage(data.Message);

        if (data.Description === 'OTP sent to customer mobile') {
            setOpenOTP(true);
            setTransactionMasterID(TransactionMasterID);
        }

        if (data.Status !== 'Ok') {
            setIsError(true);
            setSeverity('error');
            return;
        }

        if (data.Status === 'SUCCESS') {
            createKeyValuePair(data);
            setIsBeneVerifyOpen(true);
        }
    };

    const submitHandle = async (values) => {
        setResponseMessage('');

        if (values.isAddOnly === true) {
            setIsAddOnly(true);
            core_bene_add(values, values.Name);
            return;
        }
        const inputObj = {
            Amount: 1,
            OperatorID: MONEY_TRANSFER_OPERATOR_VERIFY_CODE
        };
        var reqObj = JSON.stringify(inputObj);
        var response = await PayoutReceipt(reqObj);
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();
        var result = JSON.parse(response.data);
        setCharge(result.TotalAmount);
        setIsVerifyAlertOpen(true);
        setConfirmDefaultAlertRequest(values);
    };

    const createKeyValuePair = (data) => {
        if (data.Status === 'FAILURE') {
            let rows = [
                VerifyData('Name', data.BeneficiaryName),
                VerifyData('Bank', data.BankName),
                VerifyData('IFSC', data.IFSC),
                VerifyData('Account', data.ConsumerNumber),
                VerifyData('Message', data.Description)
            ];
            setBeneVerifyDetails(rows);
        } else {
            let rows = [
                VerifyData('Name', data.BeneficiaryName),
                VerifyData('Bank', data.BankName),
                VerifyData('IFSC', data.IFSC),
                VerifyData('Account', data.ConsumerNumber)
            ];
            setBeneVerifyDetails(rows);
        }
    };

    function VerifyData(Key, Value) {
        return { Key, Value };
    }

    const GetBranch = async (values, setFieldValue) => {
        var request = {
            IFSC: values.IFSC
        };
        var requestObj = JSON.stringify(request);

        setLoading(true);

        setIsError(false);

        var response = await CHECK_BRANCH(requestObj);

        if (!response.isAuthorized) LOGOUT();

        setLoading(false);

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);

        setBankDefault({
            DefaultIFSC: data.IFSC,
            Name: data.BankName
        });

        setFieldValue('Bank.Name', data.BankName);

        setBranch(data.Branch);
        setIFSCAddress(data.Address);
    };

    const core_bene_add = async (values, beneName) => {
        setResponseMessage('');
        var request = {
            Name: beneName,
            BankName: values.Bank.Name,
            BankID: values.Bank.BankID,
            AccountNo: values.AccountNo,
            IFSC: values.IFSC,
            SenderID: values.Mobile,
            SenderMobile: values.Mobile,
            DMTName: ActiveDMTName
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
        setIsError(true);

        var param1 = {
            Name: beneName,
            AccountNo: values.AccountNo,
            IFSC: values.IFSC,
            SenderID: values.Mobile,
            BeneficiaryID: data.BeneficiaryID,
            Mobile: values.Mobile,
            ActiveDMTName: ActiveDMTName
        };
        if (data.Message === 'OTP generated successfully') {
            BeneVerifyPopup(param1);
            return;
        }
        var param = {
            Mobile: values.Mobile,
            DMTName: ActiveDMTName
        };

        history.push('/YBDMT_SenderSearch', param);
    };

    const BeneVerifyPopup = (param1) => {
        setName(param1.Name);
        setAccountNo(param1.AccountNo);
        setIFSC(param1.IFSC);
        setSenderID(param1.SenderID);
        setBeneficiaryID(param1.BeneficiaryID);
        setMobile(param1.Mobile);
        setIsOpenModel(true);
    };

    const handleCopy = (event) => {
        event.preventDefault();
    };

    const FormValidationSchema = Yup.object({
        Bank: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'bank')
            .required(PLEASE_SELECT_VALID + 'bank')
            .shape({
                Name: Yup.string()
                    .nullable(PLEASE_SELECT_VALID + 'bank')
                    .required(PLEASE_SELECT_VALID + 'bank')
            }),

        Mobile: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'mobile')
            .length(10, PLEASE_PROVIDE_VALID + 'mobile'),
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

    const Transfer = async (request) => {
        let jsonObj = {
            Mobile: Mobile,
            Name: request.BeneName,
            SenderName: SenderName,
            BankName: request.BankName,
            AccountNo: request.AccountNo,
            IFSC: request.IFSC,
            BeneficiaryID: request.BeneficiaryID,
            SenderID: request.BeneMobile,
            Amount: request.Amount,
            PaymentType: request.PaymentType,
            RechargePin: request.RechargePin,
            Remark: request.Remark,
            DMTName: ActiveDMTName
        };
        setIsError(false);
        setLoading(true);

        var apiRequest = JSON.stringify(jsonObj);
        var response = await DMT_TRANSFER(apiRequest);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            var dataError = JSON.parse(response.data);
            setSeverity('error');
            setResponseMessage(dataError.Message);

            setIsError(true);

            return;
        }

        var data = JSON.parse(response.data);
        setIsTransferAlertOpen(false);
        setResponseAlert(data);
        setIsResponseAlertOpen(true);
        // select();
    };

    const Print = ({ ID }) => {
        SelectPrintData(ID);
    };

    const SelectPrintData = async (TransactionMasterID) => {
        var req = {
            ID: TransactionMasterID
        };
        var request = JSON.stringify(req);

        const resp = await TRANSACTION_RECEIPT(request);

        if (!resp.isAuthorized) LOGOUT();

        if (!resp.status) return;

        var data = JSON.parse(resp.data);

        setPrintData(data);

        setIsPrint(true);
    };

    const sendCallback = (data) => {
        setIsResponseAlertOpen(false);

        setSeverity('success');
        if (data.Status === 'FAILURE') {
            setSeverity('error');
        }
        setResponseMessage(data.Message);
        setIsError(true);
    };

    const ReciptData = async (values) => {
        var req = {
            Amount: values.Amount,
            OperatorID: MONEY_TRANSFER_OPERATOR_TRANSFER_CODE
        };
        var request = JSON.stringify(req);

        const resp = await PayoutReceipt(request);

        if (!resp.isAuthorized) LOGOUT();

        if (!resp.status) return;

        var data = JSON.parse(resp.data);

        setTransferRequest({
            ...transferRequest,
            Charge: data.Charges,
            Gst: data.Gst,
            TotalAmount: data.TotalAmount
        });
        setIsTransferConfirmationAlert(true);
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
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={submitHandle}
                resetForm={formObject}
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    const handleChange = (event, newValue) => {
                        if (typeof setFieldValue === 'function') {
                            setTabIndex(newValue);

                            if (newValue === 'Bank') {
                                setFieldValue('Bank', '');
                                setFieldValue(
                                    'IFSC',
                                    bankDefault.DefaultIFSC || ''
                                );
                            } else if (newValue === 'IFSC') {
                                setFieldValue('Bank', {});
                                setFieldValue('IFSC', '');
                            }
                        }
                    };
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
                                    <AutoSubmitIFSCForBeneAdd
                                        callback={() => {
                                            GetBranch(values, setFieldValue);
                                        }}
                                    />
                                    <h2 className={classes.PaperHead_h2}>
                                        Add Beneficiary
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
                                                    <MaterialTextField
                                                        label="Beneficiary Mobile Number *"
                                                        name="Mobile"
                                                        type="text"
                                                        length={10}
                                                        placeholder="Mobile Number"
                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <Box
                                                        style={{
                                                            justifyContent:
                                                                'center',
                                                            display: 'flex'
                                                        }}
                                                    >
                                                        <Tabs
                                                            value={tabIndex}
                                                            onChange={
                                                                handleChange
                                                            }
                                                            variant="scrollable"
                                                            scrollButtons="auto"
                                                            aria-label="scrollable auto tabs example"
                                                            style={{
                                                                borderRadius:
                                                                    '100px',
                                                                overflow:
                                                                    'hidden'
                                                            }}
                                                        >
                                                            <Tab
                                                                label="IFSC"
                                                                value="IFSC"
                                                                className={
                                                                    tabIndex ===
                                                                    'IFSC'
                                                                        ? classess.selectedTab
                                                                        : ''
                                                                }
                                                            />
                                                            <Tab
                                                                label="BANK"
                                                                value="Bank"
                                                                className={
                                                                    tabIndex ===
                                                                    'Bank'
                                                                        ? classess.selectedTab
                                                                        : ''
                                                                }
                                                            />
                                                        </Tabs>
                                                    </Box>
                                                </Grid>
                                                {tabIndex === 'Bank' && (
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
                                                            SubFieldName="Name"
                                                            FieldName="Bank"
                                                            name="Bank"
                                                            onCustomCallback={(
                                                                name,
                                                                value
                                                            ) => {
                                                                setFieldValue(
                                                                    'BankID',
                                                                    value.BankID
                                                                );
                                                                setFieldValue(
                                                                    'IFSC',
                                                                    value.DefaultIFSC
                                                                );
                                                                setFieldValue(
                                                                    'Bank.Name',
                                                                    value.Name
                                                                );
                                                                setBankDefault({
                                                                    BankID:
                                                                        value.BankID,
                                                                    DefaultIFSC:
                                                                        value.DefaultIFSC,
                                                                    Name:
                                                                        value.Name
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
                                                                label:
                                                                    'Select Bank *'
                                                            }}
                                                        />
                                                    </Grid>
                                                )}

                                                <Grid
                                                    item
                                                    container
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                    spacing={2}
                                                    alignItems="center"
                                                >
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={9}
                                                        lg={9}
                                                    >
                                                        <MaterialTextField
                                                            label="IFSC *"
                                                            name="IFSC"
                                                            type="text"
                                                            length={11}
                                                            placeholder="IFSC"
                                                            readOnly={
                                                                isReadOnly
                                                            }
                                                        />
                                                    </Grid>
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={3}
                                                        lg={3}
                                                    >
                                                        <Button
                                                            variant="contained"
                                                            color="primary"
                                                            title="Check Branch"
                                                            endIcon={
                                                                <SearchIcon />
                                                            }
                                                            onClick={() => {
                                                                GetBranch(
                                                                    values,
                                                                    setFieldValue
                                                                );
                                                            }}
                                                            disabled={loading}
                                                        ></Button>
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
                                                        md={12}
                                                        lg={12}
                                                    >
                                                        <IFSCThroughBankAddress
                                                            Branch={Branch}
                                                            IFSCAddress={
                                                                IFSCAddress
                                                            }
                                                        />
                                                    </Grid>
                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <MaterialNumberField
                                                        label="Account Number *"
                                                        name="AccountNo"
                                                        type="password"
                                                        length={20}
                                                        onCopy={handleCopy}
                                                        placeholder="Account Number."
                                                        readOnly={isReadOnly}
                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={10}
                                                    lg={10}
                                                >
                                                    <MaterialNumberField
                                                        label="Confirm Account Number *"
                                                        name="ConfirmAccountNo"
                                                        type="text"
                                                        length={20}
                                                        onCopy={handleCopy}
                                                        placeholder="Confirm Account Number."
                                                        readOnly={isReadOnly}
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
                                                        title="Verify"
                                                        disabled={loading}
                                                        onClick={(e) => {
                                                            setFieldValue(
                                                                'isAddOnly',
                                                                false
                                                            );
                                                            handleSubmit(e);
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
                                                        label="Beneficiary Name"
                                                        name="Name"
                                                        type="text"
                                                        placeholder="Beneficiary Name"
                                                        readOnly={isReadOnly}
                                                    />
                                                </Grid>
                                                <Hidden xsUp>
                                                    <MaterialTextField
                                                        label="SenderID"
                                                        name="SenderID"
                                                        type="text"
                                                        readOnly={true}
                                                        placeholder="SenderID"
                                                    />
                                                </Hidden>
                                            </Grid>
                                        </Paper>
                                    </Grid>

                                    <Grid className={classes.footerGrid}>
                                        <Box className={classes.flexReverse}>
                                            <div
                                                className={
                                                    classes.SubmitWrapper
                                                }
                                            >
                                                <Button
                                                    type="submit"
                                                    title="Add"
                                                    className={
                                                        classes.CustomDMTButtonStyle
                                                    }
                                                    disabled={loading}
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'isAddOnly',
                                                            true
                                                        );
                                                        handleSubmit(e);
                                                    }}
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
                                                    classes.CustomDMTDangerButtonStyle
                                                }
                                            >
                                                Reset
                                            </Button>
                                        </Box>
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
                                    Beneficiary List
                                </h2>
                                <ReactTable data={list} columns={mainColumns} />
                            </Grid>
                        </Grid>
                    );
                }}
            </Formik>
            ;
            {isBeneVerifyOpen && (
                <KeyValueModal
                    isOpen={isBeneVerifyOpen}
                    request={BeneVerifyDetails}
                    title="VERIFY"
                    status={verifyStatus}
                    OKButtonClick={() => {
                        setIsBeneVerifyOpen(false);
                    }}
                />
            )}
            {isOpen && (
                <VerifyPendingModel
                    isOpen={isOpen}
                    title="VERIFY"
                    status={verifyStatus}
                    CheckStatusClick={() => {
                        setIsOpen(false);
                        StatusCheck(TransactionID);
                    }}
                    RetryClick={() => {
                        setIsOpen(false);
                        ReVerify(confirmRequest);
                    }}
                    ProceedWithoutValidationClick={() => {
                        setIsOpen(false);
                    }}
                />
            )}
            {isVerifyAlertOpen && (
                <ConfirmAlert
                    isOpen={isVerifyAlertOpen}
                    title="Verification Confirmation"
                    request={confirmDefaultAlertRequest}
                    message={`Do you want to verify your account?
                    a charge of ₹ ${charge} (inclusive of taxes) will be debited`}
                    positiveButtonClick={(request) => {
                        setIsVerifyAlertOpen(false);
                        Verify(request);
                    }}
                    negativeButtonClick={() => {
                        setIsVerifyAlertOpen(false);
                    }}
                />
            )}
            {isOpenModel && (
                <CustomModal
                    isOpen={isOpenModel}
                    title="Confirm with OTP"
                    width="40%"
                    onDismissCallback={(value) => {
                        setIsOpenModel(value);
                    }}
                >
                    <BeneficiaryVerify
                        BeniName={name}
                        AccountNo={accountno}
                        IFSC={ifsc}
                        SenderID={senderID}
                        BeneficiaryID={beneficiaryID}
                        Mobile={mobile}
                        ActiveDMTName={ActiveDMTName}
                        onDismissCallback={() => {
                            setIsOpenModel(false);
                        }}
                        onSuccessCallback={(value) => {
                            setIsOpenModel(false);
                            setIsTransferAlertOpen(true);
                            setTransferRequest(value);
                        }}
                    />
                </CustomModal>
            )}
            {isTransferAlertOpen && (
                <DMTV1TransactionConfirmation
                    isOpen={isTransferAlertOpen}
                    ActiveDMTName={ActiveDMTName}
                    title="Domestic Money Transfer"
                    request={transferRequest}
                    positiveButtonClick={(request) => {
                        setIsTransferAlertOpen(false);
                        if (
                            parseInt(request.ServiceID) ===
                                MONEY_TRANSFER_CODE &&
                            parseInt(request.OperatorID) ===
                                MONEY_TRANSFER_OPERATOR_TRANSFER_CODE
                        ) {
                            ReciptData(request);
                            setTransferConfirmationRequest(request);
                        }
                    }}
                    negativeButtonClick={() => {
                        setIsTransferAlertOpen(false);
                        history.push('/YBDMT_SenderSearch');
                    }}
                />
            )}
            {isTransferConfirmationAlert && (
                <DMTTransactionConfirmation
                    isOpen={isTransferConfirmationAlert}
                    ActiveDMTName={ActiveDMTName}
                    title="Transfer Confirmation"
                    request={transferRequest}
                    positiveButtonClick={(request) => {
                        setIsTransferConfirmationAlert(false);
                        Transfer(request);
                    }}
                    negativeButtonClick={() => {
                        setIsTransferConfirmationAlert(false);
                        history.push('/YBDMT_SenderSearch');
                    }}
                />
            )}
            {isResponseAlertOpen && (
                <TransactionResponse
                    isOpen={isResponseAlertOpen}
                    isPrint={true}
                    title={ResponseAlert.Status}
                    ServiceID={serviceCode}
                    request={ResponseAlert}
                    OkButtonClick={(request) => {
                        sendCallback(ResponseAlert);
                    }}
                    PrintButtonClick={() => {
                        setIsResponseAlertOpen(false);
                        Print({
                            ...ResponseAlert,
                            ID: ResponseAlert.VendorID
                        });
                    }}
                />
            )}
            {isPrint && (
                <DMTTransactionPrint
                    isOpen={isPrint}
                    title="Transaction Print"
                    request={printData}
                    positiveButtonClick={() => {
                        setIsPrint(false);
                        DashboardBalanceUpdate();
                    }}
                    negativeButtonClick={() => {
                        setIsPrint(false);
                    }}
                />
            )}
        </CustomContainer>
    );
};
export default BeneficiaryAdd;
