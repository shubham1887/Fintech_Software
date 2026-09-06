import {
    Box,
    Container,
    Grid,
    Tab,
    MenuItem,
    CircularProgress,
    Tabs
} from '@material-ui/core';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { Formik, Form } from 'formik';

import {
    Delete as USER_DELETE,
    SelectUserList as USER_SELECT_LIST,
    SendPassword as SEND_PASSWORD,
    ReKyc as UPDATE_RE_KYC
} from 'Components/Api/UsersApi';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    ADMIN_CODE,
    DMT_WalletID,
    SUPPORT_CODE
} from 'Components/Helper/Messages';
import {
    ALL_USERS,
    API_USER,
    CLIENT_CODE,
    DISTRIBUTOR,
    RETAILER,
    SUPER_DISTRIBUTOR,
    SUPPORT_USER,
    SDFOS,
    DFOS,
    DEACTIVE_USERS
} from 'Components/Helper/UsertypeCode';
import HOCLoader from 'Components/Loader/HOCLoader';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { CustomModal } from 'Components/Modal/CustomModal';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { HeaderSummary } from 'Pages/InnerPages/Users/BalanceHeaderSummary';
import React, { useEffect, useState, useRef } from 'react';
import { useHistory } from 'react-router';
import { DaybookPopUp } from '../DayBookReport/DaybookInPopUp';
import { EnableDisablePopUp } from 'Pages/InnerPages/Users/EnableDisable';
import { DownlinePopup } from 'Pages/InnerPages/Users/DownlinePopup';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
import { TransferInPopUp } from '../Fund/TransferInPopUp';
import { LedgerPopUp } from '../Report/LedgerInPopUp';
import UserCommission from './UserCommission';
import Accordion from '@material-ui/core/Accordion';
import { EqualsIgnoreCase } from 'Components/Helper/Validation';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';

const CustomContainer = HOCLoader(Container);

const CustomAccordion = HOCAccordion(Accordion);
export const Users = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const history = useHistory();
    let CommissionOnTransfer = localStorage.getItem('CommissionOnTransfer');

    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [IsTextValue, setIsTextValue] = useState(false);
    const [IsNumberValue, setIsNumberValue] = useState(false);
    const [IsAdharNumberValue, setIsAdharNumberValue] = useState(false);
    const [IsIsFromDateToDate, setIsFromDateToDate] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [isAlertOpenRekyc, setIsAlertOpenRekyc] = useState(false);
    const [isAlertSendPwdOpen, setIsAlertSendPwdOpen] = useState(false);

    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [value, setValue] = useState(ALL_USERS);

    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [openFilter, setOpenFilter] = useState(true);
    const [selectUserID, setselectUserID] = useState(0);

    const [UsersList, setUsersList] = useState([]);
    const [selectedUserID, setSelectedUserID] = useState(0);
    const [search, setSearch] = useState('');
    const [sortColumnName, setSortColumnName] = useState('');
    const [sortDirection, setSortDirection] = useState('');

    const [fundTransferData, setFundTransferData] = useState({});
    const [DaybookData, setDaybookData] = useState({});
    const [LedgerData, setLedgerData] = useState({});
    const [headerSummary, setHeaderSummary] = useState([]);

    const [isOpen, setIsOpen] = useState(false);
    const [isOpenDaybook, setIsOpenDaybook] = useState(false);
    const [isOpenEnableDisable, setIsOpenEnableDisable] = useState(false);
    const [isOpenDownline, setIsOpenDownline] = useState(false);

    const [isOpenLedger, setIsOpenLedger] = useState(false);
    const [isOpenUserCommission, setIsOpenUserCommission] = useState(false);
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState('');
    const [CriteriaNumberLabel, setCriteriaNumberLabel] = useState('');
    const [CriteriaAdharNumberLabel, setCriteriaAdharNumberLabel] = useState(
        ''
    );
    const [IsDMTWallet, setIsDMTWallet] = useState(false);
    const [currentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );
    const [IsSwitchValue, setIsSwitchValue] = useState(false);
    const [isAdmin, setIsAdminRole] = useState(false);

    const callSelectData = (
        TakeCount,
        SkipCount,
        UserTypeID,
        Search,
        excelDownload,
        MobileNumber,
        ParentID,
        FirmName,
        SlabName
    ) => {
        var request = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            UserTypeID: UserTypeID,
            Search: Search,
            MobileNumber: MobileNumber,
            ParentID: ParentID,
            FirmName: FirmName,
            SlabName: SlabName,
            SortColumnName: sortColumnName,
            SortDirection: sortDirection,
            IsExcelDownload: excelDownload,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText,
            CriteriaNumber: formRef.current.values.CriteriaNumber,
            CriteriaValue: formRef.current.values.CriteriaValue,
            IsTextValue: IsTextValue,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate
        };

        selectAllUsers(request);
    };

    const selectWallets = async () => {
        var request = {
            ID: 0
        };
        var json = JSON.stringify(request);
        const response = await WALLET_SELECT(json);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        var data = JSON.parse(response.data);

        data.forEach((element) => {
            if (element.WalletID === DMT_WalletID && element.IsActive === true)
                setIsDMTWallet(true);
            return;
        });
    };
    const selectAllUsers = async (request) => {
        let requestObj = JSON.stringify(request);
        setUsersList([]);
        setHeaderSummary([]);

        setLoadingOpen(true);
        const response = await USER_SELECT_LIST(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        if (data.IsExcelDownload === true) {
            downloadFile(data.ReportPath, 'User Report');
            return;
        }

        setTotalCount(data.Count);
        setUsersList(data.data);
        setHeaderSummary(data.headerlist);
    };
    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }
    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date().addDays(-30),
        Criteria: 0,
        CriteriaText: '',
        CriteriaNumber: '',
        CriteriaValue: false,
        IsTextValue: false,
        IsSwitchValue: false
    });
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setUsersList([]);
        createColumns();
        callSelectData(TakeCount, SkipCount, 0, '', false);
        setSubmitting(false);
    };

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
        if (SUPPORT_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
    };
    useEffect(() => {
        setLoadingOpen(true);
        CheckUserRole();
        callSelectData(TakeCount, SkipCount, ALL_USERS, '', false);
        createColumns();
    }, [TakeCount, IsDMTWallet]);

    useEffect(() => {
        selectWallets();
    }, []);

    const Delete = async (request) => {
        let jsonObj = {
            UserID: request
        };

        setSeverity('success');
        setResponseMessage('');

        var requestjson = JSON.stringify(jsonObj);
        const response = await USER_DELETE(requestjson);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        callSelectData(TakeCount, SkipCount, ALL_USERS, '', false);
    };
    const UpdateReKyc = async (request) => {
        let jsonObj = {
            UserID: request
        };

        setSeverity('success');
        setResponseMessage('');

        var requestjson = JSON.stringify(jsonObj);
        const response = await UPDATE_RE_KYC(requestjson);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        callSelectData(TakeCount, SkipCount, ALL_USERS, '', false);
    };
    const Edit = async (request) => {
        history.push('/Users/Edit/' + request);
    };
    const OnclickKYC = async (request) => {
        history.push('Users/Approve');
    };
    const OnclickReKYC = (value) => {
        setIsAlertOpenRekyc(true);
        setConfirmAlertRequest(value);
    };

    const FundTransfer = async (userID, firmName) => {
        setFundTransferData({ UserID: userID, FirmName: firmName });
        setIsOpen(true);
    };
    const Daybook = async (userID, firmName) => {
        setDaybookData({ UserID: userID, FirmName: firmName });
        setIsOpenDaybook(true);
    };
    const Ledger = async (userID, firmName) => {
        setLedgerData({ UserID: userID, FirmName: firmName });

        setIsOpenLedger(true);
    };
    const EnableDisable = async (userID) => {
        setselectUserID(userID);
        setIsOpenEnableDisable(true);
    };
    const selectDownline = async (userID) => {
        setselectUserID(userID);
        setIsOpenDownline(true);
    };

    const sendPassword = async (UserID) => {
        let jsonObj = {
            UserID: UserID
        };
        var request = JSON.stringify(jsonObj);

        setSeverity('success');
        setResponseMessage('');

        const response = await SEND_PASSWORD(request);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
    };

    const createColumns = () => {
        let columns = [];

        let primaryKeyIndex = 1;

        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });

        columns.push({
            label: 'Role',
            name: 'UserType',
            deactived: true
        });
        columns.push({
            label: 'Firm',
            name: 'FirmName',
            deactived: true
        });
        columns.push({
            label: 'KYC',
            name: 'IsKYCApprovedStr',
            deactived: true
        });

        if (
            currentUserTypeID === ADMIN_CODE ||
            currentUserTypeID === SUPPORT_CODE
        ) {
            columns.push({
                label: 'Slab',
                name: 'SlabName',
                deactived: true
            });
        }
        columns.push({
            label: 'Mobile',
            name: 'MobileNumber',
            deactived: true
        });

        columns.push({
            label: 'Parent',
            name: 'ParentName',
            deactived: true
        });
        columns.push({
            label: 'Down',
            name: 'UserTypeID',
            Downline: true,
            primaryKeyIndex: primaryKeyIndex,
            callback: (primaryKeyIndex) => {
                selectDownline(primaryKeyIndex);
            }
        });
        columns.push({
            label: 'Rech Bal',
            name: 'RechargeBalance',
            deactived: true
        });
        if (IsDMTWallet === true) {
            columns.push({
                label: 'DMT Bal',
                name: 'DMTBalance',
                deactived: true
            });
        }
        columns.push({
            label: 'AEPS Bal',
            name: 'AEPSBalance',
            deactived: true
        });

        var IsCashCollection =
            localStorage.getItem('IsCashCollection') === 'true';
        if (IsCashCollection === true) {
            columns.push({
                label: 'OS',
                name: 'Outstanding',
                deactived: true
            });
        }

        columns.push({
            label: 'Rech Capp',
            name: 'CappRechargeBalance',
            deactived: true
        });
        if (IsDMTWallet) {
            columns.push({
                label: 'DMT Capp',
                name: 'CappDMTBalance',
                deactived: true
            });
        }
        columns.push({
            label: 'AEPS Capp',
            name: 'CappAEPSBalance',
            deactived: true
        });
        columns.push({
            label: 'State',
            name: 'StateName'
        });
        columns.push({
            label: 'JoinedBy',
            name: 'JoiningByUser',
            deactived: true
        });
        columns.push({
            label: 'JoinDate',
            name: 'DOC',
            deactived: true
        });
        columns.push({
            label: 'Profile',
            name: 'ProfileImagePath',
            Image: true,
            deactived: true
        });

        let options = [
            {
                Title: 'Edit',
                Callback: (values) => {
                    Edit(values[0]);
                }
            }
        ];

        if (
            currentUserTypeID === ADMIN_CODE ||
            currentUserTypeID === SUPPORT_CODE
        ) {
            options.push({
                Title: 'Send Password',
                Callback: (values) => {
                    UserSendPassword(values[0]);
                }
            });
        }

        if (currentUserTypeID !== SDFOS && currentUserTypeID !== DFOS) {
            options.push({
                Title: 'Transfer',
                Callback: (values) => {
                    FundTransfer(values[0], values[2]);
                }
            });
            if (
                currentUserTypeID === ADMIN_CODE ||
                currentUserTypeID === SUPPORT_CODE
            ) {
                options.push({
                    Title: 'KYC',
                    Callback: (values) => {
                        OnclickKYC(values[0]);
                    }
                });
                options.push({
                    Title: 'Re KYC',
                    Callback: (values) => {
                        OnclickReKYC(values[0]);
                    }
                });
            }
            options.push({
                Title: 'Ledger',
                Callback: (values) => {
                    Ledger(values[0], values[2]);
                }
            });
            options.push({
                Title: 'DayBook',
                Callback: (values) => {
                    Daybook(values[0], values[2]);
                }
            });
            if (EqualsIgnoreCase(CommissionOnTransfer, 'Yes')) {
                options.push({
                    Title: 'Commission',
                    Callback: (values) => {
                        setSelectedUserID(values[0]);
                        setIsOpenUserCommission(true);
                    }
                });
            }
        }
        if (
            currentUserTypeID === ADMIN_CODE ||
            currentUserTypeID === SUPPORT_CODE
        ) {
            options.push({
                Title: 'EnableDisable',
                Callback: (values) => {
                    EnableDisable(values[0]);
                }
            });
            options.push({
                Title: 'Delete',
                Callback: (values) => {
                    DeleteUser(values[0]);
                }
            });
        }

        columns.splice(0, 0, {
            label: 'Action',
            name: 'UserID',
            MoreMenu: true,
            options: options
        });

        columns.push({
            label: 'IsActive',
            name: 'IsActive',
            hide: true
        });

        setMainColumns(columns);
    };

    const DeleteUser = (value) => {
        setIsAlertOpen(true);
        setConfirmAlertRequest(value);
    };
    const UserSendPassword = (value) => {
        setIsAlertSendPwdOpen(true);
        setConfirmAlertRequest(value);
    };
    const handleChange = (event, newValue) => {
        setValue(newValue);
        setTotalCount(0);
        callSelectData(TakeCount, SkipCount, newValue, '', false);
    };

    const customContainerCallback = (alertState) => {
        setIsError(alertState);
    };
    const DeleteConfirmationModal = isAlertOpen ? (
        <ConfirmAlert
            isOpen={isAlertOpen}
            title="Delete Confirmation"
            message="Are you sure to delete this user."
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpen(false);
                Delete(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpen(false);
            }}
        />
    ) : null;
    const SendPasswordConfirmationModal = isAlertSendPwdOpen ? (
        <ConfirmAlert
            isOpen={isAlertSendPwdOpen}
            title="Send Password Confirmation"
            message="Are you sure to reset the password and send a new one?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertSendPwdOpen(false);
                sendPassword(request);
            }}
            negativeButtonClick={() => {
                setIsAlertSendPwdOpen(false);
            }}
        />
    ) : null;
    const ReKYCConfirmationModal = isAlertOpenRekyc ? (
        <ConfirmAlert
            isOpen={isAlertOpenRekyc}
            title="Re Kyc Confirmation"
            message="Are you sure to rekyc this user."
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpenRekyc(false);
                UpdateReKyc(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpenRekyc(false);
            }}
        />
    ) : null;
    const TransferInPopUpCallback = (value) => {
        setIsOpen(value);
    };
    const FundTransferModal = isOpen ? (
        <CustomModalWithScroll
            isOpen={isOpen}
            title="Fund Transfer"
            width="75%"
            onDismissCallback={TransferInPopUpCallback}
        >
            <TransferInPopUp
                FundTransferData={fundTransferData}
                onDismissCallback={TransferInPopUpCallback}
            />
        </CustomModalWithScroll>
    ) : null;
    const LedgerPopUpCallback = (value) => {
        setIsOpenLedger(value);
    };
    const LedgerModal = isOpenLedger ? (
        <CustomModalWithScroll
            isOpen={isOpenLedger}
            title="Ledger Report"
            width="75%"
            onDismissCallback={LedgerPopUpCallback}
        >
            <LedgerPopUp
                LedgerData={LedgerData}
                onDismissCallback={LedgerPopUpCallback}
            />
        </CustomModalWithScroll>
    ) : null;
    const OpenDaybookModalCallback = (value) => {
        setIsOpenDaybook(value);
    };

    const OpenDaybookModal = isOpenDaybook ? (
        <CustomModalWithScroll
            isOpen={isOpenDaybook}
            title="DayBook Report"
            width="75%"
            onDismissCallback={OpenDaybookModalCallback}
        >
            <DaybookPopUp
                DaybookData={DaybookData}
                onDismissCallback={OpenDaybookModalCallback}
            />
        </CustomModalWithScroll>
    ) : null;
    const OpenEnableDisableModalCallback = (value) => {
        setIsOpenEnableDisable(value);
    };
    const OpenEnableDisableModal = isOpenEnableDisable ? (
        <CustomModal
            isOpen={isOpenEnableDisable}
            title="Enable Disable User"
            width="75%"
            onDismissCallback={OpenEnableDisableModalCallback}
        >
            <EnableDisablePopUp
                UserID={selectUserID}
                onDismissCallback={OpenEnableDisableModalCallback}
            />
        </CustomModal>
    ) : null;
    const OpenDownlineModalCallback = (value) => {
        setIsOpenDownline(value);
    };
    const OpenDownlineModal = isOpenDownline ? (
        <CustomModalWithScroll
            isOpen={isOpenDownline}
            title="DownLine"
            width="75%"
            onDismissCallback={OpenDownlineModalCallback}
        >
            <DownlinePopup
                UserID={selectUserID}
                onDismissCallback={OpenDownlineModalCallback}
            />
        </CustomModalWithScroll>
    ) : null;

    const UserInfoTableGetCallback = ({ currentPage, RowsPerPage }) => {
        setTakeCount(RowsPerPage);
        setSkipCount(RowsPerPage * currentPage);
        callSelectData(
            RowsPerPage,
            RowsPerPage * currentPage,
            value,
            '',
            false
        );
    };
    const UserInfoTableGetSearchCallback = (SearchText) => {
        callSelectData(
            TakeCount,
            SkipCount,
            value,
            SearchText.SearchText,
            false
        );
    };
    const UserInfoTableGetSortCallback = ({ column, direction }) => {
        setSortColumnName(column);
        setSortDirection(direction);
        callSelectData(TakeCount, SkipCount, value, search, false);
    };
    const UserInfoTable = (
        <ReactTable
            title="Users Info"
            data={UsersList}
            count={totalCount}
            columns={mainColumns}
            serverSide={true}
            rowsPerPage={TakeCount}
            getCallback={UserInfoTableGetCallback}
            getSearchCallback={UserInfoTableGetSearchCallback}
            getSortCallback={UserInfoTableGetSortCallback}
        />
    );

    const handleUserCommissionDismissCallback = (value) => {
        setIsOpenUserCommission(value);
    };
    const UserCommissionModal = isOpenUserCommission ? (
        <CustomModal
            isOpen={isOpenUserCommission}
            title="User Commission"
            width="75%"
            onDismissCallback={handleUserCommissionDismissCallback}
        >
            <UserCommission
                selectedUserID={selectedUserID}
                getModalClose={handleUserCommissionDismissCallback}
            />
        </CustomModal>
    ) : null;
    const TextDisplay = (setFieldValue) => {
        setFieldValue('CriteriaText', '');
        setIsTextValue(true);
        setIsNumberValue(false);
        setIsFromDateToDate(false);
        setIsDisplayCriteria(false);
    };
    const DateDisplay = () => {
        setIsFromDateToDate(true);
        setIsNumberValue(false);
        setIsTextValue(false);
    };
    const NumberDisplay = (setFieldValue) => {
        setFieldValue('CriteriaNumber', '');
        setIsNumberValue(true);
        setIsTextValue(false);
        setIsFromDateToDate(false);
    };

    const AdharNumberDisplay = (setFieldValue) => {
        setFieldValue('CriteriaNumber', '');
        setIsAdharNumberValue(true);
        setIsNumberValue(false);
        setIsTextValue(false);
        setIsFromDateToDate(false);
    };

    const SwitchDisplay = () => {
        //setFieldValue('CriteriaValue', '');
        setIsDisplayCriteria(true);
        setIsNumberValue(false);
        setIsSwitchValue(true);
        setIsTextValue(false);
        setIsFromDateToDate(false);
    };

    const PlaceholderDisplay = () => {
        setIsTextValue(false);
        setIsNumberValue(false);
        setIsFromDateToDate(false);
        setIsDisplayCriteria(false);
    };

    const OnChange = (name, value, setFieldValue) => {
        // setFieldValue(name, value);
    };

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={customContainerCallback}
        >
            <Formik
                enableReinitialize={true}
                innerRef={formRef}
                initialValues={ModelObj}
                onSubmit={submithandle}
                resetForm={ModelObj}
            >
                {({ setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Users Report'}
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
                                                label="Selection criteria"
                                                name="Criteria"
                                                title="criteria"
                                                onCustomCallback={(
                                                    name,
                                                    value,
                                                    setFieldValue
                                                ) => {
                                                    //setCriteriaTextLabel(value);
                                                    setIsDisplayCriteria(true);
                                                }}
                                            >
                                                <MenuItem
                                                    key={0}
                                                    value={0}
                                                    onClick={() => {
                                                        PlaceholderDisplay();
                                                    }}
                                                >
                                                    Selection Criteria
                                                </MenuItem>
                                                <MenuItem
                                                    key={1}
                                                    value={'Name'}
                                                    onClick={() => {
                                                        TextDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    Name
                                                </MenuItem>
                                                <MenuItem
                                                    key={2}
                                                    value={'MobileNumber'}
                                                    onClick={() => {
                                                        NumberDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    Mobile Number
                                                </MenuItem>
                                                <MenuItem
                                                    key={3}
                                                    value={'AliasName'}
                                                    onClick={() => {
                                                        TextDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    AliasName
                                                </MenuItem>

                                                <MenuItem
                                                    key={3}
                                                    value={'ParentMobile'}
                                                    onClick={() => {
                                                        NumberDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    Parent Mobile
                                                </MenuItem>
                                                <MenuItem
                                                    key={4}
                                                    value={'Date'}
                                                    onClick={() => {
                                                        DateDisplay();
                                                    }}
                                                >
                                                    Date
                                                </MenuItem>
                                                <MenuItem
                                                    key={5}
                                                    value={'IsCommissionHold'}
                                                    onClick={() => {
                                                        SwitchDisplay();
                                                    }}
                                                >
                                                    IsCommissionHold
                                                </MenuItem>
                                                <MenuItem
                                                    key={6}
                                                    value={'AadharNo'}
                                                    onClick={() => {
                                                        AdharNumberDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    AadharNo
                                                </MenuItem>
                                                <MenuItem
                                                    key={7}
                                                    value={'PANNo'}
                                                    onClick={() => {
                                                        TextDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    PANNo
                                                </MenuItem>
                                                <MenuItem
                                                    key={7}
                                                    value={'Pincode'}
                                                    onClick={() => {
                                                        NumberDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    Pincode
                                                </MenuItem>
                                                <MenuItem
                                                    key={7}
                                                    value={'CityName'}
                                                    onClick={() => {
                                                        TextDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    CityName
                                                </MenuItem>
                                                {/* <MenuItem
                                                    key={8}
                                                    value={'DMTBalance'}
                                                    onClick={() => {
                                                        NumberDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    DMTBalance
                                                </MenuItem>
                                                <MenuItem
                                                    key={8}
                                                    value={'RechargeBalance'}
                                                    onClick={() => {
                                                        NumberDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    RechargeBalance
                                                </MenuItem> */}
                                            </MaterialSelect>
                                        </Grid>
                                        {IsTextValue === true ? (
                                            <>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    <MaterialTextField
                                                        label={
                                                            CriteriaTextLabel
                                                        }
                                                        name="CriteriaText"
                                                        type="text"
                                                        placeholder={
                                                            'Enter' +
                                                            CriteriaTextLabel
                                                        }
                                                    />

                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={2}
                                                        lg={2}
                                                    ></Grid>
                                                </Grid>
                                            </>
                                        ) : null}
                                        {IsNumberValue === true ? (
                                            <>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    <MaterialNumberField
                                                        label={
                                                            CriteriaNumberLabel
                                                        }
                                                        name="CriteriaNumber"
                                                        length="10"
                                                        type="tel"
                                                        placeholder="Enter"
                                                        title="Mobile Number"
                                                        typed_length={1}
                                                    />

                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={2}
                                                        lg={2}
                                                    ></Grid>
                                                </Grid>
                                            </>
                                        ) : null}

                                        {IsAdharNumberValue === true && (
                                            <>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    <MaterialNumberField
                                                        label={
                                                            CriteriaAdharNumberLabel
                                                        }
                                                        name="CriteriaNumber"
                                                        length="12"
                                                        type="tel"
                                                        placeholder="Adhar Number"
                                                        title="Adhar Number"
                                                        typed_length={1}
                                                    />

                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={2}
                                                        lg={2}
                                                    ></Grid>
                                                </Grid>
                                            </>
                                        )}
                                        {IsIsFromDateToDate ? (
                                            <>
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
                                            </>
                                        ) : null}
                                        {IsSwitchValue === true ? (
                                            <FormikMaterialSwitch
                                                label={CriteriaTextLabel}
                                                name="CriteriaValue"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnChange(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                            />
                                        ) : null}
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            {!IsDisplayCriteria ? null : (
                                                <>
                                                    <div
                                                        className={
                                                            classes.SubmitWrapper
                                                        }
                                                    >
                                                        <PrimaryButton
                                                            type="submit"
                                                            title="Search"
                                                            className={
                                                                classes.CustomPrimaryButtonStyle
                                                            }
                                                            disabled={
                                                                loadingOpen
                                                            }
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

                                                        {loadingOpen && (
                                                            <CircularProgress
                                                                size={24}
                                                                className={
                                                                    classes.buttonProgress
                                                                }
                                                            />
                                                        )}
                                                    </div>
                                                </>
                                            )}
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        ></Grid>
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
                                                    disabled={loadingOpen}
                                                    classes={{
                                                        root: classes.iconRoot
                                                    }}
                                                    onClick={() => {
                                                        callSelectData(
                                                            100,
                                                            10,
                                                            value,
                                                            '',
                                                            true
                                                        );
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
                                                variant="contained"
                                                color="primary"
                                                title="New"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={() => {
                                                    history.push('/Users/Add');
                                                }}
                                            >
                                                New
                                            </PrimaryButton>
                                        </Grid>
                                    </Grid>
                                </CustomAccordion>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            <Grid container spacing={2} style={{ marginTop: '16px' }}>
                <Grid item xs={12} sm={12} md={12} lg={12}>
                    <Tabs
                        value={value}
                        onChange={handleChange}
                        variant="scrollable"
                        scrollButtons="auto"
                        aria-label="scrollable auto tabs example"
                    >
                        <Tab label="All" value={ALL_USERS} />

                        {isAdmin ? (
                            <Tab
                                label="Super Dist."
                                value={SUPER_DISTRIBUTOR}
                            />
                        ) : null}

                        {isAdmin ? (
                            <Tab label="Dist." value={DISTRIBUTOR} />
                        ) : null}

                        {isAdmin ? (
                            <Tab label="Retailer" value={RETAILER} />
                        ) : null}

                        {isAdmin ? (
                            <Tab label="API User" value={API_USER} />
                        ) : null}

                        {isAdmin ? <Tab label="SD FOS" value={SDFOS} /> : null}

                        {isAdmin ? <Tab label="D FOS" value={DFOS} /> : null}

                        {currentUserTypeID === SUPER_DISTRIBUTOR ? (
                            <Tab label="Dist." value={DISTRIBUTOR} />
                        ) : null}

                        {currentUserTypeID === SUPER_DISTRIBUTOR ? (
                            <Tab label="Retailer" value={RETAILER} />
                        ) : null}

                        {currentUserTypeID === SUPER_DISTRIBUTOR ? (
                            <Tab label="SD FOS" value={SDFOS} />
                        ) : null}

                        {currentUserTypeID === DISTRIBUTOR ? (
                            <Tab label="Retailer" value={RETAILER} />
                        ) : null}
                        {currentUserTypeID === DISTRIBUTOR ? (
                            <Tab label="D FOS" value={DFOS} />
                        ) : null}

                        {currentUserTypeID === SUPPORT_USER ? (
                            <Tab label="Support" value={SUPPORT_USER} />
                        ) : null}
                        {currentUserTypeID === ADMIN_CODE ? (
                            <Tab label="Support" value={SUPPORT_USER} />
                        ) : null}

                        {currentUserTypeID === CLIENT_CODE ? (
                            <Tab label="Client" value={CLIENT_CODE} />
                        ) : null}
                        {isAdmin ? (
                            <Tab label="Client" value={CLIENT_CODE} />
                        ) : null}
                        {isAdmin ? (
                            <Tab label="DeActive" value={DEACTIVE_USERS} />
                        ) : null}
                    </Tabs>
                </Grid>
            </Grid>
            {UserInfoTable}
            {DeleteConfirmationModal}
            {SendPasswordConfirmationModal}
            {FundTransferModal}
            {OpenDaybookModal}
            {LedgerModal}
            {UserCommissionModal}
            {OpenEnableDisableModal}
            {OpenDownlineModal}
            {ReKYCConfirmationModal}
            <HeaderSummary headerSummary={headerSummary} />
        </CustomContainer>
    );
};
