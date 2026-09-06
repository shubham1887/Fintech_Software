import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';

import { SelectAdminAccounting as SELECT } from 'Components/Api/TransactionDetailApi';
import { SelectMarketAccounting as SELECT_MARKET } from 'Components/Api/TransactionDetailApi';
import { Select as WALLET_SELECT } from 'Components/Api/UserBalanceCapping';

import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';

import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';

const CustomContainer = HOCLoader(Container);

export const AdminAccounting = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [mainColumns, setMainColumns] = useState([]);
    const [marketColumns, setMarketColumns] = useState([]);

    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [wallets, setWallets] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [marketList, setMarketList] = useState([]);

    const [formObj] = useState({
        FromDate: new Date(),
        WalletID: 0
    });

    const selectWallets = async () => {
        const response = await WALLET_SELECT();
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) return;
        var data = JSON.parse(response.data);

        setWallets(data);
    };
    const WalletList = wallets.map(({ WalletID, Name }) => (
        <MenuItem key={WalletID} value={WalletID}>
            {Name}
        </MenuItem>
    ));

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            FromDate: values.FromDate,
            WalletID: values.WalletID
        };

        setItemList([]);
        setMarketList([]);
        var requestObj = JSON.stringify(inputObj);

        setResponseMessage('');
        //setIsError(false);

        setLoading(true);
        const { isAuthorized, status, data } = await SELECT(requestObj);
        setLoading(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setSeverity('error');
            setIsError(true);
            setResponseMessage(data);
            return;
        }

        var d = JSON.parse(data);
        setItemList(d);

        const Marketdata = await SELECT_MARKET(requestObj);
        var Md = JSON.parse(Marketdata.data);
        setMarketList(Md);
        setSubmitting(false);
    };

    const validationSchema = Yup.object({
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + ' date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + ' date')
            .typeError(PLEASE_SELECT_VALID + ' date')
    });
    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };

    useEffect(() => {
        selectWallets();
        createColumns();
        createMarketColumns();
        refreshForm();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });
        columns.push({
            label: 'OpeningBal',
            name: 'OpeningBalance',
            numeric: true
        });
        columns.push({
            label: 'Purchase',
            name: 'Purchase',
            numeric: true
        });
        columns.push({
            label: 'Transfer',
            name: 'Transfer',
            numeric: true
        });
        columns.push({
            label: 'PG-Transfer',
            name: 'PGTransfer',
            numeric: true
        });
        columns.push({
            label: 'AEPS-Transfer',
            name: 'AEPSTransfer',
            numeric: true
        });
        columns.push({
            label: ' Commi.Debit(Dr)',
            name: 'CommissionDebit',
            numeric: true
        });
        columns.push({
            label: 'Commi.Refund(Cr)',
            name: 'CommissionCredit',
            numeric: true
        });
        columns.push({
            label: 'Reverse',
            name: 'Reverse',
            numeric: true
        });
        columns.push({
            label: 'Cl.Bal',
            name: 'ClosingBalance',
            numeric: true
        });
        columns.push({
            label: 'Expected.Cl',
            name: 'ExpectedClosingBalance',
            numeric: true
        });
        columns.push({
            label: 'Diff',
            name: 'Difference',
            numeric: true
        });
        setMainColumns(columns);
    };
    const createMarketColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });
        columns.push({
            label: 'OpeningBal',
            name: 'OpeningBalance',
            numeric: true
        });

        columns.push({
            label: 'Receive',
            name: 'Transfer',
            numeric: true
        });
        columns.push({
            label: 'PG-Receive',
            name: 'PGTransfer',
            numeric: true
        });
        columns.push({
            label: ' Commi',
            name: 'Commission',
            numeric: true
        });
        columns.push({
            label: 'Surcharge',
            name: 'Surcharge',
            numeric: true
        });
        columns.push({
            label: 'Reverse',
            name: 'Reverse',
            numeric: true
        });
        columns.push({
            label: 'Success',
            name: 'Success',
            numeric: true
        });
        columns.push({
            label: 'Wallet Transfer',
            name: 'WalletTransfer',
            numeric: true
        });
        columns.push({
            label: 'Failed',
            name: 'FailedAmount',
            numeric: true
        });
        columns.push({
            label: 'Past.Fail',
            name: 'PastFail',
            numeric: true
        });

        columns.push({
            label: 'Cl.Bal',
            name: 'ClosingBalance',
            numeric: true
        });
        columns.push({
            label: 'Expected.Cl',
            name: 'ExpectedClosingBalance',
            numeric: true
        });
        columns.push({
            label: 'Diff',
            name: 'Difference',
            numeric: true
        });
        setMarketColumns(columns);
    };
    const AdminAccountingReport = (
        <ReactTable data={itemList} columns={mainColumns} />
    );
    const MarketAccountingReport = (
        <ReactTable data={marketList} columns={marketColumns} />
    );

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
                innerRef={formRef}
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={formObj}
                render={({ values, setFieldValue, handleChange }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                name="WalletID"
                                                label="Wallet"
                                                title="Wallet"
                                            >
                                                <MenuItem key="0" value="0">
                                                    All Wallet
                                                </MenuItem>
                                                {WalletList}
                                            </MaterialSelect>
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
                                                label="Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={1}
                                            lg={1}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Search"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loading}
                                            >
                                                Search
                                            </PrimaryButton>
                                            {loading && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />
            <h3 className={classes.PaperHead_h2}>Admin Accounting</h3>
            {AdminAccountingReport}
            <h3 className={classes.PaperHead_h2}>Market Accounting</h3>
            {MarketAccountingReport}
        </CustomContainer>
    );
};
