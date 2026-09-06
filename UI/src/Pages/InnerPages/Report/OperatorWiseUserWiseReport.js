import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectOperatorWisesUserWiseReport as TRANSACTION_FILTER } from 'Components/Api/OperatorWiseUserWiseReportApi';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { Container, Grid, Box, Accordion, Hidden } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { StatusItems, ExportTypeItems } from 'Components/ReportHelper';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { ADMIN_CODE } from 'Components/Helper/Messages';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';

import { SelectReportByServiceByID } from 'Pages/InnerPages/Report/SelectReportByServiceByID';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const OperatorWiseUserWiseReport = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [openFilter, setopenFilter] = useState(true);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [transactions, setTransactions] = useState([]);
    const [userList, setUserList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [isOpenReport, setIsOpenReport] = useState(false);
    const [reportData, setReportData] = useState({});
    var currentUsertypeid = localStorage.getItem('UserTypeID');

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });
    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
        Status: 'ALL',
        ExportType: 'Select Download',
        User: userDefault
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);
        createColumns();
        await callSelectData(TakeCount, SkipCount);
        setSubmitting(false);
    };
    const SelectUser = async () => {
        setLoading(true);

        setLoading(false);
        const userResponse = await USER_SELECT_FOR_DROPDOWN();

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
    const callSelectData = async (TakeCount, SkipCount) => {
        let inputObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            UserID: formRef.current.values.User.UserID,
            Status: formRef.current.values.Status,
            ExportType: formRef.current.values.ExportType
        };
        var requestObj = JSON.stringify(inputObj);
        setIsError(false);
        setLoading(true);
        setResponseMessage('');

        var response = await TRANSACTION_FILTER(requestObj);
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

        if (data.IsDownload) {
            downloadFile(data.ReportPath, 'OperatorWise_UserWise_Report');
            return;
        }
        if (SkipCount <= 0) {
            setTotalCount(data.Count);
        }
        setTransactions(data.list);
    };
    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }

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

    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };
    const SelectReport = async (row, ServiceID) => {
        if (row[2] === 'Total') return;
        setReportData({
            ServiceID: ServiceID,
            UserID: formRef.current.values.User.UserID,
            FromDate:
                row[1] === 'Total' ? formRef.current.values.FromDate : row[1],
            ToDate: row[1] === 'Total' ? formRef.current.values.ToDate : row[1]
        });
        setIsOpenReport(true);
    };
    useEffect(() => {
        CheckUserRole();
        refreshForm();
        SelectUser();
    }, []);

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
    };

    const OpenReportModalCallback = (value) => {
        setIsOpenReport(value);
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'DOC',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        columns.push({
            label: 'MobileSaleID',
            name: 'MobileSaleID',
            hide: true
        });
        columns.push({
            label: 'Mobile',
            name: 'MobileSale',

            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[2]);
            }
        });
        columns.push({
            label: 'DTHSaleID',
            name: 'DTHSaleID',
            hide: true
        });
        columns.push({
            label: 'DTH',
            name: 'DTHSale',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[4]);
            }
        });
        columns.push({
            label: 'PostPaidID',
            name: 'PostPaidID',
            hide: true
        });
        columns.push({
            label: 'Post Paid',
            name: 'PostPaid',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[6]);
            }
        });
        columns.push({
            label: 'ElectricityID',
            name: 'ElectricityID',
            hide: true
        });
        columns.push({
            label: 'Electricity',
            name: 'Electricity',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[8]);
            }
        });
        columns.push({
            label: 'MoneyTransferID',
            name: 'MoneyTransferID',
            hide: true
        });
        columns.push({
            label: 'DMT',
            name: 'MoneyTransfer',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[10]);
            }
        });
        columns.push({
            label: 'ExpressMoneyTransferID',
            name: 'ExpressMoneyTransferID',
            hide: true
        });
        columns.push({
            label: 'Express-DMT',
            name: 'ExpressMoneyTransfer',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[12]);
            }
        });
        columns.push({
            label: 'AepsSaleID',
            name: 'AepsSaleID',
            hide: true
        });
        columns.push({
            label: 'AEPS',
            name: 'AepsSale',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[14]);
            }
        });
        columns.push({
            label: 'PaymentGatewaySaleID',
            name: 'PaymentGatewaySaleID',
            hide: true
        });
        columns.push({
            label: 'PG',
            name: 'PaymentGatewaySale',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[16]);
            }
        });
        columns.push({
            label: 'MAtmID',
            name: 'MAtmID',
            hide: true
        });
        columns.push({
            label: 'Micro-ATM',
            name: 'MAtm',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[18]);
            }
        });
        columns.push({
            label: 'POSID',
            name: 'POSID',
            hide: true
        });
        columns.push({
            label: 'POS',
            name: 'POS',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[20]);
            }
        });
        columns.push({
            label: 'PayoutID',
            name: 'PayoutID',
            hide: true
        });
        columns.push({
            label: 'Payout',
            name: 'Payout',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[22]);
            }
        });
        columns.push({
            label: 'UPITransferID',
            name: 'UPITransferID',
            hide: true
        });
        columns.push({
            label: 'UPI',
            name: 'UPITransfer',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[24]);
            }
        });
        columns.push({
            label: 'FasTagID',
            name: 'FasTagID',
            hide: true
        });
        columns.push({
            label: 'FastTag',
            name: 'FasTag',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[26]);
            }
        });
        columns.push({
            label: 'WaterBillID',
            name: 'WaterBillID',
            hide: true
        });
        columns.push({
            label: 'Water-Bill',
            name: 'WaterBill',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[28]);
            }
        });
        columns.push({
            label: 'GeneralInsuranceID',
            name: 'GeneralInsuranceID',
            hide: true
        });
        columns.push({
            label: 'Insurance',
            name: 'GeneralInsurance',

            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[30]);
            }
        });
        columns.push({
            label: 'BroadbandID',
            name: 'BroadbandID',
            hide: true
        });
        columns.push({
            label: 'Broadband',
            name: 'Broadband',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[32]);
            }
        });
        columns.push({
            label: 'PipedGasID',
            name: 'PipedGasID',
            hide: true
        });
        columns.push({
            label: 'Piped-Gas',
            name: 'PipedGas',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[34]);
            }
        });
        columns.push({
            label: 'GasCylinderBookingID',
            name: 'GasCylinderBookingID',
            hide: true
        });
        columns.push({
            label: 'GasCylinderBooking',
            name: 'GasCylinderBooking',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[36]);
            }
        });
        columns.push({
            label: 'CreditCardID',
            name: 'CreditCardID',
            hide: true
        });
        columns.push({
            label: 'Credit Card',
            name: 'CreditCard',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[38]);
            }
        });
        columns.push({
            label: 'CMSID',
            name: 'CMSID',
            hide: true
        });
        columns.push({
            label: 'CMS',
            name: 'CMS',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, row[40]);
            }
        });

        columns.push({
            label: 'Total',
            name: 'TotalService'
        });

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
                onSubmit={handleSubmit}
                resetForm={ModelObj}
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'OperatorWise UserWise Report'}
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
                    callSelectData(currentPage, TakeCount, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    callSelectData(
                        TakeCount,
                        currentPage,
                        // SearchText,
                        column,
                        direction
                    );
                }}
            />
            {isOpenReport && (
                <CustomModalWithScroll
                    isOpen={isOpenReport}
                    title="Date Wise Operator Wise List"
                    width="100%"
                    onDismissCallback={OpenReportModalCallback}
                >
                    <SelectReportByServiceByID
                        reportData={reportData}
                        onDismissCallback={OpenReportModalCallback}
                    />
                </CustomModalWithScroll>
            )}
        </CustomContainer>
    );
};
