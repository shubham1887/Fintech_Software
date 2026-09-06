import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';

import {
    Container,
    MenuItem,
    Grid,
    Paper,
    Button,
    Hidden
} from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import CircularProgress from '@material-ui/core/CircularProgress';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import { SelectDownlinForDropDown as SELECT_DOWNLINE_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectForFundTransfer as WALLET_SELECT } from 'Components/Api/WalletApi';
import { ReverseRequestInsert as REVERSE_REQUEST_INSERT } from 'Components/Api/FundApi';
import Box from '@material-ui/core/Box';
import { useHistory } from 'react-router';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { SelectByWalletIDAndReceiverID as WALLET_INFO } from 'Components/Api/WalletApi';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import HOCLoader from 'Components/Loader/HOCLoader';
import InputLabel from 'Components/Inputes/InputLabel';
const CustomContainer = HOCLoader(Container);

export const ReverseRequest = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [wallets, setWallets] = useState([]);
    const [users, setUsers] = useState([]);
    const [UserInfo, setUserInfo] = useState('');
    const [walletID, setWalletID] = useState(0);
    const [receiverID, setReceiverID] = useState(0);

    const [formObj] = useState({
        SenderID: 0,
        WalletID: 0,
        Amount: '',
        Remark: '',
        InWords: ''
    });

    const validationSchema = Yup.object({
        SenderID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'user')
            .required(PLEASE_SELECT_VALID + 'user'),
        WalletID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'wallet')
            .required(PLEASE_SELECT_VALID + 'wallet'),

        Amount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'amount')
            .required(PLEASE_PROVIDE_VALID + 'amount'),

        Remark: Yup.string().required(PLEASE_PROVIDE_VALID + 'remark')
    });

    const selectWallets = async () => {
        var request = {
            ID: 0
        };
        var json = JSON.stringify(request);
        const response = await WALLET_SELECT(json);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setWallets(data);
    };

    const selectDownline = async () => {
        const response = await SELECT_DOWNLINE_FOR_DROPDOWN();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setUsers(data);
    };

    const selectDownlineBalanceInformation = async (
        ReceiverID,
        WalletID,
        setFieldValue
    ) => {
        if (WalletID <= 0) return;

        if (ReceiverID <= 0) return;

        var request = {
            WalletID: WalletID,
            ReceiverID: ReceiverID
        };

        setLoadingOpen(true);
        var json = JSON.stringify(request);

        const response = await WALLET_INFO(json);

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }

        const { Balance, Outstanding, BalanceDetail } = JSON.parse(
            response.data
        );
        setUserInfo(BalanceDetail);
        setFieldValue('Info', 'Balance:' + Balance + ', OS: ' + Outstanding);
    };

    useEffect(() => {
        selectWallets();
        selectDownline();
    }, []);

    const updateWalletID = async (value, setFieldValue) => {
        setWalletID(value);
        await selectDownlineBalanceInformation(
            receiverID,
            value,
            setFieldValue
        );
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoadingOpen(true);

        var response = await REVERSE_REQUEST_INSERT(JSON.stringify(values));
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setSeverity('error');
            return;
        }
        setSubmitting(false);
        resetForm();
    };

    const OnChange = (value, setFieldValue) => {
        if (value === undefined || value <= 0) {
            return;
        }
        setFieldValue('InWords', NumberToWord(value));
    };
    const updateReceiverID = async (name, value, setFieldValue) => {
        setReceiverID(value);
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
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <Grid container spacing={2}>
                                <Grid item xs={6} sm={6} md={6} lg={6}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Reverse Request
                                    </h2>
                                </Grid>
                                <Grid
                                    item
                                    xs={6}
                                    sm={6}
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
                                                history.push(
                                                    '/FundReverseRequestReport'
                                                );
                                            }}
                                        >
                                            RAISED REQUESTS
                                        </Button>
                                    </h2>
                                </Grid>
                            </Grid>

                            <Paper className={classes.PaperStyle}>
                                <Grid container spacing={3}>
                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <MaterialSelect
                                            label="SenderID"
                                            name="SenderID"
                                            title="Select Downline"
                                            // onChange={(e) => {
                                            //     let value = e.target.value;
                                            //     updateReceiverID(value);
                                            // }}
                                            onCustomCallback={(name, value) => {
                                                updateReceiverID(
                                                    name,
                                                    value,
                                                    setFieldValue
                                                );
                                            }}
                                        >
                                            <MenuItem
                                                key="0"
                                                value="0"
                                                selected={true}
                                            >
                                                Select User
                                            </MenuItem>

                                            {users.map((item) => (
                                                <MenuItem
                                                    key={item.UserID}
                                                    value={item.UserID}
                                                >
                                                    {item.FirmName}
                                                </MenuItem>
                                            ))}
                                            <InputLabel
                                                color="green"
                                                value={UserInfo}
                                            />
                                        </MaterialSelect>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <MaterialSelect
                                            label="Wallet"
                                            name="WalletID"
                                            title="Select Wallet"
                                            onCustomCallback={(name, value) => {
                                                updateWalletID(
                                                    value,
                                                    setFieldValue
                                                );
                                            }}
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
                                        <InputLabel
                                            color="green"
                                            value={UserInfo}
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <MaterialCurrency
                                            name="Amount"
                                            label="Amount"
                                            placeholder="Amount"
                                            autoComplete="new-password"
                                            max={999999}
                                            onCustomCallback={(value) => {
                                                OnChange(value, setFieldValue);
                                            }}
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <MaterialTextField
                                            label="In words"
                                            name="InWords"
                                            type="text"
                                            readOnly={true}
                                            placeholder="InWords"
                                        />

                                        <Hidden xsUp>
                                            <MaterialTextField
                                                label="Parent"
                                                name="ParentID"
                                                type="text"
                                                readOnly={true}
                                                title="Amount"
                                            />
                                        </Hidden>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={6} lg={6}>
                                        <MaterialTextField
                                            label="Remark"
                                            name="Remark"
                                            type="text"
                                            length={50}
                                            title="Remark"
                                        />
                                    </Grid>
                                </Grid>
                            </Paper>
                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Request"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loadingOpen}
                                        >
                                            Request
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
