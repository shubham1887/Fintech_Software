import React, { useEffect, useState, useContext, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { TransactionConfirmation } from 'Components/Alert/TransactionConfirmation';
import { BBPSTransactionResponse } from 'Components/Alert/BBPSTransactionResponse';
import BBPS_LOGO from 'Assets/Images/bbpslogo.jpg';
import Box from '@material-ui/core/Box';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { InputAdornment } from '@material-ui/core';

import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import {
    FAST_TAG_CODE,
    POSTPAID_RECHARGE_CODE
} from 'Components/Helper/Messages';
import AsyncOperatorAutoComplete from 'Components/Inputes/AsyncOperatorAutoComplete';
import { Select as BBPS_OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { SelectByOperatorID as BBPS_CONFIG_SELECT_BY_OPERATORID } from 'Components/Api/BBPSConfigurationAPI';
import { BillFetch as BILL_FETCH } from 'Components/Api/UtilityAPI';
import { Recharge as TRANSACTION_RECHARGE } from 'Components/Api/TransactionAPI';

import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import BBPSConfigInput from 'Components/Inputes/BBPSConfigInput';
import { MaterialAlphaNumberField } from 'Components/Inputes/FormikMaterialAlphaNumberInput';
import { Container, Grid, Paper, Typography } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyFieldUtility';
import { AEPSPrint } from 'Components/Print/AEPSPrint';
import { GetBBPSTransctionStatus as TRANSACTION_STATUS } from 'Components/Api/TransactionAPI';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    ELECTRICITY_CODE
} from 'Components/Helper/Messages';
import { UtilityViewConfirmation } from 'Components/Alert/UtilityViewConfirmation';
import { BalanceContext } from 'Components/BalanceContext';
import HOCLoader from 'Components/Loader/HOCLoader';
import { Event } from 'ReactGa';
import { Last10BBPSReport } from 'Pages/InnerPages/Report/Last10BBPSReport';
import InputLabel from 'Components/Inputes/InputLabel';
import ResetIcon from 'Components/SvgButtons/ResetIcon';
import InfoIcon from 'Components/SvgButtons/InfoIcon';
import PayIcon from 'Components/SvgButtons/PayIcon';
import Divider from '@material-ui/core/Divider';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import Accordion from '@material-ui/core/Accordion';
import { SelectForReceipt as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { BBPSPrintV2 } from 'Components/Print/BBPSPrintV2';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const Utility = (props) => {
    const classes = commonStyle();
    const formRef = useRef();
    var IsMobikwikBBPS = localStorage.getItem('IsMobikwikBBPS') === 'true';
    const { serviceID, setRefreshRandom } = props;
    const { DashboardBalanceUpdate, SelectTodaysCommission } = useContext(
        BalanceContext
    );
    const [Operator, setOperator] = useState([]);
    const [OperatorConfig, setOperatorConfig] = useState([]);

    const [isBBPS, setIsBBPS] = useState(false);

    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [BeneficiaryName, setBeneficiaryName] = useState('');
    const [BBPS_CustomerName, setBBPS_CustomerName] = useState('');
    const [BillDate, setBillDate] = useState('');
    const [BillerID, setBillerID] = useState('');
    const [isError, setIsError] = useState(false);
    const [isViewOpen, setIsViewOpen] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [ResponseAlert, setResponseAlert] = useState({});
    const [status, setStatus] = useState('PROCESS');
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [InWords, setInWords] = useState('');
    const [Amount, setAmount] = useState('');
    const [minAmount, setMinAmount] = useState(0);
    const [maxAmount, setMaxAmount] = useState(100000);
    const [IsAmountEnabled, setIsAmountEnabled] = useState(false);
    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    const [openFilter, setopenFilter] = useState(false);
    const [isPrintOpen, setIsPrintOpen] = useState(false);

    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }
    const [currentAccountName, setCurrentAccountName] = useState(
        'ConsumerNumber'
    );
    const [currentPlaceHolder, setcurrentPlaceHolder] = useState(
        'Consumer Number'
    );
    const [isPin] = useState(localStorage.getItem('IsPin'));
    const [letterCase, setLetterCase] = useState('none');
    const [number, setNumber] = useState('none');
    const [isBillFetch, setIsBillFetch] = useState(false);
    const [printData, setPrintData] = useState(null);
    const [OperatorDefault, setOperatorDefault] = useState({
        OperatorID: 0,
        OperatorName: 'Biller Name'
    });

    const [formObj, setFormObj] = useState({
        ServiceID: serviceID,
        Operator: OperatorDefault,
        ConsumerNumber: '',
        Label: '',
        MobileNo: '',
        Amount: Amount,
        BBPSViewID: '',
        InWords: '',
        IsCustomerInfoButton: false,
        OperatorName: '',
        OperatorImagePath: '',
        Param1: '',
        IsParam1Required: false,
        Param2: '',
        Param3: '',
        Param4: '',
        Param5: '',
        Pin: '',
        IsPin: isPin,
        OperatorID: 0,
        BeneficiaryName: '',
        BBPS_CustomerName: '',
        BillDate: '',
        BillerID: ''
    });

    const [customerInfo, setCustomerInfo] = useState({
        CustomerName: '',
        DueDate: '',
        Amount: '',
        Message: '',
        OperatorName: '',
        OperatorImagePath: '',
        BBPSViewID: '',
        ResponseParam1: '',
        ResponseParam2: '',
        ResponseParam3: '',
        ResponseParam4: '',
        ResponseParam5: '',
        IsBBPS: isBBPS
    });

    const validationSchema = Yup.object({
        Operator: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator')
            .shape({
                OperatorID: Yup.number()
                    .notOneOf([0], PLEASE_SELECT_VALID + 'operator')
                    .nullable(PLEASE_SELECT_VALID + 'operator')
                    .required(PLEASE_SELECT_VALID + 'operator')
            }),

        //ConsumerNumber: Yup.string().required(PLEASE_PROVIDE_VALID + 'number'),

        IsCustomerInfoButton: Yup.boolean(),

        Pin: Yup.number().when(['IsPin', 'IsCustomerInfoButton'], {
            is: (isPin, IsCustomerInfoButton) =>
                isPin === 'true' && !IsCustomerInfoButton,
            then: Yup.number().required('Please enter pin')
        }),

        Amount: Yup.number().when(['IsCustomerInfoButton'], {
            is: (IsCustomerInfoButton) => !IsCustomerInfoButton,
            then: Yup.number()
                .required(PLEASE_PROVIDE_VALID + 'amount')
                .min(
                    minAmount,
                    PLEASE_PROVIDE_VALID +
                        'amount Minimum' +
                        minAmount +
                        ' is required'
                )
                .max(
                    maxAmount,
                    PLEASE_PROVIDE_VALID +
                        'amount maximum ' +
                        maxAmount +
                        ' is required'
                )
        })
    });

    const submitHandle = async (values, { resetForm }) => {
        values = GetConfigurationValues(values, OperatorConfig);

        if (values.IsCustomerInfoButton) {
            GetCustomerInfo(values);
            return;
        }

        let op = Operator.filter(
            (o) => o.OperatorID === values.Operator.OperatorID
        );

        values.OperatorName = op[0].OperatorName;
        values.OperatorImagePath = op[0].Path;
        values.BeneficiaryName = BeneficiaryName;
        values.BBPS_CustomerName = BBPS_CustomerName;
        values.BillDate = BillDate;
        values.BillerID = BillerID;
        //values.Param1 = BBPS_CustomerName;

        let TakeConfigParamFromViewResponse = OperatorConfig.filter(
            (o) => o.IsDisplay === false
        );

        TakeConfigParamFromViewResponse.map((item) => {
            const paramName = labelToParamMap[item.Label];

            const ResponseValue = customerInfo[item.Label];
            if (ResponseValue && ResponseValue.length > 0) {
                values[paramName] = ResponseValue;
            }
        });

        setIsAlertOpen(true);
        setConfirmAlertRequest(values);
    };

    const getStringFromObject = (inObject) => {
        if (typeof inObject === 'object') {
            return inObject.value;
        }
        return inObject;
    };

    const GetConfigurationValues = (values, OperatorConfig) => {
        if (OperatorConfig.length <= 0) return values;

        values.Param1 = getStringFromObject(values[OperatorConfig[0].Label]);
        values.IsParam1Required = OperatorConfig[0].IsRequired;

        values.Param2 =
            OperatorConfig[1] !== undefined
                ? getStringFromObject(values[OperatorConfig[1].Label])
                : '';
        values.Param3 =
            OperatorConfig[2] !== undefined
                ? getStringFromObject(values[OperatorConfig[2].Label])
                : '';

        values.Param4 =
            OperatorConfig[3] !== undefined
                ? getStringFromObject(values[OperatorConfig[3].Label])
                : '';

        values.Param5 =
            OperatorConfig[4] !== undefined
                ? getStringFromObject(values[OperatorConfig[4].Label])
                : '';

        return values;
    };

    const BBPSValidation = (values, OperatorConfig) => {
        let response = {
            status: true,
            message: ''
        };

        if (OperatorConfig.length <= 0) return response;

        if (
            OperatorConfig[0] !== undefined &&
            OperatorConfig[0].IsRequired === true
        ) {
            if (
                values.Param1 === undefined ||
                values.Param1 === '' ||
                values.Param1 === null
            )
                return {
                    status: false,
                    message: OperatorConfig[0].ErrorMeeage
                };
        }
        if (
            OperatorConfig[1] !== undefined &&
            OperatorConfig[1].IsRequired === true
        ) {
            if (
                values.Param2 === undefined ||
                values.Param2 === '' ||
                values.Param2 === null
            )
                return {
                    status: false,
                    message: OperatorConfig[1].ErrorMeeage
                };
        }
        if (
            OperatorConfig[2] !== undefined &&
            OperatorConfig[2].IsRequired === true
        ) {
            if (
                values.Param3 === undefined ||
                values.Param3 === '' ||
                values.Param3 === null
            )
                return {
                    status: false,
                    message: OperatorConfig[2].ErrorMeeage
                };
        }
        if (
            OperatorConfig[3] !== undefined &&
            OperatorConfig[3].IsRequired === true
        ) {
            if (
                values.Param4 === undefined ||
                values.Param4 === '' ||
                values.Param4 === null
            )
                return {
                    status: false,
                    message: OperatorConfig[3].ErrorMeeage
                };
        }
        if (
            OperatorConfig[4] !== undefined &&
            OperatorConfig[4].IsRequired === true
        ) {
            if (
                values.Param5 === undefined ||
                values.Param5 === '' ||
                values.Param5 === null
            )
                return {
                    status: false,
                    message: OperatorConfig[4].ErrorMeeage
                };
        }
        return response;
    };

    const labelToParamMap = {
        ResponseParam1: 'Param1',
        ResponseParam2: 'Param2',
        ResponseParam3: 'Param3',
        ResponseParam4: 'Param4',
        ResponseParam5: 'Param5'
    };

    const GetCustomerInfo = async (values) => {
        setLoadingOpen(true);
        customerInfo.Amount = '';
        customerInfo.BBPSViewID = '';

        const {
            status: validationStatus,
            message: errorMessage
        } = BBPSValidation(values, OperatorConfig);

        setLoadingOpen(false);

        if (!validationStatus) {
            setSeverity('error');
            setResponseMessage(errorMessage);

            setIsError(true);
            return;
        }

        setLoadingOpen(true);

        let param1value = '';
        if (typeof values.Param1 === 'object') {
            param1value = values.Param1.value;
        } else {
            param1value = values.Param1;
        }

        let param2value = '';
        if (typeof values.Param2 === 'object') {
            param2value = values.Param2.value;
        } else {
            param2value = values.Param2;
        }

        let param3value = '';
        if (typeof values.Param3 === 'object') {
            param3value = values.Param3.value;
        } else {
            param3value = values.Param3;
        }
        let param4value = '';
        if (typeof values.Param4 === 'object') {
            param4value = values.Param4.value;
        } else {
            param4value = values.Param4;
        }
        let param5value = '';
        if (typeof values.Param5 === 'object') {
            param5value = values.Param5.value;
        } else {
            param5value = values.Param5;
        }

        var requestData = {
            ServiceID: values.ServiceID,
            OperatorID: values.Operator.OperatorID,
            ConsumerNumber: values.ConsumerNumber,
            MobileNo: values.MobileNo,
            Amount: '',
            InWords: '',
            IsCustomerInfoButton: true,
            OperatorName: '',
            OperatorImagePath: '',
            Param1: param1value,
            IsParam1Required: values.IsParam1Required,
            Param2: param2value,
            Param3: param3value,
            Param4: param4value,
            Param5: param5value,
            Pin: values.Pin,
            IsPin: values.IsPin,
            Label: currentAccountName
        };

        var request = JSON.stringify(requestData);
        setResponseMessage('');
        const { isAuthorized, status, data } = await BILL_FETCH(request);

        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        setIsError(true);

        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);
            return;
        }

        let op = Operator.filter(
            (o) => o.OperatorID === values.Operator.OperatorID
        );

        let d = JSON.parse(data);
        d.OperatorName = op[0].OperatorName;
        d.OperatorImagePath = op[0].Path;

        d.IsBBPS = isBBPS;
        setCustomerInfo(d);
        setBBPS_CustomerName(d.CustomerName);
        setBillDate(d.BillDate);
        setBillerID(d.BillerID);
        setBeneficiaryName(d.CustomerName);
        setIsViewOpen(true);
    };

    const GetOperatorName = (OperatorID) => {
        var opName = '';
        opName = Operator.find(function (values) {
            return values.OperatorID === OperatorID ? values.OperatorName : '';
        });

        return opName.OperatorName;
    };

    const Recharge = async (values) => {
        setLoadingOpen(true);

        setIsError(false);

        Event(
            'Utility',
            'Transaction',
            GetOperatorName(values.Operator.OperatorID)
        );
        setResponseMessage('');

        var requestData = {
            ServiceID: values.ServiceID,
            OperatorID: values.Operator.OperatorID,
            ConsumerNumber: values.ConsumerNumber,
            MobileNo: values.MobileNo,
            Amount: values.Amount,
            BBPSViewID: customerInfo.BBPSViewID,
            InWords: values.InWords,
            IsCustomerInfoButton: values.IsCustomerInfoButton,
            OperatorName: values.OperatorName,
            OperatorImagePath: values.OperatorImagePath,
            Param1: values.Param1,
            IsParam1Required: values.IsParam1Required,
            Param2: values.Param2,
            Param3: values.Param3,
            Param4: values.Param4,
            Param5: values.Param5,
            Pin: values.Pin,
            IsPin: values.IsPin,
            BeneficiaryName: values.BeneficiaryName,
            BBPS_CustomerName: values.BBPS_CustomerName,
            BillDate: values.BillDate,
            BillerID: values.BillerID,
            Label: currentAccountName
        };

        var request = JSON.stringify(requestData);

        var response = await TRANSACTION_RECHARGE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSeverity('success');
        setRefreshRandom(Math.random());

        if (!response.status) {
            let data = JSON.parse(response.data);
            setSeverity('error');
            setResponseMessage(data.Message);

            setIsError(true);

            return;
        }

        var data = JSON.parse(response.data);
        if (formRef.current) formRef.current.resetForm();
        setInWords('');
        setBeneficiaryName('');
        setBBPS_CustomerName('');
        setBillDate('');
        setBillerID('');

        setResponseMessage(data.Message);

        setStatus(data.Status);
        data.IsBBPS = isBBPS;
        setResponseAlert(data);
        setIsResponseAlertOpen(true);

        await DashboardBalanceUpdate();

        if (!response.status) {
            setSeverity('error');
            return;
        }

        if (data.Status === 'FAILURE') setSeverity('error');
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

        const response = await BBPS_OPERATOR_SELECT_BY_SERVICEID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) setIsError(true);

        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);

            setOperator(data);
        }
    };

    useEffect(() => {
        fetchConsumerNumberFontSize();
        if (serviceID === FAST_TAG_CODE) {
            setLetterCase('uppercase');
        }
        if (serviceID === POSTPAID_RECHARGE_CODE) {
            setNumber('number');
        }
        selectOperator(serviceID);
    }, [serviceID]);

    const ResetForm = () => {
        setInWords('');
        setOperatorDefault({
            OperatorID: 0,
            OperatorName: 'Biller Name'
        });
        setFormObj({
            ...formObj,
            ConsumerNumber: '',
            MobileNo: '',
            Amount: '',
            InWords: '',
            IsCustomerInfoButton: false,
            OperatorName: '',
            OperatorImagePath: '',
            Param1: '',
            Param2: '',
            Param3: '',
            Param4: '',
            Param5: ''
        });
    };

    const OnOperatorChange = async (name, value, setFieldValue) => {
        ResetForm();
        setIsBillFetch(false);
        let OperatorID = value.OperatorID; //event.target.value;
        setOperatorConfig([]);

        setOperatorDefault({
            OperatorID: value.OperatorID,
            OperatorName: value.OperatorName
        });

        setFieldValue(name, value);

        let obj = Operator.find(function (op) {
            return op.OperatorID === OperatorID ? op.IsBBPS : false;
        });

        if (obj === undefined || obj === null) return;

        setMinAmount(obj.MinAmount ? obj.MinAmount : minAmount);
        setMaxAmount(obj.MaxAmount ? obj.MaxAmount : maxAmount);

        setIsAmountEnabled(!obj.IsPartialBill && obj.IsBBPS);
        setIsBBPS(obj.IsBBPS);

        selectOperatorConfig(OperatorID);

        setCurrentAccountName(
            obj.AccountName ? obj.AccountName : 'ConsumerNumber'
        );
        setcurrentPlaceHolder(obj.Remark ? obj.Remark : 'Consumer Number');

        setIsBillFetch(obj.IsBillFetch);
    };

    const Print = (VendorID) => {
        SelectPrintData(VendorID);
    };

    const SelectPrintData = async (TransactionMasterID) => {
        var req = {
            ID: TransactionMasterID
        };
        var request = JSON.stringify(req);
        setLoadingOpen(true);
        const { isAuthorized, status, data } = await TRANSACTION_RECEIPT(
            request
        );

        setLoadingOpen(false);
        if (!isAuthorized) LOGOUT();
        var d = JSON.parse(data);
        setPrintData(d);
        setIsPrintOpen(true);
    };

    const selectOperatorConfig = async (OperatorID) => {
        var requestObj = {
            OperatorID: OperatorID
        };

        var request = JSON.stringify(requestObj);

        const response = await BBPS_CONFIG_SELECT_BY_OPERATORID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);

        setOperatorConfig(data);
    };

    const StatusCheck = async (ID) => {
        setResponseMessage('');
        var inputObj = {
            ID: ID
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoadingOpen(true);

        var response = await TRANSACTION_STATUS(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        let data = JSON.parse(response.data);

        if (data.Status !== 'Ok') {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data.Message);
            return;
        }

        // if (data.IsOTP === true) {
        //     setTransactionMasterID(ID);
        //     return;
        // }
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data.Message);
    };

    const BBPSLogo = ({ isBBPS }) => {
        return isBBPS && <BBPSLogoImage />;
    };

    const BBPSLogoImage = () => {
        return (
            <img
                src={BBPS_LOGO}
                height={25}
                width={90}
                style={{ float: 'right' }}
                alt="BBPS"
            />
        );
    };

    const UtilityForm = ({ setFieldValue, values, errors }) => {
        return (
            <Form>
                <Paper className={classes.PaperStyle}>
                    <Grid container spacing={2}>
                        <Grid item xs={12} sm={12} md={12}>
                            <Field
                                component={AsyncOperatorAutoComplete}
                                defaultValue={OperatorDefault}
                                ImagePath={'Path'}
                                SubFieldName="OperatorID"
                                FieldName="Operator"
                                name="Operator"
                                options={Operator}
                                getOptionLabel={(option) => option.OperatorName}
                                getOptionSelected={(option, value) =>
                                    option.OperatorName === value.OperatorName
                                }
                                textFieldProps={{
                                    label: 'Biller Name'
                                }}
                                onCustomCallback={(name, value) => {
                                    OnOperatorChange(
                                        name,
                                        value,
                                        setFieldValue
                                    );
                                }}
                            />
                        </Grid>
                        <Grid item xs={12} sm={12} md={12} lg={12}>
                            <MaterialAlphaNumberField
                                name="ConsumerNumber"
                                //placeholder={currentPlaceHolder}
                                title={currentAccountName}
                                label={currentAccountName}
                                letter_case={letterCase}
                                number={number}
                                icon="mobile"
                                typed_length={1}
                                length={50}
                            />
                        </Grid>
                        {serviceID !== 14 && (
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <MaterialTextField
                                    label="Mobile Number"
                                    name="MobileNo"
                                    type="text"
                                    length={10}
                                    placeholder="Mobile Number"
                                />
                            </Grid>
                        )}

                        {OperatorConfig &&
                            OperatorConfig.map(
                                (
                                    {
                                        Type,
                                        Label,
                                        PlaceHolder,
                                        ConfigList,
                                        IsDisplay
                                    },
                                    index
                                ) => (
                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={12}
                                        lg={12}
                                        key={index}
                                    >
                                        <BBPSConfigInput
                                            key={index}
                                            type={Type}
                                            name={Label}
                                            title={Label}
                                            label={Label}
                                            IsDisplay={IsDisplay}
                                            placeholder={PlaceHolder}
                                            list={ConfigList || []}
                                        />
                                    </Grid>
                                )
                            )}

                        <Grid item xs={12} sm={12} md={7} lg={7}>
                            <MaterialCurrency
                                name="Amount"
                                disabled={IsAmountEnabled}
                                placeholder="Amount(₹)"
                                autoComplete="new-password"
                                max={1000000}
                                title="Amount(₹)"
                                onCustomCallback={(value) => {
                                    OnChange(value, setFieldValue);
                                }}
                                InputProps={{
                                    startAdornment: (
                                        <InputAdornment position="start">
                                            <Typography
                                                variant="h6"
                                                style={{
                                                    fontWeight: 'bold',
                                                    color: 'black'
                                                }}
                                            >
                                                ₹
                                            </Typography>
                                        </InputAdornment>
                                    )
                                }}
                            />
                            <InputLabel color="green" value={InWords} />
                        </Grid>

                        {isBillFetch === true ? (
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <PrimaryButton
                                    title="Fetch Bill"
                                    disabled={loadingOpen}
                                    onClick={(e) => {
                                        setFieldValue(
                                            'IsCustomerInfoButton',
                                            true
                                        );
                                    }}
                                />
                            </Grid>
                        ) : null}

                        {isPin === 'true' ? (
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <MaterialNumberField
                                    name="Pin"
                                    type="password"
                                    placeholder="Pin"
                                    label="Pin"
                                    title="Pin"
                                    length={4}
                                />
                            </Grid>
                        ) : null}
                    </Grid>

                    <Divider variant="middle" className="rechdevider" />

                    <Grid item container xs={12}>
                        <p className="description">{BeneficiaryName}</p>
                    </Grid>

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
                                title="Reset"
                                onClick={() => {
                                    ResetForm();
                                }}
                            />
                        </Grid>
                        <Grid item xs={12} sm={12} md={6} lg={6}>
                            <PrimaryButton
                                title="Pay"
                                onClick={(e) => {
                                    setFieldValue(
                                        'IsCustomerInfoButton',
                                        false
                                    );
                                }}
                                className={classes.CustomPrimaryButtonStyle}
                                disabled={loadingOpen}
                            />
                        </Grid>
                    </Grid>
                </Paper>
            </Form>
        );
    };
    const handleCloseCallback = (alertState) => {
        setIsError(alertState);
    };

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={handleCloseCallback}
        >
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
                {IsMobikwikBBPS && (
                    <Grid item xs={12} sm={12} md={3} lg={4}>
                        {' '}
                    </Grid>
                )}

                <Grid item xs={12} sm={12} md={6} lg={4}>
                    <Formik
                        innerRef={formRef}
                        enableReinitialize={true}
                        initialValues={formObj}
                        validationSchema={validationSchema}
                        onSubmit={submitHandle}
                        resetForm={formObj}
                    >
                        {UtilityForm}
                    </Formik>
                </Grid>

                {IsMobikwikBBPS ? (
                    <Grid
                        item
                        sx={12}
                        sm={12}
                        md={12}
                        lg={12}
                        style={{
                            marginTop: '10%'
                        }}
                    >
                        <CustomAccordion
                            title={'Last Transactions'}
                            open={openFilter}
                            setOpen={(value) => setopenFilter(value)}
                        >
                            <Grid
                                item
                                sx={12}
                                sm={12}
                                md={12}
                                lg={12}
                                className="lastrecharges"
                            >
                                <Last10BBPSReport
                                    ServiceID={serviceID}
                                    refreshRandom={1}
                                />
                            </Grid>
                        </CustomAccordion>
                    </Grid>
                ) : (
                    <Grid
                        item
                        sx={12}
                        sm={12}
                        md={6}
                        lg={8}
                        className="lastrecharges"
                    >
                        <Last10BBPSReport
                            ServiceID={serviceID}
                            refreshRandom={1}
                        />
                    </Grid>
                )}
            </Grid>

            {isAlertOpen && (
                <TransactionConfirmation
                    isOpen={isAlertOpen}
                    title="Transaction"
                    message="Do you want to proceed with the transaction?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Recharge(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            )}
            {isPrintOpen ? (
                <BBPSPrintV2
                    isOpen={isPrintOpen}
                    title="BBPS Print"
                    request={ResponseAlert}
                    positiveButtonClick={() => {
                        setIsPrintOpen(false);
                    }}
                    negativeButtonClick={() => {
                        setIsPrintOpen(false);
                    }}
                />
            ) : null}

            {isViewOpen && (
                <UtilityViewConfirmation
                    isOpen={isViewOpen}
                    title="Information"
                    message=""
                    request={customerInfo}
                    positiveButtonTitle={'OK'}
                    positiveButtonClick={(request) => {
                        setIsViewOpen(false);
                        let { Amount } = request;
                        if (formRef.current) {
                            if (Amount.length >= 1) {
                                formRef.current.setFieldValue('Amount', Amount);
                            }
                        }
                    }}
                    negativeButtonClick={() => {
                        setIsViewOpen(false);
                    }}
                />
            )}

            {isResponseAlertOpen && (
                <BBPSTransactionResponse
                    isOpen={isResponseAlertOpen}
                    title={status}
                    ServiceID={serviceID}
                    request={ResponseAlert}
                    OkButtonClick={(request) => {
                        setIsResponseAlertOpen(false);
                    }}
                    PrintButtonClick={() => {
                        Print(ResponseAlert.VendorID);
                        setIsResponseAlertOpen(false);
                    }}
                    CheckStatusClick={(request) => {
                        //setIsOpen(false);
                        StatusCheck(ResponseAlert.VendorID);
                    }}
                />
            )}
        </CustomContainer>
    );
};
