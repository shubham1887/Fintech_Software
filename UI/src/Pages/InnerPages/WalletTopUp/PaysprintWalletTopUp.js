import React, { useEffect, useState, useContext, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { TransactionConfirmation } from 'Components/Alert/TransactionConfirmation';
import { TransactionResponse } from 'Components/Alert/TransactionResponse';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import {
    Transaction as TRANSACTION,
    GenerateOTP as GENERATEOTP
} from 'Components/Api/PaysprintWalletTopUpAPI';
import AsyncOperatorAutoComplete from 'Components/Inputes/AsyncOperatorAutoComplete';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { Last10RechargeReport } from 'Pages/InnerPages/Report/Last10RechargeReport';
import { Container, Grid, Paper, Hidden } from '@material-ui/core';

import { WALLETTOPUP_CODE } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';

import { BalanceContext } from 'Components/BalanceContext';

import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import HOCLoader from 'Components/Loader/HOCLoader';
import InputLabel from 'Components/Inputes/InputLabel';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import RechargeIcon from 'Components/SvgButtons/RechargeIcon';
import ResetIcon from 'Components/SvgButtons/ResetIcon';

const CustomContainer = HOCLoader(Container);

const PaysprintWalletTopUp = () => {
    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }
    const [RefreshRandom, setRefreshRandom] = useState(0);

    const [serviceCode] = useState(WALLETTOPUP_CODE);
    const formRef = useRef();
    const { DashboardBalanceUpdate } = useContext(BalanceContext);

    const classes = commonStyle();

    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);

    const [severity, setSeverity] = useState('success');
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const [isPin] = useState(localStorage.getItem('IsPin'));
    const [InWords, setInWords] = useState('');
    const [Operator, setOperator] = useState([]);
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [ResponseAlert, setResponseAlert] = useState({});
    const [status, setStatus] = useState('PROCESS');
    const [isOTP, setOTP] = useState(false);
    const [isReadOnly, setReadOnly] = useState(false);
    const [formObj, setFormObj] = useState({
        ServiceID: serviceCode,
        MobileNumber: '',
        Amount: '',
        InWords: '',
        Pin: '',
        IsPin: isPin,
        RefernceNo: '',
        OTP: '',
        IsOTPButton: false,
        OperatorName: '',
        OperatorID: 0,
        BeneName: '',
        EmailID: ''
    });
    const [OperatorDefault, setOperatorDefault] = useState({
        OperatorID: 0,
        OperatorName: 'Select Operator'
    });

    const reset = () => {
        setOperatorDefault({
            OperatorID: 0,
            OperatorName: 'Select Operator'
        });
        setInWords('');
        setOTP(false);
        setFormObj({
            ServiceID: serviceCode,
            MobileNumber: '',
            Amount: '',
            InWords: '',
            Pin: '',
            IsPin: isPin,
            RefernceNo: '',
            OTP: '',
            BeneName: '',
            IsOTPButton: false,
            OperatorName: '',
            OperatorID: 0
        });
    };

    const validationSchema = Yup.object({
        MobileNumber: Yup.string()
            .length(10, 'Must be 10 digit')
            .required(PLEASE_PROVIDE_VALID + 'number'),

        Amount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'amount')
            .min(0, PLEASE_PROVIDE_VALID + 'amount')
            .max(100000, PLEASE_PROVIDE_VALID + 'amount'),

        Pin: Yup.number().when(['IsPin'], {
            is: (isPin) => isPin === 'true',
            then: Yup.number().required('Please enter pin')
        })
    });

    const getOTP = async (values) => {
        setResponseMessage('');
        var jsonData = {
            ServiceID: serviceCode,
            MobileNumber: values.MobileNumber,
            Amount: values.Amount,
            Pin: values.Pin,
            OperatorName: values.OperatorName,
            OperatorID: values.OperatorID
        };

        var request = JSON.stringify(jsonData);

        setIsError(false);
        setLoadingOpen(true);
        var response = await GENERATEOTP(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }
        let data = JSON.parse(response.data);

        if (data.Status === 'FAILURE') {
            setSeverity('error');
            setResponseMessage(data.Message);

            setIsError(true);
            return;
        }

        setResponseMessage(data.Message);
        setOTP(true);
        setReadOnly(true);
        setFormObj({
            ServiceID: serviceCode,
            Pin: values.Pin,
            MobileNumber: values.MobileNumber,
            Amount: values.Amount,
            RefernceNo: data.RefernceNo,
            OperatorName: values.OperatorName,
            OperatorID: values.OperatorID,
            EmailID: data.EmailID
        });
    };

    const submitHandle = async (values, { resetForm }) => {
        if (loadingOpen) return;

        if (values.IsOTPButton) {
            getOTP(values);
            return;
        }
        setIsAlertOpen(true);

        setConfirmAlertRequest(values);
    };

    const Proceed = async (values) => {
        setLoadingOpen(true);

        setResponseMessage('');
        setIsError(false);

        let response = '';

        var requestObj = {
            RefernceNo: values.RefernceNo,
            MobileNumber: values.MobileNumber,
            Amount: values.Amount,
            OTP: values.OTP,
            OperatorName: values.OperatorName,
            OperatorID: values.OperatorID,
            BeneName: values.BeneName,
            EmailID: values.EmailID
        };

        var request = JSON.stringify(requestObj);

        response = await TRANSACTION(request);

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);
        if (data.Status === 'FAILURE') {
            setSeverity('error');
            setResponseMessage(data.Message);
            setIsError(true);
            return;
        }

        setSeverity('success');
        setStatus(data.Status);
        setResponseAlert(data);
        setIsResponseAlertOpen(true);
        setIsError(true);

        await DashboardBalanceUpdate();
        setRefreshRandom(Math.random(0));
        reset();
    };

    const OnChange = (value, setFieldValue) => {
        if (value === undefined || value <= 0) return;
        setInWords(NumberToWord(value));
        setFieldValue('InWords', NumberToWord(value));
    };

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
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) setIsError(true);

        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);
            setOperator(data);
        }
    };
    const OnOperatorChange = async (name, value, setFieldValue) => {
        setFieldValue('OperatorName', value.OperatorName);
        setFieldValue('OperatorID', value.OperatorID);
        setOperatorDefault({
            OperatorID: value.OperatorID,
            OperatorName: value.OperatorName
        });
    };
    useEffect(() => {
        fetchConsumerNumberFontSize();
        selectOperator(serviceCode);
    }, [serviceCode]);

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
                validationSchema={validationSchema}
                onSubmit={submitHandle}
                resetForm={formObj}
            >
                {({ values, setFieldValue }) => {
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
                                    <Grid item xs={12} sm={12}>
                                        <Paper className={classes.PaperStyle}>
                                            <Grid container spacing={1}>
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
                                                    <MaterialNumberField
                                                        font_size={
                                                            consumerNumberFontSize
                                                        }
                                                        label="Mobile Number"
                                                        name="MobileNumber"
                                                        icon="mobile"
                                                        placeholder="Mobile Number"
                                                        title="Mobile Number"
                                                        length="10"
                                                        typed_length={1}
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
                                                    <MaterialCurrency
                                                        font_size={
                                                            consumerNumberFontSize
                                                        }
                                                        name="Amount"
                                                        label="Amount"
                                                        placeholder="Amount"
                                                        autoComplete="new-password"
                                                        size="normal"
                                                        max={99999}
                                                        readOnly={isReadOnly}
                                                        title="Amount"
                                                        onCustomCallback={(
                                                            value
                                                        ) => {
                                                            OnChange(
                                                                value,
                                                                setFieldValue
                                                            );
                                                        }}
                                                    />
                                                    <InputLabel
                                                        color="green"
                                                        value={InWords}
                                                    />

                                                    <Hidden xsUp>
                                                        <MaterialTextField
                                                            label="OTPRefernce"
                                                            name="OTPRefernce"
                                                            type="text"
                                                            readOnly={true}
                                                            placeholder="OTPRefernce"
                                                        />
                                                    </Hidden>
                                                </Grid>

                                                {isOTP === true ? (
                                                    <>
                                                        <Grid
                                                            item
                                                            xs={12}
                                                            sm={12}
                                                            md={12}
                                                            lg={12}
                                                        >
                                                            <MaterialTextField
                                                                label="Wallet Holder Name"
                                                                name="BeneName"
                                                                placeholder="Wallet Holder Name"
                                                                title="Wallet Holder Name"
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
                                                                label="OTP"
                                                                name="OTP"
                                                                placeholder="OTP"
                                                                title="OTP"
                                                                length={6}
                                                            />
                                                        </Grid>
                                                    </>
                                                ) : null}
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

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                    spacing={2}
                                                    container
                                                    className={
                                                        classes.footerGrid
                                                    }
                                                >
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <SecondaryButton
                                                            disabled={
                                                                loadingOpen
                                                            }
                                                            endIcon={
                                                                <ResetIcon />
                                                            }
                                                            title="Reset"
                                                            onClick={() => {
                                                                reset();
                                                            }}
                                                        />
                                                    </Grid>

                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        {isOTP === true ? (
                                                            <PrimaryButton
                                                                title="Proceed"
                                                                disabled={
                                                                    loadingOpen
                                                                }
                                                                endIcon={
                                                                    <RechargeIcon />
                                                                }
                                                                onClick={(
                                                                    e
                                                                ) => {
                                                                    setFieldValue(
                                                                        'IsOTPButton',
                                                                        false
                                                                    );
                                                                }}
                                                            />
                                                        ) : (
                                                            <PrimaryButton
                                                                title="Get OTP"
                                                                disabled={
                                                                    loadingOpen
                                                                }
                                                                endIcon={
                                                                    <RechargeIcon />
                                                                }
                                                                onClick={(
                                                                    e
                                                                ) => {
                                                                    setFieldValue(
                                                                        'IsOTPButton',
                                                                        true
                                                                    );
                                                                }}
                                                            />
                                                        )}
                                                    </Grid>
                                                </Grid>
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
                                <Last10RechargeReport
                                    ServiceID={serviceCode}
                                    refreshRandom={RefreshRandom}
                                />
                            </Grid>
                        </Grid>
                    );
                }}
            </Formik>

            {isAlertOpen ? (
                <TransactionConfirmation
                    isOpen={isAlertOpen}
                    title="Wallet TopUp"
                    message="Are you sure to do transaction?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Proceed(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
            {isResponseAlertOpen ? (
                <TransactionResponse
                    isOpen={isResponseAlertOpen}
                    title={status}
                    ServiceID={WALLETTOPUP_CODE}
                    request={ResponseAlert}
                    OkButtonClick={(request) => {
                        setIsResponseAlertOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};

export default PaysprintWalletTopUp;
