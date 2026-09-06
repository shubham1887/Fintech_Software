import React, { useEffect, useState, useContext, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { TransactionConfirmation } from 'Components/Alert/TransactionConfirmation';
import { TransactionResponse } from 'Components/Alert/TransactionResponse';
import { MaterialAlphaNumberField } from 'Components/Inputes/FormikMaterialAlphaNumberInput';

import {
    SelectDTHPlan as SELECT_DTH_PLAN,
    SelectDTHCustInfo as SELECT_DTH_CUST_INFO,
    HeavyRefresh as HEAVY_REFRESH
} from 'Components/Api/ROffer';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';

import { Recharge as TRANSACTION_RECHARGE } from 'Components/Api/TransactionAPI';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { Last10RechargeReport } from 'Pages/InnerPages/Report/Last10RechargeReport';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import { TabList, TabContext, TabPanel } from '@material-ui/lab';
import Divider from '@material-ui/core/Divider';
import {
    PrimaryButton,
    SecondaryButton,
    InfoButton
} from 'Components/Inputes/CustomButton';
import {
    Container,
    Grid,
    Paper,
    MenuItem,
    ListItemIcon,
    AppBar,
    Tab
} from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    DTH_RECHARGE_CODE
} from 'Components/Helper/Messages';
import { DTHViewConfirmation } from 'Components/Alert/DTHViewConfirmation';
import { HeavyRefreshConfirmation } from 'Components/Alert/HeavyRefershViewConfirmation';
import HOCLoader from 'Components/Loader/HOCLoader';
import { BalanceContext } from 'Components/BalanceContext';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import InputLabel from 'Components/Inputes/InputLabel';

import ReactTable from 'Components/ReactTable/ReactTable';
import ResetIcon from 'Components/SvgButtons/ResetIcon';
import RechargeIcon from 'Components/SvgButtons/RechargeIcon';
const CustomContainer = HOCLoader(Container);

export const DTH = (props) => {
    const { DashboardBalanceUpdate } = useContext(BalanceContext);

    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }
    const formRef = useRef();
    const classes = commonStyle();
    const [serviceCode, setserviceCode] = useState(DTH_RECHARGE_CODE);
    const [Operator, setOperator] = useState([]);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [serviceID] = useState(props.serviceID);
    const [severity, setSeverity] = useState('success');
    const [isOpen, setIsOpen] = useState(false);
    const [isViewOpen, setIsViewOpen] = useState(false);
    const [isHeavyViewOpen, setIsHeavyViewOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const [description, setDescription] = useState('');
    const [amount, setAmount] = useState(0);

    const [addOnPack, setAddOnPack] = useState([]);
    const [addOnPackColumns, setAddOnPackColumns] = useState([]);
    const [InWords, setInWords] = useState('');
    const [simplePack, setSimplePack] = useState([]);
    const [simplePlanColumns, setSimplePlanColumns] = useState([]);
    const [tabIndex, setTabIndex] = useState('SIMPLE_PACK');
    const [isPin] = useState(localStorage.getItem('IsPin'));
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [ResponseAlert, setResponseAlert] = useState({});
    const [status, setStatus] = useState('PROCESS');
    const [custInfo, setCustInfo] = useState({
        CustomerName: '',
        Status: '',
        PlanName: '',
        MonthlyRecharge: '',
        Balance: '',
        NextRechargeDate: '',
        Desc: ''
    });

    const [formObj] = useState({
        ServiceID: serviceID,
        OperatorID: '',
        ConsumerNumber: '',
        Amount: '',
        InWords: '',

        OperatorName: '',
        OperatorImagePath: '',
        Pin: '',
        IsPin: isPin
    });

    const RechargeValidationSchema = Yup.object({
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator'),

        ConsumerNumber: Yup.string()
            .min(7, 'minimum 7 digits are required')
            .max(16, 'maximum 16 digits are required')
            .required('This field is required'),

        Amount: Yup.number()
            .required('This field is required')
            .min(0, PLEASE_PROVIDE_VALID + 'amount')
            .max(25000, PLEASE_PROVIDE_VALID + 'amount'),

        Pin: Yup.number().when(['IsPin'], {
            is: (isPin) => isPin === 'true',
            then: Yup.number().required('Please enter pin')
        })
    });

    const submitHandle = async (values) => {
        if (loadingOpen) return;

        let op = Operator.filter((o) => o.OperatorID === values.OperatorID);

        values.OperatorName = op[0].OperatorName;
        values.OperatorImagePath = op[0].Path;
        setIsAlertOpen(true);
        setConfirmAlertRequest(values);
    };

    const Recharge = async (values) => {
        var request = JSON.stringify(values);

        setLoadingOpen(true);
        setIsError(false);
        setResponseMessage('');
        var response = await TRANSACTION_RECHARGE(request);
        setLoadingOpen(false);
        setserviceCode(0);
        if (!response.isAuthorized) LOGOUT();

        setIsError(true);

        setSeverity('success');
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
        setResponseMessage(data.Message);
        setStatus(data.Status);
        setResponseAlert(data);
        setIsResponseAlertOpen(true);

        await DashboardBalanceUpdate();

        if (!response.status) {
            setSeverity('error');
            return;
        }

        if (data.Status === 'FAILURE') {
            setSeverity('error');
        }

        setserviceCode(DTH_RECHARGE_CODE);
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

    useEffect(() => {
        fetchConsumerNumberFontSize();
        setLoadingOpen(true);
        selectOperator(serviceID);
    }, []);

    const getDTHPlan = async (values) => {
        setLoadingOpen(true);
        var jsonData = {
            OperatorID: values.OperatorID,
            Circle: 'Maharashtra Goa'
        };
        var request = JSON.stringify(jsonData);
        setResponseMessage('');
        var response = await SELECT_DTH_PLAN(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);

            return;
        }
        var data = JSON.parse(response.data);

        setIsOpen(true);

        if (data.Plan !== null) {
            setTabIndex('SIMPLE_PACK');
            CreateSimplePackColumns();
            setSimplePack(data.Plan);
        }

        if (data['Add-On Pack'] !== null) {
            setTabIndex('SIMPLE_PACK');
            setAddOnPack(data['Add-On Pack']);
            createAddOnColumns();
        }
    };

    const getCustomerInfo = async (values) => {
        let op = Operator.filter((o) => o.OperatorID === values.OperatorID);

        values.OperatorName = op[0].OperatorName;
        values.OperatorImagePath = op[0].Path;

        setLoadingOpen(true);
        var jsonData = {
            OperatorID: values.OperatorID,
            ConsumerNumber: values.ConsumerNumber
        };
        var request = JSON.stringify(jsonData);

        var response = await SELECT_DTH_CUST_INFO(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setSeverity('error');
            setIsError(true);
            return;
        }
        var data = JSON.parse(response.data);

        var customer = data.records[0];

        if (customer !== null) {
            setCustInfo({
                CustomerName: customer.customerName,
                Status: customer.status,
                MonthlyRecharge: customer.MonthlyRecharge,
                Balance: customer.Balance,
                NextRechargeDate: customer.NextRechargeDate,
                PlanName: customer.planname,
                OperatorImagePath: values.OperatorImagePath
            });
            setIsViewOpen(true);
        }
    };

    const getHeavyRefresh = async (values) => {
        setLoadingOpen(true);
        var jsonData = {
            OperatorID: values.OperatorID,
            ConsumerNumber: values.ConsumerNumber
        };
        var request = JSON.stringify(jsonData);

        var response = await HEAVY_REFRESH(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }
        var data = JSON.parse(response.data);

        if (data !== null) {
            setCustInfo({
                CustomerName: data.customerName,
                Desc: data.desc
            });
            setIsHeavyViewOpen(true);
        }
    };

    const margin = {
        padding: '0px',
        marginLeft: '0px',
        paddingTop: '24px',
        marginRight: '0px'
    };

    const CreateSimplePackColumns = () => {
        var columns = [];
        columns.push({
            label: '',
            name: '',
            buttonTitle: 'Buy this plan',
            buyButton: true,
            callback: (row) => {
                setAmount(row[1]);
                setDescription(
                    'Description : ' + row[2] + ' \n Validity: ' + row[3]
                );
                setIsOpen(false);
            }
        });
        columns.push({
            label: 'RS',
            name: '1Month'
        });
        columns.push({
            label: 'Name',
            name: 'plan_name'
        });
        columns.push({
            label: 'Description',
            name: 'desc'
        });

        setSimplePlanColumns(columns);
    };

    const createAddOnColumns = () => {
        var columns = [];
        columns.push({
            label: '',
            name: '',
            buttonTitle: 'Buy this plan',
            buyButton: true,
            callback: (row) => {
                setAmount(row[1]);
                setDescription(
                    'Description : ' + row[2] + ' \n Validity: ' + row[3]
                );
                setIsOpen(false);
            }
        });
        columns.push({
            label: 'RS',
            name: '1Month'
        });
        columns.push({
            label: 'Name',
            name: 'plan_name'
        });
        columns.push({
            label: 'Description',
            name: 'desc'
        });

        setAddOnPackColumns(columns);
    };
    const handleChange = (event, newValue) => {
        setTabIndex(newValue);
    };

    const OperatorList = Operator.map(({ OperatorID, Path, OperatorName }) => (
        <MenuItem key={OperatorID} value={OperatorID}>
            <ListItemIcon style={{ verticalAlign: 'middle' }}>
                <img src={Path} height={30} width={30} alt={OperatorName} />
            </ListItemIcon>
            {OperatorName}
        </MenuItem>
    ));

    const isValidfield = (
        fieldName,
        setFieldTouched,
        errors,
        validateField
    ) => {
        validateField(fieldName);

        if (errors[fieldName]) {
            setFieldTouched(fieldName, true);
            return false;
        }
        return true;
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
                initialValues={formObj}
                validationSchema={RechargeValidationSchema}
                onSubmit={submitHandle}
                resetForm={formObj}
            >
                {({
                    values,
                    setFieldTouched,
                    errors,
                    validateField,
                    setFieldValue
                }) => {
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
                                    <Paper className={classes.PaperStyle}>
                                        <Grid container spacing={2}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <MaterialSelect
                                                    label="Select DTH Operator"
                                                    name="OperatorID"
                                                    title="Operator"
                                                    //margin="dense"
                                                >
                                                    {OperatorList}
                                                </MaterialSelect>
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <MaterialNumberField
                                                    label="Registered Subscriber ID"
                                                    name="ConsumerNumber"
                                                    //size="small"
                                                    length="15"
                                                    icon="mobile"
                                                    placeholder="Subscriber ID"
                                                    title="Registered Subscriber ID"
                                                    typed_length={1}
                                                />
                                            </Grid>

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
                                                    //size="small"
                                                    placeholder="₹ Amount"
                                                    autoComplete="new-password"
                                                    size="small"
                                                    max={10000}
                                                    title="₹ Amount"
                                                    value={
                                                        amount
                                                            ? amount
                                                            : values.Amount
                                                    }
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
                                                    submitType="button"
                                                    onClick={(e) => {
                                                        let fieldError = isValidfield(
                                                            'OperatorID',
                                                            setFieldTouched,
                                                            errors,
                                                            validateField
                                                        );
                                                        if (!fieldError) {
                                                            return;
                                                        }

                                                        getDTHPlan(values);
                                                    }}
                                                />
                                            </Grid>

                                            {isPin === 'true' && (
                                                <Grid item xs={12}>
                                                    <Grid container spacing={2}>
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
                                                                length={4}
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
                                                    type="reset"
                                                    title="Reset"
                                                    // endIcon={<ResetIcon />}
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
                                                    //endIcon={<RechargeIcon />}
                                                    variant="contained"
                                                    color="primary"
                                                    disabled={loadingOpen}
                                                />
                                            </Grid>
                                        </Grid>

                                        <Grid item container xs={12}>
                                            <p>{description}</p>
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
                                <Last10RechargeReport
                                    ServiceID={serviceCode}
                                    refreshRandom={1}
                                />
                            </Grid>
                        </Grid>
                    );
                }}
            </Formik>
            ;
            {isOpen ? (
                <CustomModalWithScroll
                    isOpen={isOpen}
                    title="Plan"
                    width="75%"
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
                                    height: '50',
                                    marginLeft: '10px'
                                }}
                            >
                                <TabList
                                    onChange={handleChange}
                                    aria-label="simple tabs example"
                                >
                                    <Tab
                                        label="SIMPLE PACK"
                                        value="SIMPLE_PACK"
                                    />
                                    <Tab
                                        label="Add On Pack"
                                        value="ADD_ON_PACK"
                                    />
                                </TabList>
                            </AppBar>
                            <TabPanel value="SIMPLE_PACK" style={margin}>
                                <ReactTable
                                    data={simplePack}
                                    columns={simplePlanColumns}
                                    rowsPerPage={10}
                                    headerToolbar={false}
                                />
                            </TabPanel>
                            <TabPanel value="ADD_ON_PACK" style={margin}>
                                <ReactTable
                                    data={addOnPack}
                                    columns={addOnPackColumns}
                                    rowsPerPage={10}
                                    headerToolbar={false}
                                />
                            </TabPanel>
                        </TabContext>
                    </Grid>
                </CustomModalWithScroll>
            ) : null}
            {isViewOpen ? (
                <DTHViewConfirmation
                    isOpen={isViewOpen}
                    title="Information"
                    request={custInfo}
                    positiveButtonTitle={'OK'}
                    positiveButtonClick={(request) => {
                        setIsViewOpen(false);
                        let { Amount } = request;
                    }}
                    negativeButtonClick={() => {
                        setIsViewOpen(false);
                    }}
                />
            ) : null}
            {isHeavyViewOpen ? (
                <HeavyRefreshConfirmation
                    isOpen={isHeavyViewOpen}
                    title="Information"
                    message=""
                    request={custInfo}
                    positiveButtonTitle={'OK'}
                    positiveButtonClick={(request) => {
                        setIsHeavyViewOpen(false);
                        let { Amount } = request;
                    }}
                    negativeButtonClick={() => {
                        setIsHeavyViewOpen(false);
                    }}
                />
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
