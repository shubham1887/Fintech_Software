import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Grid,
    Box,
    Paper,
    CircularProgress,
    MenuItem
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectForFilter as SELECT_DATA } from 'Components/Api/VirtualBalanceAPI';
import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Grid);

export const VirtualBalanceReport = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [wallets, setWallets] = useState([]);

    const selectData = async () => {
        let jsonObj = {
            FromDate: new Date(),
            ToDate: new Date(),
            WalletID: 0,
            IsExcelDownload: false
        };
        var request = JSON.stringify(jsonObj);
        const response = await SELECT_DATA(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);

        setItemList(data.list);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'VirtualBalanceID',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        columns.push({
            label: 'Wallet',
            name: 'WalletName'
        });
        var textwrapWidth = 60;

        columns.push({
            label: 'Op Bal',
            name: 'OpeningBalance',
            numeric: true,
            textwrapWidth
        });
        columns.push({
            label: 'Amount',
            name: 'Amount',
            numeric: true,
            textwrapWidth
        });
        columns.push({
            label: 'Cl Bal',
            name: 'ClosingBalance',
            numeric: true,
            textwrapWidth
        });
        columns.push({
            label: 'TransactionID',
            name: 'VirtualBalanceID'
        });
        setMainColumns(columns);
    };

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        selectWallets();
        selectData();
    }, []);

    const [formObject, setFormObject] = useState({
        WalletID: 0,
        FromDate: new Date(),
        ToDate: new Date(),
        IsExcelDownload: false
        // TakeCount: TakeCount,
        // SkipCount: SkipCount,
    });

    const FormValidationSchema = Yup.object({
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'from date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'from date')
            .typeError(PLEASE_SELECT_VALID + 'from date'),

        ToDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'to date')
            .typeError(PLEASE_SELECT_VALID + 'to date')
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

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);
        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await SELECT_DATA(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);

        if (data.IsExcelDownload) {
            //formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'VirtualBal Report');
            return;
        }
        setItemList(data.list);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
        document.removeChild(a);
    }
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
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '20px' }}>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialDatePickerField
                                                name="FromDate"
                                                label="From Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialDatePickerField
                                                name="ToDate"
                                                label="To Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                label="Wallet"
                                                name="WalletID"
                                                title="All Wallet"
                                            >
                                                <MenuItem key="0" value="0">
                                                    All Wallet
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
                                            md={1}
                                            lg={1}
                                        >
                                            <Box
                                                style={{ textAlign: 'center' }}
                                            >
                                                <Icon
                                                    className={
                                                        classes.CustomImageButtonStyle
                                                    }
                                                    disabled={loadingOpen}
                                                    classes={{
                                                        root: classes.iconRoot
                                                    }}
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'IsExcelDownload',
                                                            true
                                                        );
                                                        handleSubmit(e);
                                                    }}
                                                >
                                                    <img
                                                        alt=""
                                                        className={
                                                            classes.imageIcon
                                                        }
                                                        src={excel}
                                                    />
                                                </Icon>
                                            </Box>
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
                                                title="Search"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                Search
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
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />

            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
