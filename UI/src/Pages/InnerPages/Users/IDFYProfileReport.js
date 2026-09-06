import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';

import { Container, Grid, Paper, Icon, Box } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import excel from 'Assets/Images/excel.svg';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import {
    StatusItems

} from 'Components/ReportHelper';

import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { SelectReport as SELECT } from 'Components/Api/SignupAPI';

import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const IDFYProfileReport = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [openFilter, setopenFilter] = useState(true);
    const [totalCount, setTotalCount] = useState(0);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [SearchText, setSearchText] = useState('');

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        Status: 'ALL',
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);
        createColumns();
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
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            IsExcelDownload: formRef.current.values.IsExcelDownload,
            Status: formRef.current.values.Status,
        };

        setIsError(false);
        setLoading(true);

        var requestJSON = JSON.stringify(requestObj);

        var response = await SELECT(requestJSON);

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
            downloadFile(data.ReportPath, 'Users ');
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
        document.removeChild(a);
    }

    const SelectDatewsie = async () => {
        let jsonObj = {
            ToDate: new Date(),
            FromDate: new Date(),
            TakeCount: TakeCount,
            SkipCount: SkipCount
        };

        var requestObj = JSON.stringify(jsonObj);

        setIsError(false);
        setLoading(true);
        createColumns();
        setTransactions([]);
        var response = await SELECT(requestObj);
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
        if (SkipCount <= 0) {
            setTotalCount(data.Count);
        }
        setTransactions(data.list);
    };

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

    useEffect(() => {
        SelectDatewsie();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        columns.push({
            label: 'Mobile Number',
            name: 'MobileNumber'
        });

        columns.push({
            label: 'Pan Number',
            name: 'PanNo'
        });

        columns.push({
            label: 'Pin Code',
            name: 'PinCode'
        });
        columns.push({
            label: 'Profile ID',
            name: 'ProfileID'
        });

        columns.push({
            label: 'IDFY Redirect URL',
            name: 'RedirectURL'
        });

        columns.push({
            label: 'Referral Code',
            name: 'ReferralCode'
        });
        columns.push({
            label: 'Employee ID',
            name: 'EmployeeID'
        });

        columns.push({
            label: 'Status',
            name: 'Status'
        });


        setMainColumns(columns);
    };

    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
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
                onSubmit={submithandle}
                resetForm={ModelObj}
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'IDFY Onboarding'}
                                        open={openFilter}
                                        setOpen={(value) =>
                                            setopenFilter(value)
                                        }
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
                                                <MaterialSelect
                                                    label="Status"
                                                    name="Status"
                                                    title="Status"
                                                >
                                                    {StatusItems}
                                                </MaterialSelect>
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
                                                    title="submit"
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
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={1}
                                                lg={1}
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
                                        </Grid>
                                    </CustomAccordion>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>

            {/* <ReactTable data={transactions} count={100} columns={mainColumns} /> */}

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
                        '',
                        column,
                        direction
                    );
                }}
            />
        </CustomContainer>
    );
};
