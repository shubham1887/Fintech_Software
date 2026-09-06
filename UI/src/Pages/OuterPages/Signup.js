import React, { useState, useEffect, useRef } from 'react';
import { toast } from 'react-toastify';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import Link from '@material-ui/core/Link';
import * as Yup from 'yup';
import {
    MaterialTextField,
    MaterialTextAreaField
} from 'Components/Inputes/FormikMaterialInput';
import CircularProgress from '@material-ui/core/CircularProgress';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    Button,
    Grid,
    Typography,
    Box,
    Container,
    Checkbox,
    FormControlLabel
} from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import Logo from 'Components/Logo/Logo';
import ReCAPTCHA from 'react-google-recaptcha';
import OuterFooter from 'Components/Footer/OuterFooter';
import CustomRadioButton from 'Components/Inputes/CustomRadioButton';

import {
    RE_CAPTCHA_V2_KEY,
    IS_CAPTCHA,
    CLIENT_NAME,
    PLAY_STORE_URL
} from 'Config';
import LoginSliders from 'Components/LoginSliders';
import usePosition from 'Components/Posistion';
import { LOGOUT } from 'ConstComponents';
import {
    SignUpSendOTP as SIGNUP_OTP,
    VerifySignUpOTP as VARIFY_OTP,
    SignUp as SIGNUP
} from 'Components/Api/SignupAPI';

const PageStyle = {
    height: '100%',
    width: '100%'
};

const Signup = () => {
    const classes = commonStyle();
    const recaptchaRef = useRef(null);
    const [isCaptcha, setIsCaptcha] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [OTPMasterID, setOTPMasterID] = useState(0);

    const [severity, setSeverity] = useState('');
    const [isError, setIsError] = useState('');
    const [playStoreURL, setPlayStoreURL] = useState('');
    const [captchaKey, setCaptchaKey] = useState('');
    const { error } = usePosition();

    const [otpDisplayed, setOtpDisplayed] = useState(false);
    const [otpVarify, setOtpVarify] = useState(false);
    const [isDispaly, setIsDispaly] = useState(true);
    const [isDispalyText, setIsDispalyText] = useState(true);
    const [thankYouMessage, setThankYouMessage] = useState(false);

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

    const [UserObj, setUserObj] = useState({
        MobileNumber: '',
        Pincode: '',
        PANNo: '',
        ReferralCode: '',
        EmployeeID: '',
        OTP: '',
        OTPMasterID: 0,
        GenerateOTP: false,
        IsVerify: false,
        agreeToTerms: false,
        SelectCode: 'No',
        SelectID: 'No',

        IsSignUp: false,
    });

    useEffect(() => {
        fetchCName();
        fetchRecaptcha();
        fetchPlayStoreURL();
        toast.configure();
    }, []);
    useEffect(() => {
        if (captchaKey === '' || captchaKey.length <= 1) {
            fetchRecaptcha();
        }
    }, [captchaKey]);

    const handleCaptchaChange = (value) => { };

    const validationSchema = Yup.object({
        MobileNumber: Yup.string()
            .trim()
            .length(10, PLEASE_PROVIDE_VALID + 'mobile number')
            .required(PLEASE_PROVIDE_VALID + 'mobile number'),

        PANNo: Yup.string()
            .transform((value) => value.toUpperCase())
            .matches(/[A-Z]{5}[0-9]{4}[A-Z]{1}/, PLEASE_PROVIDE_VALID + 'PAN')
            .required(PLEASE_PROVIDE_VALID + 'PAN'),

        Pincode: Yup.string()
            .trim()
            .length(6, PLEASE_PROVIDE_VALID + 'pincode')
            .required(PLEASE_PROVIDE_VALID + 'pincode'),

        agreeToTerms: Yup.boolean().when('IsSignUp', {
            is: true,
            then: Yup.boolean().oneOf([true], 'You must accept the terms and conditions'),
        }),

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

        values.Device = 'WEB';

        values.OTPMasterID = OTPMasterID;
        var request = JSON.stringify(values);

        if (values.GenerateOTP) {
            setIsError(false);
            setLoadingOpen(true);
            const response = await SIGNUP_OTP(request);
            setLoadingOpen(false);
            if (!response.isAuthorized) LOGOUT();
            if (!response.status) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage(response.data);
                return;
            }
            var ReturnResponse = JSON.parse(response.data);

            if (
                ReturnResponse.RedirectUrl.length >= 6 &&
                ReturnResponse.RedirectUrl.trim() !== ''
            ) {
                window.location.href = ReturnResponse.RedirectUrl;
                return;
            }

            if (ReturnResponse.Message === 'OTP sent on your number') {
                setOtpDisplayed(true);
                setIsDispaly(false);
            }
            setOTPMasterID(ReturnResponse.OTPMasterID);
            setResponseMessage(ReturnResponse.Message);
            setSeverity('success');
        }

        if (values.IsVerify) {
            setLoadingOpen(true);
            if (values.OTP.length <= 5) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage('Please enter a valid OTP');
            }
            const response = await VARIFY_OTP(request);

            if (response.data === 'Thank you.') {
                setThankYouMessage(true);
                setIsDispalyText(false);
                setOtpVarify(false);
                setOtpDisplayed(false);
                setResponseMessage('');
                return;
            }
            setLoadingOpen(false);

            if (!response.status) {
                setResponseMessage(response.data);

                toast.error(response.data, {
                    pauseOnHover: true,
                    progress: undefined
                });
                return;
            }

            var ReturnResponse = JSON.parse(response.data);

            setOtpVarify(true);
            setIsDispaly(false);
            setIsDispalyText(true);

            if (ReturnResponse.Message === 'OTP verified successfully') {
                setOtpDisplayed(false);
                setIsDispalyText(false);
            }
            setResponseMessage(ReturnResponse.Message);
            setSeverity('success');

        }


        if (values.IsSignUp) {
            setLoadingOpen(true);
            if (values.OTP.length <= 5) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage('Please enter a valid OTP');
            }
            const response = await SIGNUP(request);
            setLoadingOpen(false);

            if (!response.status) {
                setResponseMessage(response.data);

                toast.error(response.data, {
                    pauseOnHover: true,
                    progress: undefined
                });
                return;
            }
            var ReturnResponse = JSON.parse(response.data);

            if (
                ReturnResponse.RedirectUrl.length >= 6 &&
                ReturnResponse.RedirectUrl.trim() !== ''
            ) {
                window.location.href = ReturnResponse.RedirectUrl;
                return;
            }
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
                    {({ setFieldValue, handleSubmit, values, validateForm }) => {
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
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <Typography
                                            variant="h6"
                                            className="loginhead"
                                            gutterBottom
                                        >
                                            Signup
                                        </Typography>
                                    </Grid>
                                    {isDispalyText && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <MaterialNumberField
                                                size="small"
                                                label="Mobile Number*"
                                                name="MobileNumber"
                                                type="text"
                                                length={10}
                                                title="Mobile Number"
                                                disabled={otpDisplayed}
                                            />
                                        </Grid>)}
                                    {isDispalyText && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <MaterialTextField
                                                size="small"
                                                label="PAN*"
                                                name="PANNo"
                                                type="text"
                                                length={10}
                                                title="PAN"
                                                disabled={otpDisplayed}
                                            />
                                        </Grid>)}
                                    {isDispalyText && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <MaterialNumberField
                                                size="small"
                                                label="Pincode*"
                                                name="Pincode"
                                                type="text"
                                                length={6}
                                                title="Pincode"
                                                disabled={otpDisplayed}
                                            />
                                        </Grid>)}

                                    {thankYouMessage && (
                                        <div style={{ textAlign: 'center', padding: '20px' }}>
                                            <h2>Thank you!!</h2>
                                            <p> For your interest in becoming a Hitachi Partner.
                                            </p>
                                            <p>Unfortunately, Our services are not yet available in the selected PIN code. We will reach out to you as soon as they become available.</p>
                                        </div>
                                    )}

                                    {otpVarify && (

                                        <Grid item xs={12} sm={12} md={12} lg={12}>
                                            <Typography
                                                variant="h9"
                                                component="div"
                                                gutterBottom
                                            >
                                                Do you have a referral
                                                code/Distributor ID for
                                                registration?
                                            </Typography>

                                            <CustomRadioButton
                                                size="small"
                                                label="Yes"
                                                name="SelectCode"
                                                displayValueID="Yes"
                                                isSelected={
                                                    values.SelectCode === 'Yes'
                                                }
                                                onChange={(value) =>
                                                    setFieldValue(
                                                        'SelectCode',
                                                        value
                                                    )
                                                }
                                            />

                                            <CustomRadioButton
                                                size="small"
                                                label="No"
                                                name="SelectCode"
                                                displayValueID="No"
                                                isSelected={
                                                    values.SelectCode === 'No'
                                                }
                                                onChange={(value) =>
                                                    setFieldValue(
                                                        'SelectCode',
                                                        value
                                                    )
                                                }
                                            />
                                        </Grid>)}

                                    {values.SelectCode === 'Yes' && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <MaterialTextAreaField
                                                label="Referral code/Distributor ID"
                                                name="ReferralCode"
                                                type="TextArea"
                                                icon="shop"
                                                length={10}
                                                title="Referral code/Distributor ID"
                                            />
                                        </Grid>
                                    )}

                                    {otpVarify && (

                                        <Grid item xs={12} sm={12} md={12} lg={12}>
                                            <Typography
                                                variant="h9"
                                                component="div"
                                                gutterBottom
                                            >
                                                Are you assisted by Hitachi
                                                Representative?
                                            </Typography>

                                            <CustomRadioButton
                                                label="Yes"
                                                name="SelectID"
                                                displayValueID="Yes"
                                                isSelected={
                                                    values.SelectID === 'Yes'
                                                }
                                                onChange={(value) =>
                                                    setFieldValue('SelectID', value)
                                                }
                                            />

                                            <CustomRadioButton
                                                label="No"
                                                name="SelectID"
                                                displayValueID="No"
                                                isSelected={
                                                    values.SelectID === 'No'
                                                }
                                                onChange={(value) =>
                                                    setFieldValue('SelectID', value)
                                                }
                                            />
                                        </Grid>
                                    )}

                                    {values.SelectID === 'Yes' && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <MaterialTextAreaField
                                                label="Employee ID"
                                                name="EmployeeID"
                                                type="TextArea"
                                                icon="shop"
                                                length={10}
                                                title="Employee ID"
                                            />
                                        </Grid>
                                    )}

                                    {otpVarify && (

                                        <Grid
                                            container
                                            justify="center"
                                            style={{
                                                marginTop: 10
                                            }}
                                        >
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <Box
                                                    sx={{
                                                        textAlign: 'center',
                                                        m: 1
                                                    }}
                                                >
                                                    <FormControlLabel
                                                        control={
                                                            <Field
                                                                as={Checkbox}
                                                                name="agreeToTerms"
                                                                color="primary"
                                                                style={{
                                                                    marginTop: '-21px'
                                                                }}
                                                            />
                                                        }
                                                        label={
                                                            <span
                                                                style={{
                                                                    float: 'center'
                                                                }}
                                                            >
                                                                <p className="terms">
                                                                    By clicking on
                                                                    Signup, you
                                                                    agree to our{' '}
                                                                    <Link
                                                                        className="loginlink"
                                                                        href="/TnC.htm"
                                                                        style={{
                                                                            color:
                                                                                'blue',
                                                                            textDecoration:
                                                                                'none'
                                                                        }}
                                                                    >
                                                                        terms and
                                                                        conditions
                                                                    </Link>{' '}
                                                                    and{' '}
                                                                    <Link
                                                                        className="loginlink"
                                                                        href="/privacypolicy.htm"
                                                                        style={{
                                                                            color:
                                                                                'blue',
                                                                            textDecoration:
                                                                                'none'
                                                                        }}
                                                                    >
                                                                        privacy
                                                                        policy
                                                                    </Link>
                                                                </p>
                                                            </span>
                                                        }
                                                    />
                                                    <ErrorMessage
                                                        style={{
                                                            color: 'red'
                                                        }}
                                                        name="agreeToTerms"
                                                        component="div"
                                                        className="field-error-red"
                                                    />
                                                </Box>
                                            </Grid>
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

                                    {isDispaly && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <Grid
                                                container
                                                direction="column"
                                                alignItems="center"
                                                justify="center"
                                            >
                                                <div className="flex items-center">
                                                    <Button
                                                        type="submit"
                                                        fullWidth
                                                        title="Registration"
                                                        disabled={loadingOpen}
                                                        style={{
                                                            textTransform:
                                                                'none'
                                                        }}
                                                        variant="contained"
                                                        color="primary"
                                                        className="loginbutton"
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
                                                                'IsSignUp',
                                                                false
                                                            );
                                                            handleSubmit(e);
                                                        }}
                                                    >
                                                        Proceed
                                                    </Button>
                                                </div>

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
                                    {otpVarify && (

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            xl={12}
                                        >
                                            <Grid
                                                container
                                                direction="column"
                                                alignItems="center"
                                                justify="center"
                                            >
                                                <div className="flex items-center">
                                                    <Button
                                                        type="button"
                                                        fullWidth
                                                        title="Registration"
                                                        disabled={loadingOpen}
                                                        style={{
                                                            textTransform: 'none',
                                                        }}
                                                        variant="contained"
                                                        color="primary"
                                                        className="loginbutton"
                                                        onClick={async (e) => {
                                                            setFieldValue(
                                                                'GenerateOTP',
                                                                false
                                                            );
                                                            setFieldValue(
                                                                'IsVerify',
                                                                false
                                                            );
                                                            await setFieldValue('IsSignUp', true);

                                                            const errors = await validateForm();

                                                            if (!Object.keys(errors).length) {
                                                                handleSubmit(e);
                                                            }
                                                        }}
                                                    >
                                                        Signup
                                                    </Button>
                                                </div>

                                                {loadingOpen && (
                                                    <CircularProgress
                                                        size={20}
                                                        className={
                                                            classes.buttonProgress
                                                        }
                                                    />
                                                )}
                                            </Grid>
                                        </Grid>)}

                                    {otpDisplayed && (
                                        <Box
                                            style={{
                                                marginTop: '10px',
                                                paddingLeft: '8px',
                                                paddingRight: '8px',
                                                width: '100%'
                                            }}
                                        >
                                            <Grid
                                                container
                                                spacing={2}
                                                justify="space-between"
                                            >
                                                <Grid item xs={5}>
                                                    <div className="flex items-center">
                                                        <Button
                                                            style={{
                                                                padding: '10px',
                                                                textTransform: 'none'
                                                            }}

                                                            type="submit"
                                                            fullWidth
                                                            variant="contained"
                                                            color="primary"
                                                            disabled={
                                                                loadingOpen
                                                            }
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
                                                                    'IsSignUp',
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
                                                        style={{
                                                            padding: '10px',
                                                            textTransform: 'none'
                                                        }}
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
                                                                'IsSignUp',
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
                                        </Box>
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
                                            Already have an account?
                                            <Link
                                                className="loginlink"
                                                style={{
                                                    textDecoration: 'none',
                                                    cursor: 'pointer'
                                                }}
                                                href="/Login"
                                            >
                                                {' '}
                                                Login
                                            </Link>
                                        </div>

                                        <Grid
                                            className="donwloadbox"
                                            style={{
                                                textAlign: 'center'
                                            }}
                                        >
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

export default Signup;
