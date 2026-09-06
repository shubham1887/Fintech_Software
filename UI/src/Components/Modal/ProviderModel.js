import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { LOGOUT } from 'ConstComponents';
import * as Yup from 'yup';
import { makeStyles } from '@material-ui/core/styles';
import CustomeAlert from 'Components/Alert/Alert';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import { Modal, Grid, Button, MenuItem, FormControl } from '@material-ui/core';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
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

export function ProviderModal({
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

    const [Providers, setProviders] = useState([]);
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');

    var ProviderObj = {
        ProviderID: 0
    };
    const ProviderValidationSchema = Yup.object({
        ProviderID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'providerID')
            .required(PLEASE_SELECT_VALID + 'providerID')
    });

    const handleModalClose = () => {
        setOpen(false);
        onDismissCallback(false);
    };

    const selectProviderDropDown = async () => {
        const response = await PROVIDER_DROP_DOWN();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviders(data);
    };

    useEffect(() => {
        selectProviderDropDown();
    }, []);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(false);
        error = '';
        var requestObj = JSON.stringify(values);
        submitCallback(values);
        resetForm(ProviderObj);
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
                    initialValues={ProviderObj}
                    validationSchema={ProviderValidationSchema}
                    onSubmit={submithandle}
                    resetForm={ProviderObj}
                >
                    {(formProps) => {
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

                                    <Grid item xs={12} sm={12} md={12}>
                                        <MaterialSelect
                                            label="Provider"
                                            name="ProviderID"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Providers
                                            </MenuItem>
                                            {Providers.map((Item) => (
                                                <MenuItem
                                                    key={Item.ProviderID}
                                                    value={Item.ProviderID}
                                                >
                                                    {Item.ProviderName}
                                                </MenuItem>
                                            ))}
                                        </MaterialSelect>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={12}>
                                        <FormControl fullWidth>
                                            <Button
                                                type="submit"
                                                variant="contained"
                                                color="primary"
                                                disabled={loading}
                                            >
                                                Send
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
