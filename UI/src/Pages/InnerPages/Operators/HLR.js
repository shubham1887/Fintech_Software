import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { LOGOUT } from 'ConstComponents';
import { SelectByRechargeServices as SERVICE_SELECT_RECHARGE } from 'Components/Api/ServiceApi';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { Select as STATE_SELECT } from 'Components/Api/StatesApi';

import { Add as OPERATOR_STATE_ADD } from 'Components/Api/OperatorStateMaapingApi';
import { Select as OPERATOR_STATE_SELECT } from 'Components/Api/OperatorStateMaapingApi';
import { Activate as OPERATOR_STATE_ACTIVATE } from 'Components/Api/OperatorStateMaapingApi';
import { Delete as OPERATOR_STATE_DELETE } from 'Components/Api/OperatorStateMaapingApi';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';

import { Container, Grid, Paper, Button, MenuItem } from '@material-ui/core';

import { useHistory } from 'react-router';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import Box from '@material-ui/core/Box';
import {
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID,
    PREPAID_RECHARGE_CODE
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const HLR = () => {
    const history = useHistory();
    const [Services, setServices] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [State, setState] = useState([]);
    const [OperatorState, setOperatorState] = useState([]);
    const [ServiceID, setServiceID] = useState(PREPAID_RECHARGE_CODE);
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [formObj, SetFormObj] = useState({
        ServiceID: 0,
        OperatorID: 0,
        StateID: 0,
        HLR: ''
    });
    useEffect(() => {
        setLoadingOpen(true);
        selectServices();
        selectState();
        select();
        createColumns();
    }, []);

    const onDeleteClick = (item) => {
        var RequestObj = { OperatorStateMappingID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);

        const response = await OPERATOR_STATE_DELETE(jsonReq);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        select();
    };
    const Activate = async (item, value) => {
        var jsonObj = {
            OperatorStateMappingID: item,
            IsActive: value
        };

        var request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);
        const response = await OPERATOR_STATE_ACTIVATE(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);
        if (!response.status) setSeverity('error');

        select();
    };

    const selectServices = async () => {
        const response = await SERVICE_SELECT_RECHARGE();

        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);

        setServices(data);
        setLoadingOpen(false);
    };

    const OnOperatorChange = async (name, value, setFieldValue) => {
        setFieldValue(name, value);
        SetFormObj({ ...formObj, [name]: value });
        setServiceID(value);

        selectOperator(value);
    };

    const selectOperator = async (ServiceID) => {
        if (
            ServiceID === null ||
            ServiceID === undefined ||
            ServiceID === 0 ||
            ServiceID === '0'
        ) {
            return;
        }

        var requestObj = {
            ServiceID: ServiceID
        };
        var request = JSON.stringify(requestObj);

        const response = await OPERATOR_SELECT_BY_SERVICEID(request);

        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setIsError(true);
        }
        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);
            setOperator(data);
        }
    };

    const selectState = async () => {
        const response = await STATE_SELECT();

        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);

        setState(data);
        setLoadingOpen(false);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            return;
        }
        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);
        setIsError(false);
        var response = await OPERATOR_STATE_ADD(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        setResponseMessage(response.data);
        setIsError(true);
        setSubmitting(false);

        setSeverity('success');
        if (!response.status) {
            setSeverity('error');
            return;
        }
        select();
        resetForm(formObj);
    };
    const formValidationSchema = Yup.object({
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID, +'service')
            .required(PLEASE_SELECT_VALID, +'service'),
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID, +'operator')
            .required(PLEASE_SELECT_VALID, +'operator'),
        StateID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID, +'state')
            .required(PLEASE_SELECT_VALID, +'state'),
        HLR: Yup.string().required(PLEASE_PROVIDE_VALID + 'HLR')
    });

    const select = async () => {
        const response = await OPERATOR_STATE_SELECT();

        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);

        setOperatorState(data);

        setLoadingOpen(false);
    };
    const Edit = async (request) => {
        history.push('/Operators/HLR/Edit/' + request);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'OperatorStateMappingID',
            srno: true
        });
        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });
        columns.push({
            label: 'State',
            name: 'StateName'
        });
        columns.push({
            label: 'HLR',
            name: 'HLR',
            textwrapwidth: 180,
            textwrap: true
        });

        columns.push({
            label: 'Active',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                Activate(id, value);
            }
        });
        columns.push({
            label: 'Edit',
            name: 'OperatorStateMappingID',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'OperatorStateMappingID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });
        setMainColumns(columns);
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
                initialValues={formObj}
                validationSchema={formValidationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
            >
                {({ values, setFieldValue, handleChange }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Operator State
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
                                                label="Select Service"
                                                name="ServiceID"
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

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
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
                                                        {item.OperatorName}
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
                                                label="Select State"
                                                name="StateID"
                                                title="State"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select State
                                                </MenuItem>
                                                {State.map((item) => (
                                                    <MenuItem
                                                        key={item.StateID}
                                                        value={item.StateID}
                                                    >
                                                        {item.Name}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialTextField
                                                label="HLR"
                                                name="HLR"
                                                type="text"
                                                placeholder="HLR"
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
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <h3 className={classes.Provider_Format}>
                                    Note : Use comma separated for HLR Eg. 9545,
                                    9823
                                </h3>
                            </Grid>
                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div
                                        className={classes.SubmitWrapper}
                                    ></div>
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
                    message="Are you sure to delete this HLR."
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

            <h2 className={classes.PaperHead_h2}>Operator State List</h2>
            <ReactTable data={OperatorState} columns={mainColumns} />
        </CustomContainer>
    );
};
