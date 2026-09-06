import React, { useEffect, useState } from 'react';
import { useHistory } from 'react-router';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';

import {
    SelectByID as PROVIDER_SELECT_BY_ID,
    SelectForDropDown as PROVIDER_DROP_DOWN
} from 'Components/Api/ProviderApi';
import { Update as PROVIDER_UPDATE } from 'Components/Api/ProviderApi';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import {
    Container,
    Grid,
    Paper,
    Button,
    MenuItem,
    Box,
    CircularProgress
} from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const ProviderEdit = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [providerList, setProviderList] = useState([]);

    const [formObject, setFormObject] = useState({
        ProviderID: 0,
        ProviderName: '',
        ProviderUrl: '',
        IPAddress: '',
        LoginID: '',
        Token: '',
        CopyFromProviderID: 0,
        SupportNumber: '',
        LowBalanceAmount: 0,
        IsRealAPI: false
    });
    const selectProviderDropDown = async () => {
        setLoadingOpen(true);
        const response = await PROVIDER_DROP_DOWN();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
    };

    useEffect(() => {
        selectProvider();
        selectProviderDropDown();
    }, []);
    const selectProvider = async () => {
        let URL = window.location.pathname;
        var res = URL.split('/');

        let primaryID = res[res.length - 1];

        let JSONObj = {
            ProviderID: primaryID
        };
        setLoadingOpen(true);

        var request = JSON.stringify(JSONObj);

        const response = await PROVIDER_SELECT_BY_ID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);

        setFormObject({
            ProviderID: data.ProviderID,
            ProviderName: data.ProviderName,
            ProviderUrl: data.ProviderUrl,
            IPAddress: data.IPAddress,
            CopyFromProviderID: data.CopyFromProviderID,
            IsRealAPI: data.IsRealAPI,
            LoginID: data.LoginID,
            Token: data.Token,
            SupportNumber: data.SupportNumber,
            LowBalanceAmount: data.LowBalanceAmount
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

        var response = await PROVIDER_UPDATE(requestObj);
        if (!response.isAuthorized) LOGOUT();

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
            history.push('/ProvidersAdd');
        }, MESSAGE_CLOSE_AFTER);
    };
    const FormValidationSchema = Yup.object({
        ProviderName: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'Provider name'
        ),
        ProviderUrl: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'Provider url'
        ),
        IPAddress: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'API IP address'
        )
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
                            <h2 className={classes.PaperHead_h2}>
                                Provider edit
                            </h2>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={3}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="Provider Name"
                                                name="ProviderName"
                                                type="text"
                                                placeholder="Provider Name"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="Provider Url"
                                                name="ProviderUrl"
                                                type="text"
                                                placeholder="Provider Url"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="IP Address"
                                                name="IPAddress"
                                                type="text"
                                                placeholder="IP Address"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                name="CopyFromProviderID"
                                                label="Copy From ProviderID"
                                                title="CopyFromProviderID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Provider
                                                </MenuItem>
                                                {providerList.map(
                                                    (provider) => (
                                                        <MenuItem
                                                            key={
                                                                provider.ProviderID
                                                            }
                                                            value={
                                                                provider.ProviderID
                                                            }
                                                        >
                                                            {
                                                                provider.ProviderName
                                                            }
                                                        </MenuItem>
                                                    )
                                                )}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="LoginID"
                                                name="LoginID"
                                                type="password"
                                                placeholder="LoginID"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="Token"
                                                name="Token"
                                                type="password"
                                                placeholder="Token"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialNumberField
                                                label="Support Number"
                                                name="SupportNumber"
                                                type="text"
                                                placeholder="Support Number"
                                                length={10}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialNumberField
                                                label="LowBalance Amount"
                                                name="LowBalanceAmount"
                                                type="text"
                                                placeholder="LowBalance Amount"
                                                length={7}
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
