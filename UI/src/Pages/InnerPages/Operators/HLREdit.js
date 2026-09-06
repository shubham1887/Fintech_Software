import React, { useEffect, useState } from 'react';
import { useHistory } from 'react-router';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';

import { SelectByID as SELECT_BY_ID } from 'Components/Api/OperatorStateMaapingApi';
import { Update as HLR_UPDATE } from 'Components/Api/OperatorStateMaapingApi';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';

import { Container, Grid, Paper, Button } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';

import Box from '@material-ui/core/Box';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const HLREdit = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [formObject, setFormObject] = useState({
        OperatorStateMappingID: 0,
        OperatorName: '',
        StateName: '',
        HLR: ''
    });
    useEffect(() => {
        selectByID();
    }, []);
    const selectByID = async () => {
        let URL = window.location.pathname;
        var res = URL.split('/');

        let primaryID = res[res.length - 1];

        let JSONObj = {
            OperatorStateMappingID: primaryID
        };
        setLoadingOpen(true);

        var request = JSON.stringify(JSONObj);

        const response = await SELECT_BY_ID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        var data = JSON.parse(response.data);

        setFormObject({
            OperatorStateMappingID: data.OperatorStateMappingID,
            OperatorName: data.OperatorName,
            StateName: data.StateName,
            HLR: data.HLR
        });
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

        var response = await HLR_UPDATE(requestObj);
        if (!response.isAuthorized) {
            LOGOUT();
        }

        setIsError(true);
        setSubmitting(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }

        resetForm(formObject);

        setTimeout(() => {
            history.push('/Operators/HLR');
        }, MESSAGE_CLOSE_AFTER);
    };
    const FormValidationSchema = Yup.object({
        HLR: Yup.string().required(PLEASE_PROVIDE_VALID + 'HLR')
    });

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
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
                render={(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>HLR edit </h2>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialTextField
                                                label="Operator"
                                                name="OperatorName"
                                                type="text"
                                                placeholder="Operator"
                                                readOnly={true}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialTextField
                                                label="StateName"
                                                name="StateName"
                                                type="text"
                                                placeholder="StateName"
                                                readOnly={true}
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
                                                label="HLR"
                                                name="HLR"
                                                type="text"
                                                placeholder="HLR"
                                            />
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <h3 className={classes.Provider_Format}>
                                    Note : Use comma separated for HLR Eg. 9545,
                                    9823
                                </h3>
                            </Grid>
                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Upadate"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
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
            />
        </CustomContainer>
    );
};
