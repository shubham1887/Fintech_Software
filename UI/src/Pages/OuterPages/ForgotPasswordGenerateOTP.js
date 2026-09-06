import React, { useState, useEffect, useRef } from 'react';

import {
    ForgotPasswordSendOTP as USER_FORGOT_PASSWORD,
    VerifyForgotPasswordOTP as VERFAY_OTP
} from 'Components/Api/UsersApi';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import Link from '@material-ui/core/Link';
import CircularProgress from '@material-ui/core/CircularProgress';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { Button, Grid, Typography, Box, Container } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import Logo from 'Components/Logo/Logo';
import { OTPModal } from 'Components/Modal/OTPModal';
import ReCAPTCHA from 'react-google-recaptcha';
import OuterFooter from 'Components/Footer/OuterFooter';
import { toast } from 'react-toastify';
import {
    RE_CAPTCHA_V2_KEY,
    IS_CAPTCHA,
    CLIENT_NAME,
    PLAY_STORE_URL
} from 'Config';
import LoginSliders from 'Components/LoginSliders';
import { MaterialAlphaNumberField } from 'Components/Inputes/FormikMaterialAlphaNumberInput';
const PageStyle = {
    height: '100%',
    width: '100%'
};
const ForgotPasswordGenerateOTP = () => {
    const txtMobilenoRef = useRef(null);
    const classes = commonStyle();
    const [MobileNumber, setMobileNumber] = useState('');
    const [CaptchaToken, setCaptchaToken] = useState('');
    const recaptchaRef = useRef(null);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [openOTP, setOpenOTP] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isCaptcha, setIsCaptcha] = useState('');
    const [isError, setIsError] = useState(false);
    const [playStoreURL, setPlayStoreURL] = useState('');
    const ModelObj = {
        MobileNumber: '',
        AadharNo: '',
        PANNo: ''
    };
    const [DefaultURL, setDefaultURL] = useState(
        `${process.env.PUBLIC_URL}/Default.htm`
    );
    const [modalErrorMessage, setModalErrorMessage] = useState('');
    const [captchaKey, setCaptchaKey] = useState('');

    const fetchPlayStoreURL = async () => {
        setPlayStoreURL(await PLAY_STORE_URL());
    };

    async function fetchRecaptcha() {
        let IsCaptcha = await IS_CAPTCHA();
        setIsCaptcha(IsCaptcha);
        if (IsCaptcha === 'NO') return;
        setCaptchaKey(await RE_CAPTCHA_V2_KEY());
    }
    async function fetchCName() {
        document.title = await CLIENT_NAME();
    }
    toast.configure({
        autoClose: 20 * 1000,
        draggable: true,
        position: toast.POSITION.BOTTOM_RIGHT
    });
    useEffect(() => {
        fetchCName();
        fetchPlayStoreURL();
        // txtMobilenoRef.current.focus();
        toast.configure();
    }, []);

    useEffect(() => {
        if (captchaKey === '' || captchaKey.length <= 1) {
            fetchRecaptcha();
        }
    }, [captchaKey]);

    const validationSchema = Yup.object({
        MobileNumber: Yup.string()
            .length(10, PLEASE_PROVIDE_VALID + 'mobile number')
            .required(PLEASE_PROVIDE_VALID + 'mobile number'),
        PANNo: Yup.string()
            .matches(/[A-Z]{5}[0-9]{4}[A-Z]{1}/, PLEASE_PROVIDE_VALID + 'PAN')
            .required(PLEASE_PROVIDE_VALID + 'PAN')
    });

    const passForgotOTP = async (otpObj) => {
        setLoadingOpen(true);
        setOpenOTP(false);

        const { OTP } = otpObj;

        let RequestObj = {
            MobileNumber: MobileNumber,
            CaptchaToken: CaptchaToken,
            OTP: OTP,
            Device: 'WEB'
        };

        var response = await VERFAY_OTP(JSON.stringify(RequestObj));
        setLoadingOpen(false);

        setResponseMessage(response.data);
        if (!response.status) {
            setModalErrorMessage(response.data);
            setIsError(true);
            toast.error(response.data, {
                pauseOnHover: true,
                progress: undefined
            });
            setOpenOTP(true);
            return;
        }
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (captchaKey === '' || captchaKey.length <= 1 || isCaptcha === 'NO') {
            values.CaptchaToken = '';
        } else {
            values.CaptchaToken = await recaptchaRef.current.executeAsync();
        }

        values.Device = 'WEB';

        var request = JSON.stringify(values);
        setIsError(false);
        setLoadingOpen(true);
        const response = await USER_FORGOT_PASSWORD(request);
        setLoadingOpen(false);

        setResponseMessage(response.data);
        if (!response.status) {
            setIsError(true);
            toast.error(response.data, {
                pauseOnHover: true,
                progress: undefined
            });
            recaptchaRef.current.reset();
            resetForm(true);
            setSubmitting(false);
            return;
        }
        setMobileNumber(values.MobileNumber);
        setCaptchaToken(values.CaptchaToken);
        setOpenOTP(true);
        resetForm(ModelObj);
        setSubmitting(false);
    };
    const handleCaptchaChange = (value) => {};

    const CustomOtpModal = openOTP ? (
        <OTPModal
            isOpen={openOTP}
            title="OTP"
            error={modalErrorMessage}
            submitCallback={(value) => {
                passForgotOTP(value);
            }}
            resendCallback={(value) => {}}
            onDismissCallback={(value) => {}}
        />
    ) : null;
    const CustomReCaptcha =
        captchaKey === '' || captchaKey.length <= 1 ? null : (
            <ReCAPTCHA
                ref={recaptchaRef}
                size="invisible"
                F
                theme="dark"
                sitekey={captchaKey}
                onChange={handleCaptchaChange}
            />
        );
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

    const ForgoatFormContainer = (
        <Grid item xs={12} sm={12} md={12} lg={4}>
            <Container>
                <Formik
                    initialValues={ModelObj}
                    validationSchema={validationSchema}
                    onSubmit={handleSubmit}
                >
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
                        <Grid container spacing={2} className="loginform">
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <Typography
                                    variant="h6"
                                    className="loginhead"
                                    gutterBottom
                                >
                                    Forgot Password
                                </Typography>
                            </Grid>
                            <Grid item xs={12} sm={12} md={12}>
                                <Typography variant="subtitle1" gutterBottom>
                                    <label htmlFor="MobileNumber">
                                        Mobile Number
                                    </label>
                                </Typography>
                                <MaterialNumberField
                                    size="small"
                                    type="text"
                                    variant="outlined"
                                    length="10"
                                    name="MobileNumber"
                                    inputRef={txtMobilenoRef}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={12}>
                                <Typography variant="subtitle1" gutterBottom>
                                    <label htmlFor="PANNo">PAN</label>
                                </Typography>
                                <MaterialAlphaNumberField
                                    size="small"
                                    type="text"
                                    variant="outlined"
                                    length="15"
                                    name="PANNo"
                                    inputRef={txtMobilenoRef}
                                />
                            </Grid>
                            <Grid
                                container
                                direction="column"
                                alignItems="center"
                                justify="center"
                            >
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <div className="flex items-center">
                                        <Button
                                            type="submit"
                                            title="Forgot Password"
                                            fullWidth
                                            variant="contained"
                                            color="primary"
                                            className="loginbutton"
                                            disabled={loadingOpen}
                                            style={{ textTransform: 'none' }}
                                        >
                                            Submit
                                            {loadingOpen && (
                                                <CircularProgress size={24} />
                                            )}
                                        </Button>
                                        {/* <Button
                                            type="submit"
                                            title="Forgot Password"
                                            fullWidth
                                            variant="contained"
                                            color="primary"
                                            className="loginbutton"
                                            disabled={loadingOpen}
                                        >
                                            Submit
                                            {loadingOpen && (
                                                <CircularProgress size={24} />
                                            )}
                                        </Button> */}
                                    </div>
                                    {isError === true ? (
                                        <label
                                            style={{
                                                color: 'red'
                                            }}
                                        >
                                            {responseMessage}
                                        </label>
                                    ) : null}
                                    {isError === false ? (
                                        <label
                                            style={{
                                                color: 'green'
                                            }}
                                        >
                                            {responseMessage}
                                        </label>
                                    ) : null}
                                </Grid>
                            </Grid>
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
                                    Already have an account?{' '}
                                    <Link
                                        className="loginlink"
                                        style={{
                                            textDecoration: 'none',
                                            cursor: 'pointer'
                                        }}
                                        href="/Login"
                                    >
                                        Login
                                    </Link>
                                </div>

                                <Grid
                                    item
                                    xs={12}
                                    sm={12}
                                    md={12}
                                    lg={12}
                                    style={{ textAlign: 'center' }}
                                >
                                    <Box
                                        sx={{
                                            textAlign: 'center',
                                            m: 1
                                        }}
                                    >
                                        <a href={DefaultURL}>
                                            <Button
                                                title="Go to Home"
                                                variant="contained"
                                                size="large"
                                                className="gotohome"
                                            >
                                                Go to Home
                                                <i className="fad fa-home"></i>
                                            </Button>
                                        </a>
                                    </Box>
                                </Grid>

                                <Grid
                                    className="donwloadbox"
                                    item
                                    xs={12}
                                    sm={12}
                                    md={12}
                                    lg={12}
                                    style={{ textAlign: 'center' }}
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
                </Formik>
                <OuterFooter />
            </Container>
        </Grid>
    );
    return (
        <React.Fragment>
            <Grid container style={PageStyle}>
                {SliderContainer}
                {ForgoatFormContainer}
            </Grid>
            {CustomReCaptcha}
            {CustomOtpModal}
        </React.Fragment>
    );
};

export default ForgotPasswordGenerateOTP;
