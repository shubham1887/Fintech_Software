import React, { useEffect, useState, useContext } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    Container,
    MenuItem,
    Grid,
    Button,
    Hidden,
    CircularProgress,
    Box
} from '@material-ui/core';
import { SelectByParentID as SELECT_BY_PARENT_ID } from 'Components/Api/UserBankDetailApi';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import { SelectForFundTransfer as WALLET_SELECT } from 'Components/Api/WalletApi';
import { Select as SELECT_PAYMENT_MODE } from 'Components/Api/PaymentModesAPI';
import { SelectByWalletIDAndReceiverID as WALLET_INFO } from 'Components/Api/WalletApi';
import { Transfer as FUND_TRANSFER } from 'Components/Api/FundApi';

import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER,
    IS_CASH_COLLECTION
} from 'Components/Helper/Messages';
import { BalanceContext } from 'Components/BalanceContext';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import HOCLoader from 'Components/Loader/HOCLoader';
import InputLabel from 'Components/Inputes/InputLabel';
const CustomContainer = HOCLoader(Container);

export const TransferInPopUp = ({ FundTransferData, onDismissCallback }) => {
    var UserID = FundTransferData.UserID;
    var FirmName = FundTransferData.FirmName;
    const { DashboardBalanceUpdate } = useContext(BalanceContext);
    const classes = commonStyle();
    const [currentUserID] = useState(localStorage.getItem('UserID'));
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    var CashCollection = localStorage.getItem('IsCashCollection');
    const [isPin] = useState(localStorage.getItem('IsPin'));
    const [IsCashCollection, setIsCashCollection] = useState(false);
    const [wallets, setWallets] = useState([]);
    const [InWords, setInWords] = useState('');
    const [UserInfo, setUserInfo] = useState('');
    const [parentBankDetails, setParentBankDetails] = useState([]);
    const [shouldDisplayBankName, setShouldDisplayBankName] = useState(false);
    const [
        CashCollectionAmountInWords,
        setCashCollectionAmountInWords
    ] = useState('');
    const [paymentModes, setpaymentModes] = useState([]);
    const CheckIsCashCollection = () => {
        if (IS_CASH_COLLECTION === CashCollection) setIsCashCollection(true);
    };
    const selectPaymentModesWallets = async () => {
        const response = await SELECT_PAYMENT_MODE();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setpaymentModes(data);
    };

    const [formObj, setFormObj] = useState({
        WalletID: '0',
        UserID: UserID,
        FirmName: FirmName,
        Amount: '',
        Remark: '',
        PayMode: '0',
        Info: '',
        InWords: '',
        BankName: '',
        CashCollectionAmount: '0',
        CashCollectionAmountInWords: '',
        CashCollectionRemark: '',
        Pin: '',
        IsPin: isPin
    });
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
    const validationSchema = Yup.object({
        UserID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User'),
        WalletID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'wallet')
            .required(PLEASE_SELECT_VALID + 'wallet'),

        Amount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'amount')
            .required(PLEASE_PROVIDE_VALID + 'amount'),

        PayMode: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'mode')
            .required(PLEASE_SELECT_VALID + 'mode'),

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

    const selectDownlineBalanceInformation = async (
        WalletID,
        setFieldValue
    ) => {
        if (WalletID <= 0) return;

        var request = {
            WalletID: WalletID,
            ReceiverID: UserID
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

        const { Balance, Outstanding } = JSON.parse(response.data);
        setUserInfo('Balance:' + Balance + ', OS: ' + Outstanding);
        setFieldValue('Info', 'Balance:' + Balance + ', OS: ' + Outstanding);
    };

    useEffect(() => {
        setLoadingOpen(true);
        CheckIsCashCollection();
        selectPaymentModesWallets();
        selectWallets();

        setLoadingOpen(false);
    }, []);

    const updateWalletID = async (value, setFieldValue) => {
        await selectDownlineBalanceInformation(value, setFieldValue);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoadingOpen(true);

        const request = {
            ReceiverID: values.UserID,
            WalletID: values.WalletID,
            Amount: values.Amount,
            Remark: values.Remark,
            PayMode: values.PayMode,
            CashCollectionAmount: values.CashCollectionAmount,
            CashCollectionRemark: values.CashCollectionRemark,
            Pin: values.Pin
        };

        var response = await FUND_TRANSFER(JSON.stringify(request));

        if (!response.isAuthorized) LOGOUT();

        DashboardBalanceUpdate();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setSeverity('error');

            setTimeout(() => {
                onDismissCallback(false);
            }, MESSAGE_CLOSE_AFTER);
            return;
        }
        setSubmitting(false);
        resetForm();
        reset();

        setTimeout(() => {
            onDismissCallback(false);
        }, MESSAGE_CLOSE_AFTER);
    };

    const reset = () => {
        setUserInfo('');
        setInWords('');
        setCashCollectionAmountInWords('');
        setFormObj({
            WalletID: '0',
            UserID: 0,
            FirmName: '',
            Amount: '',
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
    };

    const OnAmountChange = (value, setFieldValue) => {
        if (value === undefined || value <= 0) return;
        setInWords(NumberToWord(value));
        setFieldValue('InWords', NumberToWord(value));
    };

    const OnCashCollectionAmountChange = (value, setFieldValue) => {
        if (value === undefined || value <= 0) return;
        setCashCollectionAmountInWords(NumberToWord(value));
        setFieldValue('CashCollectionAmountInWords', NumberToWord(value));
    };
    const handlePayModeChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);

        if (value === 'BankTransfer') {
            SelectParentBanks(currentUserID);
            return;
        }

        setShouldDisplayBankName(false);
    };
    const WalletList = wallets.map((item) => (
        <MenuItem key={item.WalletID} value={item.WalletID}>
            {item.Name}
        </MenuItem>
    ));
    const PayModeList = paymentModes.map(({ PaymentModeMasterID, Type }) => (
        <MenuItem key={PaymentModeMasterID} value={Type}>
            {Type}
        </MenuItem>
    ));
    const ParentBankDetailList = parentBankDetails.map(
        ({ BankName, AccountNumber }) => (
            <MenuItem
                key={AccountNumber}
                value={BankName + '--' + AccountNumber}
            >
                {BankName + '--' + AccountNumber}
            </MenuItem>
        )
    );
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
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
            >
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <Grid container item spacing={3}>
                                <Grid item xs={6} sm={3}>
                                    <MaterialTextField
                                        label="FirmName"
                                        name="FirmName"
                                        type="text"
                                        readOnly
                                        title="FirmName"
                                    />

                                    <Hidden xsUp>
                                        <MaterialTextField
                                            label="UserID"
                                            name="UserID"
                                            type="text"
                                            readOnly={true}
                                            placeholder="UserID"
                                        />
                                    </Hidden>
                                </Grid>
                                <Grid item xs={6} sm={3}>
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
                                    <InputLabel
                                        color="green"
                                        value={UserInfo}
                                    />
                                </Grid>

                                <Grid item xs={6} sm={3}>
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
                                <Grid item xs={6} sm={3}>
                                    {shouldDisplayBankName ? (
                                        <MaterialSelect
                                            label="BankName"
                                            name="BankName"
                                            title="BankName"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select BankName
                                            </MenuItem>

                                            {ParentBankDetailList}
                                        </MaterialSelect>
                                    ) : null}
                                </Grid>
                                <Grid item xs={6} sm={3}>
                                    <MaterialCurrency
                                        name="Amount"
                                        label="Amount"
                                        placeholder="Amount"
                                        autoComplete="new-password"
                                        max={999999}
                                        onCustomCallback={(value) => {
                                            OnAmountChange(
                                                value,
                                                setFieldValue
                                            );
                                        }}
                                    />
                                    <InputLabel color="green" value={InWords} />
                                </Grid>

                                <Grid item xs={6} sm={9}>
                                    <MaterialTextField
                                        label="Remark"
                                        name="Remark"
                                        type="text"
                                        length={150}
                                        title="Remark"
                                    />
                                </Grid>
                                {IsCashCollection ? (
                                    <>
                                        <Grid item xs={6} sm={3}>
                                            <MaterialCurrency
                                                name="CashCollectionAmount"
                                                label="Collection Amount"
                                                placeholder="Collection Amount"
                                                autoComplete="new-password"
                                                max={999999}
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

                                        <Grid item xs={6} sm={3}>
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
                                    <Grid item xs={6} sm={3}>
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

                            <Grid
                                className={classes.footerGrid}
                                style={{ marginTop: '20px' }}
                            >
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Request"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loadingOpen}
                                        >
                                            Transfer
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
                                        onClick={() => {
                                            reset();
                                        }}
                                        className={
                                            classes.CustomDangerButtonStyle
                                        }
                                    >
                                        Reset
                                    </Button>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
