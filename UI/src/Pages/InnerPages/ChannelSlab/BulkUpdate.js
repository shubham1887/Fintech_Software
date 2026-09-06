import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Box,
    Button,
    MenuItem,
    CircularProgress
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { ToggleButton, ToggleButtonGroup } from '@material-ui/lab';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { BulkUpdate as BULK_UPDATE_COMMISSION } from 'Components/Api/ChannelSlabDetailAPI';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { SelectByRechargeServices as SERVICE_SELECT_RECHARGE } from 'Components/Api/ServiceApi';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const BulkUpdate = ({ onDismissCallback }) => {
    const classes = commonStyle();

    const [loadingOpen, setLoadingOpen] = useState(false);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [Services, setServices] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [CommissionCategory, setCommissionCategory] = useState('Commission');
    const [CommissionType, setCommissionType] = useState('Fix');
    const SlabTypes = [{ Type: 'General' }, { Type: 'Real' }];

    const SlabTypeItems = SlabTypes.map(({ Type }, index) => (
        <MenuItem key={index} value={Type}>
            {Type}
        </MenuItem>
    ));
    const [formObject, setFormObject] = useState({
        ServiceID: 0,
        OperatorID: 0,
        Commission: '',
        SlabType: 'General',
        CommissionType: CommissionType,
        CommissionCategory: CommissionCategory
    });

    const FormValidationSchema = Yup.object({
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'serviceID')
            .required(PLEASE_SELECT_VALID + 'serviceID'),
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator')
    });

    const selectServices = async () => {
        const response = await SERVICE_SELECT_RECHARGE();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const OnOperatorChange = async (name, value, setFieldValue) => {
        var ServiceID = value; //event.target.value;

        setFieldValue(name, value);

        selectOperator(ServiceID);
    };

    const selectOperator = async (ServiceID) => {
        if (
            ServiceID === null ||
            ServiceID === undefined ||
            ServiceID === 0 ||
            ServiceID === '0'
        )
            return;

        var requestObj = {
            ServiceID: ServiceID
        };
        var request = JSON.stringify(requestObj);

        const response = await OPERATOR_SELECT_BY_SERVICEID(request);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) setIsError(true);

        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);
            setOperator(data);
        }
    };
    useEffect(() => {
        selectServices();
    }, []);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);
        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);

        var response = await BULK_UPDATE_COMMISSION(requestObj);

        setLoadingOpen(false);
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
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialSelect
                                            label="Service"
                                            name="ServiceID"
                                            onCustomCallback={(name, value) => {
                                                OnOperatorChange(
                                                    name,
                                                    value,
                                                    setFieldValue
                                                );
                                            }}
                                            title="Service"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Service
                                            </MenuItem>
                                            {Services.map((Item) => (
                                                <MenuItem
                                                    key={Item.ServiceID}
                                                    value={Item.ServiceID}
                                                >
                                                    {Item.ServiceName}
                                                </MenuItem>
                                            ))}
                                        </MaterialSelect>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialSelect
                                            label="Operator"
                                            name="OperatorID"
                                            title="Operator"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Operator
                                            </MenuItem>
                                            {Operator.map((Item) => (
                                                <MenuItem
                                                    key={Item.OperatorID}
                                                    value={Item.OperatorID}
                                                >
                                                    {Item.OperatorName}
                                                </MenuItem>
                                            ))}
                                        </MaterialSelect>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialSelect
                                            label="Type"
                                            name="SlabType"
                                        >
                                            {SlabTypeItems}
                                        </MaterialSelect>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialNumberField
                                            label="Commission"
                                            name="Commission"
                                            type="text"
                                            placeholder="Commission"
                                        />
                                    </Grid>

                                    <Grid item xs={1}>
                                        <ToggleButtonGroup
                                            exclusive
                                            size="small"
                                            value={CommissionCategory}
                                            onChange={(event, newValue) => {
                                                setCommissionCategory(newValue);
                                                setFieldValue(
                                                    'CommissionCategory',
                                                    newValue
                                                );
                                            }}
                                        >
                                            <ToggleButton value={'Commission'}>
                                                COM
                                            </ToggleButton>
                                            <ToggleButton value={'Surcharge'}>
                                                SUR
                                            </ToggleButton>
                                        </ToggleButtonGroup>
                                    </Grid>
                                    <Grid item xs={1}>
                                        <ToggleButtonGroup
                                            exclusive
                                            size="small"
                                            value={CommissionType}
                                            name={'CommissionType'}
                                            onChange={(event, newValue) => {
                                                setCommissionType(newValue);
                                                setFieldValue(
                                                    'CommissionType',
                                                    newValue
                                                );
                                            }}
                                        >
                                            <ToggleButton value={'Flexi'}>
                                                PER
                                            </ToggleButton>
                                            <ToggleButton value={'Fix'}>
                                                FIX
                                            </ToggleButton>
                                        </ToggleButtonGroup>
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
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
