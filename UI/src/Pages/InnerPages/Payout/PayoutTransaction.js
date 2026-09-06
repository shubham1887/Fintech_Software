import React, { memo, useState, useRef, useEffect } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { Container, Grid, Paper } from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';
import { TransactionConfirmation } from 'Components/Alert/TransactionConfirmation';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import { LOGOUT } from 'ConstComponents';
import { PayoutTransaction as PAYOUT_TRANSACTION } from 'Components/Api/TransactionAPI';
import { SelectSuccessPayoutTransaction as SUCCESS_PAYOUT_TRANSACTION } from 'Components/Api/TransactionReportAPI';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectForReceipt as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { PayoutPrint } from 'Components/Print/PayoutPrint';
import Divider from '@material-ui/core/Divider';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    IMPS_OPERATOR_ID,
    UPI_OPERATOR_ID
} from 'Components/Helper/Messages';
import { PAYOUT } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import ResetIcon from 'Components/SvgButtons/ResetIcon';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import PayIcon from 'Components/SvgButtons/PayIcon';
import { Last10RechargeReport } from 'Pages/InnerPages/Report/Last10RechargeReport';
import AsyncOperatorAutoComplete from 'Components/Inputes/AsyncOperatorAutoComplete';
import { TransactionResponse } from 'Components/Alert/TransactionResponse';
const CustomContainer = HOCLoader(Container);

export const PayoutTransaction = () => {
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [confirmAlertMessage, setConfirmAlertMessage] = useState('');
    const [serviceCode, setserviceCode] = useState(PAYOUT);
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [ResponseAlert, setResponseAlert] = useState({});
    const [status, setStatus] = useState('PROCESS');
    const [Operator, setOperator] = useState([]);
    const [IMPSTextOn, setIMPSTextOn] = useState(false);
    const [UPITextOn, setUPITextOn] = useState(false);
    const [isPrintOpen, setIsPrintOpen] = useState(false);

    const [printData, setPrintData] = useState(null);
    const formRef = useRef();
    const [OperatorDefault, setOperatorDefault] = useState({
        OperatorID: 0,
        OperatorName: 'Select Operator'
    });
    const initialValues = {
        ServiceID: PAYOUT,
        Operator: OperatorDefault,
        AccountNo: '',
        IFSC: '',
        Amount: '',
        InWords: '',
        UPIID: '',
        BeneName: ''
    };

    const [FormObj, setFormObj] = useState(initialValues);

    const FormValidationSchema = Yup.object({
        Operator: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator')
            .shape({
                OperatorID: Yup.number()
                    .notOneOf([0], PLEASE_SELECT_VALID + 'operator')
                    .nullable(PLEASE_SELECT_VALID + 'operator')
                    .required(PLEASE_SELECT_VALID + 'operator')
            }),

        Amount: Yup.number().when(['IsVerifyButton'], {
            is: (IsVerify) => !IsVerify,
            then: Yup.number()
                .required(PLEASE_PROVIDE_VALID + 'amount')
                .min(0, PLEASE_PROVIDE_VALID + 'amount')
                .max(200000, PLEASE_PROVIDE_VALID + 'amount')
        })
    });

    useEffect(() => {
        setLoading(true);
        createColumns();
        selectSuccessPayoutTransaction();
    }, []);

    useEffect(() => {
        setLoading(true);
        selectOperator(serviceCode);
    }, [serviceCode]);

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
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) setIsError(true);

        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);
            setOperator(data);
        }
    };

    const Print = (VendorID) => {
        SelectPrintData(VendorID);
    };

    const SelectPrintData = async (TransactionMasterID) => {
        var req = {
            ID: TransactionMasterID
        };
        var request = JSON.stringify(req);
        setLoading(true);
        const { isAuthorized, status, data } = await TRANSACTION_RECEIPT(
            request
        );
        setLoading(false);
        if (!isAuthorized) LOGOUT();
        var d = JSON.parse(data);
        setPrintData(d);
        setIsPrintOpen(true);
    };

    const selectSuccessPayoutTransaction = async () => {
        setLoading(true);
        setIsError(false);

        setTransactions([]);
        var response = await SUCCESS_PAYOUT_TRANSACTION();
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);

        if (data.length < 0) {
            setSeverity('error');
            setIsError(true);
            setResponseMessage('No transactions found');
            return;
        }

        setTransactions(data);
    };

    const UPIValidation = Yup.object({
        UPIID: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'UPIID')
            .min(7, PLEASE_PROVIDE_VALID + 'UPIID')
            .max(20, PLEASE_PROVIDE_VALID + 'UPIID'),
        BeneName: Yup.string().required(PLEASE_PROVIDE_VALID + 'beneficiary')
    });
    const IMPSValidation = Yup.object({
        AccountNo: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'account no')
            .min(7, PLEASE_PROVIDE_VALID + 'account no')
            .max(20, PLEASE_PROVIDE_VALID + 'account no'),
        IFSC: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'IFSC')
            .length(11, PLEASE_PROVIDE_VALID + 'IFSC'),
        BeneName: Yup.string().required(PLEASE_PROVIDE_VALID + 'beneficiary')
    });
    const submitHandle = async (values) => {
        setResponseMessage('');
        setIsError(false);

        if (values.Operator.OperatorID === UPI_OPERATOR_ID) {
            values.AccountNo = values.UPIID;
            values.IFSC = values.UPIID;
        }

        var RequestObj = {
            ServiceID: values.ServiceID,
            PaymentType: values.Operator.OperatorName,
            AccountNo: values.AccountNo,
            IFSC: values.IFSC,
            Amount: values.Amount,
            BeneName: values.BeneName,
            IsVerify: false
        };

        if (values.Operator.OperatorID === UPI_OPERATOR_ID) {
            let errorValidation = false;

            await UPIValidation.validate(values).catch(function (err) {
                errorValidation = true;
                setSeverity('error');
                setResponseMessage(err.message);

                setIsError(true);
            });

            if (errorValidation === true) {
                return;
            }
        }
        if (values.Operator.OperatorID === IMPS_OPERATOR_ID) {
            let errorValidation = false;

            await IMPSValidation.validate(values).catch(function (err) {
                errorValidation = true;
                setSeverity('error');
                setResponseMessage(err.message);

                setIsError(true);
            });

            if (errorValidation === true) {
                return;
            }
        }

        let msg = '';
        if (values.Operator.OperatorID === IMPS_OPERATOR_ID) {
            msg = `Please confirm payout details - \n\nAccountNo : ${values.AccountNo}\nIFSC : ${values.IFSC}\nAmount : ${values.Amount}`;
        } else if (values.Operator.OperatorID === UPI_OPERATOR_ID) {
            msg = `Please confirm payout details - \n\nUPI ID : ${values.AccountNo}\nAmount : ${values.Amount}`;
        }
        if (values.IsVerifyButton) {
            RequestObj = {
                ServiceID: values.ServiceID,
                PaymentType: values.Operator.OperatorName,
                AccountNo: values.AccountNo,
                IFSC: values.IFSC,
                Amount: 1,
                IsVerify: true,
                BeneName: values.BeneName
            };

            if (values.Operator.OperatorID === IMPS_OPERATOR_ID) {
                msg = `Please confirm payout details - \n\nAccountNo : ${values.AccountNo}\nIFSC : ${values.IFSC}`;
            } else if (values.Operator.OperatorID === UPI_OPERATOR_ID) {
                msg = `Please confirm payout details - \n\nUPI ID : ${values.AccountNo}`;
            }
        }

        setConfirmAlertMessage(msg);
        setConfirmAlertRequest(RequestObj);
        setIsAlertOpen(true);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ServiceID',
            srno: true
        });
        columns.push({
            label: 'BeneName',
            name: 'BeneficiaryName'
        });

        columns.push({
            label: 'PaymentType',
            name: 'PaymentType'
        });
        columns.push({
            label: 'AccountNo',
            name: 'AccountNo'
        });
        columns.push({
            label: 'Amount',
            name: 'Amount',
            length: 6,
            editableWithNumberOnly: true
        });

        columns.push({
            label: 'Transfer',
            name: 'BeneficiaryID',
            buttonTitle: 'Transfer',
            buttonWithTitle: true,
            callback: (value) => {
                TransferConfirmation(value);
            }
        });
        columns.push({
            label: 'IFSC',
            name: 'IFSC'
        });

        setMainColumns(columns);
    };

    const TransferValidation = Yup.object({
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator'),

        AccountNo: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'account no')
            .min(7, PLEASE_PROVIDE_VALID + 'account no')
            .max(20, PLEASE_PROVIDE_VALID + 'account no'),

        // IFSC: Yup.string()
        //     .required(PLEASE_PROVIDE_VALID + 'IFSC')
        //     .length(11, PLEASE_PROVIDE_VALID + 'IFSC'),

        Amount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'amount')
            .min(1, PLEASE_PROVIDE_VALID + 'amount')
            .max(500000, PLEASE_PROVIDE_VALID + 'amount')
    });
    const TransferConfirmation = async (values) => {
        let jsonObj = {
            ServiceID: PAYOUT,
            PaymentType: values[2],
            OperatorID: IMPS_OPERATOR_ID,
            AccountNo: values[3],
            IFSC: values[6],
            Amount: values[4],
            BeneName: values[1],
            IsVerify: false
        };

        setConfirmAlertRequest(jsonObj);

        setIsError(false);
        var errorValidation = false;

        await TransferValidation.validate(jsonObj).catch(function (err) {
            errorValidation = true;
            setSeverity('error');
            setResponseMessage(err.message);
            setIsError(true);
            selectSuccessPayoutTransaction();
            return;
        });

        if (errorValidation === true) return;

        let msg = `Please confirm payout details - \n\nAccountNo : ${values[2]}\nIFSC : ${values[3]}\nAmount : ${values[4]}`;

        setConfirmAlertMessage(msg);
        setIsAlertOpen(true);
    };
    const OnOperatorChange = async (name, value, setFieldValue) => {
        setOperatorDefault({
            OperatorID: value.OperatorID,
            OperatorName: value.OperatorName
        });

        let OperatorID = value.OperatorID; //event.target.value;
        setFieldValue(name, value);

        if (OperatorID === IMPS_OPERATOR_ID) {
            setIMPSTextOn(true);
            setUPITextOn(false);
        } else if (OperatorID === UPI_OPERATOR_ID) {
            setUPITextOn(true);
            setIMPSTextOn(false);
        }
    };
    const Payout = async () => {
        setResponseMessage('');
        setIsError(false);

        var requestObj = JSON.stringify(confirmAlertRequest);

        setLoading(true);

        var response = await PAYOUT_TRANSACTION(requestObj);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }

        let dataSuccess = JSON.parse(response.data);

        setSeverity('success');
        setResponseMessage(dataSuccess.Message);
        setStatus(dataSuccess.Status);
        setResponseAlert(dataSuccess);
        setIsResponseAlertOpen(true);
        selectSuccessPayoutTransaction();
        reset();
    };

    const reset = () => {
        setOperatorDefault({
            OperatorID: 0,
            OperatorName: 'Select Operator'
        });
        setFormObj({
            ServiceID: PAYOUT,
            Operator: OperatorDefault,
            AccountNo: '',
            IFSC: '',
            Amount: '',
            UPIID: '',
            InWords: '',
            BeneName: ''
        });
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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submitHandle}
                resetForm={FormObj}
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
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <Form>
                                    <Grid item xs={12} sm={12}>
                                        <Paper className={classes.PaperStyle}>
                                            <Grid container spacing={2}>
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

                                                {IMPSTextOn ? (
                                                    <>
                                                        <Grid
                                                            item
                                                            xs={12}
                                                            sm={12}
                                                            md={12}
                                                            lg={12}
                                                        >
                                                            <MaterialTextField
                                                                label="Account No."
                                                                name="AccountNo"
                                                                type="text"
                                                                length={20}
                                                                placeholder="Account No."
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
                                                            <MaterialTextField
                                                                label="IFSC"
                                                                name="IFSC"
                                                                type="text"
                                                                length={11}
                                                                placeholder="IFSC"
                                                            />
                                                        </Grid>
                                                    </>
                                                ) : null}
                                                {UPITextOn ? (
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={12}
                                                        lg={12}
                                                    >
                                                        <MaterialTextField
                                                            label="UPI ID"
                                                            name="UPIID"
                                                            type="text"
                                                            length={20}
                                                            placeholder="UPI ID"
                                                        />
                                                    </Grid>
                                                ) : null}

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
                                                        max={500000}
                                                        onCustomCallback={(
                                                            value
                                                        ) => {
                                                            setFieldValue(
                                                                'InWords',
                                                                NumberToWord(
                                                                    value
                                                                )
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
                                                    <MaterialTextField
                                                        label="In words"
                                                        name="InWords"
                                                        type="text"
                                                        readOnly={true}
                                                        placeholder="InWords"
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
                                                        label="Name"
                                                        name="BeneName"
                                                        type="text"
                                                        length={20}
                                                        placeholder="Name"
                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={6}
                                                    lg={6}
                                                ></Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={6}
                                                    lg={6}
                                                >
                                                    <PrimaryButton
                                                        title="Verify"
                                                        endIcon={<PayIcon />}
                                                        onClick={(e) => {
                                                            setFieldValue(
                                                                'IsVerifyButton',
                                                                true
                                                            );
                                                        }}
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
                                                        variant="contained"
                                                        color="secondary"
                                                        title="Reset"
                                                        disabled={loading}
                                                        endIcon={<ResetIcon />}
                                                        onClick={(e) => reset()}
                                                    ></SecondaryButton>
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
                                                        title="Payout"
                                                        endIcon={<PayIcon />}
                                                        disabled={loading}
                                                        onClick={(e) => {
                                                            setFieldValue(
                                                                'IsVerifyButton',
                                                                false
                                                            );
                                                        }}
                                                    ></PrimaryButton>
                                                </Grid>
                                            </Grid>
                                        </Paper>
                                    </Grid>
                                </Form>
                            </Grid>

                            <Grid item xs={12} sm={12} md={8} lg={8}>
                                <ReactTable
                                    data={transactions}
                                    columns={mainColumns}
                                    rowsPerPage={10}
                                />
                            </Grid>
                        </Grid>
                    );
                }}
            </Formik>

            <Last10RechargeReport ServiceID={serviceCode} refreshRandom={1} />
            {isAlertOpen ? (
                <TransactionConfirmation
                    isOpen={isAlertOpen}
                    title="Payout Confirmation"
                    message={confirmAlertMessage}
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Payout(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}

            {isResponseAlertOpen ? (
                <TransactionResponse
                    isOpen={isResponseAlertOpen}
                    isPrint={true}
                    title={status}
                    ServiceID={serviceCode}
                    request={ResponseAlert}
                    OkButtonClick={(request) => {
                        setIsResponseAlertOpen(false);
                    }}
                    PrintButtonClick={() => {
                        Print(ResponseAlert.VendorID);
                        setIsResponseAlertOpen(false);
                    }}
                />
            ) : null}
            {isPrintOpen ? (
                <PayoutPrint
                    isOpen={isPrintOpen}
                    title="Payout Print"
                    request={printData}
                    positiveButtonClick={() => {
                        setIsPrintOpen(false);
                    }}
                    negativeButtonClick={() => {
                        setIsPrintOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
