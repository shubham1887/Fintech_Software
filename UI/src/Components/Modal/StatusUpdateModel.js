import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { LOGOUT } from 'ConstComponents';
import * as Yup from 'yup';
import { makeStyles } from '@material-ui/core/styles';
import { CustomeAlert } from 'Components/Alert/Alert';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import {
    Box,
    Modal,
    Grid,
    Button,
    MenuItem,
    FormControl
} from '@material-ui/core';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { UpdateStatusmanually as STATUS_UPDATE } from 'Components/Api/TransactionAPI';

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

export function StatusUpdateModal({
    TransactionMasterID,
    isOpen,
    error,
    title,
    submitCallback,
    resendCallback,
    onDismissCallback
}) {
    const classes = useStyles();

    const [modalStyle] = React.useState(getModalStyle);
    const [open, setOpen] = React.useState(isOpen);

    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [TransactionID, setTransactionID] = useState('none');

    const [formObject, setFormObject] = useState({
        TransactionMasterID: TransactionMasterID,
        Status: '',
        ReferenceNo: ''
    });

    const OnChange = (name, value, setFieldValue) => {
        if (value === 'SUCCESS') {
            setTransactionID('inline');
        }
        if (value === 'REFUND') {
            setTransactionID('none');
        }
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }

        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);
        setSubmitting(false);
        setResponseMessage(response.data);

        var response = await STATUS_UPDATE(requestObj);

        setLoading(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }

        submitCallback(response);
    };

    const FormValidationSchema = Yup.object({
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Status')
            .required(PLEASE_SELECT_VALID + 'Status')
    });

    const handleOpen = () => {
        setOpen(true);
    };

    const handleModalClose = () => {
        setOpen(false);
        onDismissCallback(false);
    };
    const handleResend = () => {
        resendCallback(true);
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
                    initialValues={formObject}
                    validationSchema={FormValidationSchema}
                    onSubmit={submithandle}
                    resetForm={formObject}
                >
                    {({ values, setFieldValue, handleSubmit }) => {
                        return (
                            <Form className={classes.login_form}>
                                <FormControl className={classes.login_title}>
                                    {title}
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

                                    <Grid item xs={12} sm={12} md={12} lg={12}>
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
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <Box display={TransactionID}>
                                            <MaterialTextField
                                                label="Transaction ID"
                                                name="ReferenceNo"
                                                type="text"
                                                length={25}
                                                placeholder="Transaction ID"
                                            />
                                        </Box>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={12}>
                                        <FormControl fullWidth>
                                            <Button
                                                type="submit"
                                                variant="contained"
                                                color="primary"
                                                disabled={loading}
                                            >
                                                Update
                                            </Button>

                                            <Button
                                                style={{ marginTop: 10 }}
                                                variant="contained"
                                                color="secondary"
                                                onClick={handleModalClose}
                                            >
                                                Close
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
