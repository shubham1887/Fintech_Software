import {
    Container,
    Grid,
    MenuItem,
    Paper,
    Typography
} from '@material-ui/core';
import { Insert as INSERT } from 'Components/Api/VoucherAPI';
import { SelectByReceiverID as SELECT_PAYMENT_MODE_BYID } from 'Components/Api/PaymentModesAPI';
import { SelectBankForDropDown as SELECT_BY_PARENT_ID } from 'Components/Api/UserBankDetailApi';
import {
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import InputLabel from 'Components/Inputes/InputLabel';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import HOCLoader from 'Components/Loader/HOCLoader';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { LOGOUT } from 'ConstComponents';
import { Form, Formik } from 'formik';
import React, { useEffect, useState, useRef } from 'react';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import { SHOW_PAYMODE } from 'Config';
import Divider from '@material-ui/core/Divider';
import {
    PrimaryButton,
    SecondaryButton,
    InfoButton
} from 'Components/Inputes/CustomButton';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import { CONTRA_VOUCHER_ID, CASH } from 'Components/Helper/Messages';
const CustomContainer = HOCLoader(Container);

export const AddContraVoucher = () => {
    const history = useHistory();
    const formRef = useRef();
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [paymentModes, setpaymentModes] = useState([]);
    const [parentBankDetails, setParentBankDetails] = useState([]);
    const [
        shouldTransferDisplayBankName,
        setshouldTransferDisplayBankName
    ] = useState(false);
    const [currentUserID] = useState(localStorage.getItem('UserID'));
    const [InWords, setInWords] = useState('');
    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }
    const fetchPaymodeOnOff = async () => {
        let isOn = await SHOW_PAYMODE();
        if (formRef.current) {
            formRef.current.values.PayMode = 'Cash';
        }
        if (isOn === 'YES') {
            if (formRef.current) {
                formRef.current.values.PayMode = '0';
            }
            return;
        }
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
    const [formObj, setFormObj] = useState({
        Date: new Date(),
        VoucherTypeID: CONTRA_VOUCHER_ID,
        CrGroup: 'Exclusive',
        CrName: '',
        ProviderName: '',
        Amount: '',
        PayMode: '0',
        BankName: 0,
        TransferBank: CASH,
        Remark: ''
    });

    const validationSchema = Yup.object({
        BankName: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Bank Name')
            .required(PLEASE_SELECT_VALID + 'Bank Name'),
        PayMode: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Pay Mode')
            .required(PLEASE_SELECT_VALID + 'Pay Mode'),
        Remark: Yup.string().required(PLEASE_SELECT_VALID + 'Remark'),

        Amount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'amount')
            .required(PLEASE_PROVIDE_VALID + 'amount')
    });

    useEffect(() => {
        setLoadingOpen(true);
        fetchConsumerNumberFontSize();
        fetchPaymodeOnOff();
        SelectParentBanks(currentUserID);
        setshouldTransferDisplayBankName(false);
        selectPaymentModesWallets(currentUserID);
        setLoadingOpen(false);
    }, []);

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(false);
        let requestObj = {
            VoucherTypeID: values.VoucherTypeID,
            CrGroup: values.PayMode,
            CrName: values.BankName,
            DrGroup: values.PayMode,
            DrName: values.TransferBank,
            Date: values.Date,
            Amount: values.Amount,
            Remark: values.Remark
        };

        setLoadingOpen(true);
        const response = await INSERT(JSON.stringify(requestObj));
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setSeverity('error');
            setIsError(true);
            return;
        }
        reset();
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
    };

    const reset = () => {
        setInWords('');

        setshouldTransferDisplayBankName(false);
        setFormObj({
            Date: new Date(),
            VoucherTypeID: 4,
            ProviderName: '',
            Amount: '',
            PayMode: '0',
            BankName: 0,
            Remark: ''
        });
    };

    const OnAmountChangeCore = (value, setFieldValue) => {
        if (value === undefined || value === null || value <= 0) {
            setFieldValue('InWords');
            setInWords('');
            return;
        }

        let wwords = NumberToWord(value);
        setInWords(wwords);
        setFieldValue('InWords', wwords);
    };

    const handlePayModeChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
        setshouldTransferDisplayBankName(false);
        if (value === 'BankTransfer') {
            SelectParentBanks(currentUserID);
            setshouldTransferDisplayBankName(true);
            return;
        }
        if (value === 'CashDeposit') {
            SelectParentBanks(currentUserID);
            setFieldValue('TransferBank', 'Cash');

            return;
        }
        if (value === 'Google Pay' || value === 'Phone Pe') {
            setFieldValue('TransferBank', 'Online');
            return;
        }
        setshouldTransferDisplayBankName(false);
    };

    const PayModeList = paymentModes.map(({ PaymentModeMasterID, Type }) => (
        <MenuItem key={PaymentModeMasterID} value={Type}>
            {Type}
        </MenuItem>
    ));

    const SenderBankDetailList = parentBankDetails.map(
        ({ BankName, AccountNumber }) => (
            <MenuItem key={AccountNumber} value={BankName}>
                {BankName}
            </MenuItem>
        )
    );

    const TransferToBankDetailList = parentBankDetails.map(
        ({ BankName, AccountNumber }) => (
            <MenuItem key={AccountNumber} value={BankName}>
                {BankName}
            </MenuItem>
        )
    );

    const BankNamesDropDown = (
        <MaterialSelect label="Transfer To Bank" name="BankName" title="Bank">
            <MenuItem key="0" value="0">
                Select Bank
            </MenuItem>

            {SenderBankDetailList}
        </MaterialSelect>
    );
    const TransferBankNameDropDown = shouldTransferDisplayBankName && (
        <MaterialSelect
            label="Transfer From Bank"
            name="TransferBank"
            title="Bank"
        >
            <MenuItem key="0" value="0">
                Select TransferBank From
            </MenuItem>

            {TransferToBankDetailList}
        </MaterialSelect>
    );

    const TransferForm = ({ values, errors, setFieldValue }) => {
        return (
            <>
                <Paper style={{ padding: '30px' }}>
                    <Grid container>
                        <Grid item sm={12} md={12} lg={12}>
                            <Typography
                                variant="h4"
                                component="h1"
                                gutterBottom
                                style={{
                                    borderBottom: '1px solid #000',
                                    margin: '16px'
                                }}
                            >
                                Add Contra Voucher
                            </Typography>
                        </Grid>
                    </Grid>
                    <Grid container spacing={2}>
                        <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                            <CustomContainer
                                isLoading={loadingOpen}
                                isError={isError}
                                alertMessage={responseMessage}
                                severity={severity}
                                onCloseCallback={(alertState) => {
                                    setIsError(alertState);
                                }}
                            >
                                <Form>
                                    <Grid
                                        spacing={5}
                                        container
                                        style={{
                                            justifyContent: 'center'
                                        }}
                                    >
                                        <Grid
                                            item
                                            xs={4}
                                            sm={4}
                                            md={4}
                                            lg={4}
                                            xl={4}
                                        >
                                            <Grid
                                                container
                                                spacing={2}
                                                sx={{
                                                    justifyContent: 'center'
                                                }}
                                            >
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                    xl={12}
                                                >
                                                    <MaterialDatePickerField
                                                        name="Date"
                                                        label="Date"
                                                        maxDate={new Date()}
                                                    />
                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                    xl={12}
                                                >
                                                    <MaterialSelect
                                                        label="Pay Mode"
                                                        name="PayMode"
                                                        title="Select Mode"
                                                        onCustomCallback={(
                                                            name,
                                                            value
                                                        ) => {
                                                            handlePayModeChange(
                                                                name,
                                                                value,
                                                                setFieldValue
                                                            );
                                                        }}
                                                    >
                                                        <MenuItem
                                                            key="0"
                                                            value="0"
                                                        >
                                                            Select Mode
                                                        </MenuItem>
                                                        {PayModeList}
                                                    </MaterialSelect>
                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                    xl={12}
                                                >
                                                    {BankNamesDropDown}
                                                </Grid>

                                                {shouldTransferDisplayBankName && (
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={12}
                                                        lg={12}
                                                        xl={12}
                                                    >
                                                        {
                                                            TransferBankNameDropDown
                                                        }
                                                    </Grid>
                                                )}

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                    xl={12}
                                                >
                                                    <MaterialCurrency
                                                        font_size={
                                                            consumerNumberFontSize
                                                        }
                                                        name="Amount"
                                                        label="Amount"
                                                        placeholder="Amount"
                                                        autoComplete="new-password"
                                                        max={9999999}
                                                        onCustomCallback={(
                                                            value
                                                        ) => {
                                                            OnAmountChangeCore(
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
                                                    xl={12}
                                                >
                                                    <MaterialTextField
                                                        label="Remark"
                                                        name="Remark"
                                                        type="text"
                                                        length={150}
                                                        title="Remark"
                                                    />
                                                </Grid>
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
                                            >
                                                <Grid container spacing={2}>
                                                    <Grid
                                                        item
                                                        sm={12}
                                                        md={12}
                                                        lg={4}
                                                        xl={4}
                                                    >
                                                        <PrimaryButton
                                                            type="submit"
                                                            title="Submit"
                                                            sx={{
                                                                marginTop: 3,
                                                                height: '50px',
                                                                textTransform:
                                                                    'none',
                                                                paddingX:
                                                                    '50px',
                                                                borderRadius:
                                                                    '4px'
                                                            }}
                                                        />
                                                    </Grid>
                                                    <Grid
                                                        item
                                                        sm={12}
                                                        md={12}
                                                        lg={4}
                                                        xl={4}
                                                    >
                                                        <SecondaryButton
                                                            type="reset"
                                                            variant="contained"
                                                            color="secondary"
                                                            title="Reset"
                                                            sx={{
                                                                marginTop: 3,
                                                                height: '50px',
                                                                textTransform:
                                                                    'none',
                                                                paddingX:
                                                                    '50px',
                                                                borderRadius:
                                                                    '4px'
                                                            }}
                                                        />
                                                    </Grid>
                                                    <Grid
                                                        item
                                                        sm={12}
                                                        md={12}
                                                        lg={4}
                                                        xl={4}
                                                    >
                                                        <InfoButton
                                                            type="button"
                                                            variant="contained"
                                                            color="primary"
                                                            title="Request"
                                                            sx={{
                                                                marginTop: 3,
                                                                height: '50px',
                                                                textTransform:
                                                                    'none',
                                                                paddingX:
                                                                    '50px',
                                                                borderRadius:
                                                                    '4px'
                                                            }}
                                                            onClick={() => {
                                                                history.push(
                                                                    '/ApproveVoucher'
                                                                );
                                                                return;
                                                            }}
                                                        />
                                                    </Grid>
                                                </Grid>
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                </Form>
                            </CustomContainer>
                        </Grid>
                    </Grid>
                </Paper>
            </>
        );
    };

    return (
        <Formik
            innerRef={formRef}
            enableReinitialize={true}
            initialValues={formObj}
            validationSchema={validationSchema}
            onSubmit={handleSubmit}
            resetForm={formObj}
        >
            {TransferForm}
        </Formik>
    );
};
export default AddContraVoucher;
