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
    MenuItem,
    Typography
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';

import {
    Select as SELECT,
    Update as UPDATE,
    IsOnOff as ISONOFF,
    SelectByID as SELECT_BY_ID
} from 'Components/Api/EcollectionSettingAPI';
import { SelectRechargeAndDMT as WALLET_SELECT } from 'Components/Api/WalletApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const EcollectionSetting = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [IsUpdate, setIsUpdate] = useState(false);
    const [wallets, setWallets] = useState([]);
    const [FormObj, setFormObj] = useState({
        EcollectionSettingID: 0,
        PrivateKeyPath: '',
        PrivateKeyPassword: '',
        PublicKeyPath: '',
        WalletID: 0
    });

    const FormValidationSchema = Yup.object({
        PrivateKeyPath: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'private key path'
        ),
        PrivateKeyPassword: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' private key password'
        ),
        PublicKeyPath: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' public key path'
        ),
        IFSCCode: Yup.string().required(PLEASE_PROVIDE_VALID + ' IFSCCode'),
        Prefix: Yup.string().required(PLEASE_PROVIDE_VALID + ' Prefix')
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
        }
        var data = JSON.parse(response.data);
        setWallets(data);
    };
    const WalletList = wallets.map((item) => (
        <MenuItem key={item.WalletID} value={item.WalletID}>
            {item.Name}
        </MenuItem>
    ));

    const selectData = async () => {
        const response = await SELECT();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);
        setItemList(data);
    };

    const SelectByID = async (PrimaryID) => {
        setIsError(false);
        setLoadingOpen(true);

        let jsonObj = {
            EcollectionSettingID: PrimaryID
        };
        const response = await SELECT_BY_ID(JSON.stringify(jsonObj));

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        const { data, status } = response;
        if (!status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
        }

        let d = JSON.parse(data);

        setFormObj({
            ...FormObj,
            EcollectionSettingID: d.EcollectionSettingID,
            Name: d.Name,
            PrivateKeyPath: d.PrivateKeyPath,
            PrivateKeyPassword: d.PrivateKeyPassword,
            PublicKeyPath: d.PublicKeyPath,
            WalletID: d.WalletID,
            IFSCCode: d.IFSCCode,
            Prefix: d.Prefix
        });

        setIsUpdate(true);
    };

    const activate = async (item, value) => {
        var jsonObj = {
            EcollectionSettingID: item,
            IsActive: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await ISONOFF(request);
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
            name: 'EcollectionSettingID',
            srno: true
        });

        columns.push({
            label: 'Edit',
            name: 'EcollectionSettingID',
            buttonTitle: 'Edit',
            edit: true,
            callback: (ID) => {
                SelectByID(ID);
            }
        });
        columns.push({
            label: 'Name',
            name: 'Name'
        });

        columns.push({
            label: 'PrivateKeyPath',
            name: 'PrivateKeyPath'
        });
        columns.push({
            label: 'PrivateKeyPassword',
            name: 'PrivateKeyPassword'
        });
        columns.push({
            label: 'PublicKeyPath',
            name: 'PublicKeyPath'
        });
        columns.push({
            label: 'Wallet',
            name: 'WalletName'
        });

        columns.push({
            label: 'Prefix',
            name: 'Prefix'
        });

        columns.push({
            label: 'IFSC',
            name: 'IFSCCode'
        });
        columns.push({
            label: 'OnOff',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: 0,
            callback: (PaymentGatewaySettingID, value) => {
                activate(PaymentGatewaySettingID, value);
            }
        });

        setMainColumns(columns);
    };
    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        selectWallets();
        selectData();
    }, []);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await UPDATE(requestObj);
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
            PrivateKeyPath: '',
            PrivateKeyPassword: '',
            PublicKeyPath: '',
            Name: ''
        });
        setIsUpdate(false);
        selectData();
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
                                Ecollection setting
                            </h2>
                            <Grid item xs={12} sm={12} md={6} lg={6}>
                                <Typography>
                                    <span style={{ color: 'green' }}>
                                        <b>
                                            IFSC code : ICIC0000104 ( if
                                            VirtualCode in 4 digit like ABCD***
                                            )
                                        </b>
                                        <b>
                                            IFSC code : ICIC0000106 ( if
                                            VirtualCode in 6 digit like
                                            ABCDEF***)
                                        </b>
                                        <b>
                                            IFSC code : ICIC0000109 ( if
                                            VirtualCode in 9 digit like
                                            ABCDEFGHI***)
                                        </b>
                                    </span>
                                </Typography>
                            </Grid>
                            <Grid item xs={12}>
                                {IsUpdate ? (
                                    <Paper className={classes.PaperStyle}>
                                        <Grid container spacing={2}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="Name"
                                                    name="Name"
                                                    type="text"
                                                    readOnly={true}
                                                    placeholder="Name"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="Private Key Path"
                                                    name="PrivateKeyPath"
                                                    type="text"
                                                    placeholder="Private Key Path"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="Private Key Password"
                                                    name="PrivateKeyPassword"
                                                    type="text"
                                                    placeholder="Private Key Password"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="Public Key Path"
                                                    name="PublicKeyPath"
                                                    type="text"
                                                    placeholder="Public Key Path"
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
                                                    title="Select Wallet"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Wallet
                                                    </MenuItem>
                                                    {WalletList}
                                                </MaterialSelect>
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="Prefix"
                                                    name="Prefix"
                                                    type="text"
                                                    placeholder="Prefix"
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="IFSCCode"
                                                    name="IFSCCode"
                                                    type="text"
                                                    placeholder="IFSCCode"
                                                />
                                            </Grid>
                                        </Grid>
                                    </Paper>
                                ) : null}
                            </Grid>
                            {IsUpdate ? (
                                <Grid className={classes.footerGrid}>
                                    <Box className={classes.flexReverse}>
                                        <div className={classes.SubmitWrapper}>
                                            <Button
                                                type="submit"
                                                title="Update"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                Update
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
                                    </Box>
                                </Grid>
                            ) : null}
                        </Form>
                    );
                }}
            </Formik>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={100}
            />
        </CustomContainer>
    );
};
