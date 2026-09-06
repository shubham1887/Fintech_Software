import React, { memo, useState, useRef, useEffect } from 'react';
import { Formik, Form, Field } from 'formik';

import {
    Container,
    Grid,
    Accordion,
    Box,
    CircularProgress
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import { DIST_CODE, DMT_WalletID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import excel from 'Assets/Images/excel.svg';
import Icon from '@material-ui/core/Icon';
import HOCLoader from 'Components/Loader/HOCLoader';
import { HeaderSummary } from 'Pages/InnerPages/Users/BalanceHeaderSummary';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import {
    SelectDownlineReport as SELECT_DOWNLINE_REPORT,
    SelectOnlyParentUserForDropdown as USER_SELECT_FOR_DROPDOWN
} from 'Components/Api/UsersApi';
import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);
const DownlineReport = memo(() => {
    const formRef = useRef();
    const classes = commonStyle();
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [isDistRole, setIsDistRole] = useState(false);
    const [loading, setLoading] = useState(false);
    const [IsDMTWallet, setIsDMTWallet] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [openFilter, setOpenFilter] = useState(true);
    const [mainColumns, setMainColumns] = useState([]);
    const [headerSummary, setHeaderSummary] = useState([]);
    const [data, setData] = useState([]);
    const openHandler = (value) => {
        setOpenFilter(value);
    };
    var IsCashCollection = localStorage.getItem('IsCashCollection');

    const [userList, setUserList] = useState([]);
    const [ModelObj] = useState({
        User: {
            UserID: 0,
            FirmName: 'Select Own'
        },
        IsExcelDownload: false
    });
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select Own'
    });
    const CheckUserRole = () => {
        if (DIST_CODE === parseInt(currentUsertypeid)) setIsDistRole(true);
    };
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            UserID: values.User.UserID,
            IsExcelDownload: values.IsExcelDownload
        };

        if (formRef.current.values.IsExcelDownload === false) setData();

        setSubmitting(true);
        setLoading(true);
        //setData([]);

        var requestJSON = JSON.stringify(inputObj);

        const { isAuthorized, status, data } = await SELECT_DOWNLINE_REPORT(
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
        if (d.IsExcelDownload) downloadFile(d.ReportPath, 'User Downline');

        setData(d.data);
        setHeaderSummary(d.headerlist);
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

    useEffect(() => {
        CheckUserRole();
        SelectUser();
        createColumns();
        selectWallets();
    }, [IsDMTWallet]);
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });
        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });

        columns.push({
            label: 'Role',
            name: 'Role'
        });
        columns.push({
            label: 'RCH Bal',
            name: 'RechargeBalance',
            numeric: true
        });
        if (IsDMTWallet === true) {
            columns.push({
                label: 'DMT Bal',
                name: 'DMTBalance',
                numeric: true
            });
        }
        columns.push({
            label: 'AEPS Bal',
            name: 'AEPSBalance',
            numeric: true
        });
        if (IsCashCollection === 'true') {
            columns.push({
                label: 'Outstanding',
                name: 'Outstanding',
                numeric: true
            });
        }
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
                enableReinitialize={true}
                innerRef={formRef}
                initialValues={ModelObj}
                //  validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={ModelObj}
            >
                {({ setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Downline Report'}
                                    open={openFilter}
                                    setOpen={openHandler}
                                >
                                    <Grid container spacing={2}>
                                        {!isDistRole ? (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
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
                                                        label: 'Select User'
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
            <ReactTable data={data} columns={mainColumns} />
            <HeaderSummary headerSummary={headerSummary} />
        </CustomContainer>
    );
});

export default DownlineReport;
