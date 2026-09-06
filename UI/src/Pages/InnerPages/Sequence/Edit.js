import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { LOGOUT } from 'ConstComponents';
import { SelectByOperatorID as SEQUENCE_SELECT_BY_OPERATOR_ID } from 'Components/Api/OperatorSequenceApi';
import { Insert as SEQUENCE_INSERT } from 'Components/Api/OperatorSequenceApi';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import Autocomplete from '@material-ui/lab/Autocomplete';
import TextField from '@material-ui/core/TextField';

import { Container, Grid, Button, Hidden } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';

import Box from '@material-ui/core/Box';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const Edit = ({ OperatorID, setIsModalOpen, onDismissCallback }) => {
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);

    const [severity, setSeverity] = useState('success');

    const [providerList, setProviderList] = useState([]);
    const [userServiceMappingList, setUserServiceMappingList] = useState([]);

    const [formObject, setFormObject] = useState({
        ServiceID: 0,
        ServiceName: '',
        OperatorID: OperatorID,
        OperatorName: '',
        ProviderList: []
    });

    useEffect(() => {
        select();
        selectProvider();
    }, []);

    const selectProvider = async () => {
        const response = await PROVIDER_DROP_DOWN();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setProviderList(data);
    };

    const select = async () => {
        let primaryID = OperatorID;

        let JSONObj = {
            OperatorID: primaryID
        };
        setLoading(true);
        var request = JSON.stringify(JSONObj);

        const response = await SEQUENCE_SELECT_BY_OPERATOR_ID(request);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);

        setFormObject({
            ServiceID: data.ServiceID,
            ServiceName: data.ServiceName,
            OperatorID: data.OperatorID,
            OperatorName: data.OperatorName,
            ProviderList: data.ProviderList ? data.ProviderList : []
        });

        if (data.ProviderList === undefined) return;

        if (data.ProviderList === null) return;

        SeletedListIndex(data.ProviderList);
    };

    const SeletedListIndex = (list) => {
        var arryIndex = [];

        for (var parent in list) {
            for (var child in providerList) {
                if (list[parent].ServiceID === providerList[child].ServiceID) {
                    arryIndex.push(providerList[child]);
                }
            }
        }

        setUserServiceMappingList(arryIndex);
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

        var response = await SEQUENCE_INSERT(requestObj);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSubmitting(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }

        setTimeout(() => {
            onDismissCallback(false);
            setIsModalOpen(false);
        }, MESSAGE_CLOSE_AFTER);
    };

    const FormValidationSchema = Yup.object({
        ServiceName: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'service name'
        ),
        OperatorName: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'operator name'
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
            >
                {(formProps) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <MaterialTextField
                                            label="Service Name"
                                            name="ServiceName"
                                            type="text"
                                            readOnly={true}
                                            placeholder="Service Name"
                                        />

                                        <Hidden xsUp>
                                            <MaterialTextField
                                                label="ServiceID"
                                                name="ServiceID"
                                                type="text"
                                                readOnly={true}
                                                placeholder="ServiceID"
                                            />
                                        </Hidden>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <MaterialTextField
                                            label="Operator Name"
                                            name="OperatorName"
                                            type="text"
                                            readOnly={true}
                                            placeholder="Operator Name"
                                        />

                                        <Hidden xsUp>
                                            <MaterialTextField
                                                label="OperatorID"
                                                name="OperatorID"
                                                type="text"
                                                readOnly={true}
                                                placeholder="OperatorID"
                                            />
                                        </Hidden>
                                    </Grid>

                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={12}
                                        className={classes.multiSelect}
                                    >
                                        <Autocomplete
                                            multiple
                                            name="ProviderList"
                                            options={providerList}
                                            value={
                                                formProps.values.ProviderList
                                            }
                                            getOptionLabel={(option) =>
                                                option.ProviderName
                                            }
                                            getOptionSelected={(
                                                option,
                                                value
                                            ) =>
                                                option.ProviderID ===
                                                value.ProviderID
                                            }
                                            onChange={(event, values) => {
                                                formProps.values.ProviderList = [];

                                                values.map((category) => {
                                                    formProps.values.ProviderList.push(
                                                        {
                                                            ProviderID:
                                                                category.ProviderID,
                                                            ProviderName:
                                                                category.ProviderName
                                                        }
                                                    );
                                                });

                                                SeletedListIndex(
                                                    formProps.values
                                                        .ProviderList
                                                );
                                            }}
                                            renderInput={(params) => (
                                                <TextField
                                                    {...params}
                                                    variant="standard"
                                                    label="Provider(s)"
                                                    placeholder="Provider"
                                                />
                                            )}
                                        />
                                    </Grid>
                                </Grid>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Update"
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
            </Formik>
        </CustomContainer>
    );
};
