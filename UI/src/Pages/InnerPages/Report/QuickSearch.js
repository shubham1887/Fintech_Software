import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { Search as SELECT_SEARCH_NUMBER } from 'Components/Api/TransactionReportAPI';
import { SelectInitialLogByUserID as SELECT_SEARCH_API_USER_LOG } from 'Components/Api/APIUserCallbackApi';
import { SelectCallBackByUserID as SELECT_SEARCH_API_USER_CALLBACK } from 'Components/Api/APIUserCallbackApi';
import { AddComplaint } from 'Pages/InnerPages/Complaints/Add';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import { ResendCallback as RESEND_CALLBACK } from 'Components/Api/APIUserCallbackApi';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, Grid, Paper, Button } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { LOGOUT } from 'ConstComponents';
import Box from '@material-ui/core/Box';
import SearchIcon from 'Components/SvgButtons/SearchIcon';
import {
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID,
    API_USER_CODE
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';

const CustomContainer = HOCLoader(Container);

export const RetailerSearch = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [isOpen, setIsOpen] = useState(false);
    const [transactionMasterID, setTransactionMasterID] = useState(0);
    const [transaction, setTransaction] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [LogColumns, setLogColumns] = useState([]);
    const [ApiUserCallbackColumns, setApiUserCallbackColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isTransactionVisible, setTransactionVisible] = useState(false);
    const [IsAPIUserRole, setIsAPIUserRole] = useState(false);
    const [isApiUserLogVisible, setisApiUserLogVisible] = useState(false);
    const [ApiUserLog, setApiUserLog] = useState([]);
    const [ApiUserCallback, setApiUserCallback] = useState([]);

    var currentUsertypeid = localStorage.getItem('UserTypeID');

    const CheckUserRole = () => {
        if (API_USER_CODE === parseInt(currentUsertypeid))
            setIsAPIUserRole(true);
    };

    useEffect(() => {
        CheckUserRole();
    });

    var FormObj = {
        ToDate: new Date(),
        FromDate: new Date().addDays(-7),
        ConsumerNumber: ''
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;

        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);

        if (values.ApiLog) {
            setApiUserLog([]);
            setApiUserCallback([]);
            createLogColumns();
            createApiCallbackColumns();
            var Log_response = await SELECT_SEARCH_API_USER_LOG(requestObj);
            setLoadingOpen(false);

            if (!Log_response.status) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage(Log_response.data);

                return;
            }
            var Logdata = JSON.parse(Log_response.data);
            setisApiUserLogVisible(true);
            setApiUserLog(Logdata);

            var Callback_response = await SELECT_SEARCH_API_USER_CALLBACK(
                requestObj
            );
            var callbackdata = JSON.parse(Callback_response.data);
            setApiUserCallback(callbackdata);
            values.ApiLog = false;
            return;
        }
        createColumns();
        setTransaction([]);
        var response = await SELECT_SEARCH_NUMBER(requestObj);

        setLoadingOpen(false);
        var data = JSON.parse(response.data);
        setTransactionVisible(true);
        setTransaction(data);
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
        columns.push({
            label: 'Complaint',
            name: 'TransactionMasterID',
            disabledOn: ['FAILURE', 'PROCESS'],
            statusIndex: 9,
            button: true,
            callback: (value) => {
                RaiseComplaint(value);
            }
        });
        if (IsAPIUserRole === true) {
            columns.push({
                label: 'Resend Callback',
                name: 'TransactionMasterID',
                refresh: true,
                callback: (row) => {
                    ResendCallback(row[0]);
                }
            });
        }

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
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Quick Search
                            </h2>

                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={4}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="Consumer Number"
                                                name="ConsumerNumber"
                                                type="text"
                                                placeholder="Consumer Number"
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
                                            <Box width="90%">
                                                <div
                                                    className={
                                                        classes.SubmitWrapper
                                                    }
                                                >
                                                    <PrimaryButton
                                                        type="submit"
                                                        title="Search"
                                                        size="medium"
                                                        className={
                                                            classes.CustomPrimaryButtonStyle
                                                        }
                                                        onClick={(e) => {
                                                            setFieldValue(
                                                                'ApiLog',
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
                                                </div>
                                            </Box>
                                        </Grid>
                                        {IsAPIUserRole ? (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <Box width="90%">
                                                    <div
                                                        className={
                                                            classes.SubmitWrapper
                                                        }
                                                    >
                                                        <Button
                                                            type="submit"
                                                            title="API User Callback"
                                                            className={
                                                                classes.CustomPrimaryButtonStyle
                                                            }
                                                            style={{
                                                                marginTop:
                                                                    '11px'
                                                            }}
                                                            fullWidth
                                                            onClick={(e) => {
                                                                setFieldValue(
                                                                    'ApiLog',
                                                                    true
                                                                );

                                                                handleSubmit(e);
                                                            }}
                                                            disabled={
                                                                loadingOpen
                                                            }
                                                        >
                                                            API User Log
                                                        </Button>
                                                        {loadingOpen && (
                                                            <CircularProgress
                                                                size={24}
                                                                className={
                                                                    classes.buttonProgress
                                                                }
                                                            />
                                                        )}
                                                    </div>
                                                </Box>
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
            {isApiUserLogVisible ? (
                <>
                    <h3 className={classes.PaperHead_h2}>Initial log</h3>

                    <ReactTable
                        data={ApiUserLog}
                        columns={LogColumns}
                        headerToolbar={false}
                    />
                    <h3 className={classes.PaperHead_h2}>CallBack Log</h3>
                    <ReactTable
                        data={ApiUserCallback}
                        columns={ApiUserCallbackColumns}
                        headerToolbar={false}
                    />
                </>
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
