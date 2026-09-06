import React, { memo, useState, useEffect } from 'react';
import { useHistory } from 'react-router';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Box,
    Button,
    Paper,
    CircularProgress,
    MenuItem,
    ListItemIcon
} from '@material-ui/core';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';

import { LOGOUT } from 'ConstComponents';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import { Add as BBPSCONFIGURATION_ADD } from 'Components/Api/BBPSConfigurationAPI';
import { SelectBBPSOperator as SELECT_BBPS_OPERATOR } from 'Components/Api/OperatorsApi';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);
export const Add = memo(() => {
    const classes = commonStyle();
    const history = useHistory();
    const [Operator, setOperator] = useState([]);

    const [Type] = useState([
        { name: 'Text' },
        { name: 'Date' },
        { name: 'Number' },
        { name: 'Email' }
    ]);

    const [CustomContainerProps, setCustomContainerProps] = useState({
        loadingOpen: false,
        isError: false,
        severity: 'success',
        responseMessage: ''
    });

    const [FormObj] = useState({
        OperatorID: 0,
        Label: '',
        PlaceHolder: '',
        IsRequired: false,
        Sequence: '',

        ErrorMeeage: ''
    });

    const FormValidationSchema = Yup.object({
        Label: Yup.string().required(PLEASE_PROVIDE_VALID + 'service label'),
        PlaceHolder: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' placeholder'
        ),
        Sequence: Yup.string().required(PLEASE_PROVIDE_VALID + ' sequence')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (CustomContainerProps.loading) {
            setCustomContainerProps(...CustomContainerProps, {
                loading: false
            });
            return;
        }
        var requestObj = JSON.stringify(values);

        setCustomContainerProps({
            isError: false,
            loading: true
        });
        var response = await BBPSCONFIGURATION_ADD(requestObj);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setCustomContainerProps({
                isError: true,
                loading: false,
                severity: 'error',
                responseMessage: response.data
            });
            return;
        }
        setCustomContainerProps({
            isError: false,
            loading: false,
            severity: 'success',
            responseMessage: response.data
        });
        resetForm(FormObj);
    };

    const selectOperator = async () => {
        setCustomContainerProps({
            isError: false,
            loading: true
        });

        let jsonObj = {
            ServiceID: 0
        };
        const response = await SELECT_BBPS_OPERATOR(JSON.stringify(jsonObj));
        setCustomContainerProps({
            loading: false
        });
        if (!response.isAuthorized) LOGOUT();

        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);
            setOperator(data);
        }
    };

    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
    };
    useEffect(() => {
        selectOperator();
    }, []);
    return (
        <CustomContainer
            isLoading={CustomContainerProps.loading}
            isError={CustomContainerProps.isError}
            alertMessage={CustomContainerProps.responseMessage}
            severity={CustomContainerProps.severity}
            onCloseCallback={(alertState) => {
                setCustomContainerProps({
                    isError: alertState
                });
            }}
        >
            <Grid container spacing={3}>
                <Grid item xs={6}>
                    <h2 className={classes.PaperHead_h2}>Add Configurations</h2>
                </Grid>
                <Grid item xs={6}>
                    <Box className={classes.flexReverse}>
                        <Button
                            title=" All Configurations"
                            variant="contained"
                            color="primary"
                            className={classes.CustomPrimaryButtonStyle}
                            onClick={() => {
                                history.push('/BBPSConfiguration/Index');
                            }}
                        >
                            All Configurations
                        </Button>
                    </Box>
                </Grid>
            </Grid>
            <Formik
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <MaterialSelect
                                                label="Select Operator"
                                                name="OperatorID"
                                                title="Operator"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Operator
                                                </MenuItem>
                                                {Operator.map((item) => (
                                                    <MenuItem
                                                        key={item.OperatorID}
                                                        value={item.OperatorID}
                                                    >
                                                        <ListItemIcon>
                                                            <img
                                                                src={item.Path}
                                                                height={30}
                                                                width={30}
                                                                alt={
                                                                    item.OperatorName
                                                                }
                                                            />
                                                        </ListItemIcon>
                                                        {item.OperatorName}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <MaterialSelect
                                                label="Select Type"
                                                name="Type"
                                                title="Type"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Type
                                                </MenuItem>
                                                {Type.map((item) => (
                                                    <MenuItem
                                                        key={item.name}
                                                        value={item.name}
                                                    >
                                                        {item.name}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>

                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <MaterialTextField
                                                label="Label"
                                                name="Label"
                                                type="text"
                                                placeholder="Label"
                                            />
                                        </Grid>
                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <MaterialTextField
                                                label="PlaceHolder"
                                                name="PlaceHolder"
                                                type="text"
                                                placeholder="PlaceHolder"
                                            />
                                        </Grid>
                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <FormikMaterialSwitch
                                                label="IsRequired"
                                                name="IsRequired"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnChange(
                                                        name,
                                                        value,
                                                        formProps.setFieldValue
                                                    );
                                                }}
                                            />
                                        </Grid>
                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <MaterialTextField
                                                label="ErrorMeeage"
                                                name="ErrorMeeage"
                                                type="text"
                                                placeholder="ErrorMeeage"
                                            />
                                        </Grid>
                                        <Grid item xs={4} sm={4} md={4} lg={4}>
                                            <MaterialNumberField
                                                label="Sequence"
                                                name="Sequence"
                                                type="text"
                                                length={7}
                                                placeholder="Sequence"
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
                                            title="Save"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={
                                                CustomContainerProps.loading
                                            }
                                        >
                                            Save
                                        </Button>
                                        {CustomContainerProps.loading && (
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
});
