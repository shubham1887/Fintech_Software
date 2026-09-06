import React, { useState, useContext } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import {
    AEPSToOtherWalletTransfer
} from 'Components/Api/WalletApi';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import {
    Container,
    Grid,
    Paper,
    CircularProgress
} from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import InputLabel from 'Components/Inputes/InputLabel';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { BalanceContext } from 'Components/BalanceContext';
import {
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { WalletTransferConfirmation } from 'Components/Alert/WalletTransferConfirmation';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { MAIN_WalletID } from 'Components/Helper/Messages';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
const CustomContainer = HOCLoader(Container);

const WalletTransfer = () => {
    const classes = commonStyle();
    const { DashboardBalanceUpdate } = useContext(BalanceContext);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [InWords, setInWords] = useState('');
    const [isViewOpen, setIsViewOpen] = useState(false);
    const [isConfirmAlertRequest, setConfirmAlertRequest] = useState("");
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [isReceipt, setReceipt] = useState({});

    const OnAmountChange = (value, setFieldValue) => {
        if (value === undefined || value < 0) return;
        setInWords(NumberToWord(value));
    };


    const handleSubmit = async (values) => {
        setConfirmAlertRequest(values);
        setIsAlertOpen(true);
        return;
    }
    const HandleSubmit = async (values) => {

        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setIsError(false);
        setLoading(true);

        var response = await AEPSToOtherWalletTransfer(requestObj);
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        var data = JSON.parse(response.data);


        setSeverity('success');
        setResponseMessage(data.Message);
        setIsError(true);
        DashboardBalanceUpdate();
        setReceipt(data);
        setIsViewOpen(true);

        reset();
    };
    const [FormObj, setFormObj] = useState({
        WalletID: MAIN_WalletID,
        Amount: '',
        InWords: '',
        Remark: ''
    });
    const reset = () => {
        setInWords('');
        setFormObj({
            WalletID: MAIN_WalletID,
            Amount: '',
            InWords: '',
            Remark: ''
        });
    };

    const FormValidationSchema = Yup.object({
        Amount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'amount')
            .required(PLEASE_PROVIDE_VALID + 'amount'),
        Remark: Yup.string().required(PLEASE_PROVIDE_VALID + 'remark')
    });

    const WalletTransferForm = ({ setFieldValue }) => {
        return (
            <Grid
                item
                container
                style={{ justifyContent: 'center' }}
            >
                <Grid item xs={12} sm={12} md={6} lg={4} >
                    <Form>
                        <h2 className={classes.PaperHead_h2}>Move funds to e-balance</h2>
                        <Grid item xs={12}>
                            <Paper className={classes.PaperStyle}>
                                <Grid container spacing={2}>

                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialCurrency
                                            name="Amount"
                                            label="Amount (₹)"
                                            placeholder="Enter Amount(₹)"
                                            autoComplete="new-password"
                                            max={999999}
                                            onCustomCallback={(value) => {
                                                OnAmountChange(value, setFieldValue);
                                            }}
                                        />
                                        <InputLabel color="green" value={InWords} />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            label="Remark"
                                            name="Remark"
                                            type="text"
                                            length={150}
                                            title="Remark"
                                        />
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
                                                title="Cancel"
                                                onClick={() => {
                                                    reset();
                                                }}
                                                className={classes.CustomDangerButtonStyle}
                                            ></SecondaryButton>
                                        </Grid>

                                        <Grid item xs={12} sm={12} md={6} lg={6}>
                                            <PrimaryButton
                                                type="submit"
                                                title="Transfer"
                                                className={classes.CustomPrimaryButtonStyle}
                                                disabled={loading}
                                            ></PrimaryButton>
                                            {loading && (
                                                <CircularProgress
                                                    size={24}
                                                    className={classes.buttonProgress}
                                                />
                                            )}
                                        </Grid>
                                        {isViewOpen && (
                                            <WalletTransferConfirmation
                                                isOpen={isViewOpen}

                                                request={isReceipt}
                                                positiveButtonTitle={'Ok'}
                                                positiveButtonClick={(request) => {
                                                    setIsViewOpen(false);


                                                }}
                                                PrintButtonClick={() => {
                                                    setIsViewOpen(false);
                                                }}
                                                negativeButtonClick={() => {
                                                    setIsViewOpen(false);
                                                }}
                                            />
                                        )}  
                                    </Grid>
                                </Grid>
                            </Paper>
                        </Grid>
                    </Form>
                </Grid>
                {isAlertOpen && (
                    <ConfirmAlert
                        isOpen={isAlertOpen}
                        title="Add Money to My E-Balance"
                        message={`You are about to transfer ₹ ${isConfirmAlertRequest.Amount}  to your E-Balance Wallet.`}
                        request={isConfirmAlertRequest}
                        positiveButtonClick={(request) => {
                            setIsAlertOpen(false);
                            HandleSubmit(request);
                        }}
                        negativeButtonClick={() => {
                            setIsAlertOpen(false);
                        }}
                    />
                )}
            </Grid>


        );
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
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={handleSubmit}
                resetForm={FormObj}
            >
                {WalletTransferForm}

            </Formik>
        </CustomContainer>
    );
};

export default WalletTransfer;
