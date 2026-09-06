import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { Container, Grid, Button, MenuItem } from '@material-ui/core';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import Box from '@material-ui/core/Box';
import {
    API_USER_CODE,
    CLIENT_CODE,
    ADMIN_CODE,
    PLEASE_SELECT_VALID,
    RETAILER_CODE
} from 'Components/Helper/Messages';
import { SelectRoleWithoutSupport as USER_TYPE_SELECT_WITHOUT_SUPPORT } from 'Components/Api/UserTypeApi';
import { SelectRetailerAndAPIUserForDropDown as RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { Select as SEARCH } from 'Components/Api/DaybooReportAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { ExportTypeItems } from 'Components/ReportHelper';
const CustomAccordion = HOCAccordion(Accordion);

const CustomContainer = HOCLoader(Container);

export const DaybookReport = () => {
    const formRef = useRef();
    const [openFilter, setopenFilter] = useState(true);
    const [list, setList] = useState([]);
    const [downline, setDownline] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [userTypeList, setUserTypeList] = useState([]);
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [isAdminRole, setIsAdminRole] = useState(false);

    var currentUsertypeid = localStorage.getItem('UserTypeID');

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) {
            setIsAdminRole(true);
        }

        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
        }
        if (API_USER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
        }
        if (CLIENT_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
        }
    };

    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };

    useEffect(() => {
        CheckUserRole();
        createColumns();
        setLoadingOpen(false);
        selectUserTypes();
        refreshForm();
    }, [currentUsertypeid]);
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

    const UserTypeList = userTypeList.map((item) => (
        <MenuItem key={item.UserTypeID} value={item.UserTypeID}>
            {item.UserType}
        </MenuItem>
    ));

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        User: userDefault,
        UserTypeID: 0,
        ExportType: 'Select Download'
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            FromDate: values.FromDate,
            ToDate: values.ToDate,
            UserID: values.User.UserID,
            UserTypeID: values.UserTypeID,
            ExportType: formRef.current.values.ExportType
        };

        // setList([]);
        createColumns();
        var requestObj = JSON.stringify(inputObj);

        setLoadingOpen(true);
        setIsError(false);

        var response = await SEARCH(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        setSeverity('success');
        var data = JSON.parse(response.data);
        if (data.IsDownload) {
            downloadFile(data.ReportPath, 'UserDaybook Report');
            return;
        }
        setList(data.list);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }

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
        SelectUser(jsonObj);
    };

    const SelectUser = async (values) => {
        if (isRetailerRole === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setDownline(objWithSelectAll);
            return;
        }

        setLoadingOpen(true);
        var request = JSON.stringify(values);

        const userResponse = await RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN(
            request
        );
        setLoadingOpen(false);
        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(userResponse.data);

        let objWithSelectAll = [userDefault, ...data];

        setDownline(objWithSelectAll);
    };

    const validationSchema = Yup.object({
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'from date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'from date')
            .typeError(PLEASE_SELECT_VALID + 'from date'),

        ToDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'to date')
            .typeError(PLEASE_SELECT_VALID + 'to date')
    });

    const createColumns = () => {
        var columns = [];
        var textwrapwidth = 60;

        columns.push({
            label: '#',
            name: 'Operator',
            srno: true
        });
        columns.push({
            label: 'Operator',
            name: 'Operator'
        });

        columns.push({
            label: 'Total',
            name: 'TotalCount',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Total Amt',
            name: 'TotalAmount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'Success',
            name: 'SuccessCount',
            textwrapwidth
        });

        columns.push({
            label: 'Success Amt',
            name: 'SuccessAmount',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Failed',
            name: 'FailedCount',
            textwrapwidth
        });

        columns.push({
            label: 'Fail Amt',
            name: 'FailedAmount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'Pending',
            name: 'PendingCount',
            textwrapwidth
        });

        columns.push({
            label: 'Pending Amt',
            name: 'PendingAmount',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Commission',
            name: 'RetailerCommission',
            textwrapwidth,
            numeric: true
        });

        if (isAdminRole) {
            columns.push({
                label: 'D.Comm.',
                name: 'DistributerCommission',
                textwrapwidth,
                numeric: true
            });

            columns.push({
                label: 'SD. Comm.',
                name: 'SuperDistributerCommission',
                textwrapwidth,
                numeric: true
            });
        }

        columns.push({
            label: 'Surcharge',
            name: 'Surcharge',
            textwrapwidth,
            numeric: true
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
                innerRef={formRef}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={ModelObj}
                render={({
                    values,
                    setFieldValue,
                    handleChange,
                    handleSubmit
                }) => {
                    return (
                        <Form
                            style={{
                                marginBottom: '15px'
                            }}
                        >
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'DayBook'}
                                    open={openFilter}
                                    setOpen={(value) => setopenFilter(value)}
                                >
                                    <Grid container spacing={3}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialDatePickerField
                                                name="FromDate"
                                                label="From Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialDatePickerField
                                                name="ToDate"
                                                label="To Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>
                                        {!isRetailerRole ? (
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
                                                    title="Select Role"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Role
                                                    </MenuItem>
                                                    {UserTypeList}
                                                </MaterialSelect>
                                            </Grid>
                                        ) : null}
                                        {!isRetailerRole ? (
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
                                                    options={downline}
                                                    getOptionLabel={(option) =>
                                                        option.FirmName
                                                    }
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
                                        ) : null}
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="ExportType"
                                                name="ExportType"
                                                title="ExportType"
                                            >
                                                {ExportTypeItems}
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
                                                title="Search"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                Search
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
                                    </Grid>
                                </CustomAccordion>
                            </Grid>
                        </Form>
                    );
                }}
            />

            <h2 className={classes.PaperHead_h2}>Report</h2>
            <ReactTable data={list} columns={mainColumns} rowsPerPage={100} />
        </CustomContainer>
    );
};
