import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';

import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { LOGOUT } from 'ConstComponents';
import { Update as NEWS_UPDATE } from 'Components/Api/NewsAPI';
import { SelectByID as NEWS_SELECT_BY_NEWSID } from 'Components/Api/NewsAPI';
import {
    MaterialTextField,
    MaterialTextAreaField
} from 'Components/Inputes/FormikMaterialInput';
import { Container, Grid, Button } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

const NewsEdit = ({ NewsID, onDismissCallback }) => {
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [formObject, setFormObject] = useState({
        NewsID: 0,
        Title: '',
        Description: '',
        HyperLink: ''
    });
    useEffect(() => {
        SelectByID(NewsID);
    }, []);

    const SelectByID = async (NewsID) => {
        let JSONObj = {
            NewsID: NewsID
        };
        var request = JSON.stringify(JSONObj);

        const response = await NEWS_SELECT_BY_NEWSID(request);

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);

        setFormObject({
            NewsID: data.NewsID,
            Title: data.Title,
            Description: data.Description,
            HyperLink: data.HyperLink
        });
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            NewsID: values.NewsID,
            Title: values.Title,
            Description: values.Description,
            HyperLink: values.HyperLink
        };
        var requestObj = JSON.stringify(inputObj);
        setLoading(true);
        setIsError(false);

        var response = await NEWS_UPDATE(requestObj);
        if (!response.isAuthorized) LOGOUT();
        setIsError(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setLoading(false);
            setSeverity('error');
            return;
        }
        resetForm(formObject);
        setTimeout(() => {
            onDismissCallback(false);
        }, MESSAGE_CLOSE_AFTER);
    };

    const FormValidationSchema = Yup.object({
        Title: Yup.string().required(PLEASE_PROVIDE_VALID + 'title'),
        Description: Yup.string().required(PLEASE_PROVIDE_VALID + 'description')
    });

    return (
        <CustomContainer
            isLoading={loading}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Formik
                enableReinitialize={true}
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <Grid item xs={12} lg={12} md={12}>
                                <Grid container spacing={1}>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            label="Title"
                                            name="Title"
                                            type="text"
                                            placeholder="Title"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextAreaField
                                            label="Description"
                                            name="Description"
                                            type="text"
                                            placeholder="Description"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextAreaField
                                            label="HyperLink"
                                            name="HyperLink"
                                            type="text"
                                            placeholder="HyperLink"
                                        />
                                    </Grid>
                                </Grid>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Save"
                                            variant="contained"
                                            color="primary"
                                            disabled={loading}
                                        >
                                            Update
                                        </Button>
                                        {loading && (
                                            <CircularProgress
                                                size={24}
                                                className={
                                                    classes.buttonProgress
                                                }
                                            />
                                        )}
                                    </div>
                                    <Button
                                        type="reset"
                                        title="Reset"
                                        variant="contained"
                                        color="secondary"
                                        className={
                                            classes.CustomDangerButtonStyle
                                        }
                                    >
                                        Reset
                                    </Button>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};

export default NewsEdit;
