import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Box,
    Button,
    Paper,
    CircularProgress
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';

import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    UpdateAmountByID as Amount_UPDATE,
    SelectByID as SELECT_BY_ID
} from 'Components/Api/SlabDetailApi';

import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const UpdateAmount = ({ SlabDetailID, onDismissCallback }) => {
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [IsUpdate, setIsUpdate] = useState(false);

    const selectByID = async () => {
        let JSONObj = {
            SlabDetailID: SlabDetailID
        };

        var request = JSON.stringify(JSONObj);
        setLoadingOpen(true);
        const response = await SELECT_BY_ID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);

        setFormObject({
            SlabDetailID: data.SlabDetailID,
            FromAmount: data.FromAmount,
            ToAmount: data.ToAmount
        });
    };

    useEffect(() => {
        setLoadingOpen(true);
        selectByID();
    }, []);

    const [formObject, setFormObject] = useState({
        SlabDetailID: SlabDetailID,
        FromAmount: 0,
        ToAmount: 0
    });

    const FormValidationSchema = Yup.object({
        FromAmount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'from amount')
            .min(0, PLEASE_PROVIDE_VALID + 'from amount')
            .max(300000, PLEASE_PROVIDE_VALID + 'from amount'),

        ToAmount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'to amount')
            .min(0, PLEASE_PROVIDE_VALID + 'to amount')
            .max(300000, PLEASE_PROVIDE_VALID + 'to amount')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);
        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await Amount_UPDATE(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        setSubmitting(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }
        setIsUpdate(false);
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
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {() => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>Amount </h2>
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
                                            <MaterialNumberField
                                                label="From Amount"
                                                name="FromAmount"
                                                type="text"
                                                placeholder="From Amount"
                                                length={3}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialNumberField
                                                label="To Amount"
                                                name="ToAmount"
                                                type="text"
                                                length={5}
                                                placeholder="To Amount"
                                            />
                                        </Grid>
                                    </Grid>
                                </Paper>
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
                                            disabled={loadingOpen}
                                        >
                                            Update
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
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
