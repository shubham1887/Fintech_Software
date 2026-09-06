import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { useLocation } from 'react-router-dom';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';

import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { SelectByServiceID as SELECT_BY_SERVICEID } from 'Components/Api/ProviderCommissionApi';
import { Insert as SLABDETAIL_ADD } from 'Components/Api/ProviderCommissionApi';
import { Delete as SLABDETAIL_DELETE } from 'Components/Api/ProviderCommissionApi';
import { Update as SALBDETAIL_UPDATE } from 'Components/Api/ProviderCommissionApi';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';

import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialCommissionField } from 'Components/Inputes/FormikMaterialCommissionInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import {
    Container,
    Grid,
    MenuItem,
    Paper,
    Button,
    CircularProgress,
    Box
} from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const ProviderCommission = () => {
    const classes = commonStyle();
    const location = useLocation();
    const ProviderID = location.state.ProviderID;
    const formRef = useRef();

    const [providerList, setProviderList] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [SlabDetail, setSlabDetail] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [IsReal, setIsReal] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [Services, setServices] = useState([]);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const commDefault = {
        Commission: 0.0,
        CommissionType: 'Flexi',
        CommissionCategory: 'Commission'
    };

    const wholeCommissionDefault = {
        General: commDefault
    };

    var [FormObj, setFormObj] = useState({
        ServiceID: 0,
        ProviderConfigurationAdminCommissionID: 0,
        ProviderID: ProviderID,
        OperatorID: 0,
        FromAmount: 0,
        ToAmount: 0,
        UserTypeID: 0,
        Admin: wholeCommissionDefault
    });

    useEffect(() => {
        setLoadingOpen(true);
        selectProvider();
        createColumns();
        selectServices();
        setLoadingOpen(false);
    }, []);

    const selectProvider = async () => {
        const response = await PROVIDER_DROP_DOWN();
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        setIsError(false);

        var response = await SLABDETAIL_ADD(requestObj);
        if (!response.isAuthorized) LOGOUT();

        setLoadingOpen(false);

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) {
            setSeverity('error');
            setIsError(true);
            return;
        }

        SelectByServiceAndSlab(
            formRef.current.values.ServiceID,
            formRef.current.values.ProviderID
        );
        resetAll(values.ProviderID);
    };

    const resetAll = (ProviderID) => {
        formRef.current.values.OperatorID = 0;
        formRef.current.values.ToAmount = 0;
        formRef.current.values.FromAmount = 0;
        formRef.current.values.Admin = wholeCommissionDefault;
    };
    const onDeleteClick = (item) => {
        var RequestObj = { ProviderConfigurationAdminCommissionID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await SLABDETAIL_DELETE(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        SelectByServiceAndSlab(
            formRef.current.values.ServiceID,
            formRef.current.values.ProviderID
        );
    };
    const selectServices = async () => {
        setLoadingOpen(true);
        const response = await SERVICE_SELECT_ALL();

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const OnServiceChange = async (name, value, setFieldValue) => {
        setLoadingOpen(true);
        setFieldValue(name, value);
        selectOperator(value);
        SelectByServiceAndSlab(value, formRef.current.values.ProviderID);
    };

    const SelectByServiceAndSlab = async (service, ProviderID) => {
        var request = {
            ServiceID: service,
            ProviderID: ProviderID
        };

        setSlabDetail([]);
        var response = await SELECT_BY_SERVICEID(JSON.stringify(request));
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) return;
        var data = JSON.parse(response.data);

        setSlabDetail(data);
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

    const UpdateClick = async (row) => {
        let AdminIsSurcharge = false;
        let ProviderConfigurationAdminCommissionID = row[0];
        let Admin = row[4];

        if (Admin.General.CommissionCategory === 'Surcharge')
            AdminIsSurcharge = true;

        let AdminCommType = Admin.General.CommissionType;
        let AdminComm = Admin.General.Commission;

        let jsonObj = {
            ProviderConfigurationAdminCommissionID: ProviderConfigurationAdminCommissionID,
            AdminComm: AdminComm,
            AdminCommType: AdminCommType,
            AdminIsSurcharge: AdminIsSurcharge
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await SALBDETAIL_UPDATE(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        SelectByServiceAndSlab(
            formRef.current.values.ServiceID,
            formRef.current.values.ProviderID
        );
    };

    const FormValidationSchema = Yup.object({
        ProviderID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Provider')
            .required(PLEASE_SELECT_VALID + 'Provider'),

        FromAmount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'from amount')
            .min(0, PLEASE_PROVIDE_VALID + 'from amount')
            .max(300000, PLEASE_PROVIDE_VALID + 'from amount'),

        ToAmount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'to amount')
            .min(0, PLEASE_PROVIDE_VALID + 'to amount')
            .max(300000, PLEASE_PROVIDE_VALID + 'to amount')
    });

    const createColumns = () => {
        var columns = [];
        const RealCommission = false;

        columns.push({
            label: '#',
            name: 'ProviderConfigurationAdminCommissionID',
            srno: true
        });

        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });
        columns.push({
            label: 'FromAmount',
            name: 'FromAmount'
        });
        columns.push({
            label: 'ToAmount',
            name: 'ToAmount'
        });
        columns.push({
            label: 'Admin',
            name: 'Admin',
            CustomCommission: true,
            RealCommission: false
        });
        columns.push({
            label: 'Update',
            name: 'ProviderConfigurationAdminCommissionID',
            save: true,
            buttonTitle: 'Update',
            showTitle: true,
            callback: (row) => {
                UpdateClick(row);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'ProviderConfigurationAdminCommissionID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });

        setMainColumns(columns);
    };

    const ServiceItems = Services.map(({ ServiceID, ServiceName }, index) => (
        <MenuItem key={index} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));

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
                                Set Admin Commission
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
                                                label="Provider"
                                                name="ProviderID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    All
                                                </MenuItem>
                                                {providerList.map(
                                                    (
                                                        {
                                                            ProviderID,
                                                            ProviderName
                                                        },
                                                        index
                                                    ) => (
                                                        <MenuItem
                                                            key={index}
                                                            value={ProviderID}
                                                        >
                                                            {ProviderName}
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
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                label="Operator"
                                                name="OperatorID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Operator
                                                </MenuItem>
                                                {Operator.map(
                                                    (
                                                        {
                                                            OperatorID,
                                                            OperatorName
                                                        },
                                                        index
                                                    ) => (
                                                        <MenuItem
                                                            key={index}
                                                            value={OperatorID}
                                                        >
                                                            {OperatorName}
                                                        </MenuItem>
                                                    )
                                                )}
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
                                                label="From Amount"
                                                name="FromAmount"
                                                type="text"
                                                length={7}
                                                placeholder="From Amount"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialNumberField
                                                label="To Amount"
                                                name="ToAmount"
                                                type="text"
                                                length={7}
                                                placeholder="To Amount"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <label>
                                                <b>Admin</b>
                                            </label>

                                            <Field
                                                component={
                                                    MaterialCommissionField
                                                }
                                                label="Admin"
                                                name="Admin"
                                                RealCommission={IsReal}
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
                                            disabled={loadingOpen}
                                        >
                                            Save
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
                                        variant="contained"
                                        color="secondary"
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
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this slab commission?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Delete(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}

            <h2 className={classes.PaperHead_h2}>Commission Details</h2>

            <ReactTable
                data={SlabDetail}
                columns={mainColumns}
                rowsPerPage={200}
            />
        </CustomContainer>
    );
};
