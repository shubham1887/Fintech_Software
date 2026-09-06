import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, Grid, Button, MenuItem } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import Box from '@material-ui/core/Box';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { Select as PROVIDER_SELECT } from 'Components/Api/ProviderApi';
import { SelectAllOperator as OPERATOR_SELECT } from 'Components/Api/OperatorsApi';
import excel from 'Assets/Images/excel.svg';
import Icon from '@material-ui/core/Icon';
import { SelectProviderDaybook as PROVIDER_DAYBOOK } from 'Components/Api/DaybooReportAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const ProviderDayBookReport = () => {
    const [list, setList] = useState([]);
    const formRef = useRef();
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [Providers, setProviders] = useState([]);
    const [Operators, setOperators] = useState([]);
    const [openFilter, setopenFilter] = useState(true);

    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };

    useEffect(() => {
        selectProviders();
        selectOperator();
        setLoadingOpen(false);
        createColumns();
        refreshForm();
    }, []);
    const providerDefault = {
        ProviderID: 0,
        ProviderName: 'All'
    };
    const [formobj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ProviderID: 0,
        OperatorID: 0,
        IsExcelDownload: false
    });

    const selectProviders = async () => {
        const response = await PROVIDER_SELECT();
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) return;
        var data = JSON.parse(response.data);

        let objWithSelectAll = [providerDefault, ...data];
        setProviders(objWithSelectAll);
    };

    const selectOperator = async () => {
        const response = await OPERATOR_SELECT(
            JSON.stringify({ ServiceID: 0 })
        );
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) return;

        var data = JSON.parse(response.data);
        setOperators(data);
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);

        if (formRef.current.values.IsExcelDownload === false) setList();
        var requestObj = JSON.stringify(values);
        setResponseMessage('');
        setIsError(false);
        setLoadingOpen(true);

        var response = await PROVIDER_DAYBOOK(requestObj);
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
        if (data.IsExcelDownload) {
            formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'Provider Daybook');
            return;
        }

        setList(data.list);
        setSubmitting(false);
    };
    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
        document.removeChild(a);
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
        // ProviderID: Yup.string()
        //     .notOneOf(['0'], PLEASE_SELECT_VALID + 'Provider')
        //     .required(PLEASE_SELECT_VALID + 'Provider')
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
            label: 'API',
            name: 'ProviderName'
        });
        columns.push({
            label: 'Operator',
            name: 'Operator'
        });

        columns.push({
            label: 'Total',
            name: 'TotalHits',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'T Amt',
            name: 'TotalAmount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'Success',
            name: 'SuccessHits',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'S Amt',
            name: 'SuccessAmount',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Failed',
            name: 'FailedHits',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'F Amt',
            name: 'FailedAmount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'Pending',
            name: 'PendingHits',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'P Amt',
            name: 'PendingAmount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'Admin.Comm',
            name: 'AdminCommission',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'API.Comm',
            name: 'ProviderCommission',
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
            label: 'Per(%)',
            name: 'Percentage',
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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={formobj}
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={formobj}
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Provider Daybook'}
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
                                                label="Operator"
                                                name="OperatorID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    ALL
                                                </MenuItem>
                                                {Operators.map((Item) => (
                                                    <MenuItem
                                                        key={Item.OperatorID}
                                                        value={Item.OperatorID}
                                                    >
                                                        {Item.OperatorName}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>

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
                                                    disabled={loadingOpen}
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
            <ReactTable
                data={list}
                columns={mainColumns}
                rowsPerPage={10}
                headerToolbar={false}
            />
        </CustomContainer>
    );
};
