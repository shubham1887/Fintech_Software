import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';

import { SelectByDate as SELECT_PROVDER_LOG } from 'Components/Api/ProviderReqestResponceApi';
import { SelectByDatev1 as SELECT_PROVDER_CALLBACKLOG } from 'Components/Api/ProviderReqestResponceApi';

import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';

import ReactTable from 'Components/ReactTable/ReactTable';

import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const ProviderLog = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [Services, setServices] = useState([]);

    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [providerList, setProviderList] = useState([]);
    const [openFilter, setopenFilter] = useState(true);
    const [CallbackLog, setCallbackLog] = useState([]);
    const [InitialLog, setInitialLog] = useState([]);
    const [SearchText, setSearchText] = useState('');
    const [CallbacktotalCount, setCallbackTotalCount] = useState(0);
    const [callbackColumns, setcallbackColumns] = useState([]);

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ProviderID: 0,
        ServiceID: 0
    });
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        //if (formRef.current.values.IsExcelDownload === false) setTransactions([]);
        setSubmitting(true);

        await callSelectData(TakeCount, SkipCount);
        setSubmitting(false);
    };

    const callSelectData = async (TakeCount, SkipCount, SearchText) => {
        let inputObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            ProviderID: formRef.current.values.ProviderID,
            ServiceID: formRef.current.values.ServiceID,
            IsExcelDownload: formRef.current.values.IsExcelDownload
        };

        if (formRef.current.values.IsExcelDownload === false) setCallbackLog();

        var requestObj = JSON.stringify(inputObj);
        if (formRef.current.values.ProviderInital) {
            setIsError(false);
            setLoading(true);
            setInitialLog([]);
            var response = await SELECT_PROVDER_LOG(requestObj);
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
            if (data.IsExcelDownload) {
                formRef.current.values.IsExcelDownload = false;
                downloadFile(data.ReportPath, 'Provider InitialLog');
            }
            if (SkipCount <= 0) {
                setTotalCount(data.Count);
            }
            setInitialLog(data.list);
            formRef.current.values.ProviderInital = false;
            return;
        }
        if (formRef.current.values.ProviderCallback) {
            setIsError(false);
            setLoading(true);
            var responseCallback = await SELECT_PROVDER_CALLBACKLOG(requestObj);
            setLoading(false);

            if (!responseCallback.isAuthorized) LOGOUT();

            setSeverity('success');

            if (!responseCallback.status) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage(responseCallback.data);

                return;
            }
            var dataRespo = JSON.parse(responseCallback.data);

            if (dataRespo.IsExcelDownload) {
                formRef.current.values.IsExcelDownload = false;
                downloadFile(dataRespo.ReportPath, 'Provider CallbackLog');
            }
            if (SkipCount <= 0) {
                setCallbackTotalCount(dataRespo.Count);
            }
            setCallbackLog(dataRespo.list);
            formRef.current.values.ProviderCallback = false;
            return;
        }
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
    });

    const selectProvider = async () => {
        setLoading(true);
        const response = await PROVIDER_DROP_DOWN();
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
    };
    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    useEffect(() => {
        selectProvider();
        selectServices();
        createColumns();
        createCallbackColumns();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'id',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });

        columns.push({
            label: 'Request',
            name: 'Request',
            ReadOnlyTextArea: true
        });
        columns.push({
            label: 'Response',
            name: 'Response',
            ReadOnlyTextArea: true
        });

        setMainColumns(columns);
    };

    const createCallbackColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'id',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });

        columns.push({
            label: 'ProviderReply',
            name: 'ProviderReply'
        });

        setcallbackColumns(columns);
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
                render={({
                    values,
                    setFieldValue,
                    handleChange,
                    handleSubmit
                }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'Provider log'}
                                        open={openFilter}
                                        setOpen={(value) =>
                                            setopenFilter(value)
                                        }
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
                                                    label="Select Provider"
                                                    name="ProviderID"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Provider
                                                    </MenuItem>
                                                    {providerList.map(
                                                        (provider) => (
                                                            <MenuItem
                                                                key={
                                                                    provider.ProviderID
                                                                }
                                                                value={
                                                                    provider.ProviderID
                                                                }
                                                            >
                                                                {
                                                                    provider.ProviderName
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
                                                <MaterialSelect
                                                    label="Service"
                                                    name="ServiceID"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Service
                                                    </MenuItem>
                                                    {Services.map((Item) => (
                                                        <MenuItem
                                                            key={Item.ServiceID}
                                                            value={
                                                                Item.ServiceID
                                                            }
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
                                                <Box
                                                    style={{
                                                        textAlign: 'center'
                                                    }}
                                                >
                                                    <Icon
                                                        className={
                                                            classes.CustomImageButtonStyle
                                                        }
                                                        disabled={loading}
                                                        classes={{
                                                            root:
                                                                classes.iconRoot
                                                        }}
                                                        onClick={(e) => {
                                                            setFieldValue(
                                                                'IsExcelDownload',
                                                                true
                                                            );

                                                            setFieldValue(
                                                                'ProviderInital',
                                                                true
                                                            );

                                                            setFieldValue(
                                                                'ProviderCallback',
                                                                false
                                                            );
                                                            handleSubmit(e);
                                                        }}
                                                    >
                                                        <img
                                                            alt=""
                                                            className={
                                                                classes.imageIcon
                                                            }
                                                            title="Provider Initial log"
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
                                                    title="Initial log"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'ProviderInital',
                                                            true
                                                        );
                                                        setFieldValue(
                                                            'ProviderCallback',
                                                            false
                                                        );
                                                        setFieldValue(
                                                            'IsExcelDownload',
                                                            false
                                                        );
                                                        handleSubmit(e);
                                                    }}
                                                    disabled={loading}
                                                >
                                                    Initial log
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

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <Box
                                                    style={{
                                                        textAlign: 'center'
                                                    }}
                                                >
                                                    <Icon
                                                        className={
                                                            classes.CustomImageButtonStyle
                                                        }
                                                        disabled={loading}
                                                        classes={{
                                                            root:
                                                                classes.iconRoot
                                                        }}
                                                        onClick={(e) => {
                                                            setFieldValue(
                                                                'IsExcelDownload',
                                                                true
                                                            );

                                                            setFieldValue(
                                                                'ProviderInital',
                                                                false
                                                            );

                                                            setFieldValue(
                                                                'ProviderCallback',
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
                                                            title="Provider Callback log"
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
                                                    title="Callback log"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'ProviderCallback',
                                                            true
                                                        );

                                                        setFieldValue(
                                                            'ProviderInital',
                                                            false
                                                        );
                                                        setFieldValue(
                                                            'IsExcelDownload',
                                                            false
                                                        );
                                                        handleSubmit(e);
                                                    }}
                                                    disabled={loading}
                                                />

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
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />

            <h3 className={classes.PaperHead_h2}>Initial log</h3>
            <ReactTable
                data={InitialLog}
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                    formRef.current.values.ProviderInital = true;
                    callSelectData(
                        RowsPerPage,
                        RowsPerPage * currentPage,
                        '',
                        '',
                        ''
                    );
                }}
                getSearchCallback={({ SearchText }) => {
                    formRef.current.values.ProviderInital = true;
                    callSelectData(TakeCount, SkipCount, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    formRef.current.values.ProviderInital = true;
                    callSelectData(
                        TakeCount,
                        currentPage,
                        SearchText,
                        column,
                        direction
                    );
                }}
            />
            <h3 className={classes.PaperHead_h2}>Callback log</h3>
            <ReactTable
                data={CallbackLog}
                columns={callbackColumns}
                count={CallbacktotalCount}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                    formRef.current.values.ProviderCallback = true;
                    callSelectData(
                        RowsPerPage,
                        RowsPerPage * currentPage,
                        '',
                        '',
                        ''
                    );
                }}
                getSearchCallback={({ SearchText }) => {
                    formRef.current.values.ProviderCallback = true;
                    callSelectData(TakeCount, SkipCount, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    formRef.current.values.ProviderCallback = true;
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
