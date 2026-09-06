import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';

import { SelectDownlinForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';

import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';

import { SelectSlabDetails as SELECT_COMMITION_STRUCTURE } from 'Components/Api/SlabDetailApi';
import { Container, Grid, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';

import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import {
    PLEASE_SELECT_VALID,
    RETAILER_CODE,
    API_USER_CODE,
    ADMIN_CODE,
    DIST_CODE,
    CLIENT_CODE,
    SUPPORT_CODE
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { ExportTypeItems } from 'Components/ReportHelper';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const SlabDetailByUser = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [Services, setServices] = useState([]);
    const [openFilter, setopenFilter] = useState(true);
    const [Commission, setCommission] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [downline, setDownline] = useState([]);
    const [IsFirmName, setIsFirmName] = useState(false);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [IsDistRole, setIsDistRole] = useState(false);
    const [IsApiRole, setIsApiRole] = useState(false);
    const [IsRetailerRole, setIsRetailerRole] = useState(false);
    const [ClientRole, setIsClientRole] = useState(false);
    const [Operator, setOperator] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [currentUserTypeID, setCurrentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );
    const [totalCount, setTotalCount] = useState(0);
    const [headerSummary, setHeaderSummary] = useState({});
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });

    const [ModelObj] = useState({
        ServiceID: 0,
        ExportType: 'Select Download',
        User: {
            UserID: 0,
            FirmName: 'All User'
        },
        OperatorID: 0
    });

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
        if (SUPPORT_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);

        if (DIST_CODE === parseInt(currentUserTypeID)) setIsDistRole(true);

        if (RETAILER_CODE === parseInt(currentUserTypeID))
            setIsRetailerRole(true);

        if (API_USER_CODE === parseInt(currentUserTypeID)) setIsApiRole(true);
        if (CLIENT_CODE === parseInt(currentUserTypeID)) setIsClientRole(true);
    };
    const OnOperatorChange = async (name, value, setFieldValue) => {
        if (isAdminRole === false) return;

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

        setIsFirmName(true);

        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);
            setOperator(data);
        }
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        values.UserID = values.User.UserID;
        callSelectData(TakeCount, SkipCount, '', '', '');
        setSubmitting(false);
    };

    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let inputObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            Status: formRef.current.values.Status,
            UserID: formRef.current.values.User.UserID,
            ServiceID: formRef.current.values.ServiceID,
            OperatorID: formRef.current.values.OperatorID,
            ExportType: formRef.current.values.ExportType,
            ReportFor: formRef.current.values.ReportFor
        };

        setCommission([]);
        createColumns();

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await SELECT_COMMITION_STRUCTURE(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);

        if (data.list[0].FirmName.length > 5) setIsFirmName(true);
        if (data.IsDownload) {
            downloadFile(data.ReportPath, 'Comm Structure Report');
            return;
        }

        if (SkipCount <= 0) {
            setTotalCount(data.Count);
        }

        setCommission(data.list);
    };
    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }

    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const validationSchema = Yup.object({
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.number()
                    .nullable(PLEASE_SELECT_VALID + 'User')
                    .min(0, PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            })
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
        SelectUser(jsonObj);
    };

    const SelectUser = async (values) => {
        if (RETAILER_CODE === parseInt(currentUserTypeID)) {
            let objWithSelectAll = [userDefault];
            setDownline(objWithSelectAll);
            return;
        }

        setLoading(true);
        var request = JSON.stringify(values);

        const userResponse = await USER_SELECT_FOR_DROPDOWN(request);

        setLoading(false);

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

    useEffect(() => {
        CheckUserRole();
        //  SelectUser();
        selectServices();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'SlabDetailID',
            srno: true
        });

        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'Details',
            name: 'Details'
        });
        if (IsFirmName === true && SelectUser === 'All User') {
            columns.push({
                label: 'Name',
                name: 'FirmName'
            });
        }
        columns.push({
            label: 'Retailer',
            name: 'StrRetailerComm'
        });

        if (IsRetailerRole !== true && IsApiRole !== true) {
            columns.push({
                label: 'Distributer',
                name: 'StrDistributorComm'
            });
        }

        if (
            IsDistRole !== true &&
            IsRetailerRole !== true &&
            IsApiRole !== true
        ) {
            columns.push({
                label: 'SuperDistributer',
                name: 'StrSuperDistributorComm'
            });
        }
        if (ClientRole === true) {
            columns.push({
                label: 'Client',
                name: 'StrClientComm'
            });
        }

        setMainColumns(columns);
    };

    return (
        <CustomContainer
            isLoading={loading}
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
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={' Commission Structure'}
                                    open={openFilter}
                                    setOpen={(value) => setopenFilter(value)}
                                >
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

                                        {isAdminRole ? (
                                            <>
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
                                                        <MenuItem
                                                            key="0"
                                                            value="0"
                                                        >
                                                            All Operator
                                                        </MenuItem>
                                                        {Operator.map(
                                                            (Item) => (
                                                                <MenuItem
                                                                    key={
                                                                        Item.OperatorID
                                                                    }
                                                                    value={
                                                                        Item.OperatorID
                                                                    }
                                                                >
                                                                    {
                                                                        Item.OperatorName
                                                                    }
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
                                                        options={downline}
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.FirmName}
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
                                            </>
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
                                                disabled={loading}
                                            >
                                                Search
                                            </PrimaryButton>
                                            {loading && (
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

            <ReactTable
                data={Commission}
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
