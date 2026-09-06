import React, { useState, useEffect, useRef } from 'react';
import {
    Container,
    Grid,
    MenuItem,
    Paper,
    CircularProgress
} from '@material-ui/core';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { SelectForAdjustCommission as SELECT_ADJUST_COMMISSION_OPERATOR } from 'Components/Api/SlabDetailApi';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { UpdateCommission as UPDATE_COMMISSION } from 'Components/Api/SlabDetailApi';

import ReactTable from 'Components/ReactTable/ReactTable';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const AdjustCommission = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const [Services, setServices] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [selectedoperatorID, setselectedoperatorID] = useState(0);

    var [FormObj, setFormObj] = useState({
        ServiceID: 0,
        OperatorID: 0,
        Commission: 0,
        Adjustment: 0
    });

    const ServiceItems = Services.map(({ ServiceID, ServiceName }, index) => (
        <MenuItem key={index} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));

    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();

        if (!response.isAuthorized) LOGOUT();
        var data = JSON.parse(response.data);
        setServices(data);
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

        const response = await OPERATOR_SELECT_BY_SERVICEID(request);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) setIsError(true);

        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);
            setOperator(data);
        }
    };

    const OnOperatorChange = async (name, value, setFieldValue) => {
        SelectData(value);
    };
    const SelectData = async (OperatorID) => {
        var request = {
            OperatorID: OperatorID
        };
        setselectedoperatorID(OperatorID);
        setItemList([]);
        setLoadingOpen(true);
        var response = await SELECT_ADJUST_COMMISSION_OPERATOR(
            JSON.stringify(request)
        );

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) return;
        var data = JSON.parse(response.data);
        setItemList(data);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });

        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });

        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });

        columns.push({
            label: 'Retailer / APIuser',
            name: 'Retailer',
            CustomCommission: true
        });
        setMainColumns(columns);
    };
    useEffect(() => {
        selectServices();
        createColumns();
    }, []);

    const FormValidationSchema = Yup.object({
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'service')
            .required(PLEASE_SELECT_VALID + 'service'),
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator'),
        Adjustment: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'adjustment')
            .required(PLEASE_SELECT_VALID + 'adjustment'),
        Commission: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'Minimum 0.1 recharge amount')
            .required(PLEASE_PROVIDE_VALID + 'minimum recharge amount')
    });
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await UPDATE_COMMISSION(requestObj);
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

        resetForm(FormObj);
        setFormObj({
            ServiceID: 0,
            OperatorID: 0,
            Commission: 0,
            Adjustment: 0
        });

        setTimeout(() => {
            SelectData(selectedoperatorID);
        });
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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Commission Adjustment
                            </h2>

                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Service"
                                                name="ServiceID"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnServiceChange(
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
                                                {ServiceItems}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Operator"
                                                name="OperatorID"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnOperatorChange(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
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
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Adjustment"
                                                name="Adjustment"
                                                title="Adjustment"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Adjustment
                                                </MenuItem>

                                                <MenuItem
                                                    key="Increase "
                                                    value="Increase"
                                                >
                                                    Increase
                                                </MenuItem>
                                                <MenuItem
                                                    key="Decrease"
                                                    value="Decrease"
                                                >
                                                    Decrease
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialNumberField
                                                label="Commission"
                                                name="Commission"
                                                type="text"
                                                length={5}
                                                placeholder="Commission"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Add"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                Add
                                            </PrimaryButton>
                                            {loadingOpen && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <SecondaryButton
                                                type="reset"
                                                title="Reset"
                                                className={
                                                    classes.CustomDangerButtonStyle
                                                }
                                            >
                                                Reset
                                            </SecondaryButton>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={50}
                headerToolbar={false}
            />
        </CustomContainer>
    );
};
