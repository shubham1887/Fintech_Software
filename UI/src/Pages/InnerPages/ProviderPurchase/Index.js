import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Grid,
    Paper,
    CircularProgress,
    MenuItem,
    Box
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { Select as SELECT_PURCHASE } from 'Components/Api/ProviderPurchaseAPI';
import { Update as UPDATE } from 'Components/Api/ProviderPurchaseAPI';
import { Delete as DELETE } from 'Components/Api/ProviderPurchaseAPI';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Grid);

export const PurchaseReport = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const [providerList, setProviderList] = useState([]);
    const [wallets, setWallets] = useState([]);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const selectData = async () => {
        let jsonObj = {
            FromDate: new Date(),
            ToDate: new Date(),
            ProviderID: 0,
            WalletID: 0,
            IsExcelDownload: false
        };
        var request = JSON.stringify(jsonObj);
        const response = await SELECT_PURCHASE(request);
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
    const UpdateClick = async (row) => {
        let jsonObj = {
            ProviderPurchaseID: row[0],
            Amount: row[4]
        };

        var request = JSON.stringify(jsonObj);
        setItemList([]);
        setIsError(false);
        setLoadingOpen(true);
        const response = await UPDATE(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };
    const onDeleteClick = (item) => {
        var RequestObj = { ProviderPurchaseID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setItemList([]);
        setLoadingOpen(true);
        const response = await DELETE(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        selectData();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ProviderPurchaseID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });
        columns.push({
            label: 'Wallet',
            name: 'WalletName'
        });
        columns.push({
            label: 'Amount',
            name: 'Amount',
            editable: true
        });
        columns.push({
            label: 'Remark',
            name: 'Remark'
        });
        columns.push({
            label: 'Update',
            name: 'Update',
            update: true,
            buttonTitle: 'Update',
            callback: (row) => {
                UpdateClick(row);
            }
        });

        columns.push({
            label: 'Delete',
            name: 'ProviderPurchaseID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        selectProvider();
        selectWallets();
        selectData();
    }, []);

    const [formObject, setFormObject] = useState({
        ProviderID: 0,
        WalletID: 0,
        FromDate: new Date(),
        ToDate: new Date()
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

    const selectProvider = async () => {
        setLoadingOpen(true);
        const response = await PROVIDER_DROP_DOWN();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
    };

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
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);
        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await SELECT_PURCHASE(requestObj);
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
            downloadFile(data.ReportPath, 'ProviderPurchase Report');
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
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
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
                                            md={2}
                                            lg={2}
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
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Select Provider"
                                                name="ProviderID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select All
                                                </MenuItem>
                                                {providerList.map(
                                                    (
                                                        {
                                                            ProviderID,
                                                            ProviderName
                                                        },
                                                        index
                                                    ) => (
                                                        <MenuItem
                                                            key={index}
                                                            value={ProviderID}
                                                        >
                                                            {ProviderName}
                                                        </MenuItem>
                                                    )
                                                )}
                                            </MaterialSelect>
                                        </Grid>
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
                                                    Select All
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
                                            md={2}
                                            lg={2}
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
            </Formik>
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this record?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Delete(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}

            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
