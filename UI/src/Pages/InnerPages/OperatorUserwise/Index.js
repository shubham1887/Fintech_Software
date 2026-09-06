import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import Box from '@material-ui/core/Box';
import Button from '@material-ui/core/Button';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { Select as OPERATOR_USERWISE_SELECT } from 'Components/Api/OperatorUserwiseApi';
import { Insert as OPERATOR_USERWISE_INSERT } from 'Components/Api/OperatorUserwiseApi';
import { DeleteByUserID as DELETE_BY_USERID } from 'Components/Api/OperatorUserwiseApi';
import { DeleteAll as DELETE_BY_All } from 'Components/Api/OperatorUserwiseApi';
import { Activate as USER_OPERATOR_ACTIVATE } from 'Components/Api/OperatorUserwiseApi';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    RETAILER_CODE
} from 'Components/Helper/Messages';
const CustomContainer = HOCLoader(Container);

export const OperatorUserwise = () => {
    const classes = commonStyle();
    const [Services, setServices] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [downline, setDownline] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [isAlertOpenUserwise, setIsAlertOpenUserwise] = useState(false);
    const [isAlertOpenAllUserwise, setIsAlertOpenAllUserwise] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [selectedUserID, setselectedUserID] = useState(0);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const CheckUserRole = () => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
        }
    };
    const formRef = useRef();
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const formObject = {
        User: {
            UserID: 0,
            FirmName: 'All User'
        }
    };
    const UserWiseClick = async (item) => {
        var RequestObj = {
            UserID: item
        };
        setIsAlertOpenUserwise(true);
        setConfirmAlertRequest(RequestObj);
    };
    const AllUserWiseClick = async () => {
        setIsAlertOpenAllUserwise(true);
    };

    const UserwiseConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpenUserwise}
            title="User-wise Operator Confirmation"
            message="Are you sure you want to remove the user-wise operator setting?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenUserwise(false);
                SetDefaultUserwise(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpenUserwise(false);
            }}
        />
    );
    const AllUserwiseConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpenAllUserwise}
            title="All User-wise Operator Confirmation"
            message="Are you sure you want to remove the all user-wise operator setting?"
            //request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenAllUserwise(false);
                ResetAllUserwise();
            }}
            negativeButtonClick={() => {
                setIsAlertOpenAllUserwise(false);
            }}
        />
    );

    const SetDefaultUserwise = async (request) => {
        var jsonReq = JSON.stringify(request);
        setIsError(false);
        setLoadingOpen(true);
        const response = await DELETE_BY_USERID(jsonReq);
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

        if (response.data == 'Default set successfully') {
            selectData(
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
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        setSeverity('success');
        setResponseMessage(response.data);

        selectData(
            formRef.current.values.User.UserID,
            formRef.current.values.ServiceID
        );
    };

    const UpdateValidation = Yup.object({
        OperatorUserwiseID: Yup.number().required(PLEASE_PROVIDE_VALID + 'ID'),
        UserID: Yup.number().required(PLEASE_PROVIDE_VALID + 'user'),
        ServiceID: Yup.number().required(PLEASE_PROVIDE_VALID + 'Service'),
        OperatorID: Yup.number().required(PLEASE_PROVIDE_VALID + 'Operator')
    });

    const UpdateClick = async (row) => {
        if (!formRef.current) {
            return;
        }
        setIsError(false);
        let jsonObj = {
            OperatorUserwiseID: row[0],
            UserID: formRef.current.values.User.UserID,
            ServiceID: row[1],
            OperatorID: row[2],
            BlockedAmount: row[4],
            UsageLimit: row[5]
        };

        var errorValidation = false;
        await UpdateValidation.validate(jsonObj).catch(function (err) {
            errorValidation = true;
            setSeverity('error');
            setResponseMessage(err.message);
            setIsError(true);
            return;
        });

        if (errorValidation === true) {
            return;
        }

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await OPERATOR_USERWISE_INSERT(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData(
            formRef.current.values.User.UserID,
            formRef.current.values.ServiceID
        );
    };

    const selectData = async (selectedUserID, ServiceID) => {
        setItemList([]);
        if (ServiceID <= 0) return;
        var requestObj = {
            UserID: selectedUserID,
            ServiceID: ServiceID
        };

        var request = JSON.stringify(requestObj);
        setLoadingOpen(true);
        const response = await OPERATOR_USERWISE_SELECT(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            return;
        }

        var data = JSON.parse(response.data);

        setItemList(data);
    };
    function OnServiceChange(name, value, setFieldValue) {
        selectData(formRef.current.values.User.UserID, value);
    }

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
        SelectUser(jsonObj);
    };

    const SelectUser = async () => {
        if (isRetailerRole === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setDownline(objWithSelectAll);
            return;
        }
        const response = await USER_SELECT_FOR_DROPDOWN();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);

        let objWithSelectAll = [userDefault, ...data];
        setDownline(objWithSelectAll);
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
    const activate = async (item, value, row) => {
        var jsonObj = {
            UserID: formRef.current.values.User.UserID,
            ServiceID: row[1],
            OperatorID: row[2],
            UsageLimit: row[5],
            OperatorUserwiseID: item,
            IsActive: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await USER_OPERATOR_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        selectData(
            formRef.current.values.User.UserID,
            formRef.current.values.ServiceID
        );
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'OperatorUserwiseID',
            srno: true
        });
        columns.push({
            label: 'ServiceID',
            name: 'ServiceID',
            hide: true
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
            label: 'Block Amount',
            name: 'BlockedAmount',
            editable: true
        });
        columns.push({
            label: 'Usage Limit',
            name: 'UsageLimit',
            editable: true
        });
        columns.push({
            label: 'Today Sale',
            name: 'Todaysale'
        });
        columns.push({
            label: 'RemainingSale',
            name: 'RemainingSale'
        });

        columns.push({
            label: 'Active',
            name: 'IsActive',
            activateRow: true,
            primaryKeyIndex: 0,
            callback: (id, value, row) => {
                activate(id, value, row);
            }
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

        setMainColumns(columns);
    };

    useEffect(() => {
        CheckUserRole();
        SelectUser();
        selectServices();
        createColumns();
    }, []);

    const validationSchema = Yup.object({
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.string()
                    .notOneOf(['0'], PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            })
    });

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
                initialValues={formObject}
                validationSchema={validationSchema}
                resetForm={formObject}
            >
                {({
                    values,
                    errors,
                    setFieldValue,
                    handleSubmit,
                    validateForm
                }) => {
                    return (
                        <Form>
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={7} lg={7}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Userwise
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
                                            <Field
                                                component={AsyncAutoComplete}
                                                defaultValue={userDefault}
                                                FieldName="User"
                                                name="User"
                                                SubFieldName="UserID"
                                                options={downline}
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
                                                title="Set Default for User"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={async () => {
                                                    const errors = await validateForm();
                                                    if (
                                                        Object.keys(errors)
                                                            .length === 0 &&
                                                        errors.constructor ===
                                                            Object
                                                    ) {
                                                        UserWiseClick(
                                                            formRef.current
                                                                .values.User
                                                                .UserID
                                                        );
                                                    }
                                                }}
                                                style={{
                                                    marginTop: '11px'
                                                }}
                                            >
                                                Set Default for User
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
                                                title="Reset all Userwise"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                style={{
                                                    marginTop: '11px'
                                                }}
                                                onClick={() =>
                                                    AllUserWiseClick()
                                                }
                                            >
                                                Reset all Userwise
                                            </SecondaryButton>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            <Grid container spacing={2}>
                <Grid item xs={12} sm={12} md={7} lg={7}>
                    <h2 className={classes.PaperHead_h2}>Userwise Operator</h2>
                </Grid>
                <Grid item xs={12} sm={12} md={5} lg={5}>
                    <div
                        style={{
                            color: 'green'
                        }}
                    >
                        Note : if you want fix block amount then use "," and for
                        range use "-" like (eg. 10, 20, 50-200, 300-500)
                    </div>
                </Grid>
            </Grid>
            <ReactTable data={itemList} columns={mainColumns} />

            {isAlertOpenAllUserwise && AllUserwiseConfirmation}
            {isAlertOpenUserwise && UserwiseConfirmation}
        </CustomContainer>
    );
};
