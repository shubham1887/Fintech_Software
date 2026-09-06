import React, { useState, useEffect, useRef } from 'react';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import Button from '@material-ui/core/Button';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { SelectServiceMoneyTransferAndUPITransfer as SERVICE_SELECT } from 'Components/Api/ServiceApi';
import { DeleteAll as DELETE_BY_All } from 'Components/Api/UserwiseDMTSequenceApi';
import { Select as SEQUENCE_USERWISE_SELECT } from 'Components/Api/UserwiseDMTSequenceApi';
import { DeleteByUserID as USERWI_SESEQUENCE_DELETE } from 'Components/Api/UserwiseDMTSequenceApi';
import { SelectRetailerAndAPIUserForDropDown as RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { SelectDMTAPI as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import { SelectOnlyRetailerAndAPI as USER_TYPE_SELECT } from 'Components/Api/UserWisePlanLimitSettingAPI';
import { Insert as SEQUENCE_USERWISE_INSERT } from 'Components/Api/UserwiseDMTSequenceApi';
import CircularProgress from '@material-ui/core/CircularProgress';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { Formik, Form, Field } from 'formik';
import HOCLoader from 'Components/Loader/HOCLoader';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';

const CustomContainer = HOCLoader(Container);

export const UserWiseDMTSequenceAdd = () => {
    const classes = commonStyle();
    const [ServiceList, setServiceList] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [downLine, setDownLine] = useState([]);
    const [userTypeList, setUserTypeList] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [Operator, setOperator] = useState([]);
    const [providerList, setProviderList] = useState([]);
    const [loading, setLoading] = useState(false);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [isDeleteAllAlertOpen, setIsDeleteAllAlertOpen] = useState(false);
    const [
        confirmDefaultAlertRequest,
        setConfirmDefaultAlertRequest
    ] = useState({});
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const formRef = useRef();

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const [formObject, setFormObject] = useState({
        User: userDefault,
        ServiceID: 0,
        ProviderID: 0,
        UserTypeID: 0,
        OperatorID: 0,
        ServiceName: 'Select Service',
        ProviderName: '',
        UserName: '',
        OperatorName: 'Select Operator'
    });

    const ServiceItems = ServiceList?.map(({ ServiceID, ServiceName }) => (
        <MenuItem key={ServiceID} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));

    const selectService = async () => {
        const response = await SERVICE_SELECT();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);

        setServiceList(data);
    };

    const selectUserTypes = async () => {
        const response = await USER_TYPE_SELECT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setResponseMessage(response.data);
        }

        var data = JSON.parse(response.data);
        setUserTypeList(data);
    };

    const callSearch = async (text, UserTypeID) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            text.length <= 0 ||
            text === userDefault.FirmName
        )
            return;

        var jsonObj = {
            FirmName: text,
            UserTypeID: UserTypeID
        };
        selectUsers(jsonObj);
    };

    const selectUsers = async (values) => {
        var jsonReq = {
            UserTypeID: values.UserTypeID,
            FirmName: values.FirmName
        };
        var request = JSON.stringify(jsonReq);
        const {
            isAuthorized,
            status,
            data
        } = await RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN(request);
        if (!isAuthorized) LOGOUT();
        if (!status) return;
        var d = JSON.parse(data);
        let objWithSelectAll = [userDefault, ...d];

        setDownLine(objWithSelectAll);
    };

    const UserTypeList = userTypeList.map((item) => (
        <MenuItem key={item.UserTypeID} value={item.UserTypeID}>
            {item.UserType}
        </MenuItem>
    ));

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

    const OnRoleChange = async (name, value, setFieldValue) => {
        var UserTypeID = value; //event.target.value;
        setFieldValue(name, value);
        selectProvider(UserTypeID);
    };

    const selectProvider = async (UserTypeID) => {
        var jsonReq = {
            UserTypeID: UserTypeID
        };
        var request = JSON.stringify(jsonReq);

        const response = await PROVIDER_DROP_DOWN(request);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data || []);
    };

    const selectsequence = async () => {
        setItemList([]);
        setLoadingOpen(true);
        const response = await SEQUENCE_USERWISE_SELECT();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        setItemList(data);
    };

    useEffect(() => {
        selectsequence();
    }, []);

    const onDeleteClick = (item) => {
        var RequestObj = {
            UserwiseDMTMappingID: item
        };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await USERWI_SESEQUENCE_DELETE(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        const { isAuthorized, status, data } = response;

        if (!isAuthorized) LOGOUT();

        setResponseMessage(data);
        setIsError(true);

        if (!status) {
            setSeverity('error');
            return;
        }
        setSeverity('success');

        selectsequence();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'OperatorID',
            srno: true
        });
        columns.push({
            label: 'User',
            name: 'UserName'
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
            label: 'Provider',
            name: 'ProviderName'
        });

        columns.push({
            label: 'Delete',
            name: 'UserwiseDMTMappingID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });

        setMainColumns(columns);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var jsonReq = {
            UserID: values.User.UserID,
            ProviderID: values.ProviderID,
            OperatorID: values.OperatorID,
            ServiceID: values.ServiceID
        };
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(jsonReq);

        setLoading(true);
        setIsError(false);
        setSubmitting(false);

        var response = await SEQUENCE_USERWISE_INSERT(requestObj);

        if (!response.isAuthorized) {
            LOGOUT();
        }

        setIsError(true);
        setSubmitting(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }
        selectsequence();
    };

    const DeleteAllClick = () => {
        setIsDeleteAllAlertOpen(true);
        setConfirmDefaultAlertRequest();
    };

    const DeleteAllUserwise = async () => {
        setIsError(false);
        setLoadingOpen(true);
        const response = await DELETE_BY_All();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        setSeverity('success');
        setResponseMessage(response.data);

        selectsequence(
            formRef.current.values.User.UserID,
            formRef.current.values.ServiceID
        );
    };

    useEffect(() => {
        setLoadingOpen(true);
        selectUserTypes();
        selectService();
        createColumns();
    }, [itemList.length]);

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
                initialValues={formObject}
                resetForm={formObject}
                onSubmit={submithandle}
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <Grid container spacing={3}>
                                <Grid item xs={12} sm={12} md={10} lg={10}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Userwise Switching
                                    </h2>
                                </Grid>
                            </Grid>
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
                                                label="Role"
                                                name="UserTypeID"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnRoleChange(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                                title="Select Role"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Role
                                                </MenuItem>
                                                {UserTypeList}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={10}
                                            sm={10}
                                            md={2}
                                            lg={2}
                                        >
                                            <Field
                                                component={AsyncAutoComplete}
                                                defaultValue={userDefault}
                                                FieldName="User"
                                                name="User"
                                                SubFieldName="UserID"
                                                options={downLine}
                                                getOptionLabel={(option) =>
                                                    option.FirmName
                                                }
                                                textFieldProps={{
                                                    label: 'Select User'
                                                }}
                                                onSearchCallback={(text) => {
                                                    callSearch(
                                                        text,
                                                        values.UserTypeID
                                                    );
                                                }}
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    setUserDefault({
                                                        UserID: value.UserID,
                                                        FirmName: value.FirmName
                                                    });
                                                }}
                                            />
                                        </Grid>
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
                                            md={1}
                                            lg={1}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Add"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loading}
                                            >
                                                Add
                                            </PrimaryButton>
                                            {loading && (
                                                <CircularProgress
                                                    size={20}
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
                                                variant="contained"
                                                color="primary"
                                                title="Delete all"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                style={{
                                                    marginTop: '11px'
                                                }}
                                                onClick={() => DeleteAllClick()}
                                            >
                                                Delete all
                                            </SecondaryButton>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            <h2 className={classes.PaperHead_h2}>UserwiseDMT Sequence</h2>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={50}
                headerToolbar={false}
            />

            {isAlertOpen && (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete userwise switching."
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Delete(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            )}

            {isDeleteAllAlertOpen && (
                <ConfirmAlert
                    isOpen={isDeleteAllAlertOpen}
                    title="Delete All Confirmation"
                    message="Are you sure to delete this all users."
                    request={confirmDefaultAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsDeleteAllAlertOpen(false);
                        DeleteAllUserwise(request);
                    }}
                    negativeButtonClick={() => {
                        setIsDeleteAllAlertOpen(false);
                    }}
                />
            )}
        </CustomContainer>
    );
};
