import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { AddComplaint } from 'Pages/InnerPages/Complaints/Add';
import { Search as SELECT_SEARCH_NUMBER } from 'Components/Api/TransactionReportAPI';
import { Search as SELECT_SEARCH_LOG } from 'Components/Api/ProviderReqestResponceApi';
import { Search as SELECT_SEARCH_CALLBACK } from 'Components/Api/ProviderCallBackLogApi';
import { Search as SELECT_API_USER_CALLBACK } from 'Components/Api/APIUserCallbackApi';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { CustomModal } from 'Components/Modal/CustomModal';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, Grid, Paper } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import {
    PLEASE_PROVIDE_VALID,
    ADMIN_CODE,
    SUPPORT_CODE,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import { StatusEdit } from 'Pages/InnerPages/Report/ManualStatusEdit';
import HOCLoader from 'Components/Loader/HOCLoader';
import { ResendCallback as RESEND_CALLBACK } from 'Components/Api/APIUserCallbackApi';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const Search = () => {
    const formRef = useRef();
    const SearchInputRef = useRef(null);
    const classes = commonStyle();
    const [transaction, setTransaction] = useState([]);
    const [ProviderLog, setProviderLog] = useState([]);
    const [ApiUserCallback, setApiUserCallback] = useState([]);
    const [Callback, setCallback] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [LogColumns, setLogColumns] = useState([]);
    const [CallbackColumns, setCallbackColumns] = useState([]);
    const [ApiUserCallbackColumns, setApiUserCallbackColumns] = useState([]);

    const [loadingOpen, setLoadingOpen] = useState(false);

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [isAdminRole, setIsAdminRole] = useState(false);

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [transactionMasterID, setTransactionMasterID] = useState(0);

    const [isTransactionVisible, setTransactionVisible] = useState(false);
    const [isProviderLogVisible, setProviderLogVisible] = useState(false);
    const [isApiUserLogVisible, setisApiUserLogVisible] = useState(false);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [BackDays, setBackDays] = useState(
        localStorage.getItem('QuickSearchBackDays')
    );

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
        if (SUPPORT_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);

        createColumns();
    };

    useEffect(() => {
        CheckUserRole();
        createLogColumns();
        createCallbackColumns();
        createApiCallbackColumns();
        SearchInputRef.current.focus();
    }, [isAdminRole]);

    var FormObj = {
        ToDate: new Date(),
        FromDate: new Date().addDays(-BackDays),
        ConsumerNumber: '',
        ApiLog: false,
        APIUserCallback: false
    };

    const Edit = async (row) => {
        setTransactionMasterID(row[0]);
        setIsModalOpen(true);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;
        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);
        setIsError(false);
        if (values.ApiLog) {
            var Log_response = await SELECT_SEARCH_LOG(requestObj);
            setLoadingOpen(false);

            if (!Log_response.isAuthorized) LOGOUT();

            if (!Log_response.status) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage(Log_response.data);

                return;
            }
            var Logdata = JSON.parse(Log_response.data);
            setProviderLogVisible(true);
            setProviderLog(Logdata);

            var Callback_response = await SELECT_SEARCH_CALLBACK(requestObj);
            if (!Callback_response.isAuthorized) LOGOUT();
            var callbackdata = JSON.parse(Callback_response.data);

            setCallback(callbackdata);
            values.ApiLog = false;
            return;
        }
        if (values.APIUserCallback) {
            var APIUserCallback_response = await SELECT_API_USER_CALLBACK(
                requestObj
            );
            setLoadingOpen(false);
            if (!APIUserCallback_response.isAuthorized) LOGOUT();
            if (!APIUserCallback_response.status) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage(APIUserCallback_response.data);

                return;
            }
            var APIUserCallbackdata = JSON.parse(APIUserCallback_response.data);

            setisApiUserLogVisible(true);
            setApiUserCallback(APIUserCallbackdata);
            values.APIUserCallback = false;
            return;
        }
        setTransaction([]);
        setApiUserCallback([]);
        setisApiUserLogVisible(false);
        setProviderLog([]);
        setProviderLogVisible(false);
        setCallback([]);

        var response = await SELECT_SEARCH_NUMBER(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        setTransactionVisible(true);
        setTransaction(data);
    };
    const Select = async () => {
        setTransaction([]);
        const modelObj = {
            ConsumerNumber: formRef.current.values.ConsumerNumber,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate
        };

        var requestObj = JSON.stringify(modelObj);
        setLoadingOpen(true);
        var response = await SELECT_SEARCH_NUMBER(requestObj);
        setLoadingOpen(false);
        var data = JSON.parse(response.data);
        setTransactionVisible(true);
        setTransaction(data);
    };

    const ResendCallback = async (TransactionMasterID) => {
        let jsonObj = {
            TransactionMasterID: TransactionMasterID
        };

        var request = JSON.stringify(jsonObj);

        const response = await RESEND_CALLBACK(request);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
    };

    const FormValidationSchema = Yup.object({
        ConsumerNumber: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'consumer number'
        ),
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'from date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'from date')
            .typeError(PLEASE_SELECT_VALID + 'from date'),

        ToDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'to date')
            .typeError(PLEASE_SELECT_VALID + 'to date')
    });
    const RaiseComplaint = (ID) => {
        setTransactionMasterID(ID);
        setIsOpen(true);
    };
    const createColumns = () => {
        var columns = [];
        var textwrapwidth = 60;

        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        if (isAdminRole === true) {
            columns.push({
                label: 'Provider',
                name: 'ProviderName'
            });
        }

        columns.push({
            label: 'User',
            name: 'FirmName'
        });
        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });
        columns.push({
            label: 'Number',
            name: 'ConsumerNumber'
        });
        columns.push({
            label: 'Op.Bal',
            name: 'OpeningBalance',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Amount',
            name: 'Amount',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Cl.Bal',
            name: 'ClosingBalance',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });
        columns.push({
            label: 'TransID',
            name: 'ReferenceNo'
        });

        if (isAdminRole === true) {
            columns.push({
                label: 'ProviderRequestID',
                name: 'ProviderRequestID'
            });
            columns.push({
                label: 'Update',
                name: 'TransactionMasterID',
                icon: 'EditIcon',
                // disabledOn: ['FAILURE', 'SUCCESS'],
                statusIndex: 10,
                iconButtonWithRow: true,
                callback: (row) => {
                    Edit(row);
                }
            });
        }
        var options = [
            {
                Title: 'Complaint',
                disabledOn: ['FAILURE', 'PROCESS'],
                statusIndex: 11,
                Callback: (values) => {
                    RaiseComplaint(values[1]);
                }
            }
        ];

        options.push({
            Title: 'Hit Callback',
            statusIndex: 11,
            Callback: (values) => {
                ResendCallback(values[1]);
            }
        });

        columns.splice(0, 0, {
            label: ' ',
            name: 'TransactionMasterID',
            MoreMenu: true,
            options: options
        });

        setMainColumns(columns);
    };

    const createLogColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'id',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });
        columns.push({
            label: 'Response',
            name: 'Response',
            ReadOnlyTextArea: true
        });
        columns.push({
            label: 'Request',
            name: 'Request',
            ReadOnlyTextArea: true
        });

        setLogColumns(columns);
    };

    const createCallbackColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'id',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });
        columns.push({
            label: 'Provider Callback',
            name: 'ProviderReply',
            ReadOnlyTextArea: true
        });

        setCallbackColumns(columns);
    };
    const createApiCallbackColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'id',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Send Callback',
            name: 'Request',
            ReadOnlyTextArea: true
        });
        columns.push({
            label: 'Response',
            name: 'Response',
            ReadOnlyTextArea: true
        });

        setApiUserCallbackColumns(columns);
    };

    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
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
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {({ setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Quick Search
                            </h2>

                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="Consumer Number / RequestID"
                                                name="ConsumerNumber"
                                                type="text"
                                                title="ConsumerNumber / RequestID"
                                                inputRef={SearchInputRef}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialDatePickerField
                                                name="FromDate"
                                                label="From Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialDatePickerField
                                                name="ToDate"
                                                label="To Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Search"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                style={{
                                                    marginTop: '11px'
                                                }}
                                                onClick={(e) => {
                                                    setFieldValue(
                                                        'ApiLog',
                                                        false
                                                    );

                                                    setFieldValue(
                                                        'APIUserCallback',
                                                        false
                                                    );

                                                    handleSubmit(e);
                                                }}
                                                fullWidth
                                                disabled={loadingOpen}
                                            >
                                                Search
                                            </PrimaryButton>

                                            {loadingOpen && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </Grid>

                                        {isAdminRole ? (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <SecondaryButton
                                                    type="submit"
                                                    title="Api Log"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    style={{
                                                        marginTop: '11px'
                                                    }}
                                                    fullWidth
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'ApiLog',
                                                            true
                                                        );

                                                        setFieldValue(
                                                            'APIUserCallback',
                                                            false
                                                        );

                                                        handleSubmit(e);
                                                    }}
                                                    disabled={loadingOpen}
                                                >
                                                    API Log
                                                </SecondaryButton>
                                                {loadingOpen && (
                                                    <CircularProgress
                                                        size={24}
                                                        className={
                                                            classes.buttonProgress
                                                        }
                                                    />
                                                )}
                                            </Grid>
                                        ) : null}
                                        {isAdminRole ? (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <PrimaryButton
                                                    type="submit"
                                                    title="API User Callback"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    style={{
                                                        marginTop: '11px'
                                                    }}
                                                    fullWidth
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'APIUserCallback',
                                                            true
                                                        );

                                                        setFieldValue(
                                                            'ApiLog',
                                                            false
                                                        );

                                                        handleSubmit(e);
                                                    }}
                                                    disabled={loadingOpen}
                                                >
                                                    API User CallBack
                                                </PrimaryButton>
                                                {loadingOpen && (
                                                    <CircularProgress
                                                        size={24}
                                                        className={
                                                            classes.buttonProgress
                                                        }
                                                    />
                                                )}
                                            </Grid>
                                        ) : null}
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            {isTransactionVisible ? (
                <>
                    <h3 className={classes.PaperHead_h2}>Transaction</h3>
                    <ReactTable
                        data={transaction}
                        columns={mainColumns}
                        headerToolbar={false}
                    />
                </>
            ) : null}

            {isProviderLogVisible ? (
                <>
                    <h3 className={classes.PaperHead_h2}>Initial log</h3>

                    <ReactTable
                        data={ProviderLog}
                        columns={LogColumns}
                        headerToolbar={false}
                    />
                    <h3 className={classes.PaperHead_h2}>
                        CallBack from Provider
                    </h3>
                    <ReactTable
                        data={Callback}
                        columns={CallbackColumns}
                        headerToolbar={false}
                    />
                </>
            ) : null}
            {isApiUserLogVisible ? (
                <>
                    <h3 className={classes.PaperHead_h2}>
                        CallBack sent to API User
                    </h3>
                    <ReactTable
                        data={ApiUserCallback}
                        columns={ApiUserCallbackColumns}
                        headerToolbar={false}
                    />
                </>
            ) : null}

            {isModalOpen ? (
                <CustomModal
                    isOpen={isModalOpen}
                    title="Manual Status update"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsModalOpen(value);
                        refreshForm();
                    }}
                >
                    <StatusEdit
                        TransactiomMasterID={transactionMasterID}
                        onDismissCallback={(value) => {
                            setIsModalOpen(value);
                            Select();
                        }}
                    />
                </CustomModal>
            ) : null}
            {isOpen ? (
                <CustomModalWithScroll
                    isOpen={isOpen}
                    title="Complaint"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsOpen(value);
                    }}
                >
                    <AddComplaint
                        IsEdit={false}
                        TransactionMasterID={transactionMasterID}
                        ComplaintMasterID={0}
                        onDismissCallback={(value) => {
                            setIsOpen(value);
                        }}
                    />
                </CustomModalWithScroll>
            ) : null}
        </CustomContainer>
    );
};
