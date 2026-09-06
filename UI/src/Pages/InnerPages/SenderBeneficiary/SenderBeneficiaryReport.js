import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';

import {
    SelectSender as SELECT_SENDER,
    SenderActiveDeactive,
    BeneficiaryActDeact,
    SelectBeneficiary as SELECT_BENE
} from 'Components/Api/SenderBeneficiaryAPI';

import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

const SenderBeneficiaryReport = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);

    const [benetransactions, setBeneTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [currentPage, setCurrentPage] = useState(0);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [openFilter, setopenFilter] = useState(true);
    const [SearchText, setSearchText] = useState('');
    const [IsSender, setIsSender] = useState(false);
    const [IsBeneficiary, setIsBeneficiary] = useState(false);

    const [ModelObj] = useState({
        Type: ''
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setTransactions([]);
        setBeneTransactions([]);
        createBeneficiaryColumns();
        setSubmitting(true);
        createColumns();
        callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
    };

    const SelectBeneficiary = async (requestObj) => {
        setBeneTransactions([]);
        createBeneficiaryColumns();
        setIsError(false);
        setLoading(true);
        setIsSender(false);
        setIsBeneficiary(true);

        var response = await SELECT_BENE(requestObj);
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
            formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'Beneficiary Report');
        }
        if (SkipCount <= 0) {
            setTotalCount(data.Count);
        }
        setBeneTransactions(data.list);
    };

    const SelectSender = async (requestObj) => {
        createColumns();
        setTransactions([]);
        setIsSender(true);
        setIsBeneficiary(false);
        setIsError(false);
        setLoading(true);

        var response = await SELECT_SENDER(requestObj);
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
            formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'Sender Report');
        }
        if (SkipCount <= 0) {
            setTotalCount(data.Count);
        }
        setTransactions(data.list);
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
            IsExcelDownload: formRef.current.values.IsExcelDownload
        };

        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);

        if (formRef.current.values.Type === 'Sender') {
            SelectSender(requestJSON);
        } else {
            SelectBeneficiary(requestJSON);
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
        Type: Yup.string()
            .nullable(PLEASE_SELECT_VALID + 'Type')
            .required(PLEASE_SELECT_VALID + 'Type')
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Type')
    });

    const Beneficiaryactivate = async (item, value) => {
        var jsonObj = {
            BeneficiaryID: item,
            IsActive: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoading(true);
        const response = await BeneficiaryActDeact(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
    };

    const Senderactivate = async (SenderID, value) => {
        var jsonObj = {
            SenderID: SenderID,
            IsActive: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoading(true);
        const response = await SenderActiveDeactive(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'SenderID',
            srno: true
        });

        columns.push({
            label: 'Active',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: 0,
            callback: (SenderID, value) => {
                Senderactivate(SenderID, value);
            }
        });
        columns.push({
            label: 'Name',
            name: 'Name'
        });

        columns.push({
            label: 'Mobile',
            name: 'Mobile'
        });

        columns.push({
            label: 'BeneficiaryCount',
            name: 'BeneficiaryCount',
            numeric: true
        });
        columns.push({
            label: 'Address',
            name: 'Address'
        });
        columns.push({
            label: 'Pincode',
            name: 'Pincode'
        });

        setMainColumns(columns);
    };

    const createBeneficiaryColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'BeneficiaryID',
            srno: true
        });

        columns.push({
            label: 'Active',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: 0,
            callback: (BeneficiaryID, value) => {
                Beneficiaryactivate(BeneficiaryID, value);
            }
        });
        columns.push({
            label: 'BeneName',
            name: 'Name'
        });

        columns.push({
            label: 'AccNo',
            name: 'AccountNo'
        });

        columns.push({
            label: 'IFSC',
            name: 'IFSC'
        });

        columns.push({
            label: 'NameVerified',
            name: 'IsNameVerifiedStr'
        });

        columns.push({
            label: 'OTPVerified',
            name: 'IsOTPVerifiedStr'
        });

        columns.push({
            label: 'Sender',
            name: 'Sender'
        });
        columns.push({
            label: 'SenderMobile',
            name: 'SenderMobile'
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
                                        title={'Sender Beneficiary Report'}
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
                                                <MaterialSelect
                                                    label="Type"
                                                    name="Type"
                                                    title="Type"
                                                >
                                                    <MenuItem
                                                        key="Sender"
                                                        value="Sender"
                                                    >
                                                        Sender
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="Beneficiary"
                                                        value="Beneficiary"
                                                    >
                                                        Beneficiary
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
            {IsBeneficiary ? (
                <ReactTable
                    data={benetransactions}
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
                        callSelectData(
                            TakeCount,
                            currentPage,
                            SearchText,
                            '',
                            ''
                        );
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
            ) : null}
            {IsSender ? (
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
                        callSelectData(
                            TakeCount,
                            currentPage,
                            SearchText,
                            '',
                            ''
                        );
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
            ) : null}
        </CustomContainer>
    );
};

export default SenderBeneficiaryReport;
