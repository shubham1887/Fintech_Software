import React, { useState, useEffect, useContext } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { EqualsIgnoreCase } from 'Components/Helper/Validation';
import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import {
    GenerateOTP as GENERATE_OTP,
    VerifyFoundTransferOTP as VERIFY_FOUND_TRANSFER_OTP,
    Transfer as TRANSFER,
    SelectLast10BySenderID as SELECT_LAST_10_BY_SENDERID
} from 'Components/Api/RetailerToAllFundTransferAPI';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import {
    Container,
    Grid,
    Paper,
    MenuItem,
    CircularProgress
} from '@material-ui/core';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import InputLabel from 'Components/Inputes/InputLabel';
import { AEPS_WalletID } from 'Components/Helper/Messages';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { BalanceContext } from 'Components/BalanceContext';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import FormHeader from 'Components/FormHeader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { OTPModal } from 'Components/Modal/OTPModal';
const CustomContainer = HOCLoader(Container);

const RetailerToAllFundTransfer = () => {
    const classes = commonStyle();
    const { DashboardBalanceUpdate } = useContext(BalanceContext);
    const [responseMessage, setResponseMessage] = useState('');
    const [IsTrans, setIsTrans] = useState(true);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [MobileNumber, setMobileNumber] = useState('');
    const [InWords, setInWords] = useState('');
    const [showTansfer, setshowTansfer] = useState(false);
    const [openOTP, setOpenOTP] = useState(false);
    const [isError, setIsError] = useState(false);
    const [wallets, setWallets] = useState([]);
    const [transferReport, setTransferReport] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    var IsW2OWithSenderWallet =
        localStorage.getItem('IsW2OWithSenderWallet') === 'true';
    useEffect(() => {
        selectWallets();
        SelectReport();
    }, [IsW2OWithSenderWallet]);

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
        if (!IsW2OWithSenderWallet) {
            data = data.filter((d) => {
                return d.WalletID !== AEPS_WalletID;
            });
        }

        setWallets(data);
    };

    const OnAmountChange = (value, setFieldValue) => {
        if (value === undefined || value < 0) {
            setInWords('');
            return;
        }
        setInWords(NumberToWord(value));
    };

    const handleSubmit = async (values, { resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setIsError(false);

        if (values.IsGenerateOTP) {
            setLoading(true);
            const response = await GENERATE_OTP(requestObj);
            setLoading(false);

            if (!response.isAuthorized) LOGOUT();
            if (!response.status) {
                setSeverity('error');
                setResponseMessage(response.data);
                setIsError(true);
                return;
            }
            let data = JSON.parse(response.data);
            setSeverity('success');
            setResponseMessage(data.Message);
            setIsError(true);
            if (!data.IsOtpRequired) {
                setFormObj({
                    ...FormObj,
                    ReceiverID: data.UserID,
                    FirmName: data.FirmName
                });
                setshowTansfer(true);
                return;
            }
            setOpenOTP(true);
            setMobileNumber(values.MobileNumber);
            return;
        }

        if (values.IsTransfer === true) {
            let errorValidation = false;

            await TransferValidation.validate(values).catch(function (err) {
                errorValidation = true;
                setSeverity('error');
                setResponseMessage(err.message);

                setIsError(true);
            });
            if (errorValidation === true) {
                return;
            }
        }
        setLoading(true);
        var response = await TRANSFER(requestObj);
        setLoading(false);

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
        DashboardBalanceUpdate();
        setshowTansfer(false);
        resetForm(FormObj);
        reset();
    };
    const [FormObj, setFormObj] = useState({
        MobileNumber: '',
        FirmName: '',
        WalletID: 0,
        SenderWalletID: 0,
        Amount: 0,
        InWords: '',
        Remark: '',
        ReceiverID: 0,
        IsTransfer: false,
        IsGenerateOTP: false
    });
    const reset = () => {
        setInWords('');
        setFormObj({
            MobileNumber: '',
            FirmName: '',
            WalletID: 0,
            SenderWalletID: 0,
            Amount: 0,
            InWords: '',
            Remark: '',
            ReceiverID: 0,
            IsTransfer: false,
            IsGenerateOTP: false
        });
    };

    const TransferValidation = Yup.object({
        Remark: Yup.string().required(PLEASE_PROVIDE_VALID + 'remark'),
        Amount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'amount')
            //  .nullable(PLEASE_SELECT_VALID + 'amount')
            .min(1, PLEASE_PROVIDE_VALID + 'amount')
            .typeError('Amount must be numeric.')
            .optional()
            .nullable(),

        // .moreThan(0, PLEASE_PROVIDE_VALID + 'amount')
        // .required(PLEASE_PROVIDE_VALID + 'amount'),
        WalletID: Yup.number()
            .moreThan(0, PLEASE_SELECT_VALID + ' wallet')
            .required(PLEASE_SELECT_VALID + ' wallet')
    });

    const updateWalletID = async (value, setFieldValue) => {};
    const WalletList = wallets.map(({ WalletID, Name }) => (
        <MenuItem key={WalletID} value={WalletID}>
            {Name}
        </MenuItem>
    ));
    const CustomOtpModal = openOTP ? (
        <OTPModal
            isOpen={openOTP}
            title="OTP"
            error={responseMessage}
            submitCallback={(value) => {
                VerifyOTP(value);
            }}
            resendCallback={(value) => {}}
            onDismissCallback={(value) => {}}
        />
    ) : null;
    const VerifyOTP = async (otpObj) => {
        setLoading(true);
        setOpenOTP(false);

        const { OTP } = otpObj;
        let RequestObj = {
            OTP: OTP,
            MobileNumber: MobileNumber
        };

        var response = await VERIFY_FOUND_TRANSFER_OTP(
            JSON.stringify(RequestObj)
        );
        setLoading(false);

        setResponseMessage(response.data);
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            setOpenOTP(true);
            return;
        }
        let data = JSON.parse(response.data);

        setFormObj({
            ...FormObj,
            ReceiverID: data.UserID,
            FirmName: data.FirmName
        });
        setshowTansfer(true);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data.Message);
    };
    const SelectReport = async () => {
        setLoading(true);
        const response = await SELECT_LAST_10_BY_SENDERID();
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }
        createColumns();
        let data = JSON.parse(response.data);
        setTransferReport(data);
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
    const TransferForm = ({ setFieldValue }) => {
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
                        <Grid item xs={12}>
                            <Paper className={classes.PaperStyle}>
                                <Grid container spacing={2}>
                                    {!showTansfer ? (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <MaterialNumberField
                                                label="Mobile Number"
                                                name="MobileNumber"
                                                length="10"
                                                placeholder="Mobile Number"
                                                title="Mobile Number"
                                            />
                                        </Grid>
                                    ) : null}
                                    {showTansfer && (
                                        <>
                                            {IsW2OWithSenderWallet && (
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <MaterialSelect
                                                        label="Select Sender Wallet"
                                                        name="SenderWalletID"
                                                        title="Select Sender Wallet"
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
                                                        <MenuItem
                                                            key="0"
                                                            value={0}
                                                        >
                                                            Select Wallet
                                                        </MenuItem>
                                                        {WalletList}
                                                    </MaterialSelect>
                                                </Grid>
                                            )}
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <MaterialTextField
                                                    label="FirmName"
                                                    name="FirmName"
                                                    type="text"
                                                    title="FirmName"
                                                    readOnly={true}
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
                                                    label="Select Receiver Wallet"
                                                    name="WalletID"
                                                    title="Select Receiver Wallet"
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
                                                    <MenuItem key="0" value={0}>
                                                        Select Wallet
                                                    </MenuItem>
                                                    {WalletList}
                                                </MaterialSelect>
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
                                                    max={999999}
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
                                                    value={InWords}
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
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={6}
                                                lg={6}
                                            >
                                                <PrimaryButton
                                                    type="submit"
                                                    title="Transfer"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    style={{
                                                        marginTop: '11px'
                                                    }}
                                                    fullWidth
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'IsTransfer',
                                                            true
                                                        );

                                                        setFieldValue(
                                                            'IsGenerateOTP',
                                                            false
                                                        );
                                                    }}
                                                    disabled={loading}
                                                >
                                                    Transfer
                                                </PrimaryButton>
                                                {loading && (
                                                    <CircularProgress
                                                        size={24}
                                                        className={
                                                            classes.buttonProgress
                                                        }
                                                    />
                                                )}
                                            </Grid>
                                        </>
                                    )}
                                    {!showTansfer && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Submit"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                style={{
                                                    marginTop: '11px'
                                                }}
                                                fullWidth
                                                onClick={(e) => {
                                                    setFieldValue(
                                                        'IsGenerateOTP',
                                                        true
                                                    );

                                                    setFieldValue(
                                                        'IsTransfer',
                                                        false
                                                    );
                                                }}
                                                disabled={loading}
                                            >
                                                Submit
                                            </PrimaryButton>
                                            {loading && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </Grid>
                                    )}
                                </Grid>
                            </Paper>
                        </Grid>
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
                        data={transferReport}
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
            isLoading={loading}
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
                //     validationSchema={FormValidationSchema}
                onSubmit={handleSubmit}
                resetForm={FormObj}
            >
                {TransferForm}
            </Formik>
            {CustomOtpModal}
        </CustomContainer>
    );
};

export default RetailerToAllFundTransfer;
