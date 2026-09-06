import React, { useState } from 'react';
import { Formik, Form } from 'formik';

import * as Yup from 'yup';
import { makeStyles } from '@material-ui/core/styles';
import CustomeAlert from 'Components/Alert/Alert';
import {
    Modal,
    Grid,
    Button,
    FormControl,
    CircularProgress,
    Paper
} from '@material-ui/core';
import { useHistory } from 'react-router';

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

export function SubscriptionRenew({
    isOpen,
    error,
    submitCallback,
    onDismissCallback,
    requestObj
}) {
    const classes = useStyles();

    const [modalStyle] = useState(getModalStyle);
    const [open, setOpen] = useState(isOpen);

    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const history = useHistory();

    var FromObj = {
        error: ''
    };

    const handleModalClose = () => {
        setOpen(false);
        onDismissCallback(false);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(false);
        error = '';

        try {
            await submitCallback(values, requestObj);
            resetForm(FromObj);
            history.push({
                pathname: '/SubscriptionPendingPayment',
                state: { requestObj: requestObj }
            });
        } catch (error) {
            setIsError(true);
            setSeverity('error');
            setLoading(false);
        }
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
                    initialValues={FromObj}
                    onSubmit={submithandle}
                    resetForm={FromObj}
                >
                    {(formProps) => {
                        return (
                            <Form className={classes.login_form}>
                                <Grid container spacing={4}>
                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={12}
                                        style={{ textAlign: 'center' }}
                                    >
                                        <label
                                            style={{
                                                color: 'red',
                                                fontSize: '18px'
                                            }}
                                        >
                                            {error}
                                        </label>
                                    </Grid>

                                    <FormControl fullWidth>
                                        <Grid container spacing={2}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={6}
                                                md={12}
                                                style={{
                                                    justifyContent: 'end',
                                                    margin: 20,
                                                    display: 'flex'
                                                }}
                                            >
                                                <Button
                                                    style={{
                                                        marginTop: 10,
                                                        marginRight: 10
                                                    }}
                                                    type="submit"
                                                    variant="contained"
                                                    color="primary"
                                                    disabled={loading}
                                                >
                                                    Submit
                                                </Button>
                                                <Button
                                                    style={{ marginTop: 10 }}
                                                    variant="contained"
                                                    color="secondary"
                                                    onClick={handleModalClose}
                                                >
                                                    Close
                                                </Button>

                                            </Grid>
                                        </Grid>
                                    </FormControl>
                                </Grid>
                            </Form>
                        );
                    }}
                </Formik>
            </div >
        </Modal >
    );
}
