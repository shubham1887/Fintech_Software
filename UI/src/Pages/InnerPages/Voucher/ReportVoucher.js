import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectReport as SELECT_REPORT } from 'Components/Api/VoucherAPI';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { SelectForDropdown as SELECT_VOUCHER_TYPES } from 'Components/Api/VoucherAPI';
import { HeaderVoucher } from "Components/SummaryCards/HeaderVoucher";
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { Reverse as REVERSE_REQUEST } from 'Components/Api/VoucherAPI';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const ReportVoucher = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [isAlertOpen, setIsAlertOpen] = useState(false);   
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [voucherTypes, setVoucherTypes] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState(0);
    const [isheaderSummary, setHeaderSummary] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [openFilter, setopenFilter] = useState(true);
    const [currentPage, setCurrentPage] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [VoucherDefault, setVoucherDefault] = useState({
        VoucherTypeID: 0,
        VoucherTypeName: 'Select Voucher'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
        IsExcelDownload: false,
        isReverse: "ACCEPT"
    });
    const selectVoucherType = async () => {
        const response = await SELECT_VOUCHER_TYPES();
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);
        let objWithSelectAll = [VoucherDefault, ...data];
        setVoucherTypes(objWithSelectAll);
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
            voucherTypeID: formRef.current.values.voucherTypes,
            isReverse: formRef.current.values.isReverse,
            IsExcelDownload: formRef.current.values.IsExcelDownload
        };
        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await SELECT_REPORT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        if (data.IsExcelDownload) {
            formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'Approval Voucher Report');
        }

        setTransactions(data.list);
        setHeaderSummary(data.VoucherSummary);
        setTotalCount(data.Count);
    };

    const Reverse = async (VoucherMasterID) => {
        var inputObj = {
            VoucherMasterID: VoucherMasterID
        }
        const response = await REVERSE_REQUEST(JSON.stringify(inputObj));

        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        setSeverity('success');
        setResponseMessage(response.data);
        callSelectData(TakeCount, SkipCount, '', '', '');
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);

        await callSelectData(TakeCount, SkipCount, '', '', '');
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
    });

    useEffect(() => {
        createColumns();
        selectVoucherType();
        callSelectData(TakeCount, SkipCount, '', '', '');
    }, []);

    const selectByReverseVoucher = (VoucherMasterID) => {
        setIsAlertOpen(true);
        setConfirmAlertRequest(VoucherMasterID);

    }
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: 'ID',
            name: 'VoucherMasterID',
            srno: true
        });

        columns.push({
            label: 'Voucher Date',
            name: 'VoucherDate'
        });

        columns.push({
            label: 'Voucher',
            name: 'VoucherType'
        });
        columns.push({
            label: 'Amount',
            name: 'Amount'
        });
        columns.push({
            label: 'Cr Group',
            name: 'CrGroup'
        });
        columns.push({
            label: 'Cr Name',
            name: 'CrName'
        });
        columns.push({
            label: 'Dr Group',
            name: 'DrGroup'
        });

        columns.push({
            label: 'Dr Name',
            name: 'DrName'
        });

        columns.push({
            label: 'Remark',
            name: 'Remark'
        });
        columns.push({
            label: 'Submitted By',
            name: 'SubmitedByName'
        });
        columns.push({
            label: 'Approved By',
            name: 'ApproveByName'
        });
        columns.push({
            label: 'Created Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Updated Date',
            name: 'UpdatedDOC'
        });
        columns.push({
            label: 'Reverse',
            name: 'VoucherMasterID',
            button: true,
            disabledOn: [''],
            statusIndex: 8,
            callback: (row) => {
                setIsOpen(true);
                selectByReverseVoucher(
                    row
                );
            }
        });

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
                onSubmit={submithandle}
                resetForm={ModelObj}
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'Approve Voucher'}
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
                                                    label="Voucher Type"
                                                    name="voucherTypes"
                                                    title="Select voucher Types"
                                                    value={values.voucherTypes}
                                                    onChange={(e) =>
                                                        setFieldValue(
                                                            'voucherTypes',
                                                            e.target.value
                                                        )
                                                    }
                                                >
                                                    {voucherTypes.map(
                                                        (type) => (
                                                            <MenuItem
                                                                key={
                                                                    type.VoucherTypeID
                                                                }
                                                                value={
                                                                    type.VoucherTypeID
                                                                }
                                                            >
                                                                {
                                                                    type.VoucherTypeName
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
                                                    label="isReverse"
                                                    name="isReverse"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Voucher
                                                    </MenuItem>

                                                    <MenuItem
                                                        key='ACCEPT'
                                                        value='ACCEPT'
                                                    >
                                                        ACCEPT
                                                    </MenuItem>
                                                    <MenuItem
                                                        key='REVERT '
                                                        value='REVERT'
                                                    >
                                                        REVERT
                                                    </MenuItem>

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
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />
            <HeaderVoucher headerSummary={isheaderSummary} />
            {isAlertOpen && (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Reverse Confirmation"
                    message="Are you sure to Reverse Entry ."
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Reverse(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            )}
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
                    callSelectData(TakeCount, currentPage, SearchText, '', '');
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
