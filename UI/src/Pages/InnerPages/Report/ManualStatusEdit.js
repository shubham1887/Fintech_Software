import React, { useEffect, useState } from 'react';

import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import {
    Container,
    Grid,
    Button,
    MenuItem,
    Box,
    CircularProgress
} from '@material-ui/core';

import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';

import { LOGOUT } from 'ConstComponents';
import { UpdateStatusmanually as STATUS_UPDATE } from 'Components/Api/TransactionAPI';
import { SelectByID as TRANSACTION_SELECT_BY_ID } from 'Components/Api/TransactionAPI';
import { HeaderStatus } from 'Components/SummaryCards/HeaderStatus';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const StatusEdit = ({ TransactiomMasterID, onDismissCallback }) => {
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [TransactionID, setTransactionID] = useState('none');
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    var isPin = localStorage.getItem('IsPin');

    const [formObject, setFormObject] = useState({
        TransactionMasterID: TransactiomMasterID,
        Status: '0',
        ReferenceNo: '',
        Pin: ''
    });
    const [headerStatus, setheaderStatus] = useState({
        FirmName: '',
        DateStr: '',
        ConsumerNumber: '',
        Amount: 0
    });

    const OnChange = (name, value, setFieldValue) => {
        if (value === 'SUCCESS') setTransactionID('inline');

        if (value === 'REFUND') setTransactionID('none');
    };

    useEffect(() => {
        selectByID(TransactiomMasterID);
    }, [TransactiomMasterID]);

    const selectByID = async (TransactiomMasterID) => {
        let primaryID = TransactiomMasterID;

        let JSONObj = {
            ID: primaryID
        };
        setLoading(true);
        var request = JSON.stringify(JSONObj);

        const response = await TRANSACTION_SELECT_BY_ID(request);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);

        setFormObject({
            TransactionMasterID: data.TransactionMasterID,
            Status: '0',
            ReferenceNo: '',
            Pin: ''
        });

        setheaderStatus({
            FirmName: data.FirmName,
            DateStr: data.DateStr,
            ConsumerNumber: data.ConsumerNumber,
            Amount: data.Amount
        });
    };

    const UpdateStatus = async (requestObj) => {
        setLoading(true);
        setIsError(false);

        var response = await STATUS_UPDATE(requestObj);
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }
        onDismissCallback(false);
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        setSubmitting(false);

        if (values.Status === 'REFUND') {
            setConfirmAlertRequest(values);
            setIsAlertOpen(true);
            return;
        }
        var requestObj = JSON.stringify(values);
        UpdateStatus(requestObj);
        setSubmitting(true);
        resetForm(formObject);
    };

    const FormValidationSchema = Yup.object({
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Status')
            .required(PLEASE_SELECT_VALID + 'Status'),
        ReferenceNo: Yup.string().when(['Status'], {
            is: (status) => status === 'SUCCESS',
            then: Yup.string().required(PLEASE_PROVIDE_VALID + 'reference no')
        })
    });

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
            <HeaderStatus headerStatus={headerStatus} />

            <Formik
                enableReinitialize={true}
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={handleSubmit}
                resetForm={formObject}
            >
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialSelect
                                            label="Select Status"
                                            name="Status"
                                            onCustomCallback={(name, value) => {
                                                OnChange(
                                                    name,
                                                    value,
                                                    setFieldValue
                                                );
                                            }}
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Type
                                            </MenuItem>

                                            <MenuItem
                                                key="SUCCESS"
                                                value="SUCCESS"
                                            >
                                                SUCCESS
                                            </MenuItem>
                                            <MenuItem
                                                key="REFUND"
                                                value="REFUND"
                                            >
                                                REFUND
                                            </MenuItem>
                                        </MaterialSelect>
                                    </Grid>

                                    {isPin === 'true' && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialNumberField
                                                label="Pin"
                                                name="Pin"
                                                length={6}
                                                type="password"
                                                placeholder="Pin"
                                                autoComplete="off"
                                            />
                                        </Grid>
                                    )}

                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <Box display={TransactionID}>
                                            <MaterialTextField
                                                label="Operator Reference"
                                                name="ReferenceNo"
                                                type="text"
                                                length={500}
                                                placeholder="Operator Reference"
                                            />
                                        </Box>
                                    </Grid>
                                </Grid>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Update"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Update
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

            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Update Confirmation"
                    message="Are you sure to Refund this transaction?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        var requestObj = JSON.stringify(request);
                        UpdateStatus(requestObj);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
