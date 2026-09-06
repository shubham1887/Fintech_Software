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
import {
    Insert,
    Select,
    Delete as DeleteSequence,
    SelectByID,
    Update
} from 'Components/Api/RangeWiseSequenceAPI';
import { MONEY_TRANSFER_CODE } from 'Components/Helper/Messages';
import { SelectRangeWiseServices as SERVICE_SELECT_RANGRWISE } from 'Components/Api/ServiceApi';

import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { SelectDMTAsPayoutAPI as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const RangeWiseSequence = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [providerList, setProviderList] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const [Services, setServices] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [rangeWiseSequenceID, setRangeWiseSequenceID] = useState(0);

    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const [isUpdate, setIsUpdate] = useState(false);
    var [FormObj, setFormObj] = useState({
        ServiceID: MONEY_TRANSFER_CODE,
        OperatorID: 0,
        ProviderID: 0,
        FromAmount: 0,
        ToAmount: 0,
        isUpdate: false,
        RangeWiseSequenceID: rangeWiseSequenceID
    });

    const ServiceItems = Services.map(({ ServiceID, ServiceName }, index) => (
        <MenuItem key={index} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));

    const selectServices = async () => {
        const response = await SERVICE_SELECT_RANGRWISE();
        if (!response.isAuthorized) LOGOUT();
        var data = JSON.parse(response.data);
        setServices(data);
        selectOperator(MONEY_TRANSFER_CODE);
    };
    const OnServiceChange = async (name, value, setFieldValue) => {
        var ServiceID = value; //event.target.value;

        setFieldValue(name, value);

        selectOperator(ServiceID);
    };

    const selectProvider = async () => {
        const response = await PROVIDER_DROP_DOWN();
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
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

    const SelectData = async () => {
        setItemList([]);
        setLoadingOpen(true);
        var response = await Select(JSON.stringify());

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
            name: 'RangeWiseSequenceID',
            srno: true
        });

        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });

        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'Operator',
            name: 'Operator'
        });
        columns.push({
            label: 'FromAmount',
            name: 'FromAmount',
            textwrapwidth: 70,
            numeric: true
        });
        columns.push({
            label: 'ToAmount',
            name: 'ToAmount',
            textwrapwidth: 70,
            numeric: true
        });

        columns.push({
            label: 'Edit',
            name: 'RangeWiseSequenceID',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });

        columns.push({
            label: 'Delete',
            name: 'RangeWiseSequenceID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });
        setMainColumns(columns);
    };
    const Edit = async (ServiceID) => {
        setIsUpdate(true);

        selectByID(ServiceID);
    };

    const selectByID = async (RangeWiseSequenceID) => {
        let JSONObj = {
            RangeWiseSequenceID: RangeWiseSequenceID
        };
        setRangeWiseSequenceID(RangeWiseSequenceID);
        var request = JSON.stringify(JSONObj);
        setLoadingOpen(true);
        const response = await SelectByID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setIsUpdate(true);
        selectOperator(data.ServiceID);

        setFormObj({
            ServiceID: data.ServiceID,
            OperatorID: data.OperatorID,
            ProviderID: data.ProviderID,
            FromAmount: data.FromAmount,
            ToAmount: data.ToAmount
        });
    };

    const onDeleteClick = (RangeWiseSequenceID) => {
        var RequestObj = {
            RangeWiseSequenceID: RangeWiseSequenceID
        };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };
    const Delete = async (request) => {
        var requestObj = JSON.stringify(request);

        setIsError(false);

        setLoadingOpen(true);
        const response = await DeleteSequence(requestObj);

        if (!response.isAuthorized) {
            LOGOUT();
            return;
        }

        setResponseMessage(response.data);
        setSeverity(response.status ? 'success' : 'error');
        setLoadingOpen(false);
        setIsError(true);

        if (response.status) {
            setTimeout(() => {
                selectProvider();
            }, MESSAGE_CLOSE_AFTER);
        }

        SelectData();
    };
    const DeleteConfirmationModal = isAlertOpen ? (
        <ConfirmAlert
            isOpen={isAlertOpen}
            title="Delete Confirmation"
            message="Are you sure to delete this provider."
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpen(false);
                Delete(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpen(false);
            }}
        />
    ) : null;

    useEffect(() => {
        selectServices();
        createColumns();
        selectProvider();
        SelectData();
    }, [isUpdate]);

    const FormValidationSchema = Yup.object({
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'service')
            .required(PLEASE_SELECT_VALID + 'service'),
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator'),

        FromAmount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'Minimum 1 amount')
            .required(PLEASE_PROVIDE_VALID + 'from amount'),

        ToAmount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'Minimum 1 amount')
            .required(PLEASE_PROVIDE_VALID + 'to amount')
    });
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }

        let request = {
            ProviderID: values.ProviderID,
            OperatorID: values.OperatorID,
            ServiceID: values.ServiceID,
            FromAmount: values.FromAmount,
            ToAmount: values.ToAmount,
            RangeWiseSequenceID: rangeWiseSequenceID
        };

        var requestObj = JSON.stringify(request);
        var requestObjAdd = JSON.stringify(values);

        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);

        var response = isUpdate
            ? await Update(requestObj)
            : await Insert(requestObjAdd);

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
            ProviderID: 0,
            FromAmount: 0,
            ToAmount: 0
        });

        SelectData();
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
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                DMT Range wise sequence
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
                                                ) => {}}
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
                                                label="Provider"
                                                name="ProviderID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Provider
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
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialNumberField
                                                label="FromAmount"
                                                name="FromAmount"
                                                type="text"
                                                length={5}
                                                placeholder="FromAmount"
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
                                                label="ToAmount"
                                                name="ToAmount"
                                                type="text"
                                                length={5}
                                                placeholder="ToAmount"
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
                                                {isUpdate ? 'Update' : 'Add'}
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
            <h2 className={classes.PaperHead_h2}>Sequence List</h2>
            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={50}
                headerToolbar={false}
            />

            {DeleteConfirmationModal}
        </CustomContainer>
    );
};
