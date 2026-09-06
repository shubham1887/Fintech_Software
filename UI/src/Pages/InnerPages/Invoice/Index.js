import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { Box, CircularProgress } from '@material-ui/core';
import { SelectRetailerAndAPIUserForDropDown as RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { Container, Grid, Button, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import {
    Select as INVOICE_SELECT,
    SelectByType as INVOICE_SELECT_TYPE
} from 'Components/Api/InvoiceApi';
import { Select as USERTYPE_SELECT } from 'Components/Api/UserTypeApi';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID, RETAILER_CODE } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { logout } from 'Components/Helper/Helpers';
import {
    GET_PREVIOUS_MONTH,
    GET_CURRENT_YEAR
} from 'Components/Helper/Messages';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const Index = () => {
    const formRef = useRef();
    const classes = commonStyle();
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [userTypeList, setUserTypeList] = useState([]);
    const [tableItems, setTableItems] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);

    const [userList, setUserList] = useState([]);

    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [currentPage, setCurrentPage] = useState(0);

    const [openFilter, setOpenFilter] = useState(true);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });

    const [Months] = useState([
        { Key: 1, Value: 'January' },
        { Key: 2, Value: 'February' },
        { Key: 3, Value: 'March' },
        { Key: 4, Value: 'April' },
        { Key: 5, Value: 'May' },
        { Key: 6, Value: 'June' },
        { Key: 7, Value: 'July' },
        { Key: 8, Value: 'August' },
        { Key: 9, Value: 'September' },
        { Key: 10, Value: 'October' },
        { Key: 11, Value: 'November' },
        { Key: 12, Value: 'December' }
    ]);
    const [Years, setYears] = useState([]);
    const defaultUser = {
        UserID: 0,
        FirmName: 'Select User'
    };
    const [ModelObj] = useState({
        User: defaultUser,
        Month: GET_PREVIOUS_MONTH,
        Year: GET_CURRENT_YEAR,
        UserTypeID: 0
    });
    const GetYears = () => {
        var currentYear = new Date().getFullYear();
        var yearList = [];
        for (var year = currentYear; year >= 2000; year--) {
            yearList.push({ Key: year });
        }
        setYears(yearList);
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        values.UserID = values.User.UserID;

        setSubmitting(true);

        setLoading(true);
        setTableItems([]);

        let requestJSON = JSON.stringify(values);

        const { data, status, isAuthorized } = await INVOICE_SELECT(
            requestJSON
        );

        setLoading(false);
        if (!isAuthorized) logout();
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);

            return;
        }

        var d = JSON.parse(data);

        setTableItems(d);

        setSubmitting(false);
    };

    function downloadFile(URL, Title) {
        let a1 = document.createElement('a');
        a1.href = URL;
        a1.setAttribute('download', Title);
        a1.setAttribute('target', '_blank');
        a1.click();
        //   document.removeChild(a1);
    }

    const validationSchema = Yup.object({
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.number()
                    .nullable(PLEASE_SELECT_VALID + 'User')
                    .min(0, PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            }),
        Month: Yup.string()
            .nullable(PLEASE_SELECT_VALID + 'month')
            .required(PLEASE_SELECT_VALID + 'month'),
        Year: Yup.string()
            .nullable(PLEASE_SELECT_VALID + 'year')
            .required(PLEASE_SELECT_VALID + 'year')
    });

    const selectUserTypes = async () => {
        setLoading(true);
        const { isAuthorized, status, data } = await USERTYPE_SELECT();
        setLoading(false);
        if (!isAuthorized) LOGOUT();

        if (!status) {
            setSeverity('error');
            setResponseMessage(data);
            setIsError(true);
            return;
        }

        var usertypedata = JSON.parse(data);
        setUserTypeList(usertypedata);
    };
    const UserTypeList = userTypeList.map((item) => (
        <MenuItem key={item.UserTypeID} value={item.UserTypeID}>
            {item.UserType}
        </MenuItem>
    ));
    const SelectUser = async (values) => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setUserList(objWithSelectAll);
            return;
        }

        setLoading(true);
        var request = JSON.stringify(values);
        setLoading(false);
        const userResponse = await RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN(
            request
        );

        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');

            return;
        }
        var data = JSON.parse(userResponse.data);

        let objWithSelectAll = [userDefault, ...data];

        setUserList(objWithSelectAll);
    };
    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };

    useEffect(() => {
        GetYears();
        createColumns();
        selectUserTypes();
        refreshForm();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            srno: true
        });
        columns.push({
            label: 'UserID',
            name: 'UserID',
            hide: true
        });

        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });
        columns.push({
            label: 'Role',
            name: 'UserType'
        });

        columns.push({
            label: 'MobileNumber',
            name: 'MobileNumber'
        });
        columns.push({
            label: 'EmailID',
            name: 'EmailID'
        });
        columns.push({
            label: 'GSTNo',
            name: 'GSTNo'
        });

        columns.push({
            label: 'ServiceWise',
            name: 'ServiceWise',
            CustomInvoice: true,
            callback: (value) => {
                GetInvoice(value);
            }
        });
        columns.push({
            label: 'OperatorWise',
            name: 'OperatorWise',
            CustomInvoice: true,
            callback: (value) => {
                GetInvoice(value);
            }
        });

        setMainColumns(columns);
    };

    const GetInvoice = async (values) => {
        var request = JSON.stringify(values);

        setLoading(true);

        const { data, status, isAuthorized } = await INVOICE_SELECT_TYPE(
            request
        );

        setLoading(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setSeverity('error');
            setResponseMessage(data);
            setIsError(true);

            return;
        }

        //if (values.IsDownload) downloadFile(data, 'Invoice');
        if (values.IsDownload) openNewWindow(data);
    };

    const openNewWindow = (html) => {
        var newWindow = window.open();

        newWindow.document.write(html);
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
        SelectUser(jsonObj);
    };

    const MonthItems = Months.map(({ Key, Value }, index) => (
        <MenuItem key={index} value={Key}>
            {Value}
        </MenuItem>
    ));
    const YearItems = Years.map(({ Key }, index) => (
        <MenuItem key={index} value={Key}>
            {Key}
        </MenuItem>
    ));

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
                enableReinitialize={true}
                innerRef={formRef}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={ModelObj}
                render={({ values, setFieldValue, handleChange }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Invoice Report'}
                                    open={openFilter}
                                    setOpen={(value) => setOpenFilter(value)}
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
                                                label="Month"
                                                name="Month"
                                                title="Month"
                                            >
                                                {MonthItems}
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
                                                label="Year"
                                                name="Year"
                                                title="Year"
                                            >
                                                {YearItems}
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
                                                label="Role"
                                                name="UserTypeID"
                                                title="Select Role"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select All
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
                                                component={AsyncAutoComplete}
                                                defaultValue={userDefault}
                                                FieldName="User"
                                                name="User"
                                                SubFieldName="UserID"
                                                options={userList}
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
                data={tableItems}
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                }}
                getSearchCallback={({ SearchText }) => {}}
                getSortCallback={({ column, direction }) => {}}
            />
        </CustomContainer>
    );
};
