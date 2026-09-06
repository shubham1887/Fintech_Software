import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, FieldArray, Field } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';
import { LOGOUT } from 'ConstComponents';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import { Select as SLAB_SELECT } from 'Components/Api/SlabApi';
import { SelectByUserID as CHANNEL_SLAB_SELECT_BY_USERID } from 'Components/Api/ChannelSlabAPI';
import { SelectUpline as USER_SELECT_UPLINE } from 'Components/Api/UsersApi';
import { SelectAliasName as SELECT_AliasName } from 'Components/Api/UserTypeApi';

import {
    Add as USER_ADD,
    FetchDetailsForPincode as FETCH_DETAILS_FOR_PINCODE,
    PanView as VERIFY_PAN,
    GSTView as FETCH_DETAILS_FOR_GST
} from 'Components/Api/UsersApi';
import { Select } from 'Components/Api/StatesApi';
import { Select as USERTYPE_SELECT } from 'Components/Api/UserTypeApi';
import { SelectBySlabMasterID as SLAB_DETAIL_SELECT } from 'Components/Api/SlabDetailApi';
import { EKYCVerifyPANDetails } from 'Pages/InnerPages/Users/EKYCVerifyPANDetails';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import SearchIcon from 'Components/SvgButtons/SearchIcon';
import { CustomModal } from 'Components/Modal/CustomModal';
import {
    Container,
    MenuItem,
    Grid,
    Paper,
    Button,
    CircularProgress,
    Box,
    TextField
} from '@material-ui/core';
import Autocomplete from '@material-ui/lab/Autocomplete';
import { FetchDetailsForPincode } from './FetchDetailsForPincode';
import { FetchDetailsForGST } from './FetchDetailsForGST';
import { ADMIN, SUPPORT_USER } from 'Components/Helper/UsertypeCode';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    ADMIN_CODE,
    SUPPORT_CODE
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
import SlabDetailList from './SlabDetailList';
import { VARIANT } from 'Config';
const CustomContainer = HOCLoader(Container);

export const UserAdd = () => {
    const classes = commonStyle();
    const history = useHistory();

    const formRef = useRef();

    const [modalOpen, setModalOpen] = useState(false);
    const [isPincodeCheck, setPincodeCheck] = useState(false);
    const [isGSTCheck, setGSTCheck] = useState(false);
    const [stateList, setStateList] = useState([]);

    const [slabList, setSlabList] = useState([]);
    const [ChannelslabList, setChannelSlabList] = useState([]);
    const [userTypeList, setUserTypeList] = useState([]);
    const [serviceList, setServiceList] = useState([]);
    const [SlabDetails, setSlabDetails] = useState([]);
    const [upline, setUpline] = useState([]);
    const [loading, setLoading] = useState(false);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [FullName, setFullName] = useState('');
    const [PanStatus, setPanStatus] = useState('');
    const [isError, setIsError] = useState(false);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [isDisabled, setIsDisabled] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [currentUserTypeid] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );
    const [currentUserIsChannel] = useState(localStorage.getItem('IsChannel'));
    var IsCreditInstitution =
        localStorage.getItem('IsCreditInstitution') === 'yes';

    const [pincodeDetails, setPincodeDetails] = useState({
        CityName: '',
        StateID: '',
        StateName: '',
        District: '',
        Circle: '',
        Division: '',
        ShopAddress: '',
        setFieldValue: null
    });
    const [gstDetails, setGstDetails] = useState({
        CompanyName: '',
        Name: '',
        Address: '',
        DateOfRegistration: '',
        State: '',
        TaxPayerType: ''
    });
    const selectStates = async () => {
        const response = await Select();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setStateList(data);
    };
    const [stateDefault, setStateDefault] = useState({
        StateID: 0,
        Name: 'Select State'
    });
    const [ParentIDDefault, setParentIDDefault] = useState({
        UserID: 0,
        FirmName: 'Select Upline'
    });
    const [SlabIDDefault, setSlabIDDefault] = useState({
        SlabID: 0,
        Name: 'Select Slab'
    });

    const [UserObj, setUserObj] = useState({
        UserTypeID: 0,
        Upline: ParentIDDefault,
        AliasName: '',
        FirmName: '',
        PersonName: '',
        MobileNumber: '',
        EmailID: '',
        Address: '',
        GSTNo: '',
        PANNo: '',
        AadharNo: '',
        UserServiceList: [],
        Slab: SlabIDDefault,
        ChannelSlabMasterID: 0,
        AllowRealAPI: false,
        IsChannel: false,
        State: stateDefault,
        IsTwoFA: false,
        Pincode: '',
        CityName: '',
        ShopAddress: '',
        WhatsAppNumber: ''
    });

    const selectSlabs = async () => {
        if (currentUserTypeid !== ADMIN && currentUserTypeid !== SUPPORT_USER)
            return;

        const response = await SLAB_SELECT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);

        setSlabList(data);
    };
    const OnUplineChange = async (name, value, setFieldValue) => {
        selectChannelSlabs(value.UserID);
    };
    const selectChannelSlabs = async (ParentID) => {
        if (
            ParentID === null ||
            ParentID === undefined ||
            ParentID === 0 ||
            ParentID === '0'
        )
            return;
        var requestObj = {
            UserID: ParentID
        };
        const response = await CHANNEL_SLAB_SELECT_BY_USERID(
            JSON.stringify(requestObj)
        );

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }
        var data = JSON.parse(response.data);

        setChannelSlabList(data);
    };
    const selectUserTypes = async () => {
        const { isAuthorized, status, data } = await USERTYPE_SELECT();

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setSeverity('error');
            setResponseMessage(data);
            setIsError(true);
            return;
        }

        var usertypedata = JSON.parse(data);
        setUserTypeList(usertypedata);
    };

    const selectService = async () => {
        const { isAuthorized, status, data } = await SERVICE_SELECT_ALL();

        if (!isAuthorized) LOGOUT();

        if (!status) return;

        var servicedata = JSON.parse(data);

        setServiceList(servicedata);
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

        const { isAuthorized, status, data } = await USER_SELECT_UPLINE(
            request
        );

        if (!isAuthorized) LOGOUT();

        if (!status) setIsError(true);

        var uplinedata = JSON.parse(data);
        setUpline(uplinedata);
    };

    const OnRoleChange = async (name, value, setFieldValue) => {
        var UserTypeID = value; //event.target.value;

        setFieldValue(name, value);

        selectUpline(UserTypeID);
        selectAliasName(UserTypeID, setFieldValue);
    };
    const selectAliasName = async (UserTypeID, setFieldValue) => {
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

        const { isAuthorized, status, data } = await SELECT_AliasName(request);

        if (!isAuthorized) {
            LOGOUT();
            return;
        }

        if (!status) {
            setIsError(true);
            return;
        }

        setFieldValue('AliasName', data);
    };

    const OnSlabChange = async (name, value, setFieldValue) => {
        var SlabID = value;
        setFieldValue(name, value);

        var jSONObj = {
            SlabMasterID: SlabID
        };
        const response = await SLAB_DETAIL_SELECT(JSON.stringify(jSONObj));
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            return;
        }

        setSlabDetails(response.data);

        setModalOpen(true);
    };
    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUserTypeid)) setIsAdminRole(true);
        if (SUPPORT_CODE === parseInt(currentUserTypeid)) setIsAdminRole(true);
    };

    useEffect(() => {
        selectStates();
        selectUserTypes();
        selectService();
        selectSlabs();
        CheckUserRole();
    }, []);

    const isShowChannelDropDown = () => {
        if (
            currentUserIsChannel === 'true' &&
            currentUserTypeid !== ADMIN &&
            currentUserTypeid !== SUPPORT_USER
        ) {
            return true;
        }

        if (formRef.current) {
            let valll = formRef.current.values.IsChannel;
            if (valll === true) {
                return true;
            }
            if (valll === false) {
                return false;
            }
        }

        if (
            (currentUserIsChannel === 'true' && currentUserTypeid === ADMIN) ||
            currentUserTypeid === SUPPORT_USER
        ) {
            return true;
        }
        return false;
    };

    const isShowSlabDropDown = () => {
        if (currentUserTypeid !== ADMIN && currentUserTypeid !== SUPPORT_USER) {
            return false;
        }

        if (formRef.current) {
            let valll = formRef.current.values.IsChannel;
            if (valll === true) {
                return false;
            }
            if (valll === false) {
                return true;
            }
        }
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoading(true);
        setIsError(false);

        var inputObj = {
            UserTypeID: values.UserTypeID,
            ParentID: values.Upline.UserID,
            FirmName: values.FirmName,
            StateID: values.State.StateID,
            PersonName: values.PersonName,
            AliasName: values.AliasName,
            MobileNumber: values.MobileNumber,
            EmailID: values.EmailID,
            Address: values.Address,
            GSTNo: values.GSTNo,
            PANNo: values.PANNo,
            AadharNo: values.AadharNo,
            SlabID: values.Slab.SlabID,
            IsChannel: values.IsChannel,
            UserServiceList: values.UserServiceList,
            IsTwoFA: false,
            Pincode: values.Pincode,
            CityName: values.CityName,
            ShopAddress: values.ShopAddress,
            WhatsAppNumber: values.WhatsAppNumber,
            ChannelSlabMasterID: values.ChannelSlabMasterID
        };

        const { isAuthorized, status, data } = await USER_ADD(
            JSON.stringify(inputObj)
        );
        setLoading(false);
        if (!isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(data);
        setIsError(true);
        if (!status) {
            setSeverity('error');
            return;
        }

        setSubmitting(false);

        resetForm(UserObj);

        history.push('/Users/Index');
    };

    const validationSchema = Yup.object({
        UserTypeID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'role')
            .required(PLEASE_SELECT_VALID + 'role'),

        Upline: Yup.object()
            .nullable(PLEASE_SELECT_VALID + ' parent')
            .required(PLEASE_SELECT_VALID + ' parent'),
        // .shape({
        //     UserID: Yup.string()
        //         .notOneOf(['0'], PLEASE_SELECT_VALID + 'parent')
        //         .required(PLEASE_SELECT_VALID + 'parent')
        // }),
        FirmName: Yup.string()
            .trim()
            .required(PLEASE_PROVIDE_VALID + 'firm name'),
        PersonName: Yup.string()
            .trim()
            .required(PLEASE_PROVIDE_VALID + 'person name'),
        MobileNumber: Yup.string()
            .trim()
            .length(10, PLEASE_PROVIDE_VALID + 'mobile number')
            .required(PLEASE_PROVIDE_VALID + 'mobile number'),
        EmailID: Yup.string()
            .trim()
            .email()
            .required(PLEASE_PROVIDE_VALID + 'email address'),
        Address: Yup.string()
            .trim()
            .required(PLEASE_PROVIDE_VALID + 'address')
            .max(150, 'Please enter address with 150 char. only')
    });
    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
    };

    const PANcardValidation = (text) => {
        let regex = /^([a-zA-Z]){5}([0-9]){4}([a-zA-Z]){1}?$/;

        if (regex.test(text)) {
            return true;
        }

        return false;
    };

    const VerifyPan = async (values, setFieldValue) => {
        setLoading(true);
        if (
            values.PANNo == undefined ||
            values.PANNo == null ||
            values.PANNo.length != 10 ||
            PANcardValidation(values.PANNo) == false
        ) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage('Please ,Enter valid PAN number');
            setLoading(false);
            return;
        }

        var request = {
            PanNumber: values.PANNo
        };
        var requestObj = JSON.stringify(request);

        setIsError(false);

        var response = await VERIFY_PAN(requestObj);

        if (!response.isAuthorized) LOGOUT();
        setLoading(false);
        setIsError(true);
        setSeverity('success');

        var data = JSON.parse(response.data);
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(data.Message);

            return;
        }

        setFieldValue('PersonName', data.FullName);
        setFieldValue('PANNo', values.PANNo);

        setIsDisabled(true);
        setFullName(data.FullName);
        setPanStatus(data.Status);
        setIsAlertOpen(true);
    };

    function validatePIN(pin) {
        return /^(\d{4}|\d{6})$/.test(pin);
    }

    const FetchDetails = async (values, setFieldValue) => {
        var request = {
            Pincode: values.Pincode
        };

        setLoading(true);
        if (
            values.Pincode == undefined ||
            values.Pincode == null ||
            values.Pincode.length != 6 ||
            validatePIN(values.Pincode) == false
        ) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage('Please ,Enter valid Pincode number');
            setLoading(false);
            return;
        }

        var requestObj = JSON.stringify(request);

        //setLoading(true);
        setIsError(false);

        var response = await FETCH_DETAILS_FOR_PINCODE(requestObj);

        if (!response.isAuthorized) LOGOUT();

        setLoading(false);
        setIsError(true);
        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);

        setFieldValue('State.Name', data.State);
        setFieldValue('State.StateID', data.StateID);
        setFieldValue('StateID', data.StateID);
        setFieldValue('Address', data.CityName);
        setFieldValue('ShopAddress', data.ShopAddress);

        setFieldValue('CityName', data.District);
        setPincodeDetails({
            ...pincodeDetails,
            CityName: data.CityName,
            ShopAddress: data.ShopAddress,
            StateID: data.StateID,
            StateName: data.State,
            District: data.District,
            Circle: data.Circle,
            Division: data.Division,
            setFieldValue: setFieldValue
        });

        setPincodeCheck(true);
    };
    const FetchGSTDetails = async (values) => {
        var request = {
            GSTNumber: values.GSTNo
        };
        var requestObj = JSON.stringify(request);

        setLoading(true);
        setIsError(false);

        var response = await FETCH_DETAILS_FOR_GST(requestObj);

        if (!response.isAuthorized) LOGOUT();

        setLoading(false);
        setIsError(true);
        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);

        setGstDetails({
            ...gstDetails,
            CompanyName: data.CompanyName,
            Name: data.Name,
            Address: data.Address,
            DateOfRegistration: data.DateOfRegistration,
            State: data.State,
            TaxPayerType: data.TaxPayerType
        });
        setGSTCheck(true);
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
                enableReinitialize={true}
                initialValues={UserObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={UserObj}
                render={({ values, setFieldValue }) => {
                    return (
                        <Form>
                            <Paper
                                style={{
                                    padding: 40
                                }}
                                className={classes.PaperStyle}
                            >
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <h2 className={classes.PaperHead_h2}>
                                            User Role & upline
                                            <span className={classes.Require}>
                                                *
                                            </span>
                                        </h2>
                                    </Grid>

                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={6}
                                        lg={6}
                                        style={{
                                            display: 'flex',
                                            direction: 'rtl'
                                        }}
                                    >
                                        <h2 className={classes.PaperHead_h2}>
                                            <Button
                                                variant="contained"
                                                color="primary"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={() => {
                                                    history.push('/Users');
                                                }}
                                            >
                                                User's List
                                            </Button>
                                        </h2>
                                    </Grid>
                                </Grid>
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
                                                {userTypeList.map(
                                                    (
                                                        {
                                                            UserTypeID,
                                                            UserType
                                                        },
                                                        index
                                                    ) => (
                                                        <MenuItem
                                                            key={index}
                                                            value={UserTypeID}
                                                        >
                                                            {UserType}
                                                        </MenuItem>
                                                    )
                                                )}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
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
                                                //readOnly={isDisabled}
                                            />
                                        </Grid>
                                    </Grid>
                                </Grid>

                                <h2 className={classes.PaperHead_h2}>
                                    Personal Details
                                    <span className={classes.Require}>*</span>
                                </h2>
                                <Grid item xs={12}>
                                    <Grid container spacing={2}>
                                        <Grid item xs={11} sm={11} md={3}>
                                            <MaterialTextField
                                                label="Alias Name"
                                                name="AliasName"
                                                type="text"
                                                title="Alias Name"
                                            />
                                        </Grid>
                                        <Grid item xs={11} sm={11} md={3}>
                                            <MaterialTextField
                                                label="Firm Name"
                                                name="FirmName"
                                                type="text"
                                                title="Firm Name"
                                            />
                                        </Grid>
                                        <Grid item xs={11} sm={11} md={3}>
                                            <MaterialNumberField
                                                label="Mobile Number"
                                                name="MobileNumber"
                                                type="text"
                                                length={10}
                                                title="Mobile Number"
                                            />
                                        </Grid>

                                        <Grid item xs={11} sm={11} md={3}>
                                            <MaterialNumberField
                                                label="WhatsAppNumber"
                                                name="WhatsAppNumber"
                                                type="text"
                                                length={15}
                                                title="WhatsAppNumber"
                                            />
                                        </Grid>

                                        <Grid item xs={11} sm={11} md={3}>
                                            <MaterialTextField
                                                label="Email ID "
                                                name="EmailID"
                                                type="text"
                                                title="Email ID"
                                            />
                                        </Grid>

                                        {IsCreditInstitution && (
                                            <Grid item xs={11} sm={11} md={3}>
                                                <MaterialTextField
                                                    label="Unique Cust Code"
                                                    name="uniqueCustCode"
                                                    type="text"
                                                    length={50}
                                                    title="Unique Cust Code"
                                                />
                                            </Grid>
                                        )}
                                    </Grid>
                                </Grid>

                                <h2 className={classes.PaperHead_h2}>
                                    Address
                                </h2>
                                <Grid item xs={12}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <Field
                                                component={AutoCompleteTextView}
                                                defaultValue={stateDefault}
                                                FieldName="State"
                                                SubFieldName="StateID"
                                                name="State"
                                                options={stateList}
                                                getOptionLabel={(option) =>
                                                    option.Name
                                                }
                                                textFieldProps={{
                                                    label: 'Select State'
                                                }}
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    setStateDefault({
                                                        StateID: value.StateID,
                                                        Name: value.Name
                                                    });
                                                }}
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
                                                length={6}
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
                                            <MaterialTextField
                                                label="ShopAddress"
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
                                            md={1}
                                            lg={1}
                                        >
                                            <Button
                                                variant="contained"
                                                color="primary"
                                                endIcon={<SearchIcon />}
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={() => {
                                                    FetchDetails(
                                                        values,
                                                        setFieldValue
                                                    );
                                                }}
                                                style={{
                                                    height: '100%',
                                                    width: '100%'
                                                }}
                                            ></Button>
                                        </Grid>
                                    </Grid>
                                </Grid>

                                <h2 className={classes.PaperHead_h2}>
                                    Tax Details
                                </h2>
                                <Grid item xs={12}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="GST Number"
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
                                            md={1}
                                            lg={1}
                                        >
                                            <Button
                                                variant="contained"
                                                color="primary"
                                                endIcon={<SearchIcon />}
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={() => {
                                                    FetchGSTDetails(values);
                                                }}
                                                style={{
                                                    height: '100%',
                                                    width: '100%'
                                                }}
                                            ></Button>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
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
                                            md={1}
                                            lg={1}
                                        >
                                            <Button
                                                variant="contained"
                                                color="primary"
                                                endIcon={<SearchIcon />}
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={() => {
                                                    VerifyPan(
                                                        values,
                                                        setFieldValue
                                                    );
                                                }}
                                                style={{
                                                    height: '100%',
                                                    width: '100%'
                                                }}
                                            ></Button>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
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

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={12}
                                    lg={12}
                                    container
                                    spacing={2}
                                >
                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <h2 className={classes.PaperHead_h2}>
                                            Service
                                        </h2>

                                        <Grid container spacing={2}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                                className={classes.multiSelect}
                                            >
                                                <FieldArray
                                                    name="UserServiceList"
                                                    render={(serviceIdList) => (
                                                        <Autocomplete
                                                            multiple
                                                            id="ServiceData"
                                                            options={
                                                                serviceList
                                                            }
                                                            getOptionLabel={(
                                                                option
                                                            ) =>
                                                                option.ServiceName
                                                            }
                                                            onChange={(
                                                                event,
                                                                values
                                                            ) => {
                                                                values.UserServiceList = [];
                                                                values.map(
                                                                    (
                                                                        category
                                                                    ) => {
                                                                        serviceIdList.push(
                                                                            {
                                                                                ServiceID:
                                                                                    category.ServiceID
                                                                            }
                                                                        );
                                                                        values.UserServiceList.push(
                                                                            {
                                                                                ServiceID:
                                                                                    category.ServiceID
                                                                            }
                                                                        );
                                                                    }
                                                                );
                                                            }}
                                                            renderInput={(
                                                                params
                                                            ) => (
                                                                <TextField
                                                                    variant={
                                                                        VARIANT
                                                                    }
                                                                    {...params}
                                                                    label="Service(s)"
                                                                    placeholder="Service"
                                                                />
                                                            )}
                                                        />
                                                    )}
                                                />
                                                <div
                                                    style={{
                                                        color: 'green'
                                                    }}
                                                >
                                                    Note: By default All
                                                    services are activated
                                                </div>
                                            </Grid>
                                        </Grid>
                                    </Grid>

                                    {isShowChannelDropDown() ? (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <h2
                                                className={classes.PaperHead_h2}
                                            >
                                                Channel Slab
                                            </h2>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <MaterialSelect
                                                    label="Channel Slab"
                                                    name="ChannelSlabMasterID"
                                                    title="Channel Slab"
                                                >
                                                    <MenuItem value="0">
                                                        Select Channel Slab
                                                    </MenuItem>
                                                    {ChannelslabList.map(
                                                        (item) => (
                                                            <MenuItem
                                                                key={
                                                                    item.ChannelSlabMasterID
                                                                }
                                                                value={
                                                                    item.ChannelSlabMasterID
                                                                }
                                                            >
                                                                {item.Name}
                                                            </MenuItem>
                                                        )
                                                    )}
                                                </MaterialSelect>
                                            </Grid>
                                        </Grid>
                                    ) : null}

                                    {isShowSlabDropDown() ? (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <h2
                                                className={classes.PaperHead_h2}
                                            >
                                                Slab
                                            </h2>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <Field
                                                    component={
                                                        AutoCompleteTextView
                                                    }
                                                    defaultValue={SlabIDDefault}
                                                    FieldName="Slab"
                                                    SubFieldName="SlabID"
                                                    name="Slab"
                                                    options={slabList}
                                                    getOptionLabel={(option) =>
                                                        option.Name
                                                    }
                                                    textFieldProps={{
                                                        label: 'Select Slab'
                                                    }}
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        setSlabIDDefault({
                                                            SlabID:
                                                                value.SlabID,
                                                            Name: value.Name
                                                        });
                                                    }}
                                                />
                                            </Grid>
                                        </Grid>
                                    ) : null}

                                    {isAdminRole && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <Grid container spacing={2}>
                                                {/* <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <FormikMaterialSwitch
                                                        label="AllowRealAPI"
                                                        name="AllowRealAPI"
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
                                                </Grid> */}
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <FormikMaterialSwitch
                                                        label="IsChannel"
                                                        name="IsChannel"
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
                                                </Grid>
                                                {/* <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <FormikMaterialSwitch
                                                        label="IsTwoFA"
                                                        name="IsTwoFA"
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
                                                </Grid>*/}
                                            </Grid>
                                        </Grid>
                                    )}
                                </Grid>
                            </Paper>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Register"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Register
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

            {modalOpen ? (
                <CustomModal
                    isOpen={modalOpen}
                    title="Slab detail"
                    width="75%"
                    onDismissCallback={(value) => {
                        setModalOpen(value);
                    }}
                >
                    <SlabDetailList
                        SlabDetails={SlabDetails}
                        onDismissCallback={(value) => {
                            setModalOpen(value);
                        }}
                    />
                </CustomModal>
            ) : null}

            {isAlertOpen ? (
                <EKYCVerifyPANDetails
                    isOpen={isAlertOpen}
                    title="Verify PAN"
                    name={FullName}
                    status={PanStatus}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}

            {isPincodeCheck ? (
                <FetchDetailsForPincode
                    isOpen={isPincodeCheck}
                    request={pincodeDetails}
                    title="Verify Pincode"
                    negativeButtonClick={() => {
                        setPincodeCheck(false);
                        setStateDefault({
                            StateID: pincodeDetails.StateID,
                            Name: pincodeDetails.StateName
                        });
                    }}
                />
            ) : null}
            {isGSTCheck ? (
                <FetchDetailsForGST
                    isOpen={isGSTCheck}
                    request={gstDetails}
                    title="GST Details"
                    negativeButtonClick={() => {
                        setGSTCheck(false);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
