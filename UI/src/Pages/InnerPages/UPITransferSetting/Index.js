import React, { memo, useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { Container, Grid, Paper, CircularProgress } from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import { scrollToTop } from 'Components/utils.js';
import { LOGOUT } from 'ConstComponents';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import {
    Select as SELECT_SETTINGS,
    SelectByID as SELECT_SETTINGS_BY_ID,
    Update as UPDATE_SETTINGS
} from 'Components/Api/UPITransferSettingsAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const Index = memo(() => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [IsUpdate, setIsUpdate] = useState(false);

    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [FormObj, setFormObj] = useState({
        UPITransferSettingID: 0,
        Name: '',
        DisplayName: '',
        UserName: '',
        Token: '',
        IsActive: false,
        CallBackURL: '',
        AuthCode: '',
        MaximumAmount: 0,
        Sequence: 0,
        MinAmount: 0,
        SenderLimit: 0
    });

    const FormValidationSchema = Yup.object({
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        UserName: Yup.string().required(PLEASE_PROVIDE_VALID + ' username'),
        Token: Yup.string().required(PLEASE_PROVIDE_VALID + ' token')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setResponseMessage('');
        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        var response = await UPDATE_SETTINGS(requestObj);

        if (!response.isAuthorized) LOGOUT();
        setLoadingOpen(false);
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        setLoadingOpen(false);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);
        resetForm(FormObj);

        setFormObj({
            UPITransferSettingID: 0,
            Name: '',
            DisplayName: '',
            UserName: '',
            Token: '',
            IsActive: false,
            CallBackURL: '',
            AuthCode: '',
            MaximumAmount: 0,
            Sequence: 0,
            MinAmount: 0,
            SenderLimit: 0
        });

        selectData();
    };

    useEffect(() => {
        createColumns();
        selectData();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UPITransferSettingID',
            srno: true
        });

        columns.push({
            label: 'Edit',
            name: 'UPITransferSettingID',
            edit: true,
            callback: (UPITransferSettingID) => {
                setIsUpdate(true);
                SelectByID(UPITransferSettingID);
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
            displayAsPassword: true
        });

        columns.push({
            label: 'Token',
            name: 'Token',
            displayAsPassword: true
        });

        columns.push({
            label: 'CallBackURL',
            name: 'CallBackURL',
            actived: true
        });

        columns.push({
            label: 'MinAmount',
            name: 'MinAmount',
            actived: true
        });

        columns.push({
            label: 'SenderLimit',
            name: 'SenderLimit',
            actived: true
        });

        columns.push({
            label: 'MaxAmount',
            name: 'MaximumAmount',
            actived: true
        });

        columns.push({
            label: 'Active',
            name: 'IsActiveStr',
            actived: true
        });

        setMainColumns(columns);
    };

    const selectData = async () => {
        setLoadingOpen(false);
        const response = await SELECT_SETTINGS();

        if (!response.isAuthorized) LOGOUT();
        setLoadingOpen(false);

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);

        setItemList(data);
    };

    const SelectByID = async (UPITransferSettingID) => {
        setIsError(false);

        let jsonObj = {
            UPITransferSettingID: UPITransferSettingID
        };
        const response = await SELECT_SETTINGS_BY_ID(JSON.stringify(jsonObj));

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        const { data, status } = response;
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);
        }

        let d = JSON.parse(data);

        setFormObj({
            UPITransferSettingID: d.UPITransferSettingID,
            Name: d.Name,
            DisplayName: d.DisplayName,
            UserName: d.UserName,
            Token: d.Token,
            IsActive: d.IsActive,
            CallBackURL: d.CallBackURL,
            MaximumAmount: d.MaximumAmount,
            Sequence: d.Sequence,
            MinAmount: d.MinAmount,
            AuthCode: d.AuthCode,
            SenderLimit: d.SenderLimit
        });
        scrollToTop();
    };
    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
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
            <Grid container spacing={3}>
                <Grid item xs={6}>
                    <h2 className={classes.PaperHead_h2}>
                        UPITransfer Setting
                    </h2>
                </Grid>
                <Grid item xs={6}></Grid>
            </Grid>
            <Formik
                enableReinitialize={true}
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {(formProps) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
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
                                                    label="UserName / UserID"
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
                                                    label="Token / secret key"
                                                    name="Token"
                                                    type="text"
                                                    placeholder="Token / secret key"
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
                                                    label="CallBackURL / ActionURL"
                                                    name="CallBackURL"
                                                    type="text"
                                                    placeholder="CallBackURL / ActionURL"
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
                                                    label="AuthCode"
                                                    name="AuthCode"
                                                    type="text"
                                                    placeholder="AuthCode"
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
                                                    name="MinAmount"
                                                    type="text"
                                                    placeholder="Minimum Amount"
                                                    length={7}
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialNumberField
                                                    label="Sender Limit"
                                                    name="SenderLimit"
                                                    type="text"
                                                    placeholder="Sender Limit"
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
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
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
                                            {/* <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <FormikMaterialSwitch
                                                    label="IsNeft"
                                                    name="IsNeft"
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
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <FormikMaterialSwitch
                                                    label="IsCCF"
                                                    name="IsCCF"
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
                                            </Grid> */}
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <PrimaryButton
                                                    type="submit"
                                                    title="Update"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    disabled={loadingOpen}
                                                >
                                                    Update
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
                                                md={2}
                                                lg={2}
                                            >
                                                <SecondaryButton
                                                    type="reset"
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
                                ) : null}
                            </Grid>
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
                    <strong>CashFree UPITransfer</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Client id]</b> = UserName ,<b>[ClientSecret]</b>{' '}
                        = Token
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>RazorPay UPITransfer</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[APIKey]</b> = UserName ,<b>[APISecret]</b> =
                        Token , <b>[AdminAccountNumber]</b> = CallBackURL
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>InstantPay UPITransfer</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[ClientID]</b> = UserName ,<b>[ClientSecret]</b>{' '}
                        = Token ,{' '}
                        <b>
                            [AuthCode,EndPointIP,BankId,BankProfileId,PayerAccountNumber]
                        </b>{' '}
                        = CallBackURL ,<b>Note</b> : If your using instant pay
                        wallet then use BankId ,<b>[BankProfileId]</b> = "0" and{' '}
                        <b>[PayerAccountNumber]</b> = "your registered mobile
                        number"
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>ICICI UPITransfer</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[DebitAccountNo]</b> = UserName ,
                        <b>[ClientPrivateKeyPassword]</b> = Token , and{' '}
                        <b>
                            [other param need to store in json in callback param
                            like:]
                        </b>{' '}
                        {`{"AdminVA":"00","AdminMobile":"00","DeviceID":"00","ProfileID":"00","PassCode":"00","BcCode":"00", "AggrName":"AA", "AggrId":"A00", "CorpId":"B00", "CrpUsr":"C00", "ApiKey":"D00", "Urn": "E00", "IciciPublicKey": "PUBLIC KEY PATH", "ClientPrivateKey": "PRIVATE KEY PATH"}`}
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>JaiJuiUPI</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = UserName, <b>[Token]</b> =
                        Token, <b>[AuthCode]</b>= AuthCode , <b>ActionURL</b> :
                    </span>
                    <span className={classes.action_url}>
                        https://api.jaijuirecharge.in/UPI
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>EaseBuzz UPITransfer</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Key]</b> = Token ,<b>[Salt]</b> = AuthCode
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>DikshuPay UPITransfer</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Token]</b> = Token
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>ToshaniBank UPITransfer</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Secretkey]</b> = Token
                    </span>
                </h3>
            </label>
            {/* <label>
                <h3 className={classes.Provider_Format}>
                    <strong>MyNetLink UPITransfer</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = UserName, <b>[APIToken]</b> =
                        Token , <b>ActionURL</b> :
                    </span>
                    <span className={classes.action_url}>
                    https://api.mynetlink.in/CP
                    </span>
                </h3>
            </label>
            */}
            {/* <label>
                <h3 className={classes.Provider_Format}>
                 <strong>ClickPayUPI UPITransfer</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = UserName, <b>[APIToken]</b> =
                        Token , <b>ActionURL</b> :
                    </span>
                    <span className={classes.action_url}>
                  https://api.mynetlink.in/CF
                    </span>
                </h3>
            </label> */}
        </CustomContainer>
    );
});
