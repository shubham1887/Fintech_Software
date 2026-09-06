import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import Typography from '@material-ui/core/Typography';
import {
    Container,
    Grid,
    Button,
    Icon,
    CircularProgress
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectOperatorSale as SELECT_OPERATOR_SALE } from 'Components/Api/TransactionReportAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import { SelectReportByOperatorID } from 'Pages/InnerPages/Report/SelectReportByOperatorID';
export const SelectReportByServiceByID = ({ reportData }) => {
    var ServiceID = reportData.ServiceID;
    var UserID = reportData.UserID;
    var OperatorName = reportData.OperatorName;
    var OperatorID = reportData.OperatorID;
    var fromDateString =
        reportData.FromDate instanceof Date
            ? reportData.FromDate.toLocaleDateString()
            : new Date(reportData.FromDate).toLocaleDateString();
    var toDateString =
        reportData.ToDate instanceof Date
            ? reportData.ToDate.toLocaleDateString()
            : new Date(reportData.ToDate).toLocaleDateString();

    var FromDate = reportData.FromDate;
    var ToDate = reportData.ToDate;
    const formRef = useRef();
    const classes = commonStyle();

    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [transactions, setTransactions] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [isOpenReport, setIsOpenReport] = useState(false);
    const [SearchText, setSearchText] = useState('');
    const [operatorReportData, setoperatorReportData] = useState({});

    const [SortDirection, setSortDirection] = useState('');
    const CustomContainer = HOCLoader(Container);
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });

    const [ModelObj] = useState({
        ToDate: ToDate,
        FromDate: FromDate,
        UserID: UserID,
        ServiceID: 0,
        User: userDefault,
        OperatorName: OperatorName,
        OperatorID: OperatorID,
        Status: 'ALL',
        IsExcelDownload: false,
        ExportType: ''
    });

    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        setSortDirection
    ) => {
             let requestObj = {
                 TakeCount: TakeCount,
                 SkipCount: SkipCount,
                 Search: SearchText,
                 SortColumnName: SortColumnName,
                 SortDirection: SortDirection,
                 FromDate: FromDate,
                 ToDate: ToDate,
                 UserID: UserID,
                 ServiceID: ServiceID,
                 IsExcelDownload: formRef.current.values.IsExcelDownload,
                 ExportType: formRef.current.values.ExportType,
                 IsPDFDownload: formRef.current.values.IsPDFDownload
             };

             var requestJSON = JSON.stringify(requestObj);

             setIsError(false);

             setLoadingOpen(true);

             var response = await SELECT_OPERATOR_SALE(requestJSON);

             setLoadingOpen(false);

             if (!response.isAuthorized) LOGOUT();

             if (!response.status) {
                 setIsError(true);
                 setSeverity('error');
                 setResponseMessage(response.data);

                 return;
             }
             setSeverity('success');
             var data = JSON.parse(response.data);

             if (data.IsExcelDownload) {
                 formRef.current.values.IsExcelDownload = false;
                 downloadFile(data.ReportPath, 'OperatorWiseReport');
             }

             setTransactions(data.list);
             setTotalCount(data.Count);
         };

    useEffect(() => {
        createColumns();
        callSelectData(TakeCount, SkipCount, '', '', '');
    }, []);
    const SelectReport = async (row) => {
        if (row[2] === 'Total') return;
        setoperatorReportData({
            ServiceID: row[1],
            ServiceName: row[2],
            //OperatorName: row[3],
            OperatorID: row[3],
            UserID: formRef.current.values.User.UserID,

            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate
        });
        setIsOpenReport(true);
    };
    const OpenReportModalCallback = (value) => {
        setIsOpenReport(value);
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });
        columns.push({
            label: 'User',
            name: 'UserName'
        });
        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });

        columns.push({
            label: 'OperatorID',
            name: 'OperatorID',
            hide: true
        });

        columns.push({
            label: 'Operator',
            name: 'OperatorName',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row);
            }
        });

        columns.push({
            label: 'SuccessCount',
            name: 'SuccessCount',
            numeric: true
        });

        columns.push({
            label: 'SuccessAmount',
            name: 'SuccessAmount',
            numeric: true
        });

        columns.push({
            label: 'RetailerComm',
            name: 'ReatilerComm',
            numeric: true
        });

        columns.push({
            label: 'RetailerComm in %',
            name: 'RetailerCommPercentage'
        });

        columns.push({
            label: 'Dist.Comm',
            name: 'DistributerComm',
            numeric: true
        });

        columns.push({
            label: 'Dist.Comm in %',
            name: 'DistributerCommPercentage'
        });

        columns.push({
            label: 'SuperDist.Comm',
            name: 'SuperDistributerComm',
            numeric: true
        });

        columns.push({
            label: 'SuperDist.Comm in %',
            name: 'SuperDistributerCommPercentage'
        });

        setMainColumns(columns);
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);
        createColumns();
        callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
    };
    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }

    const ReportTable = (
        <ReactTable
            data={transactions}
            columns={mainColumns}
            count={totalCount}
            rowsPerPage={TakeCount}
            serverSide={true}
            headerToolbar={false}
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
                callSelectData(currentPage, TakeCount, SearchText, '', '');
            }}
            getSortCallback={({ column, direction }) => {
                callSelectData(
                    currentPage,
                    TakeCount,
                    SearchText,
                    column,
                    direction
                );
            }}
        />
    );

    const loadingProgress = loadingOpen && (
        <CircularProgress size={24} className={classes.buttonProgress} />
    );

    const ReportForm = ({ setFieldValue, handleSubmit }) => {
        return (
            <Form>
                <Grid item xs={12}>
                    <Grid container spacing={1}>
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <Typography>From Date: {fromDateString}</Typography>
                        </Grid>
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <Typography>To Date: {toDateString}</Typography>
                        </Grid>
                    </Grid>
                </Grid>
            </Form>
        );
    };
    const customContainerCallback = (alertState) => {
        setIsError(alertState);
    };
    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={customContainerCallback}
        >
            <Formik
                initialValues={ModelObj}
                innerRef={formRef}
                onSubmit={handleSubmit}
                resetForm={ModelObj}
            >
                {ReportForm}
            </Formik>

            {ReportTable}
            {isOpenReport && (
                <CustomModalWithScroll
                    isOpen={isOpenReport}
                    title="Operator Wise User Sale Report"
                    width="100%"
                    onDismissCallback={OpenReportModalCallback}
                >
                    <SelectReportByOperatorID
                        reportData={operatorReportData}
                        onDismissCallback={OpenReportModalCallback}
                    />
                </CustomModalWithScroll>
            )}
        </CustomContainer>
    );
};
