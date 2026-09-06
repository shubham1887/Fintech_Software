import React, { useEffect, useState, useContext } from 'react';
import { Formik, Form } from 'formik';
import { LOGOUT } from 'ConstComponents';
import { useHistory } from 'react-router';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import { SelectProfile as USER_SELECT_BY_ID } from 'Components/Api/UsersApi';
import { SelectForeCollection as WALLET_SELECT_ECOLLECTION } from 'Components/Api/WalletApi';
import { SelectForStaticQR as WALLET_SELECT_STATIC_QR } from 'Components/Api/WalletApi';
import { ProfileUpload as PROFILE_UPLOAD } from 'Components/Api/UsersApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import { ProfileUpdate as PROFILE_UPDATE } from 'Components/Api/UsersApi';
import { SelectParentBanks as SELECT_PARENT_BANK } from 'Components/Api/UserBankDetailApi';
import { AmountInWordLabel } from 'Components/Inputes/FormikMaterialInput';
import { BalanceContext } from 'Components/BalanceContext';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { Deactivate as USER_ACTIVATE } from 'Components/Api/UsersApi';

import CopyToClipBoardInput from 'Components/CopyToClipBoardInput';
import {
    ADMIN_CODE,
    RETAILER_CODE,
    API_USER_CODE,
    DIST_CODE,
    SUPPORT_CODE,
    SUPERDIST_CODE,
    CLIENT_CODE,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import {
    Container,
    Grid,
    Paper,
    Button,
    TextField,
    Box,
    CircularProgress,
    MenuItem
} from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);
var currentUsertypeid = localStorage.getItem('UserTypeID');
var IsEkycUserProfileUpdate = localStorage.getItem('IsEkycUserProfileUpdate');
const isEditable = IsEkycUserProfileUpdate === 'no';

export const SelectProfile = () => {
    const { ProfileImageRefresh } = useContext(BalanceContext);
    const classes = commonStyle();

    const [ProfileImage, setProfileImage] = useState('');
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [SaticQRwallets, setSaticQRwallets] = useState([]);
    const [Ecollectionwallets, setEcollectionwallets] = useState([]);
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [isClientRole, setIsClientRole] = useState(false);
    const [isRetailerORAdmin, setIsRetailerORAdmin] = useState(false);
    const [isOpertionalRole, setIsOpertionalRole] = useState(false);
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [isAlertAllOpen, setIsAlertAllOpen] = useState(false);
    const history = useHistory();
    const [formObject, setFormObject] = useState({
        UserTypeID: '',
        UserType: '',
        ParentID: 0,
        FirmName: '',
        PersonName: '',
        MobileNumber: '',
        EmailID: '',
        GSTNo: '',
        PANNo: '',
        PinCode: '',
        City: '',
        AadharNo: '',
        Address: '',
        ShopAddress: '',
        StateName: '',
        ParentFirmName: '',
        ParentMobileNumber: '',
        ParentRole: '',
        KYCStatus: '',
        Pin: '',
        VirtualAccountNumber: '',
        IsInvoiceApplicable: '',
        IsChannel: '',
        SlabName: '',
        IsTwoFA: false,
        IsPin: false,
        IsReferralPopup: false,
        CommissionOnTransfer: 0,
        NumberFirst: false,
        AllowRealAPI: false,
        WalletIDforEcollection: 0,
        StaticQRWalletID: 0,
        WhatsAppNumber: '',
        ReferralURL: '',
        IsCommissionHold: false,
        ResponseType: '',
        Document: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        }
    });

    const SelectForeCollection = async () => {
        const response = await WALLET_SELECT_ECOLLECTION();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);

        setEcollectionwallets(data);
    };
    const SelectForStaticQR = async () => {
        const response = await WALLET_SELECT_STATIC_QR();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }
        var data = JSON.parse(response.data);

        setSaticQRwallets(data);
    };
    const WalletListSaticQR = SaticQRwallets.map((item) => (
        <MenuItem key={item.WalletID} value={item.WalletID}>
            {item.Name}
        </MenuItem>
    ));
    const WalletListEcollection = Ecollectionwallets.map((item) => (
        <MenuItem key={item.WalletID} value={item.WalletID}>
            {item.Name}
        </MenuItem>
    ));
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;
        setSubmitting(true);

        var requestObj = JSON.stringify(values);

        if (values.Upload) {
            setLoadingOpen(true);
            setIsError(false);

            var response = await PROFILE_UPLOAD(requestObj);

            setLoadingOpen(false);

            if (!response.isAuthorized) LOGOUT();

            if (!response.status) {
                setSeverity('error');
                setResponseMessage(response.data);
                setIsError(true);
                return;
            }
            var data = JSON.parse(response.data);
            setResponseMessage(data.Message);
            setSeverity('success');
            setIsError(true);
            localStorage.setItem('ProfileImagePath', data.ProfileImagePath);
            ProfileImageRefresh();

            SelectByID();
            setSubmitting(false);

            return;
        }

        setLoadingOpen(true);
        setIsError(false);

        var responseUpload = await PROFILE_UPDATE(requestObj);

        setLoadingOpen(false);

        if (!responseUpload.isAuthorized) LOGOUT();
        setResponseMessage(responseUpload.data);
        if (!responseUpload.status) {
            setSeverity('error');
            setIsError(true);
            return;
        }

        setSeverity('success');
        setIsError(true);
        localStorage.setItem('IsPin', values.IsPin);
        localStorage.setItem('NumberFirst', values.NumberFirst);
        //localStorage.setItem('IsCommissionHold', values.IsCommissionHold);

        SelectByID();
        setSubmitting(false);
    };
    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) {
            setIsAdminRole(true);
            setIsRetailerORAdmin(true);
            setIsOpertionalRole(false);
        }
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
            setIsRetailerORAdmin(true);
            setIsOpertionalRole(true);
        }
        if (API_USER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
            setIsOpertionalRole(true);
        }
        if (DIST_CODE === parseInt(currentUsertypeid)) {
            setIsOpertionalRole(true);
        }
        if (SUPERDIST_CODE === parseInt(currentUsertypeid)) {
            setIsOpertionalRole(true);
        }
        if (SUPPORT_CODE === parseInt(currentUsertypeid)) {
            setIsOpertionalRole(false);
        }
        if (CLIENT_CODE === parseInt(currentUsertypeid)) {
            setIsClientRole(true);
        }
    };
    const SelectParentBanks = async () => {
        if (isOpertionalRole === false) {
            return;
        }
        createColumns();
        const response = await SELECT_PARENT_BANK();
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        if (response.status) {
            setItemList(data);
        }
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });
        columns.push({
            label: 'HolderName',
            name: 'HolderName'
        });

        columns.push({
            label: 'BankName',
            name: 'BankName'
        });

        columns.push({
            label: 'Acc.No',
            name: 'AccountNumber'
        });
        columns.push({
            label: 'IFSC',
            name: 'IFSC'
        });
        columns.push({
            label: 'Branch',
            name: 'Branch'
        });
        setMainColumns(columns);
    };
    useEffect(() => {
        setLoadingOpen(true);
        CheckUserRole();
        SelectByID();
        SelectForStaticQR();
        SelectForeCollection();
        SelectParentBanks();

        if (!ProfileImage) {
            return;
        }
        var img = new Image();
        img.onload = function () {
            let newWidth = (this.width * 200) / this.height;
        };
        img.src = ProfileImage;
    }, []);

    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
    };

    const SelectByID = async () => {
        setLoadingOpen(true);

        const { isAuthorized, status, data } = await USER_SELECT_BY_ID();
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) return;

        var d = JSON.parse(data);

        setProfileImage(d.ProfileImagePath);
        setFormObject({
            UserType: d.UserType,
            ParentID: d.ParentID,
            FirmName: d.FirmName,
            PersonName: d.PersonName,
            WalletIDforEcollection: d.WalletIDforEcollection,
            StaticQRWalletID: d.StaticQRWalletID,
            MobileNumber: d.MobileNumber,
            EmailID: d.EmailID,
            GSTNo: d.GSTNo,
            PANNo: d.PANNo,
            PinCode: d.PinCode,
            City: d.City,
            AadharNo: d.AadharNo,
            Address: d.Address,
            ShopAddress: d.ShopAddress,
            StateName: d.StateName,
            ParentFirmName: d.ParentFirmName,
            ParentMobileNumber: d.ParentMobileNumber,
            KYCStatus: d.KYCStatus,
            Pin: d.Pin,
            IsReferralPopup: d.IsReferralPopup,
            VirtualAccountNumber: d.VirtualAccountNumber,
            IsInvoiceApplicable: d.IsActiveWebStr,
            IsChannel: d.IsChannelStr,
            SlabName: d.SlabName,
            IsTwoFA: false,
            AllowRealAPI: d.AllowRealAPI,
            CommissionOnTransfer: d.CommissionOnTransfer,
            IsPin: d.IsPin,
            NumberFirst: d.NumberFirst,
            WhatsAppNumber: d.WhatsAppNumber,
            ReferralURL: d.ReferralURL,
            IsCommissionHold: d.IsCommissionHold,
            ResponseType: d.ResponseType
        });
    };

    const Deactivate = async () => {
        setIsError(false);
        setLoadingOpen(true);
        const response = await USER_ACTIVATE();

        if (!response.isAuthorized) LOGOUT();
        setLoadingOpen(false);

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) setSeverity('error');

        setTimeout(() => {
            history.push('/Login');
        }, MESSAGE_CLOSE_AFTER);
    };

    const DeactivateClick = () => {
        setIsAlertAllOpen(true);
    };

    const DeactiveAccountConfirmation = (
        <ConfirmAlert
            isOpen={isAlertAllOpen}
            title="Deactivate Account Confirmation"
            message="Are you sure to Deactivate Your Account."
            positiveButtonClick={() => {
                setIsAlertAllOpen(false);
                Deactivate();
            }}
            negativeButtonClick={() => {
                setIsAlertAllOpen(false);
            }}
        />
    );

    const UploadForm = (formProps) => {
        return (
            <Form>
                <Grid container spacing={2}>
                    <Grid item xs={12} sm={12} md={8} lg={8}>
                        <h2 className={classes.PaperHead_h2}>Profile </h2>
                    </Grid>
                    {!isAdminRole ? (
                        <Grid item xs={12} sm={12} md={4} lg={4}>
                            <Box className={classes.flexReverse}>
                                <Button
                                    variant="contained"
                                    color="secondary"
                                    title="DeactivateAccount"
                                    className={classes.CustomDangerButtonStyle}
                                    onClick={() => {
                                        DeactivateClick();
                                    }}
                                >
                                    Deactivate Account
                                </Button>

                                <Button
                                    variant="contained"
                                    color="primary"
                                    className={classes.CustomPrimaryButtonStyle}
                                    onClick={() => {
                                        history.push('/CommissionStructure');
                                    }}
                                    style={{ marginRight: 10 }}
                                >
                                    Commission
                                </Button>
                            </Box>
                        </Grid>
                    ) : null}
                </Grid>
                <Grid item xs={12}>
                    <Paper className={classes.PaperStyle}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <MaterialTextField
                                    label="FirmName"
                                    name="FirmName"
                                    type="text"
                                    readOnly={true}
                                    placeholder="Name"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <MaterialTextField
                                    label="User Type"
                                    name="UserType"
                                    type="text"
                                    readOnly={true}
                                    placeholder="User Type"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <MaterialTextField
                                    label="Mobile Number"
                                    name="MobileNumber"
                                    type="text"
                                    readOnly={true}
                                    placeholder="Mobile Number"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <MaterialTextField
                                    label="EmailID"
                                    name="EmailID"
                                    type="text"
                                    readOnly={true}
                                    placeholder="EmailID"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <MaterialTextField
                                    label="State Name"
                                    name="StateName"
                                    type="text"
                                    readOnly={true}
                                    placeholder="State Name"
                                />
                            </Grid>
                            {!isAdminRole ? (
                                <>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialTextField
                                            label="Parent"
                                            name="ParentFirmName"
                                            type="text"
                                            readOnly={true}
                                            placeholder="Parent"
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialTextField
                                            label="Parent Mobile"
                                            name="ParentMobileNumber"
                                            type="text"
                                            readOnly={true}
                                            placeholder="Parent Mobile"
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialTextField
                                            label="KYC Approved"
                                            name="KYCStatus"
                                            type="text"
                                            readOnly={true}
                                            placeholder="KYC Approved"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialTextField
                                            label="Virtual Account Number"
                                            name="VirtualAccountNumber"
                                            type="text"
                                            readOnly={true}
                                            placeholder="VirtualAccountNumber"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialTextField
                                            label="CommissionOnTransfer"
                                            name="CommissionOnTransfer"
                                            type="text"
                                            readOnly={true}
                                            placeholder="Commission On Transfer"
                                        />
                                    </Grid>
                                </>
                            ) : null}

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialTextField
                                    label="Pin"
                                    name="Pin"
                                    type="password"
                                    readOnly={true}
                                    placeholder="Pin"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <MaterialTextField
                                    label="ReferralURL"
                                    name="ReferralURL"
                                    type="text"
                                    readOnly={true}
                                    placeholder="ReferralURL"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <MaterialTextField
                                    label="IsCommissionHold"
                                    name="IsCommissionHold"
                                    type="text"
                                    readOnly={true}
                                    placeholder="IsCommissionHold"
                                />
                            </Grid>

                            {!isRetailerORAdmin ? (
                                <>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialTextField
                                            label="IsInvoiceApplicable"
                                            name="IsInvoiceApplicable"
                                            type="text"
                                            readOnly={true}
                                            placeholder="IsInvoiceApplicable"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialTextField
                                            label="SlabName"
                                            name="SlabName"
                                            type="text"
                                            readOnly={true}
                                            placeholder="SlabName"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialTextField
                                            label="IsChannel"
                                            name="IsChannel"
                                            type="text"
                                            readOnly={true}
                                            placeholder="IsChannel"
                                        />
                                    </Grid>
                                </>
                            ) : null}
                        </Grid>
                    </Paper>
                </Grid>
                <h2 className={classes.PaperHead_h2}>Edit</h2>
                <Grid item xs={12}>
                    <Paper className={classes.PaperStyle}>
                        <Grid container spacing={3}>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="Person Name"
                                    name="PersonName"
                                    type="text"
                                    placeholder="Person Name"
                                    readOnly={isEditable}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialNumberField
                                    label="WhatsApp Number"
                                    name="WhatsAppNumber"
                                    type="text"
                                    length={10}
                                    title="WhatsApp Number"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="GST No"
                                    name="GSTNo"
                                    type="text"
                                    length={15}
                                    placeholder="GST No"
                                    readOnly={isEditable}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialNumberField
                                    label="Aadhar No"
                                    name="AadharNo"
                                    type="text"
                                    length={12}
                                    title="Aadhar Number"
                                    readOnly={isEditable}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="Address"
                                    name="Address"
                                    type="text"
                                    placeholder="Address"
                                    readOnly={isEditable}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="Shop Address"
                                    name="ShopAddress"
                                    type="text"
                                    placeholder="Shop Address"
                                    readOnly={isEditable}
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="PAN No"
                                    name="PANNo"
                                    type="text"
                                    placeholder="PAN No"
                                    readOnly={isEditable}
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="City"
                                    name="City"
                                    type="text"
                                    placeholder="City"
                                    readOnly={isEditable}
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialNumberField
                                    label="PinCode"
                                    name="PinCode"
                                    type="text"
                                    length={6}
                                    placeholder="PinCode"
                                    readOnly={isEditable}
                                />
                            </Grid>
                            {!isAdminRole ? (
                                <>
                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialSelect
                                            label="e-Collection Amount deposit in"
                                            name="WalletIDforEcollection"
                                            title="e-Collection Amount deposit in"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Wallet
                                            </MenuItem>
                                            {WalletListEcollection}
                                        </MaterialSelect>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialSelect
                                            label="StaticQR Amount deposit in"
                                            name="StaticQRWalletID"
                                            title="StaticQR Amount deposit in"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Wallet
                                            </MenuItem>
                                            {WalletListSaticQR}
                                        </MaterialSelect>
                                    </Grid>
                                    {isRetailerRole || isClientRole ? (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                label="ResponseType"
                                                name="ResponseType"
                                                title="ResponseType"
                                            >
                                                <MenuItem key="ALL" value="ALL">
                                                    Select Type
                                                </MenuItem>
                                                <MenuItem
                                                    key="Initial"
                                                    value="Initial"
                                                >
                                                    Initial
                                                </MenuItem>
                                                <MenuItem
                                                    key="Callback"
                                                    value="Callback"
                                                >
                                                    Callback
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>
                                    ) : null}

                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <FormikMaterialSwitch
                                            label="IsReferralPopup"
                                            name="IsReferralPopup"
                                            onCustomCallback={(name, value) => {
                                                OnChange(
                                                    name,
                                                    value,
                                                    formProps.setFieldValue
                                                );
                                            }}
                                        />
                                    </Grid>
                                </>
                            ) : null}
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <FormikMaterialSwitch
                                    label="IsPin"
                                    name="IsPin"
                                    onCustomCallback={(name, value) => {
                                        OnChange(
                                            name,
                                            value,
                                            formProps.setFieldValue
                                        );
                                    }}
                                />
                            </Grid>

                            {isRetailerRole || isClientRole ? (
                                <>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <FormikMaterialSwitch
                                            label="NumberFirst"
                                            name="NumberFirst"
                                            onCustomCallback={(name, value) => {
                                                OnChange(
                                                    name,
                                                    value,
                                                    formProps.setFieldValue
                                                );
                                            }}
                                        />
                                    </Grid>
                                </>
                            ) : null}

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <PrimaryButton
                                    type="submit"
                                    title="Profile update"
                                    className={classes.CustomPrimaryButtonStyle}
                                    onClick={(e) => {
                                        formProps.setFieldValue('Update', true);

                                        formProps.setFieldValue(
                                            'Upload',
                                            false
                                        );

                                        // handleSubmit(e);
                                    }}
                                    disabled={loadingOpen}
                                >
                                    Update
                                </PrimaryButton>
                                {loadingOpen && (
                                    <CircularProgress
                                        size={24}
                                        className={classes.buttonProgress}
                                    />
                                )}
                            </Grid>
                        </Grid>
                    </Paper>
                </Grid>

                <h2 className={classes.PaperHead_h2}>Profile Upload</h2>
                <Grid item xs={12}>
                    <Paper className={classes.PaperStyle}>
                        <Grid container spacing={3}>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <label>Profile Image</label>
                                <TextField
                                    id="file"
                                    name="Document"
                                    type="file"
                                    accept=".png, .jpg, .jpeg"
                                    onChange={(e) => {
                                        var file = e.target.files[0];

                                        setProfileImage(
                                            URL.createObjectURL(file)
                                        );

                                        var reader = new FileReader();
                                        formProps.setFieldValue(
                                            'Document.Name',
                                            file.name
                                        );
                                        formProps.setFieldValue(
                                            'Document.Size',
                                            file.size
                                        );
                                        formProps.setFieldValue(
                                            'Document.Type',
                                            file.type
                                        );
                                        reader.onload = function (item) {
                                            formProps.setFieldValue(
                                                'Document.Data',
                                                item.target.result
                                                    .replace('data:', '')
                                                    .replace(/^.+,/, '')
                                            );
                                        };

                                        reader.readAsDataURL(file);
                                    }}
                                />
                                <AmountInWordLabel
                                    name="Note"
                                    readOnly={true}
                                    title="Only jpg/ jpeg/ pdf and png files are allowed under 3MB!"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <img
                                    name="OperatorImage"
                                    src={ProfileImage}
                                    height={60}
                                    width={80}
                                    alt={ProfileImage}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <PrimaryButton
                                    type="submit"
                                    title="Profile upload"
                                    className={classes.CustomPrimaryButtonStyle}
                                    onClick={(e) => {
                                        formProps.setFieldValue(
                                            'Update',
                                            false
                                        );

                                        formProps.setFieldValue('Upload', true);

                                        //   handleSubmit(e);
                                    }}
                                    disabled={loadingOpen}
                                >
                                    Upload
                                </PrimaryButton>
                                {loadingOpen && (
                                    <CircularProgress
                                        size={24}
                                        className={classes.buttonProgress}
                                    />
                                )}
                            </Grid>
                        </Grid>
                    </Paper>
                </Grid>
                {isAlertAllOpen ? DeactiveAccountConfirmation : null}
            </Form>
        );
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
                initialValues={formObject}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {UploadForm}
            </Formik>
            {isOpertionalRole === true ? (
                <div>
                    <h2 className={classes.PaperHead_h2}>
                        Parent Bank Details
                    </h2>
                    <ReactTable
                        data={itemList}
                        columns={mainColumns}
                        rowsPerPage={10}
                        headerToolbar={false}
                    />
                </div>
            ) : null}
        </CustomContainer>
    );
};
