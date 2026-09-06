import React, { useEffect, useState, useContext, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { TransactionConfirmation } from 'Components/Alert/TransactionConfirmation';
import { TransactionResponse } from 'Components/Alert/TransactionResponse';

import {
    SelectPlan as SELECT_PLAN,
    SelectROffer as SELECT_OFFER
} from 'Components/Api/ROffer';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';

import {
    Recharge as TRANSACTION_RECHARGE,
    SelectOperatorByConsumerNumber as OPERATOR_BY_NUMBER
} from 'Components/Api/TransactionAPI';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialConsumerNumber } from 'Components/Inputes/MaterialConsumerNumber';
import TabList from '@material-ui/lab/TabList';
import TabContext from '@material-ui/lab/TabContext';
import TabPanel from '@material-ui/lab/TabPanel';
import { Last10RechargeReport } from 'Pages/InnerPages/Report/Last10RechargeReport';
import Divider from '@material-ui/core/Divider';
import {
    Container,
    Grid,
    Paper,
    TextField,
    MenuItem,
    ListItemIcon,
    AppBar,
    Tab
} from '@material-ui/core';
import { PREPAID_RECHARGE_CODE } from 'Components/Helper/Messages';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';

import { BalanceContext } from 'Components/BalanceContext';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    PrimaryButton,
    SecondaryButton,
    InfoButton
} from 'Components/Inputes/CustomButton';
import HOCLoader from 'Components/Loader/HOCLoader';
import ReactTable from 'Components/ReactTable/ReactTable';
import { Event } from 'ReactGa';
import InputLabel from 'Components/Inputes/InputLabel';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import RechargeIcon from 'Components/SvgButtons/RechargeIcon';
import ResetIcon from 'Components/SvgButtons/ResetIcon';
import AutoSubmitConsumerNumber from 'Components/Helper/AutoSubmitConsumerNumber';

const CustomContainer = HOCLoader(Container);

export const Prepaid = (props) => {
    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }

    const { serviceID, setRefreshRandom } = props;
    const formRef = useRef();
    const { DashboardBalanceUpdate } = useContext(BalanceContext);
    const [serviceCode, setserviceCode] = useState(PREPAID_RECHARGE_CODE);
    const [planColumns, setPlanColumns] = useState([]);
    const [offerData, setOfferData] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [offerColumns, setOfferColumns] = useState([]);
    const classes = commonStyle();

    const [Operator, setOperator] = useState([]);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');

    const [severity, setSeverity] = useState('success');
    const [tabIndex, setTabIndex] = useState('FULLTT');
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const [planFULLTTData, setPlanFULLTTData] = useState([]);
    const [planJioPhoneData, setPlanJioPhoneData] = useState([]);
    const [planComboData, setPlanComboData] = useState([]);
    const [planTOPUPData, setPlanTOPUPData] = useState([]);
    const [planRomingData, setPlanRomingData] = useState([]);
    const [plan3G4GData, setPlan3G4GData] = useState([]);
    const [plan2GData, setPlan2GData] = useState([]);

    const [description, setDescription] = useState('');
    const [isPin] = useState(localStorage.getItem('IsPin'));
    const [InWords, setInWords] = useState('');
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [ResponseAlert, setResponseAlert] = useState({});
    const [status, setStatus] = useState('PROCESS');
    const [NumberFirst] = useState(localStorage.getItem('NumberFirst'));

    const [Circle, setCircle] = useState('');
    const [formObj, setFormObj] = useState({
        ServiceID: serviceID,
        OperatorID: '',
        ConsumerNumber: '',
        Amount: '',
        InWords: '',
        IsPlanButton: false,
        IsROfferButton: false,
        OperatorName: '',
        OperatorImagePath: '',
        Pin: '',
        IsPin: isPin
    });
    const reset = () => {
        setInWords('');
        setCircle('');
        setFormObj({
            ServiceID: serviceID,
            OperatorID: '',
            ConsumerNumber: '',
            Amount: '',
            InWords: '',
            IsPlanButton: false,
            IsROfferButton: false,
            OperatorName: '',
            OperatorImagePath: '',
            Pin: '',
            IsPin: isPin
        });
    };

    const margin = {
        padding: '0px',
        marginLeft: '0px',
        paddingTop: '24px',
        marginRight: '0px'
    };

    const validationSchema = Yup.object({
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator'),

        ConsumerNumber: Yup.string()
            .min(7, 'minimum 7 digits are required')
            .max(16, 'maximum 16 digits are required')
            .required('This field is required'),

        IsPlanButton: Yup.boolean(),
        IsROfferButton: Yup.boolean(),

        Amount: Yup.number().when(['IsPlanButton', 'IsROfferButton'], {
            is: (isplan, isOffer) => !isplan && !isOffer,
            then: Yup.number()
                .required('This field is required')
                .min(0, PLEASE_PROVIDE_VALID + 'amount')
                .max(10000, PLEASE_PROVIDE_VALID + 'amount')
        }),

        Pin: Yup.number().when(['IsPin', 'IsPlanButton', 'IsROfferButton'], {
            is: (isPin, isplan, isOffer) =>
                isPin === 'true' && !isplan && !isOffer,
            then: Yup.number().required('Please enter pin')
        })
    });

    const submitHandle = async (values, { resetForm }) => {
        if (loadingOpen) return;

        if (values.IsPlanButton) {
            getPlan(values);
            return;
        }
        if (values.IsROfferButton) {
            getROffer(values);
            return;
        }

        let op = Operator.filter((o) => o.OperatorID === values.OperatorID);

        values.OperatorName = op[0].OperatorName;
        values.OperatorImagePath = op[0].Path;

        setIsAlertOpen(true);
        setConfirmAlertRequest(values);
    };

    const GetOperatorName = (OperatorID) => {
        var opName = '';
        opName = Operator.find(function (values) {
            if (values.OperatorID === OperatorID) return values.OperatorName;
            return '';
        });

        return opName.OperatorName;
    };

    const isTabDisabled = (data) => {
        if (data === undefined) return true;

        if (data === null) return true;

        if (data.length <= 0) return true;

        return false;
    };

    const resetToast = () => {
        setIsError(false);
        setResponseMessage('');
    };

    const Recharge = async (values) => {
        setLoadingOpen(true);
        var request = JSON.stringify(values);
        resetToast();
        Event('PrePaid', 'Recharge', GetOperatorName(values.OperatorID));
        var response = await TRANSACTION_RECHARGE(request);
        setLoadingOpen(false);
        setserviceCode(0);
        if (!response.isAuthorized) LOGOUT();

        setRefreshRandom(Math.random(0));

        let data1 = JSON.parse(response.data);

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(data1.Message);
            setIsError(true);

            return;
        }

        setIsError(true);
        setSeverity('success');
        var data = JSON.parse(response.data);
        if (formRef.current) formRef.current.resetForm();
        setInWords('');
        setResponseMessage(data.Message);

        setStatus(data.Status);
        setResponseAlert(data);
        setIsResponseAlertOpen(true);

        await DashboardBalanceUpdate();

        if (data.Status === 'FAILURE') {
            setSeverity('error');
            setResponseMessage(data.Message);
            return;
        }

        setserviceCode(PREPAID_RECHARGE_CODE);
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

    const FetchOperator = async (values, setFieldValue) => {
        if (values === undefined || values <= 0) return;

        var requestObj = {
            ConsumerNumber: values.ConsumerNumber
        };

        var request = JSON.stringify(requestObj);

        const response = await OPERATOR_BY_NUMBER(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) setIsError(true);

        if (response.status) {
            var data = JSON.parse(response.data);

            setFieldValue('OperatorID', data.OperatorID);

            setFieldValue('OperatorName', data.OperatorName);

            updateCircleData(setFieldValue, data.Circle);
        }
    };

    const updateCircleData = (setFieldValue, Circle) => {
        if (Circle === undefined || Circle === null || Circle.length <= 1) {
            return;
        }
        setFieldValue('Circle', Circle);
        setCircle(Circle);
    };

    useEffect(() => {
        fetchConsumerNumberFontSize();
        selectOperator(serviceID);
    }, [serviceID]);

    const getROffer = async (values) => {
        var jsonData = {
            OperatorID: values.OperatorID,
            ConsumerNumber: values.ConsumerNumber
        };

        var request = JSON.stringify(jsonData);
        setResponseMessage('');
        setIsError(false);
        setLoadingOpen(true);
        var response = await SELECT_OFFER(request);

        setLoadingOpen(false);

        var data = JSON.parse(response.data);

        setIsOpen(true);
        if (data !== null) {
            setTabIndex('OFFERS');
            setOfferData(data);
            createOfferColumns();
        }

        return;
    };

    const createOfferColumns = () => {
        var columns = [];
        columns.push({
            label: '',
            name: '',
            buttonTitle: 'Buy this plan',
            buyButton: true,
            callback: (row) => {
                if (formRef.current) {
                    formRef.current.setFieldValue('Amount', row[1]);
                }
                setDescription(
                    'Description : ' + row[2] + ' \n Validity: ' + row[3]
                );
                setIsOpen(false);
            }
        });
        columns.push({
            label: 'RS',
            name: 'rs'
        });

        columns.push({
            label: 'Description',
            name: 'desc'
        });

        setOfferColumns(columns);
    };

    const getPlan = async (values) => {
        var jsonData = {
            OperatorID: values.OperatorID,
            ConsumerNumber: values.ConsumerNumber,
            Circle: 'Maharashtra Goa'
        };

        Event('PrePaid', 'Plan', GetOperatorName(values.OperatorID));

        var request = JSON.stringify(jsonData);
        setResponseMessage('');
        setIsError(false);
        setLoadingOpen(true);
        var response = await SELECT_PLAN(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        const data = JSON.parse(response.data);

        CreatePlanColumns();
        setIsOpen(true);

        setPlanRomingData(data.Romaing || []);
        setPlanTOPUPData(data.TOPUP || []);
        setPlan3G4GData(data['3G/4G'] || []);
        setPlanComboData(data['COMBO'] || []);
        setPlanFULLTTData(data.FULLTT || []);
        setPlanJioPhoneData(data['JioPhone'] || []);
        setPlan2GData(data['2G'] || []);

        const availableTabs = [
            'COMBO',
            'TOPUP',
            '3G/4G',
            'FULLTT',
            'JioPhone',
            '2G'
        ];
        const firstAvailableTab = availableTabs.find(
            (tab) => data[tab] !== null
        );

        setTabIndex(firstAvailableTab || 'COMBO');
    };

    const CreatePlanColumns = () => {
        var columns = [];
        columns.push({
            label: '',
            name: '',
            buttonTitle: 'Buy this plan',
            buyButton: true,

            callback: (row) => {
                if (formRef.current)
                    formRef.current.setFieldValue('Amount', row[1]);

                setDescription(
                    'Description : ' + row[2] + ' \n Validity: ' + row[3]
                );
                setIsOpen(false);
            }
        });
        columns.push({
            label: 'RS',
            name: 'rs',
            draggle: true
        });

        columns.push({
            label: 'Description',
            name: 'desc',
            draggle: true
        });
        columns.push({
            label: 'validity',
            name: 'validity'
        });

        setPlanColumns(columns);
    };

    const handleChange = (event, newValue) => {
        setTabIndex(newValue);
    };

    const ConsumerFirst = (values, setFieldValue) => {
        return (
            <>
                <Grid item xs={12} sm={12} md={12} lg={12}>
                    <AutoSubmitConsumerNumber
                        callback={() => {
                            FetchOperator(values, setFieldValue);
                        }}
                    />

                    <MaterialConsumerNumber
                        label="Enter Mobile Number"
                        name="ConsumerNumber"
                        icon="mobile"
                        length="10"
                        //placeholder="Enter Mobile Number"
                        title="Enter Mobile Number"
                        typed_length={1}
                    />
                </Grid>

                <Grid item xs={12} sm={12} md={12} lg={12}>
                    <MaterialSelect
                        label="Operator"
                        name="OperatorID"
                        title="Operator"
                        size="small"
                        //margin="dense"
                    >
                        {Operator.map((item) => (
                            <MenuItem
                                key={item.OperatorID}
                                value={item.OperatorID}
                            >
                                <ListItemIcon
                                    style={{
                                        verticalAlign: 'middle'
                                    }}
                                >
                                    <img
                                        src={item.Path}
                                        height={30}
                                        width={30}
                                        alt={item.OperatorName}
                                    />
                                </ListItemIcon>
                                {item.OperatorName}
                            </MenuItem>
                        ))}
                    </MaterialSelect>
                </Grid>

                <Grid container spacing={2}>
                    {Circle ? (
                        <Grid item xs={12} sm={12} md={12} lg={12}>
                            <TextField
                                label="Circle"
                                name="Circle"
                                variant="outlined"
                                size="small"
                                value={Circle}
                                fullWidth
                                InputProps={{
                                    style: {
                                        color: 'green'
                                    }
                                }}
                                inputProps={{
                                    style: {
                                        padding: '15px 25px'
                                    }
                                }}
                            />
                        </Grid>
                    ) : null}
                </Grid>
            </>
        );
    };

    const OperatorFirst = () => {
        return (
            <>
                <Grid item xs={12} sm={12} md={12} lg={12}>
                    <MaterialConsumerNumber
                        label="Enter Mobile Number"
                        name="ConsumerNumber"
                        icon="mobile"
                        length="10"
                        //placeholder=" Enter Mobile Number"
                        title="Enter Mobile Number"
                        typed_length={1}
                    />
                </Grid>

                <Grid item xs={12} sm={12} md={12} lg={12}>
                    <MaterialSelect
                        label="Operator"
                        name="OperatorID"
                        title="Operator"
                        size="small"
                    >
                        {Operator.map((item) => (
                            <MenuItem
                                key={item.OperatorID}
                                value={item.OperatorID}
                            >
                                <ListItemIcon
                                    style={{
                                        verticalAlign: 'middle'
                                    }}
                                >
                                    <img
                                        src={item.Path}
                                        height={30}
                                        width={30}
                                        alt={item.OperatorName}
                                    />
                                </ListItemIcon>
                                {item.OperatorName}
                            </MenuItem>
                        ))}
                    </MaterialSelect>
                </Grid>
            </>
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
                validationSchema={validationSchema}
                onSubmit={submitHandle}
                resetForm={formObj}
            >
                {({ values, setFieldValue }) => {
                    return (
                        <>
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
                                            <Paper
                                                elevation={0}
                                                className={classes.PaperStyle}
                                            >
                                                <Grid container spacing={2}>
                                                    {NumberFirst === 'true'
                                                        ? ConsumerFirst(
                                                              values,
                                                              setFieldValue
                                                          )
                                                        : OperatorFirst()}

                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={7}
                                                        lg={7}
                                                    >
                                                        <MaterialCurrency
                                                            name="Amount"
                                                            label="₹ Amount"
                                                            //placeholder="₹ Amount"
                                                            autoComplete="new-password"
                                                            size="small"
                                                            max={10000}
                                                            title="₹ Amount"
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
                                                    </Grid>

                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={5}
                                                        lg={5}
                                                        style={{
                                                            display: 'flex-end',
                                                            justifyContent:
                                                                'space-between'
                                                        }}
                                                    >
                                                        <InfoButton
                                                            title="View Plan"
                                                            onClick={(e) => {
                                                                setFieldValue(
                                                                    'IsPlanButton',
                                                                    true
                                                                );
                                                                setFieldValue(
                                                                    'IsROfferButton',
                                                                    false
                                                                );
                                                            }}
                                                        />
                                                    </Grid>

                                                    {isPin === 'true' && (
                                                        <Grid item xs={12}>
                                                            <Grid
                                                                container
                                                                spacing={2}
                                                            >
                                                                <Grid
                                                                    item
                                                                    xs={12}
                                                                    sm={6}
                                                                    md={4}
                                                                    lg={4}
                                                                >
                                                                    <MaterialNumberField
                                                                        label="Pin"
                                                                        type="password"
                                                                        name="Pin"
                                                                        placeholder="Pin"
                                                                        title="Pin"
                                                                        length={
                                                                            4
                                                                        }
                                                                    />
                                                                </Grid>
                                                            </Grid>
                                                        </Grid>
                                                    )}
                                                </Grid>

                                                <Divider
                                                    variant="middle"
                                                    className="rechdevider"
                                                />

                                                <Grid item container xs={12}>
                                                    <p className="description">
                                                        {description}
                                                    </p>
                                                </Grid>

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
                                                            // endIcon={<ResetIcon />}
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
                                                        <PrimaryButton
                                                            title="Proceed"
                                                            disabled={
                                                                loadingOpen
                                                            }
                                                            // endIcon={<RechargeIcon />}
                                                            onClick={(e) => {
                                                                setFieldValue(
                                                                    'IsPlanButton',
                                                                    false
                                                                );
                                                                setFieldValue(
                                                                    'IsROfferButton',
                                                                    false
                                                                );
                                                            }}
                                                        />
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
                                        refreshRandom={1}
                                    />
                                </Grid>
                            </Grid>
                        </>
                    );
                }}
            </Formik>

            {isOpen ? (
                <CustomModalWithScroll
                    isOpen={isOpen}
                    title="Plan"
                    width="75vw"
                    onDismissCallback={(value) => {
                        setIsOpen(value);
                    }}
                >
                    <Grid item xs={12} sm={12} md={12} lg={12}>
                        <TabContext value={tabIndex}>
                            <AppBar
                                position="static"
                                color="default"
                                style={{
                                    height: '50'
                                }}
                            >
                                <TabList
                                    onChange={handleChange}
                                    aria-label="Tabs"
                                >
                                    <Tab label="COMBO" value="COMBO" />
                                    <Tab label="TOPUP" value="TOPUP" />
                                    <Tab label="3G/4G" value="3G4G" />
                                    <Tab label="OFFERS" value="OFFERS" />
                                    <Tab label="Roaming" value="Romaing" />
                                    <Tab label="FULLTT" value="FULLTT" />
                                    <Tab label="JioPhone" value="JioPhone" />
                                    <Tab label="2G" value="2G" />
                                </TabList>
                            </AppBar>

                            <TabPanel value="FULLTT" style={margin}>
                                {planFULLTTData.length > 0 ? (
                                    <ReactTable
                                        data={planFULLTTData}
                                        columns={planColumns}
                                        rowsPerPage={10}
                                        headerToolbar={false}
                                    />
                                ) : (
                                    <div
                                        style={{
                                            display: 'flex',
                                            justifyContent: 'center',
                                            alignItems: 'center',
                                            height: '100%',
                                            fontSize: '20px',
                                            fontWeight: 'bold',
                                            color: 'red',
                                            textAlign: 'center'
                                        }}
                                    >
                                        No data found
                                    </div>
                                )}
                            </TabPanel>

                            <TabPanel value="COMBO" style={margin}>
                                {planComboData.length > 0 ? (
                                    <ReactTable
                                        data={planComboData}
                                        columns={planColumns}
                                        rowsPerPage={10}
                                        headerToolbar={false}
                                    />
                                ) : (
                                    <div
                                        style={{
                                            display: 'flex',
                                            justifyContent: 'center',
                                            alignItems: 'center',
                                            height: '100%',
                                            fontSize: '20px',
                                            fontWeight: 'bold',
                                            color: 'red',
                                            textAlign: 'center'
                                        }}
                                    >
                                        No data found
                                    </div>
                                )}
                            </TabPanel>

                            <TabPanel value="TOPUP" style={margin}>
                                {planTOPUPData.length > 0 ? (
                                    <ReactTable
                                        data={planTOPUPData}
                                        columns={planColumns}
                                        rowsPerPage={10}
                                        headerToolbar={false}
                                    />
                                ) : (
                                    <div
                                        style={{
                                            display: 'flex',
                                            justifyContent: 'center',
                                            alignItems: 'center',
                                            height: '100%',
                                            fontSize: '20px',
                                            fontWeight: 'bold',
                                            color: 'red',
                                            textAlign: 'center'
                                        }}
                                    >
                                        No data found
                                    </div>
                                )}
                            </TabPanel>

                            <TabPanel value="3G4G" style={margin}>
                                {plan3G4GData.length > 0 ? (
                                    <ReactTable
                                        data={plan3G4GData}
                                        columns={planColumns}
                                        rowsPerPage={10}
                                        headerToolbar={false}
                                    />
                                ) : (
                                    <div
                                        style={{
                                            display: 'flex',
                                            justifyContent: 'center',
                                            alignItems: 'center',
                                            height: '100%',
                                            fontSize: '20px',
                                            fontWeight: 'bold',
                                            color: 'red',
                                            textAlign: 'center'
                                        }}
                                    >
                                        No data found
                                    </div>
                                )}
                            </TabPanel>

                            <TabPanel value="OFFERS" style={margin}>
                                {offerData.length > 0 ? (
                                    <ReactTable
                                        data={offerData}
                                        columns={offerColumns}
                                        rowsPerPage={10}
                                        headerToolbar={false}
                                    />
                                ) : (
                                    <div
                                        style={{
                                            display: 'flex',
                                            justifyContent: 'center',
                                            alignItems: 'center',
                                            height: '100%',
                                            fontSize: '20px',
                                            fontWeight: 'bold',
                                            color: 'red',
                                            textAlign: 'center'
                                        }}
                                    >
                                        No data found
                                    </div>
                                )}
                            </TabPanel>

                            <TabPanel value="Romaing" style={margin}>
                                {planRomingData.length > 0 ? (
                                    <ReactTable
                                        data={planRomingData}
                                        columns={planColumns}
                                        rowsPerPage={10}
                                        headerToolbar={false}
                                    />
                                ) : (
                                    <div
                                        style={{
                                            display: 'flex',
                                            justifyContent: 'center',
                                            alignItems: 'center',
                                            height: '100%',
                                            fontSize: '20px',
                                            fontWeight: 'bold',
                                            color: 'red',
                                            textAlign: 'center'
                                        }}
                                    >
                                        No data found
                                    </div>
                                )}
                            </TabPanel>

                            <TabPanel value="JioPhone" style={margin}>
                                {planJioPhoneData.length > 0 ? (
                                    <ReactTable
                                        data={planJioPhoneData}
                                        columns={planColumns}
                                        rowsPerPage={10}
                                        headerToolbar={false}
                                    />
                                ) : (
                                    <div
                                        style={{
                                            display: 'flex',
                                            justifyContent: 'center',
                                            alignItems: 'center',
                                            height: '100%',
                                            fontSize: '20px',
                                            fontWeight: 'bold',
                                            color: 'red',
                                            textAlign: 'center'
                                        }}
                                    >
                                        No data found
                                    </div>
                                )}
                            </TabPanel>

                            <TabPanel value="2G" style={margin}>
                                {plan2GData.length > 0 ? (
                                    <ReactTable
                                        data={plan2GData}
                                        columns={planColumns}
                                        rowsPerPage={10}
                                        headerToolbar={false}
                                    />
                                ) : (
                                    <div
                                        style={{
                                            display: 'flex',
                                            justifyContent: 'center',
                                            alignItems: 'center',
                                            height: '100%',
                                            fontSize: '20px',
                                            fontWeight: 'bold',
                                            color: 'red',
                                            textAlign: 'center'
                                        }}
                                    >
                                        No data found
                                    </div>
                                )}
                            </TabPanel>
                        </TabContext>
                    </Grid>
                </CustomModalWithScroll>
            ) : null}

            {isAlertOpen ? (
                <TransactionConfirmation
                    isOpen={isAlertOpen}
                    title="Verify Transaction Details"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Recharge(request);
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
                    ServiceID={serviceCode}
                    request={ResponseAlert}
                    OkButtonClick={(request) => {
                        setIsResponseAlertOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
