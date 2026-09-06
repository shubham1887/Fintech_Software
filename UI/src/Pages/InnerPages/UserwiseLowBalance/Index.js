import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Box,
    Button,
    Paper,
    CircularProgress,
    MenuItem
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    Select as SELECT,
    Update as UPDATE,
    Insert as INSERT,
    SelectByID as SELECT_BY_ID
} from 'Components/Api/UserwiseLowBalanceAPI';
import { SelectForPaymentGateway as WALLET_SELECT_GATEWAY } from 'Components/Api/WalletApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import {
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { UserLowBalance } from 'Pages/InnerPages/UserwiseLowBalance/LowBalance';
import { useHistory } from 'react-router';
import { SelectLowBalance as SELECT_LOW_BALANCE } from 'Components/Api/UserwiseLowBalanceAPI';
const CustomContainer = HOCLoader(Container);

export const UserwiseLowBalance = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [IsUpdate, setIsUpdate] = useState(false);
    const [wallets, setWallets] = useState([]);
    const [lowBalance, setLowBalance] = useState(false);
    const [LowBalanceMessage, setLowBalanceMessage] = useState('');
    const history = useHistory();

    const [FormObj, setFormObj] = useState({
        WalletID: 0,
        Amount: ''
    });
    const FormValidationSchema = Yup.object({
        WalletID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'wallet')
            .required(PLEASE_SELECT_VALID + 'wallet'),

        Amount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'amount')
            .required(PLEASE_PROVIDE_VALID + 'amount')
    });
    const selectWallets = async () => {
        var request = {
            ID: 0
        };
        var json = JSON.stringify(request);
        const response = await WALLET_SELECT_GATEWAY(json);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setWallets(data);
    };

    const selectData = async () => {
        const response = await SELECT();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(response.data);
        setItemList(data);
    };

    const SelectByID = async (UserwiseLowBalanceID) => {
        setLoadingOpen(false);

        let jsonObj = {
            UserwiseLowBalanceID: UserwiseLowBalanceID
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await SELECT_BY_ID(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
        }

        let d = JSON.parse(response.data);

        setFormObj({
            UserwiseLowBalanceID: d.UserwiseLowBalanceID,
            WalletID: d.WalletID,
            Amount: d.Amount
        });
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserwiseLowBalanceID',
            srno: true
        });
        columns.push({
            label: 'Wallet Name',
            name: 'WalletName'
        });
        columns.push({
            label: 'Amount',
            name: 'Amount'
        });
        columns.push({
            label: 'Edit',
            name: 'UserwiseLowBalanceID',
            edit: true,
            callback: (UserwiseLowBalanceID) => {
                setIsUpdate(true);
                SelectByID(UserwiseLowBalanceID);
            }
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        selectWallets();
        selectData();
        selectLowBalance();
    }, []);

    const selectLowBalance = async () => {
        setLowBalance(false);
        setLowBalanceMessage('');

        const response = await SELECT_LOW_BALANCE();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        let data = JSON.parse(response.data);

        if (data.Status === 'Ok') {
            setLowBalance(false);
            setLowBalanceMessage('');
            return;
        }
        setLowBalance(true);
        setLowBalanceMessage(data.Message);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);

        var response = IsUpdate
            ? await UPDATE(requestObj)
            : await INSERT(requestObj);

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        setSubmitting(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }

        resetForm(FormObj);
        setFormObj({
            WalletID: 0,
            Amount: ''
        });

        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);
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
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Low Balance Setting
                            </h2>

                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
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
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialNumberField
                                                label="Amount"
                                                name="Amount"
                                                type="text"
                                                placeholder="Amount "
                                                length={5}
                                            />
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title={IsUpdate ? 'Update' : 'Add'}
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loadingOpen}
                                        >
                                            {IsUpdate ? 'Update' : 'Add'}
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
            </Formik>
            <ReactTable data={itemList} columns={mainColumns} />

            {lowBalance ? (
                <UserLowBalance
                    isOpen={lowBalance}
                    title="Low Balance"
                    message={LowBalanceMessage}
                    closeButtonClick={() => {
                        setLowBalance(false);
                    }}
                    loadMoneyButtonClick={() => {
                        setLowBalance(false);
                        history.push('/PaymentGatewayTransaction');
                        return;
                    }}
                    fundRequestButtonClick={() => {
                        setLowBalance(false);
                        history.push('/Fund/Request');
                        return;
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
