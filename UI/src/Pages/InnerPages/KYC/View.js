import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { LOGOUT } from 'ConstComponents';
import { AmountInWordLabel } from 'Components/Inputes/FormikMaterialInput';
import { Container, Grid } from '@material-ui/core';

import { SelectBYID as SELECT_BY_ID } from 'Components/Api/KYCAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const KYCView = ({ UserDocumentID, onDismissCallback }) => {
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [Document, setKYCImage] = useState('');
    const SearchByID = async () => {
        let JSONObj = {
            UserDocumentID: UserDocumentID
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
            Name: data.Name
        });
        setKYCImage(data.Document);
    };

    const [KYCObject, setKYCObject] = useState({
        UserDocumentID: 0,
        DocumentType: '',
        DocumentNo: '',
        Name: ''
    });

    useEffect(() => {
        SearchByID();
    }, [UserDocumentID]);

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
                initialValues={KYCObject}
                resetForm={KYCObject}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <Grid container spacing={1}>
                                <Grid item xs={3}>
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

                                <Grid item xs={3}>
                                    <AmountInWordLabel
                                        label="DocumentNo"
                                        name="DocumentNo"
                                        type="text"
                                        readOnly={true}
                                        title="DocumentNo"
                                    />
                                </Grid>

                                <Grid item xs={12} sm={12} md={4} lg={4}>
                                    <img
                                        name="KYCImage"
                                        src={Document}
                                        height={250}
                                        width={400}
                                        alt={Document}
                                    />
                                </Grid>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
