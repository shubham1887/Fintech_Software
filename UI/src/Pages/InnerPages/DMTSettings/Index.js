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
    Select as SELECT_DMT_SETTINGS,
    SelectByID as SELECT_DMT_SETTINGS_BY_ID,
    Update as UPDATE_DMT_SETTINGS
} from 'Components/Api/DMTSettingsAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { API_URL } from 'Config';
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
    const [callbackURL, setcallbackURL] = useState('');
    async function fetchcallbackURL() {
        setcallbackURL(await API_URL());
    }
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [FormObj, setFormObj] = useState({
        DMTSettingID: 0,
        Name: '',
        Mode: '',
        DisplayName: '',
        UserName: '',
        Token: '',
        IsActive: false,
        IsExpressDMT: false,
        CallBackURL: '',
        WalletID: '',
        SplitAmount: 0,
        MinAmount: 0,
        SenderLimit: 0,
        Sequence: 0,
        IsNeft: false,
        IsCCF: false,
        IsPayoutDMT: false
    });

    const FormValidationSchema = Yup.object({
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        UserName: Yup.string().required(PLEASE_PROVIDE_VALID + ' username'),
        Token: Yup.string().required(PLEASE_PROVIDE_VALID + ' token'),
        WalletID: Yup.string().required(PLEASE_PROVIDE_VALID + ' walletid'),
        CallBackURL: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' callbackurl'
        )
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setResponseMessage('');
        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        var response = await UPDATE_DMT_SETTINGS(requestObj);

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
            DMTSettingID: 0,
            Name: '',
            Mode: '',
            DisplayName: '',
            UserName: '',
            Token: '',
            IsActive: false,
            CallBackURL: '',
            WalletID: '',
            SplitAmount: 0,
            MinAmount: 0,
            SenderLimit: 0,
            Sequence: 0,
            IsNeft: false,
            IsCCF: false,
            IsPayoutDMT: false,
            IsExpressDMT: false
        });

        selectData();
    };

    useEffect(() => {
        createColumns();
        selectData();
        fetchcallbackURL();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'DMTSettingID',
            srno: true
        });

        columns.push({
            label: 'Edit',
            name: 'DMTSettingID',
            edit: true,
            callback: (DMTSettingID) => {
                setIsUpdate(true);
                SelectByID(DMTSettingID);
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
            label: 'WalletID',
            name: 'WalletID',
            displayAsPassword: true
        });
        columns.push({
            label: 'CallBackURL',
            name: 'CallBackURL',
            actived: true
        });
        columns.push({
            label: 'SplitAmount',
            name: 'SplitAmount',
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
            label: 'Neft',
            name: 'IsNeftStr',
            actived: true
        });

        columns.push({
            label: 'CCF',
            name: 'IsCCFStr',
            actived: true
        });
        columns.push({
            label: 'Mode',
            name: 'Mode',
            actived: true
        });
        columns.push({
            label: 'PayoutDMT',
            name: 'IsPayoutDMTStr',
            actived: true
        });
        columns.push({
            label: 'ExpressDMT',
            name: 'IsExpressDMTStr',
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
        const response = await SELECT_DMT_SETTINGS();

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

    const SelectByID = async (DMTSettingID) => {
        setIsError(false);

        let jsonObj = {
            DMTSettingID: DMTSettingID
        };
        const response = await SELECT_DMT_SETTINGS_BY_ID(
            JSON.stringify(jsonObj)
        );

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
            DMTSettingID: d.DMTSettingID,
            Name: d.Name,
            Mode: d.Mode,
            DisplayName: d.DisplayName,
            UserName: d.UserName,
            Token: d.Token,
            IsActive: d.IsActive,
            CallBackURL: d.CallBackURL,
            WalletID: d.WalletID,
            SplitAmount: d.SplitAmount,
            MinAmount: d.MinAmount,
            SenderLimit: d.SenderLimit,
            Sequence: d.Sequence,
            IsNeft: d.IsNeft,
            IsCCF: d.IsCCF,
            IsPayoutDMT: d.IsPayoutDMT,
            IsExpressDMT: d.IsExpressDMT
        });
        scrollToTop();
    };
    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
    };

    const dmtContentMap = {
        MasterMoneyDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>MASTERMONEY DMT</strong> :{' '}
                    <span className={classes.normalTextColor}>
                        use <b>[MobileNumber]</b> = UserName, <b>[Token]</b> =
                        Token,
                        <b>[Password]</b> = WalletID and <b>callback URL</b> =
                    </span>
                    <span className={classes.callback_url}>
                        {callbackURL} Callback/postMasterMoneyResponse
                    </span>
                </h3>
            </label>
        ),
        PayU: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>PAYU DMT </strong> : {''}
                    <span className={classes.normalTextColor}>
                        use <b>[Payout ID]</b> = UserName,
                        <b>[Client Secret]</b> = Token,
                        <b>[Client ID]</b> =WalletID, <b>callback URL</b> =
                    </span>
                    <span className={classes.callback_url}>
                        {callbackURL}
                        PayUCallback/postResponse
                    </span>
                </h3>
            </label>
        ),
        Paysprint: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>PAYSPRINT DMT</strong> : {''}
                    <span className={classes.normalTextColor}>
                        use <b>[PartnerID]</b> = UserName, <b>[JWTKEY]</b> =
                        WalletID,
                        <b>[Authorisedkey]</b> =Token, ,<b>CallBackURL</b> =
                    </span>
                    <span className={classes.callback_url}>[Key,IV]</span>
                </h3>
            </label>
        ),

        IServeULite: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>ISERVEULITE DMT</strong> :{''}
                    <span className={classes.normalTextColor}>
                        use <b>[ClientID]</b> = UserName, <b>[ClientSecret]</b>{' '}
                        =Token
                    </span>
                </h3>
            </label>
        ),
        PaytmCCFDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>PAYTMCCF DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Secret]</b> = Token, <b>[ParentID]</b> =
                        UserName ,<b>[ParentSubID]</b> = WalletID
                    </span>
                </h3>
            </label>
        ),

        OpenBankDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>OPENBANK DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[AccessKey]</b> = UserName,{' '}
                        <b>[DebitAccountNumber]</b> = WalletID,{' '}
                        <b>[SecretKey]</b> =Token
                    </span>
                </h3>
            </label>
        ),

        ICICI_Composite: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>ICICI COMPOSITE</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>DebitAccountNo</b> = UserName,{' '}
                        <b>ClientPrivateKeyPassword</b> = Token,
                        <b>PassCode</b> = WalletID, and{' '}
                        <b>
                            other param need to store in json in callback param
                            like
                        </b>{' '}
                        :
                        {`{"BcCode":"00", "AggrName":"AA", "AggrId":"A00", "CorpId":"B00", "CrpUsr":"C00", "ApiKey":"D00", "Urn": "E00", "IciciPublicKey": "PUBLIC KEY PATH", "ClientPrivateKey": "PRIVATE KEY PATH"}`}
                    </span>
                </h3>
            </label>
        ),

        Mahagram: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>MAHAGRAM</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = UserName, <b>[Token]</b> =
                        Token,
                    </span>
                </h3>
            </label>
        ),
        Eko: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>EKO DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Dedeveloperkey]</b> = Token, <b>[SercretKey]</b>{' '}
                        = UserName ,<b>[InitiatorID]</b> = WalletID
                    </span>
                </h3>
            </label>
        ),

        Ambika: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>AMBIKA DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserID]</b> = UserName (it is numeric),{' '}
                        <b>[AgentID]</b> = WalletID, <b>[Token]</b> = Token
                    </span>
                </h3>
            </label>
        ),

        MyNetLinkDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>MYNETLINKDMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = UserName, <b>[Token]</b> =
                        Token, <b>[AuthCode]</b>= WalletID , <b>ActionURL</b> :
                    </span>
                    <span className={classes.action_url}>
                        https://api.mynetlink.in/DMT
                    </span>
                </h3>
            </label>
        ),
        ZPlusDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>ZPLUS DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>Client id</b> = UserName, <b>[User id]</b> =
                        WalletID, <b>[key]</b>= Token
                    </span>
                </h3>
            </label>
        ),

        PayOneMoneyDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>PAYONEMONEY DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = UserName, <b>[Token]</b> = Token
                        , <b>ActionURL</b> :
                    </span>
                    <span className={classes.action_url}>
                        https://www.payonemoney.in/POMAPI/DMRService.asmx
                    </span>
                </h3>
            </label>
        ),

        JaiJuiDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>JAIJUI DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[MobileNumber]</b> = UserName, <b>[Token]</b> =
                        Token ,<b>ActionURL</b> :
                    </span>
                    <span className={classes.action_url}>
                        https://api.jaijuirecharge.in/JaijuiDMTAPI
                    </span>
                </h3>
            </label>
        ),

        CashFree: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>CASHFREE DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Client id]</b> = UserName, <b>[ClientSecret]</b>{' '}
                        = Token
                    </span>
                </h3>
            </label>
        ),

        EzyPay: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>EZYPAY DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[SecretKey]</b> = Auth Code
                    </span>
                </h3>
            </label>
        ),

        PrimePayDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>PRIMEPAY DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = UserName, <b>[Password]</b> =
                        Token, <b>[AuthCode]</b>= CallBackURL
                    </span>
                </h3>
            </label>
        ),

        ZPlusPayout: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>ZPLUSPAYOUT DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Client id]</b> = UserName , <b>[User id]</b> =
                        WalletID , <b>[key]</b>= Token
                    </span>
                </h3>
            </label>
        ),

        Razorpay: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>RAZORPAY DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[APIKey]</b> = UserName , <b>[APISecret]</b> =
                        Token ,<b>[AdminAccountNumber]</b> = WalletID
                    </span>
                    <span className={classes.normalTextColor}>
                        <b>[web hook need to set ]</b>{' '}
                        https://x.razorpay.com/auth/ -- setting -- webhook ,
                        <b>[set for 5 Events]</b> :<b>[set url as]</b> use :
                    </span>
                    <span className={classes.callback_url}>
                        {callbackURL}
                        RazorPayXCallback/webhook_Payout
                    </span>
                </h3>
            </label>
        ),

        RoyalPayDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>ROYALPAY DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = UserName , <b>[Token]</b> =
                        Token
                    </span>
                </h3>
            </label>
        ),

        InstantPayPayout: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>INSTANTPAYPAYOUT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[ClientID]</b> = UserName , <b>[ClientSecret]</b>{' '}
                        = Token ,<b>[AuthCode]</b> = WalletID and
                        <b>[BankId,BankProfileId,PayerAccountNumber]</b> =
                    </span>
                    <span className={classes.action_url}>
                        ActionURL (ex. abc,xyz,mno)
                    </span>
                    <span className={classes.normalTextColor}>
                        <b>callback URL</b> ={' '}
                    </span>
                    <span className={classes.callback_url}>
                        {callbackURL} InstantPayCallback/WebhookResponse
                    </span>
                </h3>
            </label>
        ),

        Paisarewards: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>PAISAREWARDSPAYOUT DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[PaisaRewardsId]</b> = UserName , <b>[Salt]</b> =
                        Token , <b>[MID]</b> = WalletID
                    </span>
                </h3>
            </label>
        ),

        Dikshupay: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>DIKSHUPAYPAYOUT DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Token]</b> = Token
                    </span>
                </h3>
            </label>
        ),

        EaseBuzz: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>EASEBUZZ DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Token]</b> = Key , <b>[Slat]</b> = UserName
                    </span>
                </h3>
            </label>
        ),

        Shantilex: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>SHANTILEX DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserID]</b> = UserName (it is numeric),{' '}
                        <b>[AgentID]</b> = WalletID, <b>[Token]</b> =Token ,{' '}
                        <b>ActionURL</b> :
                    </span>
                    <span className={classes.action_url}>[BaseURL]</span>
                </h3>
            </label>
        ),

        SafexPayDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>SAFEXPAY DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Encryptionkey]</b> = Token , <b>[AGID]</b> =
                        UserName , <b>[MID]</b>= WalletID ,
                    </span>
                    <span className={classes.normalTextColor}>
                        <b>callback URL</b> ={' '}
                    </span>
                    <span className={classes.callback_url}>
                        {callbackURL}SafexCallback/postResponse
                    </span>
                </h3>
            </label>
        ),

        FidyPay: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>FIDYPAYPAYOUT DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Username]</b> = Mode , <b>[Password]</b> =
                        UserName , <b>[ClientId]</b>= Token ,{' '}
                        <b>[ClientSecret]</b> = WalletID ,{' '}
                        <b>[bankaccountkey] CallBackURL</b>
                    </span>
                </h3>
            </label>
        ),

        RMGPayDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>RMGPAY DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = UserName , <b>[Token]</b> =
                        Token , <b>ActionURL</b> :
                    </span>
                    <span className={classes.action_url}>
                        https://www.rmgpayapi.com/rmgapi/RIDMRService.asmx
                    </span>
                </h3>
            </label>
        ),

        EzyPayPaytm: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>EZYPAYPAYTM DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[SecretKey]</b> = Auth Code
                    </span>
                </h3>
            </label>
        ),

        IServeUDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>ISERVEU DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[ClientID]</b> = UserName, <b>[ClientSecret]</b>{' '}
                        = Token ,<b>ActionURL</b> =
                    </span>
                    <span className={classes.action_url}>
                        https://apigatewayprod.iserveu.tech
                    </span>
                    <span className={classes.normalTextColor}>
                        <b> use callback URL as </b>
                    </span>
                    <span className={classes.callback_url}>
                        "{callbackURL} IServeUCallback/postResponse"
                    </span>
                </h3>
            </label>
        ),

        HyptoDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>HYPTO DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[key]</b> = API Token
                    </span>
                </h3>
            </label>
        ),

        YesBankDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>YESBANK DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[MerchantID]</b> = UserName ,<b>[MerchantKey]</b>{' '}
                        = WalletID ,
                        <b>
                            [XIBMClientSecret,XIBMClientID,PrivateKeyFilePath]
                        </b>{' '}
                        =
                    </span>
                    <span className={classes.action_url}>
                        <b>ActionURL</b>
                    </span>
                    ,
                    <span className={classes.normalTextColor}>
                        <b>[PrivateKeyPassword]</b> = Token , <b>[Mode]</b> =
                        Mode,
                    </span>
                </h3>
            </label>
        ),

        MottoPeDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>MOTTOPE DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = UserName ,<b>[Token]</b> = Token
                        ,<b>[AuthCode]</b> = WalletID ,{' '}
                        <b>BasicURL = callback URL </b>
                    </span>
                    <span className={classes.callback_url}>
                        "https://api.mottope.in/DMT"
                    </span>
                </h3>
            </label>
        ),

        ThoshaniBankDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>TOSHANIBANK DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Secretkey]</b> = Token
                    </span>
                </h3>
            </label>
        ),

        VSmartDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>VSmart DMT</strong> :{' '}
                    <span className={classes.normalTextColor}>
                        use <b>[UserName]</b> = IP, <b>[APIToken]</b> = Token,
                    </span>
                </h3>
            </label>
        ),

        BranchxDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>Branchx DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Token]</b> = Token
                    </span>
                </h3>
            </label>
        ),

        RoundPayDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>RoundPay DMT</strong> :{' '}
                    <span className={classes.normalTextColor}>
                        use <b>[UserID]</b> = UserName, <b>[Token]</b> = Token,
                        <b>[OutletID]</b> = WalletID and <b>callback URL</b> =
                    </span>
                    <span className={classes.callback_url}>
                        "https://api.roundpay.net/"
                    </span>
                </h3>
            </label>
        ),
        EkoPayoutDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>EKOPayout DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Dedeveloperkey]</b> = Token, <b>[SercretKey]</b>{' '}
                        = UserName ,<b>[InitiatorID]</b> = WalletID,
                        <b>[Mode]</b>= Mode
                    </span>
                </h3>
            </label>
        ),
        VastWebDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>VastWeb DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[UserID]</b> = UserName, <b>[Token]</b>= Token
                    </span>
                </h3>
            </label>
        ),
        ZwitchDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>Zwitch DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Authorization]</b> = UserName,{' '}
                        <b>[AccountID]</b>= Token
                    </span>
                </h3>
            </label>
        ),
        WaayuPayDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>WaayuPay DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[Email]</b> = Token, <b>[Password]</b> =
                        UserName,
                        <b>[UserKey]</b> = WalletID,<b>[Mode]</b> = Mode
                    </span>
                </h3>
            </label>
        ),
        JusPayDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>JusPay DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[APIKey]</b> = Token, <b>[MerchantID]</b> =
                        UserName, <b>[Mode]</b> = Mode
                    </span>
                </h3>
            </label>
        ),
        LevinDMT: (
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong>Levin DMT</strong> :
                    <span className={classes.normalTextColor}>
                        use <b>[APIToken]</b> = Token, <b>[UserID]</b> =
                        UserName, <b>[MobileNum]</b> = WalletID
                    </span>
                </h3>
            </label>
        )
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
                    <h2 className={classes.PaperHead_h2}>DMT Settings </h2>
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
                                                md={2}
                                                lg={2}
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
                                                md={2}
                                                lg={2}
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
                                                md={2}
                                                lg={2}
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
                                                md={2}
                                                lg={2}
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
                                                md={2}
                                                lg={2}
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
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialTextField
                                                    label="WalletID / subwalletID"
                                                    name="WalletID"
                                                    type="text"
                                                    placeholder="Wallet / subwalletID"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
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
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialNumberField
                                                    label="Split Amount"
                                                    name="SplitAmount"
                                                    type="text"
                                                    placeholder="Split Amount"
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
                                                md={2}
                                                lg={2}
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
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <FormikMaterialSwitch
                                                    label="ExpressDMT"
                                                    name="IsExpressDMT"
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

            {/* <label>
                <h3 className={classes.Provider_Format}>
                    Alle DMT: [MobileNumber] =UserName, [ApiToken] =Token
                </h3>
            </label> <br /> */}

            {itemList.map((item, index) => {
                if (dmtContentMap[item.Name]) {
                    return <div key={index}>{dmtContentMap[item.Name]}</div>;
                }
                return null;
            })}

            {/* <label>
                <h3 className={classes.Provider_Format}>
                    Hypto DMT: use [key] = API Token
                </h3>
            </label>
            <br /> */}
            {/* <label>
                <h3 className={classes.Provider_Format}>
                    IDMT (InstantPay) : use [UserName]= [key] = [debitAccount],
                    [Token] =Token,
                </h3>
            </label>
            <br /> */}
            {/* <label>
                <h3 className={classes.Provider_Format}>
                    ePocketDMT : use [Token] =Token
                </h3>
            </label> <br /> */}

            {/* <label>
                <h3 className={classes.Provider_Format}>
                    MinksDMT : use [AgentID] =WalletID, [UserName] =UserName,
                    [SecretKey] =Token, [TimeStamp] =CallBackURL
                </h3>
            </label>
            <br /> */}

            {/* <br />
            <label>
                <h3 className={classes.Provider_Format}>
                Cyberplate : use [key] = API Token
                </h3>
            </label> */}

            {/* <label>
                <h3 className={classes.Provider_Format}>
                    IMoneyPay : use [key] = API Token
                </h3>
            </label>
            <br /> */}
            {/* 
            
            <label>
                <h3 className={classes.Provider_Format}>
                    Eko DMT: use [Dedeveloperkey] =Token, [SercretKey]
                    =UserName, [InitiatorID] =WalletID
                </h3>
            </label>
            <br /> <br /> */}
        </CustomContainer>
    );
});
