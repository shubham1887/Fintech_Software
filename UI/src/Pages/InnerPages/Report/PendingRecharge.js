import {
    Button,
    Container,
    Grid,
    MenuItem,
    CircularProgress,
    Paper
} from '@material-ui/core';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import {
    GetRechargeTransactionStatus as TRANSACTION_STATUS,
    RefundAll as REFUND_ALL,
    SuccessAll as SUCCESS_ALL,
    Resend as TRANSACTION_RESEND,
    ResendAll as RESEND_ALL,
    StatusCheckAll as STATUS_CHECK_ALL,
    Shift as TRANSACTION_SHIFT,
    ShiftAll as SHIFT_ALL,
    UpdateStatusmanually as STATUS_UPDATE
} from 'Components/Api/TransactionAPI';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { SelectByPrimeServices as SERVICE_SELECT_PRIME } from 'Components/Api/ServiceApi';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import {
    SelectPending as SELECT_PENDING_TRANSACTION,
    SelectPendingWithFilter as SELECT_PENDING_TRANSACTIONS_WITH_FILTER
} from 'Components/Api/TransactionReportAPI';
import FormHeader from 'Components/FormHeader';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

import {
    MESSAGE_CLOSE_AFTER,
    ADMIN_CODE,
    SUPPORT_CODE
} from 'Components/Helper/Messages';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import HOCLoader from 'Components/Loader/HOCLoader';
import { CustomModal } from 'Components/Modal/CustomModal';
import { ProviderModal } from 'Components/Modal/ProviderModel';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { SelectByIDLog } from 'Pages/InnerPages/Report/LogByTransactionMasterID';
import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';

import { HeaderPendingSummary } from 'Components/SummaryCards/HeaderPendingSummary';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

const CustomContainer = HOCLoader(Container);

export const PendingRecharge = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [Services, setServices] = useState([]);
    const [headerSummary, setHeaderSummary] = useState({});
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [IsStatusCheck, setIsStatusCheck] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [modalErrorMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [openProvider, setOpenProvider] = useState(false);
    const [openProviderForShiftAll, setOpenProviderForShiftAll] = useState(
        false
    );
    const [transactionsForShift, setTransactionsForShift] = useState([]);
    const [consumerNumber, setConsumerNumber] = useState(0);
    const [Amount, setAmount] = useState(0);
    const [TransactionID, setTransactionMasterID] = useState();
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isAlertOpenRefund, setIsAlertOpenRefund] = useState(false);
    const [isAlertOpenRefundAll, setIsAlertOpenRefundAll] = useState(false);
    const [isAlertOpenSuccessAll, setIsAlertOpenSuccessAll] = useState(false);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [isAlertOpenSuccess, setIsAlertOpenSuccess] = useState(false);
    const [isAlertOpenResend, setIsAlertOpenResend] = useState(false);
    const [isAlertOpenResendAll, setIsAlertOpenResendAll] = useState(false);
    const [isAlertOpenResendWithout, setIsAlertOpenResendWithout] = useState(
        false
    );
    const [providers, setProviders] = useState([]);
    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    const [isStatusCheckAllClicked, setIsStatusCheckAllClicked] = useState(
        false
    );

    const [buttonPressed, setButtonPressed] = useState('');

    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const currentUserTypeID = localStorage.getItem('UserTypeID');
    const [operators, setOperators] = useState([]);

    const SelectPending = async () => {
        setLoadingOpen(true);
        const response = await SELECT_PENDING_TRANSACTION();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        let d = JSON.parse(response.data);
        setHeaderSummary(d.HeaderSummary);
        setItemList(d.list);
        setLoadingOpen(false);
    };

    const SelectPendingRechargeWithFilter = async (values) => {
        setItemList([]);
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
            let Message = data;
            setIsError(true);
            setSeverity('error');
            setResponseMessage(Message);

            return;
        }

        let d = JSON.parse(data);

        setHeaderSummary(d.HeaderSummary);
        setItemList(d.list);
    };

    const StatusCheck = async (request) => {
        let inputObj = {
            TransactionMasterID: request
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoadingOpen(true);

        var response = await TRANSACTION_STATUS(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        SelectPending();
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
    const selectProvider = async () => {
        setLoadingOpen(true);
        const { isAuthorized, status, data } = await PROVIDER_DROP_DOWN();
        setLoadingOpen(false);
        if (!isAuthorized) LOGOUT();

        if (!status) return;

        setProviders(JSON.parse(data));
    };
    const ReSendClick = async (request) => {
        let jsonObj = {
            TransactionMasterID: request.TransactionMasterID,
            IsStatusCheck: IsStatusCheck
        };
        var requestJson = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await TRANSACTION_RESEND(requestJson);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            setIsError(true);
        }
        setIsError(true);
        setIsStatusCheck(false);
        SelectPending();
    };

    const ThroughStatusCheckClick = async (TransactionMasterID, BtnTitle) => {
        setIsStatusCheck(true);
        if (BtnTitle === 'SHIFT') {
            ShiftClick(TransactionMasterID);
        }
        if (BtnTitle === 'RESEND') {
            var RequestObj = {
                TransactionMasterID: TransactionMasterID
            };
            setIsAlertOpenResend(true);
            setConfirmAlertRequest(RequestObj);
        }
    };

    const WithoutStatusCheckClick = async (TransactionMasterID, BtnTitle) => {
        if (BtnTitle === 'SHIFT') {
            ShiftClick(TransactionMasterID);
        }
        if (BtnTitle === 'RESEND') {
            var RequestObj = {
                TransactionMasterID: TransactionMasterID
            };
            setIsAlertOpenResendWithout(true);
            setConfirmAlertRequest(RequestObj);
        }
    };

    const ShiftClick = async (value) => {
        setOpenProvider(true);
        setTransactionMasterID(value);
    };

    const passProviderShift = async (value) => {
        let jsonObj = {
            ProviderID: value.ProviderID,
            TransactionMasterID: TransactionID,
            IsStatusCheck: IsStatusCheck
        };

        let request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await TRANSACTION_SHIFT(request);
        setOpenProvider(false);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        setIsStatusCheck(false);
        SelectPending();
    };

    const logClick = async (request, ConsumerNumber, Amount) => {
        setIsModalOpen(true);
        setTransactionMasterID(request);
        setConsumerNumber(ConsumerNumber);
        setAmount(Amount);
    };

    const createColumns = () => {
        let columns = [];

        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            srno: true
        });
        columns.push({
            label: 'Log',
            name: 'TransactionMasterID',
            buttonTitle: 'Log',
            testbutton: true,
            callback: (value, row) => {
                logClick(value, row[4], row[6]);
            }
        });

        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        columns.push({
            label: 'Name',
            name: 'FirmName'
        });
        columns.push({
            label: 'Number',
            name: 'ConsumerNumber'
        });

        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });

        columns.push({
            label: 'Amount',
            name: 'Amount'
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
            label: 'With status check',
            name: 'TransactionMasterID',
            CustomShiftResend: true,
            callback: (TransactionMasterID, BtnTitle) => {
                ThroughStatusCheckClick(TransactionMasterID, BtnTitle);
            }
        });

        columns.push({
            label: 'Without status check',
            name: 'TransactionMasterID',
            CustomShiftResend: true,
            callback: (TransactionMasterID, BtnTitle) => {
                WithoutStatusCheckClick(TransactionMasterID, BtnTitle);
            }
        });
        columns.push({
            label: '',
            name: 'TransactionMasterID',
            hide: true
        });
        columns.push({
            label: 'Check',
            name: 'TransactionMasterID',
            button: true,
            buttonTitle: 'Check',
            callback: (value) => {
                StatusCheck(value);
            }
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });

        columns.push({
            label: 'ProviderRequestID',
            name: 'ProviderRequestID'
        });
        columns.push({
            label: 'AdditionalParam',
            name: 'AdditionalParam1'
        });

        setMainColumns(columns);
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
    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
        if (SUPPORT_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
    };

    useEffect(() => {
        CheckUserRole();
        createColumns();
        selectServices();
        selectProvider();
        SelectPending();
    }, []);

    const resetAllFlag = () => {
        setIsAlertOpenRefundAll(false);
        setIsAlertOpenSuccessAll(false);
        setIsAlertOpenResend(false);
        setIsAlertOpenResendAll(false);
    };

    const ProcessUpdatedData = (data) => {
        setShouldGetUpdatedDataFromTable(false);

        resetAllFlag();

        if (isStatusCheckAllClicked) {
            StatusCheckAll(data);
            return;
        }
        if (buttonPressed === 'RefundAll') {
            setIsAlertOpenRefundAll(true);
            setConfirmAlertRequest(data);
            return;
        }
        if (buttonPressed === 'SuccessAll') {
            setIsAlertOpenSuccessAll(true);
            setConfirmAlertRequest(data);
            return;
        }
        if (buttonPressed === 'ResendAll') {
            setIsAlertOpenResendAll(true);
            setConfirmAlertRequest(data);
            return;
        }
        if (buttonPressed === 'ShiftAll') {
            setButtonPressed('ShiftAll');
            ShiftAllClick(data);
            return;
        }
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
            request.push({
                ID: value
            });
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

    const RefundAll = async (requestArr) => {
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
    const SuccessAll = async (requestArr) => {
        if (requestArr.length <= 0) {
            setSeverity('error');
            setResponseMessage(
                'Please select one or more transactions to success'
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

        let { isAuthorized, status, data } = await SUCCESS_ALL(
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
        if (requestArr.length <= 0) {
            setSeverity('error');
            setResponseMessage(
                'Please select one or more transactions to resend'
            );
            setIsError(true);
            return;
        }
        let request = [];
        requestArr.forEach((element) => {
            let { value } = element[0];
            request.push({
                TransactionMasterID: value,
                IsStatusCheck: false
            });
        });
        setLoadingOpen(true);
        let { isAuthorized, status, data } = await RESEND_ALL(
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

        SelectPending();
    };

    const ShiftAllClick = (requestArr) => {
        if (requestArr.length <= 0) {
            setSeverity('error');
            setResponseMessage(
                'Please select one or more transactions to shift'
            );
            setIsError(true);
            return;
        }
        setTransactionsForShift(requestArr);
        setOpenProviderForShiftAll(true);
    };

    const ShiftAll = async (providerID) => {
        let requestArr = transactionsForShift;
        let request = [];
        requestArr.forEach((element) => {
            let { value } = element[0];
            request.push({
                TransactionMasterID: value,
                IsStatusCheck: false,
                ProviderID: providerID
            });
        });
        setLoadingOpen(true);
        let { isAuthorized, status, data } = await SHIFT_ALL(
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
    const PendingRechargeList = (
        <ReactTable
            data={itemList}
            columns={mainColumns}
            rowsPerPage={1000}
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
    const ShiftForm = (
        <ProviderModal
            isOpen={openProvider}
            title="Shift"
            error={modalErrorMessage}
            submitCallback={(value) => {
                passProviderShift(value);
            }}
            resendCallback={(value) => {}}
            onDismissCallback={(value) => {
                setOpenProvider(value);
            }}
        />
    );
    const ShiftAllForm = (
        <ProviderModal
            isOpen={openProviderForShiftAll}
            title="Shift"
            error={modalErrorMessage}
            submitCallback={(value) => {
                let { ProviderID } = value;
                setOpenProviderForShiftAll(false);
                ShiftAll(ProviderID);
            }}
            resendCallback={(value) => {}}
            onDismissCallback={(value) => {
                setOpenProviderForShiftAll(value);
            }}
        />
    );
    const APILogModal = (
        <CustomModal
            isOpen={isModalOpen}
            title="API Log"
            width="75%"
            onDismissCallback={(value) => {
                setIsModalOpen(value);
            }}
        >
            <SelectByIDLog
                TransactionMasterID={TransactionID}
                ConsumerNumber={consumerNumber}
                Amount={Amount}
                setLoading={(value) => setLoadingOpen(value)}
            />
        </CustomModal>
    );
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
    const RefundAllConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpenRefundAll}
            title="Refund Confirmation"
            message="Are you sure to refund this selected transaction?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenRefundAll(false);
                RefundAll(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpenRefundAll(false);
            }}
        />
    );
    const SuccessAllConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpenSuccessAll}
            title="Success Confirmation"
            message="Are you sure to success this selected transaction?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenSuccessAll(false);
                SuccessAll(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpenSuccessAll(false);
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

    const ResendAllConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpenResendAll}
            title="Resend Confirmation"
            message="Are you sure to resend through status check?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenResendAll(false);
                ResendAll(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpenResendAll(false);
            }}
        />
    );

    const ResendConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpenResend}
            title="Resend Confirmation"
            message="Are you sure to resend through status check?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenResend(false);
                ReSendClick(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpenResend(false);
            }}
        />
    );
    const ResendConfirmationWithoutStatusCheck = (
        <ConfirmAlert
            isOpen={isAlertOpenResendWithout}
            title="Resend Confirmation"
            message="Are you sure to resend without status check?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenResendWithout(false);
                ReSendClick(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpenResendWithout(false);
            }}
        />
    );
    const CustomContainerCloseCallback = (alertState) => {
        setIsError(alertState);
    };

    const handleStatusCheckAllClick = (e) => {
        setShouldGetUpdatedDataFromTable(true);
        setIsStatusCheckAllClicked(true);
    };
    const handleRefundAllClick = (e) => {
        setShouldGetUpdatedDataFromTable(true);
        setButtonPressed('RefundAll');
    };
    const handleSuccessAllClick = (e) => {
        setShouldGetUpdatedDataFromTable(true);
        setButtonPressed('SuccessAll');
    };

    const handleResendAllClick = (e) => {
        setShouldGetUpdatedDataFromTable(true);

        setButtonPressed('ResendAll');
    };
    const handleShiftAllClick = (e) => {
        setShouldGetUpdatedDataFromTable(true);

        setButtonPressed('ShiftAll');
    };

    const selectServices = async () => {
        const response = await SERVICE_SELECT_PRIME();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const OnOperatorChange = async (name, value, setFieldValue) => {
        var ServiceID = value; //event.target.value;

        setFieldValue(name, value);

        selectOperator(ServiceID);
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
        let request = JSON.stringify(requestObj);

        const {
            isAuthorized,
            status,
            data
        } = await OPERATOR_SELECT_BY_SERVICEID(request);

        if (!isAuthorized) LOGOUT();

        if (!status) setIsError(true);

        if (data !== 'Requested operators not found') {
            setOperators(JSON.parse(data));
        }
    };
    const ModelObj = {
        ProviderID: 0,
        OperatorID: 0,
        ConsumerNumber: '',
        ServiceID: 0
    };

    const handleSubmit = async (values, { setSubmitting }, resetForm) => {
        setSubmitting(true);
        SelectPendingRechargeWithFilter(values);
        setSubmitting(false);
    };

    const ProviderList = providers.map(
        ({ ProviderID, ProviderName }, index) => (
            <MenuItem key={index} value={ProviderID}>
                {ProviderName}
            </MenuItem>
        )
    );
    const operatorsList = operators.map(({ OperatorID, OperatorName }) => (
        <MenuItem key={OperatorName} value={OperatorID}>
            {OperatorName}
        </MenuItem>
    ));
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
                                label="Select Provider"
                                name="ProviderID"
                            >
                                <MenuItem key="0" value={0}>
                                    Select Provider
                                </MenuItem>
                                {ProviderList}
                            </MaterialSelect>
                        </Grid>
                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <MaterialSelect
                                label="Service"
                                name="ServiceID"
                                onCustomCallback={(name, value) => {
                                    OnOperatorChange(
                                        name,
                                        value,
                                        setFieldValue
                                    );
                                }}
                                title="Service"
                            >
                                <MenuItem key="0" value="0">
                                    All Service
                                </MenuItem>
                                {Services.map((Item) => (
                                    <MenuItem
                                        key={Item.ServiceID}
                                        value={Item.ServiceID}
                                    >
                                        {Item.ServiceName}
                                    </MenuItem>
                                ))}
                            </MaterialSelect>
                        </Grid>
                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <MaterialSelect
                                label="Operator"
                                name="OperatorID"
                                title="Operator"
                            >
                                <MenuItem key="0" value={0}>
                                    All Operator
                                </MenuItem>
                                {operatorsList}
                            </MaterialSelect>
                        </Grid>
                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <MaterialNumberField
                                label={'Consumer Number'}
                                name="ConsumerNumber"
                                length="16"
                                type="text"
                                placeholder={'Consumer Number'}
                            />
                        </Grid>

                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <Button
                                type="submit"
                                title="submit"
                                className={classes.CustomPrimaryButtonStyle}
                                disabled={loadingOpen}
                                fullWidth
                            >
                                Search
                            </Button>
                            {loadingOpen && (
                                <CircularProgress
                                    size={24}
                                    className={classes.buttonProgress}
                                />
                            )}
                        </Grid>
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <Button
                                type="button"
                                className={classes.CustomPrimaryButtonStyle}
                                style={{
                                    background: 'orange'
                                }}
                                disabled={loadingOpen}
                                title="Resend"
                                onClick={handleResendAllClick}
                                fullWidth
                            >
                                Resend
                            </Button>
                        </Grid>
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <Button
                                type="button"
                                className={classes.CustomPrimaryButtonStyle}
                                style={{
                                    background: 'green'
                                }}
                                disabled={loadingOpen}
                                title="Success"
                                onClick={handleSuccessAllClick}
                                fullWidth
                            >
                                Success
                            </Button>
                        </Grid>
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <Button
                                type="button"
                                className={classes.CustomPrimaryButtonStyle}
                                style={{
                                    background: 'red'
                                }}
                                disabled={loadingOpen}
                                title="Refund"
                                onClick={handleRefundAllClick}
                                fullWidth
                            >
                                Refund
                            </Button>
                        </Grid>
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <Button
                                type="button"
                                className={classes.CustomPrimaryButtonStyle}
                                style={{
                                    background: 'blue'
                                }}
                                disabled={loadingOpen}
                                title="Shift"
                                onClick={handleShiftAllClick}
                                fullWidth
                            >
                                Shift
                            </Button>
                        </Grid>
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <Button
                                type="button"
                                className={classes.CustomPrimaryButtonStyle}
                                style={{
                                    background: 'green'
                                }}
                                disabled={loadingOpen}
                                title="Check"
                                onClick={handleStatusCheckAllClick}
                                fullWidth
                            >
                                Check
                            </Button>
                        </Grid>
                    </Grid>
                </Grid>
            </Form>
        );
    };
    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={CustomContainerCloseCallback}
        >
            <FormHeader
                title="Pending Recharge"
                shouldDisplayButton={false}
                buttonName=""
                link="/"
            />
            <Paper
                className={classes.PaperStyle}
                style={{
                    marginBottom: '10px'
                }}
            >
                <Formik
                    initialValues={ModelObj}
                    onSubmit={handleSubmit}
                    // validationSchema={validationSchema}
                    resetForm={ModelObj}
                    render={PendingRechargeForm}
                />
            </Paper>
            <HeaderPendingSummary headerSummary={headerSummary} />
            {PendingRechargeList}
            {openProvider ? ShiftForm : null}
            {openProviderForShiftAll ? ShiftAllForm : null}
            {isModalOpen ? APILogModal : null}
            {isAlertOpenRefund ? RefundConfirmation : null}
            {isAlertOpenRefundAll ? RefundAllConfirmation : null}
            {isAlertOpenSuccess ? SuccessConfirmation : null}
            {isAlertOpenResend ? ResendConfirmation : null}

            {isAlertOpenResendAll ? ResendAllConfirmation : null}
            {isAlertOpenSuccessAll ? SuccessAllConfirmation : null}
            {isAlertOpenResendWithout
                ? ResendConfirmationWithoutStatusCheck
                : null}
        </CustomContainer>
    );
};
