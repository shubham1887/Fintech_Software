import {
    Box,
    Button,
    CircularProgress,
    Container,
    Grid,
    Typography,
    Link
} from '@material-ui/core';
import { Select as SELECT_MENU } from 'Components/Api/MenuMasterApi';
import {
    LoginAPI as USER_LOGIN,
    NewDeviceLoginAPI,
    TwoFALoginAPI
} from 'Components/Api/UsersApi';
import { BalanceContext } from 'Components/BalanceContext';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import OuterFooter from 'Components/Footer/OuterFooter';
import { textToSpeech } from 'Components/Helper/Helpers';
import {
    ADMIN_CODE,
    DEVELOPER_CODE,
    DIST_CODE,
    PLEASE_PROVIDE_VALID,
    SUPERDIST_CODE
} from 'Components/Helper/Messages';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import LoginSliders from 'Components/LoginSliders';
import Logo from 'Components/Logo/Logo';
import { CustomModal } from 'Components/Modal/CustomModal';
import { TwoFAModal } from 'Components/Modal/TwoFAModal';
import { OTPModal } from 'Components/Modal/OTPModal';
import usePosition from 'Components/Posistion';
import {
    CLIENT_NAME,
    RE_CAPTCHA_V2_KEY,
    IS_CAPTCHA,
    PLAY_STORE_URL
} from 'Config';
import { Form, Formik } from 'formik';
import getBrowserFingerprint from 'get-browser-fingerprint';
import initFireBase from 'init-fcm';

import { toast } from 'react-toastify';

import { InitialChangePassword } from 'Pages/OuterPages/InitialChangePassword';
import React, { useContext, useEffect, useRef, useState } from 'react';
import ReCAPTCHA from 'react-google-recaptcha';
import { ChangePasswordAfterExpiry } from 'Pages/OuterPages/ChangePasswordAfterExpiry';
import * as Yup from 'yup';


import { SelectPlayStoreURL } from 'Components/Api/SignupAPI';
const PageStyle = {
    height: '100%',
    width: '100%'
};

const Login = () => {
    const {
        DashboardBalanceUpdate,
        AppbarRefresh,
        AppbarRefreshSD
    } = useContext(BalanceContext);
    const classes = commonStyle();

    const captchaRef = useRef(null);
    const txtUserNameRef = useRef(null);
    const [ChangePasswordOpen, setChangePasswordOpen] = useState(false);
    const [IsTwoFAOpen, setIsTwoFAOpen] = useState(false);

    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [DefaultURL, setDefaultURL] = useState(
        `${process.env.PUBLIC_URL}/Default.htm`
    );
    const [openOTP, setOpenOTP] = useState(false);
    const [userName, setUserName] = useState('');
    const [password, setPassword] = useState('');
    const [modalErrorMessage, setModalErrorMessage] = useState('');
    const [isCaptcha, setIsCaptcha] = useState('');
    const [SubscriptionmodalOpen, setSubscriptionmodalOpen] = useState(false);
    const [
        ChangePasswordAfterExpiryOpen,
        setChangePasswordAfterExpiryOpen
    ] = useState(false);
    const [fingerPrint, setFingerPrint] = useState('');

    const { latitude, longitude, error } = usePosition();

    const [ModelObj, setFormObj] = useState({
        UserName: '',
        Password: '',
        FingerPrint: '',
        OTP: '',
        CaptchaToken: '',
        Device: 'Web',
        Latitude: latitude,
        Longitude: longitude
    });

    async function fetchCName() {
        document.title = await CLIENT_NAME();
    }
    const [playStoreURL, setPlayStoreURL] = useState('');

    const fetchPlayStoreURL = async () => {
        var response = await SelectPlayStoreURL();

        if (!response.status) {
            return;
        }

        if (response.data.length > 10) {
            setPlayStoreURL(response.data);
        } else {
            setPlayStoreURL(await PLAY_STORE_URL());
        }
    };

    const [captchaKey, setCaptchaKey] = useState('');
    async function fetchRecaptcha() {
        let IsCaptcha = await IS_CAPTCHA();
        setIsCaptcha(IsCaptcha);
        if (IsCaptcha === 'NO') return;
        setCaptchaKey(await RE_CAPTCHA_V2_KEY());
    }
    toast.configure({
        autoClose: 20 * 1000,
        draggable: true,
        position: toast.POSITION.BOTTOM_RIGHT
    });

    const ClearSession = () => {
        localStorage.clear();
    };

    useEffect(() => {
        fetchPlayStoreURL();
        fetchCName();
        fetchRecaptcha();

        ClearSession();
        txtUserNameRef.current.focus();
        toast.configure();
    }, []);

    useEffect(() => {
        if (captchaKey === '' || captchaKey.length <= 1) {
            fetchRecaptcha();
        }
    }, [captchaKey]);

    const validationSchema = Yup.object({
        UserName: Yup.string()
            .min(2, PLEASE_PROVIDE_VALID + 'password')
            .required(PLEASE_PROVIDE_VALID + 'username'),
        Password: Yup.string()
            .min(4, PLEASE_PROVIDE_VALID + 'password')
            .required(PLEASE_PROVIDE_VALID + 'password')
    });

    const setUISettings = async (UISettingsInString) => {
        if (
            UISettingsInString === undefined ||
            UISettingsInString === null ||
            UISettingsInString === ''
        )
            return;

        let UISettings = JSON.parse(UISettingsInString);
        if (
            UISettings === undefined ||
            UISettings === null ||
            UISettings === ''
        )
            return;

        await UISettings.forEach(({ Key, Value }) => {
            localStorage.setItem(Key, Value);
        });
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (error) {
            let message = 'Please allow this site to access your location';

            toast.error(message, {
                pauseOnHover: true,
                progress: undefined
            });

            return;
        }
        setResponseMessage('');
        setIsError(false);
        toast.dismiss();
        setSubmitting(true);
        values.Latitude = latitude;
        values.Longitude = longitude;
        setLoadingOpen(true);

        if (captchaKey === '' || captchaKey.length <= 1 || isCaptcha === 'NO') {
            values.CaptchaToken = '';
        } else {
            values.CaptchaToken = await captchaRef.current.executeAsync();
        }

        values.FingerPrint = getBrowserFingerprint();

        setFingerPrint(values.FingerPrint);
        setIsError(false);

        let request = JSON.stringify(values);
        setUserName(values.UserName);
        setPassword(values.Password);

        const response = await USER_LOGIN(request);
        setLoadingOpen(false);

        if (
            response.data ===
            'Your subscription is expired, Please renew from http://support.consisty.com/'
        ) {
            setSubscriptionmodalOpen(true);
            return;
        }

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
            const customId = response.data;

            if (toast.isActive(customId)) {
                toast.update(customId);
                return;
            }

            toast.error(response.data, {
                position: 'bottom-right',
                pauseOnHover: false,
                draggable: true,
                progress: undefined,
                toastId: customId
            });

            // toast.error(response.data, {
            //     pauseOnHover: true,
            //     progress: undefined
            // });
            captchaRef.current.reset();
            resetForm(true);
            setSubmitting(false);
            return;
        }

        let ReturnResponse = JSON.parse(response.data);
        localStorage.clear();

        if (ReturnResponse.IsTwoFA) {
            setIsTwoFAOpen(true);
            return;
        }

        if (ReturnResponse.IsInitialPassword) {
            setChangePasswordOpen(true);
            return;
        }

        if (ReturnResponse.IsNewDevice) {
            setOpenOTP(true);
            return;
        }
        if (ReturnResponse.IsPasswordExpired) {
            setChangePasswordAfterExpiryOpen(true);
            return;
        }

        await parseLoginresponse(ReturnResponse);
        resetForm(true);
        setSubmitting(false);
    };

    const parseLoginresponse = async (ReturnResponse) => {
        textToSpeech('Welcome ' + ReturnResponse.FirmName);
        localStorage.setItem('WhatsappURL', ReturnResponse.WhatsappURL);
        localStorage.setItem(
            'IsFundRequestAmountEdit',
            ReturnResponse.IsFundRequestAmountEdit
        );
        localStorage.setItem('Voucher', ReturnResponse.Voucher);
        localStorage.setItem('UserID', ReturnResponse.UserID);
        localStorage.setItem('ParentID', ReturnResponse.ParentID);
        localStorage.setItem('SecureToken', ReturnResponse.SecureToken);
        localStorage.setItem('FirmName', ReturnResponse.FirmName);
        localStorage.setItem('MobileNumber', ReturnResponse.MobileNumber);
        localStorage.setItem('AadharNo', ReturnResponse.AadharNo);
        localStorage.setItem('PANNo', ReturnResponse.PANNo);
        localStorage.setItem('IsServiceCharge', ReturnResponse.IsServiceCharge);
        localStorage.setItem(
            'IsProviderPerformanceGraph',
            ReturnResponse.IsProviderPerformanceGraph
        );
        localStorage.setItem('UserType', ReturnResponse.UserType);
        localStorage.setItem('UserTypeID', ReturnResponse.UserTypeID);
        localStorage.setItem('IsChannel', ReturnResponse.IsChannel);
        localStorage.setItem('NewsMessage', ReturnResponse.NewsMessage);
        localStorage.setItem('HyperLink', ReturnResponse.HyperLink);
        localStorage.setItem('NumberFirst', ReturnResponse.NumberFirst);
        localStorage.setItem(
            'QuickSearchBackDays',
            ReturnResponse.QuickSearchBackDays
        );
        localStorage.setItem(
            'DMTSenderNumberLength',
            ReturnResponse.DMTSenderNumberLength
        );

        localStorage.setItem(
            'ProfileImagePath',
            ReturnResponse.ProfileImagePath
        );
        localStorage.setItem(
            'SubscriptionDays',
            ReturnResponse.SubscriptionDays
        );

        localStorage.setItem('UISettings', ReturnResponse.UISettingsInString);

        await setUISettings(ReturnResponse.UISettingsInString);

        localStorage.setItem('Wallets', JSON.stringify(ReturnResponse.Wallets));
        localStorage.setItem('Authorize', true);
        localStorage.setItem('SystemIP', ReturnResponse.LoginIP);
        localStorage.setItem('RefreshToken', ReturnResponse.RefreshToken);
        localStorage.setItem('NewsCount', ReturnResponse.NewsCount);
        localStorage.setItem('ActiveDMT', ReturnResponse.ActiveDMT);

        if (ReturnResponse.ActiveDMTList.length >= 1) {
            localStorage.setItem(
                'ActiveDMT',
                ReturnResponse.ActiveDMTList[0].Name
            );
            localStorage.setItem(
                'DisplayName1',
                ReturnResponse.ActiveDMTList[0].DisplayName
            );
        }
        if (ReturnResponse.ActiveDMTList.length >= 2) {
            localStorage.setItem(
                'ActiveDMT2',
                ReturnResponse.ActiveDMTList[1].Name
            );

            localStorage.setItem(
                'DisplayName2',
                ReturnResponse.ActiveDMTList[1].DisplayName
            );
        }

        localStorage.setItem('ActiveDMTNeft', ReturnResponse.ActiveDMTNeft);
        localStorage.setItem('ActiveAEPS', ReturnResponse.ActiveAEPS);
        localStorage.setItem('IsPin', ReturnResponse.IsPin);
        localStorage.setItem('IsUPIVerify', ReturnResponse.IsUPIVerify);
        localStorage.setItem(
            'IsUserListOnDashboard',
            ReturnResponse.IsUserListOnDashboard
        );
        localStorage.setItem(
            'IsCashCollection',
            ReturnResponse.IsCashCollection
        );
        localStorage.setItem(
            'IsEkycUserProfileUpdate',
            ReturnResponse.IsEkycUserProfileUpdate
        );

        localStorage.setItem('IsTargetOnSale', ReturnResponse.IsTargetOnSale);
        localStorage.setItem('IsOverDraft', ReturnResponse.IsOverDraft);
        localStorage.setItem(
            'IsCashFreeAiUser',
            ReturnResponse.IsCashFreeAiUser
        );

        localStorage.setItem(
            'IsCreditInstitution',
            ReturnResponse.IsCreditInstitution
        );

        localStorage.setItem(
            'IsFintechLeadGeneration',
            ReturnResponse.IsFintechLeadGeneration
        );

        localStorage.setItem(
            'IsW2OFundTransfer',
            ReturnResponse.IsW2OFundTransfer
        );
        localStorage.setItem(
            'IsW2OWithSenderWallet',
            ReturnResponse.IsW2OWithSenderWallet
        );
        localStorage.setItem(
            'IsOverDraftActive',
            ReturnResponse.IsOverDraftActive
        );

        localStorage.setItem(
            'CommissionOnTransfer',
            ReturnResponse.CommissionOnTransfer
        );
        localStorage.setItem('IsWhiteLabel', ReturnResponse.IsWhiteLabel);
        localStorage.setItem(
            'IsPendingProvider',
            ReturnResponse.IsPendingProvider
        );
        localStorage.setItem(
            'MenuResponse',
            JSON.stringify(ReturnResponse.MenuResponse)
        );

        localStorage.setItem(
            'IsGSTHold',
            JSON.stringify(ReturnResponse.IsGSTHold)
        );

        localStorage.setItem(
            'IsSignupReferral',
            JSON.stringify(ReturnResponse.IsSignupReferral)
        );
        localStorage.setItem('CommissionHold', ReturnResponse.CommissionHold);
        localStorage.setItem(
            'PlanApiForAPIUser',
            ReturnResponse.PlanApiForAPIUser
        );
        localStorage.setItem(
            'AutoBalanceTransfer',
            ReturnResponse.AutoBalanceTransfer
        );
        localStorage.setItem(
            'ServicewiseSubscription',
            ReturnResponse.ServicewiseSubscription
        );
        localStorage.setItem(
            'BackDateFundTransfer',
            ReturnResponse.BackDateFundTransfer
        );
        localStorage.setItem(
            'IsPGSettlementHold',
            ReturnResponse.IsPGSettlementHold
        );
        localStorage.setItem('ReportFontSize', ReturnResponse.ReportFontSize);
        localStorage.setItem('IsMobikwikBBPS', ReturnResponse.IsMobikwikBBPS);
        // SelectMenu(ReturnResponse.UserTypeID);
        // GetNotifications();
        if (ReturnResponse.UserTypeID !== DEVELOPER_CODE) {
            initFireBase();
            DashboardBalanceUpdate();
        }
        localStorage.setItem('HomePage', ReturnResponse.RedirectTo);

        if (
            ReturnResponse.UserTypeID === DIST_CODE ||
            ReturnResponse.UserTypeID === SUPERDIST_CODE
        ) {
            localStorage.setItem(
                'FundReversalUpline',
                ReturnResponse.FundReversalUpline
            );
            AppbarRefreshSD();
        }
        //history.push(ReturnResponse.RedirectTo);
        window.location.href = ReturnResponse.RedirectTo;
        if (ReturnResponse.UserTypeID === ADMIN_CODE) AppbarRefresh();
    };

    const SelectMenu = async (UserTypeID) => {
        var jsonObj = {
            UserTypeID: UserTypeID
        };
        var requestObj = JSON.stringify(jsonObj);
        const menuResponse = await SELECT_MENU(requestObj);

        var MenuJson = JSON.parse(menuResponse.data);
        localStorage.setItem('ServerMenuJson', MenuJson);
    };

    const passLoginOTP = async (otpObj) => {
        setLoadingOpen(true);
        setOpenOTP(false);
        setModalErrorMessage('');

        const { OTP } = otpObj;

        let RequestObj = {
            UserName: userName,
            Password: password,
            IsTrustable: true,
            FingerPrint: fingerPrint,
            OTP: OTP,
            Device: 'WEB',
            SimCardDetails: fingerPrint
        };

        var response = await NewDeviceLoginAPI(JSON.stringify(RequestObj));

        if (!response.status) {
            setLoadingOpen(false);
            setOpenOTP(true);
            setModalErrorMessage(response.data);
            return;
        }
        localStorage.clear();

        var ReturnResponse = JSON.parse(response.data);

        await parseLoginresponse(ReturnResponse);
    };

    const handleCaptchaChange = (value) => {};

    const VerifyTWOFAAndLogin = async (TwoFAObj) => {
        setLoadingOpen(true);
        setOpenOTP(false);
        setModalErrorMessage('');
        const { TwoFAPasscode } = TwoFAObj;

        let RequestObj = {
            UserName: userName,
            Password: password,
            IsTwoFA: true,
            Device: 'WEB',
            SimCardDetails: fingerPrint,
            TwoFAPasscode: TwoFAPasscode
        };
        var response = await TwoFALoginAPI(JSON.stringify(RequestObj));

        if (!response.status) {
            setLoadingOpen(false);
            setIsTwoFAOpen(true);
            setModalErrorMessage(response.data);
            return;
        }
        localStorage.clear();

        var ReturnResponse = JSON.parse(response.data);

        await parseLoginresponse(ReturnResponse);
    };
    const handleDismissCallback = () => {
        setSubscriptionmodalOpen(false);
    };

    const CustomChangePasswordModal = ChangePasswordOpen ? (
        <CustomModal
            isOpen={ChangePasswordOpen}
            title="Initial Password change"
            width="30%"
            onDismissCallback={(value) => {
                setChangePasswordOpen(value);
            }}
        >
            <InitialChangePassword
                userName={userName}
                setChangepasswordOpen={setChangePasswordOpen}
                onDismissCallback={(value) => {
                    setChangePasswordOpen(value);
                }}
            />
        </CustomModal>
    ) : null;

    const CustomChangePasswordAfterExpiryModal = ChangePasswordAfterExpiryOpen ? (
        <CustomModal
            isOpen={ChangePasswordAfterExpiryOpen}
            title="Password Change After Expiry"
            width="30%"
            onDismissCallback={(value) => {
                setChangePasswordAfterExpiryOpen(value);
            }}
        >
            <ChangePasswordAfterExpiry
                userName={userName}
                setChangePasswordAfterExpiryOpen={
                    setChangePasswordAfterExpiryOpen
                }
                onDismissCallback={(value) => {
                    setChangePasswordAfterExpiryOpen(value);
                }}
            />
        </CustomModal>
    ) : null;

    const CustomTwoFAModal = IsTwoFAOpen ? (
        <TwoFAModal
            isOpen={IsTwoFAOpen}
            title="Two-Factor Authentication (2FA)"
            error={modalErrorMessage}
            submitCallback={(value) => {
                VerifyTWOFAAndLogin(value);
            }}
            resendCallback={(value) => {}}
            onDismissCallback={(value) => {
                setIsTwoFAOpen(value);
            }}
        />
    ) : null;

    const CustomOtpModal = openOTP ? (
        <OTPModal
            isOpen={openOTP}
            title="OTP"
            error={modalErrorMessage}
            submitCallback={(value) => {
                passLoginOTP(value);
            }}
            resendCallback={(value) => {}}
            onDismissCallback={(value) => {
                setOpenOTP(value);
            }}
        />
    ) : null;

    const CustomReCaptcha =
        captchaKey === '' || captchaKey.length <= 1 ? null : (
            <ReCAPTCHA
                ref={captchaRef}
                size="invisible"
                theme="dark"
                sitekey={captchaKey}
                onChange={handleCaptchaChange}
            />
        );
    const SliderContainer = (
        <Grid item xs={12} sm={12} md={12} lg={8} xl={8}>
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

    const LoginFormContainer = (
        <Grid item xs={12} sm={12} md={12} lg={4}>
            <Container>
                <Formik
                    initialValues={ModelObj}
                    validationSchema={validationSchema}
                    onSubmit={handleSubmit}
                    resetForm={ModelObj}
                    className="loginform"
                >
                    <Form className={classes.login_form}>
                        <Grid
                            item
                            xs={12}
                            sm={12}
                            md={12}
                            lg={12}
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
                                    Sign In
                                </Typography>
                            </Grid>

                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <label
                                    htmlFor="Mobile Number"
                                    className="inputlabel"
                                >
                                    Mobile Number
                                </label>

                                <MaterialTextField
                                    type="text"
                                    variant="outlined"
                                    size="small"
                                    length="10"
                                    className="inputlogin"
                                    name="UserName"
                                    inputRef={txtUserNameRef}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <label
                                    htmlFor="Password"
                                    className="inputlabel"
                                >
                                    Password
                                </label>

                                <MaterialTextField
                                    autoComplete="false"
                                    variant="outlined"
                                    size="small"
                                    className="inputlogin"
                                    name="Password"
                                    type="password"
                                />
                            </Grid>
                            <Grid
                                item
                                xs={12}
                                sm={12}
                                md={12}
                                lg={12}
                                className="forgotpassword"
                            >
                                <Link
                                    href="/ForgotPassword"
                                    className="loginlink"
                                >
                                    Forgot Password ?
                                </Link>
                            </Grid>
                            <Grid
                                container
                                justify="center"
                                item
                                xs={12}
                                sm={12}
                                md={12}
                                lg={12}
                                style={{ marginTop: 20 }}
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
                                            By clicking on Login, you agree to
                                            our
                                        </p>
                                        <Link
                                            className="loginlink"
                                            href="https://www.hitachi-msp.com/terms-of-use/"
                                            style={{
                                                color: 'blue',
                                                textDecoration: 'none'
                                            }}
                                        >
                                            terms and conditions
                                        </Link>{' '}
                                        and{' '}
                                        <Link
                                            className="loginlink"
                                            href="https://www.hitachi-msp.com/privacy-policy/"
                                            style={{
                                                color: 'blue',
                                                textDecoration: 'none'
                                            }}
                                        >
                                            privacy policy
                                        </Link>
                                    </span>
                                </Box>
                            </Grid>

                            <Grid
                                container
                                direction="column"
                                alignItems="center"
                                justify="center"
                            >
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <Button
                                        type="submit"
                                        title="Login"
                                        fullWidth
                                        variant="contained"
                                        color="primary"
                                        className="loginbutton"
                                        disabled={loadingOpen}
                                        style={{
                                            textTransform: 'none'
                                        }}
                                    >
                                        Login
                                        {loadingOpen && (
                                            <CircularProgress size={24} />
                                        )}
                                    </Button>

                                    {isError === true ? (
                                        <label
                                            style={{
                                                color: 'red'
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
                                    If you are not registered?{' '}
                                    <Link
                                        className="loginlink"
                                        style={{
                                            textDecoration: 'none',
                                            cursor: 'pointer'
                                        }}
                                        href="/IndexSignUp"
                                    >
                                        Signup
                                    </Link>
                                </div>

                                <Grid
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
                                        <a href="https://www.hitachi-msp.com/">
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
                </Formik>

                <OuterFooter />
            </Container>
        </Grid>
    );

    return (
        <React.Fragment>
            <Grid container style={PageStyle}>
                {SliderContainer}
                {LoginFormContainer}
            </Grid>
            {CustomTwoFAModal}
            {CustomReCaptcha}
            {CustomOtpModal}
            {CustomChangePasswordModal}
            {CustomChangePasswordAfterExpiryModal}
        </React.Fragment>
    );
};

export default Login;
