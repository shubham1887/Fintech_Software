import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectUserwiseAccounting as SELECT_USER_WISE_ACCOUNTING } from 'Components/Api/TransactionReportAPI';
import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
import { SelectDownlineForLedger as RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { Container, Grid, MenuItem, Paper } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectReportByOldAmountAdjustment } from './SelectReportByOldAmountAdjustment';
import { SelectReportByNewAmountAdjustment } from './SelectReportByNewAmountAdjustment';
import { SelectManualUpdateReport } from './ManualUpdateReport';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import { ExportTypeItems } from 'Components/ReportHelper';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
const CustomContainer = HOCLoader(Container);

const UserwiseAccounting = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [downLine, setDownLine] = useState([]);

    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [wallets, setWallets] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [TakeCount, setTakeCount] = useState(500);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);

    const [currentPage, setCurrentPage] = useState(0);

    const [SearchText, setSearchText] = useState('');
    const [isOpenReport, setIsOpenReport] = useState(false);
    const [isOpenNewAmtAdjReport, setIsOpenNewAmtAdjReport] = useState(false);
    const [isOpenManualUpdateReport, setIsOpenManualUpdateReport] = useState(false);
    const [ReportData, setReportData] = useState({});
    const [isNewAmtAdjReport, setNewAmtAdjReport] = useState({});
    const [isManualUpdateReport, setManualUpdateReport] = useState({});

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });
    const [formObj] = useState({
        User: userDefault,
        Date: new Date(),
        WalletID: 0,
        ExportType: 'Select Download'
    });


    const selectWallets = async () => {
        var request = {
            ID: 0
        };
        var json = JSON.stringify(request);
        const response = await WALLET_SELECT(json);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setWallets(data);
    };
    const WalletList = wallets.map((item) => (
        <MenuItem key={item.WalletID} value={item.WalletID}>
            {item.Name}
        </MenuItem>
    ));

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
            FirmName: text,
        };
        selectUsers(jsonObj);
    };

    const selectUsers = async (values) => {
        var request = JSON.stringify(values);
        const {
            isAuthorized,
            status,
            data
        } = await RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN(request);
        if (!isAuthorized) LOGOUT();
        if (!status) return;
        var d = JSON.parse(data);
        let objWithSelectAll = [userDefault, ...d];

        setDownLine(objWithSelectAll);
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
            SearchText: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            Date: formRef.current.values.Date,
            WalletID: formRef.current.values.WalletID,
            UserID: formRef.current.values.User.UserID,
            ExportType: formRef.current.values.ExportType,
            IsPDFDownload: formRef.current.values.IsPDFDownload
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await SELECT_USER_WISE_ACCOUNTING(requestObj);

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
            downloadFile(data.ReportPath, 'User Sale');
            return;
        }

        setItemList(data.list);
        setTotalCount(data.Count);
    };

    const submithandle = async (values, { setSubmitting }) => {
        setSubmitting(true);
        createColumns();

        await callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }

    const validationSchema = Yup.object({
        Date: Yup.date()
            .nullable(PLEASE_SELECT_VALID + ' date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'date')
            .typeError(PLEASE_SELECT_VALID + 'date'),

        WalletID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'wallet')
            .required(PLEASE_SELECT_VALID + 'wallet')
    });

    useEffect(() => {
        selectWallets();
        createColumns();
        selectUsers();
    }, []);

    const SelectReport = async (row) => {

        setReportData({ UserID: row[1], FirmName: row[2], FillerDate: formRef.current.values.Date , WalletID: formRef.current.values.WalletID});
        setIsOpenReport(true);
    };
    const SelectForFilterByNewAmtAdj = async (row) => {

        setNewAmtAdjReport({ UserID: row[1], FirmName: row[2], FillerDate: formRef.current.values.Date, WalletID: formRef.current.values.WalletID });
        setIsOpenNewAmtAdjReport(true);
    };

    const SelectbyManualUpdateReport = async (row) => {

        setManualUpdateReport({ UserID: row[1], FirmName: row[2], FillerDate: formRef.current.values.Date , WalletID: formRef.current.values.WalletID });
        setIsOpenManualUpdateReport(true);
    };
    const OpenReportModalCallback = (value) => {
        setIsOpenReport(value);
    };
    const OpenNewAmtAdjReportModalCallback = (value) => {
        setIsOpenNewAmtAdjReport(value);
    };
    const OpenManualUpdateReportModalCallback = (value) => {
        setIsOpenManualUpdateReport(value);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });
        columns.push({
            label: 'UserID',
            name: 'UserID',
            hide: true
        });

        columns.push({
            label: 'Name',
            name: 'FirmName'
        });
        columns.push({
            label: 'UserMobile',
            name: 'UserMobile',

        });

        columns.push({
            label: 'Role',
            name: 'Role'
        });
        columns.push({
            label: 'Wallet',
            name: 'WalletName'
        });

        columns.push({
            label: 'OpeningBal\nA',
            name: 'OpeningBal',

        });
        columns.push({
            label: 'Fund Transfer\nB',
            name: 'Purchase',

        });
        // columns.push({
        //     label: 'Manual Pass Fail',
        //     name: 'ManualPassFail',
        //     numeric: true
        // });
        columns.push({
            label: 'Old Amt Adj\n          C',
            name: 'TotalOldAmountAdjustment',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row);
            }
        });


        columns.push({
            label: 'New Amt Adj\n         D',
            name: 'TotalNewAmountAdjustment',
            hiddenbutton: true,
            callback: (row) => {
                SelectForFilterByNewAmtAdj(row);
            }
        });
        columns.push({
            label: 'Total Cr Amt\nP = A+B+C-D',
            name: 'TotalCrAmt',

        })
        columns.push({
            label: 'Sale\nE',
            name: 'Sale',

        });


        columns.push({
            label: 'Txn Comm\nF',
            name: 'CommsionAmount',

        })
        columns.push({
            label: 'Txn Surcharge\nG',
            name: 'Surcharge',

        })

        columns.push({
            label: 'Total Dr Amt\nQ = E-F-(-G)',
            name: 'TotalDrAmt',

        })
        columns.push({
            label: 'Manual Update',
            name: 'ManualUpdateCount',
            hiddenbutton: true,
            callback: (row) => {
                SelectbyManualUpdateReport(row);
            }
        });
        columns.push({
            label: 'ClosingBal',
            name: 'ClosingBalance',

        });
        columns.push({
            label: 'ExpClosingBal\n R =(P-Q)',
            name: 'ExpClosingBal',

        });
        columns.push({
            label: 'Diff',
            name: 'Difference',

        });

        setMainColumns(columns);
    };
    const UserWiseSaleReport = (
        <ReactTable
            data={itemList}
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
    );
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
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
                render={({ values }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
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
                                            <MaterialDatePickerField
                                                name="Date"
                                                label="Date"
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
                                            <MaterialSelect
                                                label="Wallet"
                                                name="WalletID"
                                                title="Select Wallet"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Wallet
                                                </MenuItem>
                                                {WalletList}
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
                                                defaultValue={userDefault}
                                                FieldName="User"
                                                name="User"
                                                SubFieldName="UserID"
                                                options={downLine}
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
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />

            <h3 className={classes.PaperHead_h2}>UserWise Accounting</h3>
            {UserWiseSaleReport}

            {isOpenNewAmtAdjReport && (
                <CustomModalWithScroll
                    isOpen={isOpenNewAmtAdjReport}
                    title={`New Amt Adj - { ${isNewAmtAdjReport.FirmName} } Date -{ ${isNewAmtAdjReport.FillerDate} }`}
                    width="100%"
                    onDismissCallback={OpenNewAmtAdjReportModalCallback}
                >
                    <SelectReportByNewAmountAdjustment
                        ReportData={isNewAmtAdjReport}
                        onDismissCallback={OpenNewAmtAdjReportModalCallback}
                    />
                </CustomModalWithScroll>
            )};
            {isOpenReport && (
                <CustomModalWithScroll
                    isOpen={isOpenReport}
                    title={`Old Amt Adj - { ${ReportData.FirmName} } Date -{ ${ReportData.FillerDate} }`}
                    width="100%"
                    onDismissCallback={OpenReportModalCallback}
                >
                    <SelectReportByOldAmountAdjustment
                        ReportData={ReportData}
                        onDismissCallback={OpenReportModalCallback}
                    />
                </CustomModalWithScroll>
            )};

            {isOpenManualUpdateReport && (
                <CustomModalWithScroll
                    isOpen={isOpenManualUpdateReport}
                    title={`Manual Update Report- { ${isManualUpdateReport.FirmName} } Date -{ ${isManualUpdateReport.FillerDate} }`}
                    width="100%"
                    onDismissCallback={OpenManualUpdateReportModalCallback}
                >
                    <SelectManualUpdateReport
                        ReportData={isManualUpdateReport}
                        onDismissCallback={OpenManualUpdateReportModalCallback}
                    />
                </CustomModalWithScroll>

            )}


        </CustomContainer>
    );
};

export default UserwiseAccounting;
