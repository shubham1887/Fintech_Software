import React, { useEffect, useState } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import { SelectUpline as USER_SELECT_UPLINE } from 'Components/Api/UsersApi';
import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';
import { Update as USER_UPDATE } from 'Components/Api/UsersApi';
import { SelectUser as USER_SELECT_BY_ID } from 'Components/Api/UsersApi';
import { Select as USERTYPE_SELECT } from 'Components/Api/UserTypeApi';
import { Select as STATES_SELECT_ALL } from 'Components/Api/StatesApi';
import { SelectRechargeAndDMT as WALLET_SELECT } from 'Components/Api/WalletApi';
import { useHistory } from 'react-router';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { API_USER, RETAILER } from 'Components/Helper/UsertypeCode';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import {
    Container,
    MenuItem,
    Grid,
    Paper,
    Button,
    Box,
    TextField,
    CircularProgress
} from '@material-ui/core';
import { ADMIN } from 'Components/Helper/UsertypeCode';

import Autocomplete from '@material-ui/lab/Autocomplete';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MESSAGE_CLOSE_AFTER } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { VARIANT } from 'Config';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';

const CustomContainer = HOCLoader(Container);
export const UserEdit = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [stateList, setStateList] = useState([]);
    const [userTypeList, setUserTypeList] = useState([]);
    const [upline, setUpline] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [serviceList, setServiceList] = useState([]);
    const [wallets, setWallets] = useState([]);
    const [userServiceMappingList, setUserServiceMappingList] = useState([]);
    //const [currentUsertypeID, setCurrentUsertypeID] = useState('');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [isAPIRole, setIsAPIRole] = useState(false);
    var currentUsertypeID = localStorage.getItem('UserTypeID');
    const [ParentIDDefault, setParentIDDefault] = useState({
        UserID: 0,
        FirmName: 'Select Upline'
    });

    const [UserObj, setUserObj] = useState({
        UserID: 0,
        UserTypeID: 0,
        ParentID: 0,
        FirmName: '',
        AliasName: '',
        PersonName: '',
        MobileNumber: '',
        EmailID: '',
        StateID: 0,
        Address: '',
        ShopAddress: '',
        GSTNo: '',
        PANNo: '',
        AadharNo: '',
        TargetAmount: 0,
        UserServiceList: [],
        AllowRealAPI: false,
        IsChannel: false,
        VirtualAccountNumber: '',
        IsTwoFA: false,
        Pincode: '',
        CityName: '',
        WalletIDforEcollection: 0,
        StaticQRWalletID: 0,
        MaxOutstanding: 0,
        PayoutAccountCount: 0,
        ResponseType: 'ALL',
        WhatsAppNumber: ''
    });
    const selectWallets = async () => {
        const response = await WALLET_SELECT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);

        setWallets(data);
    };
    const WalletList = wallets.map((item) => (
        <MenuItem key={item.WalletID} value={item.WalletID}>
            {item.Name}
        </MenuItem>
    ));

    const selectService = async () => {
        const response = await SERVICE_SELECT_ALL();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setServiceList(data);
    };

    const selectStates = async () => {
        const response = await STATES_SELECT_ALL();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setStateList(data);
    };

    const selectUserTypes = async () => {
        const response = await USERTYPE_SELECT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setUserTypeList(data);
    };

    const selectUpline = async (UserTypeID) => {
        if (
            UserTypeID === null ||
            UserTypeID === undefined ||
            UserTypeID === 0 ||
            UserTypeID === '0'
        )
            return;

        var requestObj = {
            UserTypeID: UserTypeID
        };
        var request = JSON.stringify(requestObj);

        const response = await USER_SELECT_UPLINE(request);

        if (!response.isAuthorized) {
            LOGOUT();
        }

        setResponseMessage(response.data);
        if (!response.status) {
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setUpline(data);
    };
    const OnUplineChange = async (name, value, setFieldValue) => {
        //selectChannelSlabs(value.UserID);
    };
    const OnRoleChange = async (name, value, setFieldValue) => {
        var UserTypeID = value; //event.target.value;

        setFieldValue(name, value);

        selectUpline(UserTypeID);
    };

    const selectUser = async () => {
        let URL = window.location.pathname;
        var res = URL.split('/');

        let UserID = res[res.length - 1];

        let JSONObj = {
            UserID: UserID
        };

        var request = JSON.stringify(JSONObj);

        const response = await USER_SELECT_BY_ID(request);

        if (!response.isAuthorized) LOGOUT();

        setLoadingOpen(false);
        var data = JSON.parse(response.data);
        selectUpline(data.UserTypeID);

        setUserObj({
            UserID: data.UserID,
            FirmName: data.FirmName,
            AliasName: data.AliasName,
            PersonName: data.PersonName,
            MobileNumber: data.MobileNumber,
            ParentID: data.ParentID,
            EmailID: data.EmailID,
            StateID: data.StateID,
            Address: data.Address,
            ShopAddress: data.ShopAddress,
            GSTNo: data.GSTNo,
            PANNo: data.PANNo,
            AadharNo: data.AadharNo,
            UserTypeID: data.UserTypeID,
            TargetAmount: data.TargetAmount,
            UserServiceList: data.UserServiceList ? data.UserServiceList : [],
            AllowRealAPI: data.AllowRealAPI,
            IsChannel: data.IsChannel,
            VirtualAccountNumber: data.VirtualAccountNumber,
            IsTwoFA: false,
            Pincode: data.Pincode,
            CityName: data.CityName,
            WalletIDforEcollection: data.WalletIDforEcollection,
            StaticQRWalletID: data.StaticQRWalletID,
            MaxOutstanding: data.MaxOutstanding,
            PayoutAccountCount: data.PayoutAccountCount,
            ResponseType: data.ResponseType,
            WhatsAppNumber: data.WhatsAppNumber
        });

        //setCurrentUsertypeID(data.UserTypeID);

        if (data.UserServiceList === undefined) return;

        if (data.UserServiceList === null) return;

        SeletedServicesIndex(data.UserServiceList);

        setParentIDDefault({
            UserID: data.ParentID,
            FirmName: data.ParentFirmName
        });
    };

    const SeletedServicesIndex = (list) => {
        var arryIndex = [];

        for (var parent in list) {
            for (var child in serviceList) {
                if (list[parent].ServiceID === serviceList[child].ServiceID) {
                    arryIndex.push(serviceList[child]);
                }
            }
        }

        setUserServiceMappingList(arryIndex);
    };

    const CheckUserRole = () => {
        if (RETAILER === parseInt(currentUsertypeID)) return true;
        if (API_USER === parseInt(currentUsertypeID)) return true;
        if (ADMIN === parseInt(currentUsertypeID)) {
            setIsAdminRole(true);
            return true;
        }
        return false;
    };

    useEffect(() => {
        setLoadingOpen(true);
        selectService();
        selectStates();
        selectUserTypes();
        selectUser();
        selectWallets();
        CheckUserRole();
    }, [serviceList.length, isAdminRole]);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }

        if (values.Upline != undefined && values.Upline != null)
            values.ParentID = values.Upline.UserID;

        setLoading(true);
        var requestObj = JSON.stringify(values);

        var response = await USER_UPDATE(requestObj);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setLoading(false);
            setSubmitting(false);
            setSeverity('error');
            return;
        }

        setSubmitting(false);
        resetForm(UserObj);
        setTimeout(() => {
            history.push('/Users/Index');
        }, MESSAGE_CLOSE_AFTER);
    };

    const RegistrationScheama = Yup.object({
        UserTypeID: Yup.string()
            .notOneOf(['0'], 'Please select valid Role')
            .required('Required'),
        ParentID: Yup.string()
            .notOneOf(['0'], 'Please select valid parent')
            .required('Required'),
        FirmName: Yup.string().required('Please enter valid firm name'),
        PersonName: Yup.string().required('Please enter valid person name'),
        MobileNumber: Yup.string()
            .length(10, 'Please enter valid mobile number')
            .required('Required'),
        EmailID: Yup.string().required('Please enter valid email address'),
        StateID: Yup.string()
            .notOneOf(['0'], 'Please select valid State')
            .required('Required'),
        Address: Yup.string().required('Please enter valid address')
    });
    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
    };

    const OnAmountChange = (value) => {
        if (value === undefined || value <= 0) {
            return;
        }
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
                enableReinitialize={true}
                initialValues={UserObj}
                validationSchema={RegistrationScheama}
                onSubmit={submithandle}
                resetForm={UserObj}
                render={({ values, setFieldValue, handleChange, errors }) => {
                    return (
                        <Form>
                            <Paper className={classes.PaperStyle}>
                                <h2 className={classes.PaperHead_h2}>
                                    User Role & Upline
                                </h2>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                label="Role"
                                                name="UserTypeID"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnRoleChange(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                                title="Role"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Role
                                                </MenuItem>
                                                {userTypeList.map((item) => (
                                                    <MenuItem
                                                        key={item.UserTypeID}
                                                        value={item.UserTypeID}
                                                    >
                                                        {item.UserType}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            {/* <MenuItem value="0">
                                                    Select Upline
                                                </MenuItem>

                                                {upline.map((user) => (
                                                    <MenuItem
                                                        key={user.UserID}
                                                        value={user.UserID}
                                                    >
                                                        {user.FirmName}
                                                    </MenuItem>
                                                ))} */}

                                            <Field
                                                component={AutoCompleteTextView}
                                                defaultValue={ParentIDDefault}
                                                FieldName="Upline"
                                                SubFieldName="UserID"
                                                name="Upline"
                                                options={upline}
                                                getOptionLabel={(option) =>
                                                    option.FirmName
                                                }
                                                textFieldProps={{
                                                    label: 'Select Upline'
                                                }}
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnUplineChange(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );
                                                    setParentIDDefault({
                                                        UserID: value.UserID,
                                                        FirmName: value.FirmName
                                                    });
                                                }}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={12} md={3}>
                                            <MaterialTextField
                                                label="Person Name"
                                                name="PersonName"
                                                type="text"
                                                title="Person Name"
                                            />
                                        </Grid>
                                    </Grid>
                                </Grid>

                                <h2 className={classes.PaperHead_h2}>
                                    Personal Details
                                </h2>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="Alias Name"
                                                name="AliasName"
                                                type="text"
                                                title="Alias Name"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="Firm Name"
                                                name="FirmName"
                                                type="text"
                                                title="Firm Name"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialNumberField
                                                label="Mobile Number"
                                                name="MobileNumber"
                                                type="text"
                                                length={10}
                                                title="Mobile Number"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialNumberField
                                                label="WhatsAppNumber"
                                                name="WhatsAppNumber"
                                                type="text"
                                                length={15}
                                                title="WhatsAppNumber"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="Email ID "
                                                name="EmailID"
                                                type="text"
                                                title="Email ID"
                                            />
                                        </Grid>
                                    </Grid>
                                </Grid>

                                <h2 className={classes.PaperHead_h2}>
                                    Address
                                </h2>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                label="State"
                                                name="StateID"
                                                title="State ID"
                                                value={UserObj.StateID}
                                            >
                                                <MenuItem value="0">
                                                    Select State
                                                </MenuItem>
                                                {stateList.map((item) => (
                                                    <MenuItem
                                                        key={item.StateID}
                                                        value={item.StateID}
                                                    >
                                                        {item.Name}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="Address"
                                                name="Address"
                                                type="TextArea"
                                                icon="shop"
                                                title="Address"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="Shop Address"
                                                name="ShopAddress"
                                                type="TextArea"
                                                icon="shop"
                                                title="Shop Address"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="CityName"
                                                name="CityName"
                                                type="text"
                                                icon="shop"
                                                title="Address"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialNumberField
                                                label="Pin code"
                                                name="Pincode"
                                                type="text"
                                                length={10}
                                                title="Pin code"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialNumberField
                                                label="TargetAmount"
                                                name="TargetAmount"
                                                type="text"
                                                title="TargetAmount"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialCurrency
                                                label="MaxOutstanding"
                                                name="MaxOutstanding"
                                                max="9999999"
                                                placeholder="Max Outstanding"
                                                autoComplete="new-password"
                                                title="MaxOutstanding"
                                                onCustomCallback={(value) => {
                                                    OnAmountChange(value);
                                                }}
                                            />
                                        </Grid>
                                    </Grid>
                                </Grid>

                                <h2 className={classes.PaperHead_h2}>
                                    Tax Details
                                </h2>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialTextField
                                                label="GST Number "
                                                name="GSTNo"
                                                type="text"
                                                length={15}
                                                title="GST Number"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialTextField
                                                label="PAN Number"
                                                name="PANNo"
                                                type="text"
                                                length={10}
                                                title="PAN Number"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialNumberField
                                                label="Aadhar Number"
                                                name="AadharNo"
                                                type="text"
                                                length={12}
                                                title="Aadhar Number"
                                            />
                                        </Grid>
                                    </Grid>
                                </Grid>

                                <h2 className={classes.PaperHead_h2}>
                                    Service
                                </h2>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <Grid container spacing={2}>
                                        {isAdminRole ? (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                className={classes.multiSelect}
                                            >
                                                <Autocomplete
                                                    multiple
                                                    name="UserServiceList"
                                                    options={serviceList}
                                                    value={
                                                        values.UserServiceList
                                                    }
                                                    getOptionLabel={(option) =>
                                                        option.ServiceName
                                                    }
                                                    getOptionSelected={(
                                                        option,
                                                        value
                                                    ) =>
                                                        option.ServiceID ===
                                                        value.ServiceID
                                                    }
                                                    onChange={(
                                                        event,
                                                        CallBackValues
                                                    ) => {
                                                        values.UserServiceList = [];

                                                        CallBackValues.map(
                                                            (category) => {
                                                                values.UserServiceList.push(
                                                                    {
                                                                        ServiceID:
                                                                            category.ServiceID,
                                                                        ServiceName:
                                                                            category.ServiceName
                                                                    }
                                                                );
                                                            }
                                                        );

                                                        SeletedServicesIndex(
                                                            values.UserServiceList
                                                        );
                                                    }}
                                                    renderInput={(params) => (
                                                        <TextField
                                                            variant={VARIANT}
                                                            {...params}
                                                            label="Service(s)"
                                                            placeholder="Service"
                                                        />
                                                    )}
                                                />
                                            </Grid>
                                        ) : null}
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                label="e-Collection Amount deposit in"
                                                name="WalletIDforEcollection"
                                                title="e-Collection Amount deposit in"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Wallet
                                                </MenuItem>
                                                {WalletList}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                label="StaticQR Amount deposit in"
                                                name="StaticQRWalletID"
                                                title="StaticQR Amount deposit in"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Wallet
                                                </MenuItem>
                                                {WalletList}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="Virtual AccountNumber"
                                                name="VirtualAccountNumber"
                                                type="text"
                                                title="Virtual AccountNumber"
                                            />
                                        </Grid>
                                        {isAdminRole ? (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="Payout Account Count "
                                                    name="PayoutAccountCount"
                                                    type="text"
                                                    title="Payout Account Count "
                                                />
                                            </Grid>
                                        ) : null}
                                    </Grid>
                                </Grid>
                            </Paper>
                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Update"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Update
                                        </Button>
                                        {loading && (
                                            <CircularProgress
                                                size={24}
                                                className={
                                                    classes.buttonProgress
                                                }
                                            />
                                        )}
                                    </div>
                                    <Button
                                        type="reset"
                                        title="Reset"
                                        className={
                                            classes.CustomDangerButtonStyle
                                        }
                                    >
                                        Reset
                                    </Button>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            />
        </CustomContainer>
    );
};
