import React, { useEffect, useState, useRef } from 'react';
import { Formik, Field, Form, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { Grid, Paper, Container, FormControlLabel, Checkbox, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Button } from '@material-ui/core'
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import HOCLoader from 'Components/Loader/HOCLoader';
import { GetFingerPrint } from 'Components/FingerPrintProcessing';
import { YesBankBioMatricVerification as EKYCBiomateric } from 'Components/Api/EKYCAPI';
import { useHistory } from 'react-router';
import { SelectProfile as SELECT_BY_USERID } from 'Components/Api/UsersApi';
const CustomContainer = HOCLoader(Container);
const Yesbank_eKYC_BiomatricVerifcation = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const aadharNumber = localStorage.getItem('AadharNo');
    const [formObj, setFormObj] = useState({
        AadharNo: aadharNumber,
        FingerPrint: '',
        agreeToTerms: false,
    });
    const [openAgentTC, setOpenAgentTC] = useState(false);
    const [openAadhaarConsent, setOpenAadhaarConsent] = useState(false);

    const handleOpenAgentTC = () => setOpenAgentTC(true);
    const handleCloseAgentTC = () => setOpenAgentTC(false);

    const handleOpenAadhaarConsent = () => setOpenAadhaarConsent(true);
    const handleCloseAadhaarConsent = () => setOpenAadhaarConsent(false);
    const history = useHistory();

    const SubmitHandle = async (values, { resetForm }) => {
        setIsError(false);
        setResponseMessage('');
        setLoading(true);

        const { status, xmldata } = await GetFingerPrint('');
        if (status === false) {
            alert(xmldata);
            return;
        }
        let fingerPrintXML = xmldata;
        var request = {
            FingerPrint: fingerPrintXML,
            AadharNo: values.AadharNo
        };
        var requestObj = JSON.stringify(request);
        var response = await EKYCBiomateric(requestObj);
        setLoading(false);

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
        setIsError(true);
        history.push('/YesbankeKYC');
        return;
    };

    useEffect(() => {
        SelectReport();
    }, []);

    const SelectReport = async () => {
        setLoading(true);
        const response = await SELECT_BY_USERID();
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        let data = JSON.parse(response.data);
        setFormObj(data);
        if (data.IsKYCApproved) {
            history.push('/YesbankeKYC');
        }

    };

    const validationSchema = Yup.object().shape({

        agreeToTerms: Yup.boolean().oneOf([true], 'You must accept the terms and conditions'),
    });
    const reset = () => {
        setFormObj({
            AadharNo: '',
            FingerPrint: '',
            agreeToTerms: false,
        });
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
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={SubmitHandle}
                resetForm={formObj}
            >
                {({ values, setFieldValue }) => {
                    return (
                        <Grid
                            item
                            container
                            style={{ justifyContent: 'center' }}
                        >
                            <Grid item xs={12} sm={12} md={6} lg={4} >
                                <Form>
                                    <h2 className={classes.PaperHead_h2}>Biometric Authentication</h2>
                                    <Grid item xs={12}>
                                        <Paper className={classes.PaperStyle}>
                                            <Grid container spacing={2}>
                                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                                    <MaterialNumberField
                                                        label="Aadhaar Number"
                                                        name="AadharNo"
                                                        length="12"
                                                        placeholder="AAadharNo"
                                                        title="AAadharNo"
                                                    />
                                                </Grid>
                                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                                    <FormControlLabel
                                                        control={
                                                            <Field
                                                                style={{ marginTop: '-21px' }}
                                                                as={Checkbox}
                                                                name="agreeToTerms"
                                                                color="primary"
                                                            />
                                                        }
                                                        label={
                                                            <span>
                                                                By clicking on Proceed, you agree to our
                                                                <span style={{ color: 'blue', cursor: 'pointer' }} onClick={handleOpenAgentTC}> Agent T & C </span>
                                                                and
                                                                <span style={{ color: 'blue', cursor: 'pointer' }} onClick={handleOpenAadhaarConsent}> Customer Aadhaar Consent clause T& C</span>
                                                            </span>
                                                        }
                                                    />
                                                    <ErrorMessage
                                                        style={{ color: 'red' }}
                                                        name="agreeToTerms"
                                                        component="div"
                                                        className="field-error-red"
                                                    />
                                                </Grid>
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                                spacing={2}
                                                container
                                                className={classes.footerGrid}
                                            >
                                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                                    <SecondaryButton
                                                        variant="contained"
                                                        color="secondary"
                                                        type="reset"
                                                        title="Reset"
                                                        onClick={() => {
                                                            reset();
                                                        }}
                                                        className={
                                                            classes.CustomDangerButtonStyle
                                                        }
                                                    >
                                                        Reset
                                                    </SecondaryButton>
                                                </Grid>
                                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                                    <PrimaryButton
                                                        type="submit"
                                                        title="Proceed"
                                                        className={
                                                            classes.CustomPrimaryButtonStyle
                                                        }

                                                    >
                                                        Proceed
                                                    </PrimaryButton>
                                                </Grid>
                                            </Grid>
                                        </Paper>
                                    </Grid>
                                </Form>
                            </Grid>
                            <Dialog open={openAgentTC} onClose={handleCloseAgentTC}>
                                <DialogTitle>Agent Terms & Conditions</DialogTitle>
                                <DialogContent>
                                    <DialogContentText>
                                        <p>1 I hereby confirm that the Customer has been explained (in a language known to the customer) about the nature of information, sharing of such information upon authentication, aspects of the transaction and the terms and conditions applicable to the transaction. The Customer has understood and has authorized the Bank to fetch applicable data from UIDAI.
                                        </p>
                                        <p> 2 I hereby confirm that the Customer has provided all the information voluntarily to the Bank and are true, correct and complete."
                                        </p>
                                    </DialogContentText>
                                </DialogContent>
                                <DialogActions>
                                    <Button onClick={handleCloseAgentTC} color="primary">
                                        Close
                                    </Button>
                                </DialogActions>
                            </Dialog>

                            <Dialog open={openAadhaarConsent} onClose={handleCloseAadhaarConsent}>
                                <DialogTitle>Customer Aadhaar Consent clause Terms & Conditions</DialogTitle>
                                <DialogContent>
                                    <DialogContentText>
                                        <p>
                                            1. I, the Customer hereby authorizes the Bank store my AADHAAR number and fetch, authenticate and store all such necessary details retrieved or to be retrieved from UIDAI through AADHAAR Number and IRIS/biometric authentication for the purpose of this transaction. In case of any discrepancies, the Bank reserves the sole right to block my account/ relation and transaction without any further notice or intimation.
                                        </p>
                                        <p>2. Customer hereby states and undertake that it has no objection, if the information disclosed to the Bank is authenticated vide Aadhaar based system. Customer hereby voluntarily agree and confirm to provide or disclose (as required under the Aadhaar Act 2016 and Regulations framed there under) my identification information (Aadhaar number, biometric information & demographic information) for Aadhaar based authentication system and/or such similar authentication mechanism as provided or stipulated by the Government, from time to time, for the purpose of availing banking service, including operations of account or any other facility relating to banking operations.
                                        </p>
                                        <p> 3. The Customer hereby agree and confirm that the Bank will not oblige to process any request made by me, if:
                                        </p>
                                        <ol>
                                            <li> Aadhaar number provided by me is incorrect.</li>
                                            <li> Details in account does not match with details available with UIDAI.</li>
                                        </ol>
                                        <p>
                                            4. The Customer hereby agree and confirm that the Aadhaar details may be updated for all my banking service, including but not limited to the operation of bank account, for the purposes of authenticate.
                                        </p>
                                        <p>5. The Customer hereby agree and confirm that my account may be used by receiving any government payments across schemes that the Customer is eligible and/or any other payment using the Aadhaar based information. The Customer hereby acknowledge and agree that NPCI may map the Customer's Account in the Aadhaar mapper of NPCI.
                                        </p>
                                        <p>6. The Customer hereby confirms that the Aadhaar detail provided by the Customer is true, correct and complete in all aspect.
                                        </p>
                                        <p>
                                            7. The Customer hereby acknowledges that the Bank will not have any liability or responsibility, if the details provided by the Customer is/or subsequently become false, incorrect or incomplete, either in part or as whole.
                                        </p>
                                        <p>8. Before agreeing to the  above mentioned terms and conditions, the Customer has been explained (in a language known to the Customer) about the nature of information that may be shared upon authentication and have understood the nature and implication of the same, as stipulated under the applicable law. The Customer declares all the information has been voluntarily furnished by them to the Bank, without any duress or coercion."
                                        </p>
                                    </DialogContentText>
                                </DialogContent>
                                <DialogActions>
                                    <Button onClick={handleCloseAadhaarConsent} color="primary">
                                        Close
                                    </Button>
                                </DialogActions>
                            </Dialog>

                        </Grid>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};

export default Yesbank_eKYC_BiomatricVerifcation;
