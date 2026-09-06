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
} from 'Components/Api/PayoutSettingAPI';
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

export const PayoutSetting = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [IsUpdate, setIsUpdate] = useState(false);
    const [callbackURL, setcallbackURL] = useState('');
    async function fetchcallbackURL() {
        setcallbackURL(await API_URL());
    }
    const [FormObj, setFormObj] = useState({
        PayoutSettingID: 0,
        Name: '',
        Mode: '',
        MerchantID: '',
        Slat: '',
        Token: '',
        ActionURL: '',
        StatusCheckURL: '',
        MinimumAmount: 0,
        MaximumAmount: 0,
        IsActive: false
    });
    const FormValidationSchema = Yup.object({
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        MerchantID: Yup.string().required(PLEASE_PROVIDE_VALID + ' MerchantID'),
        Slat: Yup.string().required(PLEASE_PROVIDE_VALID + ' slat'),
        Token: Yup.string().required(PLEASE_PROVIDE_VALID + ' token'),
        ActionURL: Yup.string().required(PLEASE_PROVIDE_VALID + ' actionURL'),
        StatusCheckURL: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' statusCheckURL'
        ),
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

    const SelectByID = async (PayoutSettingID) => {
        setLoadingOpen(false);

        let jsonObj = {
            PayoutSettingID: PayoutSettingID
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
            PayoutSettingID: d.PayoutSettingID,
            Name: d.Name,
            Mode: d.Mode,
            MerchantID: d.MerchantID,
            Slat: d.Slat,
            Token: d.Token,
            ActionURL: d.ActionURL,
            StatusCheckURL: d.StatusCheckURL,
            MinimumAmount: d.MinimumAmount,
            MaximumAmount: d.MaximumAmount,
            IsActive: d.IsActive
        });
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'PayoutSettingID',
            srno: true
        });
        columns.push({
            label: 'Edit',
            name: 'PayoutSettingID',
            edit: true,
            callback: (PayoutSettingID) => {
                setIsUpdate(true);
                SelectByID(PayoutSettingID);
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
            label: 'ActionURL',
            name: 'ActionURL',
            actived: true
        });
        columns.push({
            label: 'StatusCheck URL',
            name: 'StatusCheckURL',
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
        fetchcallbackURL();
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
            Mode: '',
            MerchantID: '',
            Slat: '',
            Token: '',
            ActionURL: '',
            StatusCheckURL: '',
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
                                Payout Setting
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
                                                md={6}
                                                lg={6}
                                            >
                                                <MaterialTextField
                                                    label="ActionURL"
                                                    name="ActionURL"
                                                    type="text"
                                                    placeholder="ActionURL"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={6}
                                                lg={6}
                                            >
                                                <MaterialTextField
                                                    label="StatusCheck URL"
                                                    name="StatusCheckURL"
                                                    type="text"
                                                    placeholder="Status Check URL"
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
            {/* <label>
            <h3 className={classes.Provider_Format}>
                    <strong>For Alle :</strong> 
                    <span className={classes.normalTextColor}>
                        put your <b>[UserName]</b> in MerchantID, <b>[Token]</b> =
                        Token
                    </span>
                </h3>
            </label> */}
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>For RazorPay :</strong>
                    <span className={classes.normalTextColor}>
                        put your <b>[APIKey]</b> in MerchantID,{' '}
                        <b>[APISecret]</b> in Token, <b>[AdminAccountNumber]</b>{' '}
                        in Slat
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>For Paytm :</strong>
                    <span className={classes.normalTextColor}>
                        put your <b>[mid]</b> in MerchantID,{' '}
                        <b>[merchantKey]</b> in Token,{' '}
                        <b>[subwalletGuid or salesWalletGuid]</b> in Slat ,
                    </span>
                    <span className={classes.callback_url}>callback URL</span>
                    <span className={classes.normalTextColor}>
                        <b> in </b>
                    </span>
                    <span className={classes.action_url}>ActionURL</span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>ICICI Composite :</strong>
                    <span className={classes.normalTextColor}>
                        <b>[DebitAccountNo]</b> = MerchantID,{' '}
                        <b>[ClientPrivateKeyPassword]</b> = Token,{' '}
                        <b>[PassCode]</b>= Slat and{' '}
                        <b>
                            other param need to store in json in ActionURL param
                            like:
                        </b>{' '}
                        {`{"BcCode":"00", "AggrName":"AA", "AggrId":"A00", "CorpId":"B00", "CrpUsr":"C00", "ApiKey":"D00", "Urn": "E00", "IciciPublicKey": "PUBLIC KEY PATH", "ClientPrivateKey": "PRIVATE KEY PATH"}`}
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>For InstantPay :</strong>
                    <span className={classes.normalTextColor}>
                        put your <b>[AuthCode]</b> in Slat, <b>[ClientID]</b> in
                        MerchantID, <b>[ClientSecret]</b>
                        in Token and{' '}
                        <b>BankId,BankProfileId,PayerAccountNumber in </b>
                    </span>
                    <span className={classes.action_url}>
                        ActionURL (ex. abc,xyz,mno)
                    </span>
                </h3>
            </label>
            <br />
            {/* <label>
              <h3 className={classes.Provider_Format}>
                    <strong>For ApiClub :</strong> 
                    <span className={classes.normalTextColor}>
                        put your <b>[API-Key]</b> in Token,{' '}
                        <b>[WebsiteURL]</b> in MerchantID
                    </span>
                </h3>
            </label> */}
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>PayU Payout :</strong>
                    <span className={classes.normalTextColor}>
                        put <b>[Client ID]</b> = Slat, <b>[Payout ID]</b>
                        MerchantID,<b>[Client Secret]</b> =Token and{' '}
                        <b>callback URL =</b>
                    </span>
                    <span className={classes.callback_url}>
                        {callbackURL} PayUCallback/postResponse
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>OpenBank Payout :</strong>
                    <span className={classes.normalTextColor}>
                        use <b>[AccessKey]</b> = Slat,{' '}
                        <b>[DebitAccountNumber]</b> = MerchantID{' '}
                        <b>[SecretKey]</b> =Token
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>Jaijui Payout :</strong>
                    <span className={classes.normalTextColor}>
                        use <b>[MobileNumber]</b> = MerchantID, <b>[Token]</b> =
                        Token <b>[Slat]</b>{' '}
                        :https://api.jaijuirecharge.in/JaijuiDMTAPI
                    </span>
                    <strong> , CashFree Payout :</strong>
                    <span className={classes.normalTextColor}>
                        use <b>[Client id]</b> = MerchantID,{' '}
                        <b>[ClientSecret]</b> = Token{' '}
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong> PrimePay Payout: </strong>
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = MerchantID,{' '}
                        <b>[ClientSPasswordecret]</b> = Token <b>[AuthCode]</b>{' '}
                        = Slat
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong> ZPlus Payout: </strong>
                    <span className={classes.normalTextColor}>
                        use <b>[key]</b> = MerchantID, <b>[Client id]</b> =
                        Token <b>[User id]</b> = Slat
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong> SafexPay Payout: </strong>
                    <span className={classes.normalTextColor}>
                        use <b>[EncryptionKey]</b> = Token, <b>[MID]</b> =
                        MerchantID <b>[AGID]</b> = StatusCheckURL ,{' '}
                        <b>[SessionID]</b>= Slat, <b>[UID] = </b>
                    </span>
                    <span className={classes.action_url}>ActionURL</span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong> In Paysprint Payout: </strong>
                    <span className={classes.normalTextColor}>
                        use <b>[MerchantID]</b> as ParentID, <b>[ActionURL]</b>{' '}
                        as JWTKEY, <b>[Token]</b> as Authorize Key,
                        <b>[Slat contains(comma separated)]</b> as AESKey and
                        AESIV
                        <b>[SessionID]</b>= Slat, <b>[UID] = </b>
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>PaisaRewardsPayout : </strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[PaisaRewardsId]</b> =Token, <b>[Salt]</b> =
                        Salt, <b>[MID]</b> = MerchantID
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>DikshuPayPayout :</strong>
                    <span className={classes.normalTextColor}>
                        use <b>[Token]</b> =Token
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>EaseBuzz :</strong>
                    <span className={classes.normalTextColor}>
                        use <b>[Token]</b> =Key , <b>[Slat]</b> =Salt
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>FidyPay :</strong>
                    <span className={classes.normalTextColor}>
                        use <b>[Username]</b>=MerchantID , <b>[Password]</b>{' '}
                        =Token,<b>[ClientId]=</b>
                    </span>
                    <span className={classes.action_url}>ActionURL</span>
                    <span className={classes.normalTextColor}>
                        <b>[ClientSecret]</b> =StatusCheckURL,{' '}
                        <b>[bankaccountkey]</b>=Salt
                    </span>
                </h3>
            </label>
            {/* <label>
                <h3 className={classes.Provider_Format}>
                <strong>In Hypto Payout: </strong>
                <span>
                <b>Token</b> as API Token             
            </label> */}
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>IServeU</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[ClientID]</b> = MerchantID ,{' '}
                        <b>[ClientSecret]</b> = Token ,
                    </span>
                    <span className={classes.normalTextColor}>
                        <b>ActionURL</b> ={' '}
                    </span>
                    <span className={classes.action_url}>
                        https://apigatewayprod.iserveu.tech,
                    </span>
                    <span className={classes.normalTextColor}>
                        <b>callback URL as</b> ={' '}
                    </span>
                    <span className={classes.callback_url}>
                        "{callbackURL}IServeUCallback/postResponse"
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>YesBank :</strong>
                    <span className={classes.normalTextColor}>
                        use <b>[MerchantID]</b>=MerchantID ,{' '}
                        <b>[MerchantKey]</b> =Slat,
                        <b>
                            [XIBMClientSecret,XIBMClientID,PrivateKeyFilePath,AuthUserName,AuthPassword]=
                        </b>
                    </span>
                    <span className={classes.action_url}>ActionURL ,</span>
                    <span className={classes.normalTextColor}>
                        <b>[PrivateKeyPassword]</b> =Token, <b>[Mode]</b>=Mode
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>ToshaniBank :</strong>
                    <span className={classes.normalTextColor}>
                        use <b>[Secretkey]</b> =Token
                    </span>
                </h3>
            </label>
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong> OnePaisa Payout: </strong>
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = Salt, <b>[Token]</b> = Token ,{' '}
                        <b>[BaseURL]</b> = ActionURL ,<b>[AuthCode]</b> =
                        MerchantID
                    </span>
                </h3>
            </label>
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong> Zwitch Payout: </strong>
                    <span className={classes.normalTextColor}>
                        use <b>[Authorization]</b> = MerchantID,{' '}
                        <b>[AccountID]</b> = Token
                    </span>
                </h3>
            </label>
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong> VSmart Payout: </strong>
                    <span className={classes.normalTextColor}>
                        use <b>[APIToken]</b> = Token, <b>[IP]</b> = MerchantID
                    </span>
                </h3>
            </label>
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong> EKOPayout: </strong>
                    <span className={classes.normalTextColor}>
                        use <b>[Dedeveloperkey]</b> = Token, <b>[SercretKey]</b>{' '}
                        = MerchantID,
                        <b>[UserCode]</b> = Salt,<b>[InitiatorID]</b> =
                        ActionURL, <b>[Mode]</b> = Mode
                    </span>
                </h3>
            </label>
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong> JusPay Payout: </strong>
                    <span className={classes.normalTextColor}>
                        use <b>[APIKey]</b> = Salt, <b>[MerchantID]</b> = Token,
                        <b>[Mode]</b> = Mode
                    </span>
                </h3>
            </label>
        </CustomContainer>
    );
};
