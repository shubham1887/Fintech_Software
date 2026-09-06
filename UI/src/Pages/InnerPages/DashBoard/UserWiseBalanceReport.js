import {
    Container,
    Grid,
    Tab,
    Tabs
} from '@material-ui/core';

import { Formik, Form } from 'formik';
import {
    SelectUserList as USER_SELECT_LIST,
} from 'Components/Api/UsersApi';
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

import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import React, { useEffect, useState, useRef } from 'react';

import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
const CustomContainer = HOCLoader(Container);

const UserWiseBalanceReport = () => {
    const formRef = useRef();

    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [IsTextValue, setIsTextValue] = useState(false);

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [value, setValue] = useState(ALL_USERS);

    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);

    const [UsersList, setUsersList] = useState([]);
    const [search, setSearch] = useState('');
    const [sortColumnName, setSortColumnName] = useState('');
    const [sortDirection, setSortDirection] = useState('');

    const [IsDMTWallet, setIsDMTWallet] = useState(false);
    const [currentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );
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

        columns.push({
            label: 'IsActive',
            name: 'IsActive',
            hide: true
        });

        setMainColumns(columns);
    };

    const handleChange = (event, newValue) => {
        setValue(newValue);
        setTotalCount(0);
        callSelectData(TakeCount, SkipCount, newValue, '', false);
    };

    const customContainerCallback = (alertState) => {
        setIsError(alertState);
    };

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
            </Formik>
            <Grid container spacing={2} style={{ marginTop: '16px' }}>
                <Grid item xs={12} sm={12} md={12} lg={12}>
                    <h3 className="text-center border-bottom pb-2 dashheading">
                        User List
                    </h3>
                </Grid>
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
                            <Tab label="Retailer" value={RETAILER} />
                        ) : null}


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

        </CustomContainer>
    );
};
export default UserWiseBalanceReport;