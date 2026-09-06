import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { Container, Grid } from '@material-ui/core';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const NewsView = ({
    NewsImagePath,
    Title,
    Description,
    onDismissCallback
}) => {
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    useEffect(() => {}, []);

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
            <Formik enableReinitialize={true}>
                {(formProps) => {
                    return (
                        <Form>
                            <Grid container spacing={1}>
                                <Grid item xs={12} sm={12} md={4} lg={4}>
                                    <p>Title: {Title}</p>
                                    <br></br>
                                    <p>Description: {Description}</p>
                                    <br></br>
                                    <img
                                        name="Image"
                                        src={NewsImagePath}
                                        height={300}
                                        width={500}
                                        alt={NewsImagePath}
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
