import React, { useEffect, useState } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';

import { LOGOUT } from 'ConstComponents';
import { Insert as USER_WISE_SERVICE_CHARGE_INSERT } from 'Components/Api/UserwiseServiceChargeApi';
import { Select as USER_WISE_SERVICE_CHARGE_SELECT } from 'Components/Api/UserwiseServiceChargeApi';
import { Delete as USER_WISE_SERVICE_CHARGE_DELETE } from 'Components/Api/UserwiseServiceChargeApi';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { SelectUserForDropDown as USER_SELECT_LIST } from 'Components/Api/UsersApi';
import { SelectChargeServices as SELRCT_CHARGE_SERVICES } from 'Components/Api/ServiceApi';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, Grid, MenuItem } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';

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

export const UserwiseServiceCharge = () => {
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [list, setlist] = useState([]);
    const [UsersList, setUsers] = useState([]);
    const [Services, setServices] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [openFilter, setOpenFilter] = useState(true);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });
    var formObj = {
        UserwiseServiceChargeMasterID: 0,
        ServiceID: 0,
        OneTime: 0,
        User: userDefault
    };
    const reset = () => {
        setUserDefault({
            UserID: 0,
            FirmName: 'Select User'
        });
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

    const selectUsers = async () => {
        const response = await USER_SELECT_LIST();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        let objWithSelectAll = [userDefault, ...data];

        setUsers(objWithSelectAll);
    };

    const selectServices = async () => {
        const response = await SELRCT_CHARGE_SERVICES();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };
    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
    };

    useEffect(() => {
        setLoadingOpen(true);
        CheckUserRole();
        select();
        selectServices();
        createColumns();
    }, []);

    const onDeleteClick = (item) => {
        var RequestObj = { UserwiseServiceChargeMasterID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await USER_WISE_SERVICE_CHARGE_DELETE(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        select();
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;
        var jsonReq = {
            UserwiseServiceChargeMasterID: 0,
            UserID: values.User.UserID,
            ServiceID: values.ServiceID,
            OneTime: values.OneTime
        };

        setIsError(false);
        var requestObj = JSON.stringify(jsonReq);
        setLoadingOpen(true);

        var response = await USER_WISE_SERVICE_CHARGE_INSERT(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

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
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.string()
                    .notOneOf(['0'], PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            }),
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'service')
            .required(PLEASE_SELECT_VALID + 'service'),
        OneTime: Yup.number().moreThan(
            49,
            PLEASE_PROVIDE_VALID + 'Minimum 50 Charge amount'
        )
    });

    const select = async () => {
        const response = await USER_WISE_SERVICE_CHARGE_SELECT();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setlist(data);
        setLoadingOpen(false);
    };

    const UpdateClick = async (row) => {
        let jsonObj = {
            UserwiseServiceChargeMasterID: row[0],
            UserID: row[2],
            ServiceID: row[3],
            OneTime: row[5]
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await USER_WISE_SERVICE_CHARGE_INSERT(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        select();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserwiseServiceChargeMasterID',
            srno: true
        });
        columns.push({
            label: 'Name',
            name: 'UserName'
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
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'OneTime',
            name: 'OneTime',
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
            name: 'UserwiseServiceChargeMasterID',
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
                onSubmit={handleSubmit}
                resetForm={formObj}
                render={() => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Service Charge'}
                                    open={openFilter}
                                    setOpen={(value) => setOpenFilter(value)}
                                >
                                    <Grid container spacing={2}>
                                        {isAdminRole ? (
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
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Service"
                                                name="ServiceID"
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
                                            <MaterialNumberField
                                                label="OneTime Charge"
                                                name="OneTime"
                                                type="text"
                                                placeholder="OneTime Charge"
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
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete service charge ."
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

            <h2 className={classes.PaperHead_h2}>User charge List</h2>
            <ReactTable data={list} columns={mainColumns} />
        </CustomContainer>
    );
};
