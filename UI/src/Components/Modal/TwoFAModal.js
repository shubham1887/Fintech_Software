import React, { useState } from 'react';
import { Formik, Form } from 'formik';

import * as Yup from 'yup';
import { makeStyles } from '@material-ui/core/styles';
import CustomeAlert from 'Components/Alert/Alert';
import { Modal, Grid, Button, FormControl, Typography, Paper } from '@material-ui/core';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';

function getModalStyle() {
    const top = 50;
    const left = 50;

    return {
        top: `${top}%`,
        left: `${left}%`,
        transform: `translate(-${top}%, -${left}%)`,
    };
}

const useStyles = makeStyles((theme) => ({
    login_form: {
        display: 'flex',
        flexDirection: 'column'
    },
    login_title: {
        fontSize: '20px',
        lineHeight: '1.2',
        textAlign: 'center',
        textTransform: 'uppercase'
    },
    customButton: {
        marginTop: '2px',
        alignItems: 'right'
    },

    modalContent: {
        position: 'absolute',
        background: 'transform',
        padding: theme.spacing(2)
    }
}));

export function TwoFAModal({
    isOpen,
    error,
    title,
    submitCallback,
    resendCallback,
    onDismissCallback
}) {
    const classes = useStyles();

    const [modalStyle] = useState(getModalStyle);
    const [open, setOpen] = useState(isOpen);

    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');

    var OTPObj = {
        TwoFAPasscode: ''
    };
    const OTPValidationSchema = Yup.object({
        TwoFAPasscode: Yup.string().required(
            'Please enter valid 2FA security code'
        )
    });

    const handleModalClose = () => {
        setOpen(false);
        onDismissCallback(false);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(false);
        error = '';

        submitCallback(values);
        resetForm(OTPObj);
    };
    return (
        <Modal open={open} onClose={handleModalClose}>
            <div style={modalStyle} className={classes.modalContent}>
                {isError ? (
                    <Grid
                        item
                        xs={12}
                        sm={12}
                        md={12}
                        style={{ marginTop: '2em' }}
                    >
                        <CustomeAlert
                            message={error}
                            isOpen={isError}
                            severity={severity}
                            onCloseCallback={(AlertState) => {
                                setIsError(AlertState);
                            }}
                        />
                    </Grid>
                ) : null}

                <Formik
                    initialValues={OTPObj}
                    validationSchema={OTPValidationSchema}
                    onSubmit={submithandle}
                    resetForm={OTPObj}
                >
                    {(formProps) => {
                        return (
                            <Form className={classes.login_form}>

                                <Grid container style={{ justifyContent: "center", alignItems: "center", }}>
                                    <Grid item xs={12} sm={12} md={8} lg={8} xl={8}>

                                        <Paper style={{ padding: "50px 30px", borderRadius: "6px", border: "none", boxShadow: "none", background: "#fff", }}>
                                            <FormControl fullWidth>
                                                <Grid container spacing={2}>

                                                    <Grid style={{ justifyContent: "center", textAlign: "center", marginBottom: "18px", }}>

                                                        <Typography style={{ fontSize: "20px", textAlign: "center", textTransform: "none", fontWeight: "600", marginLeft: "5px", }}>
                                                            Two-Factor Authentication (2FA)
                                                        </Typography>
                                                    </Grid>

                                                    <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                                                        <MaterialTextField
                                                            label="2FA security code"
                                                            name="TwoFAPasscode"
                                                            type="text"
                                                            length={6}
                                                            placeholder="Enter 2FA security code"
                                                            fullWidth
                                                        />
                                                    </Grid>

                                                    <Grid item xs={12} sm={12} md={6} lg={6} xl={6} style={{ marginTop: "6px" }}>
                                                        <Button

                                                            variant="contained"
                                                            onClick={handleModalClose}
                                                            fullWidth
                                                            style={{ height: "46px", background: "#dddddd", border: "solid 1px", borderColor: "#dddddd", textTransform: "none" }}
                                                        >
                                                            Close
                                                        </Button>
                                                    </Grid>

                                                    <Grid item xs={12} sm={12} md={6} lg={6} xl={6} style={{ marginTop: "6px", textTransform: "none", }}>
                                                        <Button
                                                            type="submit"
                                                            variant="contained"
                                                            color="primary"
                                                            disabled={loading}
                                                            fullWidth
                                                            style={{ height: "46px" }}
                                                        >
                                                            Submit
                                                        </Button>
                                                    </Grid>

                                                </Grid>
                                            </FormControl>
                                        </Paper>

                                    </Grid>
                                </Grid>

                            </Form>
                        );
                    }}
                </Formik>
            </div>
        </Modal>
    );
}
