import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { LOGOUT } from 'ConstComponents';
import * as Yup from 'yup';
import { makeStyles } from '@material-ui/core/styles';

import { MaterialTextAreaField } from 'Components/Inputes/FormikMaterialInput';
import { TestResponse as TEST_RESPONSE } from 'Components/Api/ProviderResponseAPI';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { Modal, Grid, Button, FormControl, Container } from '@material-ui/core';

import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);
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

export function TestResponseModal({
    isOpen,
    text,
    TextMustExists,
    error,
    title,
    submitCallback,
    resendCallback,
    onDismissCallback
}) {
    const classes = useStyles();
    const classe = commonStyle();
    const [modalStyle] = React.useState(getModalStyle);
    const [open, setOpen] = React.useState(isOpen);
    const [responseMessage, setResponseMessage] = useState('');

    const [loading, setLoading] = React.useState(false);

    const [TestObject, setTestObject] = useState({
        ProviderRequestID: '',
        ConsumerNumber: '',
        Amount: '',
        ReferenceNo: '',
        ClosingBalance: '',
        Message: '',
        RefID: '',
        LapuNo: ''
    });

    var formObj = {
        TextCompare: text,
        TextMustExists: TextMustExists,
        OriginalText: ''
    };
    const ProviderValidationSchema = Yup.object({
        TextCompare: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'text compare'
        ),
        OriginalText: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'original text'
        )
    });

    const handleModalClose = () => {
        setOpen(false);
        onDismissCallback(false);
    };

    useEffect(() => {}, []);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);
        setLoading(true);

        setResponseMessage('');
        var response = await TEST_RESPONSE(requestObj);

        setLoading(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setResponseMessage(response.data);
        }

        var data = JSON.parse(response.data);

        setTestObject({
            ProviderRequestID: data.ProviderRequestID,
            ConsumerNumber: data.ConsumerNumber,
            Amount: data.Amount,
            ReferenceNo: data.ReferenceNo,
            ClosingBalance: data.ClosingBalance,
            Message: data.Message,
            RefID: data.RefID,
            LapuNo: data.LapuNo
        });
    };
    return (
        <Modal open={open} onClose={handleModalClose}>
            <div style={modalStyle} className={classes.modalContent}>
                <Formik
                    initialValues={formObj}
                    validationSchema={ProviderValidationSchema}
                    onSubmit={submithandle}
                    resetForm={formObj}
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
                                        <span style={{ color: 'blue' }}>
                                            TextMustExists :
                                        </span>
                                        {TextMustExists}
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={12}></Grid>
                                    <Grid item xs={12} sm={12} md={12}>
                                        <MaterialTextAreaField
                                            label="Text Compare"
                                            name="TextCompare"
                                            type="text"
                                            placeholder="Text Compare"
                                            readOnly={true}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={12}>
                                        <MaterialTextAreaField
                                            label="Original Text"
                                            name="OriginalText"
                                            type="text"
                                            placeholder="Original Text"
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={12}>
                                        <h4 style={{ color: 'red' }}>
                                            {responseMessage}
                                        </h4>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={12}>
                                        [RequestID] :
                                        <h4 className={classe.TestResponse}>
                                            {TestObject.ProviderRequestID}
                                        </h4>
                                        <Grid item xs={12} sm={12} md={12}>
                                            [Number] :
                                            <h4 className={classe.TestResponse}>
                                                {TestObject.ConsumerNumber}
                                            </h4>
                                        </Grid>
                                        [Amount] :
                                        <h4 className={classe.TestResponse}>
                                            {TestObject.Amount}
                                        </h4>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={12}>
                                        [TransID] :
                                        <h4 className={classe.TestResponse}>
                                            {TestObject.ReferenceNo}
                                        </h4>
                                        <Grid item xs={12} sm={12} md={12}>
                                            [ClBal] :
                                            <h4 className={classe.TestResponse}>
                                                {TestObject.ClosingBalance}
                                            </h4>
                                        </Grid>
                                        [Message] :
                                        <h4 className={classe.TestResponse}>
                                            {TestObject.Message}
                                        </h4>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={12}>
                                        [RefID] :
                                        <h4 className={classe.TestResponse}>
                                            {TestObject.RefID}
                                        </h4>
                                        <Grid item xs={12} sm={12} md={12}>
                                            [LapuNo] :
                                            <h4 className={classe.TestResponse}>
                                                {TestObject.LapuNo}
                                            </h4>
                                        </Grid>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={12}>
                                        <FormControl fullWidth>
                                            <Button
                                                type="submit"
                                                variant="contained"
                                                color="primary"
                                                disabled={loading}
                                            >
                                                Test
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
