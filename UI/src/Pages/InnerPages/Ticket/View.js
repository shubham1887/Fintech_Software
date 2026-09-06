import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { LOGOUT } from 'ConstComponents';
import { AmountInWordLabel } from 'Components/Inputes/FormikMaterialInput';
import { Container, Grid } from '@material-ui/core';

import { SelectByID as SELECT_BY_ID } from 'Components/Api/TicketAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const TicketView = ({ TicketMasterID, onDismissCallback }) => {
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [Document, setTicketImage] = useState('');
    const SearchByID = async () => {
        let JSONObj = {
            TicketMasterID: TicketMasterID
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

        setTicketObject({
            TicketMasterID: data.TicketMasterID,
            Body: data.Body,
            Subject: data.Subject,
            FirmName: data.FirmName
        });

        setTicketImage(data.Document);
    };

    const [TicketObject, setTicketObject] = useState({
        TicketMasterID: 0,
        Body: '',
        Subject: '',
        FirmName: ''
    });

    useEffect(() => {
        SearchByID();
    }, [TicketMasterID]);

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
                initialValues={TicketObject}
                //validationSchema={validationSchema}
                //  onSubmit={submithandle}
                resetForm={TicketObject}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <Grid container spacing={4}>
                                <Grid item xs={3}>
                                    <AmountInWordLabel
                                        label="FirmName"
                                        name="FirmName"
                                        type="text"
                                        readOnly={true}
                                        title="FirmName"
                                    />
                                </Grid>
                                <Grid item xs={3}>
                                    <AmountInWordLabel
                                        label="Subject"
                                        name="Subject"
                                        type="text"
                                        readOnly={true}
                                        title="Subject"
                                    />
                                </Grid>

                                <Grid item xs={6}>
                                    <AmountInWordLabel
                                        label="Body"
                                        name="Body"
                                        type="text"
                                        readOnly={true}
                                        title="Body"
                                    />
                                </Grid>

                                <Grid item xs={12} sm={12} md={4} lg={4}>
                                    <img
                                        name="TicketImage"
                                        src={Document}
                                        height={300}
                                        width={500}
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
