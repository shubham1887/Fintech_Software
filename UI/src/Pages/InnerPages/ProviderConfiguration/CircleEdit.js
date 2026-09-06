import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { SelectByProviderIDOperatorID as SELECT_BY_OPERATOR_ID } from 'Components/Api/ProviderConfigurationApi';
import { CirecleMappingInsert as MAPPING_INSERT } from 'Components/Api/ProviderConfigurationApi';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import Autocomplete from '@material-ui/lab/Autocomplete';
import TextField from '@material-ui/core/TextField';

import { Container, Grid, Button, Hidden } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';
import { ZoneMasterSelect as STATE_SELECT } from 'Components/Api/StatesApi';
import Box from '@material-ui/core/Box';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const CircleEdit = ({
    operatorName,
    OperatorID,
    ProviderID,
    setIsModalOpen,
    onDismissCallback
}) => {
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);

    const [severity, setSeverity] = useState('success');

    const [circleList, setCircleList] = useState([]);
    const [userServiceMappingList, setUserServiceMappingList] = useState([]);

    const [formObject, setFormObject] = useState({
        OperatorID: OperatorID,
        OperatorName: operatorName,
        ProviderID: ProviderID,
        CircleList: []
    });

    useEffect(() => {
        select();
        selectCircle();
    }, []);

    const selectCircle = async () => {
        const response = await STATE_SELECT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setCircleList(data);
    };

    const select = async () => {
        let primaryID = OperatorID;

        let JSONObj = {
            OperatorID: primaryID,
            ProviderID: ProviderID
        };
        setLoading(true);
        var request = JSON.stringify(JSONObj);

        const response = await SELECT_BY_OPERATOR_ID(request);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);

        setFormObject({ ...formObject, CircleList: data ? data : [] });

        if (data === undefined) return;

        if (data === null) return;

        SeletedListIndex(data);
    };

    const SeletedListIndex = (list) => {
        var arryIndex = [];

        for (var parent in list) {
            for (var child in circleList) {
                if (list[parent].ServiceID === circleList[child].ServiceID) {
                    arryIndex.push(circleList[child]);
                }
            }
        }

        setUserServiceMappingList(arryIndex);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);
        setSubmitting(false);

        var response = await MAPPING_INSERT(requestObj);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSubmitting(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setLoading(false);
            setSeverity('error');
            return;
        }

        setTimeout(() => {
            onDismissCallback(false);
            setIsModalOpen(false);
        }, MESSAGE_CLOSE_AFTER);
    };

    const FormValidationSchema = Yup.object({
        OperatorID: Yup.number().required(
            PLEASE_PROVIDE_VALID + 'operator name'
        ),
        ProviderID: Yup.number().required(
            PLEASE_PROVIDE_VALID + 'provider name'
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
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            label="Operator Name"
                                            name="OperatorName"
                                            type="text"
                                            readOnly={true}
                                            placeholder="Operator Name"
                                        />
                                        <Hidden xsUp>
                                            <MaterialTextField
                                                label="ProviderID"
                                                name="ProviderID"
                                                type="text"
                                                readOnly={true}
                                                placeholder="ProviderID"
                                            />

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
                                        lg={12}
                                        className={classes.multiSelect}
                                    >
                                        <Autocomplete
                                            multiple
                                            name="CircleList"
                                            options={circleList}
                                            value={formProps.values.CircleList}
                                            getOptionLabel={(option) =>
                                                option.Name
                                            }
                                            getOptionSelected={(
                                                option,
                                                value
                                            ) =>
                                                option.StateID === value.StateID
                                            }
                                            onChange={(event, values) => {
                                                formProps.values.CircleList = [];

                                                values.map((category) => {
                                                    formProps.values.CircleList.push(
                                                        {
                                                            StateID:
                                                                category.StateID,
                                                            Name: category.Name
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
                                                    label="Circle(s)"
                                                    placeholder="Circle"
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
