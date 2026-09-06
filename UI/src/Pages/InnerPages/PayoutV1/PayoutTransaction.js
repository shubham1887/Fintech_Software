import React, { useState, useRef, useEffect } from 'react';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Button,
    Checkbox,
    Box,
    TextField,
    FormControlLabel,
    CircularProgress,
    Paper
} from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { LOGOUT } from 'ConstComponents';
import { PayoutTransactionV1 as PAYOUT_TRANSACTION } from 'Components/Api/TransactionAPI';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectForReceipt as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { PayoutPrint } from 'Components/Print/PayoutPrint';
import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    IMPS
} from 'Components/Helper/Messages';
import { PAYOUT } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    BeneAdd as BENE_ADD,
    VerifyOTP,
    DeleteBene,
    SelectBeneficiary,
    PayoutReceipt
} from 'Components/Api/PayoutBeneficiaryAPI';
import AsyncOperatorAutoComplete from 'Components/Inputes/AsyncOperatorAutoComplete';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';

import { Last10PayoutReport } from 'Pages/InnerPages/Report/Last10PayoutReport';
import { PayoutTransactionResponse } from 'Components/Alert/PayoutTransactionResponse';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import { OTPModal } from 'Components/Modal/OTPModal';
import { Select as BANK_SELECT } from 'Components/Api/DMTBankAPI';
import { useHistory } from 'react-router';
import { PayoutTransactionConfirmation } from 'Components/Alert/PayoutTransactionConfirmation';
import usePosition from 'Components/Posistion';
import { styled } from '@mui/material/styles';

import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import { useLocation } from 'react-router-dom';
import { SubscriptionConfirmation } from 'Components/Alert/SubscriptionConfirmation';
import { SelectActiveService as SELECT_ACTIVE } from 'Components/Api/ServiceSubscriptionChargeAPI';
const CustomContainer = HOCLoader(Container);

var PayoutOperators = [];

export const PayoutTransactionV1 = () => {
    const classes = commonStyle();
    const location = useLocation();
    const [responseMessage, setResponseMessage] = useState('');
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [confirmAlertMessage, setConfirmAlertMessage] = useState('');
    const [serviceCode, setserviceCode] = useState(PAYOUT);
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
    const ActivePayoutName = location?.state?.ActivePayoutName;
    const [isAddBeneficiaryShow, setAddBeneficiaryShow] = useState(false);
    const [documentImage, setDocumentImage] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }
    const history = useHistory();
    const [OperatorDefault, setOperatorDefault] = useState({
        OperatorID: 1,
        OperatorName: 'Select Operator'
    });

   const [bankDefault, setBankDefault] = useState({
       Name: 'Select Bank Name',
       DefaultIFSC: ''
   });

   const VisuallyHiddenInput = styled('input')({
       clip: 'rect(0 0 0 0)',
       clipPath: 'inset(50%)',
       height: 1,
       overflow: 'hidden',
       position: 'absolute',
       bottom: 0,
       left: 0,
       whiteSpace: 'nowrap',
       width: 1
   });
   const [FormObj, setFormObj] = useState({
       ServiceID: PAYOUT,
       Name: '',
       Bank: bankDefault,
       AccountNo: '',
       ConfirmAccountNo: '',
       IFSC: '',
       Document: {
           Name: '',
           Size: '',
           Type: '',
           Data: ''
       },
       Operator: OperatorDefault,
       isAddOnly: false,
       IsNameVerified: false,
       Latitude: latitude,
       Longitude: longitude,
       agreeToTerms: false
   });

   const FormValidationSchema = Yup.object({
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
           .required(PLEASE_PROVIDE_VALID + 'account number')
           .min(7, PLEASE_PROVIDE_VALID + 'account number')
           .max(20, PLEASE_PROVIDE_VALID + 'account number'),

       ConfirmAccountNo: Yup.string()
           .required(PLEASE_PROVIDE_VALID + 'confirm account number')
           .min(7, PLEASE_PROVIDE_VALID + 'account number')
           .max(20, PLEASE_PROVIDE_VALID + 'account number')
           .oneOf([Yup.ref('AccountNo'), null], "Account number don't match"),

       agreeToTerms: Yup.boolean().when(['isAddOnly'], {
           is: (isAddOnly) => isAddOnly,

           then: Yup.boolean().oneOf(
               [true],
               'You must accept the terms and conditions'
           ),
           otherwise: Yup.boolean().notRequired()
       }),
       Document: Yup.object().when(['isAddOnly'], {
           is: (isAddOnly) => isAddOnly,

           then: Yup.object().shape({
               Name: Yup.string()
                   .nullable()
                   .required('File Upload is required'),
               Data: Yup.string().required('File data is required')
           }),
           otherwise: Yup.object().notRequired()
       }),
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
           if (row.OperatorName === IMPS) {
               setOperatorDefault(row.OperatorID, row.OperatorName);
           }

           return {
               ID: row.OperatorID,
               Type: row.OperatorName
           };
       });

       PayoutOperators = optionsProviders;

       createColumns();

       setOperator(data);
   };

   const Print = (VendorID) => {
       SelectPrintData(VendorID);
   };
   const handleCopy = (event) => {
       event.preventDefault();
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
           ActivePayoutName: ActivePayoutName,
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

   const submitHandle = async (values) => {
       setResponseMessage('');
       setModalErrorMessage('');
       setIsError(false);
       if (values.isAddOnly === false) {
           setIsAddOnly(false);
           const inputObj = {
               Amount: 1,
               OperatorID: OperatorDefault
           };
           var reqObj = JSON.stringify(inputObj);
           var response = await PayoutReceipt(reqObj);
           setLoading(false);
           if (!response.isAuthorized) LOGOUT();
           var result = JSON.parse(response.data);

           let jsonObj = {
               ServiceID: PAYOUT,
               BeneficiaryName: values.Name,
               BeneName: values.Name,
               BankName: values.Bank.Name,
               AccountNo: values.AccountNo,
               IFSC: values.IFSC,
               IsNameVerified: values.IsNameVerified,
               OperatorID: OperatorDefault,
               BeneficiaryID: 0,
               Amount: 1,
               IsVerify: true,
               PaymentType: values.Operator.OperatorName,
               Latitude: latitude,
               Longitude: longitude,
               ActivePayoutName: ActivePayoutName,
               Charge: result.Charges,
               Gst: result.Gst,
               TotalAmount: result.TotalAmount
           };

           let msg = `Please confirm payout details - \n\nAccountNo : ${
               values.AccountNo
           }\nIFSC : ${values.IFSC}\nAmount : ${'1'}`;

           TransferConfirmation(jsonObj, msg);

           return;
       }
       setIsAddOnly(true);

       var request = {
           Name: values.Name,
           BankName: values.Bank.Name,
           AccountNo: values.AccountNo,
           IFSC: values.IFSC,
           IsNameVerified: values.IsNameVerified,
           ActivePayoutName: ActivePayoutName,
           Document: {
               Name: values.Document.Name,
               Size: values.Document.Size,
               Type: values.Document.Type,
               Data: values.Document.Data
           }
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

       reset('', false, '', '', 0, '', '');
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
       setAddBeneficiaryShow(false);
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
           label: 'A/c Holder Name',
           name: 'Name'
       });
       columns.push({
           label: 'Status',
           name: 'Status',
           customStatus: true
       });

       columns.push({
           label: 'Account Number',
           name: 'AccountNo'
       });
       columns.push({
           label: 'IFSC',
           name: 'IFSC'
       });
       columns.push({
           label: 'Bank Name',
           name: 'BankName'
       });
       columns.push({
           label: 'Verify',
           name: 'IsNameVerified',
           buttonTitle: 'Verify',
           buttonOrLabel: true,
           callback: (value) => {
               VerifyConfirmation(value);
           }
       });
       columns.push({
           label: 'Payment Mode',
           name: 'OperatorID',
           dropdown: true,
           list: PayoutOperators
       });

       columns.push({
           label: 'Amount(₹)',
           name: 'Amount',
           length: 6,
           editableWithNumberOnly: true
       });

       columns.push({
           label: 'Action',
           name: 'BeneficiaryID',
           buttonTitle: 'Transfer',
           statusIndex: 2,
           buttonWithTitle: true,
           callback: (value) => {
               PayoutConfirmation(value);
           }
       });
       columns.push({
           label: 'Delete',
           name: 'BeneficiaryID',
           delete: true,
           callback: (value) => {
               onDeleteClick(value[9], value[1], ActivePayoutName);
           }
       });
       setMainColumns(columns);
   };

    const onDeleteClick = (item, Name, ActivePayoutName) => {
       var RequestObj = {
           BeneficiaryID: item,
           Name: Name,
           ActivePayoutName: ActivePayoutName
       };
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
   const VerifyConfirmation = async (values) => {
       const inputObj = {
           Amount: 1,
           OperatorID: OperatorDefault.OperatorID
       };

       var reqObj = JSON.stringify(inputObj);
       setLoading(true);
       var response = await PayoutReceipt(reqObj);
       setLoading(false);
       if (!response.isAuthorized) LOGOUT();
       var result = JSON.parse(response.data);
       let jsonObj = {
           ServiceID: PAYOUT,
           OperatorID: OperatorDefault.OperatorID,
           AccountNo: values[3],
           BankName: values[5],
           IFSC: values[4],
           Amount: 1,
           BeneficiaryID: values[9],
           BeneficiaryName: values[1],
           IsVerify: false,
           PaymentType: '',
           Latitude: latitude,
           Longitude: longitude,
           ActivePayoutName: ActivePayoutName,
           Charge: result.Charges,
           Gst: result.Gst,
           TotalAmount: result.TotalAmount
       };
       let msg = `Please confirm payout details - \n\nAccountNo : ${values[3]}\nIFSC : ${values[7]}\nAmount : ${values[5]}`;

       TransferConfirmation(jsonObj, msg);
   };
   const TransferValidation = Yup.object({
       OperatorID: Yup.string()
           .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
           .required(PLEASE_SELECT_VALID + 'operator'),

       AccountNo: Yup.string()
           .required(PLEASE_PROVIDE_VALID + 'account number')
           .min(7, PLEASE_PROVIDE_VALID + 'account number')
           .max(20, PLEASE_PROVIDE_VALID + 'account number'),

       Amount: Yup.number()
           .required(PLEASE_PROVIDE_VALID + 'amount')
           .min(1, PLEASE_PROVIDE_VALID + 'amount')
           .max(500000, PLEASE_PROVIDE_VALID + 'amount')
   });

   const PayoutConfirmation = async (values) => {
       const inputObj = {
           Amount: values[8],
           OperatorID: values[7]
       };

       var reqObj = JSON.stringify(inputObj);
       setLoading(true);
       var response = await PayoutReceipt(reqObj);
       setLoading(false);
       if (!response.isAuthorized) LOGOUT();
       var result = JSON.parse(response.data);
       let jsonObj = {
           ServiceID: PAYOUT,
           OperatorID: values[7],
           AccountNo: values[3],
           BankName: values[5],
           IFSC: values[4],
           Amount: values[8],
           BeneficiaryID: values[9],
           BeneficiaryName: values[1],
           IsVerify: false,
           PaymentType: '',
           Latitude: latitude,
           Longitude: longitude,
           ActivePayoutName: ActivePayoutName,
           Charge: result.Charges,
           Gst: result.Gst,
           TotalAmount: result.TotalAmount
       };
       let msg = `Please confirm payout details - \n\nAccountNo : ${values[3]}\nIFSC : ${values[7]}\nAmount : ${values[5]}`;

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
       //setIsTransactionAlertOpen(true);
   };

   const Payout = async () => {
       setResponseMessage('');
       setIsError(false);

       var requestObj = JSON.stringify(confirmAlertRequest);

       setLoading(true);
       var response = await PAYOUT_TRANSACTION(requestObj);
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
           //confirmAlertRequest.Remark
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
           setBankDefault({
               Name: 'Select Bank',
               DefaultIFSC: ''
           });

           setFormObj({
               ServiceID: PAYOUT,
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
                           {isAddBeneficiaryShow && (
                               <Grid item xs={12} sm={12} md={4} lg={4}>
                                   <Form>
                                       <h2 className={classes.PaperHead_h2}>
                                           Link Bank Account
                                       </h2>
                                       <Grid item xs={12}>
                                           <Paper
                                               className={classes.PaperStyle}
                                           >
                                               <Grid container spacing={2}>
                                                   <Grid
                                                       item
                                                       xs={12}
                                                       sm={12}
                                                       md={12}
                                                       lg={12}
                                                   ></Grid>

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
                                                               label:
                                                                   'Bank Name'
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
                                                       <MaterialTextField
                                                           font_size={
                                                               consumerNumberFontSize
                                                           }
                                                           label="Account Number"
                                                           name="AccountNo"
                                                           autoComplete="new-password"
                                                           type="text"
                                                           length={20}
                                                           onCopy={handleCopy}
                                                           //onChange={HandleSecondTextBoxVisible}
                                                           placeholder="Enter Account Number"
                                                           typed_length={1}
                                                           readOnly={false}
                                                       />
                                                   </Grid>

                                                   <Grid
                                                       item
                                                       xs={12}
                                                       sm={12}
                                                       md={9}
                                                       lg={9}
                                                   >
                                                       <MaterialTextField
                                                           font_size={
                                                               consumerNumberFontSize
                                                           }
                                                           label="Confirm Account  Number"
                                                           name="ConfirmAccountNo"
                                                           type="text"
                                                           length={20}
                                                           onCopy={handleCopy}
                                                           placeholder="Enter Confirm Account Number"
                                                           typed_length={1}
                                                           // readOnly={isAllowSecondTextBox}
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
                                                                   handleSubmit(
                                                                       e
                                                                   );
                                                               }}
                                                               disabled={
                                                                   loading
                                                               }
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
                                                           label="A/c Holder Name"
                                                           name="Name"
                                                           type="text"
                                                           placeholder="Enter A/c Holder Name"
                                                           readOnly={isReadOnly}
                                                       />
                                                   </Grid>

                                                   <Grid
                                                       item
                                                       xs={12}
                                                       sm={12}
                                                       md={8}
                                                       lg={8}
                                                   >
                                                       Upload the bank account
                                                       proof <br></br>
                                                       (Passbook, Cheque image
                                                       or A/c statement)
                                                   </Grid>

                                                   <Grid
                                                       item
                                                       xs={12}
                                                       sm={12}
                                                       md={4}
                                                       lg={4}
                                                   >
                                                       <div
                                                           className={
                                                               classes.SubmitWrapper
                                                           }
                                                       >
                                                           <Button
                                                               component="label"
                                                               role={undefined}
                                                               variant="contained"
                                                               tabIndex={-1}
                                                               startIcon={
                                                                   <CloudUploadIcon />
                                                               }
                                                           >
                                                               Upload file
                                                               <VisuallyHiddenInput
                                                                   type="file"
                                                                   onChange={(
                                                                       e
                                                                   ) => {
                                                                       var file =
                                                                           e
                                                                               .target
                                                                               .files[0];

                                                                       setDocumentImage(
                                                                           URL.createObjectURL(
                                                                               file
                                                                           )
                                                                       );

                                                                       var reader = new FileReader();
                                                                       setFieldValue(
                                                                           'Document.Name',
                                                                           file.name
                                                                       );
                                                                       setFieldValue(
                                                                           'Document.Size',
                                                                           file.size
                                                                       );
                                                                       setFieldValue(
                                                                           'Document.Type',
                                                                           file.type
                                                                       );
                                                                       reader.onload = function (
                                                                           item
                                                                       ) {
                                                                           setFieldValue(
                                                                               'Document.Data',
                                                                               item.target.result
                                                                                   .replace(
                                                                                       'data:',
                                                                                       ''
                                                                                   )
                                                                                   .replace(
                                                                                       /^.+,/,
                                                                                       ''
                                                                                   )
                                                                           );
                                                                       };

                                                                       reader.readAsDataURL(
                                                                           file
                                                                       );
                                                                   }}
                                                               />
                                                           </Button>
                                                           <b
                                                               style={{
                                                                   textAlign:
                                                                       'center',
                                                                   color: 'red'
                                                               }}
                                                           >
                                                               {
                                                                   values
                                                                       .Document
                                                                       ?.Name
                                                               }
                                                           </b>

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
                                               </Grid>
                                               <span style={{ color: 'blue' }}>
                                                   Document image should clearly
                                                   show a/c no, IFSC and A/c
                                                   holder's name
                                               </span>
                                               <ErrorMessage
                                                   style={{ color: 'red' }}
                                                   name="Document.Name"
                                                   component="div"
                                                   className="field-error-red"
                                               />
                                               <br></br>
                                               <br></br>

                                               <Grid
                                                   item
                                                   xs={12}
                                                   sm={12}
                                                   md={12}
                                                   lg={12}
                                               >
                                                   <FormControlLabel
                                                       control={
                                                           <Field
                                                               style={{
                                                                   marginTop:
                                                                       '-21px'
                                                               }}
                                                               as={Checkbox}
                                                               name="agreeToTerms"
                                                               color="primary"
                                                           />
                                                       }
                                                       label="I hereby confirm that the information provided is true and correct, and that the account belongs to me."
                                                   />
                                                   <ErrorMessage
                                                       style={{ color: 'red' }}
                                                       name="agreeToTerms"
                                                       component="div"
                                                       className="field-error-red"
                                                   />
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
                           )}

                           <Grid
                               item
                               xs={12}
                               sm={12}
                               md={isAddBeneficiaryShow ? 8 : 12}
                               lg={isAddBeneficiaryShow ? 8 : 12}
                           >
                               <Grid
                                   item
                                   xs={12}
                                   sm={12}
                                   md={12}
                                   lg={12}
                                   spacing={2}
                                   style={{
                                       display: 'flex',
                                       float: 'right'
                                   }}
                               >
                                   <h2 className={classes.PaperHead_h2}>
                                       <Grid
                                           container
                                           justifyContent="center"
                                           spacing={2}
                                       >
                                           <Grid item>
                                               <Button
                                                   variant="contained"
                                                   color="primary"
                                                   className={
                                                       classes.CustomPrimaryButtonStyle
                                                   }
                                                   onClick={(e) => {
                                                       setAddBeneficiaryShow(
                                                           true
                                                       );
                                                   }}
                                               >
                                                   Add Account
                                               </Button>
                                           </Grid>
                                           <Grid item>
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
                                                   Linked Bank Accounts
                                               </Button>
                                           </Grid>
                                       </Grid>
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

           <Last10PayoutReport ServiceID={serviceCode} refreshRandom={1} />

           {isAlertOpen ? (
               <PayoutTransactionConfirmation
                   isOpen={isAlertOpen}
                   title="Confirmation"
                   message={confirmAlertMessage}
                   request={confirmAlertRequest}
                   positiveButtonClick={(request) => {
                       setIsAlertOpen(false);
                       Payout(request);
                   }}
                   negativeButtonClick={() => {
                       setIsAlertOpen(false);
                       callSelectData();
                   }}
               />
           ) : null}

           {isResponseAlertOpen ? (
               <PayoutTransactionResponse
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
                       callSelectData();
                       setIsResponseAlertOpen(false);
                   }}
               />
           ) : null}
           {isPrintOpen ? (
               <PayoutPrint
                   isOpen={isPrintOpen}
                   title="Payout Print"
                   request={printData}
                   positiveButtonClick={() => {
                       setIsPrintOpen(false);
                   }}
                   negativeButtonClick={() => {
                       callSelectData();
                       setIsPrintOpen(false);
                   }}
               />
           ) : null}

           {openOTP ? (
               <OTPModal
                   isOpen={openOTP}
                   title="Enter OTP to link Bank Account"
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
                   message={`Are you sure to delete this A/C Holder :- ${confirmAlertRequest.Name}`}
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
