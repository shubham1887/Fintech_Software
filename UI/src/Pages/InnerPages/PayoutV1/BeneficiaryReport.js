import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    SelectBeneficiary as SELECT_BENE,
    DeleteBene
} from 'Components/Api/PayoutBeneficiaryAPI';
import { CustomModal } from 'Components/Modal/CustomModal';
import { LOGOUT } from 'ConstComponents';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import {
    Paper,
    Container,
    Grid,
    Box,
    MenuItem,
    CircularProgress
} from '@material-ui/core';

import { ActionBeneStatus } from 'Pages/InnerPages/PayoutV1/ActionBeneStatus';

import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { ADMIN_CODE, RETAILER_CODE } from 'Components/Helper/Messages';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { ExportTypeItems } from 'Components/ReportHelper';
const CustomContainer = HOCLoader(Container);

export const BeneficiaryReport = () => {
    const classes = commonStyle();

    const formRef = useRef();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [severity, setSeverity] = useState('success');
    const [currentPage, setCurrentPage] = useState(0);
    const [mainColumns, setMainColumns] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [userList, setUserList] = useState([]);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [isOpen, setIsOpen] = useState(false);
    const [isDeleteAlertOpen, setIsDeleteAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [Beneficiary, setBeneficiary] = useState({});
    const [isViewOpen, setIsViewOpen] = useState(false);
    const [ViewDocumentURL, setViewDocumentURL] = useState("");
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select Entity'
    });
    var ModelObj = {
        Status: 'PROCESS',
        User: userDefault,
        ExportType: 'Select Download'
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'BeneficiaryID',
            srno: true
        });

        if (isAdminRole) {
            var options = [
                {
                    Title: 'Action',
                    disabledOn: ['REJECT', 'APPROVE'],
                    statusIndex: 2,
                    Callback: (values) => {
                        UpdateStatus(values);
                    }
                }
            ];
            columns.push({
                label: 'Action',
                name: 'BeneficiaryID',
                MoreMenu: true,
                options: options
            });
        }

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });
        if (isAdminRole) {
            columns.push({
                label: 'Entity Name',
                name: 'FirmName'
            });
        }
        columns.push({
            label: 'A/c Holder Name',
            name: 'Name'
        });

        columns.push({
            label: 'Account Number',
            name: 'AccountNo'
        });
        columns.push({
            label: 'IFSC',
            name: 'IFSC'
        });
        columns.push({
            label: 'Bank Name',
            name: 'BankName'
        });

        columns.push({
            label: 'Name Verified',
            name: 'IsNameVerifiedStr'
        });

        columns.push({
            label: 'Bank A/c Proof',
            name: 'Document',

            statusIndex: 8,
            iconButton: true,
            callback: (value) => {
                ViewDocument(value);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'BeneficiaryID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });

        setMainColumns(columns);
    };
    const ViewDocument = (ViewDocumentURL) => {
        setViewDocumentURL(ViewDocumentURL);
        setIsViewOpen(true);
    };

    const onDeleteClick = (value) => {

        var RequestObj = { BeneficiaryID: value[0], AccountNumber: isAdminRole ? value[5] : value[3] };
        setIsDeleteAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (requestObj) => {
        setLoadingOpen(true);
        var reqObj = JSON.stringify(requestObj);
        var response = await DeleteBene(reqObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }
        callSelectData(TakeCount, SkipCount, '', '', '');
    };
    const UpdateStatus = (values) => {
        setBeneficiary(values);
        setIsOpen(true);
    };

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);

        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    useEffect(() => {
        CheckUserRole();
    }, [isAdminRole]);

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

    const SelectUser = async (values) => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setUserList(objWithSelectAll);
            return;
        }

        setLoadingOpen(true);
        var request = JSON.stringify(values);
        setLoadingOpen(false);
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

    const validationSchema = Yup.object({});

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setItemList([]);
        setSubmitting(true);

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
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            UserID: formRef.current.values.User.UserID,
            Status: formRef.current.values.Status,
            ExportType: formRef.current.values.ExportType
        };

        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await SELECT_BENE(requestJSON);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        setItemList([]);
        createColumns();

        var data = JSON.parse(response.data);
        if (data.IsDownload) {
            downloadFile(data.ReportPath, 'Payout Beneficiary Report');
            return;
        }
        if (SkipCount <= 0) {
            // setHeaderSummary(data.HeaderSummary);
            setTotalCount(data.Count);
        }

        setItemList(data.list);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }

    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
    };
    return (
        <CustomContainer
            isLoading={loadingOpen}
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
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Bank A/c linkage request
                            </h2>
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
                                            <MaterialSelect
                                                label="Status"
                                                name="Status"
                                                title="Status"
                                            >
                                                <MenuItem key="ALL" value="ALL">
                                                    All Status
                                                </MenuItem>
                                                <MenuItem
                                                    key="APPROVE"
                                                    value="APPROVE"
                                                >
                                                    Accepted
                                                </MenuItem>
                                                <MenuItem
                                                    key="REJECT"
                                                    value="REJECT"
                                                >
                                                    Rejected
                                                </MenuItem>
                                                <MenuItem
                                                    key="PROCESS"
                                                    value="PROCESS"
                                                >
                                                    Pending
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>

                                        {isAdminRole === true ? (
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
                                                    options={userList}
                                                    getOptionLabel={(option) =>
                                                        option.FirmName
                                                    }
                                                    textFieldProps={{
                                                        label: 'Select Entity'
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
                                                disabled={loadingOpen}
                                            >
                                                Search
                                            </PrimaryButton>
                                            {loadingOpen && (
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
                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div
                                        className={classes.SubmitWrapper}
                                    ></div>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            />

            <h2 className={classes.PaperHead_h2}>Linked Bank Accounts</h2>

            <ReactTable
                data={itemList}
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

            {isOpen ? (
                <CustomModalWithScroll
                    isOpen={isOpen}
                    title="Action"
                    width="100%"
                    onDismissCallback={(value) => {
                        setIsOpen(value);
                        refreshForm();
                    }}
                >
                    <ActionBeneStatus
                        IsEdit={true}
                        Beneficiary={Beneficiary}
                        onDismissCallback={(value) => {
                            setIsOpen(value);
                            refreshForm();
                        }}
                    />
                </CustomModalWithScroll>
            ) : null}

            {isDeleteAlertOpen ? (
                <ConfirmAlert
                    isOpen={isDeleteAlertOpen}
                    title="Delete Confirmation"
                    message={`Please confirm the deletion of linked bank account number.${confirmAlertRequest.AccountNumber} `}
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsDeleteAlertOpen(false);
                        Delete(request);
                    }}
                    negativeButtonClick={() => {
                        setIsDeleteAlertOpen(false);
                    }}
                />
            ) : null}
            {isViewOpen && (
                <CustomModal
                    isOpen={isViewOpen}
                    title="Document Upload"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsViewOpen(value);

                    }}
                >
                    <img
                        src={ViewDocumentURL}
                    />
                </CustomModal>
            )}
        </CustomContainer>
    );
};
