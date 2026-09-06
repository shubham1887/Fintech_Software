import React, { useState, useEffect, useRef } from 'react';
import { toast } from 'react-toastify';
import { Formik, Form, Field } from 'formik';
import Link from '@material-ui/core/Link';
import * as Yup from 'yup';
import {
    MaterialTextField,
    MaterialTextAreaField
} from 'Components/Inputes/FormikMaterialInput';
import { CLIENT_CODE } from 'Components/Helper/Messages';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import CircularProgress from '@material-ui/core/CircularProgress';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    Button,
    Grid,
    Typography,
    Box,
    MenuItem,
    Container
} from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import Logo from 'Components/Logo/Logo';
import ReCAPTCHA from 'react-google-recaptcha';
import OuterFooter from 'Components/Footer/OuterFooter';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import {
    RE_CAPTCHA_V2_KEY,
    IS_CAPTCHA,
    CLIENT_NAME,
    PLAY_STORE_URL
} from 'Config';
import LoginSliders from 'Components/LoginSliders';
import usePosition from 'Components/Posistion';
import { LOGOUT } from 'ConstComponents';
import { Add as USER_ADD } from 'Components/Api/SignupAPI';
import { SelectState as SELECT_STATE } from 'Components/Api/SignupAPI';
import { SelectRoleForSignUp as USERTYPE_SELECT } from 'Components/Api/SignupAPI';
import {
    SignupeKYCInitiate as EKYC_SIGNUP,
    SignupKYCValidate as EKYC_SIGNUP_VALIDATE
} from 'Components/Api/EKYCAPI';

const PageStyle = {
    height: '100%',
    width: '100%'
};

const SignupWithEkyc = () => {
    const classes = commonStyle();
    const recaptchaRef = useRef(null);
    const [stateList, setStateList] = useState([]);
    const [userTypeList, setUserTypeList] = useState([]);
    const [isCaptcha, setIsCaptcha] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [eKYCMasterID, setEKYCMasterID] = useState(0);

    const [ReferralCode, setReferralCode] = useState('');
    const [isClientRole, setIsClientRole] = useState(false);
    const [severity, setSeverity] = useState('');
    const [isError, setIsError] = useState('');
    const [playStoreURL, setPlayStoreURL] = useState('');
    const [captchaKey, setCaptchaKey] = useState('');
    const { error } = usePosition();
    const [otpDisplayed, setOtpDisplayed] = useState(false);
    const [registerDisplayed, setRegisterDisplayed] = useState(false);
    const [generateotpDisplayed, setGenerateotpDisplayed] = useState(true);

    async function fetchRecaptcha() {
        let IsCaptcha = await IS_CAPTCHA();
        setIsCaptcha(IsCaptcha);
        if (IsCaptcha === 'NO') return;
        setCaptchaKey(await RE_CAPTCHA_V2_KEY());
    }

    async function fetchCName() {
        document.title = await CLIENT_NAME();
    }

    const fetchPlayStoreURL = async () => {
        setPlayStoreURL(await PLAY_STORE_URL());
    };

    toast.configure({
        autoClose: 20 * 1000,
        draggable: true,
        position: toast.POSITION.BOTTOM_RIGHT
    });

    const [stateDefault, setStateDefault] = useState({
        StateID: 0,
        Name: 'Select State'
    });

    const [UserObj, setUserObj] = useState({
        UserTypeID: 0,
        ParentID: 0,
        PersonName: '',
        FirmName: '',
        MobileNumber: '',
        EmailID: '',
        State: stateDefault,
        StateID: 0,
        Address: '',
        Pincode: '',
        uniqueCustCode: '',
        WhatsAppNumber: '',
        AadharNo: '',
        PANNo: '',
        UserServiceList: [],
        OTP: '',
        OTPMasterID: 0,
        IseKYC: true,
        ReferralCode: ReferralCode,
        GenerateOTP: false,
        IsVerify: false,
        IsRegister: false
    });

    const [formData, setFormData] = useState({
        PersonName: '',
        Pincode: '',
        Address: ''
    });

    const selectUserTypes = async (ReferralCode) => {
        let Request = {
            ReferralCode: ReferralCode
        };

        let jSONObj = JSON.stringify(Request);

        const { isAuthorized, status, data } = await USERTYPE_SELECT(jSONObj);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setResponseMessage(data);

            return;
        }

        var usertypedata = JSON.parse(data);
        setUserTypeList(usertypedata);
    };
    const selectStates = async (values) => {
        var request = JSON.stringify(values);
        const response = await SELECT_STATE(request);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
        }

        var data = JSON.parse(response.data);
        setStateList(data);
    };
    useEffect(() => {
        let URL = window.location.pathname;
        var res = URL.split('/');
        let ReferralCode = res[res.length - 1];
        setReferralCode(ReferralCode);
    }, []);

    useEffect(() => {
        selectStates();
        fetchRecaptcha();
        fetchPlayStoreURL();
        fetchCName();
        toast.configure();
        selectUserTypes(ReferralCode);
    }, [ReferralCode]);

    useEffect(() => {
        if (captchaKey === '' || captchaKey.length <= 1) {
            fetchRecaptcha();
        }
    }, [captchaKey]);

    const handleCaptchaChange = (value) => {};

    const validationSchema = Yup.object({
        UserTypeID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'role')
            .required(PLEASE_SELECT_VALID + 'role'),

        FirmName: Yup.string().when(['isClientRole'], {
            is: (isClientRole) => isClientRole === false,
            then: Yup.string()
                .trim()
                .required(PLEASE_PROVIDE_VALID + 'firm name')
                .max(30, 'Please enter firm name with 30 char. only')
        })
    });
    const CustomReCaptcha =
        captchaKey === '' || captchaKey.length <= 1 ? null : (
            <ReCAPTCHA
                ref={recaptchaRef}
                size="invisible"
                theme="dark"
                sitekey={captchaKey}
                onChange={handleCaptchaChange}
            />
        );
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (error) {
            let message = 'Please allow this site to access your location';

            toast.error(message, {
                pauseOnHover: true,
                progress: undefined
            });

            return;
        }

        if (captchaKey === '' || captchaKey.length <= 1 || isCaptcha === 'NO') {
            values.CaptchaToken = '';
        } else {
            values.CaptchaToken = await recaptchaRef.current.executeAsync();
        }

        if (isClientRole) {
            values.FirmName = 'FirmName';

            values.WhatsAppNumber = values.MobileNumber;
        }

        values.Device = 'WEB';
        values.StateID = values.State.StateID;
        values.OTPMasterID = eKYCMasterID;
        values.IseKYC = true;

        values.ReferralCode = ReferralCode;
        var request = JSON.stringify(values);

        if (values.GenerateOTP) {
            let RequestObj = {
                Aadhaar: values.AadharNo
            };
            var requestObj = JSON.stringify(RequestObj);
            setIsError(false);
            setLoadingOpen(true);
            const response = await EKYC_SIGNUP(requestObj);
            setLoadingOpen(false);

            if (!response.isAuthorized) LOGOUT();
            if (!response.status) {
                setSeverity('error');
                setResponseMessage(response.data);
                setIsError(true);
                return;
            }
            let data = JSON.parse(response.data);
            setSeverity('success');
            setResponseMessage(data.Message);
            setEKYCMasterID(data.eKYCMasterID);

            setIsError(true);
            setOtpDisplayed(true);
            setGenerateotpDisplayed(false);
            return;
        }

        if (values.IsVerify) {
            setLoadingOpen(true);
            if (values.OTP.length <= 5) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage('Please enter a valid OTP');
            }

            let RequestObj = {
                OTP: values.OTP,
                eKYCMasterID: eKYCMasterID
            };

            var response = await EKYC_SIGNUP_VALIDATE(
                JSON.stringify(RequestObj)
            );

            setLoadingOpen(false);

            if (!response.isAuthorized) LOGOUT();
            if (!response.status) {
                setSeverity('error');
                setResponseMessage(response.data);
                setIsError(true);
                return;
            }
            let data = JSON.parse(response.data);

            setUserObj({
                PersonName: data.FullName,
                Pincode: data.Pin,
                Address: data.Address
            });
            setSeverity('success');
            setResponseMessage(data.Message);
            setEKYCMasterID(data.eKYCMasterID);

            setIsError(true);
            setOtpDisplayed(false);
            setRegisterDisplayed(true);
            setGenerateotpDisplayed(false);
            return;
        }

        if (values.IsRegister) {
            setLoadingOpen(true);

            const response = await USER_ADD(request);
            setLoadingOpen(false);

            if (!response.status) {
                setResponseMessage(response.data);

                toast.error(response.data, {
                    pauseOnHover: true,
                    progress: undefined
                });
                return;
            }

            setSubmitting(false);
            resetForm(UserObj);
            setEKYCMasterID(0);

            setIsError(true);
            setSeverity('success');
            setResponseMessage(response.data);
            setGenerateotpDisplayed(false);
            setOtpDisplayed(false);
        }
    };

    const OnUserTypeChange = async (name, value) => {
        setIsClientRole(false);
        if (CLIENT_CODE === parseInt(value)) {
            setIsClientRole(true);
        }
    };

    const SliderContainer = (
        <Grid item xs={12} sm={12} md={12} lg={8}>
            <Box
                display={{
                    xs: 'none',
                    sm: 'none',
                    md: 'none',
                    lg: 'block',
                    xl: 'block'
                }}
                className="centerslider"
            >
                <LoginSliders />
            </Box>
        </Grid>
    );

    const FormContainer = (
        <Grid item xs={12} sm={12} md={12} lg={4}>
            <Container>
                <Formik
                    initialValues={UserObj}
                    validationSchema={validationSchema}
                    onSubmit={submithandle}
                    resetForm={UserObj}
                >
                    {({ setFieldValue, handleSubmit }) => {
                        return (
                            <Form className={classes.login_form}>
                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={12}
                                    className="loginlogo"
                                >
                                    <Logo />
                                </Grid>

                                <Grid
                                    container
                                    spacing={2}
                                    className="loginform"
                                >
                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={12}
                                        lg={12}
                                        xl={12}
                                    >
                                        <Typography variant="h6" gutterBottom>
                                            Registration
                                        </Typography>
                                    </Grid>
                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={12}
                                        lg={12}
                                        xl={12}
                                    >
                                        <MaterialSelect
                                            label="Role"
                                            name="UserTypeID"
                                            title="Role"
                                            onCustomCallback={(name, value) => {
                                                OnUserTypeChange(name, value);
                                            }}
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Role
                                            </MenuItem>
                                            {userTypeList.map(
                                                (
                                                    { UserTypeID, UserType },
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

                                    {!isClientRole ? (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <MaterialTextField
                                                label="Firm Name"
                                                name="FirmName"
                                                type="text"
                                                length={30}
                                                title="Firm Name"
                                            />
                                        </Grid>
                                    ) : null}

                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={12}
                                        lg={12}
                                        xl={12}
                                    >
                                        <MaterialNumberField
                                            label="Aadhar Number"
                                            name="AadharNo"
                                            type="text"
                                            length={15}
                                            title="Aadhar Number"
                                        />
                                    </Grid>

                                    {registerDisplayed && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <MaterialTextField
                                                label="Email ID "
                                                name="EmailID"
                                                type="text"
                                                length={50}
                                                title="Email ID"
                                            />
                                        </Grid>
                                    )}

                                    {registerDisplayed && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <Field
                                                component={AsyncAutoComplete}
                                                defaultValue={stateDefault}
                                                FieldName="State"
                                                name="State"
                                                SubFieldName="StateID"
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
                                    )}

                                    {registerDisplayed && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <MaterialNumberField
                                                label="Mobile Number"
                                                name="MobileNumber"
                                                type="text"
                                                length={10}
                                                title="Mobile Number"
                                            />
                                        </Grid>
                                    )}

                                    {registerDisplayed && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <MaterialNumberField
                                                label="WhatsApp Number"
                                                name="WhatsAppNumber"
                                                type="text"
                                                length={15}
                                                title="WhatsApp Number"
                                            />
                                        </Grid>
                                    )}
                                    {registerDisplayed && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <MaterialTextField
                                                label="Person Name"
                                                name="PersonName"
                                                type="text"
                                                length={50}
                                                title="Person Name"
                                                value={UserObj.PersonName}
                                                readOnly
                                            />
                                        </Grid>
                                    )}

                                    {registerDisplayed && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <MaterialTextAreaField
                                                label="Address"
                                                name="Address"
                                                type="TextArea"
                                                icon="shop"
                                                length={50}
                                                title="Address"
                                                value={UserObj.Address}
                                                readOnly
                                            />
                                        </Grid>
                                    )}
                                    {registerDisplayed && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            F
                                            xl={12}
                                        >
                                            <MaterialTextField
                                                label="Pin code"
                                                name="Pincode"
                                                type="text"
                                                length={6}
                                                title="Pin code"
                                                value={UserObj.Pincode}
                                                readOnly
                                            />
                                        </Grid>
                                    )}

                                    {otpDisplayed && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <MaterialNumberField
                                                label="OTP"
                                                name="OTP"
                                                type="text"
                                                length={6}
                                                title="OTP"
                                            />
                                        </Grid>
                                    )}

                                    <div
                                        style={{
                                            color: 'green'
                                        }}
                                    >
                                        {responseMessage}
                                    </div>

                                    {/* {!otpDisplayed && ( */}
                                    {generateotpDisplayed && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <Grid container justify="flex-end">
                                                <Button
                                                    type="submit"
                                                    fullWidth
                                                    title="Generate OTP"
                                                    disabled={loadingOpen}
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'GenerateOTP',
                                                            true
                                                        );

                                                        setFieldValue(
                                                            'IsVerify',
                                                            false
                                                        );
                                                        setFieldValue(
                                                            'IsRegister',
                                                            false
                                                        );
                                                        handleSubmit(e);
                                                    }}
                                                >
                                                    Generate OTP
                                                </Button>
                                                {loadingOpen && (
                                                    <CircularProgress
                                                        size={20}
                                                        className={
                                                            classes.buttonProgress
                                                        }
                                                    />
                                                )}
                                            </Grid>
                                        </Grid>
                                    )}

                                    {otpDisplayed && (
                                        <Grid container justify="space-between">
                                            <Grid item xs={5}>
                                                <div className="flex items-center">
                                                    <Button
                                                        type="submit"
                                                        fullWidth
                                                        variant="contained"
                                                        color="primary"
                                                        disabled={loadingOpen}
                                                        onClick={(e) => {
                                                            setFieldValue(
                                                                'GenerateOTP',
                                                                false
                                                            );
                                                            setFieldValue(
                                                                'IsVerify',
                                                                true
                                                            );
                                                            setFieldValue(
                                                                'IsRegister',
                                                                false
                                                            );

                                                            handleSubmit(e);
                                                        }}
                                                    >
                                                        Verify
                                                    </Button>
                                                    {loadingOpen && (
                                                        <CircularProgress
                                                            size={20}
                                                            className={
                                                                classes.buttonProgress
                                                            }
                                                        />
                                                    )}
                                                </div>
                                            </Grid>
                                            <Grid item xs={5}>
                                                <Button
                                                    type="submit"
                                                    fullWidth
                                                    title="Resend OTP"
                                                    disabled={loadingOpen}
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'GenerateOTP',
                                                            true
                                                        );
                                                        setFieldValue(
                                                            'IsVerify',
                                                            false
                                                        );
                                                        setFieldValue(
                                                            'IsRegister',
                                                            false
                                                        );

                                                        handleSubmit(e);
                                                    }}
                                                >
                                                    Resend OTP
                                                </Button>
                                                {loadingOpen && (
                                                    <CircularProgress
                                                        size={20}
                                                        className={
                                                            classes.buttonProgress
                                                        }
                                                    />
                                                )}
                                            </Grid>
                                        </Grid>
                                    )}

                                    {registerDisplayed && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <Grid container justify="flex-end">
                                                <Button
                                                    type="submit"
                                                    fullWidth
                                                    title="Register"
                                                    disabled={loadingOpen}
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'GenerateOTP',
                                                            false
                                                        );

                                                        setFieldValue(
                                                            'IsVerify',
                                                            false
                                                        );

                                                        setFieldValue(
                                                            'IsRegister',
                                                            true
                                                        );
                                                        handleSubmit(e);
                                                    }}
                                                >
                                                    Register
                                                </Button>
                                                {loadingOpen && (
                                                    <CircularProgress
                                                        size={20}
                                                        className={
                                                            classes.buttonProgress
                                                        }
                                                    />
                                                )}
                                            </Grid>
                                        </Grid>
                                    )}

                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={12}
                                        lg={12}
                                        className={classes.wrapper}
                                    >
                                        <div
                                            style={{
                                                textAlign: 'center',
                                                m: 1
                                            }}
                                        >
                                            Do you want to login?
                                            <Link
                                                className="loginlink"
                                                style={{
                                                    textDecoration: 'none',
                                                    cursor: 'pointer'
                                                }}
                                                href="/Login"
                                            >
                                                Return to Login
                                            </Link>
                                        </div>

                                        <Grid className="donwloadbox">
                                            <Box
                                                sx={{
                                                    textAlign: 'center',
                                                    m: 1
                                                }}
                                            >
                                                <Grid className="downloadtext">
                                                    <span className="uson">
                                                        Download our app on
                                                    </span>
                                                    <span className="mborder">
                                                        &nbsp;
                                                    </span>
                                                </Grid>
                                                <Grid>
                                                    <a
                                                        rel="noreferrer"
                                                        href={playStoreURL}
                                                        target="_blank"
                                                    >
                                                        <img
                                                            src="PlayStore.png"
                                                            height="80"
                                                            alt="Play store"
                                                        />
                                                    </a>
                                                </Grid>
                                            </Box>
                                        </Grid>
                                        <Grid
                                            container
                                            justify="center"
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            style={{
                                                marginTop: 20
                                            }}
                                        >
                                            <Box
                                                sx={{
                                                    textAlign: 'center',
                                                    m: 1
                                                }}
                                            >
                                                <span
                                                    style={{
                                                        float: 'center'
                                                    }}
                                                >
                                                    <p className="terms">
                                                        By sign-up, you agree to
                                                        our
                                                    </p>
                                                    <Link
                                                        className="loginlink"
                                                        href="/TnC.htm"
                                                        style={{
                                                            color: 'blue',
                                                            textDecoration:
                                                                'none'
                                                        }}
                                                    >
                                                        Term and Condition
                                                    </Link>
                                                    and
                                                    <Link
                                                        className="loginlink"
                                                        href="/privacypolicy.htm"
                                                        style={{
                                                            color: 'blue',
                                                            textDecoration:
                                                                'none'
                                                        }}
                                                    >
                                                        Privacy Policy
                                                    </Link>
                                                </span>
                                            </Box>
                                        </Grid>
                                    </Grid>
                                </Grid>
                            </Form>
                        );
                    }}
                </Formik>

                <OuterFooter />
            </Container>
        </Grid>
    );

    return (
        <React.Fragment>
            <Grid container style={PageStyle}>
                {SliderContainer}
                {FormContainer}
            </Grid>
            {CustomReCaptcha}
        </React.Fragment>
    );
};

export default SignupWithEkyc;
