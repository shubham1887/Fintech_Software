import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import {
    PanVerification as PAN_VERIFIACTION,
    SelectByUserID as SELECT_BY_USERID
} from 'Components/Api/EKYCAPI';

import { Container, Grid, Paper } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PanDetails } from 'Pages/InnerPages/eKYC/PanDetails';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const PaysprintPan = () => {
    const classes = commonStyle();
    const [ResponsDetails, setResponsDetails] = useState();
    const [openDetail, setOpenDetail] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isError, setIsError] = useState(false);
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);
        setIsError(false);
        setLoading(true);
        const response = await PAN_VERIFIACTION(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        let data = JSON.parse(response.data);
        setSeverity('success');
        setResponseMessage(data.Message);
        setIsError(true);
        reset();
        SelectDetails();
        return;
    };
    const [FormObj, setFormObj] = useState({
        PanNumber: ''
    });
    const reset = () => {
        setFormObj({
            PanNumber: ''
        });
    };
    const FormValidationSchema = Yup.object({
        PanNumber: Yup.string()
            .length(10, 'Must be 10 alpha numberic')
            .required(PLEASE_PROVIDE_VALID + ' PanNumber')
    });

    useEffect(() => {
        SelectDetails();
    }, []);

    const SelectDetails = async () => {
        let jsonObj = {
            DocumentType: 'PAN'
        };
        var request = JSON.stringify(jsonObj);
        setLoading(true);
        const response = await SELECT_BY_USERID(request);

        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }
        let data = JSON.parse(response.data);

        let item = JSON.parse(data.Response);

        setResponsDetails(item);
        setOpenDetail(true);
    };

    const TransferForm = ({ setFieldValue }) => {
        return (
            <Grid
                item
                container
                margin="normal"
                xs={12}
                md={12}
                sm={12}
                lg={12}
                spacing={2}
            >
                <Grid item xs={12} sm={12} md={6} lg={4}>
                    <Form>
                        <Grid item xs={12}>
                            <Paper className={classes.PaperStyle}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            label="PanNumber"
                                            name="PanNumber"
                                            length="10"
                                            placeholder="PanNumber"
                                            title="PanNumber"
                                        />
                                    </Grid>
                                </Grid>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={12}
                                    lg={12}
                                    spacing={2}
                                    container
                                    className={classes.footerGrid}
                                >
                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <SecondaryButton
                                            variant="contained"
                                            color="secondary"
                                            type="reset"
                                            title="Reset"
                                            onClick={() => {
                                                reset();
                                            }}
                                            className={
                                                classes.CustomDangerButtonStyle
                                            }
                                        >
                                            Reset
                                        </SecondaryButton>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <PrimaryButton
                                            type="submit"
                                            title="Submit"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Submit
                                        </PrimaryButton>
                                    </Grid>
                                </Grid>
                            </Paper>
                        </Grid>
                    </Form>
                </Grid>
                <Grid
                    item
                    xs={12}
                    sm={12}
                    md={6}
                    lg={8}
                    className="lastrecharges"
                >
                    {openDetail ? (
                        <PanDetails request={ResponsDetails} />
                    ) : null}
                </Grid>
            </Grid>
        );
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
                enableReinitialize
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={handleSubmit}
                resetForm={FormObj}
            >
                {TransferForm}
            </Formik>
        </CustomContainer>
    );
};
