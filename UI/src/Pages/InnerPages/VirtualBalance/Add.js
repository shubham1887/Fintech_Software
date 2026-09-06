import React, { useEffect, useState, useContext } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { LOGOUT } from 'ConstComponents';
import {
    Add as VIRTUAL_BALANCE_ADD,
    AddWithOTP as VIRTUAL_BALANCE_ADD_WITH_OTP
} from 'Components/Api/VirtualBalanceAPI';
import { OTPModal } from 'Components/Modal/OTPModal';
import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
import { VirtualBalanceReport } from 'Pages/InnerPages/VirtualBalance/Index';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { BalanceContext } from 'Components/BalanceContext';

import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import {
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const VirtualBalanceAdd = () => {
    const { DashboardBalanceUpdate } = useContext(BalanceContext);

    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [wallets, setWallets] = useState([]);
    const [modalErrorMessage, setModalErrorMessage] = useState('');
    const [openOTP, setOpenOTP] = useState(false);
    const [WalletID, setWalletID] = useState(0);

    const [Amount, setAmount] = useState(0);
    useEffect(() => {
        selectWallets();
    }, []);

    const selectWallets = async () => {
        setLoadingOpen(true);
        setModalErrorMessage('');
        var request = {
            ID: 0
        };
        var json = JSON.stringify(request);
        const response = await WALLET_SELECT(json);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setWallets(data);
    };

    const OnChange = (value, setFieldValue) => {
        if (value === undefined || value < 0) return;

        setFieldValue('InWords', NumberToWord(value));
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;

        setModalErrorMessage('');
        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);
        setIsError(false);
        var response = await VIRTUAL_BALANCE_ADD(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSubmitting(false);

        setSeverity('success');
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);

        if (data.IsOTPSent) {
            setAmount(data.Amount);
            setOpenOTP(true);
            setWalletID(data.WalletID);
        }
        resetForm(formObj);
    };

    const passVirtualBalanceOTP = async (otpObj) => {
        setLoadingOpen(true);
        setOpenOTP(false);
        setModalErrorMessage('');

        const { OTP } = otpObj;

        let RequestObj = {
            WalletID: WalletID,
            Amount: Amount,
            OTP: OTP
        };
        var response = await VIRTUAL_BALANCE_ADD_WITH_OTP(
            JSON.stringify(RequestObj)
        );
        setLoadingOpen(false);
        if (!response.status) {
            setLoadingOpen(false);
            setOpenOTP(true);
            setModalErrorMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);
        setSeverity('success');
        setResponseMessage(data.Message);
        setIsError(true);

        DashboardBalanceUpdate();
        setOpenOTP(false);
        setTimeout(() => {
            window.location.reload();
        }, MESSAGE_CLOSE_AFTER);
    };

    var formObj = {
        WalletID: 0,
        Amount: '',
        InWords: ''
    };

    const formValidationSchema = Yup.object({
        WalletID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'wallet')
            .required(PLEASE_SELECT_VALID + 'wallet'),
        Amount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'amount')
            .lessThan(
                500000001,
                PLEASE_PROVIDE_VALID +
                    'amount, less than 500000000 (50 Cr) only'
            )
            .moreThan(0, PLEASE_PROVIDE_VALID + 'amount, positive only')
    });

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
                initialValues={formObj}
                validationSchema={formValidationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Virtual Balance
                            </h2>

                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Wallet"
                                                name="WalletID"
                                                title="Select Wallet"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Wallet
                                                </MenuItem>
                                                {wallets.map((item) => (
                                                    <MenuItem
                                                        key={item.WalletID}
                                                        value={item.WalletID}
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
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialCurrency
                                                name="Amount"
                                                label="Amount"
                                                placeholder="Amount"
                                                autoComplete="new-password"
                                                max={500000000}
                                                onCustomCallback={(value) => {
                                                    OnChange(
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={5}
                                            lg={5}
                                        >
                                            <MaterialTextField
                                                label="In words"
                                                name="InWords"
                                                type="text"
                                                readOnly={true}
                                                placeholder="InWords"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={1}
                                            lg={1}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Add"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                Add
                                            </PrimaryButton>
                                            {loadingOpen && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={1}
                                            lg={1}
                                        >
                                            <SecondaryButton
                                                type="reset"
                                                variant="contained"
                                                color="secondary"
                                                title="Reset"
                                                className={
                                                    classes.CustomDangerButtonStyle
                                                }
                                            >
                                                Reset
                                            </SecondaryButton>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />
            <VirtualBalanceReport />

            {openOTP ? (
                <OTPModal
                    isOpen={openOTP}
                    title="OTP"
                    error={modalErrorMessage}
                    submitCallback={(value) => {
                        passVirtualBalanceOTP(value);
                    }}
                    resendCallback={(value) => {}}
                    onDismissCallback={(value) => {
                        setOpenOTP(value);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
