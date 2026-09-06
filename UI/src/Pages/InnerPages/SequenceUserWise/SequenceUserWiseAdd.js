import React, { useState, useEffect, useRef } from 'react';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { useHistory } from 'react-router';
import { LOGOUT } from 'ConstComponents';
import Box from '@material-ui/core/Box';
import Button from '@material-ui/core/Button';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import { Select as SEQUENCE_USERWISE_SELECT } from 'Components/Api/OperatorSequenceUserWiseApi';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { DeleteByUserID as DELETE_BY_USERID } from 'Components/Api/OperatorSequenceUserWiseApi';
import { DeleteAll as DELETE_BY_All } from 'Components/Api/OperatorSequenceUserWiseApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { CustomModal } from 'Components/Modal/CustomModal';
import { Edit } from './Edit';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { Formik, Form, Field } from 'formik';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';

const CustomContainer = HOCLoader(Container);

export const SequenceUserWiseAdd = () => {
    const history = useHistory();
    const classes = commonStyle();
    const [Services, setServices] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [UsersList, setUsers] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [operatorID, setOperatorID] = useState(0);
    const [selectedUserID, setselectedUserID] = useState(0);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [isDefaultAlertOpen, setIsDefaultAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [
        confirmDefaultAlertRequest,
        setConfirmDefaultAlertRequest
    ] = useState({});

    const formRef = useRef();

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const formObject = {
        User: userDefault,
        ServiceID: 0,
        ServiceName: 'Select Service'
    };

    const SetDefaultUserwise = async (request) => {
        var requestjson = JSON.stringify(request);
        setIsError(false);
        setLoadingOpen(true);
        const response = await DELETE_BY_USERID(requestjson);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        setConfirmDefaultAlertRequest(request);
        setSeverity('success');
        setResponseMessage(response.data);

        if (response.data === 'Default set successfully') {
            selectsequence(
                formRef.current.values.User.UserID,
                formRef.current.values.ServiceID
            );
        }
    };

    const ResetAllUserwise = async () => {
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
    const ServiceItems = Services.map(({ ServiceID, ServiceName }, index) => (
        <MenuItem key={index} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));

    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);

        setServices(data);
    };

    const selectUsers = async () => {
        const response = await USER_SELECT_FOR_DROPDOWN();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);

        let objWithSelectAll = [userDefault, ...data];
        setUsers(objWithSelectAll);
    };

    function handleChange(value) {
        setselectedUserID(value);
    }
    function OnServiceChange(name, value, setFieldValue) {
        selectsequence(selectedUserID, value);
    }

    const selectsequence = async (selectedUserID, ServiceID) => {
        setItemList([]);
        if (ServiceID <= 0) return;

        var requestObj = {
            UserID: selectedUserID,
            ServiceID: ServiceID
        };

        setLoadingOpen(true);
        var request = JSON.stringify(requestObj);
        const response = await SEQUENCE_USERWISE_SELECT(request);
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

    const EditClick = (value) => {
        setIsModalOpen(true);
        setOperatorID(value);
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
            name: 'ProviderList',
            array: true,
            keyName: 'ProviderName'
        });
        columns.push({
            label: 'Edit',
            name: 'OperatorID',
            edit: true,
            callback: (value) => {
                EditClick(value);
            }
        });

        setMainColumns(columns);
    };
    const callSearch = async (text) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            text.length <= 0 ||
            text === userDefault.FirmName
        )
            return;

        var jsonObj = {
            FirmName: text
        };
        selectUsers(jsonObj);
    };
    useEffect(() => {
        setLoadingOpen(true);
        selectUsers();
        selectServices();
        createColumns();
    }, [itemList.length]);

    const ResetAllClick = () => {
        setIsAlertOpen(true);
        setConfirmAlertRequest();
    };

    const SetDefaultAllClick = (item) => {
        var RequestObj = { UserID: item };
        setIsDefaultAlertOpen(true);
        setConfirmDefaultAlertRequest(RequestObj);
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
                initialValues={formObject}
                resetForm={formObject}
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
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <Box className={classes.flexReverse}>
                                        <Button
                                            variant="contained"
                                            color="primary"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            onClick={() => {
                                                history.push(
                                                    '/UserwiseSequenceIndex'
                                                );
                                            }}
                                        >
                                            User-wise details
                                        </Button>
                                    </Box>
                                </Grid>
                            </Grid>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <Field
                                                component={AsyncAutoComplete}
                                                defaultValue={userDefault}
                                                FieldName="User"
                                                name="User"
                                                SubFieldName="UserID"
                                                options={UsersList}
                                                getOptionLabel={(option) =>
                                                    option.FirmName
                                                }
                                                textFieldProps={{
                                                    label: 'Select User'
                                                }}
                                                onSearchCallback={(text) => {
                                                    callSearch(text);
                                                }}
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    if (value === undefined) {
                                                        return;
                                                    }

                                                    if (value === null) {
                                                        return;
                                                    }
                                                    setUserDefault({
                                                        UserID: value.UserID,
                                                        FirmName: value.FirmName
                                                    });

                                                    handleChange(value.UserID);
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
                                            <PrimaryButton
                                                variant="contained"
                                                color="primary"
                                                title="Reset all Userwise"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                style={{
                                                    marginTop: '11px'
                                                }}
                                                onClick={() => ResetAllClick()}
                                            >
                                                Reset all Userwise
                                            </PrimaryButton>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <SecondaryButton
                                                variant="contained"
                                                color="primary"
                                                title="Set Default Userwise"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={() =>
                                                    SetDefaultAllClick(
                                                        selectedUserID
                                                    )
                                                }
                                                style={{
                                                    marginTop: '11px'
                                                }}
                                            >
                                                Set Default Userwise
                                            </SecondaryButton>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            <h2 className={classes.PaperHead_h2}>Userwise Sequence</h2>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={50}
                headerToolbar={false}
            />
            {isModalOpen && (
                <CustomModal
                    isOpen={isModalOpen}
                    title="User-wise API Seqence"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsModalOpen(value);
                        selectsequence(selectedUserID);
                    }}
                >
                    <Edit
                        OperatorID={operatorID}
                        selectUserID={selectedUserID}
                        setIsModalOpen={setIsModalOpen}
                        onDismissCallback={(value) => {
                            setIsModalOpen(false);
                            selectsequence(
                                selectedUserID,
                                formRef.current.values.ServiceID
                            );
                        }}
                    />
                </CustomModal>
            )}

            {isAlertOpen && (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Reset Confirmation"
                    message="Are you sure to reset this all users."
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        ResetAllUserwise(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            )}

            {isDefaultAlertOpen && (
                <ConfirmAlert
                    isOpen={isDefaultAlertOpen}
                    title="SetDefault Confirmation"
                    message="Are you sure to set default this all users."
                    request={confirmDefaultAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsDefaultAlertOpen(false);
                        SetDefaultUserwise(request);
                    }}
                    negativeButtonClick={() => {
                        setIsDefaultAlertOpen(false);
                    }}
                />
            )}
        </CustomContainer>
    );
};
