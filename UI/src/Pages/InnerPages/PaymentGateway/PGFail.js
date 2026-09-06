import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { Container, Grid, Paper } from '@material-ui/core';
import { useHistory } from 'react-router';
import { LOGOUT } from 'ConstComponents';
import { SelectByOrderID as SELECT_BY_ORDERID } from 'Components/Api/PaymentGatewayAPI';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const PGFail = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [formObject, setFormObject] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);

    const getQueryVariable = (variable) => {
        var query = window.location.search.substring(1);

        var vars = query.split('&');

        for (var i = 0; i < vars.length; i++) {
            var pair = vars[i].split('=');

            if (pair[0] === variable) {
                return pair[1];
            }
        }
        return false;
    };

    const initialValues = {
        Amount: '',
        Status: '',
        WalletName: ''
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
            WalletName: data.WalletName
        });
    };
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
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <label>OrderID</label> :
                                            {getQueryVariable('OrderID')}
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        ></Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <label>Amount</label> :
                                            {FormObj.Amount}
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        ></Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <label>Status</label> :
                                            {FormObj.Status}
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        ></Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <label>Wallet Name</label> :
                                            {FormObj.WalletName}
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        ></Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <label>PayMode</label> :
                                            {FormObj.PayMode}
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        ></Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <label>TransactionID</label> :
                                            {FormObj.TransactionId}
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        ></Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
