import React, { useEffect, useState } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { SelectForCallBackUrl as SERVICE_SELECT } from 'Components/Api/ServiceApi';
import { useHistory } from 'react-router';
import {
    Container,
    Grid,
    Button,
    Box,
    CircularProgress,
    Paper,
    MenuItem
} from '@material-ui/core';
import {
    Insert as INSERT_CALLBACK,
    Select as SELECT_CALLBACK,
    Delete as DELETE_CALLBACK
} from 'Components/Api/CallbackApi';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    API_USER_CODE
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import HOCLoader from 'Components/Loader/HOCLoader';
import ReactTable from 'Components/ReactTable/ReactTable';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { SelectRetailerAndAPIUserForDropDown as USER_SELECT_LIST } from 'Components/Api/UsersApi';
import { ADMIN } from 'Components/Helper/UsertypeCode';
import { SelectByID as SELECT_BY_ID } from 'Components/Api/CallbackApi';
const CustomContainer = HOCLoader(Container);

export const CallbackAdd = () => {
    const history = useHistory();
    const classes = commonStyle();
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [Services, setServices] = useState([]);
    const [IsUpdate, setIsUpdate] = useState(false);
    const [UsersList, setUsers] = useState([]);
    const [isAdminRole, setIsAdminRole] = useState(false);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const [formObj, setFormObject] = useState({
        CallbackURL: '',
        ServiceID: 0,
        CallbackMasterID: 0,
        User: userDefault,
        UserID: 0,
        CallbackMasterID: 0
    });

    const CheckUserRole = () => {
        if (ADMIN === parseInt(currentUsertypeid)) setIsAdminRole(true);
    };

    var currentUsertypeid = localStorage.getItem('UserTypeID');

    const formValidationSchema = Yup.object({
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'service')
            .required(PLEASE_SELECT_VALID + 'service'),
        CallbackURL: Yup.string().required(
            PLEASE_PROVIDE_VALID,
            +'callback url'
        )
    });
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

    const selectUsers = async () => {
        var request = {
            UserTypeID: API_USER_CODE
        };
        var json = JSON.stringify(request);

        const response = await USER_SELECT_LIST(json);
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        let objWithSelectAll = [userDefault, ...data];

        setUsers(objWithSelectAll);
    };

    useEffect(() => {
        CheckUserRole();
        createColumns();
        selectData();
        selectServices();
    }, [isAdminRole]);
    const Edit = async (CallbackMasterID) => {
        setIsUpdate(true);

        let JSONObj = {
            CallbackMasterID: CallbackMasterID
        };
        setLoadingOpen(true);

        var request = JSON.stringify(JSONObj);

        const response = await SELECT_BY_ID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        var data = JSON.parse(response.data);

        setUserDefault({
            UserID: data.UserID,
            FirmName: data.FirmName
        });
        setFormObject({
            User: {
                UserID: data.UserID,
                FirmName: data.FirmName
            },
            CallbackURL: data.CallbackURL,
            ServiceID: data.ServiceID,
            CallbackMasterID: data.CallbackMasterID,
            ServiceName: data.ServiceName,
            CallbackMasterID: CallbackMasterID
        });
    };
    const selectServices = async () => {
        const response = await SERVICE_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const selectData = async () => {
        setLoadingOpen(true);
        CheckUserRole();
        createColumns();
        setIsError(false);

        var response = await SELECT_CALLBACK('');

        setLoadingOpen(false);
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

        setItemList(data);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'CallbackMasterID',
            srno: true
        });

        if (isAdminRole === true) {
            columns.push({
                label: 'FirmName',
                name: 'FirmName'
            });
        }
        columns.push({
            label: 'Date',
            name: 'Date'
        });

        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });

        columns.push({
            label: 'Url',
            name: 'CallbackURL'
        });
        columns.push({
            label: 'Edit',
            name: 'CallbackMasterID',
            buttonTitle: 'Edit',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'CallbackMasterID',
            delete: true,
            callback: (value) => {
                {
                    onDeleteClick(value);
                }
            }
        });

        setMainColumns(columns);
    };

    const onDeleteClick = (item) => {
        var RequestObj = { CallbackMasterID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (requestObj) => {
        setLoadingOpen(true);
        var reqObj = JSON.stringify(requestObj);
        var response = await DELETE_CALLBACK(reqObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }

        window.location.reload();
    };
    const submitHandle = async (
        values,
        { setSubmitting, setErrors, setStatus, resetForm }
    ) => {
        setLoadingOpen(true);

        values.UserID = values.User.UserID;

        var requestObj = JSON.stringify(values);

        setIsError(false);

        var response = await INSERT_CALLBACK(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }
        resetForm(formObj);
        selectData();
        window.location.reload();
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
                initialValues={formObj}
                validationSchema={formValidationSchema}
                onSubmit={submitHandle}
                resetForm={formObj}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Callback add
                            </h2>
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={1}>
                                        {isAdminRole ? (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={4}
                                                lg={4}
                                            >
                                                <Field
                                                    component={
                                                        AsyncAutoComplete
                                                    }
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
                                                    onSearchCallback={(
                                                        text
                                                    ) => {
                                                        callSearch(text);
                                                    }}
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        setUserDefault({
                                                            UserID:
                                                                value.UserID,
                                                            FirmName:
                                                                value.FirmName
                                                        });
                                                    }}
                                                />
                                            </Grid>
                                        ) : null}

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialSelect
                                                label="Service"
                                                name="ServiceID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    select Service
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
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialTextField
                                                label="Callback url"
                                                name="CallbackURL"
                                                type="text"
                                                placeholder="Callback url"
                                                title="Callback url"
                                            />
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <Grid className={classes.footerGrid}>
                                    <Box className={classes.flexReverse}>
                                        <div className={classes.SubmitWrapper}>
                                            <Button
                                                type="submit"
                                                title={
                                                    IsUpdate ? 'Update' : 'Add'
                                                }
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                {IsUpdate ? 'Update' : 'Add'}
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
                                            title="Reset"
                                            className={
                                                classes.CustomDangerButtonStyle
                                            }
                                        >
                                            Reset
                                        </Button>
                                    </Box>
                                </Grid>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this callback."
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
            <ReactTable
                data={itemList}
                rowsPerPage={100}
                columns={mainColumns}
            />
        </CustomContainer>
    );
};
