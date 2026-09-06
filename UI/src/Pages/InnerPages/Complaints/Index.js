import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { SelectByIDLog } from 'Pages/InnerPages/Report/LogByTransactionMasterID';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { CustomModal } from 'Components/Modal/CustomModal';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import { MESSAGE_CLOSE_AFTER } from 'Components/Helper/Messages';

import { Container, Grid, MenuItem, Box } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID, SUPPORT_CODE } from 'Components/Helper/Messages';
import {
    Select as SELECT,
    AcceptAll as ACCEPT_ALL,
    RejectAll as REJECT_All
} from 'Components/Api/ComplaintsAPI';
import { ActionComplaint } from 'Pages/InnerPages/Complaints/Action';
import { ADMIN, SUPPORT_USER } from 'Components/Helper/UsertypeCode';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import excel from 'Assets/Images/excel.svg';
import Icon from '@material-ui/core/Icon';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const Complaints = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isOpen, setIsOpen] = useState(false);
    const [consumerNumber, setConsumerNumber] = useState(0);
    const [Amount, setAmount] = useState(0);
    const [TransactionMasterID, setTransactionMasterID] = useState(0);
    const [ComplaintMasterID, setComplaintMasterID] = useState(0);
    const [openFilter, setopenFilter] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [currentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );
    const [SearchText, setSearchText] = useState('');
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [isAlertOpenRefundAll, setIsAlertOpenRefundAll] = useState(false);
    const [isAlertOpenRejectAll, setIsAlertOpenRejectAll] = useState(false);
    const [isDateType, setDateType] = useState(false);
    const [buttonPressed, setButtonPressed] = useState('');
    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    
    const [ModelObj] = useState({
        FromDate: new Date().addDays(-60),
        ToDate: new Date(),
        Status: 'PROCESS',
        IsExcelDownload: false, 
        DateType: 'TODAY'
    });
   

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        createColumns();
        callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
    };
    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let inputObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            Status: formRef.current.values.Status,
            DateType:formRef.current.values.DateType,
            IsExcelDownload: formRef.current.values.IsExcelDownload
        };
        if (formRef.current.values.IsExcelDownload === false) setTransactions();

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await SELECT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);
        if (data.IsExcelDownload) {
            formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'Complaints Report');
        }
        if (SkipCount <= 0) setTotalCount(data.Count);
        setTransactions(data.list);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
        document.removeChild(a);
    }

    const AcceptAll = async (requestArr) => {
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

        let { isAuthorized, status, data } = await ACCEPT_ALL(
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

    const RejectAll = async (requestArr) => {
        if (requestArr.length <= 0) {
            setSeverity('error');
            setResponseMessage('Please select one or more transactions');
            setIsError(true);
            return;
        }
        let request = [];

        requestArr.forEach((element) => {
            let { value } = element[0];
            request.push({ ID: value });
        });

        setLoadingOpen(true);

        let { isAuthorized, status, data } = await REJECT_All(
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

    const SelectPending = async () => {
        let jsonObj = {
            TakeCount: 10,
            SkipCount: 0,
            ToDate: new Date(),
            FromDate: new Date(),
            DateType: 'TODAY'
        };

        var requestObj = JSON.stringify(jsonObj);

        setIsError(false);
        setLoading(true);
        createColumns();
        setTransactions([]);
        var response = await SELECT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        setSeverity('success');
        var data = JSON.parse(response.data);
        setTransactions(data.list);
        setTotalCount(data.Count);
    };

    const validationSchema = Yup.object({
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
  

    useEffect(() => {
        createColumns();
        SelectPending();
       
    }, []);
    const OnChange = (value) => {
        if (value === 'ALL') {
            setDateType(true);
        } else {
            setDateType(false);
            formRef.current.values.FromDate = new Date();
            formRef.current.values.ToDate= new Date();
        }
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ComplaintMasterID',
            hide: true
        });
        if (currentUserTypeID === ADMIN || currentUserTypeID === SUPPORT_USER) {
            var options = [
                {
                    Title: 'Action',
                    disabledOn: ['REJECT', 'APPROVE'],
                    statusIndex: 9,
                    Callback: (values) => {
                        RaiseComplaint(values[1], values[0]);
                    }
                }
            ];
            columns.push({
                label: 'More',
                name: 'TransactionMasterID',
                MoreMenu: true,
                options: options
            });
        }
        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            hide: true
        });

        columns.push({
            label: 'Firm',
            name: 'FirmName'
        });
        columns.push({
            label: 'Type',
            name: 'ComplaintType'
        });
        columns.push({
            label: 'Operator',
            name: 'Operator'
        });
        columns.push({
            label: 'Number',
            name: 'ConsumerNumber',
            bold: true
        });
        // columns.push({
        //     label: 'NewNumber',
        //     name: 'NewConsumerNumber'
        // });
        columns.push({
            label: 'TXN ID',
            name: 'TransactionMasterID'
        });
        columns.push({
            label: 'Date Type',
            name: 'DateType',

        });
        columns.push({
            label: 'Amount',
            name: 'Amount',
            numeric: true
        });
        columns.push({
            label: 'Old Status',
            name: 'OldStatus',
            customStatus: true
        });
        columns.push({
            label: 'Complaint Status',
            name: 'ComplaintStatus',
            customStatus: true
        });

        columns.push({
            label: 'Remark',
            name: 'CustomerRemark',
            ReadOnlyTextArea: true,
            textwrap: 60
        });

        columns.push({
            label: 'Admin Remark',
            name: 'AdminRemark',
            ReadOnlyTextArea: true,
            textwrap: 60
        });
        columns.push({
            label: 'Compl. Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Rch Date',
            name: 'TransactionDate'
        });
        if (currentUserTypeID === ADMIN || currentUserTypeID === SUPPORT_CODE) {
            columns.push({
                label: 'Provider',
                name: 'ProviderName'
            });

            columns.push({
                label: 'Log',
                name: 'TransactionMasterID',
                buttonTitle: 'Log',
                testbutton: true,
                callback: (value, row) => {
                    logClick(value, row[6], row[8]);
                }
            });
        }
        columns.push({
            label: 'OperatorTXNID',
            name: 'ReferenceNo'
        });

        setMainColumns(columns);
    };
    const logClick = async (request, ConsumerNumber, Amount) => {
        setIsModalOpen(true);
        setTransactionMasterID(request);
        setConsumerNumber(ConsumerNumber);
        setAmount(Amount);
    };
    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
    };

    const RaiseComplaint = (ID, ComplaintMasterID) => {
        setTransactionMasterID(ID);
        setComplaintMasterID(ComplaintMasterID);
        setIsOpen(true);
    };

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
                TransactionMasterID={TransactionMasterID}
                ConsumerNumber={consumerNumber}
                Amount={Amount}
                setLoading={(value) => setLoading(value)}
            />
        </CustomModal>
    );
    const loadingProgress = loading && (
        <CircularProgress size={24} className={classes.buttonProgress} />
    );

    const handleRefundAllClick = (e) => {
        setShouldGetUpdatedDataFromTable(true);
        setButtonPressed('RefundAll');
    };

    const handleRejectClick = (e) => {
        setShouldGetUpdatedDataFromTable(true);
        setButtonPressed('RejectAll');
    };

    const resetAllFlag = () => {
        setIsAlertOpenRefundAll(false);
        setIsAlertOpenRejectAll(false);
    };
    const ProcessUpdatedData = (data) => {
        setShouldGetUpdatedDataFromTable(false);

        resetAllFlag();

        if (buttonPressed === 'RefundAll') {
            setIsAlertOpenRefundAll(true);
            setConfirmAlertRequest(data);
            return;
        }

        if (buttonPressed === 'RejectAll') {
            setIsAlertOpenRejectAll(true);
            setConfirmAlertRequest(data);
            return;
        }
    };

    const RefundAllConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpenRefundAll}
            title="Accept Confirmation"
            message="Are you sure to refund this selected transaction?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenRefundAll(false);
                AcceptAll(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpenRefundAll(false);
            }}
        />
    );

    const RejectAllConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpenRejectAll}
            title="Reject Confirmation"
            message="Are you sure to reject selected complaint?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenRejectAll(false);
                RejectAll(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpenRejectAll(false);
            }}
        />
    );

    const ComplaintForm = ({ setFieldValue, handleSubmit, values }) => {
       
      
        return (
            <Form style={{ marginBottom: '15px' }}>
                <Grid item xs={12}>
                    <CustomAccordion
                        title={'Complaints Report'}
                        open={openFilter}
                        setOpen={(value) => setopenFilter(value)}
                    >
                        <Grid container spacing={2}>
                            {isDateType && (
                                <>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialDatePickerField
                                            name="FromDate"
                                            label="From Date"
                                            maxDate={new Date()}
                                            
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialDatePickerField
                                            name="ToDate"
                                            label="To Date"
                                            maxDate={new Date()}
                                           
                                        />
                                    </Grid>
                                </>
                            )}

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="Status"
                                    name="Status"
                                    title="Status"
                                >
                                    <MenuItem key="ALL" value="ALL">
                                        All Status
                                    </MenuItem>
                                    <MenuItem key="ACCEPT" value="ACCEPT">
                                        ACCEPT
                                    </MenuItem>
                                    <MenuItem key="REJECT" value="REJECT">
                                        REJECT
                                    </MenuItem>
                                    <MenuItem key="PROCESS" value="PROCESS">
                                        PROCESS
                                    </MenuItem>
                                </MaterialSelect>
                            </Grid>

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="DateType"
                                    name="DateType"
                                    title="DateType"
                                    onCustomCallback={(name, value) => {
                                        OnChange(
                                            value
                                            
                                        );
                                    }}
                                  
                                >
                                    <MenuItem key="ALL" value="ALL">
                                    ALL
                                    </MenuItem>
                                    <MenuItem key="PAST" value="PAST">
                                        PAST
                                    </MenuItem>
                                    <MenuItem key="TODAY" value="TODAY">
                                        TODAY
                                    </MenuItem>
                                </MaterialSelect>
                            </Grid>

                            <Grid item xs={12} sm={12} md={1} lg={1}>
                                <Box style={{ textAlign: 'center' }}>
                                    <Icon
                                        className={
                                            classes.CustomImageButtonStyle
                                        }
                                        disabled={loading}
                                        classes={{ root: classes.iconRoot }}
                                        onClick={(e) => {
                                            setFieldValue(
                                                'IsExcelDownload',
                                                true
                                            );
                                            handleSubmit(e);
                                        }}
                                    >
                                        <img
                                            alt=""
                                            className={classes.imageIcon}
                                            src={excel}
                                        />
                                    </Icon>
                                </Box>
                            </Grid>
                            <Grid item xs={12} sm={12} md={1} lg={1}>
                                <PrimaryButton
                                    type="submit"
                                    title="Search"
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={loading}
                                    onClick={(e) => {
                                        setFieldValue('IsExcelDownload', false);
                                        handleSubmit(e);
                                    }}
                                >
                                    Search
                                </PrimaryButton>
                                {loadingProgress}
                            </Grid>
                            <Grid item xs={12} sm={12} md={1} lg={1}>
                                <PrimaryButton
                                    type="button"
                                    className={classes.CustomPrimaryButtonStyle}
                                    style={{
                                        background: 'red'
                                    }}
                                    disabled={loadingOpen}
                                    title="Accept All"
                                    onClick={handleRefundAllClick}
                                >
                                    Accept
                                </PrimaryButton>
                            </Grid>

                            <Grid item xs={12} sm={12} md={1} lg={1}>
                                <PrimaryButton
                                    type="button"
                                    className={classes.CustomPrimaryButtonStyle}
                                    style={{
                                        background: 'red'
                                    }}
                                    disabled={loadingOpen}
                                    title="Reject All"
                                    onClick={handleRejectClick}
                                >
                                    Reject
                                </PrimaryButton>
                            </Grid>
                        </Grid>
                    </CustomAccordion>
                </Grid>
            </Form>
        );
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
                initialValues={ModelObj}
                innerRef={formRef}
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={ModelObj}
            >
                {ComplaintForm}
            </Formik>

            <ReactTable
                data={transactions}
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                    callSelectData(
                        RowsPerPage,
                        RowsPerPage * currentPage,
                        '',
                        '',
                        ''
                    );
                }}
                getSearchCallback={({ SearchText }) => {
                    callSelectData(TakeCount, SkipCount, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    callSelectData(
                        TakeCount,
                        currentPage,
                        SearchText,
                        column,
                        direction
                    );
                }}
                getUpdatedData={(data) => {
                    ProcessUpdatedData(data);
                    setShouldGetUpdatedDataFromTable(false);
                }}
                isSelectableRows={'multiple'}
                shouldGetUpdatedData={shouldGetUpdatedDataFromTable}
            />

            {isModalOpen ? APILogModal : null}
            {isOpen ? (
                <CustomModalWithScroll
                    isOpen={isOpen}
                    title="Complaint"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsOpen(value);
                        refreshForm();
                    }}
                >
                    <ActionComplaint
                        IsEdit={true}
                        TransactionMasterID={TransactionMasterID}
                        ComplaintMasterID={ComplaintMasterID}
                        onDismissCallback={(value) => {
                            setIsOpen(value);
                            refreshForm();
                        }}
                    />
                </CustomModalWithScroll>
            ) : null}
            {isAlertOpenRefundAll ? RefundAllConfirmation : null}
            {isAlertOpenRejectAll ? RejectAllConfirmation : null}
        </CustomContainer>
    );
};
