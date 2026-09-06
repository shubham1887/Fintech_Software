import React, { useEffect, useState, useContext, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { TransactionConfirmation } from 'Components/Alert/TransactionConfirmation';
import { TransactionResponse } from 'Components/Alert/TransactionResponse';

import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';

import { Recharge as TRANSACTION_RECHARGE } from 'Components/Api/TransactionAPI';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

import { Last10RechargeReport } from 'Pages/InnerPages/Report/Last10RechargeReport';
import {
    Container,
    Grid,
    Paper,
    MenuItem,
    ListItemIcon
} from '@material-ui/core';
import { GAME_CODE } from 'Components/Helper/Messages';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';

import { BalanceContext } from 'Components/BalanceContext';

import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import HOCLoader from 'Components/Loader/HOCLoader';

import { Event } from 'ReactGa';
import InputLabel from 'Components/Inputes/InputLabel';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import RechargeIcon from 'Components/SvgButtons/RechargeIcon';
import ResetIcon from 'Components/SvgButtons/ResetIcon';
import Divider from '@material-ui/core/Divider';

const CustomContainer = HOCLoader(Container);

export const Game = (props) => {
    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }
    const { serviceID, setRefreshRandom } = props;
    const formRef = useRef();
    const { DashboardBalanceUpdate } = useContext(BalanceContext);
    const [serviceCode, setserviceCode] = useState(GAME_CODE);

    const classes = commonStyle();

    const [Operator, setOperator] = useState([]);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);

    const [severity, setSeverity] = useState('success');

    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [ResponseAlert, setResponseAlert] = useState({});
    const [status, setStatus] = useState('PROCESS');

    const [description, setDescription] = useState('');
    const [isPin] = useState(localStorage.getItem('IsPin'));
    const [InWords, setInWords] = useState('');
    const [formObj, setFormObj] = useState({
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
    const reset = () => {
        setInWords('');
        setFormObj({
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
    };

    const validationSchema = Yup.object({
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator'),
        ConsumerNumber: Yup.string()
            .length(10, 'Must be 10 digit')
            .required(PLEASE_PROVIDE_VALID + 'number'),

        Amount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'amount')
            .min(0, PLEASE_PROVIDE_VALID + 'amount')
            .max(2000, PLEASE_PROVIDE_VALID + 'amount'),

        Pin: Yup.number().when(['IsPin'], {
            is: (isPin) => isPin === 'true',
            then: Yup.number().required('Please enter pin')
        })
    });

    const submitHandle = async (values, { resetForm }) => {
        if (loadingOpen) return;

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

    const Recharge = async (values) => {
        setLoadingOpen(true);
        var request = JSON.stringify(values);

        setIsError(false);
        setResponseMessage('');

        Event('PrePaid', 'Recharge', GetOperatorName(values.OperatorID));

        var response = await TRANSACTION_RECHARGE(request);
        setLoadingOpen(false);
        setserviceCode(0);
        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSeverity('success');
        setRefreshRandom(Math.random(0));

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
        // await SelectTodaysCommission();

        if (!response.status) {
            setSeverity('error');
            return;
        }

        if (data.Status === 'FAILURE') setSeverity('error');

        setTimeout(() => {
            setserviceCode(GAME_CODE);
        }, MESSAGE_CLOSE_AFTER);
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
        selectOperator(serviceID);
    }, [serviceID]);

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
                {({ setFieldValue }) => {
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
                            <Grid item xs={12} sm={12} md={4} lg={4}>
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
                                                    <MaterialSelect
                                                        label="Select Operator"
                                                        name="OperatorID"
                                                        title="Operator"
                                                        size="small"
                                                        //margin="dense"
                                                    >
                                                        {Operator.map(
                                                            (item) => (
                                                                <MenuItem
                                                                    key={
                                                                        item.OperatorID
                                                                    }
                                                                    value={
                                                                        item.OperatorID
                                                                    }
                                                                >
                                                                    <ListItemIcon
                                                                        style={{
                                                                            verticalAlign:
                                                                                'middle'
                                                                        }}
                                                                    >
                                                                        <img
                                                                            src={
                                                                                item.Path
                                                                            }
                                                                            height={
                                                                                30
                                                                            }
                                                                            width={
                                                                                30
                                                                            }
                                                                            alt={
                                                                                item.OperatorName
                                                                            }
                                                                        />
                                                                    </ListItemIcon>
                                                                    {
                                                                        item.OperatorName
                                                                    }
                                                                </MenuItem>
                                                            )
                                                        )}
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
                                                        font_size={
                                                            consumerNumberFontSize
                                                        }
                                                        label="Consumer Number"
                                                        name="ConsumerNumber"
                                                        icon="mobile"
                                                        placeholder="Consumer Number"
                                                        title="Consumer Number"
                                                        typed_length={1}
                                                    />
                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                    container
                                                >
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
                                                            max={2000}
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
                                                    </Grid>
                                                </Grid>

                                                {isPin === 'true' ? (
                                                    <Grid item xs>
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
                                                        disabled={loadingOpen}
                                                        endIcon={<ResetIcon />}
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
                                                        title="Recharge"
                                                        disabled={loadingOpen}
                                                        endIcon={
                                                            <RechargeIcon />
                                                        }
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
                                                <Grid item container xs={12}>
                                                    <p className="description">
                                                        {description}
                                                    </p>
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
                                md={8}
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

            {isAlertOpen ? (
                <TransactionConfirmation
                    isOpen={isAlertOpen}
                    title="Recharge"
                    message="Are you sure to do transaction?"
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
