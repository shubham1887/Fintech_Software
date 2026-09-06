import React, { useState, useEffect } from 'react';
import { useHistory } from 'react-router';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { useLocation } from 'react-router-dom';
import Box from '@material-ui/core/Box';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { Container, Button, Grid, Paper } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { UserOnbording as DMTREGISTRTION } from 'Components/Api/EkoDMTAPI';
import { UserOnbording as AEPSREGISTRTION } from 'Components/Api/EkoAEPSAPI';
import { SelectProfile as USER_SELECT_BY_ID } from 'Components/Api/UsersApi';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import CircularProgress from '@material-ui/core/CircularProgress';
import HOCLoader from 'Components/Loader/HOCLoader';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
const CustomContainer = HOCLoader(Container);

export const EkoUserOnbording = () => {
    const history = useHistory();
    const classes = commonStyle();
    const location = useLocation();
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [formObject, setFormObject] = useState({
        NameOnPanCard: '',
        PanNo: ''
    });
    const OnbordingFor = location?.state?.OnbordingFor;
    useEffect(() => {
        setLoading(true);
        SelectByID();
    }, []);

    const SelectByID = async () => {
        setLoading(true);

        const { isAuthorized, status, data } = await USER_SELECT_BY_ID();
        setLoading(false);

        if (!isAuthorized) LOGOUT();

        if (!status) return;

        var d = JSON.parse(data);

        setFormObject({
            PanNo: d.PANNo
        });
    };
    const submithandle = async (values, { resetForm }) => {
        setResponseMessage('');
        var requestObj = JSON.stringify(values);
        setIsError(false);
        setLoading(true);

        if (OnbordingFor === 'DMT') {
            var response = await DMTREGISTRTION(requestObj);
        } else {
            var response = await AEPSREGISTRTION(requestObj);
        }

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSeverity('success');
        if (!response.status) {
            setSeverity('error');
            return;
        }

        var data = JSON.parse(response.data);

        setResponseMessage(data.Message);
        if (data.Status === 'FAILURE') {
            setSeverity('error');
            setIsError(true);
            if (data.Message === 'User already onboarded') {
                if (OnbordingFor === 'DMT') {
                    history.push('/EkoDMTSenderSearch');
                    return;
                } else {
                    history.push('/EAEPS');
                    return;
                }
            }
        }
        setResponseMessage(data.Message);
        if (OnbordingFor === 'DMT') {
            history.push('/EkoDMTSenderSearch');
            return;
        } else {
            history.push('/EAEPS');
            return;
        }
    };

    const FormValidationSchema = Yup.object({
        PanNo: Yup.string().required(PLEASE_PROVIDE_VALID + 'PanNo'),
        NameOnPanCard: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'Name On PanCard'
        )
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
                render={({ values, setFieldValue }) => {
                    return (
                        <Grid
                            item
                            container
                            margin="normal"
                            xs={12}
                            md={12}
                            sm={12}
                            lg={12}
                            spacing={2}
                        >
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <Form>
                                    <h2 className={classes.PaperHead_h2}>
                                        User Onbording
                                    </h2>
                                    <Grid item xs={12}>
                                        <Paper className={classes.PaperStyle}>
                                            <Grid container spacing={2}>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <MaterialTextField
                                                        label="Pan Card"
                                                        name="PanNo"
                                                        type="text"
                                                        length={10}
                                                        readOnly={true}
                                                        placeholder="PanNo"
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
                                                        label="Name on Pan Card"
                                                        name="NameOnPanCard"
                                                        type="text"
                                                        placeholder="Name on Pan Card"
                                                    />
                                                </Grid>
                                            </Grid>
                                        </Paper>
                                    </Grid>

                                    <Grid className={classes.footerGrid}>
                                        <Box className={classes.flexReverse}>
                                            <div
                                                className={
                                                    classes.SubmitWrapper
                                                }
                                            >
                                                <Button
                                                    type="submit"
                                                    title="Register"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    disabled={loading}
                                                >
                                                    Register
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
                            </Grid>
                        </Grid>
                    );
                }}
            />
        </CustomContainer>
    );
};
