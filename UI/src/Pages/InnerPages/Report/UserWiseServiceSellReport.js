import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import { SelectUserWiseOperatorWiseSellReport as TRANSACTION_FILTER } from 'Components/Api/OperatorWiseUserWiseReportApi';
import { Container, Grid, Accordion, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { ExportTypeItems } from 'Components/ReportHelper';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import {
    ADMIN_CODE,
    API_USER_CODE,
    RETAILER_CODE,
    CLIENT_CODE
} from 'Components/Helper/Messages';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import { SelectRoleWithoutSupport as USER_TYPE_SELECT_WITHOUT_SUPPORT } from 'Components/Api/UserTypeApi';
import { SelectDownlineForLedger as SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectReportByServiceByID } from 'Pages/InnerPages/Report/SelectReportByServiceByID';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const UserWiseServiceSellReport = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [openFilter, setopenFilter] = useState(true);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [transactions, setTransactions] = useState([]);
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
    const [isRetailer, setIsRetailerRole] = useState(true);
    const [userTypeList, setUserTypeList] = useState([]);
    const [downline, setDownline] = useState([]);
    const [SearchText] = useState('');
    var currentUsertypeid = localStorage.getItem('UserTypeID');

    const [userAllDefault] = useState({
        UserID: 0,
        FirmName: 'All'
    });

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });
    const [ModelObj] = useState({
        Status: 'ALL',
        ExportType: 'Select Download',
        User: userAllDefault
    });
    const today = new Date();
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);
        createColumns();
        await callSelectData(TakeCount, SkipCount, SearchText);
        setSubmitting(false);
    };
    const SelectUser = async (values) => {
        if (isRetailer === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setDownline(objWithSelectAll);
            return;
        }

        setLoading(true);
        var request = JSON.stringify(values);

        const userResponse = await SELECT_FOR_DROPDOWN(request);
        setLoading(false);
        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }

        var data = JSON.parse(userResponse.data);

        let objWithSelectAll = [userAllDefault, ...data];

        setDownline(objWithSelectAll);
    };
    const callSelectData = async (TakeCount, SkipCount, SearchText) => {
        let inputObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            UserID: formRef.current.values.User.UserID,
            UserTypeID: formRef.current.values.UserTypeID,
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

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }

    const OnChangeUserRole = (name, value, setFieldValue) => {
        setDownline([]);
        var jsonObj = {
            FirmName: '',
            UserTypeID: value
        };
        SelectUser(jsonObj);
    };

    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };
    const SelectReport = async (row, SelectDate, ServiceID) => {
        const FillerDays = new Date(today);
        if (SelectDate == 'Past') {
            FillerDays.setDate(FillerDays.getDate() - 1);
        }
        setReportData({
            ServiceID: ServiceID,
            UserID: row[0],
            FromDate: FillerDays,
            ToDate: FillerDays
        });
        setIsOpenReport(true);
    };
    useEffect(() => {
        CheckUserRole();
        refreshForm();
        SelectUser();
        selectUserTypes();
    }, []);

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
        if (RETAILER_CODE === parseInt(currentUsertypeid))
            setIsRetailerRole(false);
        if (API_USER_CODE === parseInt(currentUsertypeid))
            setIsRetailerRole(false);
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
    const OpenReportModalCallback = (value) => {
        setIsOpenReport(value);
    };

    const createColumns = () => {
        var columns = [];
        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });
        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });
        columns.push({
            label: 'Role',
            name: 'Role'
        });
        columns.push({
            label: 'MobileSaleID',
            name: 'MobileSaleID',
            hide: true
        });
        columns.push({
            label: 'Y-Mobile',
            name: 'YesterdayMobileSale',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[3]);
            }
        });
        columns.push({
            label: 'T-Mobile',
            name: 'TodayMobileSale',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[3]);
            }
        });

        columns.push({
            label: 'DTHSaleID',
            name: 'DTHSaleID',
            hide: true
        });
        columns.push({
            label: 'Y-DTH',
            name: 'YesterdayDTHSale',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[6]);
            }
        });
        columns.push({
            label: 'T-DTH',
            name: 'TodayDTHSale',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[6]);
            }
        });

        columns.push({
            label: 'PostPaidID',
            name: 'PostPaidID',
            hide: true
        });
        columns.push({
            label: 'Y-Post Paid',
            name: 'YesterdayPostPaid',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[9]);
            }
        });
        columns.push({
            label: 'T-Post Paid',
            name: 'TodayPostPaid',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[9]);
            }
        });
        columns.push({
            label: 'ElectricityID',
            name: 'ElectricityID',
            hide: true
        });
        columns.push({
            label: 'Y-Electricity',
            name: 'YesterdayElectricity',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[12]);
            }
        });
        columns.push({
            label: 'T-Electricity',
            name: 'TodayElectricity',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[12]);
            }
        });
        columns.push({
            label: 'MoneyTransferID',
            name: 'MoneyTransferID',
            hide: true
        });
        columns.push({
            label: 'Y-DMT',
            name: 'YesterdayMoneyTransfer',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[15]);
            }
        });
        columns.push({
            label: 'T-DMT',
            name: 'TodayMoneyTransfer',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[15]);
            }
        });
        columns.push({
            label: 'ExpressMoneyTransferID',
            name: 'ExpressMoneyTransferID',
            hide: true
        });
        columns.push({
            label: 'Y-Express-DMT',
            name: 'YesterdayExpressMoneyTransfer',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[18]);
            }
        });
        columns.push({
            label: 'T-Express-DMT',
            name: 'TodayExpressMoneyTransfer',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[18]);
            }
        });
        columns.push({
            label: 'AepsSaleID',
            name: 'AepsSaleID',
            hide: true
        });
        columns.push({
            label: 'Y-AEPS',
            name: 'YesterdayAepsSale',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[21]);
            }
        });
        columns.push({
            label: 'T-AEPS',
            name: 'TodayAepsSale',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[21]);
            }
        });
        columns.push({
            label: 'PaymentGatewaySaleID',
            name: 'PaymentGatewaySaleID',
            hide: true
        });
        columns.push({
            label: 'Y-PG',
            name: 'YesterdayPaymentGatewaySale',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[24]);
            }
        });
        columns.push({
            label: 'T-PG',
            name: 'TodayPaymentGatewaySale',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[24]);
            }
        });
        columns.push({
            label: 'MAtmID',
            name: 'MAtmID',
            hide: true
        });
        columns.push({
            label: 'Y-M.ATM',
            name: 'YesterdayMAtm',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[27]);
            }
        });
        columns.push({
            label: 'T-M.ATM',
            name: 'TodayMAtm',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[27]);
            }
        });
        columns.push({
            label: 'POSID',
            name: 'POSID',
            hide: true
        });
        columns.push({
            label: 'Y-POS',
            name: 'YesterdayPOS',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[30]);
            }
        });
        columns.push({
            label: 'T-POS',
            name: 'TodayPOS',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[30]);
            }
        });
        columns.push({
            label: 'PayoutID',
            name: 'PayoutID',
            hide: true
        });
        columns.push({
            label: 'Y-Payout',
            name: 'YesterdayPayout',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[33]);
            }
        });
        columns.push({
            label: 'T-Payout',
            name: 'TodayPayout',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[33]);
            }
        });
        columns.push({
            label: 'UPITransferID',
            name: 'UPITransferID',
            hide: true
        });
        columns.push({
            label: 'Y-UPI',
            name: 'YesterdayUPITransfer',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[36]);
            }
        });
        columns.push({
            label: 'T-UPI',
            name: 'TodayUPITransfer',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[36]);
            }
        });
        columns.push({
            label: 'FasTagID',
            name: 'FasTagID',
            hide: true
        });
        columns.push({
            label: 'Y-FastTag',
            name: 'YesterdayFasTag',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[39]);
            }
        });
        columns.push({
            label: 'T-FastTag',
            name: 'TodayFasTag',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[39]);
            }
        });

        columns.push({
            label: 'BroadbandID',
            name: 'BroadbandID',
            hide: true
        });
        columns.push({
            label: 'Y-Broadband',
            name: 'YesterdayBroadband',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[42]);
            }
        });
        columns.push({
            label: 'T-Broadband',
            name: 'TodayBroadband',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[42]);
            }
        });
        columns.push({
            label: 'PipedGasID',
            name: 'PipedGasID',
            hide: true
        });
        columns.push({
            label: 'Y-Piped-Gas',
            name: 'YesterdayPipedGas',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[45]);
            }
        });
        columns.push({
            label: 'T-Piped-Gas',
            name: 'TodayPipedGas',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[45]);
            }
        });
        columns.push({
            label: 'CreditCardID',
            name: 'CreditCardID',
            hide: true
        });
        columns.push({
            label: 'Y-Credit Card',
            name: 'YesterdayCreditCard',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[48]);
            }
        });
        columns.push({
            label: 'T-Credit Card',
            name: 'TodayCreditCard',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[48]);
            }
        });
        columns.push({
            label: 'CMSID',
            name: 'CMSID',
            hide: true
        });
        columns.push({
            label: 'Y-CMS',
            name: 'YesterdayCMS',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Past', row[51]);
            }
        });
        columns.push({
            label: 'T-CMS',
            name: 'TodayCMS',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row, 'Today', row[51]);
            }
        });

        columns.push({
            label: 'Y - Total',
            name: 'PastTotalService'
        });

        columns.push({
            label: 'T - Total',
            name: 'TodayTotalService'
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
                //validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={ModelObj}
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={
                                        'UserWise  OperatorWise  Sale Report'
                                    }
                                    open={openFilter}
                                    setOpen={(value) => setopenFilter(value)}
                                >
                                    <Grid container spacing={2}>
                                        {isRetailer && (
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
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        OnChangeUserRole(
                                                            name,
                                                            value,
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    <MenuItem key="0" value="0">
                                                        All Role
                                                    </MenuItem>
                                                    {userTypeList.map(
                                                        (item) => (
                                                            <MenuItem
                                                                key={
                                                                    item.UserTypeID
                                                                }
                                                                value={
                                                                    item.UserTypeID
                                                                }
                                                            >
                                                                {item.UserType}
                                                            </MenuItem>
                                                        )
                                                    )}
                                                </MaterialSelect>
                                            </Grid>
                                        )}

                                        {isRetailer && (
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
                                                        label: 'User'
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
                                        )}

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
                        SearchText,
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
