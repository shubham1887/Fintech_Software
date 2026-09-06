import React, { memo, useState, useRef, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import {
    Container,
    Grid,
    Accordion,
    Box,
    Icon,
    CircularProgress,
    MenuItem
} from '@material-ui/core';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import excel from 'Assets/Images/excel.svg';
import HOCLoader from 'Components/Loader/HOCLoader';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { LOGOUT } from 'ConstComponents';
import { SelectPastSuccessFail as PAST_SUCCESS_FAIL } from 'Components/Api/TransactionReportAPI';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { StatusItem } from 'Components/ReportHelper';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);
const PastSuccessFailReport = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [openFilter, setOpenFilter] = useState(true);
    const [transactions, setTransactions] = useState([]);
    const [totalCount, setTotalCount] = useState(0);
    const [mainColumns, setMainColumns] = useState([]);
    const [providerList, setProviderList] = useState([]);
    const openHandler = (value) => {
        setOpenFilter(value);
    };

    const ExcelImage = (
        <img alt="Excel Download" className={classes.imageIcon} src={excel} />
    );

    const [ModelObj] = useState({
        FromDate: new Date(),
        ProviderID: 0,
        Status: 'SUCCESS'
    });

    const validationSchema = Yup.object({
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'from date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'from date')
            .typeError(PLEASE_SELECT_VALID + 'from date')
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

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setLoading(true);

        var requestJSON = JSON.stringify(values);
        const { isAuthorized, status, data } = await PAST_SUCCESS_FAIL(
            requestJSON
        );
        setLoading(false);
        if (!isAuthorized) LOGOUT();
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);
        }
        var d = JSON.parse(data);

        setTransactions(d.list);
        setTotalCount(d.Count);
        setSubmitting(false);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            srno: true
        });

        columns.push({
            label: 'Name',
            name: 'FirmName'
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });

        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });
        columns.push({
            label: 'Number',
            name: 'ConsumerNumber'
        });
        columns.push({
            label: 'Amount',
            name: 'Amount'
        });
        columns.push({
            label: 'DebitAmount',
            name: 'DebitAmount'
        });
        columns.push({
            label: 'Transaction Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        columns.push({
            label: 'Updated Date',
            name: 'UpdatedDOC'
        });
        columns.push({
            label: 'ProcTime (in Min.)',
            name: 'ProcTime'
        });

        setMainColumns(columns);
    };

    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };

    useEffect(() => {
        createColumns();
        selectProvider();
        refreshForm();
    }, []);
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
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Past Success Fail Report'}
                                    open={openFilter}
                                    setOpen={openHandler}
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
                                                label="Status"
                                                name="Status"
                                                title="Status"
                                            >
                                                {StatusItem}
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
                                                    {ExcelImage}
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
            />
        </CustomContainer>
    );
};

export default memo(PastSuccessFailReport);
