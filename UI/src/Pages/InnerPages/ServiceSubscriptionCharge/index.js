import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import { LOGOUT } from 'ConstComponents';
import {
    Insert as USER_WISE_SERVICE_CHARGE_INSERT,
    AutoDebitOnOff as AUTO_DEBIT_ACTIVATE,
    Delete as USER_WISE_SERVICE_CHARGE_DELETE,
    Select as USER_WISE_SERVICE_CHARGE_SELECT,
    Update as USER_WISE_SERVICE_CHARGE_UPDATE
} from 'Components/Api/ServiceSubscriptionChargeAPI';
import {
    API_USER_CODE,
    RETAILER_CODE,
    CLIENT_CODE
} from 'Components/Helper/Messages';
import { MaterialTextAreaField } from 'Components/Inputes/FormikMaterialInput';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { SelectByRole as SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectSubscriptionAllowedServices as SELECT_CHARGE_SERVICES } from 'Components/Api/ServiceApi';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, Grid, MenuItem } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { SelectRoleWithoutSupport as USER_TYPE_SELECT_WITHOUT_SUPPORT } from 'Components/Api/UserTypeApi';
import { Select as SELECT_PLAN } from 'Components/Api/ServiceSubscriptionPlanMasterAPI';
import {
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID,
    ADMIN_CODE
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const ServiceSubscriptionCharge = () => {
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const formRef = useRef();
    const [list, setlist] = useState([]);
    const [UsersList, setUsers] = useState([]);
    const [Services, setServices] = useState([]);
    const [plans, setPlan] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [userTypeList, setUserTypeList] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [isRetailer, setIsRetailerRole] = useState(false);
    const [openFilter, setOpenFilter] = useState(true);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [currentPage, setCurrentPage] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });

    const [formObj] = useState({
        ServiceAndUserwiseSubscriptionID: 0,
        ServiceID: 0,
        Charge: 0,
        User: userDefault,
        SubscriptionType: 'Monthly',
        UserTypeID: 0,
        ServiceSubscriptionPlanMasterID: 0,
        OriginalCharge: 0,
        LoginID: ''
    });

    const formValidationSchema = Yup.object({
        Charge: Yup.number().moreThan(
            0,
            PLEASE_PROVIDE_VALID + ' charge amount'
        ),
        OriginalCharge: Yup.number().moreThan(
            0,
            PLEASE_PROVIDE_VALID + 'original charge amount'
        ),
       
    });

    const reset = () => {
        setUserDefault({
            UserID: 0,
            FirmName: 'Select User'
        });
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

    const [selectedService, setSelectedService] = useState('');

    const handleServiceChange = (event) => {
        setSelectedService(event.target.value);
    };

    const selectServices = async () => {
        const response = await SELECT_CHARGE_SERVICES();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
        if (RETAILER_CODE === parseInt(currentUsertypeid))
            setIsRetailerRole(true);
    };

    const selectUserTypes = async () => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            return;
        }
        if (API_USER_CODE === parseInt(currentUsertypeid)) {
            return;
        }
        if (CLIENT_CODE === parseInt(currentUsertypeid)) {
            return;
        }
        const response = await USER_TYPE_SELECT_WITHOUT_SUPPORT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setIsError(true);
            setResponseMessage(response.data);
        }

        var data = JSON.parse(response.data);
        setUserTypeList(data);
    };

    const selectPlan = async () => {
        const response = await SELECT_PLAN();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setPlan(data);
    };
    useEffect(() => {
        CheckUserRole();

        selectServices();
        createColumns();
        selectUserTypes();
        selectPlan();
        callSelectData(TakeCount, SkipCount, '', '', '');
    }, []);

    const onDeleteClick = (item) => {
        var RequestObj = { ServiceAndUserwiseSubscriptionID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await USER_WISE_SERVICE_CHARGE_DELETE(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            callSelectData(TakeCount, SkipCount, '', '', '');
            return;
        }
        var data = JSON.parse(response.data);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data);

        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;
        var jsonReq = {
            ServiceAndUserwiseSubscriptionID: 0,
            UserID: values.User.UserID,
            ServiceID: values.ServiceID,
            Charge: values.Charge,
            Remark: values.Remark,
            SubscriptionType: values.SubscriptionType,
            UserTypeID: values.UserTypeID,
            ServiceSubscriptionPlanMasterID:
                values.ServiceSubscriptionPlanMasterID,
            OriginalCharge: values.OriginalCharge,
            LoginID: values.LoginID
        };

        setIsError(false);
        var requestObj = JSON.stringify(jsonReq);
        setLoadingOpen(true);

        var response = await USER_WISE_SERVICE_CHARGE_INSERT(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data.Message);
        setSubmitting(false);
        resetForm(formObj);
        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection
        };
        SelectReport(requestObj);
    };
    const SelectReport = async (requestObj) => {
        var request = JSON.stringify(requestObj);
        setlist([]);
        setIsError(false);
        setLoadingOpen(true);
        const response = await USER_WISE_SERVICE_CHARGE_SELECT(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);

        if (SkipCount <= 0) {
            setTotalCount(data.Count);
        }

        setlist(data.list);
    };

    const UpdateClick = async (row) => {
        let jsonObj = {
            ServiceAndUserwiseSubscriptionID: row[0],
            UserID: row[1],
            ServiceID: row[2],
            OriginalCharge: row[8],
            Charge: row[9]
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await USER_WISE_SERVICE_CHARGE_UPDATE(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data);

        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ServiceAndUserwiseSubscriptionID',
            srno: true
        });
        columns.push({
            label: 'UserID',
            name: 'UserID',
            hide: true
        });
        columns.push({
            label: 'ServiceID',
            name: 'ServiceID',
            hide: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Role',
            name: 'UserType'
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
            label: 'Type',
            name: 'SubscriptionType'
        });

        columns.push({
            label: 'Original Charge',
            name: 'OriginalCharge',
            editable: true
        });

        columns.push({
            label: 'Discounted Charge',
            name: 'Charge',
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
            name: 'ServiceAndUserwiseSubscriptionID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });
        columns.push({
            label: 'Plan',
            name: 'PlanName'
        });
        columns.push({
            label: 'LoginID',
            name: 'LoginID'
        });

        columns.push({
            label: 'Remark',
            name: 'Remark'
        });

        setMainColumns(columns);
    };
    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
    };

    const activate = async (item, value) => {
        var jsonObj = {
            ServiceAndUserwiseSubscriptionID: item,
            IsAutoDebit: value
        };

        var request = JSON.stringify(jsonObj);

        setLoadingOpen(true);

        setIsError(false);
        const response = await AUTO_DEBIT_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data);

        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const selectUsers = async (values) => {
        var request = JSON.stringify(values);
        const { isAuthorized, status, data } = await SELECT_FOR_DROPDOWN(
            request
        );
        if (!isAuthorized) LOGOUT();
        if (!status) return;
        var d = JSON.parse(data);
        let objWithSelectAll = [userDefault, ...d];

        setUsers(objWithSelectAll);
    };

    const UserTypeList = userTypeList.map((item) => (
        <MenuItem key={item.UserTypeID} value={item.UserTypeID}>
            {item.UserType}
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
                initialValues={formObj}
                validationSchema={formValidationSchema}
                onSubmit={handleSubmit}
                resetForm={formObj}
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Service Subscription Charge'}
                                    open={openFilter}
                                    setOpen={(value) => setOpenFilter(value)}
                                >
                                    <Grid container spacing={2}>
                                        {isAdminRole && (
                                            <>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    <MaterialSelect
                                                        label="Select Role"
                                                        name="UserTypeID"
                                                        title="Select Role"
                                                    >
                                                        <MenuItem
                                                            key="0"
                                                            value="0"
                                                        >
                                                            Select Role
                                                        </MenuItem>
                                                        {UserTypeList}
                                                    </MaterialSelect>
                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    <Field
                                                        component={
                                                            AsyncAutoComplete
                                                        }
                                                        defaultValue={
                                                            userDefault
                                                        }
                                                        FieldName="User"
                                                        name="User"
                                                        SubFieldName="UserID"
                                                        options={UsersList}
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.FirmName}
                                                        textFieldProps={{
                                                            label: 'Select User'
                                                        }}
                                                        onSearchCallback={(
                                                            text
                                                        ) => {
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
                                                                UserID:
                                                                    value.UserID,
                                                                FirmName:
                                                                    value.FirmName
                                                            });
                                                        }}
                                                    />
                                                </Grid>
                                            </>
                                        )}
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
                                                value={selectedService}
                                                onChange={handleServiceChange}
                                            >
                                                <MenuItem key="0" value="0">
                                                    All Service
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
                                                label="Select Subscription Type"
                                                name="SubscriptionType"
                                                title="Subscription Type"
                                            >
                                                <MenuItem
                                                    key="Monthly"
                                                    value="Monthly"
                                                >
                                                    Monthly
                                                </MenuItem>
                                                <MenuItem
                                                    key="Quarterly"
                                                    value="Quarterly"
                                                >
                                                    Quarterly
                                                </MenuItem>
                                                <MenuItem
                                                    key="Yearly"
                                                    value="Yearly"
                                                >
                                                    Yearly
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
                                            <MaterialSelect
                                                label="Select Plan"
                                                name="ServiceSubscriptionPlanMasterID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Plan
                                                </MenuItem>
                                                {plans.map((Item) => (
                                                    <MenuItem

                                                        key={
                                                            Item.ServiceSubscriptionPlanMasterID
                                                        }
                                                        value={
                                                            Item.ServiceSubscriptionPlanMasterID
                                                        }
                                                    >
                                                        {Item.PlanName}

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
                                            <MaterialNumberField
                                                label="Original Charge"
                                                name="OriginalCharge"
                                                type="text"
                                                placeholder="Original Charge"
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
                                                label="Discounted Charge"
                                                name="Charge"
                                                type="text"
                                                placeholder="Discounted Charge"
                                            />
                                        </Grid>
                                        {/* {selectedService === "Demat" && ( */}
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextAreaField
                                                label="LoginID"
                                                name="LoginID"
                                                type="text"
                                                length={200}
                                                title="LoginID"
                                            />
                                        </Grid>
                                        {/* )} */}
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextAreaField
                                                label="Remark"
                                                name="Remark"
                                                type="text"
                                                length={100}
                                                title="Remark"
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
                                                variant="contained"
                                                color="secondary"
                                                title="Reset"
                                                className={
                                                    classes.CustomDangerButtonStyle
                                                }
                                                onClick={() => {
                                                    reset();
                                                }}
                                            >
                                                Reset
                                            </SecondaryButton>
                                        </Grid>
                                    </Grid>
                                </CustomAccordion>
                            </Grid>
                        </Form>
                    );
                }}
            />
            {isAlertOpen && (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete subscription charge ?"
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

            <h2 className={classes.PaperHead_h2}>Subscription Charge List</h2>

            <ReactTable
                data={list}
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                    callSelectData(
                        RowsPerPage,
                        RowsPerPage * currentPage,
                        '',
                        '',
                        ''
                    );
                }}
                getSearchCallback={({ SearchText }) => {
                    callSelectData(TakeCount, SkipCount, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    callSelectData(
                        TakeCount,
                        currentPage,
                        SearchText,
                        column,
                        direction
                    );
                }}
            />
        </CustomContainer>
    );
};
