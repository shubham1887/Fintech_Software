import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectCallbackLog as SELECT_APIUSER_CALLBACK_LOG } from 'Components/Api/LogAPI';
import { SelectInitialLog as SELECT_APIUSER_INITIAL_LOG } from 'Components/Api/LogAPI';
import { SelectRetailerAndAPIUserForDropDown as APIUSER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import ReactTable from 'Components/ReactTable/ReactTable';

import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { PLEASE_SELECT_VALID, API_USER_CODE } from 'Components/Helper/Messages';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';

const CustomContainer = HOCLoader(Container);

export const APIUserInitialLog = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [downline, setDownline] = useState([]);
    const [InitialLog, setInitialLog] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [callbackColumns, setcallbackColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [CallbacktotalCount, setCallbackTotalCount] = useState(0);

    const [currentPage, setCurrentPage] = useState(0);

    const [CallbackLog, setCallbackLog] = useState([]);
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState(false);
    const [isAPIUserRole, setIsAPIUserRole] = useState(false);
    const [SearchText, setSearchText] = useState('');
    var currentUsertypeid = localStorage.getItem('UserTypeID');

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });
    const [formObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        User: userDefault,
        ServiceID: 0,
        IsExcelDownload: false,
        Criteria: 0,
        CriteriaText: '',
        APIUserInital: false,
        APIUserCallback: false
    });
    const OnChangeCriteria = (value) => {
        if (value === undefined || value <= 0) {
            setIsDisplayCriteria(false);
            setCriteriaTextLabel('');
            return;
        }
        setCriteriaTextLabel(value);
        setIsDisplayCriteria(true);
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
        selectUsers(jsonObj);
    };

    const selectUsers = async () => {
        var request = {
            UserTypeID: API_USER_CODE
        };
        var json = JSON.stringify(request);

        const response = await APIUSER_SELECT_FOR_DROPDOWN(json);
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        let objWithSelectAll = [userDefault, ...data];

        setDownline(objWithSelectAll);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        await callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
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
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            UserID: formRef.current.values.User.UserID,
            IsExcelDownload: formRef.current.values.IsExcelDownload,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText
        };

        //if (formRef.current.values.IsExcelDownload === false) setInitialLog();

        if (formRef.current.values.IsExcelDownload === false) setCallbackLog();

        var requestObj = JSON.stringify(inputObj);
        if (formRef.current.values.APIUserInital) {
            setIsError(false);
            setLoading(true);
            setInitialLog([]);
            var response = await SELECT_APIUSER_INITIAL_LOG(requestObj);
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
                downloadFile(data.ReportPath, 'APIUser InitialLog');
            }
            if (SkipCount <= 0) {
                setTotalCount(data.Count);
            }
            setInitialLog(data.list);
            formRef.current.values.APIUserInital = false;
            return;
        }
        if (formRef.current.values.APIUserCallback) {
            setIsError(false);
            setLoading(true);
            // setCallbackLog([]);
            var responseCallback = await SELECT_APIUSER_CALLBACK_LOG(
                requestObj
            );
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
                downloadFile(dataRespo.ReportPath, 'APIUser CallbackLog');
            }
            if (SkipCount <= 0) {
                setCallbackTotalCount(dataRespo.Count);
            }
            setCallbackLog(dataRespo.list);
            formRef.current.values.APIUserCallback = false;
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
    const CheckUserRole = () => {
        if (API_USER_CODE === parseInt(currentUsertypeid)) {
            setIsAPIUserRole(true);
        }
    };

    useEffect(() => {
        CheckUserRole();
        createColumns();
        createCallbackColumns();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        if (isAPIUserRole === false) {
            columns.push({
                label: 'Name',
                name: 'FirmName'
            });
        }
        columns.push({
            label: 'ConsumerNumber',
            name: 'ConsumerNumber'
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
            name: 'ID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        if (isAPIUserRole === false) {
            columns.push({
                label: 'Name',
                name: 'FirmName'
            });
        }
        columns.push({
            label: 'ConsumerNumber',
            name: 'ConsumerNumber'
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
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
                render={({ values, setFieldValue, handleSubmit }) => {
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
                                        {!isAPIUserRole ? (
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
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Selection Criteria"
                                                name="Criteria"
                                                title="criteria"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnChangeCriteria(value);
                                                }}
                                            >
                                                <MenuItem key={0} value={0}>
                                                    Selection Criteria
                                                </MenuItem>
                                                <MenuItem
                                                    key={2}
                                                    value={'ConsumerNumber'}
                                                >
                                                    ConsumerNumber
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>

                                        {!IsDisplayCriteria ? null : (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialTextField
                                                    label={CriteriaTextLabel}
                                                    name="CriteriaText"
                                                    type="text"
                                                    placeholder={
                                                        'Enter ' +
                                                        CriteriaTextLabel
                                                    }
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

                                                        setFieldValue(
                                                            'APIUserInital',
                                                            true
                                                        );

                                                        setFieldValue(
                                                            'APIUserCallback',
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
                                                        title="Initial log"
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
                                                        'APIUserInital',
                                                        true
                                                    );
                                                    setFieldValue(
                                                        'APIUserCallback',
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

                                                        setFieldValue(
                                                            'APIUserInital',
                                                            false
                                                        );

                                                        setFieldValue(
                                                            'APIUserCallback',
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
                                                        title="Callback log"
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
                                                        'APIUserCallback',
                                                        true
                                                    );

                                                    setFieldValue(
                                                        'APIUserInital',
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
                    formRef.current.values.APIUserInital = true;
                    callSelectData(
                        RowsPerPage,
                        RowsPerPage * currentPage,
                        '',
                        '',
                        ''
                    );
                }}
                getSearchCallback={({ SearchText }) => {
                    callSelectData(TakeCount, 0, SearchText, '', '');
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
                    formRef.current.values.APIUserCallback = true;
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
        </CustomContainer>
    );
};
