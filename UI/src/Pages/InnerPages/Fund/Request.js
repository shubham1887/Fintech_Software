import React, { useEffect, useState, useRef } from 'react';
import { useHistory } from 'react-router';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { AmountInWordLabel } from 'Components/Inputes/FormikMaterialInput';
import {
    Container,
    MenuItem,
    Grid,
    Paper,
    Button,
    Hidden,
    CircularProgress,
    TextField
} from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import { SelectParentWithAdmin as SELECT_PARENT_WITH_ADMIN } from 'Components/Api/UsersApi';
import { SelectByParentID as SELECT_BY_PARENT_ID } from 'Components/Api/UserBankDetailApi';
import { SelectByReceiverID as SELECT_PAYMENT_MODE_BYID } from 'Components/Api/PaymentModesAPI';
import RechargeIcon from 'Components/SvgButtons/RechargeIcon';
import { SelectForFundTransfer as WALLET_SELECT } from 'Components/Api/WalletApi';
import { RequestInsert } from 'Components/Api/FundApi';
import { SelectLast10Request as TRANSACTION_LAST10 } from 'Components/Api/FundApi';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import ReactTable from 'Components/ReactTable/ReactTable';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import HOCLoader from 'Components/Loader/HOCLoader';

import Divider from '@material-ui/core/Divider';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import ResetIcon from 'Components/SvgButtons/ResetIcon';

const CustomContainer = HOCLoader(Container);

export const Request = () => {
    const classes = commonStyle();
    const history = useHistory();
    const formRef = useRef();
    const inputRef = useRef(null);
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [wallets, setWallets] = useState([]);
    const [requestToList, setRequestToList] = useState([]);
    const [paymentModes, setpaymentModes] = useState([]);
    const [RequestToID, setRequestToID] = useState(0);
    const [parentBankDetails, setParentBankDetails] = useState([]);
    const [shouldDisplayBankName, setShouldDisplayBankName] = useState(false);
    const [DisplayImage, setDisplayImage] = useState(false);
    const [ReceiptImage, setReceiptImage] = useState('');
    const [currentUserID] = useState(parseInt(localStorage.getItem('UserID')));
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [DropDownOpen, setDropDownOpen] = useState(true);
    const [IsShowUTRNumber, setIsShowUTRNumber] = useState(true);

    const [formObj, setFormObj] = useState({
        WalletID: 0,
        Amount: '',
        BankName: 0,
        Remark: '',
        UTRNumber: '',
        TransactionDate: new Date(),
        PayMode: '0',
        ParentID: 0,
        ParentInfo: '',
        InWords: '',
        RequestTo: 0,
        Document: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        }
    });

    const resetImage = () => {
        setDisplayImage(false);
        setReceiptImage('');
        inputRef.current.value = '';
    };

    const validationSchema = Yup.object({
        WalletID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'wallet')
            .required(PLEASE_SELECT_VALID + 'wallet'),

        Amount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'amount')
            .required(PLEASE_PROVIDE_VALID + 'amount'),

        PayMode: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'mode')
            .required(PLEASE_SELECT_VALID + 'mode'),

        Remark: Yup.string().required(PLEASE_PROVIDE_VALID + 'remark')
    });
    const bankNameValidationSchema = Yup.object({
        BankName: shouldDisplayBankName ? Yup.string() .notOneOf(['0'], PLEASE_SELECT_VALID + 'Bank').required('BankName is required') : Yup.string(),
      });

      const UTRNumberValidationSchema = Yup.object({
        UTRNumber: IsShowUTRNumber ? Yup.string().required('UTR No/Receipt No is required') : Yup.string(),
      });

      
      let mergedValidationSchema;
      if (shouldDisplayBankName ==true) {
          mergedValidationSchema = validationSchema
              .concat(bankNameValidationSchema)
              .concat(UTRNumberValidationSchema);
      }
      else if(IsShowUTRNumber == true){
        mergedValidationSchema = validationSchema
        .concat(UTRNumberValidationSchema);
      } else {
          mergedValidationSchema = validationSchema;
      }
      

    const Select = async () => {
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

    const [completed, setCompleted] = useState(0);
    const progress = () => {
        setCompleted((oldCompleted) => {
            if (oldCompleted === 100) return 0;

            const diff = Math.random() * 10;
            return Math.min(oldCompleted + diff, 100);
        });
    };

    const selectPaymentModesWallets = async (currentUserID) => {
        var request = {
            UserID: currentUserID
        };
        var json = JSON.stringify(request);

        const response = await SELECT_PAYMENT_MODE_BYID(json);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setpaymentModes(data);
    };

    const selectWallets = async () => {
        var request = {
            ID: 0
        };
        var json = JSON.stringify(request);

        const { isAuthorized, status, data } = await WALLET_SELECT(json);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setResponseMessage(data);
            setIsError(true);
            return;
        }

        var d = JSON.parse(data);

        if (d.length <= 1) {
            setDropDownOpen(false);

            if (formRef.current) {
                formRef.current.values.WalletID = d[0].WalletID;
            }
        }

        setWallets(d);
    };

    const selectParent = async () => {
        const { isAuthorized, status, data } = await SELECT_PARENT_WITH_ADMIN();

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setResponseMessage(data);
            setIsError(true);
            return;
        }

        var d = JSON.parse(data);
        setRequestToList(d);
    };

    useEffect(() => {
        selectWallets();
        selectParent();
        selectPaymentModesWallets(currentUserID);
        Select();
        createColumns();
    }, [currentUserID]);

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setLoadingOpen(true);
        setIsError(false);

        const { isAuthorized, status, data } = await RequestInsert(
            JSON.stringify(values)
        );
        setLoadingOpen(false);
        if (!isAuthorized) LOGOUT();

        if (!status) {
            setSeverity('error');
            setResponseMessage(data);
            setIsError(true);
            return;
        }

        setSeverity('success');
        setResponseMessage(data);
        setIsError(true);

        setSubmitting(false);
        setShouldDisplayBankName(false);
        resetForm(formObj);
        Select();
        history.push('/Fund/Request');
    };

    const OnChange = (value, setFieldValue) => {
        if (value === undefined || value < 0) return;

        setFieldValue('InWords', NumberToWord(value));
    };

    const handlePayModeChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
        setIsShowUTRNumber(true);
        if (
            value === 'BankTransfer' ||
            value === 'BranchDeposit/CDM' ||
            value === 'CashDeposit'
        ) {
            SelectParentBanks(RequestToID);
            return;
        }
        if (value === 'Cash') {
            setIsShowUTRNumber(false);
            setShouldDisplayBankName(false);
            return;
        }
        if (value === 'Credit') {
            setIsShowUTRNumber(false);
            setShouldDisplayBankName(false);
            return;
        }
        if (value === 'Exchange') {
            setIsShowUTRNumber(false);
            setShouldDisplayBankName(false);
            return;
        }
        setShouldDisplayBankName(false);
    };

    const SelectParentBanks = async (ParentID) => {
        if (ParentID <= 0) return;
        var request = {
            ParentID: ParentID
        };
        var json = JSON.stringify(request);

        const { isAuthorized, status, data } = await SELECT_BY_PARENT_ID(json);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setResponseMessage(data);
            setIsError(true);
            return;
        }

        var d = JSON.parse(data);

        setParentBankDetails(d);
        if (d.length > 0) setShouldDisplayBankName(true);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'FundTransactionID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        columns.push({
            label: 'To',
            name: 'Sender'
        });

        columns.push({
            label: 'Wallet',
            name: 'Wallet'
        });

        columns.push({
            label: 'Mode',
            name: 'PayMode'
        });

        columns.push({
            label: 'Amount',
            name: 'Amount',
            numeric: true
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });
        columns.push({
            label: 'Bank',
            name: 'BankName'
        });

        columns.push({
            label: 'R.Remark',
            name: 'Remark'
        });

        setMainColumns(columns);
    };

    const UploadForm = (formProps) => {
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
                <Grid item xs={12} sm={12} md={6} lg={6}>
                    <h2 className={classes.PaperHead_h2}>Fund Request</h2>
                </Grid>
                <Grid
                    item
                    xs={12}
                    sm={12}
                    md={6}
                    lg={6}
                    style={{
                        display: 'flex',
                        direction: 'rtl'
                    }}
                >
                    <h2 className={classes.PaperHead_h2}>
                        <Button
                            variant="contained"
                            color="primary"
                            className={classes.CustomPrimaryButtonStyle}
                            onClick={() => {
                                history.push('/Fund');
                            }}
                        >
                            RAISED REQUESTS
                        </Button>
                    </h2>
                </Grid>
                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <Form>
                        <Paper className={classes.PaperStyle}>
                            <Grid container spacing={3}>
                                {DropDownOpen ? (
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialSelect
                                            label="Wallet"
                                            name="WalletID"
                                            title="Select Wallet"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Wallet
                                            </MenuItem>
                                            {wallets.map((item) => (
                                                <MenuItem
                                                    key={item.WalletID}
                                                    value={item.WalletID}
                                                >
                                                    {item.ShortName}
                                                </MenuItem>
                                            ))}
                                        </MaterialSelect>
                                    </Grid>
                                ) : null}

                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <MaterialSelect
                                        label="Request TO"
                                        name="RequestTo"
                                        title="Select Parent"
                                        onCustomCallback={(name, value) => {
                                            setRequestToID(value);
                                        }}
                                    >
                                        <MenuItem
                                            key="0"
                                            value="0"
                                            selected={true}
                                        >
                                            Select User
                                        </MenuItem>
                                        {requestToList.map((item) => (
                                            <MenuItem
                                                key={item.ParentID}
                                                value={item.ParentID}
                                            >
                                                {item.FirmName}-
                                                {item.MobileNumber}
                                            </MenuItem>
                                        ))}
                                    </MaterialSelect>
                                </Grid>

                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <MaterialSelect
                                        label="PayMode"
                                        name="PayMode"
                                        title="Select PayMode"
                                        onCustomCallback={(name, value) => {
                                            handlePayModeChange(
                                                name,
                                                value,
                                                formProps.setFieldValue
                                            );
                                        }}
                                    >
                                        <MenuItem key="0" value="0">
                                            Select PayMode
                                        </MenuItem>

                                        {paymentModes.map((item) => (
                                            <MenuItem
                                                key={item.PaymentModeMasterID}
                                                value={item.Type}
                                            >
                                                {item.Type}
                                            </MenuItem>
                                        ))}
                                    </MaterialSelect>
                                </Grid>

                                {shouldDisplayBankName && (
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialSelect
                                            label="BankName"
                                            name="BankName"
                                            title="BankName"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select BankName
                                            </MenuItem>

                                            {parentBankDetails.map(
                                                ({
                                                    BankName,
                                                    AccountNumber,
                                                    IFSC
                                                }) => (
                                                    <MenuItem
                                                        key={AccountNumber}
                                                        value={
                                                            BankName +
                                                            '--' +
                                                            AccountNumber +
                                                            '-' +
                                                            '[' +
                                                            IFSC +
                                                            ']'
                                                        }
                                                    >
                                                        {BankName +
                                                            '--' +
                                                            AccountNumber +
                                                            '-' +
                                                            '[' +
                                                            IFSC +
                                                            ']'}
                                                    </MenuItem>
                                                )
                                            )}
                                        </MaterialSelect>
                                    </Grid>
                                )}

                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <MaterialCurrency
                                        name="Amount"
                                        label="Amount"
                                        placeholder="Amount"
                                        autoComplete="new-password"
                                        max={9999999}
                                        onCustomCallback={(value) => {
                                            OnChange(
                                                value,
                                                formProps.setFieldValue
                                            );
                                        }}
                                    />
                                </Grid>
                                {IsShowUTRNumber ? (
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            label="UTR No/Receipt No"
                                            name="UTRNumber"
                                            type="text"
                                            length={50}
                                            title="UTR No/Receipt No"
                                        />
                                    </Grid>
                                ) : null}

                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <MaterialTextField
                                        label="Remark"
                                        name="Remark"
                                        type="text"
                                        length={50}
                                        title="Remark"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <MaterialDatePickerField
                                        name="TransactionDate"
                                        label="Transaction Date"
                                        maxDate={new Date()}
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <MaterialTextField
                                        label="In words"
                                        name="InWords"
                                        type="text"
                                        readOnly={true}
                                        placeholder="InWords"
                                    />

                                    <Hidden xsUp>
                                        <MaterialTextField
                                            label="Parent"
                                            name="ParentID"
                                            type="text"
                                            readOnly={true}
                                            title="Amount"
                                        />
                                    </Hidden>
                                </Grid>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <label
                                        style={{
                                            margin: '10px'
                                        }}
                                    >
                                        Receipt Image
                                    </label>
                                    <TextField
                                        ref={inputRef}
                                        id="file"
                                        name="Document"
                                        type="file"
                                        accept=".png, .jpg, .jpeg"
                                        onChange={(e) => {
                                            var file = e.target.files[0];

                                            setReceiptImage(
                                                URL.createObjectURL(file)
                                            );

                                            var reader = new FileReader();
                                            formProps.setFieldValue(
                                                'Document.Name',
                                                file.name
                                            );
                                            formProps.setFieldValue(
                                                'Document.Size',
                                                file.size
                                            );
                                            formProps.setFieldValue(
                                                'Document.Type',
                                                file.type
                                            );
                                            reader.onload = function (item) {
                                                formProps.setFieldValue(
                                                    'Document.Data',
                                                    item.target.result
                                                        .replace('data:', '')
                                                        .replace(/^.+,/, '')
                                                );
                                            };

                                            reader.readAsDataURL(file);
                                            setDisplayImage(true);
                                        }}
                                    />
                                    <AmountInWordLabel
                                        name="Note"
                                        readOnly={true}
                                        title="Only jpg/ jpeg and png files are allowed under 3MB!"
                                    />
                                </Grid>
                                {DisplayImage ? (
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <img
                                            name="ReceiptImage"
                                            src={ReceiptImage}
                                            height={100}
                                            width={300}
                                            alt={ReceiptImage}
                                        />
                                    </Grid>
                                ) : null}
                            </Grid>

                            <Divider variant="middle" className="rechdevider" />

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
                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <SecondaryButton
                                        type="reset"
                                        variant="contained"
                                        endIcon={<ResetIcon />}
                                        color="secondary"
                                        title="Reset"
                                        className={
                                            classes.CustomDangerButtonStyle
                                        }
                                        onClick={() => window.location.reload()}
                                    />
                                </Grid>

                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <PrimaryButton
                                        type="submit"
                                        title="Request"
                                        endIcon={<RechargeIcon />}
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        disabled={loadingOpen}
                                    />
                                    {loadingOpen && (
                                        <CircularProgress
                                            size={24}
                                            className={classes.buttonProgress}
                                        />
                                    )}
                                </Grid>
                            </Grid>
                        </Paper>
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
                    <ReactTable
                        data={transactions}
                        columns={mainColumns}
                        headerToolbar={false}
                        footerToolbar={false}
                    />
                </Grid>
            </Grid>
        );
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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={formObj}
                validationSchema={mergedValidationSchema}
                onSubmit={handleSubmit}
                resetForm={formObj}
            >
                {UploadForm}
            </Formik>
        </CustomContainer>
    );
};
