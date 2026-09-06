import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';

import {
    Container,
    Grid,
    Button,
    Icon,
    CircularProgress
} from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';

import excel from 'Assets/Images/excel.svg';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectByProviderID as SELECT_BY_PROVIDER_ID } from 'Components/Api/DaybooReportAPI';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const SelectByProviderID = ({ ReportData }) => {
    var ProviderID = ReportData.ProviderID;
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
        ToDate: ReportData.ToDate,
        FromDate: ReportData.FromDate,
        ProviderID: ProviderID,
        UserID: ReportData.UserID,
        IsExcelDownload: false
    });

    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
    };

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
            ProviderID: formRef.current.values.ProviderID,
            UserID: formRef.current.values.UserID,

            IsExcelDownload: formRef.current.values.IsExcelDownload
        };

        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);

        setLoadingOpen(true);

        var response = await SELECT_BY_PROVIDER_ID(requestJSON);
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
            downloadFile(data.ReportPath, 'Provider Transaction Report');
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
            name: 'Operator',
            srno: true
        });
        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'Operator',
            name: 'Operator'
        });

        columns.push({
            label: 'Total',
            name: 'TotalCount'
        });
        columns.push({
            label: 'Total Amt',
            name: 'TotalAmount'
        });
        columns.push({
            label: 'Success',
            name: 'SuccessCount'
        });
        columns.push({
            label: 'Success Amt',
            name: 'SuccessAmount'
        });
        columns.push({
            label: 'Failed',
            name: 'FailedCount'
        });
        columns.push({
            label: 'Fail Amt',
            name: 'FailedAmount'
        });
        columns.push({
            label: 'Pending',
            name: 'PendingCount'
        });

        columns.push({
            label: 'Pending Amt',
            name: 'PendingAmount'
        });
        columns.push({
            label: 'R.Comm',
            name: 'RetailerCommission'
        });
        columns.push({
            label: 'D.Comm',
            name: 'DistributerCommission'
        });
        columns.push({
            label: 'S.Comm',
            name: 'SuperDistributerCommission'
        });
        columns.push({
            label: 'Admin.Comm',
            name: 'AdminCommission'
        });

        setMainColumns(columns);
    };

    const validationSchema = Yup.object({
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + ' date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'from date')
            .typeError(PLEASE_SELECT_VALID + 'from date'),

        ToDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + ' date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'to date')
            .typeError(PLEASE_SELECT_VALID + 'to date')
    });

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
        // document.removeChild(a);
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
                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <MaterialDatePickerField
                                name="FromDate"
                                label="From Date"
                                maxDate={new Date()}
                            />
                        </Grid>

                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <MaterialDatePickerField
                                name="ToDate"
                                label="To Date"
                                maxDate={new Date()}
                            />
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
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <div className={classes.SubmitWrapper}>
                                <Button
                                    type="submit"
                                    title="submit"
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={loadingOpen}
                                    onClick={(e) => {
                                        setFieldValue('IsExcelDownload', false);
                                        handleSubmit(e);
                                    }}
                                >
                                    Search
                                </Button>
                                {loadingProgress}
                            </div>
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
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={ModelObj}
            >
                {ReportForm}
            </Formik>

            {ReportTable}
        </CustomContainer>
    );
};
