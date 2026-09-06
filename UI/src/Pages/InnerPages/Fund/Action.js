import React, { useEffect, useState, useContext } from 'react';
import { Formik, Form } from 'formik';

import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { SelectRequestByID, FundAction } from 'Components/Api/FundApi';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { Container, MenuItem, Grid, Button } from '@material-ui/core';
import InputLabel from 'Components/Inputes/InputLabel';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import CircularProgress from '@material-ui/core/CircularProgress';
import Box from '@material-ui/core/Box';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER,
    IS_CASH_COLLECTION
} from 'Components/Helper/Messages';
import { BalanceContext } from 'Components/BalanceContext';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const Action = ({ FundTransactionID, onDismissCallback }) => {
    const { DashboardBalanceUpdate } = useContext(BalanceContext);
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    var CashCollection = localStorage.getItem('IsCashCollection');
    var IsFundRequestAmountEdit = localStorage.getItem('IsFundRequestAmountEdit');
    const [IsCashCollection, setIsCashCollection] = useState(false);
    const [FundTransferAmountEdit, setisFundTransferAmountEdit] = useState(false);
    const [CollectionInWords, setCollectionInWords] = useState('');
    const CheckIsCashCollection = () => {
        if (IS_CASH_COLLECTION === CashCollection) setIsCashCollection(true);
    };
    const CheckIsFundRequestAmountEdit = () => {
        if (IsFundRequestAmountEdit === "true") setisFundTransferAmountEdit(true);
    };

    const [formObject, setFormObject] = useState({
        FundTransactionID: 0,
        Wallet: '',
        Amount: 0,
        Remark: '',
        PayMode: '',
        DOC: '',
        Receiver: '',
        Sender: '',
        SenderBalance: 0,
        Status: '',
        SenderRemark: '',
        CashCollectionAmount: 0,
        CashCollectionRemark: '',
        InWords: '',
        CollectionInWords: '',
        ParentName: '',
        Amount: 0
    });

    const validationSchema = Yup.object({
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'status')
            .required(PLEASE_SELECT_VALID + 'status'),

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
            Remark: data.UTRNumber + '-' + data.Remark,
            PayMode: data.PayMode,
            Receiver: data.Receiver,
            Sender: data.Sender,
            SenderBalance: data.SenderBalance,
            ParentName: data.ParentName,
            Status: 0,
            SenderRemark: '',
            CashCollectionAmount: 0,
            CashCollectionRemark: '',
            CollectionInWords: '',

            InWords: NumberToWord(data.Amount)
        });
    };

    useEffect(() => {
        CheckIsCashCollection();
        CheckIsFundRequestAmountEdit();
        select(FundTransactionID);
    }, []);

    const OnChange = (value, setFieldValue) => {
        if (value === undefined || value <= 0) {
            return;
        }
        let wwords = NumberToWord(value);
        setCollectionInWords(wwords);
        setFieldValue('CollectionInWords', wwords);
    };

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
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        Wallet :
                                        <span className={classes.Success_h3}>
                                            {formObject.Wallet}
                                        </span>
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        Sender :
                                        <span className={classes.Success_h3}>
                                            {formObject.Sender}
                                        </span>
                                    </h2>
                                </Grid>
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        Sender Balance :
                                        <span className={classes.Success_h3}>
                                            {'₹ ' + formObject.SenderBalance}
                                        </span>
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        Date :
                                        <span className={classes.Success_h3}>
                                            {formObject.DOC}
                                        </span>
                                    </h2>
                                </Grid>

                                {FundTransferAmountEdit ?
                                    <Grid item xs={12} sm={12} md={3} lg={3}>

                                        <MaterialTextField
                                            label="Amount"
                                            name="Amount"
                                            type="text"
                                            title="Amount"
                                        />
                                    </Grid>


                                    :
                                    <Grid item xs={12} sm={12} md={3} lg={3}>

                                        <h2 className={classes.recharge_h2}>
                                            Amount :
                                            <span className={classes.Success_h3}>
                                                {'₹ ' + formObject.Amount}
                                            </span>
                                            <span className={classes.Success_h3}>
                                                ({formObject.InWords})
                                            </span>
                                        </h2>
                                    </Grid>
                                }


                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        Receiver :
                                        <span className={classes.Success_h3}>
                                            {formObject.Receiver}
                                        </span>
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        Parent :
                                        <span className={classes.Success_h3}>
                                            {formObject.ParentName}
                                        </span>
                                    </h2>
                                </Grid>
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        Mode :
                                        <span className={classes.Success_h3}>
                                            {formObject.PayMode}
                                        </span>
                                    </h2>
                                </Grid>

                                {/* <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <h2 className={classes.recharge_h2}>
                                        Remark :
                                        <span className={classes.Success_h3}>
                                            {formObject.Remark}
                                        </span>
                                    </h2>
                                </Grid> */}

                                <Grid item xs={12} sm={12} md={6} lg={6}>
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
                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <MaterialTextField
                                        label="Sender Remark"
                                        name="SenderRemark"
                                        type="text"
                                        title="Sender Remark"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
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

                            <Grid container spacing={3}>
                                {values.Status === 'Accept' ? (
                                    <>
                                        {IsCashCollection ? (
                                            <>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={6}
                                                    lg={6}
                                                >
                                                    <MaterialCurrency
                                                        name="CashCollectionAmount"
                                                        label="Collection Amount"
                                                        placeholder="Collection Amount"
                                                        autoComplete="new-password"
                                                        max={1000000}
                                                        onCustomCallback={(
                                                            value
                                                        ) => {
                                                            OnChange(
                                                                value,
                                                                setFieldValue
                                                            );
                                                        }}
                                                    />
                                                    <InputLabel
                                                        color="green"
                                                        value={
                                                            CollectionInWords
                                                        }
                                                    />
                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={6}
                                                    lg={6}
                                                >
                                                    <MaterialTextField
                                                        label="Collection Remark"
                                                        name="CashCollectionRemark"
                                                        type="text"
                                                        title="Collection Remark"
                                                    />
                                                </Grid>
                                            </>
                                        ) : null}
                                    </>
                                ) : null}
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
