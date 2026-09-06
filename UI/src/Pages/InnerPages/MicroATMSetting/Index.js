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
} from 'Components/Api/MicroATMSettingAPI';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';

import ReactTable from 'Components/ReactTable/ReactTable';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { API_URL } from 'Config';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const MicroATMSetting = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [callbackURL, setcallbackURL] = useState('');
    const [IsUpdate, setIsUpdate] = useState(false);

    async function fetchcallbackURL() {
        setcallbackURL(await API_URL());
    }
    const [FormObj, setFormObj] = useState({
        MicroATMSettingID: 0,
        Name: '',
        UserName: '',
        Password: '',
        Sequence: 0,
        DisplayName: '',
        MerchantID: '',
        Slat: '',
        Token: '',
        PublicKey: '',
        MinimumAmount: 0,
        MaximumAmount: 0,
        IsActive: false
    });
    const FormValidationSchema = Yup.object({
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        UserName: Yup.string().required(PLEASE_PROVIDE_VALID + ' UserName'),
        Password: Yup.string().required(PLEASE_PROVIDE_VALID + ' Password'),
        MerchantID: Yup.string().required(PLEASE_PROVIDE_VALID + ' MerchantID'),
        Slat: Yup.string().required(PLEASE_PROVIDE_VALID + ' slat'),
        Token: Yup.string().required(PLEASE_PROVIDE_VALID + ' token'),
        MinimumAmount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'Minimum 1  amount')
            .required(PLEASE_PROVIDE_VALID + 'minimum amount'),
        MaximumAmount: Yup.number()
            .moreThan(9, PLEASE_PROVIDE_VALID + 'amount more than 9')
            .required(PLEASE_PROVIDE_VALID + 'maximum amount')
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

    const SelectByID = async (MicroATMSettingID) => {
        setLoadingOpen(false);

        let jsonObj = {
            MicroATMSettingID: MicroATMSettingID
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
            MicroATMSettingID: d.MicroATMSettingID,
            Name: d.Name,
            DisplayName: d.DisplayName,
            Sequence: d.Sequence,
            MerchantID: d.MerchantID,
            Slat: d.Slat,
            Token: d.Token,
            PublicKey: d.PublicKey,
            MinimumAmount: d.MinimumAmount,
            MaximumAmount: d.MaximumAmount,
            UserName: d.UserName,
            Password: d.Password,
            IsActive: d.IsActive
        });
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'MicroATMSettingID',
            srno: true
        });
        columns.push({
            label: 'Edit',
            name: 'MicroATMSettingID',
            edit: true,
            callback: (MicroATMSettingID) => {
                setIsUpdate(true);
                SelectByID(MicroATMSettingID);
            }
        });
        columns.push({
            label: 'Name',
            name: 'Name',
            actived: true
        });
        columns.push({
            label: 'Sequence',
            name: 'Sequence',
            actived: true
        });

        columns.push({
            label: 'Display Name',
            name: 'DisplayName',
            actived: true
        });
        columns.push({
            label: 'UserName',
            name: 'UserName',
            actived: true
        });
        columns.push({
            label: 'Password',
            name: 'Password',
            displayAsPassword: true
        });
        columns.push({
            label: 'MerchantID',
            name: 'MerchantID',
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
            actived: true
        });

        columns.push({
            label: 'Public Key',
            name: 'PublicKey',
            actived: true
        });
        columns.push({
            label: 'Min.Amount',
            name: 'MinimumAmount',
            actived: true
        });
        columns.push({
            label: 'Max.Amount',
            name: 'MaximumAmount',
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
        fetchcallbackURL();
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
            MerchantID: '',
            Sequence: 0,
            DisplayName: '',
            Slat: '',
            Token: '',
            PublicKey: '',
            UserName: '',
            Password: '',
            MinimumAmount: '',
            MaximumAmount: '',
            IsActive: false
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
                                MicroATM Setting
                            </h2>

                            <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
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
                                                    label="Sequence"
                                                    name="Sequence"
                                                    type="text"
                                                    placeholder="Sequence"
                                                    length={1}
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
                                                    label="Display Name"
                                                    name="DisplayName"
                                                    type="text"
                                                    placeholder="Display Name"
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
                                                    label="UserName"
                                                    name="UserName"
                                                    type="text"
                                                    placeholder="UserName"
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
                                                    label="Password"
                                                    name="Password"
                                                    type="text"
                                                    placeholder="Password"
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
                                                    label="Public Key"
                                                    name="PublicKey"
                                                    type="text"
                                                    placeholder="PublicKey"
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
                                                    label="Minimum Amount"
                                                    name="MinimumAmount"
                                                    type="text"
                                                    placeholder="Minimum Amount"
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
                                                <MaterialNumberField
                                                    label="Maximum Amount"
                                                    name="MaximumAmount"
                                                    type="text"
                                                    placeholder="Maximum Amount"
                                                    length={7}
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

            <Grid container>
                <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                    <br></br>
                    <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                        <label>
                            <h3 className={classes.Provider_Format}>
                                <strong> In MosambeeMF919 MicroATM</strong> :
                                <span className={classes.normalTextColor}>
                                    use <b> callback update status as </b>
                                </span>
                                <span className={classes.callback_url}>
                                    "{callbackURL}{' '}
                                    MosambeeCallback/post_Mosambee_Response"
                                </span>
                            </h3>
                        </label>
                    </Grid>
                    <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                        <label>
                            <h3 className={classes.Provider_Format}>
                                <strong>For MosambeeMF919</strong> :
                                <span className={classes.normalTextColor}>
                                    use <b>[MerchantID]</b> = MerchantID ,
                                    <b>[Slat]</b> = Slat ,<b>[UserName]</b> as 0{' '}
                                    , <b>[Password]</b> = as Password
                                </span>
                            </h3>
                        </label>
                    </Grid>
                    <Grid item xs={12} sm={12} md={10} lg={10} xl={10}>
                        <label>
                            <h3 className={classes.Provider_Format}>
                                <strong> In Paysprint MicroATM</strong> :
                                <span className={classes.normalTextColor}>
                                    use <b> callback update status as </b>
                                </span>
                                <span className={classes.callback_url}>
                                    "{callbackURL}{' '}
                                    PaysprintCallback/post_PaysprintMATM_Response"
                                </span>
                            </h3>
                        </label>
                    </Grid>
                    <Grid item xs={12} sm={12} md={10} lg={10} xl={10}>
                        <label>
                            <h3 className={classes.Provider_Format}>
                                <strong>For Paysprint MicroATM</strong> :
                                <span className={classes.normalTextColor}>
                                    use <b>[MerchantID]</b> as PartnerID ,
                                    <b>[PublicKey]</b> as JWTKEY ,{' '}
                                    <b>[Token]</b> as Authorize Key, ,{' '}
                                    <b>[AESIV]</b> and <b>[PIPE]</b>
                                </span>
                            </h3>
                        </label>
                    </Grid>
                    <Grid item xs={12} sm={12} md={10} lg={10} xl={10}>
                        <label>
                            <h3 className={classes.Provider_Format}>
                                <strong> In Fingpay MicroATM</strong> :
                                <span className={classes.normalTextColor}>
                                    use <b> callback update status as </b>
                                </span>
                                <span className={classes.callback_url}>
                                    "{callbackURL}{' '}
                                    FingpayCallback/post_Fingpay_Response"
                                </span>
                            </h3>
                        </label>
                    </Grid>
                    <Grid item xs={12} sm={12} md={10} lg={10} xl={10}>
                        <label>
                            <h3 className={classes.Provider_Format}>
                                <strong>For Fingpay MicroATM</strong> :
                                <span className={classes.normalTextColor}>
                                    use <b>[username]</b> as supermerchentid
                                </span>
                            </h3>
                        </label>
                    </Grid>

                    <Grid item xs={12} sm={12} md={11} lg={11} xl={11}>
                        <label>
                            <h3 className={classes.Provider_Format}>
                                <strong>In EzyPay MicroATM</strong> :{' '}
                                <span className={classes.normalTextColor}>
                                    use <b>[MPIN]</b> = as MerchantID,{' '}
                                    <b>[Auth Code]</b> = as Token
                                </span>
                            </h3>
                        </label>
                    </Grid>
                    <Grid item xs={12} sm={12} md={10} lg={10} xl={10}>
                        <label>
                            <h3 className={classes.Provider_Format}>
                                <strong>For BankIT MicroATM</strong> :
                                <span className={classes.normalTextColor}>
                                    use <b>[MerchantID]</b> as agentID ,
                                    <b>[Token]</b> as Password,{' '}
                                    <b>[Password]</b> as Password
                                </span>
                            </h3>
                        </label>
                    </Grid>
                    {/* <Grid item xs={12} sm={12} md={10} lg={10} xl={10}>
                        <label>
                            <h3 className={classes.Provider_Format}>
                                <strong>For ApiClub MicroATM</strong> :
                                <span className={classes.normalTextColor}>
                                    use <b>[UserName]</b> as UserName ,
                                    <b>[API Key]</b> as Token
                                </span>
                            </h3>
                        </label>
                    </Grid> */}
                    <Grid item xs={12} sm={12} md={10} lg={10} xl={10}>
                        <label>
                            <h3 className={classes.Provider_Format}>
                                <strong>For CredoPay MicroATM</strong> :
                                <span className={classes.normalTextColor}>
                                    use <b>[MerchantID]</b> as default password
                                    ,<b>[Token]</b> as API Token
                                </span>
                            </h3>
                        </label>
                    </Grid>

                    <Grid item xs={12} sm={12} md={10} lg={10} xl={10}>
                        <label>
                            <h3 className={classes.Provider_Format}>
                                <strong> In MorePay MicroATM</strong> :
                                <span className={classes.normalTextColor}>
                                    use <b> Password and Token </b> is used in
                                    MobileApp ,{' '}
                                    <b>
                                        while Merchant ID(SecretKey) and Slat
                                        (ApiToken)
                                    </b>
                                    used in onboarding process password ,{' '}
                                    <b>[Token]</b> as API Token,
                                    <b>
                                        {' '}
                                        " Set below callback at MorePay panel as
                                        "
                                    </b>
                                </span>
                                <span className={classes.callback_url}>
                                    {callbackURL}
                                    MosambeeCallback/post_Morepay_Response"
                                </span>
                            </h3>
                        </label>
                    </Grid>
                </Grid>
            </Grid>
        </CustomContainer>
    );
};
