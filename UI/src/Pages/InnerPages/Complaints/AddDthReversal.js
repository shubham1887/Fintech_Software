import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';

import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { Container, Grid, Button } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import CircularProgress from '@material-ui/core/CircularProgress';

import Box from '@material-ui/core/Box';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';

import {
    Insert as INSERT,
    SelectByID as SELECT_BY_ID
} from 'Components/Api/ComplaintsAPI';

import { SelectForComplaint as SELECT_FOR_COMPLAINT } from 'Components/Api/ComplaintsAPI';

import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const DthReversal = ({
    IsEdit,
    TransactionMasterID,
    ComplaintMasterID,
    onDismissCallback
}) => {
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [ComplaintObject, setComplaintObject] = useState({
        ComplaintMasterID: 0,
        TransactionMasterID: TransactionMasterID,
        Operator: '',
        DOC: '',
        ConsumerNumber: '',
        NewConsumerNumber: '',
        Amount: 0,
        Status: '0',
        CustomerRemark: '',
        AdminRemark: '',
        ComplaintType: 'DTHReversal'
    });

    const validationSchema = Yup.object({
        CustomerRemark: Yup.string().required(PLEASE_PROVIDE_VALID + 'remark'),
        NewConsumerNumber: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'new number '
        )
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoadingOpen(true);

        var response = await INSERT(JSON.stringify(values));
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setSeverity('error');
            setLoadingOpen(false);
            return;
        }

        setTimeout(() => {
            onDismissCallback(false);
        }, MESSAGE_CLOSE_AFTER);
    };

    useEffect(() => {
        GetTransactionByID(TransactionMasterID, ComplaintMasterID);
    }, []);

    const GetTransactionByID = async (
        TransactionMasterID,
        ComplaintMasterID
    ) => {
        setLoadingOpen(true);

        let jsonObj = {
            ID: TransactionMasterID,
            ComplaintMasterID: ComplaintMasterID
        };
        var request = JSON.stringify(jsonObj);
        let response = '';
        if (!IsEdit) response = await SELECT_FOR_COMPLAINT(request);
        else response = await SELECT_BY_ID(request);
        setLoadingOpen(false);

        let data = JSON.parse(response.data);

        setComplaintObject({
            ...ComplaintObject,
            ComplaintMasterID: data.ComplaintMasterID,
            TransactionMasterID: TransactionMasterID,
            ConsumerNumber: data.ConsumerNumber,

            Amount: data.Amount,
            Operator: data.Operator,
            CustomerRemark: data.CustomerRemark,
            AdminRemark: data.AdminRemark,
            DOC: data.TransactionDate
        });
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
                initialValues={ComplaintObject}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={ComplaintObject}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <Grid container spacing={3}>
                                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                                    <MaterialTextField
                                        name="ConsumerNumber"
                                        label="Number"
                                        placeholder="Number"
                                        readOnly={true}
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                                    <MaterialCurrency
                                        name="Amount"
                                        label="Amount"
                                        readOnly={true}
                                        placeholder="Amount"
                                        autoComplete="new-password"
                                        max={5000}
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                                    <MaterialTextField
                                        label="Operator"
                                        name="Operator"
                                        type="text"
                                        readOnly={true}
                                        title="Operator"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                                    <MaterialTextField
                                        label="Rch Date"
                                        name="DOC"
                                        type="text"
                                        readOnly={true}
                                        title="Rch DOC"
                                    />
                                </Grid>

                                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                                    <MaterialNumberField
                                        label="Correct Consumer Number"
                                        name="NewConsumerNumber"
                                        type="text"
                                        title="Correct Consumer Number"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={9} lg={9} xl={9}>
                                    <MaterialTextField
                                        label="Remark"
                                        name="CustomerRemark"
                                        type="text"
                                        title="Remark"
                                    />
                                </Grid>
                            </Grid>

                            <Grid className={classes.footerGrid}>
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
