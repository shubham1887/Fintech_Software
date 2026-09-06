import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import { Container, Grid, MenuItem } from '@material-ui/core';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { Select as PROVIDER_SELECT } from 'Components/Api/ProviderApi';
import { ProviderSale as PROVIDER_SALE } from 'Components/Api/TransactionReportAPI';
import { SelectByRechargeServices as SERVICE_SELECT_RECHARGE } from 'Components/Api/ServiceApi';
import { SelectByProviderID } from '../DayBookReport/SelectByProviderID';
import { FETCH_LIST } from 'Config';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { ExportTypeItems } from 'Components/ReportHelper';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const ProviderSale = () => {
    const formRef = useRef();
    const [list, setList] = useState([]);
    const [userList, setUserList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [Providers, setProviders] = useState([]);
    const [IsFetchAllList, setisFetchAllList] = useState(false);
    const [openFilter, setopenFilter] = useState(true);
    const [isOpenReport, setIsOpenReport] = useState(false);
    const [ReportData, setReportData] = useState({});
    const [Services, setServices] = useState([]);
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });
    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };
    useEffect(() => {
        selectProviders();
        selectServices();
        fetchUserList();
        setLoadingOpen(false);
        createColumns();
        refreshForm();
    }, []);

    const [formobj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ProviderID: 0,
        ServiceID: 0,
        User: {
            UserID: 0,
            FirmName: 'Select User'
        },
        ExportType: 'Select Download'
    });
    const fetchUserList = async () => {
        let listValue = await FETCH_LIST();

        if (listValue === undefined || listValue === null || listValue === '') {
            return;
        }

        if (listValue === 'All') {
            setisFetchAllList(true);
            SelectUser('');
            return;
        }
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
        setLoadingOpen(true);
        var request = JSON.stringify(values);
        setLoadingOpen(false);
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
    const selectServices = async () => {
        const response = await SERVICE_SELECT_RECHARGE();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };
    const selectProviders = async () => {
        const response = await PROVIDER_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);
        setProviders(data);
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoadingOpen(true);
        setList([]);
        values.UserID = values.User.UserID;
        var requestObj = JSON.stringify(values);
        setIsError(false);

        var response = await PROVIDER_SALE(requestObj);
        setLoadingOpen(false);

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
            downloadFile(data.ReportPath, 'Provider Sale');
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

    const SelectReport = async (row) => {
        if (row[2] === 'Total') return;
        setReportData({
            ProviderID: row[1],
            ProviderName: row[2],
            UserID: formRef.current.values.User.UserID,
            ToDate: formRef.current.values.ToDate,
            FromDate: formRef.current.values.FromDate
        });
        setIsOpenReport(true);
    };

    const OpenReportModalCallback = (value) => {
        setIsOpenReport(value);
    };
    const OpenReportModal = isOpenReport ? (
        <CustomModalWithScroll
            isOpen={isOpenReport}
            title={ReportData.ProviderName}
            width="75%"
            onDismissCallback={OpenReportModalCallback}
        >
            <SelectByProviderID
                ReportData={ReportData}
                onDismissCallback={OpenReportModalCallback}
            />
        </CustomModalWithScroll>
    ) : null;

    const createColumns = () => {
        var columns = [];
        var textwrapwidth = 60;

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });
        columns.push({
            label: 'ProviderID',
            name: 'ProviderID',
            hide: true
        });
        columns.push({
            label: 'API',
            name: 'ProvderName',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row);
            }
        });
        columns.push({
            label: 'Purchase',
            name: 'Purchase',
            numeric: true
        });

        columns.push({
            label: 'Sale',
            name: 'Sale',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Failed',
            name: 'Fail',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'R.Comm',
            name: 'RetailerCommission',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'D.Comm',
            name: 'DistributerCommission',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'S.Comm',
            name: 'SuperDistributerCommission',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Op.Bal',
            name: 'OpeningBalance',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Admin.comm',
            name: 'AdminCommission',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Cl.Bal',
            name: 'ClosingBalance',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Diff',
            name: 'Difference',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Profit',
            name: 'Profit',
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
                initialValues={formobj}
                innerRef={formRef}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formobj}
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
                                    title={'Provider Sale'}
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

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Provider"
                                                name="ProviderID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Providers
                                                </MenuItem>
                                                {Providers.map((Item) => (
                                                    <MenuItem
                                                        key={Item.ProviderID}
                                                        value={Item.ProviderID}
                                                    >
                                                        {Item.ProviderName}
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
                                                label="Service"
                                                name="ServiceID"
                                                title="Service"
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
                                                    callSearch(text);
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
            <ReactTable data={list} columns={mainColumns} rowsPerPage={25} />
            {OpenReportModal}
        </CustomContainer>
    );
};
