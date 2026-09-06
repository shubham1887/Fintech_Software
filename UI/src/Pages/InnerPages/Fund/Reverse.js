import React, { useEffect, useState, useContext, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { Container, MenuItem, Grid, Paper, Button } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import CircularProgress from '@material-ui/core/CircularProgress';
import { SelectUserForDropDown as SELECT_USER_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { TransactionConfirmation } from 'Components/Alert/TransactionConfirmation';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { SelectForFundTransfer as WALLET_SELECT } from 'Components/Api/WalletApi';
import { SelectByWalletIDAndReceiverID as WALLET_INFO } from 'Components/Api/WalletApi';
import {
    Reverse as FUND_REVERSE,
    SelectLast10ForReverseTransaction as SELECT_LAST_10_BY_RECEIVERID,
    ReverseWithOTP as FUND_REVERSE_WITH_OTP,
    GenerateFundReverseOTP as GENERATE_FUNDREVERSE_OTP
} from 'Components/Api/FundApi';
import { SelectValueByKey as SELECT_BY_KEY } from 'Components/Api/SettingMasterApi';
import { OTPModal } from 'Components/Modal/OTPModal';
import Box from '@material-ui/core/Box';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import InputLabel from 'Components/Inputes/InputLabel';
import { BalanceContext } from 'Components/BalanceContext';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import HOCLoader from 'Components/Loader/HOCLoader';
import Divider from '@material-ui/core/Divider';
import {
    ADMIN,
    DISTRIBUTOR,
    SUPER_DISTRIBUTOR
} from 'Components/Helper/UsertypeCode';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { is } from 'date-fns/locale';

const CustomContainer = HOCLoader(Container);

export const Reverse = () => {
    const { DashboardBalanceUpdate } = useContext(BalanceContext);
    const classes = commonStyle();
    const formRef = useRef();
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [FundReversalForUpline, setFundReversalForUpline] = useState('No');
    const [wallets, setWallets] = useState([]);
    const [downline, setDownline] = useState([]);
    const [downlineReport, setDownlineReport] = useState([]);
    const [IsOTP, setIsOTP] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [senderID, setSenderID] = useState(0);
    const [walletID, setWalletID] = useState(0);
    const [amount, setAmount] = useState(0);
    const [remark, setRemark] = useState('');
    const [pin, setPin] = useState('');
    const [currentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );
    const [isPin] = useState(localStorage.getItem('IsPin'));
    const [openOTP, setOpenOTP] = useState(false);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [isReverseAccess, setIsReverseAccess] = useState(false);
    const [modalErrorMessage, setModalErrorMessage] = useState('');
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const [formObj, setFormObj] = useState({
        ServiceID: 1,
        WalletID: '0',
        User: userDefault,
        Amount: '',
        TotalAmount: '',
        Remark: '',
        PayMode: '0',
        Info: '',
        InWords: '',
        CashCollectionAmount: '0',
        CashCollectionAmountInWords: '',
        CashCollectionRemark: '',
        Pin: '',
        IsPin: isPin
    });

    const validationSchema = Yup.object({
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.string()
                    .notOneOf(['0'], PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            }),
        WalletID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'wallet')
            .required(PLEASE_SELECT_VALID + 'wallet'),

        Amount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'amount')
            .required(PLEASE_PROVIDE_VALID + 'amount'),

        Remark: Yup.string().required(PLEASE_PROVIDE_VALID + 'remark'),

        Pin: Yup.number().when(['IsPin'], {
            is: (isPin) => isPin === 'true',
            then: Yup.number().required('Please enter pin')
        })
    });

    const selectWallets = async () => {
        var request = {
            ID: 0
        };
        var json = JSON.stringify(request);
        const response = await WALLET_SELECT(json);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setWallets(data);
    };
    const callSearch = async (text) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            text.length <= 0 ||
            text === userDefault.FirmName
        )
            return;

        var jsonObj = {
            FirmName: text
        };
        selectDownline(jsonObj);
    };
    const selectDownline = async (values) => {
        setLoadingOpen(true);
        var request = JSON.stringify(values);
        setLoadingOpen(false);
        const response = await SELECT_USER_FOR_DROPDOWN(request);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);
        let objWithSelectAll = [userDefault, ...data];

        setDownline(objWithSelectAll);
    };

    const selectDownlineBalanceInformation = async (
        senderID,
        WalletID,
        setFieldValue
    ) => {
        if (WalletID <= 0) return;

        if (senderID <= 0) return;

        var request = {
            WalletID: WalletID,
            ReceiverID: senderID
        };

        setLoadingOpen(true);
        var json = JSON.stringify(request);

        const response = await WALLET_INFO(json);

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);

        setFieldValue(
            'Info',
            'Balance:' + data.Balance + ', OS: ' + data.Outstanding
        );
    };

    useEffect(() => {
        setLoadingOpen(true);
        if (
            currentUserTypeID === DISTRIBUTOR ||
            currentUserTypeID === SUPER_DISTRIBUTOR
        ) {
            GetFlagFundReversal();
        }

        ReverseAccess(FundReversalForUpline);
        createColumns();
        selectWallets();

        setLoadingOpen(false);
    }, []);

    const ReverseAccess = async (FundReversalForUpline) => {
        if (currentUserTypeID === ADMIN) setIsReverseAccess(true);

        if (FundReversalForUpline === 'Yes') {
            if (
                currentUserTypeID === DISTRIBUTOR ||
                currentUserTypeID === SUPER_DISTRIBUTOR
            )
                setIsReverseAccess(true);
        }
    };

    const GetFlagFundReversal = async () => {
        let jsonObj = {
            Key: 'FundReversalForUpline'
        };

        setLoadingOpen(true);

        const { isAuthorized, status, data } = await SELECT_BY_KEY(
            JSON.stringify(jsonObj)
        );
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) return;

        var d = JSON.parse(data);

        setFundReversalForUpline(d);
        ReverseAccess(d);
        setIsOTP(true);
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
            label: 'From',
            name: 'Sender'
        });

        columns.push({
            label: 'To',
            name: 'Receiver'
        });

        columns.push({
            label: 'Wallet',
            name: 'Wallet'
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
            label: 'Remark',
            name: 'Remark'
        });

        setMainColumns(columns);
    };

    const updateSenderID = async (value, setFieldValue) => {
        setSenderID(value.UserID);
        setUserDefault({
            UserID: value.UserID,
            FirmName: value.FirmName
        });
        await selectDownlineBalanceInformation(
            value.UserID,
            walletID,
            setFieldValue
        );
    };

    const SelectDownLineReport = async (UserID, WalletID) => {
        var request = {
            ReceiverID: UserID,
            SenderID: UserID,
            WalletID: WalletID
        };

        setLoadingOpen(true);
        var json = JSON.stringify(request);

        const response = await SELECT_LAST_10_BY_RECEIVERID(json);

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }
        createColumns();
        var data = JSON.parse(response.data);

        setDownlineReport(data);
    };

    const updateWalletID = async (value, setFieldValue) => {
        setWalletID(value);

        await selectDownlineBalanceInformation(senderID, value, setFieldValue);
        await SelectDownLineReport(senderID, value);
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        let op = wallets.filter((o) => o.WalletID === values.WalletID);
        values.Name = op[0].Name;
        values.TotalAmount = values.Amount;
        setIsAlertOpen(true);
        setConfirmAlertRequest(values);
    };

    const GenerateFundReverseOTP = async (values) => {
        setResponseMessage('');
        setLoadingOpen(true);
        const request = {
            SenderID: values.User.UserID,
            WalletID: values.WalletID,
            Amount: values.Amount,
            Remark: values.Remark,
            Pin: values.Pin
        };
        var response = await GENERATE_FUNDREVERSE_OTP(JSON.stringify(request));

        if (!response.isAuthorized) LOGOUT();

        setLoadingOpen(false);
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        var data = JSON.parse(response.data);

        if (data.IsOTPSent) {
            setAmount(data.Amount);
            setOpenOTP(true);
            setWalletID(data.WalletID);
            setRemark(data.Remark);
            setPin(data.Pin);
            setSenderID(data.SenderID);
        }
    };
    const FundReverse = async (values) => {
        setLoadingOpen(true);
        setResponseMessage('');
        if (IsOTP === true) {
            GenerateFundReverseOTP(values);
            return;
        }
        const request = {
            SenderID: values.User.UserID,
            WalletID: values.WalletID,
            Amount: values.Amount,
            Remark: values.Remark,
            Pin: values.Pin
        };

        var response = await FUND_REVERSE(JSON.stringify(request));

        if (!response.isAuthorized) LOGOUT();

        DashboardBalanceUpdate();
        setLoadingOpen(false);

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        SelectDownLineReport(
            formRef.current.values.User.UserID,
            formRef.current.values.WalletID
        );
        reset();
    };

    const reset = () => {
        setSenderID(0);
        setWalletID(0);
        setAmount(0);
        setRemark('');
        setPin('');
        setUserDefault({
            UserID: 0,
            FirmName: 'Select User'
        });
        setFormObj({
            ServiceID: 1,
            WalletID: '0',
            User: userDefault,
            Amount: '',
            TotalAmount: '',
            Remark: '',
            Info: '',
            InWords: '',
            Pin: '',
            IsPin: isPin
        });
    };

    const FundReverseWithOTP = async (otpObj) => {
        setLoadingOpen(true);
        setOpenOTP(false);
        const { OTP } = otpObj;

        const request = {
            SenderID: senderID,
            WalletID: walletID,
            Amount: amount,
            Remark: remark,
            Pin: pin,
            OTP: OTP
        };

        var response = await FUND_REVERSE_WITH_OTP(JSON.stringify(request));

        if (!response.isAuthorized) LOGOUT();

        DashboardBalanceUpdate();
        setLoadingOpen(false);

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        SelectDownLineReport(
            formRef.current.values.User.UserID,
            formRef.current.values.WalletID
        );
        reset();
    };

    const OnAmountChange = (value, setFieldValue) => {
        if (value === undefined || value <= 0) return;

        setFieldValue('InWords', NumberToWord(value));
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
                key={'formTransfer'}
                enableReinitialize={true}
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
                render={({
                    values,
                    setFieldValue,
                    resetForm,
                    handleSubmit
                }) => {
                    return (
                        <Grid
                            item
                            container
                            margin="normal"
                            width="100vw"
                            xs={12}
                            md={12}
                            sm={12}
                            lg={12}
                            spacing={2}
                        >
                            <Grid item xs={12} sm={12} md={6} lg={4}>
                                <Form>
                                    <h2 className={classes.PaperHead_h2}>
                                        Fund Reverse
                                    </h2>

                                    <Paper className={classes.PaperStyle}>
                                        <Grid container item spacing={3}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <Field
                                                    component={
                                                        AsyncAutoComplete
                                                    }
                                                    defaultValue={userDefault}
                                                    FieldName="User"
                                                    name="User"
                                                    SubFieldName="UserID"
                                                    options={downline}
                                                    getOptionLabel={(option) =>
                                                        option.FirmName
                                                    }
                                                    textFieldProps={{
                                                        label: 'Select Receiver'
                                                    }}
                                                    onSearchCallback={(
                                                        text
                                                    ) => {
                                                        callSearch(text);
                                                    }}
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        updateSenderID(
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
                                                <MaterialSelect
                                                    label="Wallet"
                                                    name="WalletID"
                                                    title="Select Wallet"
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        updateWalletID(
                                                            value,
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Wallet
                                                    </MenuItem>
                                                    {wallets.map((item) => (
                                                        <MenuItem
                                                            key={item.WalletID}
                                                            value={
                                                                item.WalletID
                                                            }
                                                        >
                                                            {item.Name}
                                                        </MenuItem>
                                                    ))}
                                                </MaterialSelect>
                                                <InputLabel
                                                    color="green"
                                                    value={values.Info}
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <MaterialCurrency
                                                    name="Amount"
                                                    label="Amount"
                                                    placeholder="Amount"
                                                    autoComplete="new-password"
                                                    max={9999999}
                                                    onCustomCallback={(
                                                        value
                                                    ) => {
                                                        OnAmountChange(
                                                            value,
                                                            setFieldValue
                                                        );
                                                    }}
                                                />
                                                <InputLabel
                                                    color="green"
                                                    value={values.InWords}
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
                                                    label="Remark"
                                                    name="Remark"
                                                    type="text"
                                                    length={150}
                                                    title="Remark"
                                                />
                                            </Grid>
                                            {isPin === 'true' ? (
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
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
                                                    variant="contained"
                                                    color="secondary"
                                                    type="reset"
                                                    title="Reset"
                                                    onClick={() => {
                                                        reset();
                                                        setFieldValue(
                                                            'User',
                                                            userDefault
                                                        );
                                                    }}
                                                    className={
                                                        classes.CustomDangerButtonStyle
                                                    }
                                                >
                                                    Reset
                                                </SecondaryButton>
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={6}
                                                lg={6}
                                            >
                                                {isReverseAccess === true ? (
                                                    <PrimaryButton
                                                        type="submit"
                                                        title="Reverse"
                                                        className={
                                                            classes.CustomPrimaryButtonStyle
                                                        }
                                                        disabled={loadingOpen}
                                                        onClick={(e) => {
                                                            setFieldValue(
                                                                'IsOTP',
                                                                IsOTP
                                                            );
                                                            handleSubmit(e);
                                                        }}
                                                    >
                                                        Reverse
                                                    </PrimaryButton>
                                                ) : null}
                                            </Grid>
                                        </Grid>
                                    </Paper>
                                </Form>
                            </Grid>
                            <Grid
                                item
                                xs={12}
                                sm={12}
                                md={6}
                                lg={8}
                                className="lastrecharges"
                            >
                                <h2 className={classes.PaperHead_h2}>
                                    Last 10 Fund Reverse
                                </h2>
                                <ReactTable
                                    data={downlineReport}
                                    columns={mainColumns}
                                    headerToolbar={false}
                                    footerToolbar={false}
                                />
                            </Grid>
                        </Grid>
                    );
                }}
            />

            {openOTP ? (
                <OTPModal
                    isOpen={openOTP}
                    title="OTP"
                    error={modalErrorMessage}
                    submitCallback={(value) => {
                        FundReverseWithOTP(value);
                    }}
                    resendCallback={(value) => {}}
                    onDismissCallback={(value) => {
                        setOpenOTP(value);
                    }}
                />
            ) : null}
            {isAlertOpen ? (
                <TransactionConfirmation
                    isOpen={isAlertOpen}
                    title="Fund Reverse"
                    message="Are you sure to fund reverse?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        FundReverse(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
