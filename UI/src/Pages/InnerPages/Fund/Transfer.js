import { Container, Grid, MenuItem, Paper } from '@material-ui/core';
import {
    SelectLast10ByReceiverID as SELECT_LAST_10_BY_RECEIVERID,
    Transfer as FUND_TRANSFER
} from 'Components/Api/FundApi';
import { SelectUserCommissionByUserID as SELECT_USER_COMMISSION_BY_USER_ID } from 'Components/Api/UsersApi';
import { SelectByReceiverID as SELECT_PAYMENT_MODE_BYID } from 'Components/Api/PaymentModesAPI';
import { SelectByParentID as SELECT_BY_PARENT_ID } from 'Components/Api/UserBankDetailApi';
import { SelectDownlinForFundTransfer as USER_SELECT_DOWNLINE_FOR_FUND_TRANSFER } from 'Components/Api/UsersApi';
import { TransactionConfirmation } from 'Components/Alert/TransactionConfirmation';

import {
    SelectForFundTransfer as WALLET_SELECT,
    SelectByWalletIDAndReceiverID as WALLET_INFO
} from 'Components/Api/WalletApi';
import { BalanceContext } from 'Components/BalanceContext';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    IS_CASH_COLLECTION,
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import InputLabel from 'Components/Inputes/InputLabel';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import HOCLoader from 'Components/Loader/HOCLoader';
import ReactTable from 'Components/ReactTable/ReactTable';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { LOGOUT } from 'ConstComponents';
import { Field, Form, Formik } from 'formik';
import React, { useContext, useEffect, useState, useRef } from 'react';
import { useLocation } from 'react-router-dom';
import * as Yup from 'yup';
import { EqualsIgnoreCase } from 'Components/Helper/Validation';
import { SHOW_PAYMODE } from 'Config';
import { FETCH_LIST } from 'Config';
import Divider from '@material-ui/core/Divider';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import {FUND_TRANSFER as FUND_TRANSFER_ID}from 'Components/Helper/Messages';
const CustomContainer = HOCLoader(Container);

export const Transfer = () => {
    const location = useLocation();
    const formRef = useRef();
    const { DashboardBalanceUpdate } = useContext(BalanceContext);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [wallets, setWallets] = useState([]);
    const [downline, setDownline] = useState([]);
    const [downlineReport, setDownlineReport] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [receiverID, setReceiverID] = useState(0);
    const [WalletDropDownOpen, setWalletDropDownOpen] = useState(true);
    const [walletID, setWalletID] = useState(0);
    const [paymentModes, setpaymentModes] = useState([]);
    let CashCollection = localStorage.getItem('IsCashCollection');
    let CommissionOnTransfer = localStorage.getItem('CommissionOnTransfer');
    const [isPin] = useState(localStorage.getItem('IsPin'));
    const [IsCashCollection, setIsCashCollection] = useState(false);
    const [IsFetchAllList, setisFetchAllList] = useState(false);
    const [parentBankDetails, setParentBankDetails] = useState([]);
    const [shouldDisplayBankName, setShouldDisplayBankName] = useState(false);
    const [currentUserID] = useState(localStorage.getItem('UserID'));
    const [UserInfo, setUserInfo] = useState('');
    const [InWords, setInWords] = useState('');
    const [BackDateFundTransfer] = useState(
        localStorage.getItem('BackDateFundTransfer')
    );
    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }
    const [TotalAmountInWords, setTotalAmountInWords] = useState('');
    const [
        CashCollectionAmountInWords,
        setCashCollectionAmountInWords
    ] = useState('');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const [PaymodeOnOff, setPaymodeOnOff] = useState(false);

    const fetchPaymodeOnOff = async () => {
        let isOn = await SHOW_PAYMODE();
        if (formRef.current) {
            formRef.current.values.PayMode = 'Cash';
        }
        if (isOn === 'YES') {
            setPaymodeOnOff(true);
            if (formRef.current) {
                formRef.current.values.PayMode = '0';
            }
            return;
        }
    };

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const CheckIsCashCollection = () => {
        if (IS_CASH_COLLECTION === CashCollection) setIsCashCollection(true);
    };

    const selectPaymentModesWallets = async (UserID) => {
        var request = {
            UserID: UserID
        };
        var json = JSON.stringify(request);

        const response = await SELECT_PAYMENT_MODE_BYID(json);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);
        setpaymentModes(data);
    };

    const fetchUserList = async () => {
        let listValue = await FETCH_LIST();

        if (listValue === undefined || listValue === null || listValue === '') {
            return;
        }

        if (listValue === 'All') {
            setisFetchAllList(true);
            selectDownline('');
            return;
        }
    };

    const [formObj, setFormObj] = useState({
        ServiceID: 1,
        WalletID: 0,
        User: userDefault,
        Amount: '',
        TotalAmount: '',
        Remark: '',
        PayMode: '0',
        BankName: 0,
        Info: '',
        InWords: '',
        TotalAmountInWords: '',
        CashCollectionAmount: '',
        CashCollectionAmountInWords: '',
        CashCollectionRemark: '',
        Pin: '',
        ToDate: new Date(),
        IsPin: isPin
    });

    const [userCommission, setUserCommission] = useState({
        Comm: 0,
        CommType: '',
        CommissionCategory: '',
        UserID: 0
    });

    const validationSchema = Yup.object({
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + ' User')
            .required(PLEASE_SELECT_VALID + ' User')
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
            return;
        }

        var data = JSON.parse(response.data);
        if (data.length <= 1) {
            setWalletDropDownOpen(false);
            setWalletID(data[0].WalletID);

            if (formRef.current) {
                formRef.current.values.WalletID = data[0].WalletID;
            }
        }
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

        selectDownline(text);
    };

    const isValidToRefreshList = (list) => {
        if (IsFetchAllList === false) {
            return true;
        }

        if (list === undefined || list === null) {
            return true;
        }

        if (list.length <= 1) {
            return true;
        }

        return false;
    };

    const selectDownline = async (values) => {
        if (IsFetchAllList === true) {
            values = '';
        }

        var jsonObj = {
            FirmName: values
        };

        if (isValidToRefreshList(downline) === false) {
            return;
        }

        setLoadingOpen(true);
        var request = JSON.stringify(jsonObj);
        setLoadingOpen(false);
        const response = await USER_SELECT_DOWNLINE_FOR_FUND_TRANSFER(request);
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
        ReceiverID,
        WalletID,
        setFieldValue
    ) => {
        if (WalletID <= 0) return;

        if (ReceiverID <= 0) return;

        var request = {
            WalletID: WalletID,
            ReceiverID: ReceiverID
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

        const { Balance, Outstanding, BalanceDetail } = JSON.parse(
            response.data
        );
        setUserInfo(BalanceDetail);
        setFieldValue('Info', 'Balance:' + Balance + ', OS: ' + Outstanding);
    };

    useEffect(() => {
        setLoadingOpen(true);
        CheckIsCashCollection();
        fetchConsumerNumberFontSize();
        fetchPaymodeOnOff();
        createColumns();
        fetchUserList();
        selectWallets();
        setLoadingOpen(false);
    }, []);

    useEffect(() => {
        const timer = setTimeout(() => {
            SelectUserFromLocation();
        }, 300);
        return () => clearTimeout(timer);
    }, []);

    const SelectUserFromLocation = () => {
        if (location.state === undefined) return;

        if (location.state === null) return;

        if (location.state.FundTransfer === null) return;

        setReceiverID(location.state.FundTransfer.UserID);
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
            label: 'Remark',
            name: 'Remark'
        });

        setMainColumns(columns);
    };

    const updateReceiverID = async (value, setFieldValue) => {
        CommissionByUserID(value.UserID);
        setReceiverID(value.UserID);
        setUserDefault({
            UserID: value.UserID,
            FirmName: value.FirmName
        });
        await selectDownlineBalanceInformation(
            value.UserID,
            walletID,
            setFieldValue
        );

        SelectDownLineReport(value.UserID, walletID);
        selectPaymentModesWallets(value.UserID);
    };

    const SelectDownLineReport = async (UserID, WalletID) => {
        if (WalletID === undefined || WalletID === null) {
            return;
        }

        if (UserID === undefined || UserID === null) {
            return;
        }
        let request = {
            ReceiverID: UserID,
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
        let data = JSON.parse(response.data);
        setDownlineReport(data);
        //setWalletID(WalletID);
    };

    const updateWalletID = async (value, setFieldValue) => {
        setWalletID(value);
        await selectDownlineBalanceInformation(
            receiverID,
            value,
            setFieldValue
        );

        await SelectDownLineReport(receiverID, value);
    };
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(false);

        let op = wallets.filter((o) => o.WalletID === values.WalletID);

        values.Name = op[0].Name;
        values.ServiceID = FUND_TRANSFER_ID;
        setIsAlertOpen(true);
        setConfirmAlertRequest(values);
    };

    const FundTransfer = async (values) => {
        setLoadingOpen(true);
        setIsError(false);

        if (
            values.Remark === undefined ||
            values.Remark === null ||
            values.Remark === ''
        )
            values.Remark = values.Amount + ',' + values.PayMode;
        if (
            values.CashCollectionAmount === undefined ||
            values.CashCollectionAmount === null ||
            values.CashCollectionAmount === ''
        )
            values.CashCollectionAmount = 0;

        const request = {
            ReceiverID: values.User.UserID,
            WalletID: values.WalletID,
            Amount: values.Amount,
            Remark: values.Remark,
            PayMode: values.PayMode,
            CashCollectionAmount: values.CashCollectionAmount,
            CashCollectionRemark: values.CashCollectionRemark,
            BankName: values.BankName,
            Pin: values.Pin,
            ToDate: values.ToDate
        };

        setResponseMessage('');
        let response = await FUND_TRANSFER(JSON.stringify(request));
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        DashboardBalanceUpdate();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        setResponseMessage(response.data);
        setSeverity('success');
        setIsError(true);
        SelectDownLineReport(values.User.UserID, values.WalletID);
        reset();
        setFormObj({
            Amount: '',
            Remark: ''
            // WalletID : walletID
        });
    };
    const SelectParentBanks = async (ParentID) => {
        if (ParentID <= 0) return;
        let request = {
            ParentID: ParentID
        };
        let json = JSON.stringify(request);

        const { isAuthorized, status, data } = await SELECT_BY_PARENT_ID(json);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setResponseMessage(data);
            setIsError(true);
            return;
        }

        let d = JSON.parse(data);

        setParentBankDetails(d);
        if (d.length > 0) setShouldDisplayBankName(true);
    };

    const reset = () => {
        selectWallets();
        if (WalletDropDownOpen) {
            setWalletID(0);
        }
        setUserInfo('');
        setInWords('');
        setReceiverID(0);
        setShouldDisplayBankName(false);
        setCashCollectionAmountInWords('');
        setTotalAmountInWords('');
        setUserDefault({
            UserID: 0,
            FirmName: 'Select User'
        });

        if (WalletDropDownOpen) {
            setFormObj({
                ServiceID: 1,
                User: userDefault,
                WalletID: 0,
                Amount: '',
                TotalAmount: '',
                Remark: '',
                PayMode: '0',
                BankName: 0,
                Info: '',
                InWords: '',
                CashCollectionAmount: '',
                CashCollectionAmountInWords: '',
                CashCollectionRemark: '',
                Pin: '',
                IsPin: isPin,
                ToDate: new Date()
            });
        } else {
            setFormObj({
                ServiceID: 1,
                User: userDefault,
                WalletID: walletID,
                Amount: '',
                TotalAmount: '',
                Remark: '',
                BankName: 0,
                Info: '',
                InWords: '',
                CashCollectionAmount: '',
                CashCollectionAmountInWords: '',
                CashCollectionRemark: '',
                Pin: '',
                IsPin: isPin,
                ToDate: new Date()
            });
        }
    };

    const OnAmountChangeCore = (value, setFieldValue) => {
        if (value === undefined || value === null || value <= 0) {
            setFieldValue('InWords');
            setInWords('');
            setTotalAmountInWords('');
            return;
        }

        let wwords = NumberToWord(value);
        setInWords(wwords);
        setFieldValue('InWords', wwords);
        let total = (parseFloat(userCommission.Comm) / 100) * parseFloat(value);

        if (walletID === 1) {
            total += parseFloat(value);
        } else {
            total = parseFloat(value);
        }

        setFieldValue('TotalAmount', total);
        setTotalAmountInWords(NumberToWord(total));
        setFieldValue('TotalAmountInWords', NumberToWord(total));
    };

    const OnCashCollectionAmountChange = (value, setFieldValue) => {
        if (value === undefined || value <= 0 || value === null) {
            setFieldValue('CashCollectionAmountInWords');
            setCashCollectionAmountInWords('');
            return;
        }

        setCashCollectionAmountInWords(NumberToWord(value));
        setFieldValue('CashCollectionAmountInWords', NumberToWord(value));
    };

    const handlePayModeChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);

        if (value === 'BankTransfer') {
            SelectParentBanks(currentUserID);
            return;
        }
        if (value === 'CashDeposit') {
            SelectParentBanks(currentUserID);
            return;
        }

        setShouldDisplayBankName(false);
    };

    const PayModeList = paymentModes.map(({ PaymentModeMasterID, Type }) => (
        <MenuItem key={PaymentModeMasterID} value={Type}>
            {Type}
        </MenuItem>
    ));

    const ParentBankDetailList = parentBankDetails.map(
        ({ BankName, AccountNumber, IFSC }) => (
            <MenuItem
                key={AccountNumber}
                value={BankName + '-' + AccountNumber + ' [' + IFSC + ']'}
            >
                {BankName + '-' + AccountNumber + ' [' + IFSC + ']'}
            </MenuItem>
        )
    );

    const WalletList = wallets.map((item) => (
        <MenuItem key={item.WalletID} value={item.WalletID}>
            {item.ShortName}
        </MenuItem>
    ));

    const BankNamesDropDown = shouldDisplayBankName ? (
        <MaterialSelect label="Bank" name="BankName" title="Bank">
            <MenuItem key="0" value="0">
                Select BankName
            </MenuItem>

            {ParentBankDetailList}
        </MaterialSelect>
    ) : null;

    const CommissionByUserID = async (UserID) => {
        let request = { UserID: UserID };
        setLoadingOpen(true);
        const {
            isAuthorized,
            status,
            data
        } = await SELECT_USER_COMMISSION_BY_USER_ID(JSON.stringify(request));
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();
        if (!status) {
            setSeverity('error');
            setResponseMessage(data);
            setIsError(true);

            return;
        }

        let d = JSON.parse(data);

        setUserCommission({
            Comm: d.Comm,
            CommType: d.CommType,
            CommissionCategory: d.CommissionCategory,
            UserID: UserID
        });
    };

    const TransferForm = ({ values, errors, setFieldValue }) => {
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
                <Grid item xs={12} sm={12} md={6} lg={4}>
                    <Form>
                        <h2 className={classes.PaperHead_h2}>Fund Transfer</h2>
                        <Paper className={classes.PaperStyle}>
                            <Grid container item spacing={3}>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <Field
                                        component={AsyncAutoComplete}
                                        defaultValue={userDefault}
                                        FieldName="User"
                                        name="User"
                                        SubFieldName="UserID"
                                        options={downline}
                                        getOptionLabel={(option) =>
                                            option.FirmName
                                        }
                                        textFieldProps={{
                                            label: 'Select User'
                                        }}
                                        onSearchCallback={(text) => {
                                            callSearch(text);
                                        }}
                                        onCustomCallback={(name, value) => {
                                            updateReceiverID(
                                                value,
                                                setFieldValue
                                            );
                                            setUserDefault({
                                                UserID: value.UserID,
                                                FirmName: value.FirmName
                                            });
                                        }}
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    {WalletDropDownOpen ? (
                                        <MaterialSelect
                                            label="Wallet"
                                            name="WalletID"
                                            title="Select Wallet"
                                            onCustomCallback={(name, value) => {
                                                updateWalletID(
                                                    value,
                                                    setFieldValue
                                                );
                                            }}
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Wallet
                                            </MenuItem>
                                            {WalletList}
                                        </MaterialSelect>
                                    ) : null}
                                    <InputLabel
                                        color="green"
                                        value={UserInfo}
                                    />
                                </Grid>
                                {PaymodeOnOff && (
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialSelect
                                            label="Pay Mode"
                                            name="PayMode"
                                            title="Select Mode"
                                            onCustomCallback={(name, value) => {
                                                handlePayModeChange(
                                                    name,
                                                    value,
                                                    setFieldValue
                                                );
                                            }}
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Mode
                                            </MenuItem>
                                            {PayModeList}
                                        </MaterialSelect>
                                    </Grid>
                                )}
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    {BankNamesDropDown}
                                </Grid>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <MaterialCurrency
                                        font_size={consumerNumberFontSize}
                                        name="Amount"
                                        label="Amount"
                                        placeholder="Amount"
                                        autoComplete="new-password"
                                        max={9999999}
                                        onCustomCallback={(value) => {
                                            OnAmountChangeCore(
                                                value,
                                                setFieldValue
                                            );
                                        }}
                                    />
                                    <InputLabel color="green" value={InWords} />
                                </Grid>
                                {EqualsIgnoreCase(
                                    CommissionOnTransfer,
                                    'Yes'
                                ) ? (
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            name="TotalAmount"
                                            label="TotalAmount"
                                            placeholder="TotalAmount"
                                            title="TotalAmount"
                                            disabled
                                            type="text"
                                        />
                                        <InputLabel
                                            color="green"
                                            value={TotalAmountInWords}
                                        />
                                    </Grid>
                                ) : null}
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <MaterialTextField
                                        label="Remark"
                                        name="Remark"
                                        type="text"
                                        length={150}
                                        title="Remark"
                                    />
                                </Grid>
                                {EqualsIgnoreCase(
                                    CommissionOnTransfer,
                                    'Yes'
                                ) ? null : (
                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={12}
                                        lg={12}
                                    ></Grid>
                                )}
                                {IsCashCollection ? (
                                    <>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <MaterialCurrency
                                                name="CashCollectionAmount"
                                                font_size={
                                                    consumerNumberFontSize
                                                }
                                                label="Collection Amount"
                                                placeholder="Collection Amount"
                                                autoComplete="new-password"
                                                max={9999999}
                                                onCustomCallback={(value) => {
                                                    OnCashCollectionAmountChange(
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                            />
                                            <InputLabel
                                                color="green"
                                                value={
                                                    CashCollectionAmountInWords
                                                }
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
                                                label="Collection Remark"
                                                name="CashCollectionRemark"
                                                type="text"
                                                length={150}
                                                title="Remark"
                                            />
                                        </Grid>
                                    </>
                                ) : null}
                                {isPin === 'true' ? (
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
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

                                {BackDateFundTransfer === 'true' ? (
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialDatePickerField
                                            name="ToDate"
                                            label="To Date"
                                            maxDate={new Date()}
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
                                        variant="contained"
                                        color="secondary"
                                        type="reset"
                                        title="Reset"
                                        onClick={() => {
                                            reset();
                                            //setFieldValue('User', userDefault);
                                        }}
                                        className={
                                            classes.CustomDangerButtonStyle
                                        }
                                    >
                                        Reset
                                    </SecondaryButton>
                                </Grid>

                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <PrimaryButton
                                        type="submit"
                                        title="Transfer"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        disabled={loadingOpen}
                                    >
                                        Transfer
                                    </PrimaryButton>
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
                    <h2 className={classes.PaperHead_h2}>Last 10 Fund</h2>
                    <ReactTable
                        data={downlineReport}
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
                key={'formTransfer'}
                enableReinitialize={true}
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={formObj}
            >
                {TransferForm}
            </Formik>
           
            {isAlertOpen ? (
                <TransactionConfirmation
                    isOpen={isAlertOpen}
                    title="Fund Transfer"
                    message="Are you sure to fund transfer?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        FundTransfer(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
