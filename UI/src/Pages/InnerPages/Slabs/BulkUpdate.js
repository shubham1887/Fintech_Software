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
import { ToggleButton, ToggleButtonGroup } from '@material-ui/lab';

import { LOGOUT } from 'ConstComponents';

import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { BulkUpdate as BULK_UPDATE_COMMISSION } from 'Components/Api/SlabDetailApi';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { SelectByRechargeServices as SERVICE_SELECT_RECHARGE } from 'Components/Api/ServiceApi';
import { SelectWithoutAPIUserType as USERTYPE_SELECT_WITHOUT_APIUSERTYPE } from 'Components/Api/UserTypeApi';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SUPPORT_USER } from 'Components/Helper/UsertypeCode';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const BulkUpdate = ({ SlabID, onDismissCallback }) => {
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [Services, setServices] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [userTypeList, setUserTypeList] = useState([]);

    const [CommissionType, setCommissionType] = useState('Fix');

    const [CommissionCategory, setCommissionCategory] = useState('Commission');
    const SlabTypes = [{ Type: 'General' }, { Type: 'Real' }];

    const selectUserTypes = async () => {
        const {
            isAuthorized,
            status,
            data
        } = await USERTYPE_SELECT_WITHOUT_APIUSERTYPE();

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setSeverity('error');
            setResponseMessage(data);
            setIsError(true);
            return;
        }

        var usertypedata = JSON.parse(data);
        setUserTypeList(usertypedata);
    };
    const [formObject] = useState({
        SlabID: SlabID,
        ServiceID: 0,
        OperatorID: 0,
        UserTypeID: 0,
        SlabType: 'General',
        FromAmount: 0,
        ToAmount: 0,
        CommissionType: CommissionType,
        CommissionCategory: CommissionCategory
    });

    const FormValidationSchema = Yup.object({
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'serviceID')
            .required(PLEASE_SELECT_VALID + 'serviceID')

        // OperatorID: Yup.string()
        //     .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
        //     .required(PLEASE_SELECT_VALID + 'operator'),
    });

    const selectServices = async () => {
        const {
            isAuthorized,

            data
        } = await SERVICE_SELECT_RECHARGE();
        if (!isAuthorized) LOGOUT();

        setServices(JSON.parse(data));
    };

    const OnServiceChange = async (name, value, setFieldValue) => {
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

        const {
            isAuthorized,
            status,
            data
        } = await OPERATOR_SELECT_BY_SERVICEID(request);

        if (!isAuthorized) LOGOUT();

        if (!status) setIsError(true);

        if (data !== 'Requested operators not found') {
            setOperator(JSON.parse(data));
        }
    };
    useEffect(() => {
        selectServices();
        selectUserTypes();
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
    const ServiceItems = Services.map(({ ServiceID, ServiceName }, index) => (
        <MenuItem key={index} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));
    const OperatorItems = Operator.map(
        ({ OperatorID, OperatorName }, index) => (
            <MenuItem key={index} value={OperatorID}>
                {OperatorName}
            </MenuItem>
        )
    );
    const SlabTypeItems = SlabTypes.map(({ Type }, index) => (
        <MenuItem key={index} value={Type}>
            {Type}
        </MenuItem>
    ));
    const UserTypeItems = userTypeList.map(({ UserTypeID, UserType }, index) =>
        UserTypeID !== SUPPORT_USER ? (
            <MenuItem key={index} value={UserTypeID}>
                {UserType}
            </MenuItem>
        ) : null
    );

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
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialSelect
                                            label="Service"
                                            name="ServiceID"
                                            onCustomCallback={(name, value) => {
                                                OnServiceChange(
                                                    name,
                                                    value,
                                                    setFieldValue
                                                );
                                            }}
                                            title="Service"
                                        >
                                            <MenuItem key="0" value={0}>
                                                Select Service
                                            </MenuItem>
                                            {ServiceItems}
                                        </MaterialSelect>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialSelect
                                            label="Operator"
                                            name="OperatorID"
                                        >
                                            <MenuItem key="0" value={0}>
                                                Select Operator
                                            </MenuItem>

                                            {OperatorItems}
                                        </MaterialSelect>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialSelect
                                            label="Role"
                                            name="UserTypeID"
                                            title="Role"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Role
                                            </MenuItem>
                                            {UserTypeItems}
                                        </MaterialSelect>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialNumberField
                                            label="Commission"
                                            name="Commission"
                                            type="text"
                                            length={7}
                                            placeholder="Commission Amount"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={1} lg={1}>
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
                                    <Grid item xs={12} sm={12} md={1} lg={1}>
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
