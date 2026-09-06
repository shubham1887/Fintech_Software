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
import { PurchaseReport as PURCHASE_SELECT } from 'Components/Api/TransactionReportAPI';
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
    GET_PREVIOUS_MONTH,
    GET_CURRENT_YEAR
} from 'Components/Helper/Messages';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { PaymentReceipt } from './PaymentReceipt';

import { PaymentPurchase } from './PaymentPurchase';
import Receipt from '@material-ui/icons/Receipt';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

const PurchaseReport = () => {
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const formRef = useRef();
    const classes = commonStyle();

    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [userList, setUserList] = useState([]);
    const [IsReceiptModalOpen, setIsReceiptModalOpen] = useState(false);
    const [IsPurchaseModalOpen, setIsPurchaseModalOpen] = useState(false);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [openFilter, setOpenFilter] = useState(true);
    const [printData, setPrintData] = useState(null);
    const [totalCount, setTotalCount] = useState(0);
    const [Month, setMonth] = useState('');
    const [Year, setYear] = useState('');
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [transactions, setTransactions] = useState([]);

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
        Month: GET_PREVIOUS_MONTH,
        Year: GET_CURRENT_YEAR,
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
            SortDirection: SortDirection,
            Status: formRef.current.values.Status,
            UserID: formRef.current.values.User.UserID,
            Month: formRef.current.values.Month,
            Year: formRef.current.values.Year,
            IsExcelDownload: formRef.current.values.IsExcelDownload,
            IsPurchaseReport: formRef.current.values.IsExcelDownload,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText,
            Amount: formRef.current.values.Amount
        };
        //if (formRef.current.values.IsExcelDownload === false) setTransactions();

        // if (SkipCount <= 0) {
        //     setHeaderSummary({});
        // }
        setIsError(false);
        setLoading(true);

        var requestJSON = JSON.stringify(requestObj);

        var response = await PURCHASE_SELECT(requestJSON);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        setSeverity('success');

        var data = JSON.parse(response.data);

        if (data.IsExcelDownload) {
            downloadFile(data.ReportPath, 'Purchase Report');
            return;
        }
        if (SkipCount <= 0) {
            //setHeaderSummary(data.HeaderSummary);
            setTotalCount(data.TotalCount);
        }

        setTransactions(data.list);
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setMonth(values.Month);
        setTransactions([]);
        setYear(values.Year);
        setSubmitting(true);

        await callSelectData(TakeCount, SkipCount, '', '', '');

        // setLoading(true);
        // setTableItems([]);
        // var inputObj = {
        //     Month: values.Month,
        //     Year: values.Year,
        //     UserID: values.User.UserID
        // };
        // let requestJSON = JSON.stringify(inputObj);

        // const { data, status, isAuthorized } = await PURCHASE_SELECT(
        //     requestJSON
        // );

        // setLoading(false);
        // if (!isAuthorized) logout();
        // if (!status) {
        //     setIsError(true);
        //     setSeverity('error');
        //     setResponseMessage(data);

        //     return;
        // }

        // var d = JSON.parse(data);

        // setTableItems(d.list);
        // setTotalCount(data.Count);
        // if (d.IsExcelDownload) downloadFile(d.ReportPath, 'Purchase Report');
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
    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };
    useEffect(() => {
        GetYears();
        createColumns();
        CheckUserRole();
        refreshForm();
    }, []);

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
    };
    const ReceiptModal = (values) => {
        setPrintData(values);
        setIsReceiptModalOpen(true);
    };
    const Payment = (values) => {
        setPrintData(values);
        setIsPurchaseModalOpen(true);
    };

    const createColumns = () => {
        var columns = [];
        var statusIndex = 1;
        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });

        columns.push({
            label: 'Month',
            name: 'Month'
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
            label: 'Credit',
            name: 'Credit'
        });
        columns.push({
            label: 'Debit',
            name: 'Debit'
        });
        var options = [
            {
                Title: 'Payment',
                statusIndex: statusIndex,
                Callback: (values) => {
                    Payment(values);
                }
            },
            {
                Title: 'Receipt',
                statusIndex: statusIndex,
                Callback: (values) => {
                    ReceiptModal(values);
                }
            }
        ];

        columns.splice(0, 0, {
            label: ' ',
            name: 'TransactionMasterID',
            MoreMenu: true,
            options: options
        });

        columns.push({
            label: 'UserID',
            name: 'UserID',
            hide: true
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
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <CustomAccordion
                                    title={'Purchase Report'}
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
                                                onClick={(e) => {
                                                    setFieldValue(
                                                        'IsExcelDownload',
                                                        false
                                                    );
                                                    handleSubmit(e);
                                                }}
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
                data={transactions}
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

            {IsReceiptModalOpen ? (
                <PaymentReceipt
                    isOpen={IsReceiptModalOpen}
                    title="Receipt"
                    UserID={printData[7]}
                    Month={Month}
                    Year={Year}
                    positiveButtonClick={(printData) => {
                        setIsReceiptModalOpen(false);
                    }}
                    negativeButtonClick={() => {
                        setIsReceiptModalOpen(false);
                    }}
                />
            ) : null}
            {IsPurchaseModalOpen ? (
                <PaymentPurchase
                    isOpen={IsPurchaseModalOpen}
                    title="PAYMENT VOUCHER"
                    UserID={printData[7]}
                    Month={Month}
                    Year={Year}
                    positiveButtonClick={(printData) => {
                        setIsPurchaseModalOpen(false);
                    }}
                    negativeButtonClick={() => {
                        setIsPurchaseModalOpen(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};

export default PurchaseReport;
