import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    MenuItem,
    Icon,
    Box,
    CircularProgress,
    Accordion
} from '@material-ui/core';
import {
    SelectGSTHold,
    SelectForGSTHold,
    BulkUpdateGST
} from 'Components/Api/GSTHoldAPI';
import { useHistory } from 'react-router';
import { CustomModal } from 'Components/Modal/CustomModal';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { logout } from 'Components/Helper/Helpers';
import excel from 'Assets/Images/excel.svg';
import { GSTStatusEdit } from 'Pages/InnerPages/Report/GSTStatusEdit';
import {
    GET_Month_FOR_GST,
    GET_YEAR_FOR_GST
} from 'Components/Helper/Messages';

import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';

import { ADMIN_CODE, SUPPORT_CODE } from 'Components/Helper/Messages';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const GSTHold = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const history = useHistory();
    const [itemList, setItemList] = useState([]);
    const [tableItems, setTableItems] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [mainColumnsGST, setMainColumnsGST] = useState([]);

    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [currentPage, setCurrentPage] = useState(0);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [openFilter, setOpenFilter] = useState(true);
    const [TakeCount, setTakeCount] = useState(100);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);

    const [IsCheckForGSTUserwise, setIsCheckForGSTUserwise] = useState(false);

    const [IsGSTHoldButton, setIsGSTHoldButton] = useState(false);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
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
    const [GSTHoldID, setGSTHoldID] = useState(0);
    const [Years, setYears] = useState([]);

    const [ModelObj] = useState({
        Month: GET_Month_FOR_GST(),
        Year: GET_YEAR_FOR_GST()
    });

    const GetYears = () => {
        var currentYear = new Date().getFullYear();
        var yearList = [];
        for (var year = currentYear - 1; year <= 2030; year++) {
            yearList.push({ Key: year });
        }
        setYears(yearList);
    };
    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
        if (SUPPORT_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);

        createColumns();
    };
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (values.IsCheckForGSTUserwise === true) {
            CheckForGSTUserwise(values);
            createColumnsGST();
            setIsCheckForGSTUserwise(true);
            setIsGSTHoldButton(true);
            return;
        }

        setIsCheckForGSTUserwise(false);
        setIsGSTHoldButton(false);
        setSubmitting(true);

        setLoading(true);
        setTableItems([]);

        let requestJSON = JSON.stringify(values);

        const { data, status, isAuthorized } = await SelectGSTHold(requestJSON);

        setLoading(false);
        if (!isAuthorized) logout();
        if (!status) {
            setIsError(true);
            setResponseMessage(data);
            setSeverity('error');
            return;
        }

        var d = JSON.parse(data);

        setTableItems(d.list);
        setTotalCount(data.Count);
        if (d.IsExcelDownload) downloadFile(d.ReportPath, 'GST Report');
        setSubmitting(false);
    };

    const GstUpdateUserwise = async () => {
        setIsUpdateClicked(true);
        setShouldGetUpdatedDataFromTable(true);
    };

    const CheckForGSTUserwise = async (values) => {
        setLoading(true);
        setTableItems([]);

        let requestJSON = JSON.stringify(values);

        const { data, status, isAuthorized } = await SelectForGSTHold(
            requestJSON
        );

        setLoading(false);
        if (!isAuthorized) logout();
        if (!status) {
            setIsError(true);
            setResponseMessage(data);
            setSeverity('error');
            return;
        }

        var d = JSON.parse(data);

        setItemList(d.list);
        setTotalCount(data.Count);
        if (d.IsExcelDownload) downloadFile(d.ReportPath, 'GST Report');
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
    const Edit = async (row) => {
        setGSTHoldID(row[0]);

        if (row[2] === 'PROCESS') setIsModalOpen(true);
    };
    useEffect(() => {
        GetYears();
        CheckUserRole();
        createColumns();
    }, [isAdminRole]);

    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'GSTHoldID',
            srno: true
        });

        if (isAdminRole === true) {
            columns.push({
                label: 'Action',
                name: 'GSTHoldID',
                icon: 'EditIcon',
                statusIndex: 2,
                iconButtonWithRow: true,
                callback: (row) => {
                    Edit(row);
                }
            });
        }

        columns.push({
            label: 'Status',
            name: 'Status',
            actived: true
        });
        columns.push({
            label: 'LastYearMonth',
            name: 'LastYearMonth',
            actived: true
        });

        columns.push({
            label: 'FirmName',
            name: 'FirmName',
            actived: true
        });

        columns.push({
            label: 'GSTNo',
            name: 'GSTNo',
            actived: true
        });
        columns.push({
            label: 'HoldAmount',
            name: 'HoldAmount',
            numeric: true,
            actived: true
        });
        columns.push({
            label: 'TaxableValue',
            name: 'TaxableValue',
            numeric: true,
            actived: true
        });

        columns.push({
            label: 'IGST',
            name: 'IGST',
            numeric: true,
            actived: true
        });
        columns.push({
            label: 'CGST',
            name: 'CGST',
            numeric: true,
            actived: true
        });
        columns.push({
            label: 'SGST',
            name: 'SGST',
            numeric: true,
            actived: true
        });

        columns.push({
            label: 'Remark',
            name: 'GSTUploadRemark',
            actived: true
        });

        columns.push({
            label: 'AdminRemark',
            name: 'AdminRemark',
            actived: true
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            hide: true
        });

        columns.push({
            label: 'State',
            name: 'State',
            actived: true
        });

        setMainColumns(columns);
    };

    const createColumnsGST = () => {
        var columns = [];
        columns.push({
            label: 'UserID',
            name: 'UserID',
            hide: true
        });

        columns.push({
            label: 'Year',
            name: 'Year',
            hide: true
        });
        columns.push({
            label: 'Month',
            name: 'Month',
            hide: true
        });

        columns.push({
            label: '#',
            name: 'GSTHoldID',
            srno: true
        });

        columns.push({
            label: 'LastYearMonth',
            name: 'LastYearMonth'
        });

        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });

        columns.push({
            label: 'GSTNo',
            name: 'GSTNo'
        });
        columns.push({
            label: 'HoldAmount',
            name: 'HoldAmount',
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

        columns.push({
            label: 'State',
            name: 'State'
        });

        setMainColumnsGST(columns);
    };

    const MonthItems = Months.map(({ Key, Value }, index) => (
        <MenuItem key={index} value={Key}>
            {Value}
        </MenuItem>
    ));
    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    const [IsUpdateClicked, setIsUpdateClicked] = useState(false);
    const YearItems = Years.map(({ Key }, index) => (
        <MenuItem key={index} value={Key}>
            {Key}
        </MenuItem>
    ));

    const ProcessUpdatedData = (data) => {
        setShouldGetUpdatedDataFromTable(false);
        if (IsUpdateClicked) {
            UpdateBulkClick(data);
            return;
        }
    };

    const UpdateBulkClick = async (requestArr, resetForm) => {
        setIsUpdateClicked(false);
        if (requestArr.length <= 0) {
            setSeverity('error');
            setIsError(true);
            setResponseMessage('Please select one or more GST user(s)');
            return;
        }

        let request = [];
        requestArr.forEach((element) => {
            let UserID = element[0].value;
            let Year = element[1].value;
            let Month = element[2].value;

            request.push({
                UserID: UserID,
                Year: Year,
                Month: Month
            });
        });

        setLoading(true);
        var RequestJson = JSON.stringify(request);
        setIsError(false);
        const Response = await BulkUpdateGST(RequestJson);

        setLoading(false);
        if (!Response.isAuthorized) logout();

        if (!Response.status) {
            setSeverity('error');
            setResponseMessage(Response.data);
            setIsError(true);
            return;
        }
        history.push('/GSTHold');
        setSeverity('success');
        setResponseMessage(Response.data);
        setIsError(true);
        setItemList([]);
        createColumnsGST();
        createColumns();
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
                                    title={'GST Hold'}
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
                                            md={1}
                                            lg={1}
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

                                        {isAdminRole === true ? (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <SecondaryButton
                                                    variant="contained"
                                                    color="primary"
                                                    title="Check For GST"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'IsCheckForGSTUserwise',
                                                            true
                                                        );
                                                        handleSubmit(e);
                                                    }}
                                                    style={{
                                                        marginTop: '11px'
                                                    }}
                                                >
                                                    Check For GST
                                                </SecondaryButton>
                                            </Grid>
                                        ) : null}
                                        {IsGSTHoldButton === true ? (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <SecondaryButton
                                                    variant="contained"
                                                    color="primary"
                                                    title="GST Hold"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    onClick={() =>
                                                        GstUpdateUserwise()
                                                    }
                                                    style={{
                                                        marginTop: '11px'
                                                    }}
                                                >
                                                    GST Hold Update
                                                </SecondaryButton>
                                            </Grid>
                                        ) : null}
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Hold and Processed GST"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loading}
                                            >
                                                Hold and Processed GST
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

            {IsCheckForGSTUserwise === false ? (
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
            ) : null}

            {IsCheckForGSTUserwise === true ? (
                <ReactTable
                    data={itemList}
                    columns={mainColumnsGST}
                    headerToolbar={false}
                    SuccessFailHeader={true}
                    primaryIndex={12}
                    setLoadingOpen={(value) => {
                        setLoading(value);
                    }}
                    setIsError={(value) => {
                        setIsError(value);
                    }}
                    setSeverity={(value) => {
                        setSeverity(value);
                    }}
                    setResponseMessage={(value) => {
                        setResponseMessage(value);
                    }}
                    getUpdatedData={(data) => {
                        ProcessUpdatedData(data);
                        setShouldGetUpdatedDataFromTable(false);
                    }}
                    shouldGetUpdatedData={shouldGetUpdatedDataFromTable}
                    isSelectableRows={'multiple'}
                />
            ) : null}

            {isModalOpen ? (
                <CustomModal
                    isOpen={isModalOpen}
                    title="GST Status update"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsModalOpen(value);
                        refreshForm();
                    }}
                >
                    <GSTStatusEdit
                        GSTHoldID={GSTHoldID}
                        onDismissCallback={(value) => {
                            setIsModalOpen(value);
                        }}
                    />
                </CustomModal>
            ) : null}
        </CustomContainer>
    );
};
