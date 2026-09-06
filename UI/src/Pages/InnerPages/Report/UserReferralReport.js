import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { Select as TRANSACTION_FILTER } from 'Components/Api/UserReferralAPI';
import { Container, Grid, Box, Accordion } from '@material-ui/core';
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

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const UserReferralReport = () => {
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
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date().addDays(-15),
        Status: 'ALL',
        ExportType: 'Select Download'
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);
        createColumns();
        await callSelectData(TakeCount, SkipCount);
        setSubmitting(false);
    };

    const callSelectData = async (TakeCount, SkipCount) => {
        let inputObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
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
            downloadFile(data.ReportPath, 'ReferralRegistration_Report');
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

    useEffect(() => {
        CheckUserRole();
        refreshForm();
    }, []);

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ReferralRegistrationID',
            srno: true
        });

        if (isAdminRole === true) {
            columns.push({
                label: 'Referred User',
                name: 'ReferredUser'
            });
        }
        columns.push({
            label: 'Referral User',
            name: 'ReferralUser'
        });

        columns.push({
            label: 'Referral Amount',
            name: 'ReferralAmount',
            textwrapwidth: 60,
            numeric: true
        });
        columns.push({
            label: 'Benefit Utilize',
            name: 'IsBenefitUtilizeStr',
            customStatus: true
        });

        columns.push({
            label: 'Created Date',
            name: 'CreatedDOC'
        });
        if (isAdminRole === true) {
            columns.push({
                label: 'Transfer Date',
                name: 'BenefitUtilizedDOC'
            });
        }

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
                                    title={'User Referral Report'}
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
        </CustomContainer>
    );
};
