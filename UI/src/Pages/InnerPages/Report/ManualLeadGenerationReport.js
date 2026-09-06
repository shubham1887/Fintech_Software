import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';

import { Container, Grid, MenuItem, Accordion, Box } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { CustomModalWthText } from 'Components/Modal/CustomModalWthText';
import { ActionLeadGenerationStatus } from 'Pages/InnerPages/LeadGenerationMaster/ActionLeadGenerationStatus';
import {
    PLEASE_SELECT_VALID,
    ADMIN_CODE,
    RETAILER_CODE,
} from 'Components/Helper/Messages';


import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';

import {
    Select as SELECT
} from 'Components/Api/LeadGenerationMasterAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
import { StatusItems, ExportTypeItems } from 'Components/ReportHelper';

import { PrimaryButton } from 'Components/Inputes/CustomButton';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';

import usePosition from 'Components/Posistion';
import { faSleigh } from '@fortawesome/free-solid-svg-icons';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

const ManualLeadGenerationReport = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [openFilter, setopenFilter] = useState(true);
    const [userList, setUserList] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isOpen, setIsOpen] = useState(false);
    const [Beneficiary, setBeneficiary] = useState({});
    const [isAdminRole, setIsAdminRole] = useState(false);
    const { latitude, longitude, error } = usePosition();
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ServiceID: 0,
        OperatorID: 0,
        Status: 'ALL',
        ReportBy: 'Inline',
        ProviderID: 0,
        ExportType: 'Select Download',
        IsRechargeReport: false,
        Criteria: 0,
        CriteriaText: '',
        CriteriaTextAmount: '',
        User: userDefault,
        Latitude: latitude,
        Longitude: longitude
    });


    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        //setTransactions([]);
        createColumns();
        callSelectData(TakeCount, SkipCount, '', '', '');

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
            SearchText: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            Status: formRef.current.values.Status,
            UserID: formRef.current.values.User.UserID,
            ExportType: formRef.current.values.ExportType,
            IsPDFDownload: formRef.current.values.IsPDFDownload
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);
        setTransactions([]);
        var response = await SELECT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);

        if (data.IsDownload) {
            downloadFile(data.ReportPath, 'UPITransfer Report');
            return;
        }

        setTransactions(data.list);
        setTotalCount(data.Count);
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
        SelectUser(jsonObj);
    };

    const UpdateStatus = (values) => {
        setBeneficiary(values);
        setIsOpen(true);
    };

    const SelectUser = async (values) => {
        var currentUsertypeid = localStorage.getItem('UserTypeID');

        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setUserList(objWithSelectAll);
            return;
        }

        setLoading(true);
        var request = JSON.stringify(values);
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

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);

        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    useEffect(() => {
        CheckUserRole();
    }, [isAdminRole]);

    useEffect(() => {
        createColumns();
        refreshForm();
        SelectUser();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'LeadGenerationMasterID',
            srno: true
        });

        if (isAdminRole) {

            var options = [
                {
                    Title: 'Action',
                    disabledOn: ['SUCCESS'],
                    statusIndex: 3,
                    Callback: (values) => {
                        UpdateStatus(values);
                    }
                }
            ];
            columns.push({
                label: 'More',
                name: 'LeadGenerationMasterID',
                MoreMenu: true,
                options: options
            });
        }
        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true,

        });

        columns.push({
            label: 'FormID',
            name: 'FormID',
        });

        columns.push({
            label: 'Merchant Name',
            name: 'MerchantName'
        });
        columns.push({
            label: 'Retailer Name',
            name: 'RetailerName'
        });
        columns.push({
            label: 'Consumer Name',
            name: 'ConsumerName'
        });
        columns.push({
            label: 'Mobile Number',
            name: 'MobileNumber',
            bold: true
        });

        columns.push({
            label: 'Email ID',
            name: 'EmailID'
        });

        columns.push({
            label: 'Gender',
            name: 'Gender'
        });

        columns.push({
            label: 'Date of Birth',
            name: 'FromDate'
        });

        columns.push({
            label: 'Aadhar Number',
            name: 'AadharNumber',
        });

        columns.push({
            label: 'Pan Number',
            name: 'PanNumber',
        });

        columns.push({
            label: 'Product Type',
            name: 'LoanType'
        });

        columns.push({
            label: 'Employment Type',
            name: 'ConsumerType'
        });

        columns.push({
            label: 'Required Amount',
            name: 'RequiredAmount'
        });

        columns.push({
            label: 'Approved Amount',
            name: 'ApprovedAmount'
        });

        columns.push({
            label: 'LoanTypeMasterID',
            name: 'LoanTypeMasterID',
            hide: true
        });

        setMainColumns(columns);
    };

    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };
    const loadingProgress = loading && (
        <CircularProgress size={24} className={classes.buttonProgress} />
    );

    const LeadGenerationForm = ({ setFieldValue, handleSubmit }) => {
        return (
            <Form style={{ marginBottom: '15px' }}>
                <Grid item xs={12}>
                    <CustomAccordion
                        title={'Lead Generation Report'}
                        open={openFilter}
                        setOpen={(value) => setopenFilter(value)}
                    >
                        <Grid container spacing={2}>

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

                            {isAdminRole && (

                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <Field
                                        component={AsyncAutoComplete}
                                        defaultValue={userDefault}
                                        FieldName="User"
                                        name="User"
                                        SubFieldName="UserID"
                                        options={userList}
                                        getOptionLabel={(option) =>
                                            option.FirmName
                                        }
                                        textFieldProps={{
                                            label: 'Select User'
                                        }}
                                        onSearchCallback={(text) => {
                                            callSearch(text);
                                        }}
                                        onCustomCallback={(name, value) => {
                                            setUserDefault({
                                                UserID: value.UserID,
                                                FirmName: value.FirmName
                                            });
                                        }}
                                    />
                                </Grid>
                            )}
                            {isAdminRole && (
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <MaterialSelect
                                        label="ExportType"
                                        name="ExportType"
                                        title="ExportType"
                                    >
                                        {ExportTypeItems}
                                    </MaterialSelect>
                                </Grid>
                            )}
                            <Grid item xs={12} sm={12} md={1} lg={1}>
                                <PrimaryButton
                                    type="submit"
                                    title="Search"
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={loading}
                                >
                                    Search
                                </PrimaryButton>
                                {loadingProgress}
                            </Grid>
                        </Grid>
                    </CustomAccordion>
                </Grid>
            </Form>
        );
    };
    const customContainerCallback = (alertState) => {
        setIsError(alertState);
    };
    return (
        <CustomContainer
            isLoading={loading}
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
                {LeadGenerationForm}
            </Formik>

            <ReactTable
                data={transactions}
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    //setCurrentPage(currentPage);
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
            />

            {isOpen ? (
                <CustomModalWthText
                    isOpen={isOpen}
                    title="Lead Generation Status"
                    width="50%"
                    onDismissCallback={(value) => {
                        setIsOpen(value);
                        refreshForm();
                    }}
                >
                    <ActionLeadGenerationStatus
                        IsEdit={true}
                        Beneficiary={Beneficiary}
                        onDismissCallback={(value) => {
                            setIsOpen(value);
                            refreshForm();
                        }}
                    />
                </CustomModalWthText>
            ) : null}

        </CustomContainer>
    );
};
export default ManualLeadGenerationReport;
