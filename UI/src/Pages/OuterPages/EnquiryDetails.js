import React, { useState, useEffect, useRef } from 'react';

import { toast } from 'react-toastify';
import { Formik, Form } from 'formik';
import Link from '@material-ui/core/Link';
import * as Yup from 'yup';
import {
    MaterialTextField,
    MaterialTextAreaField
} from 'Components/Inputes/FormikMaterialInput';

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

import {
    RE_CAPTCHA_V2_KEY,
    IS_CAPTCHA,
    CLIENT_NAME,
    PLAY_STORE_URL
} from 'Config';
import LoginSliders from 'Components/LoginSliders';
import usePosition from 'Components/Posistion';

import { Insert as ENQUIRY_ADD } from 'Components/Api/EnquiryAPI';

const PageStyle = {
    height: '100%',
    width: '100%'
};

const Enquiry = () => {
    const classes = commonStyle();

    const recaptchaRef = useRef(null);

    const [isCaptcha, setIsCaptcha] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [DefaultURL, setDefaultURL] = useState(
        `${process.env.PUBLIC_URL}/Default.htm`
    );
    const [playStoreURL, setPlayStoreURL] = useState('');
    const [captchaKey, setCaptchaKey] = useState('');
    const { latitude, longitude, error } = usePosition();
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
        PersonName: '',
        MobileNumber: '',
        EmailID: '',
        Address: '',
        EnquiryType: '',
        Remark: ''
    });
    useEffect(() => {
        fetchRecaptcha();
        fetchPlayStoreURL();
        fetchCName();
        toast.configure();
    }, []);
    useEffect(() => {
        if (captchaKey === '' || captchaKey.length <= 1) {
            fetchRecaptcha();
        }
    }, [captchaKey]);
    const validationSchema = Yup.object({
        EnquiryType: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'enquiry type')
            .required(PLEASE_SELECT_VALID + 'enquiry type'),

        PersonName: Yup.string()
            .trim()
            .required(PLEASE_PROVIDE_VALID + 'Name')
            .max(50, 'Please enter firm name with 50 char. only'),

        MobileNumber: Yup.string()
            .trim()
            .length(10, PLEASE_PROVIDE_VALID + 'mobile number')
            .required(PLEASE_PROVIDE_VALID + 'mobile number'),

        EmailID: Yup.string()
            .trim()
            .email()
            .required(PLEASE_PROVIDE_VALID + 'email address')
            .max(50, 'Please enter firm name with 50 char. only'),

        Address: Yup.string()
            .trim()
            .required(PLEASE_PROVIDE_VALID + 'address')
            .max(50, 'Please enter address with 50 char. only'),

        Remark: Yup.string()
            .trim()
            .required(PLEASE_PROVIDE_VALID + 'remark')
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
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
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

        var request = JSON.stringify(values);

        setLoadingOpen(true);
        const response = await ENQUIRY_ADD(request);
        setLoadingOpen(false);
        setResponseMessage(response.data);
        if (!response.status) {
            setIsError(true);
            toast.error(response.data, {
                pauseOnHover: true,
                progress: undefined
            });
            return;
        }
        setSubmitting(false);
        resetForm(UserObj);
    };

    const handleCaptchaChange = (value) => {};

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
                    onSubmit={handleSubmit}
                >
                    {(formProps) => {
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
                                            EnquiryDetails
                                        </Typography>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialSelect
                                            label="EnquiryType"
                                            name="EnquiryType"
                                            title="EnquiryType"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select EnquiryType
                                            </MenuItem>

                                            <MenuItem
                                                key="Become-Retailer"
                                                value="Become-Retailer"
                                            >
                                                Become-Retailer
                                            </MenuItem>
                                            <MenuItem
                                                key="Become-Distributor"
                                                value="Become-Distributor"
                                            >
                                                Become-Distributor
                                            </MenuItem>
                                            <MenuItem
                                                key="Become-Super Distributor"
                                                value="Become-Super Distributor"
                                            >
                                                Become-Super Distributor
                                            </MenuItem>
                                            <MenuItem
                                                key="Become-API Partner"
                                                value="Become-API Partner"
                                            >
                                                Become-API Partner
                                            </MenuItem>
                                        </MaterialSelect>
                                    </Grid>
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
                                        />
                                    </Grid>

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
                                        />
                                    </Grid>
                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={12}
                                        lg={12}
                                        xl={12}
                                    >
                                        <MaterialTextAreaField
                                            label="Remark"
                                            name="Remark"
                                            length={200}
                                            type="text"
                                            placeholder="Remark"
                                            rows={3}
                                        />
                                    </Grid>
                                    <div
                                        style={{
                                            color: 'green'
                                        }}
                                    >
                                        {responseMessage}
                                    </div>

                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={12}
                                        lg={12}
                                        xl={12}
                                    >
                                        <div className="flex items-center">
                                            <Button
                                                type="submit"
                                                fullWidth
                                                variant="contained"
                                                color="primary"
                                                disabled={loadingOpen}
                                            >
                                                Send
                                            </Button>
                                            {loadingOpen && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </div>
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
                                        <Grid>
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

export default Enquiry;
