import React, { useEffect, useState } from 'react';

import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { AmountInWordLabel } from 'Components/Inputes/FormikMaterialInput';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { Modal, Container, Button, Grid } from '@material-ui/core';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { makeStyles } from '@material-ui/core/styles';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextArea } from 'Components/Inputes/FormikMaterialTextArea';
import { Approve, SelectBYID as SELECT_BY_ID } from 'Components/Api/KYCAPI';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

const useModalStyles = makeStyles((theme) => ({
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

function getModalStyle() {
    const top = 50;
    const left = 50;

    return {
        top: `${top}%`,
        left: `${left}%`,
        transform: `translate(-${top}%, -${left}%)`
    };
}

export const KYCAction = ({ DocumentID, modalOpen, onDismissCallback }) => {
    const modalClasee = useModalStyles();

    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [isError, setIsError] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [severity, setSeverity] = useState('success');
    const [modalStyle] = useState(getModalStyle);
    const [open, setOpen] = useState(modalOpen);
    const [Document, setKYCImage] = useState('');
    const [isApproved, setIsApproved] = useState(false);

    const SearchByID = async () => {
        let JSONObj = {
            UserDocumentID: DocumentID
        };

        var request = JSON.stringify(JSONObj);

        const response = await SELECT_BY_ID(request);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);

        setKYCObject({
            UserDocumentID: data.UserDocumentID,
            DocumentType: data.DocumentType,
            DocumentNo: data.DocumentNo,
            Name: data.Name,
            UserID: data.UserID
        });

        // setUserID(data.UserID);
        setKYCImage(data.Document);
    };

    const [KYCObject, setKYCObject] = useState({
        UserDocumentID: 0,
        UserID: 0,
        IsApproved: isApproved,
        DocumentType: '',
        DocumentNo: '',
        Name: '',
        Remark: ''
    });

    useEffect(() => {
        //
    }, [KYCObject]);

    useEffect(() => {
        SearchByID();
    }, []);

    const handleModalClose = () => {
        setOpen(false);
    };

    const KYCObjSchema = Yup.object({
        Remark: Yup.string().required(PLEASE_PROVIDE_VALID + 'Remark')
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;
        setSubmitting(true);

        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        setIsError(false);

        const response = await Approve(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setResponseMessage(response.data);

        setIsError(true);
        setSubmitting(false);
        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            return;
        }
        // handleModalClose();
        onDismissCallback(false);
        resetForm(KYCObject);
    };

    return (
        <Modal open={open} onClose={handleModalClose}>
            <div style={modalStyle} className={modalClasee.modalContent}>
                <Grid container style={{ display: 'flex', direction: 'rtl' }}>
                    <Button
                        variant="contained"
                        color="secondary"
                        size="small"
                        onClick={handleModalClose}
                    >
                        X
                    </Button>
                </Grid>

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
                        initialValues={KYCObject}
                        validationSchema={KYCObjSchema}
                        onSubmit={handleSubmit}
                        resetForm={KYCObject}
                    >
                        {({ setFieldValue }) => {
                            return (
                                <Form>
                                    <h2 className={classes.PaperHead_h2}>
                                        KYC Approve/Reject
                                    </h2>
                                    <Grid container spacing={1}>
                                        <Grid item xs={5}>
                                            <AmountInWordLabel
                                                label="Name"
                                                name="Name"
                                                type="text"
                                                readOnly={true}
                                                title="Name"
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <AmountInWordLabel
                                                label="DocumentType"
                                                name="DocumentType"
                                                type="text"
                                                readOnly={true}
                                                title="DocumentType"
                                            />
                                        </Grid>

                                        <Grid item xs={4}>
                                            <AmountInWordLabel
                                                label="DocumentNo"
                                                name="DocumentNo"
                                                type="text"
                                                readOnly={true}
                                                title="DocumentNo"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <img
                                                name="KYCImage"
                                                src={Document}
                                                height={250}
                                                width={400}
                                                alt={Document}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <MaterialTextField
                                                label="Remark"
                                                name="Remark"
                                                type="text"
                                                placeholder="Remark"
                                            />
                                        </Grid>
                                        <Grid
                                            container
                                            style={{
                                                margin: '10px',
                                                display: 'flex',
                                                direction: 'rtl'
                                            }}
                                        >
                                            <Button
                                                style={{
                                                    margin: '10px'
                                                }}
                                                type="submit"
                                                variant="contained"
                                                color="primary"
                                                size="small"
                                                onClick={(e) => {
                                                    setFieldValue(
                                                        'IsApproved',
                                                        true
                                                    );
                                                }}
                                            >
                                                Approve
                                            </Button>

                                            <Button
                                                style={{
                                                    margin: '10px'
                                                }}
                                                type="submit"
                                                variant="contained"
                                                color="secondary"
                                                size="small"
                                                onClick={(e) => {
                                                    setFieldValue(
                                                        'IsApproved',
                                                        false
                                                    );
                                                }}
                                            >
                                                Reject
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </Form>
                            );
                        }}
                    </Formik>
                </CustomContainer>
            </div>
        </Modal>
    );
};
