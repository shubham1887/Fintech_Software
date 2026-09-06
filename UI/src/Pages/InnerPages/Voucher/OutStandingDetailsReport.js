import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import { Container, Grid, Icon, CircularProgress } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import excel from 'Assets/Images/excel.svg';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { VoucherDetailsReport as SELECT_REPORT } from 'Components/Api/VoucherAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);
export const OutStandingDetailsReport = ({ ReportData }) => {
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
        UserID: ReportData,
        FromDate: new Date().addDays(-60),
        ToDate: new Date(),
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
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            UserID: formRef.current.values.UserID,
            IsExcelDownload: formRef.current.values.IsExcelDownload
        };

        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);

        setLoadingOpen(true);

        var response = await SELECT_REPORT(requestJSON);

        setLoadingOpen(false);

        // if (!response.isAuthorized) LOGOUT();

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

        setTransactions(data.list);
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
            label: 'FirmName',
            name: 'FirmName'
        });
        columns.push({
            label: 'Mode',
            name: 'Mode'
        });
        columns.push({
            label: 'OpeningBal',
            name: 'OutstandingOpeningBal'
        });
        columns.push({
            label: 'Amount',
            name: 'Amount'
        });
        columns.push({
            label: 'ClosingBal',
            name: 'OutstandingClossingBal',
            numeric: true
        });
        columns.push({
            label: 'Credit',
            name: 'Credit'
        });
        columns.push({
            label: 'Debit',
            name: 'Debit'
        });
        columns.push({
            label: 'Remark',
            name: 'Remark'
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
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <MaterialDatePickerField
                                name="FromDate"
                                label="FromDate"
                                maxDate={new Date()}
                            />
                        </Grid>
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <MaterialDatePickerField
                                name="ToDate"
                                label="ToDate"
                                maxDate={new Date()}
                            />
                        </Grid>
                        <Grid item xs={12} sm={12} md={1} lg={1}>
                            <PrimaryButton
                                type="submit"
                                title="SEARCH"
                                className={classes.CustomPrimaryButtonStyle}
                                disabled={loadingOpen}
                            >
                                Search
                            </PrimaryButton>
                        </Grid>
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
