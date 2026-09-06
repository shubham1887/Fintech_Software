import React, { useState } from 'react';
import { Formik, Form } from 'formik';

import * as Yup from 'yup';
import { makeStyles } from '@material-ui/core/styles';
import CustomeAlert from 'Components/Alert/Alert';
import { Modal, Grid, Button, FormControl } from '@material-ui/core';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';

function getModalStyle() {
    const top = 50;
    const left = 50;

    return {
        top: `${top}%`,
        left: `${left}%`,
        transform: `translate(-${top}%, -${left}%)`
    };
}

const useStyles = makeStyles((theme) => ({
    login_form: {
        display: 'flex',
        flexDirection: 'column'
    },
    login_title: {
        fontFamily: 'Raleway-Medium',
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
        background: '#fff',
        padding: theme.spacing(2)
    }
}));

export function OTPModal({
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
        OTP: ''
    };
    const OTPValidationSchema = Yup.object({
        OTP: Yup.string().required('Please enter valid OTP')
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
                                <FormControl className={classes.login_title}>
                                    <b>{title}</b>
                                </FormControl>
                                <Grid container spacing={4}>
                                    <Grid item xs={12} sm={12} md={12}>
                                        <label
                                            style={{
                                                color: 'red'
                                            }}
                                        >
                                            {error}
                                        </label>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={12}>
                                        <MaterialTextField
                                            label="Enter your OTP"
                                            name="OTP"
                                            type="text"
                                            length={6}
                                            placeholder="Enter your OTP"
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={12}>
                                        <FormControl fullWidth>
                                            <Button
                                                type="submit"
                                                variant="contained"
                                                color="primary"
                                                disabled={loading}
                                            >
                                                Verify OTP
                                            </Button>
                                        </FormControl>
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
