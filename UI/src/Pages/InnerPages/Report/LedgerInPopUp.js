import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';

import { Select as SELECT_LEDGER } from 'Components/Api/LedgerApi';
import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
import { Container, Grid, Button, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import excel from 'Assets/Images/excel.svg';
import Icon from '@material-ui/core/Icon';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const LedgerPopUp = ({ LedgerData }) => {
    var UserID = LedgerData.UserID;
    var FirmName = LedgerData.FirmName;
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [headertransactions, setHeaderTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [HeadermainColumns, setHeaderMainColumns] = useState([]);

    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [SortDirection, setSortDirection] = useState('');
    const [SearchText, setSearchText] = useState('');
    const [currentPage, setCurrentPage] = useState(0);
    const [totalCount, setTotalCount] = useState(0);

    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isRetailer, setIsRetailerRole] = useState(false);
    const [wallets, setWallets] = useState([]);

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        IsExcelDownload: false,
        TopRecords: TakeCount,
        WalletID: 0,
        UserID: UserID,
        FirmName: FirmName
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
            WalletID: formRef.current.values.WalletID,
            UserID: formRef.current.values.UserID,
            IsExcelDownload: formRef.current.values.IsExcelDownload
        };
        var requestJSON = JSON.stringify(requestObj);

        setIsError(false);
        setLoading(true);

        var response = await SELECT_LEDGER(requestJSON);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        if (SkipCount <= 0) {
            setHeaderTransactions([]);
        }
        var data = JSON.parse(response.data);

        if (data.IsExcelDownload) {
            formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'User Ledger');
            return;
        }

        if (SkipCount <= 0) {
            setHeaderTransactions(data.Headerlist);
            setTotalCount(data.Count);
        }
        setTransactions(data.list);
    };
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        createHeaderColumns();
        createColumns();
        setTransactions([]);

        await callSelectData(TakeCount, SkipCount, '', '', '');
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

    const selectWallets = async () => {
        var request = {
            ID: 0
        };
        var json = JSON.stringify(request);
        const response = await WALLET_SELECT(json);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setWallets(data);
    };

    useEffect(() => {
        callSelectData(TakeCount, SkipCount, '', '', '');
        selectWallets();
    }, []);

    const createHeaderColumns = () => {
        var columns = [];

        columns.push({
            label: 'Op.Bal',
            name: 'OpeningBal',
            CreditAmount: true
        });

        columns.push({
            label: 'Purchase',
            name: 'ReceiverAmount',
            CreditAmount: true
        });

        columns.push({
            label: 'F.Deducted',
            name: 'ReverseAmountDr',
            CreditAmount: true
        });
        columns.push({
            label: 'Comm',
            name: 'CommissionAmount',
            CreditAmount: true
        });
        columns.push({
            label: 'Debited',
            name: 'Debited',
            DebitAmount: true
        });
        columns.push({
            label: 'Requested',
            name: 'Requested',
            DebitAmount: true
        });

        if (!isRetailer) {
            columns.push({
                label: 'F.Transfer',
                name: 'TransferAmount',
                DebitAmount: true
            });
        }

        columns.push({
            label: 'Pu.Return',
            name: 'ReverseAmountCr',
            DebitAmount: true
        });

        columns.push({
            label: 'Surcharge',
            name: 'Surcharge',
            DebitAmount: true
        });

        columns.push({
            label: 'Compl.Amt',
            name: 'ComplaintAmount',
            DebitAmount: true
        });
        if (isRetailer === true) {
            columns.push({
                label: 'P.Success',
                name: 'PastSuccessAmount',
                DebitAmount: true
            });
        }
        if (isRetailer === true) {
            columns.push({
                label: 'P.Fail',
                name: 'PastFailAmount',
                DebitAmount: true
            });
        }

        columns.push({
            label: 'Cl.Bal',
            name: 'ClosingBalance',
            DebitAmount: true
        });
        columns.push({
            label: 'Exp.Cl.Bal',
            name: 'ExpectedCloseBal',
            DebitAmount: true
        });
        columns.push({
            label: 'Diff',
            name: 'Difference',
            DebitAmount: true
        });

        setHeaderMainColumns(columns);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'TransactionDetailID',
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
            label: 'Wallet',
            name: 'WalletName'
        });

        columns.push({
            label: 'Description',
            name: 'Description',
            ReadOnlyTextArea: true
        });

        columns.push({
            label: 'Op.Bal',
            name: 'OpeningBalance',
            numeric: true
        });

        columns.push({
            label: 'Amt',
            name: 'customAmount',
            customAmount: true
        });
        columns.push({
            label: 'Comm',
            name: 'CommissionAmount',
            numeric: true
        });
        columns.push({
            label: 'Cl.Bal',
            name: 'ClosingBalance',
            numeric: true
        });
        columns.push({
            label: 'TxnID ',
            name: 'ReferenceID'
        });

        setMainColumns(columns);
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
                enableReinitialize={true}
                innerRef={formRef}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={ModelObj}
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Grid container spacing={2}>
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

                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialSelect
                                            label="Wallet"
                                            name="WalletID"
                                            title="Select Wallet"
                                        >
                                            <MenuItem key="0" value="0">
                                                All Wallet
                                            </MenuItem>
                                            {wallets.map((item) => (
                                                <MenuItem
                                                    key={item.WalletID}
                                                    value={item.WalletID}
                                                >
                                                    {item.Name}
                                                </MenuItem>
                                            ))}
                                        </MaterialSelect>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialTextField
                                            label="FirmName"
                                            name="FirmName"
                                            type="text"
                                            readOnly
                                            title="FirmName"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <Box className={classes.flexReverse}>
                                            <div
                                                className={
                                                    classes.SubmitWrapper
                                                }
                                            >
                                                <Icon
                                                    className={
                                                        classes.CustomImageButtonStyle
                                                    }
                                                    disabled={loading}
                                                    classes={{
                                                        root: classes.iconRoot
                                                    }}
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'IsExcelDownload',
                                                            true
                                                        );
                                                        handleSubmit(e);
                                                        setFieldValue(
                                                            'IsExcelDownload',
                                                            false
                                                        );
                                                    }}
                                                >
                                                    <img
                                                        alt=""
                                                        className={
                                                            classes.imageIcon
                                                        }
                                                        src={excel}
                                                    />
                                                </Icon>
                                                <Button
                                                    type="submit"
                                                    title="submit"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    disabled={loading}
                                                >
                                                    Search
                                                </Button>
                                                {loading && (
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
                                </Grid>
                            </Grid>

                            <Grid className={classes.footerGrid}></Grid>
                        </Form>
                    );
                }}
            />
            <ReactTable
                data={headertransactions}
                columns={HeadermainColumns}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                }}
                count={totalCount}
                rowsPerPage={10}
                headerToolbar={false}
                footerToolbar={false}
            />
            <br />
            <ReactTable
                data={transactions}
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={10}
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
                getSearchCallback={(SearchText) => {
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
        </CustomContainer>
    );
};
