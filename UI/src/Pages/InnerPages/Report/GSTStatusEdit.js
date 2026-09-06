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

import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';

import { LOGOUT } from 'ConstComponents';
import { UpdateStatus as STATUS_UPDATE } from 'Components/Api/GSTHoldAPI';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const GSTStatusEdit = ({ GSTHoldID, onDismissCallback }) => {
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);

    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [formObject, setFormObject] = useState({
        GSTHoldID: GSTHoldID,
        Status: '0',
        AdminRemark: ''
    });

    const OnChange = (name, value, setFieldValue) => {};

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
        var requestObj = JSON.stringify(values);
        UpdateStatus(requestObj);
        setSubmitting(true);
        resetForm(formObject);
    };

    const FormValidationSchema = Yup.object({
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Status')
            .required(PLEASE_SELECT_VALID + 'Status'),

        AdminRemark: Yup.string().when(['Status'], {
            is: (status) => status === 'GSTPaid',
            then: Yup.string().required(PLEASE_PROVIDE_VALID + 'remark')
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
                                    <Grid item xs={12} sm={12} md={8} lg={8}>
                                        <Box display={GSTHoldID}>
                                            <MaterialTextField
                                                label="Remark"
                                                name="AdminRemark"
                                                type="text"
                                                length={25}
                                                placeholder="Admin Remark"
                                            />
                                        </Box>
                                    </Grid>

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
                                                Select Status
                                            </MenuItem>

                                            <MenuItem
                                                key="GSTPaid"
                                                value="GSTPaid"
                                            >
                                                GSTPaid
                                            </MenuItem>
                                        </MaterialSelect>
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
                    message="Are you sure to update status?"
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
