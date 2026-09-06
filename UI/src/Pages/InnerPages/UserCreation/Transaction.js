import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

import {
    Insert as TRANSACTION_INSERT,
    Select as SELECT
} from 'Components/Api/UserCreationMasterAPI';
import { SelectForUserCreation as WALLET_SELECT } from 'Components/Api/WalletApi';
import { SelectByUserID as SELECT_USER_COST } from 'Components/Api/UserCreationSettingAPI';

import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';

import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const IDPurchase = () => {
    const classes = commonStyle();
    const formRef = useRef();

    const [mainColumns, setMainColumns] = useState([]);
    const [WalletDropDownOpen, setWalletDropDownOpen] = useState(true);
    const [wallets, setWallets] = useState([]);
    const [walletID, setWalletID] = useState(0);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);

    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [itemList, setItemList] = useState([]);
    const [openFilter, setopenFilter] = useState(true);

    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }

    const [ModelObj, setModelObj] = useState({
        PurchaseCount: 0,
        Amount: 0,
        WalletID: 0
    });
    const [userData, setUserData] = useState({
        Cost: 0,
        MinimumCount: 0
    });
    const CostByUserID = async () => {
        setLoading(true);
        const { isAuthorized, status, data } = await SELECT_USER_COST();
        setLoading(false);

        if (!isAuthorized) LOGOUT();
        if (!status) {
            setSeverity('error');
            setResponseMessage(data);
            setIsError(true);

            return;
        }

        let d = JSON.parse(data);

        setUserData({
            Cost: d.Cost,
            MinimumCount: d.MinimumCount
        });
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);

        setIsError(false);
        setLoading(true);
        var response = await TRANSACTION_INSERT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        reset();
        selectData();
    };
    const validationSchema = Yup.object({
        PurchaseCount: Yup.number()
            .min(
                userData.MinimumCount,
                PLEASE_PROVIDE_VALID + `minimum ${userData.MinimumCount} Count`
            )
            .required(PLEASE_SELECT_VALID + 'Count'),
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
        const response = await WALLET_SELECT(json);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);
        if (data.length <= 1) {
            setWalletDropDownOpen(false);
            setWalletID(data[0].WalletID);

            if (formRef.current) {
                formRef.current.values.WalletID = data[0].WalletID;
            }
        }
        setWallets(data);
    };
    const WalletList = wallets.map((item) => (
        <MenuItem key={item.WalletID} value={item.WalletID}>
            {item.Name}
        </MenuItem>
    ));

    useEffect(() => {
        fetchConsumerNumberFontSize();
        selectWallets();
        CostByUserID();
        selectData();
        createColumns();
    }, []);
    const reset = () => {
        setModelObj({
            PurchaseCount: 0,
            Amount: 0,
            WalletID: 0
        });
    };

    const selectData = async () => {
        setItemList([]);
        const { isAuthorized, status, data } = await SELECT();

        setLoading(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var d = JSON.parse(data);

        setItemList(d);
    };

    const OnOnIDChangeCore = (value, setFieldValue) => {
        if (value === undefined || value === null || value <= 0) {
            return;
        }
        let total = userData.Cost * value;

        setFieldValue('Amount', total);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserCreationMasterID',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        columns.push({
            label: 'User',
            name: 'FirmName'
        });
        columns.push({
            label: 'Op.Id',
            name: 'OpeningIdCount'
        });
        columns.push({
            label: 'Count',
            name: 'PurchaseCount'
        });

        columns.push({
            label: 'Cl.IdC',
            name: 'ClosingIdCount'
        });

        columns.push({
            label: 'Op.Bal',
            name: 'OpeningBalance'
        });
        columns.push({
            label: 'Amount',
            name: 'Amount'
        });
        columns.push({
            label: 'Cl.Bal',
            name: 'ClosingBalance'
        });

        setMainColumns(columns);
    };

    const PurchaseForm = ({ values, errors, setFieldValue }) => {
        return (
            <Grid
                item
                container
                margin="normal"
                xs={12}
                md={12}
                sm={12}
                lg={12}
                spacing={2}
            >
                <Grid item xs={12} sm={12} md={6} lg={4}>
                    <Form style={{ marginBottom: '15px' }}>
                        <Grid item xs={12}>
                            <Paper>
                                <CustomAccordion
                                    title={'ID Purchase'}
                                    open={openFilter}
                                    setOpen={(value) => setopenFilter(value)}
                                >
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            {WalletDropDownOpen ? (
                                                <MaterialSelect
                                                    label="Wallet"
                                                    name="WalletID"
                                                    title="Select Wallet"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Wallet
                                                    </MenuItem>
                                                    {WalletList}
                                                </MaterialSelect>
                                            ) : null}
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <MaterialCurrency
                                                font_size={
                                                    consumerNumberFontSize
                                                }
                                                label="ID Count"
                                                name="PurchaseCount"
                                                type="text"
                                                placeholder="ID Count"
                                                onCustomCallback={(value) => {
                                                    OnOnIDChangeCore(
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
                                            md={12}
                                            lg={12}
                                        >
                                            <MaterialNumberField
                                                font_size={
                                                    consumerNumberFontSize
                                                }
                                                label="Amount"
                                                name="Amount"
                                                type="text"
                                                placeholder="Amount"
                                                disabled
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
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={6}
                                                lg={6}
                                            >
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

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={6}
                                                lg={6}
                                            >
                                                <PrimaryButton
                                                    type="submit"
                                                    title="Transfer"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    disabled={loading}
                                                >
                                                    Purchase
                                                </PrimaryButton>
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                </CustomAccordion>
                            </Paper>
                        </Grid>
                    </Form>
                </Grid>
                <Grid
                    item
                    xs={12}
                    sm={12}
                    md={6}
                    lg={8}
                    className="lastrecharges"
                >
                    <h2 className={classes.PaperHead_h2}>Last 10 Purchase</h2>
                    <ReactTable
                        data={itemList}
                        columns={mainColumns}
                        getCallback={({ currentPage, RowsPerPage }) => {
                            setTakeCount(RowsPerPage);
                            setSkipCount(RowsPerPage * currentPage);
                        }}
                        count={totalCount}
                        rowsPerPage={10}
                    />
                </Grid>
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
                innerRef={formRef}
                key={'formTransfer'}
                enableReinitialize={true}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={ModelObj}
            >
                {PurchaseForm}
            </Formik>
        </CustomContainer>
    );
};
