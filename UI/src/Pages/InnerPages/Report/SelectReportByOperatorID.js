import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import Typography from '@material-ui/core/Typography';

import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

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
import { SelectForFilterByOperatorID as SELECT_BY_OPERATOR_ID } from 'Components/Api/TransactionReportAPI';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';

import { StatusItems } from 'Components/ReportHelper';
 
const CustomContainer = HOCLoader(Container);

export const SelectReportByOperatorID = ({ reportData }) => {
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

    const [SearchText, setSearchText] = useState('');

    const [SortDirection, setSortDirection] = useState('');

    const [ModelObj] = useState({
        ToDate: ToDate,
        FromDate: FromDate,
        UserID: UserID,

        OperatorName: OperatorName,
        OperatorID: OperatorID,
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
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,

            UserID: formRef.current.values.UserID,

            OperatorName: formRef.current.values.OperatorName,
            OperatorID: formRef.current.values.OperatorID,

            IsExcelDownload: formRef.current.values.IsExcelDownload
        };
        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);

        setLoadingOpen(true);

        var response = await SELECT_BY_OPERATOR_ID(requestJSON);

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

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            srno: true
        });

        columns.push({
            label: 'Name',
            name: 'FirmName'
        });

        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });
        columns.push({
            label: 'Count',
            name: 'SuccessCount'
        });

        columns.push({
            label: 'Amount',
            name: 'Amount'
        });

        columns.push({
            label: 'RetailerComm',
            name: 'RetailerCommission'
        });

        columns.push({
            label: 'RetailerComm in %',
            name: 'RetailerCommPercentage'
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
