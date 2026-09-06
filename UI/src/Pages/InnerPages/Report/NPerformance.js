import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import {
    Container,
    Grid,
    MenuItem,
    Icon,
    Box,
    Accordion,
    CircularProgress
} from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import excel from 'Assets/Images/excel.svg';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';

import { SelectTopPerformer as SELECT_PERFORMER } from 'Components/Api/TransactionReportAPI';
import { SelectAll as SERVICE_SELECT_RECHARGE } from 'Components/Api/ServiceApi';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const NPerformance = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [openFilter, setOpenFilter] = useState(true);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [transactions, setTransactions] = useState([]);
    const [TakeCount, setTakeCount] = useState(1);

    const [Services, setServices] = useState([]);

    const TopPerformance = [1, 5, 10, 50, 100, 1000];
    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        TopPerformance: TakeCount,
        ServiceID: 0
    });

    useEffect(() => {
        createColumns();
        selectServices();
    }, []);

    const selectServices = async () => {
        setLoadingOpen(true);
        const { data, status, isAuthorized } = await SERVICE_SELECT_RECHARGE();
        setLoadingOpen(false);
        if (!isAuthorized) LOGOUT();
        if (!status) return;
        var d = JSON.parse(data);
        setServices(d);
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
            label: 'Email',
            name: 'EmailID'
        });
        // columns.push({
        //     label: 'Service',
        //     name: 'Service'
        // });

        columns.push({
            label: 'TotalSale',
            name: 'TotalSale',
            numeric: true,
            bold: true
        });

        setMainColumns(columns);
    };

    const validationSchema = Yup.object({
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + ' date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'from date')
            .typeError(PLEASE_SELECT_VALID + 'from date'),

        ToDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + ' date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'to date')
            .typeError(PLEASE_SELECT_VALID + 'to date')
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);

        var requestObj = JSON.stringify(values);
        setIsError(false);

        var response = await SELECT_PERFORMER(requestObj);

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
            downloadFile(data.ReportPath, 'Performance Report');
            return;
        }
        setTransactions(data.list);

        //resetForm();
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
    const PerformanceReport = (
        <ReactTable data={transactions} columns={mainColumns} />
    );

    const loadingProgress = loadingOpen && (
        <CircularProgress size={24} className={classes.buttonProgress} />
    );

    const ServiceItems = Services.map(({ ServiceID, ServiceName }) => (
        <MenuItem key={ServiceID} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));
    const ExcelImage = (
        <img alt="DownLoad Excel" className={classes.imageIcon} src={excel} />
    );
    const PerformanceForm = ({ setFieldValue, handleSubmit }) => {
        return (
            <Form style={{ marginBottom: '15px' }}>
                <Grid item xs={12}>
                    <CustomAccordion
                        title={'Performance Report'}
                        open={openFilter}
                        setOpen={(value) => setOpenFilter(value)}
                    >
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="Top Performance"
                                    name="TopPerformance"
                                    title="Top Performance"
                                    onCustomCallback={(name, value) => {
                                        setTakeCount(
                                            value === 1000 ? 10 : value
                                        );
                                    }}
                                >
                                    {TopPerformanceItems}
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialDatePickerField
                                    name="FromDate"
                                    label="From Date"
                                    maxDate={new Date()}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialDatePickerField
                                    name="ToDate"
                                    label="To Date"
                                    maxDate={new Date()}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="Services"
                                    name="ServiceID"
                                    title="ServiceName"
                                >
                                    <MenuItem key={0} value={0}>
                                        All
                                    </MenuItem>
                                    {ServiceItems}
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={1} lg={1}>
                                <Box style={{ textAlign: 'center' }}>
                                    <Icon
                                        className={
                                            classes.CustomImageButtonStyle
                                        }
                                        disabled={loadingOpen}
                                        classes={{ root: classes.iconRoot }}
                                        onClick={(e) => {
                                            setFieldValue(
                                                'IsExcelDownload',
                                                true
                                            );
                                            handleSubmit(e);
                                        }}
                                    >
                                        {ExcelImage}
                                    </Icon>
                                </Box>
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <PrimaryButton
                                    type="submit"
                                    title="submit"
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={loadingOpen}
                                    onClick={(e) => {
                                        setFieldValue('IsExcelDownload', false);
                                        handleSubmit(e);
                                    }}
                                >
                                    Search
                                </PrimaryButton>
                                {loadingProgress}
                            </Grid>
                        </Grid>
                    </CustomAccordion>
                </Grid>
                <Grid className={classes.footerGrid}>
                    <Box className={classes.flexReverse}>
                        <div className={classes.SubmitWrapper}></div>
                    </Box>
                </Grid>
            </Form>
        );
    };
    const customContainerCallback = (alertState) => {
        setIsError(alertState);
    };
    const TopPerformanceItems = TopPerformance.map((item) => {
        return (
            <MenuItem key={item} value={item}>
                {item}
            </MenuItem>
        );
    });
    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={customContainerCallback}
        >
            <Formik
                initialValues={ModelObj}
                innerRef={formRef}
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={ModelObj}
            >
                {PerformanceForm}
            </Formik>

            {PerformanceReport}
        </CustomContainer>
    );
};
