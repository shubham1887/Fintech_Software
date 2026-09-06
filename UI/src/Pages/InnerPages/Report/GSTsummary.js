import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Button,
    MenuItem,
    Icon,
    Box,
    CircularProgress,
    Accordion
} from '@material-ui/core';
import { GSTsummary as GST_SUMMARY_SELECT } from 'Components/Api/TransactionReportAPI';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { logout } from 'Components/Helper/Helpers';
import excel from 'Assets/Images/excel.svg';
import {
    GET_Month_FOR_GST,
    GET_YEAR_FOR_GST
} from 'Components/Helper/Messages';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const GSTSummary = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [tableItems, setTableItems] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [currentPage, setCurrentPage] = useState(0);

    const [openFilter, setOpenFilter] = useState(true);
    const [TakeCount, setTakeCount] = useState(100);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);

    const [Months] = useState([
        { Key: 1, Value: 'January' },
        { Key: 2, Value: 'February' },
        { Key: 3, Value: 'March' },
        { Key: 4, Value: 'April' },
        { Key: 5, Value: 'May' },
        { Key: 6, Value: 'June' },
        { Key: 7, Value: 'July' },
        { Key: 8, Value: 'August' },
        { Key: 9, Value: 'September' },
        { Key: 10, Value: 'October' },
        { Key: 11, Value: 'November' },
        { Key: 12, Value: 'December' }
    ]);
    const [Years, setYears] = useState([]);

    const [ModelObj] = useState({
        Month: GET_Month_FOR_GST(),
        Year: GET_YEAR_FOR_GST()
    });
    const GetYears = () => {
        var currentYear = new Date().getFullYear();
        var yearList = [];
        for (var year = currentYear; year >= 2000; year--) {
            yearList.push({ Key: year });
        }
        setYears(yearList);
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);

        setLoading(true);
        setTableItems([]);

        let requestJSON = JSON.stringify(values);

        const { data, status, isAuthorized } = await GST_SUMMARY_SELECT(
            requestJSON
        );

        setLoading(false);
        if (!isAuthorized) logout();
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);

            return;
        }

        var d = JSON.parse(data);

        setTableItems(d.list);
        setTotalCount(data.Count);
        if (d.IsExcelDownload) downloadFile(d.ReportPath, 'Recharge Report');
        setSubmitting(false);
    };

    function downloadFile(URL, Title) {
        let a1 = document.createElement('a');
        a1.href = URL;
        a1.setAttribute('download', Title);
        a1.setAttribute('target', '_blank');
        a1.click();
        document.removeChild(a1);
    }

    const validationSchema = Yup.object({
        Month: Yup.string()
            .nullable(PLEASE_SELECT_VALID + 'month')
            .required(PLEASE_SELECT_VALID + 'month'),
        Year: Yup.string()
            .nullable(PLEASE_SELECT_VALID + 'year')
            .required(PLEASE_SELECT_VALID + 'year')
    });

    useEffect(() => {
        GetYears();
        createColumns();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });
        columns.push({
            label: 'Month',
            name: 'InvoiceMonth'
        });

        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });

        columns.push({
            label: 'UserType',
            name: 'UserType'
        });
        columns.push({
            label: 'GSTNo',
            name: 'GSTNo'
        });
        columns.push({
            label: 'InvoiceNo',
            name: 'InvoiceNo'
        });

        columns.push({
            label: 'I.Type',
            name: 'InvoiceType'
        });
        columns.push({
            label: 'State',
            name: 'State'
        });
        columns.push({
            label: 'ReverseCharge',
            name: 'ReverseCharge'
        });
        columns.push({
            label: 'Rate',
            name: 'Rate',
            numeric: true
        });
        columns.push({
            label: 'InvoiceValue',
            name: 'InvoiceValue',
            numeric: true
        });
        columns.push({
            label: 'TaxableValue',
            name: 'TaxableValue',
            numeric: true
        });
        columns.push({
            label: 'IGST',
            name: 'IGST',
            numeric: true
        });
        columns.push({
            label: 'CGST',
            name: 'CGST',
            numeric: true
        });
        columns.push({
            label: 'SGST',
            name: 'SGST',
            numeric: true
        });

        setMainColumns(columns);
    };

    const MonthItems = Months.map(({ Key, Value }, index) => (
        <MenuItem key={index} value={Key}>
            {Value}
        </MenuItem>
    ));

    const YearItems = Years.map(({ Key }, index) => (
        <MenuItem key={index} value={Key}>
            {Key}
        </MenuItem>
    ));

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
                enableReinitialize={true}
                innerRef={formRef}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={ModelObj}
            >
                {({ setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'GST Summary'}
                                    open={openFilter}
                                    setOpen={(value) => setOpenFilter(value)}
                                >
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Month"
                                                name="Month"
                                                title="Month"
                                            >
                                                {MonthItems}
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
                                                label="Year"
                                                name="Year"
                                                title="Year"
                                            >
                                                {YearItems}
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
                                                    disabled={loading}
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
                                            md={2}
                                            lg={2}
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
                data={tableItems}
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                }}
                getSearchCallback={({ SearchText }) => {}}
                getSortCallback={({ column, direction }) => {}}
            />
        </CustomContainer>
    );
};
