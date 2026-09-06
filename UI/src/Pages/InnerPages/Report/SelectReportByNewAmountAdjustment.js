import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import { Container, Grid, Icon, CircularProgress } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import excel from 'Assets/Images/excel.svg';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectForFilterByNewAmtAdj as SELECT_BY_NEW_AMT_ADJ } from 'Components/Api/TransactionReportAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);
export const SelectReportByNewAmountAdjustment = ({ ReportData }) => {
    var UserID = ReportData.UserID;
    var FillerDate = ReportData.FillerDate;
    var WalletID = ReportData.WalletID;
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

    const [SearchText, setSearchText] = useState('');

    const [SortDirection, setSortDirection] = useState('');

    const [ModelObj] = useState({
        Date: FillerDate,
        UserID: UserID,
        WalletID:WalletID,
        Status: 'ALL',
        IsExcelDownload: false
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
            SearchText: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            Date: FillerDate,
            UserID: formRef.current.values.UserID,
            IsExcelDownload: formRef.current.values.IsExcelDownload,
            WalletID :WalletID
        };

        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);

        setLoadingOpen(true);

        var response = await SELECT_BY_NEW_AMT_ADJ(requestJSON);

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
            downloadFile(data.ReportPath, 'New Amount Adjustment Report');
        }

        setTransactions(data.NewAmountAdjustmentlist);
        setTotalCount(data.Count);
    };

    useEffect(() => {
        createColumns();
        callSelectData(TakeCount, SkipCount, '', '', '');
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: '1',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Updated Date',
            name: 'UpdatedDOC'
        }); 
        columns.push({
            label: 'Firm',
            name: 'FirmName'
        });
        columns.push({
            label: 'Number',
            name: 'ConsumerNumber'
        });
        columns.push({
            label: 'Amount',
            name: 'Amount',
            numeric: true
        });
        columns.push({
            label: 'Commission',
            name: 'Commission'
        });
        columns.push({
            label: 'Debit Amount',
            name: 'DebitAmount'
        });
        columns.push({
            label: 'Operator',
            name: 'Operator'
        });
        columns.push({
            label: 'Remark',
            name: 'Description',
            ReadOnlyTextArea: true,
            textwrap: 60
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
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
        document.removeChild(a);
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
                    <Grid container spacing={2}>
                        <Grid item xs={12} sm={12} md={1} lg={1}>
                            <Icon
                                className={classes.CustomImageButtonStyle}
                                disabled={loadingOpen}
                                classes={{ root: classes.iconRoot }}
                                onClick={(e) => {
                                    setFieldValue('IsExcelDownload', true);
                                    handleSubmit(e);
                                }}
                            >
                                <img
                                    alt=""
                                    className={classes.imageIcon}
                                    src={excel}
                                />
                            </Icon>
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
        </CustomContainer>
    );
};
