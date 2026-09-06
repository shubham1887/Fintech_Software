import {
    Container,
    Grid,
    MenuItem,
    Paper,
    Typography
} from '@material-ui/core';
import { Insert as INSERT } from 'Components/Api/VoucherAPI';
import { SelectByReceiverID as SELECT_PAYMENT_MODE_BYID } from 'Components/Api/PaymentModesAPI';
import { SelectByParentID as SELECT_BY_PARENT_ID } from 'Components/Api/UserBankDetailApi';
import { SelectDownlinForFundTransfer as USER_SELECT_DOWNLINE_FOR_FUND_TRANSFER } from 'Components/Api/UsersApi';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import InputLabel from 'Components/Inputes/InputLabel';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import HOCLoader from 'Components/Loader/HOCLoader';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { LOGOUT } from 'ConstComponents';
import { Field, Form, Formik } from 'formik';
import React, { useEffect, useState, useRef } from 'react';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { SHOW_PAYMODE } from 'Config';
import { FETCH_LIST } from 'Config';
import Divider from '@material-ui/core/Divider';
import {
    PrimaryButton,
    SecondaryButton,
    InfoButton
} from 'Components/Inputes/CustomButton';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import { RECEIPT_VOUCHER_ID } from 'Components/Helper/Messages';
const CustomContainer = HOCLoader(Container);

export const Voucher = () => {
    const history = useHistory();
    const formRef = useRef();
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [downline, setDownline] = useState([]);
    const [paymentModes, setpaymentModes] = useState([]);
    const [IsFetchAllList, setisFetchAllList] = useState(false);
    const [parentBankDetails, setParentBankDetails] = useState([]);
    const [shouldDisplayBankName, setShouldDisplayBankName] = useState(false);
    const [currentUserID] = useState(localStorage.getItem('UserID'));
    const [InWords, setInWords] = useState('');
    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }

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
        Date: new Date(),
        VoucherTypeID: 1,
        User: userDefault,
        ProviderName: '',
        Amount: '',
        CrGroup: '',
        PayMode: '0',
        BankName: 0,
        Remark: ''
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
        PayMode: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Pay Mode')
            .required(PLEASE_SELECT_VALID + 'Pay Mode'),
        Remark: Yup.string().required(PLEASE_SELECT_VALID + 'Remark'),

        Amount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'amount')
            .required(PLEASE_PROVIDE_VALID + 'amount')
    });

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

    useEffect(() => {
        setLoadingOpen(true);
        fetchConsumerNumberFontSize();
        fetchPaymodeOnOff();
        fetchUserList();
        setLoadingOpen(false);
    }, []);
    const updateReceiverID = async (value, setFieldValue) => {
        setUserDefault({
            UserID: value.UserID,
            FirmName: value.FirmName
        });

        selectPaymentModesWallets(value.UserID);
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(false);

        let requestObj = {
            VoucherTypeID: values.VoucherTypeID,
            DrName: userDefault.FirmName,
            UserID: userDefault.UserID,
            Mode: values.PayMode,
            Date: values.Date,
            Amount: values.Amount,
            CrName: values.BankName,
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
        if (d.length > 0) setShouldDisplayBankName(true);
    };

    const reset = () => {
        setInWords('');
        setShouldDisplayBankName(false);
        setUserDefault({
            UserID: 0,
            FirmName: 'Select User'
        });

        setFormObj({
            Date: new Date(),
            VoucherTypeID: RECEIPT_VOUCHER_ID,
            User: userDefault,
            ProviderName: '',
            Amount: '',
            CrGroup: '',
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

        if (value === 'BankTransfer') {
            SelectParentBanks(currentUserID);
            setFieldValue('CrGroup', 'Bank');
            return;
        }
        if (value === 'Cash') {
            setFieldValue('BankName', 'Cash');
            return;
        }
        if (value === 'Google Pay' || value === 'Phone Pe') {
            setFieldValue('BankName', 'Online');
            return;
        }
        setFieldValue('BankName', 'Cash');
        setShouldDisplayBankName(false);
    };

    const PayModeList = paymentModes.map(({ PaymentModeMasterID, Type }) => (
        <MenuItem key={PaymentModeMasterID} value={Type}>
            {Type}
        </MenuItem>
    ));

    const ParentBankDetailList = parentBankDetails.map(
        ({ BankName, AccountNumber }) => (
            <MenuItem key={AccountNumber} value={BankName}>
                {BankName}
            </MenuItem>
        )
    );

    const BankNamesDropDown = shouldDisplayBankName ? (
        <MaterialSelect label="Bank" name="BankName" title="Bank">
            <MenuItem key="0" value="0">
                Select BankName
            </MenuItem>

            {ParentBankDetailList}
        </MaterialSelect>
    ) : null;

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
                                Add Receipt Voucher
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
                                                    <Field
                                                        component={
                                                            AsyncAutoComplete
                                                        }
                                                        defaultValue={
                                                            userDefault
                                                        }
                                                        FieldName="User"
                                                        name="User"
                                                        SubFieldName="UserID"
                                                        options={downline}
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.FirmName}
                                                        textFieldProps={{
                                                            label: 'Select User'
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
                                                            updateReceiverID(
                                                                value,
                                                                setFieldValue
                                                            );
                                                            setUserDefault({
                                                                UserID:
                                                                    value.UserID,
                                                                FirmName:
                                                                    value.FirmName
                                                            });
                                                        }}
                                                    />
                                                </Grid>

                                                {PaymodeOnOff && (
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
                                                )}
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
                                                        max={9999999999}
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
export default Voucher;
