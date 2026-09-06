import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { Container, Grid, Button, MenuItem } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import CircularProgress from '@material-ui/core/CircularProgress';
import CopyToUrlPopUp from 'Components/CopyToUrlPopUp';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { HeaderBeneficiary } from 'Components/SummaryCards/HeaderBeneficiary';
import CopyToClipBoardInput from 'Components/CopyToClipBoardInput';

import Box from '@material-ui/core/Box';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import {
    Update as UPDATE
} from 'Components/Api/LeadGenerationMasterAPI';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const LeadGenerationPopUP = ({

}) => {
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [formObject, setformObject] = useState({

    });

    const validationSchema = Yup.object({
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Status')
            .required(PLEASE_SELECT_VALID + 'Status')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoadingOpen(true);

        var response = await UPDATE(JSON.stringify(values));

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setSeverity('error');
            setLoadingOpen(false);
            return;
        }

        setTimeout(() => {
            //onDismissCallback(false);
        }, MESSAGE_CLOSE_AFTER);
    };

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
                initialValues={formObject}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <b>Thank you for choosing us for your loan needs! To generate a loan application, please follow the few steps below.URL also sent on your mobile number</b>
                                </Grid>
                            </Grid>

                            <Grid container spacing={2}>
                                <Grid item xs={6} sm={6} md={4} lg={4}>
                                    <div><b>For link -</b></div>
                                    <ol>
                                        <li>Copy the link</li>
                                        <li>Open the link in your mobile browser.</li>
                                        <li>Fill out the loan application form:</li>
                                        <li>
                                            <a href="https://oneapp.abfldirect.com/b2c/login?dsa_hash=9acb66f208afe963fca5a3a876fee1d2e0625e102437ff5bd9df938486a5f95f"
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                style={{ color: 'blue' }}>
                                                Link for loan
                                            </a>
                                        </li>
                                        <CopyToUrlPopUp
                                            text="[https://oneapp.abfldirect.com/b2c/login?
                                            dsa_hash=9acb66f208afe963fca5a3a876fee1d2e0625e102437ff5bd9df938486a5f95f]"
                                        />
                                    </ol>
                                </Grid>
                            </Grid>

                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
