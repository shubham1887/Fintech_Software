import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { Container, Grid, Box, Button, Paper } from '@material-ui/core';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import { withStyles } from '@material-ui/core/styles';
import Logo from 'Components/Logo/Logo';
import TableRow from '@material-ui/core/TableRow';

import { useHistory } from 'react-router';
import { LOGOUT } from 'ConstComponents';
import { SelectByOrderID as SELECT_BY_ORDERID } from 'Components/Api/PaymentGatewayAPI';

import { CLIENT_NAME } from 'Config';
import { PGTransactionPrint } from 'Components/Print/PGPrint';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);
const StyledTableCell = withStyles((theme) => ({
    body: {
        fontSize: 16,
        fontWeight: 'bold'
    }
}))(TableCell);
const ValueTableCell = withStyles((theme) => ({
    body: {
        fontSize: 16,
        fontWeight: 'bold',
        color: 'green'
    }
}))(TableCell);
export const PGSuccess = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [formObject, setFormObject] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [cName, setCName] = useState('');
    const [printData, setPrintData] = useState(null);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }
    useEffect(() => {
        fetchCName();
    }, []);
    const getQueryVariable = (variable) => {
        var query = window.location.search.substring(1);

        var vars = query.split('&');

        for (var i = 0; i < vars.length; i++) {
            var pair = vars[i].split('=');

            if (pair[0] == variable) {
                return pair[1];
            }
        }
        return false;
    };

    const initialValues = {
        Amount: '',
        Status: '',
        WalletName: '',
        TransactionId: '',
        PayMode: ''
    };

    const [FormObj, setFormObj] = useState(initialValues);

    const selectData = async () => {
        var OrderID = getQueryVariable('OrderID');

        var inputObj = {
            OrderID: OrderID
        };
        var requestObj = JSON.stringify(inputObj);

        const response = await SELECT_BY_ORDERID(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        var data = JSON.parse(response.data);
        setFormObj({
            ...FormObj,
            Amount: data.Amount,
            Status: data.Status,
            WalletName: data.WalletName,
            TransactionId: data.TransactionID,
            PayMode: data.PayMode
        });
        createKeyValuePair(data);
        setPrintData(data);
    };
    const [PGSuccessData, setPGSuccessData] = useState([]);
    const createKeyValuePair = (data) => {
        let rows = [
            RechargeData(<Logo />, data.CompanyName),
            RechargeData('Date', data.DisplayDOC),
            RechargeData('OrderID', data.OrderID),
            RechargeData('Amount', data.Amount === '' ? '0' : data.Amount),
            RechargeData('Status', data.Status),
            RechargeData('Txn ID', data.TransactionID),
            RechargeData('Retailer Name', data.UserName)
        ];
        setPGSuccessData(rows);
    };
    function RechargeData(Parameter, Values) {
        return { Parameter, Values };
    }

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        history.push('/PaymentGatewayTransaction');
    };
    useEffect(() => {
        selectData();
    }, []);

    const Print = async () => {
        var OrderID = getQueryVariable('OrderID');

        var inputObj = {
            OrderID: OrderID
        };
        var requestObj = JSON.stringify(inputObj);

        const response = await SELECT_BY_ORDERID(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);

        setPrintData(data);

        setIsAlertOpen(true);
    };

    return (
        <CustomContainer>
            <Formik
                enableReinitialize={true}
                initialValues={formObject}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {() => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Payment Receipt
                            </h2>

                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid item xs={12}>
                                        <TableContainer>
                                            <Table aria-label="customized table">
                                                <TableBody>
                                                    {PGSuccessData.map(
                                                        (row) => (
                                                            <TableRow
                                                                key={
                                                                    row.Parameter
                                                                }
                                                            >
                                                                <StyledTableCell align="left">
                                                                    {
                                                                        row.Parameter
                                                                    }
                                                                </StyledTableCell>
                                                                <ValueTableCell align="left">
                                                                    {row.Values}
                                                                </ValueTableCell>
                                                            </TableRow>
                                                        )
                                                    )}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                        <h3 className={classes.document_h3}>
                                            Thank you for using {cName}
                                        </h3>
                                    </Grid>
                                </Paper>
                            </Grid>
                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <Button
                                        variant="contained"
                                        color="secondary"
                                        title="Print"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={() => {
                                            Print(getQueryVariable('OrderID'));
                                        }}
                                    >
                                        Print
                                    </Button>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>

            {isAlertOpen ? (
                <PGTransactionPrint
                    isOpen={isAlertOpen}
                    title="Transaction Print"
                    request={printData}
                    positiveButtonClick={(printData) => {
                        setIsAlertOpen(false);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
