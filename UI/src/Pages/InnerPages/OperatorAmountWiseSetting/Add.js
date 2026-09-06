import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Paper,
    CircularProgress,
    MenuItem
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import {
    Insert as INSERT_OPERATOR,
    Select as SELECT_ALL,
    Update as UPDATE_OPERATOR,
    Delete as DELETE_OPERATOR
} from 'Components/Api/OperatorAmountWiseAPI';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { SelectByPrimeServices as SERVICE_SELECT_PRIME } from 'Components/Api/ServiceApi';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const AddOperatorAmount = () => {
    const classes = commonStyle();
    const [Services, setServices] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [operators, setOperators] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        selectServices();
        selectData();
    }, []);

    const [formObject, setFormObject] = useState({
        ServiceID: '',
        OperatorID: '',
        Amount: 0,
        SerialCount: 0
    });

    const FormValidationSchema = Yup.object({
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'service')
            .required(PLEASE_SELECT_VALID + 'service'),
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator'),

        Amount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'amount')
            .min(10, PLEASE_PROVIDE_VALID + 'amount')
            .max(2000, PLEASE_PROVIDE_VALID + 'amount')
    });

    const selectServices = async () => {
        const response = await SERVICE_SELECT_PRIME();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setLoadingOpen(false);
        setServices(data);
    };
    const operatorsList = operators.map(({ OperatorID, OperatorName }) => (
        <MenuItem key={OperatorName} value={OperatorID}>
            {OperatorName}
        </MenuItem>
    ));
    const OnOperatorChange = async (name, value) => {
        var ServiceID = value; //event.target.value;
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
        let request = JSON.stringify(requestObj);

        const {
            isAuthorized,
            status,
            data
        } = await OPERATOR_SELECT_BY_SERVICEID(request);

        if (!isAuthorized) LOGOUT();

        if (!status) setIsError(true);

        if (data !== 'Requested operators not found') {
            setOperators(JSON.parse(data));
        }
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);
        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await INSERT_OPERATOR(requestObj);

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
        resetForm(formObject);
        selectData();
    };
    const selectData = async () => {
        const response = await SELECT_ALL();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);
        setItemList(data);
    };
    const UpdateClick = async (row) => {
        setIsError(false);
        let jsonObj = {
            OperatorAmountWiseSettingID: row[0],
            OperatorID: row[1],
            Amount: row[3],
            SerialCount: row[4]
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await UPDATE_OPERATOR(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        selectData();
    };
    const onDeleteClick = (item) => {
        var RequestObj = { OperatorAmountWiseSettingID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };
    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        const response = await DELETE_OPERATOR(jsonReq);
        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        setSeverity('success');
        setResponseMessage(response.data);
        selectData();
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'OperatorAmountWiseSettingID',
            srno: true
        });
        columns.push({
            label: 'OperatorID',
            name: 'OperatorID',
            hide: true
        });
        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });
        columns.push({
            label: 'Amount',
            name: 'Amount'
        });

        columns.push({
            label: 'Serial count',
            name: 'SerialCount',
            editable: true
        });
        columns.push({
            label: 'Update',
            name: 'Update',
            save: true,
            buttonTitle: 'Update',
            callback: (row) => {
                UpdateClick(row);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'OperatorAmountWiseSettingID',
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
                enableReinitialize={true}
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {() => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Operator Amount Wise Setting
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
                                                    OnOperatorChange(
                                                        name,
                                                        value
                                                    );
                                                }}
                                                title="Select Service"
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
                                                label="Operator"
                                                name="OperatorID"
                                                title="Operator"
                                            >
                                                <MenuItem key="0" value={0}>
                                                    Select Operator
                                                </MenuItem>
                                                {operatorsList}
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
                                                label="Amount"
                                                name="Amount"
                                                type="text"
                                                placeholder="Amount"
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
                                                label="SerialCount"
                                                name="SerialCount"
                                                type="text"
                                                placeholder="SerialCount"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={1}
                                            lg={1}
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
                                            md={1}
                                            lg={1}
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
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this Operator"
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
            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
