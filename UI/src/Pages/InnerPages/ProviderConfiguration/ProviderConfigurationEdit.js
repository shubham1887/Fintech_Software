import React, { useEffect, useState } from 'react';
import { useHistory } from 'react-router';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';

import { SelectByID as PROVIDER_OPERATOR_SELECT_BY_ID } from 'Components/Api/ProviderConfigurationApi';
import { Update as PROVIDER_OPERATOR_UPDATE } from 'Components/Api/ProviderConfigurationApi';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';

import HOCLoader from 'Components/Loader/HOCLoader';

import {
    Container,
    Grid,
    MenuItem,
    Paper,
    Button,
    Hidden
} from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';

import Box from '@material-ui/core/Box';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';

export const ProviderOperatorEdit = () => {
    const CustomContainer = HOCLoader(Container);
    const classes = commonStyle();
    const history = useHistory();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [ProviderID, setProviderID] = useState(0);
    const [ProviderName, setProviderName] = useState(0);
    const [severity, setSeverity] = useState('success');
    const [formObject, setFormObject] = useState({
        ProviderID: 0,
        ServiceName: '',
        OperatorName: '',
        OpCode: '',
        Quantity: '',
        AdminCommType: '0',
        AdminCommi: 0,
        ConfigurationID: 0
    });
    useEffect(() => {
        selectProviderOperator();
    }, []);

    const selectProviderOperator = async () => {
        let URL = window.location.pathname;
        var res = URL.split('/');

        let ConfigurationID = res[res.length - 1];

        let JSONObj = {
            ConfigurationID: ConfigurationID
        };
        setLoadingOpen(true);
        var request = JSON.stringify(JSONObj);

        const response = await PROVIDER_OPERATOR_SELECT_BY_ID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        var data = JSON.parse(response.data);

        setProviderID(data.ProviderID);
        setProviderName(data.ProviderName);

        setFormObject({
            ServiceName: data.ServiceName,
            ProviderID: data.ProviderID,
            OperatorName: data.OperatorName,
            OpCode: data.OpCode,
            Quantity: data.Quantity,
            AdminCommType: data.AdminCommType,
            AdminCommi: data.AdminCommi,
            ConfigurationID: ConfigurationID
        });
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        setIsError(false);
        setSubmitting(false);

        var response = await PROVIDER_OPERATOR_UPDATE(requestObj);
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
            history.push('/ProviderConfiguration/Index/' + ProviderID);
        }, MESSAGE_CLOSE_AFTER);
    };
    const FormValidationSchema = Yup.object({
        OpCode: Yup.string().required(PLEASE_PROVIDE_VALID + 'operator code'),

        AdminCommType: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'comm type')
            .required(PLEASE_SELECT_VALID + 'comm type')
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
                            <h3 className={classes.PaperHead_h2}>
                                <h2>{ProviderName} Provider :</h2>Operator edit
                            </h3>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialTextField
                                                label="Service Name"
                                                name="ServiceName"
                                                type="text"
                                                readOnly={true}
                                                placeholder="ServiceName"
                                            />

                                            <Hidden xsUp>
                                                <MaterialTextField
                                                    label="ConfigurationID"
                                                    name="ConfigurationID"
                                                    type="text"
                                                    readOnly={true}
                                                    placeholder="ConfigurationID"
                                                />
                                            </Hidden>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialTextField
                                                label="Operator Name"
                                                name="OperatorName"
                                                type="text"
                                                readOnly={true}
                                                placeholder="OperatorName"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialTextField
                                                label="OpCode"
                                                name="OpCode"
                                                type="text"
                                                placeholder="OpCode"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialTextField
                                                label="Allowed Quantity"
                                                name="Quantity"
                                                type="text"
                                                placeholder="Quantity"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialSelect
                                                label="Admin Commssion Type"
                                                name="AdminCommType"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Commssion Type
                                                </MenuItem>
                                                <MenuItem
                                                    key="Flexi"
                                                    value="Flexi"
                                                >
                                                    Flexi
                                                </MenuItem>
                                                <MenuItem
                                                    key="Fixed"
                                                    value="Fixed"
                                                >
                                                    Fixed
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialTextField
                                                label="Admin Commission"
                                                name="AdminCommi"
                                                type="text"
                                                placeholder="Admin Commission"
                                            />
                                        </Grid>
                                    </Grid>
                                </Paper>
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
                                            disabled={loadingOpen}
                                        >
                                            Update
                                        </Button>
                                        {loadingOpen && (
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
