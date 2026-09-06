import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Box,
    Button,
    Paper,
    CircularProgress
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    Select as SELECT,
    Update as UPDATE,
    IsOnOff as ISONOFF
} from 'Components/Api/eKYCSettingAPI';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import ReactTable from 'Components/ReactTable/ReactTable';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const EKYCSetting = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [IsUpdate, setIsUpdate] = useState(false);

    const [FormObj, setFormObj] = useState({
        eKYCSettingID: 0,
        MerchantID: '',
        EncryptionKey: '',
        Name: '',
        Mode: '',
        Slat: '',
        Token: '',
        AuthKey: '',
        ChargeAmount: 0,
        IsActive: false
    });
    const FormValidationSchema = Yup.object({
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        AuthKey: Yup.string().required(PLEASE_PROVIDE_VALID + ' auth key'),
        Slat: Yup.string().required(PLEASE_PROVIDE_VALID + ' slat'),
        Token: Yup.string().required(PLEASE_PROVIDE_VALID + ' token'),
        EncryptionKey: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' encryption key'
        ),
        Mode: Yup.string().required(PLEASE_PROVIDE_VALID + 'Mode')
    });
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

    const SelectByID = async (eKYCSettingID) => {
        setLoadingOpen(false);

        let jsonObj = {
            eKYCSettingID: eKYCSettingID
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await ISONOFF(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setIsError(true);
        if (!response.status) setSeverity('error');

        let d = JSON.parse(response.data);
        setFormObj({
            eKYCSettingID: d.eKYCSettingID,
            Name: d.Name,
            Mode: d.Mode,
            AuthKey: d.AuthKey,
            Slat: d.Slat,
            Token: d.Token,
            ChargeAmount: d.ChargeAmount,
            EncryptionKey: d.EncryptionKey,
            MerchantID: d.MerchantID,
            IsActive: d.IsActive
        });
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'eKYCSettingID',
            srno: true
        });
        columns.push({
            label: 'Edit',
            name: 'eKYCSettingID',
            edit: true,
            callback: (eKYCSettingID) => {
                setIsUpdate(true);
                SelectByID(eKYCSettingID);
            }
        });

        columns.push({
            label: 'Name',
            name: 'Name',
            actived: true
        });
        columns.push({
            label: 'MerchantID',
            name: 'MerchantID',
            displayAsPassword: true
        });
        columns.push({
            label: 'AuthKey',
            name: 'AuthKey',
            displayAsPassword: true
        });
        columns.push({
            label: 'Slat',
            name: 'Slat',
            displayAsPassword: true
        });
        columns.push({
            label: 'Token',
            name: 'Token',
            displayAsPassword: true
        });
        columns.push({
            label: 'EncryptionKey',
            name: 'EncryptionKey',
            displayAsPassword: true
        });

        columns.push({
            label: 'Charge',
            name: 'ChargeAmount',
            actived: true
        });

        columns.push({
            label: 'Mode',
            name: 'Mode',
            actived: true
        });

        columns.push({
            label: 'IsActive',
            name: 'IsActiveStr',
            actived: true
        });

        setMainColumns(columns);
    };
    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
    };
    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
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
            Name: '',
            Slat: '',
            Mode: '',
            Token: '',
            AuthKey: '',
            ActionURL: '',
            StatusCheckURL: '',
            ChargeAmount: '',
            EncryptionKey: '',
            MerchantID: ''
        });

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
                                eKYC Setting
                            </h2>

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
                                                    placeholder="Name"
                                                    readOnly={true}
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
                                                    label="MerchantID"
                                                    name="MerchantID"
                                                    type="text"
                                                    placeholder="MerchantID"
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
                                                    label="AuthKey"
                                                    name="AuthKey"
                                                    type="text"
                                                    placeholder="AuthKey"
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
                                                    label="Slat"
                                                    name="Slat"
                                                    type="text"
                                                    placeholder="Slat"
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
                                                    label="Token"
                                                    name="Token"
                                                    type="text"
                                                    placeholder="Token"
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
                                                    label="EncryptionKey"
                                                    name="EncryptionKey"
                                                    type="text"
                                                    placeholder="EncryptionKey"
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialNumberField
                                                    label="Charge Amount"
                                                    name="ChargeAmount"
                                                    type="text"
                                                    placeholder="Charge Amount"
                                                    length={7}
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
                                                    label="Mode"
                                                    name="Mode"
                                                    type="text"
                                                    placeholder="Mode"
                                                    length={5}
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={3}
                                                sm={3}
                                                md={3}
                                                lg={3}
                                            >
                                                <FormikMaterialSwitch
                                                    label="IsActive"
                                                    name="IsActive"
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        OnChange(
                                                            name,
                                                            value,
                                                            formProps.setFieldValue
                                                        );
                                                    }}
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
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>For InstantPay : </strong>
                    <span className={classes.normalTextColor}>
                        put your <b>AuthKey</b> in AuthKey,{' '}
                        <b>[EncryptionKey]</b>in EncryptionKey,{' '}
                        <b>[ClientID]</b>in Slat , <b>[ClientSecret]</b> in
                        Token
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>For Paysprint : </strong>
                    <span className={classes.normalTextColor}>
                        put your <b>[MerchantID]</b> as PartnerID,{' '}
                        <b>[Token]</b>
                        in as JWTKEY, <b>[AuthKey]</b>as Authorize Key ,{' '}
                        <b>[AESKey]</b> in slat
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>For PDRS : </strong>
                    <span className={classes.normalTextColor}>
                        put your <b>MerchantID</b> as Username, <b>[AuthKey]</b>
                        as Password, <b>[Token]</b>as Token
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>For Quick : </strong>
                    <span className={classes.normalTextColor}>
                        put your <b>[Token]</b>as Token
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>For CashFree : </strong>
                    <span className={classes.normalTextColor}>
                        put your <b>MerchantID</b> as Client ID,{' '}
                        <b>[AuthKey]</b>
                        as Client Secret
                    </span>
                </h3>
            </label>
            <br />
            <br /> <br />
        </CustomContainer>
    );
};
