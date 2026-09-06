import React, { useState, useEffect } from 'react';
import { Container, Paper, MenuItem, Grid } from '@material-ui/core';
import { Formik, Form } from 'formik';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { LOGOUT } from 'ConstComponents';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import {
    SelectDmtPending as SELECT_DMI_PENDING_TRANSACTION,
    SelectDMTPendingWithFilter as SELECT_PENDING_TRANSACTIONS_WITH_FILTER
} from 'Components/Api/TransactionReportAPI';
import { HeaderPendingSummary } from 'Components/SummaryCards/HeaderPendingSummary';
import {
    GetDMTTransactionStatus as TRANSACTION_STATUS,
    UpdateStatusmanually as STATUS_UPDATE,
    RefundAll as REFUND_ALL,
    DMTTransactionResendAll as RESEND_ALL,
    DMTStatusCheckAll as STATUS_CHECK_ALL
} from 'Components/Api/TransactionAPI';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import {
    MONEY_TRANSFER_CODE,
    ADMIN_CODE,
    MESSAGE_CLOSE_AFTER,
    SUPPORT_CODE
} from 'Components/Helper/Messages';
import { SelectByIDLog } from 'Pages/InnerPages/Report/LogByTransactionMasterID';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
const CustomContainer = HOCLoader(Container);

export const PendingDmt = () => {
    const classes = commonStyle();
    const currentUserTypeID = localStorage.getItem('UserTypeID');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [headerSummary, setHeaderSummary] = useState({});
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [transactionMasterID, setTransactionMasterID] = useState(0);
    const [itemList, setItemList] = useState([]);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isInitialLoading, setIsInitialLoading] = useState(true);
    const [isAlertOpenRefund, setIsAlertOpenRefund] = useState(false);
    const [isAlertOpenSuccess, setIsAlertOpenSuccess] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [buttonPressed, setButtonPressed] = useState('');
    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    const [isStatusCheckAllClicked, setIsStatusCheckAllClicked] = useState(
        false
    );

    //const [buttonPressed, setButtonPressed] = useState('');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [isAlertOpenResend, setIsAlertOpenResend] = useState(false);
    const [isRefundAllClicked, setIsRefundAllClicked] = useState(false);
    const [isResendAllClicked, setIsResendAllClicked] = useState(false);
    const [consumerNumber, setConsumerNumber] = useState(0);
    const [Amount, setAmount] = useState(0);
    const ModelObj = {
        ProviderID: 0,
        OperatorID: 0,
        ConsumerNumber: '',
        SenderNo: ''
    };
    const handleSubmit = async (values, { setSubmitting }) => {
        setIsInitialLoading(false);
        setSubmitting(true);

        setLoadingOpen(true);
        const {
            isAuthorized,
            status,
            data
        } = await SELECT_PENDING_TRANSACTIONS_WITH_FILTER(
            JSON.stringify(values)
        );

        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setResponseMessage(data);
            setSeverity('error');
            setIsError(true);
            return;
        }
        var d = JSON.parse(data);
        setHeaderSummary(d.HeaderSummary);
        setItemList(d.list);
        setSubmitting(false);
    };

    const SelectPending = async () => {
        setLoadingOpen(true);
        const response = await SELECT_DMI_PENDING_TRANSACTION();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var d = JSON.parse(response.data);
        setHeaderSummary(d.HeaderSummary);
        setItemList(d.list);
    };

    const StatusCheck = async (request) => {
        var inputObj = {
            ID: request
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoadingOpen(true);

        var response = await TRANSACTION_STATUS(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }

        var data = JSON.parse(response.data);
        setResponseMessage(data.Message);

        if (!data.Status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
    };
    const StatusUpdate = async (request) => {
        let jsonObj = {
            TransactionMasterID: request.TransactionMasterID,
            ReferenceNo: request.TxnID,
            Pin: request.Pin,
            Status: request.BtnTitle
        };

        let requestJson = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const { isAuthorized, status, data } = await STATUS_UPDATE(requestJson);

        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        setResponseMessage(data);

        if (!status) {
            setSeverity('error');
            setIsError(true);
        }
        setSeverity('success');
        setIsError(true);

        SelectPending();
    };
    const logClick = async (request, ConsumerNumber, Amount) => {
        setIsModalOpen(true);
        setTransactionMasterID(request);
        setConsumerNumber(ConsumerNumber);
        setAmount(Amount);
    };

    const StatusUpdateClick = (TransactionMasterID, TxnID, Pin, BtnTitle) => {
        if (!TxnID && BtnTitle === 'SUCCESS') {
            setIsError(true);
            setSeverity('error');
            setResponseMessage('Please enter valid transaction id');

            return;
        }
        let RequestObj = {
            TransactionMasterID: TransactionMasterID,
            TxnID: TxnID,
            Pin: Pin,
            BtnTitle: BtnTitle
        };
        if (BtnTitle === 'REFUND') setIsAlertOpenRefund(true);
        if (BtnTitle === 'SUCCESS') setIsAlertOpenSuccess(true);
        setConfirmAlertRequest(RequestObj);
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            srno: true
        });

        columns.push({
            label: 'TXN Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Sender Name',
            name: 'SenderName'
        });
        columns.push({
            label: 'Mobile Number',
            name: 'SenderNo'
        });
        columns.push({
            label: 'Account Number',
            name: 'ConsumerNumber'
        });
        columns.push({
            label: 'Bank Name',
            name: 'Bank'
        });
        columns.push({
            label: 'Beneficiary Name',
            name: 'BeneficiaryName'
        });
        columns.push({
            label: 'Payment Type',
            name: 'Type'
        });
        columns.push({
            label: 'Total Amount (₹)',
            name: 'Amount'
        });
        columns.push({
            label: 'TXN Ref. ID',
            name: 'ProviderRequestID'
        });
        columns.push({
            label: 'Success/Refund',
            name: 'TransactionMasterID',
            CustomSuccessFail: true,
            callback: (TransactionMasterID, TxnID, Pin, BtnTitle) => {
                StatusUpdateClick(TransactionMasterID, TxnID, Pin, BtnTitle);
            }
        });
        columns.push({
            label: 'Check Status',
            name: 'TransactionMasterID',
            button: true,
            buttonTitle: 'Check Status',
            callback: (value) => {
                StatusCheck(value);
            }
        });
        columns.push({
            label: 'Log',
            name: 'TransactionMasterID',
            buttonTitle: 'Log',
            testbutton: true,
            callback: (value, row) => {
                logClick(value, row[4], row[8]);
            }
        });

        setMainColumns(columns);
    };

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
        if (SUPPORT_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
    };
    useEffect(() => {
        CheckUserRole();
        createColumns();
        SelectPending();
    }, []);

    useEffect(() => {
        if (isInitialLoading) SelectPending();
    }, [isInitialLoading]);

    const handleStatusCheckAllClick = (e) => {
        setShouldGetUpdatedDataFromTable(true);
        setIsStatusCheckAllClicked(true);
    };
    const handleRefundAllClick = (e) => {
        setShouldGetUpdatedDataFromTable(true);
        //setIsRefundAllClicked(true);
        setButtonPressed('RefundAll');
    };

    const StatusCheckAll = async (requestArr) => {
        setIsStatusCheckAllClicked(false);

        if (requestArr.length <= 0) {
            setSeverity('error');
            setResponseMessage(
                'Please select one or more transactions to refund'
            );
            setIsError(true);
            return;
        }
        let request = [];

        requestArr.forEach((element) => {
            let { value } = element[0];
            request.push({ ID: value });
        });

        setLoadingOpen(true);

        let { isAuthorized, status, data } = await STATUS_CHECK_ALL(
            JSON.stringify(request)
        );
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        setResponseMessage(data);

        if (!status) {
            setSeverity('error');
            setIsError(true);
            return;
        }
        setSeverity('success');
        setIsError(true);
        setTimeout(() => {
            SelectPending();
        }, MESSAGE_CLOSE_AFTER);
    };

    const ResendAll = async (requestArr) => {
        setIsResendAllClicked(false);
        if (requestArr.length <= 0) {
            setSeverity('error');
            setResponseMessage(
                'Please select one or more transactions to refund'
            );
            setIsError(true);
            return;
        }
        let request = [];

        requestArr.forEach((element) => {
            let { value } = element[0];
            request.push({ ID: value });
        });

        setLoadingOpen(true);

        let { isAuthorized, status, data } = await RESEND_ALL(
            JSON.stringify(request)
        );
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        let d = JSON.parse(data);

        setResponseMessage(d.Message);

        if (!status) {
            setSeverity('error');
            setIsError(true);
            return;
        }
        setSeverity('success');
        setIsError(true);
        setTimeout(() => {
            SelectPending();
        }, MESSAGE_CLOSE_AFTER);
    };

    const RefundAll = async (requestArr) => {
        setIsRefundAllClicked(false);
        if (requestArr.length <= 0) {
            setSeverity('error');
            setResponseMessage(
                'Please select one or more transactions to refund'
            );
            setIsError(true);
            return;
        }
        let request = [];

        requestArr.forEach((element) => {
            let { value } = element[0];
            request.push({ ID: value });
        });

        setLoadingOpen(true);

        let { isAuthorized, status, data } = await REFUND_ALL(
            JSON.stringify(request)
        );
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        setResponseMessage(data);

        if (!status) {
            setSeverity('error');
            setIsError(true);
            return;
        }
        setSeverity('success');
        setIsError(true);
        setTimeout(() => {
            SelectPending();
        }, MESSAGE_CLOSE_AFTER);
    };
    const PendingRechargeForm = ({
        values,
        setFieldValue,
        handleChange,
        handleSubmit
    }) => {
        return (
            <Form>
                <Grid item xs={12}>
                    <Grid container spacing={2}>
                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <MaterialSelect
                                label="Payment Type"
                                name="PaymentType"
                                title="Payment Type"
                            >
                                <MenuItem key="ALL" value="ALL" selected={true}>
                                    All Type
                                </MenuItem>
                                <MenuItem key="IMPS" value="IMPS">
                                    IMPS
                                </MenuItem>
                                <MenuItem key="NEFT" value="NEFT">
                                    NEFT
                                </MenuItem>
                            </MaterialSelect>
                        </Grid>
                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <MaterialTextField
                                label={'Sender Number'}
                                name="SenderNo"
                                type="number"
                                placeholder={'Sender Number'}
                            />
                        </Grid>
                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <MaterialTextField
                                label={'Account Number'}
                                name="ConsumerNumber"
                                type="number"
                                placeholder={'Account Number'}
                            />
                        </Grid>

                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <PrimaryButton
                                type="submit"
                                title="Search"
                                className={classes.CustomPrimaryButtonStyle}
                                disabled={loadingOpen}
                                fullWidth
                            >
                                Search
                            </PrimaryButton>
                        </Grid>
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <PrimaryButton
                                type="button"
                                className={classes.CustomPrimaryButtonStyle}
                                style={{
                                    background: 'red',
                                    marginLeft: '16px'
                                }}
                                disabled={loadingOpen}
                                title="Refund"
                                onClick={handleRefundAllClick}
                                fullWidth
                            >
                                Refund
                            </PrimaryButton>
                        </Grid>
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <SecondaryButton
                                type="button"
                                className={classes.CustomPrimaryButtonStyle}
                                style={{
                                    background: 'green',
                                    marginLeft: '16px'
                                }}
                                disabled={loadingOpen}
                                title="Check"
                                onClick={handleStatusCheckAllClick}
                                fullWidth
                            >
                                Check
                            </SecondaryButton>
                        </Grid>
                    </Grid>
                </Grid>
            </Form>
        );
    };
    const RefundConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpenRefund}
            title="Refund Confirmation"
            message="Are you sure to refund this transaction?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenRefund(false);
                StatusUpdate(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpenRefund(false);
            }}
        />
    );

    const RefundConfirmationV2 = (
        <ConfirmAlert
            isOpen={isRefundAllClicked}
            title="Refund Confirmation"
            message="Are you sure to refund this selected transaction?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsRefundAllClicked(false);
                RefundAll(request);
            }}
            negativeButtonClick={() => {
                setIsRefundAllClicked(false);
            }}
        />
    );

    const ResendConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpenResend}
            title="Resend Confirmation"
            message="Are you sure to resend this transaction?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenResend(false);
            }}
            negativeButtonClick={() => {
                setIsAlertOpenResend(false);
            }}
        />
    );
    const SuccessConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpenSuccess}
            title="Success Confirmation"
            message="Are you sure to success this transaction?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenSuccess(false);
                StatusUpdate(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpenSuccess(false);
            }}
        />
    );
    const PendingDMTList = (
        <ReactTable
            data={itemList}
            columns={mainColumns}
            headerToolbar={false}
            SuccessFailHeader={true}
            primaryIndex={12}
            setLoadingOpen={(value) => {
                setLoadingOpen(value);
            }}
            setIsError={(value) => {
                setIsError(value);
            }}
            setSeverity={(value) => {
                setSeverity(value);
                SelectPending();
            }}
            setResponseMessage={(value) => {
                setResponseMessage(value);
                SelectPending();
            }}
            getUpdatedData={(data) => {
                ProcessUpdatedData(data);
                setShouldGetUpdatedDataFromTable(false);
            }}
            shouldGetUpdatedData={shouldGetUpdatedDataFromTable}
            isSelectableRows={'multiple'}
        />
    );
    const ProcessUpdatedData = (data) => {
        setShouldGetUpdatedDataFromTable(false);

        if (isStatusCheckAllClicked) {
            StatusCheckAll(data);
            return;
        }

        if (buttonPressed === 'RefundAll') {
            setIsRefundAllClicked(true);
            setConfirmAlertRequest(data);
            return;
        }

        if (isResendAllClicked) {
            ResendAll(data);
            return;
        }
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
            <Paper
                className={classes.PaperStyle}
                style={{ marginBottom: '10px' }}
            >
                <Formik
                    initialValues={ModelObj}
                    onSubmit={handleSubmit}
                    resetForm={ModelObj}
                    render={PendingRechargeForm}
                />
            </Paper>

            {isModalOpen ? (
                <CustomModalWithScroll
                    isOpen={isModalOpen}
                    title="API Log"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsModalOpen(value);
                    }}
                >
                    <SelectByIDLog
                        TransactionMasterID={transactionMasterID}
                        ConsumerNumber={consumerNumber}
                        Amount={Amount}
                        setLoading={(value) => setLoadingOpen(value)}
                    />
                </CustomModalWithScroll>
            ) : null}
            <HeaderPendingSummary headerSummary={headerSummary} />
            {PendingDMTList}
            {isAlertOpenRefund ? RefundConfirmation : null}
            {isRefundAllClicked ? RefundConfirmationV2 : null}
            {isAlertOpenResend ? ResendConfirmation : null}
            {isAlertOpenSuccess ? SuccessConfirmation : null}
        </CustomContainer>
    );
};
