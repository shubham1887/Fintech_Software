import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';

import { Select as SELECT_REPORT } from 'Components/Api/EKYCAPI';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { Container, Grid, MenuItem, Paper } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { RETAILER_CODE, ADMIN_CODE } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

const ReporteKyc = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);

    const [isAdminRole, setIsAdminRole] = useState(false);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [userList, setUserList] = useState([]);
    const [IsFetchAllList, setisFetchAllList] = useState(false);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [openFilter, setopenFilter] = useState(true);
    const [currentPage, setCurrentPage] = useState(0);
    const [SearchText, setSearchText] = useState('');

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });

    const [ModelObj] = useState({
        DocumentType: 'ALL',
        ToDate: new Date(),
        FromDate: new Date(),
        User: userDefault,
        IsExcelDownload: false
    });
    const isValidToRefreshList = (list) => {
        if (IsFetchAllList === false) {
            return true;
        }

        if (list === undefined || list === null) {
            return true;
        }

        if (list.length <= 1) {
            return true;
        }

        return false;
    };

    const SelectUser = async (text) => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setUserList(objWithSelectAll);
            return;
        }

        var jsonObj = {
            FirmName: text
        };

        if (isValidToRefreshList(userList) === false) {
            return;
        }

        setLoading(true);
        var request = JSON.stringify(jsonObj);
        setLoading(false);
        const userResponse = await USER_SELECT_FOR_DROPDOWN(request);

        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(userResponse.data);

        let objWithSelectAll = [userDefault, ...data];

        setUserList(objWithSelectAll);
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

        SelectUser(text);
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
            ToDate: formRef.current.values.ToDate,
            FromDate: formRef.current.values.FromDate,
            UserID: formRef.current.values.User.UserID,
            DocumentType: formRef.current.values.DocumentType,
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
            downloadFile(data.ReportPath, 'eKYC Report');
        }

        setTransactions(data.list);
        setTotalCount(data.Count);
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

    const validationSchema = Yup.object({});

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
    };
    useEffect(() => {
        CheckUserRole();
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
            label: 'User',
            name: 'FirmName'
        });
        columns.push({
            label: 'Full Name',
            name: 'FullName'
        });

        columns.push({
            label: 'EmailID',
            name: 'EmailID'
        });
        columns.push({
            label: 'MobileNumber',
            name: 'MobileNumber'
        });
        columns.push({
            label: 'Gender',
            name: 'Gender'
        });
        columns.push({
            label: 'PANNo',
            name: 'PANNo'
        });
        columns.push({
            label: 'Document Type',
            name: 'DocumentType'
        });
        columns.push({
            label: 'Document No',
            name: 'DocumentNo'
        });
        columns.push({
            label: 'Dob',
            name: 'Dob'
        });
        columns.push({
            label: 'Address',
            name: 'Address'
        });
        columns.push({
            label: 'Pin',
            name: 'Pin'
        });
       

        columns.push({
            label: 'Profile',
            name: 'ProfileImage',
            ImageHoverComponent: true,
            deactived: true
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
                                        title={'eKYC Report'}
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
                                            {isAdminRole ? (
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
                                                        defaultValue={
                                                            userDefault
                                                        }
                                                        FieldName="User"
                                                        name="User"
                                                        SubFieldName="UserID"
                                                        options={userList}
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.FirmName}
                                                        textFieldProps={{
                                                            label: 'User'
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
                                                    label="DocumentType"
                                                    name="DocumentType"
                                                    title="DocumentType"
                                                >
                                                    <MenuItem
                                                        key="ALL"
                                                        value="ALL"
                                                        selected={true}
                                                    >
                                                        All
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="AADHAR"
                                                        value="AADHAR"
                                                    >
                                                        AADHAR
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="PAN"
                                                        value="PAN"
                                                    >
                                                        PAN
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="GST"
                                                        value="GST"
                                                    >
                                                        GST
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
                                        </Grid>
                                    </CustomAccordion>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />

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

export default ReporteKyc;
