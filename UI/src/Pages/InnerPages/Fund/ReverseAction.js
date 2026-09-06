import React, { useEffect, useState, useContext } from 'react';
import { Formik, Form } from 'formik';

import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { SelectRequestByID, FundAction } from 'Components/Api/FundApi';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { Container, MenuItem, Grid, Button } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import CircularProgress from '@material-ui/core/CircularProgress';
import Box from '@material-ui/core/Box';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import { BalanceContext } from 'Components/BalanceContext';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const ReverseAction = ({ FundTransactionID, onDismissCallback }) => {
    const { DashboardBalanceUpdate } = useContext(BalanceContext);
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [formObject, setFormObject] = useState({
        FundTransactionID: 0,
        Wallet: '',
        Amount: 0,
        Remark: '',
        PayMode: '',
        DOC: '',
        Receiver: '',
        Sender: '',
        Status: '',
        SenderRemark: '',
        CashCollectionAmount: 0,
        CashCollectionRemark: '',
        InWords: '',
        CollectionInWords: ''
    });

    const validationSchema = Yup.object({
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'status')
            .required(PLEASE_SELECT_VALID + 'status'),

        SenderRemark: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'sender remark'
        ),
        FundTransactionID: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'ID')
            .min(1, PLEASE_PROVIDE_VALID + 'ID')
    });

    const select = async (FundTransactionID) => {
        let primaryID = FundTransactionID;
        let JSONObj = {
            FundTransactionID: primaryID
        };
        setLoadingOpen(true);
        var request = JSON.stringify(JSONObj);

        const response = await SelectRequestByID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        var data = JSON.parse(response.data);

        setFormObject({
            FundTransactionID: data.FundTransactionID,
            Amount: data.Amount,
            DOC: data.DOC,
            Wallet: data.Wallet,
            Remark: data.Remark,
            PayMode: data.PayMode,
            Receiver: data.Receiver,
            Sender: data.Sender,
            Status: 0,
            SenderRemark: '',
            CashCollectionAmount: 0,
            CashCollectionRemark: '',

            CollectionInWords: '',
            InWords: NumberToWord(data.Amount)
        });
    };

    useEffect(() => {
        select(FundTransactionID);
    }, []);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoadingOpen(true);

        setResponseMessage('');
        setIsError(false);

        var response = await FundAction(JSON.stringify(values));

        if (!response.isAuthorized) LOGOUT();

        DashboardBalanceUpdate();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setSeverity('error');
            setLoadingOpen(false);
            return;
        }
        setSubmitting(false);

        resetForm(formObject);
        setTimeout(() => {
            onDismissCallback(false);
        }, MESSAGE_CLOSE_AFTER);
    };

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Formik
                enableReinitialize={true}
                initialValues={formObject}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <Grid container spacing={3}>
                                <Grid item xs={4}>
                                    <MaterialTextField
                                        label="Date"
                                        name="DOC"
                                        type="text"
                                        readOnly={true}
                                        title="DOC"
                                    />
                                </Grid>
                                <Grid item xs={4}>
                                    <MaterialTextField
                                        label="Wallet"
                                        name="Wallet"
                                        type="text"
                                        readOnly={true}
                                        title="Wallet"
                                    />
                                </Grid>
                                <Grid item xs={4}>
                                    <MaterialTextField
                                        label="Sender"
                                        name="Sender"
                                        type="text"
                                        readOnly={true}
                                        title="Sender"
                                    />
                                </Grid>
                                <Grid item xs={4}>
                                    <MaterialCurrency
                                        name="Amount"
                                        label="Amount"
                                        readOnly={true}
                                        placeholder="Amount"
                                        autoComplete="new-password"
                                        max={1000000}
                                    />
                                </Grid>
                                <Grid item xs={4}>
                                    <MaterialTextField
                                        label="In words"
                                        name="InWords"
                                        type="text"
                                        readOnly={true}
                                        placeholder="InWords"
                                    />
                                </Grid>
                                <Grid item xs={4}>
                                    <MaterialTextField
                                        label="Receiver"
                                        name="Receiver"
                                        type="text"
                                        readOnly={true}
                                        title="Receiver"
                                    />
                                </Grid>
                            </Grid>
                            <Grid container spacing={3}>
                                <Grid item xs={8}>
                                    <MaterialTextField
                                        label="Remark"
                                        name="Remark"
                                        type="text"
                                        readOnly={true}
                                        title="Remark"
                                    />
                                </Grid>
                                <Grid item xs={4}>
                                    <MaterialSelect
                                        label="Status"
                                        name="Status"
                                        title="Select Status"
                                    >
                                        <MenuItem key="0" value="0">
                                            Select Status
                                        </MenuItem>
                                        <MenuItem key="Accept" value="Accept">
                                            Accept
                                        </MenuItem>
                                        <MenuItem key="Reject" value="Reject">
                                            Reject
                                        </MenuItem>
                                    </MaterialSelect>
                                </Grid>
                                <Grid item xs={8}>
                                    <MaterialTextField
                                        label="Sender Remark"
                                        name="SenderRemark"
                                        type="text"
                                        title="Sender Remark"
                                    />
                                </Grid>
                                <Grid item xs={3}>
                                    <Box display="none">
                                        <MaterialTextField
                                            label="FundTransactionID"
                                            name="FundTransactionID"
                                            type="text"
                                            title="FundTransactionID"
                                        />
                                    </Box>
                                </Grid>
                            </Grid>

                            <Grid
                                className={classes.footerGrid}
                                style={{ marginTop: '20px' }}
                            >
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Submit"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loadingOpen}
                                        >
                                            Submit
                                        </Button>
                                        {loadingOpen && (
                                            <CircularProgress
                                                size={24}
                                                className={
                                                    classes.buttonProgress
                                                }
                                            />
                                        )}
                                    </div>
                                    <Button
                                        type="reset"
                                        title="Reset"
                                        className={
                                            classes.CustomDangerButtonStyle
                                        }
                                    >
                                        Reset
                                    </Button>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
