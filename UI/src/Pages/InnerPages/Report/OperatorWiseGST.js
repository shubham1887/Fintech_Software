import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    MenuItem,
    Icon,
    Box,
    CircularProgress,
    Accordion
} from '@material-ui/core';
import { GSTOperatorWise as GST_OPERATORWISE_SELECT } from 'Components/Api/TransactionReportAPI';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_SELECT_VALID,
    RETAILER_CODE,
    ADMIN_CODE
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { logout } from 'Components/Helper/Helpers';
import excel from 'Assets/Images/excel.svg';
import { LOGOUT } from 'ConstComponents';
import {
    GET_Month_FOR_GST,
    GET_YEAR_FOR_GST
} from 'Components/Helper/Messages';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

const OperatorWiseGST = () => {
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const formRef = useRef();
    const classes = commonStyle();
    const [tableItems, setTableItems] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [userList, setUserList] = useState([]);
    const [currentPage, setCurrentPage] = useState(0);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [openFilter, setOpenFilter] = useState(true);
    const [TakeCount, setTakeCount] = useState(100);
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

    const [ModelObj] = useState({
        Month: GET_Month_FOR_GST(),
        Year: GET_YEAR_FOR_GST(),
        User: userDefault
    });
    const GetYears = () => {
        var currentYear = new Date().getFullYear();
        var yearList = [];
        for (var year = currentYear; year >= 2000; year--) {
            yearList.push({ Key: year });
        }
        setYears(yearList);
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
        SelectUser(jsonObj);
    };
    const SelectUser = async (values) => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setUserList(objWithSelectAll);
            return;
        }

        setLoading(true);
        var request = JSON.stringify(values);
        setLoading(false);
        const userResponse = await USER_SELECT_FOR_DROPDOWN(request);

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

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);

        setLoading(true);
        setTableItems([]);
        var inputObj = {
            Month: values.Month,
            Year: values.Year,
            UserID: values.User.UserID
        };
        var requestJSON = JSON.stringify(inputObj);

        const { data, status, isAuthorized } = await GST_OPERATORWISE_SELECT(
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

        setTableItems(d.list);
        setTotalCount(data.Count);
        if (d.IsExcelDownload) downloadFile(d.ReportPath, 'Recharge Report');
        setSubmitting(false);
    };

    function downloadFile(URL, Title) {
        let a1 = document.createElement('a');
        a1.href = URL;
        a1.setAttribute('download', Title);
        a1.setAttribute('target', '_blank');
        a1.click();
        document.removeChild(a1);
    }

    const validationSchema = Yup.object({
        Month: Yup.string()
            .nullable(PLEASE_SELECT_VALID + 'month')
            .required(PLEASE_SELECT_VALID + 'month'),
        Year: Yup.string()
            .nullable(PLEASE_SELECT_VALID + 'year')
            .required(PLEASE_SELECT_VALID + 'year')
    });

    useEffect(() => {
        GetYears();
        createColumns();
        CheckUserRole();
    }, []);

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });
        columns.push({
            label: 'Month',
            name: 'InvoiceMonth'
        });
        columns.push({
            label: 'Operator',
            name: 'Operator'
        });
        columns.push({
            label: 'Name',
            name: 'FirmName'
        });

        columns.push({
            label: 'Role',
            name: 'UserType'
        });
        columns.push({
            label: 'Amount',
            name: 'Amount',
            numeric: true
        });
        columns.push({
            label: 'Dist Comm',
            name: 'DCommission',
            numeric: true
        });

        columns.push({
            label: 'SD Comm',
            name: 'SDCommission',
            numeric: true
        });
        columns.push({
            label: 'R Comm',
            name: 'RetailerCommission',
            numeric: true
        });
        columns.push({
            label: 'Charge',
            name: 'Charge',
            numeric: true
        });
        columns.push({
            label: 'Comm. Given',
            name: 'GivenCommission',
            numeric: true
        });
        columns.push({
            label: '%',
            name: 'Percentage'
        });
        columns.push({
            label: 'Comm. Less',
            name: 'CommLess'
        });
        columns.push({
            label: 'ReviceGST',
            name: 'ReviceGST'
        });
        columns.push({
            label: 'Taxable Value',
            name: 'TaxableValue',
            numeric: true
        });

        setMainColumns(columns);
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
            >
                {({ setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Operator Wise GST'}
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
                                                    options={userList}
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
                                            md={1}
                                            lg={1}
                                        >
                                            <Box
                                                style={{ textAlign: 'center' }}
                                            >
                                                <Icon
                                                    className={
                                                        classes.CustomImageButtonStyle
                                                    }
                                                    disabled={loading}
                                                    classes={{
                                                        root: classes.iconRoot
                                                    }}
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'IsExcelDownload',
                                                            true
                                                        );
                                                        handleSubmit(e);
                                                    }}
                                                >
                                                    <img
                                                        alt=""
                                                        className={
                                                            classes.imageIcon
                                                        }
                                                        src={excel}
                                                    />
                                                </Icon>
                                            </Box>
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
            </Formik>

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

export default OperatorWiseGST;
