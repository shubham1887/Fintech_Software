import React, { useState } from 'react';
import { Formik, Form } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { LOGOUT } from 'ConstComponents';
import { UpdatePin as USER_PIN_UPDATE } from 'Components/Api/UsersApi';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { Container, Grid, Paper, Button } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const UpdateMpin = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);

        var response = await USER_PIN_UPDATE(requestObj);
        setLoading(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }
        setIsError(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setLoading(false);
            setSeverity('error');
            return;
        }
        resetForm(FormObj);

        setTimeout(() => {}, MESSAGE_CLOSE_AFTER);
    };

    var FormObj = {
        NewPin: '',
        ConfirmPin: '',
        CurrentPassword: ''
    };

    const FormValidationSchema = Yup.object({
        NewPin: Yup.string()
            .length(4, 'Must be 4 digit')

            .required(PLEASE_PROVIDE_VALID + 'new pin'),
        ConfirmPin: Yup.string()
            .length(4, 'Must be 4 digit')
            .required(PLEASE_PROVIDE_VALID + 'confirm pin'),
        CurrentPassword: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'current password'
        )
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
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>Change Pin</h2>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="Current Password"
                                                name="CurrentPassword"
                                                type="password"
                                                placeholder="Current Password"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialNumberField
                                                label="New Pin"
                                                name="NewPin"
                                                type="password"
                                                length={4}
                                                placeholder="New Pin"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialNumberField
                                                label="Confirm Pin"
                                                name="ConfirmPin"
                                                type="password"
                                                length={4}
                                                placeholder="Confirm Pin"
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
                                        variant="contained"
                                        color="secondary"
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
