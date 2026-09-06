import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import TabList from '@material-ui/lab/TabList';
import TabContext from '@material-ui/lab/TabContext';
import TabPanel from '@material-ui/lab/TabPanel';
import { makeStyles } from '@material-ui/core/styles';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { Grid, Paper, AppBar, Tab, MenuItem, Container, Button } from '@material-ui/core';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { PrimaryButton, SecondaryButton } from 'Components/Inputes/CustomButton';
import ResetIcon from 'Components/SvgButtons/ResetIcon';
import RechargeIcon from 'Components/SvgButtons/RechargeIcon';
import { SelectByID as SELECT_BY_ID } from 'Components/Api/OnboardingCallbackAPI';
import { LOGOUT } from 'ConstComponents';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
//import SlabCommission from './SlabCommisuionDetails';
import { Select as SLAB_SELECT } from 'Components/Api/SlabApi';
import { SelectBySlabMasterID as SLAB_DETAIL_SELECT_SLABMASTERID } from 'Components/Api/SlabDetailApi';
import { useHistory } from 'react-router';
import { ManualInsert as INSERT } from 'Components/Api/OnboardingCallbackAPI';
import ImageUploadComponent from 'Components/ImageUploadComponent';
import { Select as USERTYPE_SELECT } from 'Components/Api/UserTypeApi';
const CustomContainer = HOCLoader(Container);

const useStyles = makeStyles((theme) => ({
    marginTab: {
        padding: '0px',
        paddingTop: '24px',
        overflowWrap: 'break-word'
    },
    appBarHeight: {
        height: 50,
        marginTop: '10px'
    },
    p10: {
        padding: 10
    },
    document_h4: {
        fontWeight: 'normal',
        fontSize: 16
    },
    blue_h4: {
        color: '#481BAB',
        fontWeight: 'bold',
        fontSize: 16
    },
    method: {
        fontSize: 16,
        color: '#FFA500',
        fontWeight: 'bold'
    },
    response: {
        fontSize: 16,
        color: '#00807A',
        fontWeight: 'bold'
    },
    Tab: {
        overflowWrap: 'break-word',
        paddingLeft: '10%',
        paddingRight: '10%'
    }
}));


export const ManualAgentOnboarding = () => {
    const classes = commonStyle();
    const classesInner = useStyles();
    const history = useHistory();
    const [tabIndex, setTabIndex] = useState('1');
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);

    const [Slabs, setSlabs] = useState([]);
    const [severity, setSeverity] = useState('success');
    const [isSlabCommission, setSlabCommission] = useState([]);
    const [isDocumentList, setDocumentList] = useState({});
    const [userTypeList, setUserTypeList] = useState([]);
    const handleChange = (event, newValue) => {
        setTabIndex(newValue);
    };
    useEffect(() => {
        SelectByID();
        selectSlabs();
        selectUserTypes();
    }, []);
    const [UserObj, setUserObj] = useState({
        FullName: "",
        DOB: "",
        FirmName: "",
        EmailID: "",
        PanNo: "",
        MobileNumber: "",
        Address: "",
        IFSC: "",
        AccountNumber: "",
        Gender: "",
        PinCode: "",
        BankName: "",
        DOB: new Date(),
        Residential_Address: "",
        Business_Address: "",
        Residential_Locality: "",

        GstNo: "",
        SlabID: "",
        State: "",
        Business_Locality: "",
        Country: "",
        PinCode: "",
        SlabID: ""
    });

    const selectSlabs = async () => {
        const response = await SLAB_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);
        setSlabs(data);
    };
    const SelectByID = async () => {
        let URL = window.location.pathname;
        var res = URL.split('/');

        // let onboardingUserID = res[res.length - 1];

        let JSONObj = {
            //     OnboardingUserID: onboardingUserID
        };


        var response = await SELECT_BY_ID(JSON.stringify(JSONObj));
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {

            setSeverity('response');
            setIsError(true);
            setResponseMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);
        setUserObj({
            FullName: data.FullName,
            DOB: data.DOB,
            FirmName: data.FirmName,
            EmailID: data.EmailID,
            PanNo: data.PanNo,
            MobileNumber: data.MobileNumber,
            Address: data.Address,
            IFSC: data.IFSC,
            AccountNumber: data.AccountNumber,
            Gender: data.Gender,
            PinCode: data.PinCode,
            BankName: data.BankName,
            Residential_Address: data.Residential_Address,
            Business_Address: data.Business_Address,
            GstNo: data.GstNo,
            SlabID: data.SlabID,
            Residential_Locality: data.Residential_Locality,
            State: data.State,
            Business_Locality: data.Business_Locality,
            Country: data.Country,
            PinCode: data.PinCode,

        });
        setDocumentList(data.list);
        SelectBySlabMasterID1(data.SlabID);
    }

    const SelectBySlabMasterID1 = async (values) => {
        setSlabCommission([]);
        if (values === "" || values === 0) {
            return;
        }

        const inputObj = {
            SlabMasterID: values
        }
        const response = await SLAB_DETAIL_SELECT_SLABMASTERID(JSON.stringify(inputObj));
        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {

            setSeverity('error');
            setIsError(true);
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);

        setSlabCommission(data);
    };
    const handleSlabChange = (value) => {
        const selectedSlabID = value;

        SelectBySlabMasterID1(selectedSlabID);
    };

    const submithandle = async (values) => {

        const inputObj = {
            // OnboardingUserID: onboardingUserID,
            FullName: values.FullName,
            FirmName: values.FirmName,
            DOB: values.DOB,
            PanNo: values.PanNo,
            Residential_Address: values.Residential_Address,
            Gender: values.Gender,
            GstNo: values.GstNo,
            PinCode: values.PinCode,
            Address: values.Address,
            Country: values.Country,
            EmailID: values.EmailID,
            SlabID: values.SlabID,
            MobileNumber: values.MobileNumber,
            AccountNumber: values.AccountNumber,
            IFSC: values.IFSC,
            BankName: values.BankName,
            Business_Address: values.Business_Address,
            Residential_Locality: values.Residential_Locality,
            State: values.State,
            Business_Locality: values.Business_Locality,


            SelfieImage: values.Selfie,
            PANImage: values.PAN,
            AadhaarImage: values.Aadhaar,
            GSTImage: values.GST,
            Establishment1Image: values.Establishment1,
            Establishment2Image: values.Establishment2,
            Shop_images1Image: values.Shop_images1,
            Shop_images2Image: values.Shop_images2

        }
        const response = await INSERT(JSON.stringify(inputObj));
        setLoading(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {

            setSeverity('error');
            setIsError(true);
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);
        history.push('/ViewAgentOnboarding');
        setSeverity('success');
        setResponseMessage(data);
        setIsError(true);

    }
    const selectUserTypes = async () => {
        const response = await USERTYPE_SELECT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setUserTypeList(data);
    };
    return (

        <TabContext value={tabIndex}>
            <AppBar
                position="static"
                color="default"
                className={classesInner.appBarHeight}
            >
                <TabList onChange={handleChange} aria-label="recharge tabs">
                    <Tab
                        label="Personal Details"
                        value="1"
                        className={classesInner.Tab}
                    />
                    <Tab
                        label="Business Details"
                        value="2"
                        className={classesInner.Tab}
                    />
                    <Tab
                        label="Documents Upload"
                        value="3"
                        className={classesInner.Tab}
                    />
                    <Tab
                        label="Bank Details"
                        value="4"
                        className={classesInner.Tab}
                    />
                    <Tab
                        label="Commission Profile"
                        value="5"
                        className={classesInner.Tab}
                    />
                </TabList>
            </AppBar>
            <Paper className={classes.PaperStyle}>
                <CustomContainer
                    isLoading={loading}
                    isError={isError}
                    alertMessage={responseMessage}
                    severity={severity}
                // onCloseCallback={(alertState) => {
                //     setIsError(alertState);
                // }}
                >
                    <Formik
                        enableReinitialize={true}
                        initialValues={UserObj}

                        resetForm={UserObj}
                        onSubmit={submithandle}
                    // resetForm={ModelObj}
                    >

                        {({ setFieldValue, handleSubmit }) => {
                            return (
                                <Form>
                                    <TabPanel value="1" className={classesInner.marginTab}>
                                        <Grid item xs={12} sm={12} md={12} lg={12} >
                                            <Grid container spacing={2}>
                                                <Grid item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}>

                                                    <MaterialSelect
                                                        label="Role"
                                                        name="UserTypeID"

                                                        title="Role"
                                                    >
                                                        <MenuItem key="0" value="0">
                                                            Select Role
                                                        </MenuItem>
                                                        {userTypeList.map((item) => (
                                                            <MenuItem
                                                                key={item.UserTypeID}
                                                                value={item.UserTypeID}
                                                            >
                                                                {item.UserType}
                                                            </MenuItem>
                                                        ))}
                                                    </MaterialSelect>
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Full Name"
                                                        name="FullName"
                                                        placeholder="Full Name"

                                                    />

                                                </Grid>


                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Gender"
                                                        name="Gender"
                                                        placeholder="Gender "
                                                        // readOnly={true}
                                                    />

                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialDatePickerField
                                                        name="DOB"
                                                        label="Birth Date"
                                                        //  readOnly={true}
                                                    />

                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Email ID"
                                                        name="EmailID"

                                                    />

                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Aadhaar Number"
                                                        name="AadhaarNumber"


                                                    />

                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="PAN"
                                                        name="PanNo"
                                                        // readOnly={true}
                                                    />

                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Mobile Number"
                                                        name="MobileNumber"
                                                        //readOnly={true}
                                                    />

                                                </Grid>


                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Residential  Address"
                                                        name="Residential_Address"

                                                    />

                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Residential Locality"
                                                        name="Residential_Locality"

                                                    />

                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="State"
                                                        name="State"

                                                    />

                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Country"
                                                        name="Country"
                                                        //readOnly={true}
                                                    />


                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="PinCode"
                                                        name="PinCode"

                                                    />


                                                </Grid>
                                            </Grid>
                                        </Grid>
                                        <br></br>

                                        <Grid item xs={12} sm={12} md={12} lg={12} >
                                            <Grid container spacing={2} style={{ justifyContent: 'end', marginTop: '20px', borderTop: '1px solid #ccc', padding: '20px 0 0 0' }}>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}

                                                >
                                                    <SecondaryButton
                                                        // disabled={
                                                        //     loadingOpen
                                                        // }
                                                        endIcon={
                                                            <ResetIcon />
                                                        }
                                                        title="Back"
                                                        onClick={() => handleChange("", "2")}

                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <PrimaryButton
                                                        title="Proceed"
                                                        // disabled={
                                                        //     loadingOpen
                                                        // }
                                                        onClick={() => handleChange("", "2")}

                                                        endIcon={
                                                            <RechargeIcon />
                                                        }
                                                    />
                                                </Grid>
                                            </Grid>
                                        </Grid>
                                    </TabPanel>
                                    <TabPanel value="2" className={classesInner.marginTab}>
                                        <Grid item xs={12} sm={12} md={12} lg={12} >
                                            <Grid container spacing={2}>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Firm Name"
                                                        name="FirmName"
                                                        placeholder="Firm Name"

                                                    />

                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Business Address"
                                                        name="Business_Address"

                                                    />


                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Business Locality"
                                                        name="Business_Locality"

                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="State"
                                                        name="State"

                                                    />

                                                </Grid>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Country"
                                                        name="Country"
                                                        readOnly={true}
                                                    />


                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="PinCode"
                                                        name="PinCode"

                                                    />


                                                </Grid>
                                            </Grid>
                                        </Grid>
                                        <br></br>

                                        <Grid item xs={12} sm={12} md={12} lg={12} >
                                            <Grid container spacing={2} style={{ justifyContent: 'end', marginTop: '20px', borderTop: '1px solid #ccc', padding: '20px 0 0 0' }}>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <SecondaryButton
                                                        // disabled={
                                                        //     loadingOpen
                                                        // }
                                                        endIcon={
                                                            <ResetIcon />
                                                        }
                                                        title="Back"
                                                        onClick={() => handleChange("", "1")}

                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <PrimaryButton
                                                        title="Proceed"
                                                        // disabled={
                                                        //     loadingOpen
                                                        // }
                                                        onClick={() => handleChange("", "3")}

                                                        endIcon={
                                                            <RechargeIcon />
                                                        }
                                                    />
                                                </Grid>
                                            </Grid>
                                        </Grid>
                                    </TabPanel>

                                    <TabPanel value="3" className={classesInner.marginTab}>
                                        <Grid item xs={12} sm={12} md={12} lg={12} >
                                            <Grid container spacing={2}>
                                                <Grid item xs={12} sm={4}>
                                                    <ImageUploadComponent
                                                        documentType="Selfie"
                                                        setFieldValue={setFieldValue}
                                                    />
                                                </Grid>
                                                <Grid item xs={12} sm={4}>
                                                    <ImageUploadComponent
                                                        documentType="PAN"
                                                        setFieldValue={setFieldValue}
                                                    />
                                                </Grid>
                                                <Grid item xs={12} sm={4}>
                                                    <ImageUploadComponent
                                                        documentType="Aadhaar"
                                                        setFieldValue={setFieldValue}
                                                    />
                                                </Grid>
                                                <Grid item xs={12} sm={4}>
                                                    <ImageUploadComponent
                                                        documentType="Shop_images1"
                                                        setFieldValue={setFieldValue}
                                                    />
                                                </Grid>
                                                <Grid item xs={12} sm={4}>
                                                    <ImageUploadComponent
                                                        documentType="Shop_images2"
                                                        setFieldValue={setFieldValue}
                                                    />
                                                </Grid>
                                                <Grid item xs={12} sm={4}>
                                                    <ImageUploadComponent
                                                        documentType="Establishment1"
                                                        setFieldValue={setFieldValue}
                                                    />
                                                </Grid>
                                                <Grid item xs={12} sm={4}>
                                                    <ImageUploadComponent
                                                        documentType="Establishment2"
                                                        setFieldValue={setFieldValue}
                                                    />
                                                </Grid>
                                                <Grid item xs={12} sm={4}>
                                                    <ImageUploadComponent
                                                        documentType="GST"
                                                        setFieldValue={setFieldValue}
                                                    />
                                                </Grid>
                                                <Grid item xs={12} sm={4}>
                                                    <ImageUploadComponent
                                                        documentType="COR"
                                                        setFieldValue={setFieldValue}
                                                    />
                                                </Grid>

                                                <Grid container spacing={2} style={{ margin: '20px 0 0 0', justifyContent: 'end', borderTop: '1px solid #ccc', padding: '20px 0 0 0' }}>
                                                    <Grid item
                                                        xs={12}
                                                        sm={12}
                                                        md={3}
                                                        lg={3}>
                                                        <SecondaryButton
                                                            // disabled={
                                                            //     loadingOpen
                                                            // }
                                                            endIcon={
                                                                <ResetIcon />
                                                            }
                                                            title="Back"
                                                            onClick={() => handleChange("", "2")}
                                                        />
                                                    </Grid>
                                                    <Grid item
                                                        xs={12}
                                                        sm={12}
                                                        md={3}
                                                        lg={3}>
                                                        <PrimaryButton
                                                            title="Proceed"
                                                            // disabled={
                                                            //     loadingOpen
                                                            // }
                                                            onClick={() => handleChange("", "4")}

                                                            endIcon={
                                                                <RechargeIcon />
                                                            }
                                                        />
                                                    </Grid>
                                                </Grid>
                                            </Grid>
                                        </Grid>
                                    </TabPanel>
                                    <TabPanel value="4" className={classesInner.marginTab}>
                                        <Grid item xs={12} sm={12} md={12} lg={12} >
                                            <Grid container spacing={2}>

                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="IFSC"
                                                        name="IFSC"

                                                        placeholder="IFSC"

                                                    />

                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={4}
                                                    lg={4}
                                                >
                                                    <MaterialTextField
                                                        type={'text'}
                                                        label="Account Number"
                                                        name="AccountNumber"

                                                        placeholder="Account Number"

                                                    />

                                                </Grid>
                                                <Grid container spacing={2} style={{ margin: '20px 0 0 0', justifyContent: 'end', borderTop: '1px solid #ccc', padding: '20px 0 0 0' }}>


                                                    <Grid item
                                                        xs={12}
                                                        sm={12}
                                                        md={3}
                                                        lg={3}>
                                                        <SecondaryButton
                                                            // disabled={
                                                            //     loadingOpen
                                                            // }
                                                            endIcon={
                                                                <ResetIcon />
                                                            }
                                                            title="Back"
                                                            onClick={() => handleChange("", "3")}

                                                        />
                                                    </Grid>
                                                    <Grid item
                                                        xs={12}
                                                        sm={12}
                                                        md={3}
                                                        lg={3}>
                                                        <PrimaryButton
                                                            title="Proceed"
                                                            // disabled={
                                                            //     loadingOpen
                                                            // }
                                                            onClick={() => handleChange("", "5")}

                                                            endIcon={
                                                                <RechargeIcon />
                                                            }
                                                        />
                                                    </Grid>
                                                </Grid>
                                            </Grid>
                                        </Grid>
                                    </TabPanel>
                                    <TabPanel value="5" className={classesInner.marginTab}>
                                        <Grid item xs={12} sm={12} md={12} lg={12} >
                                            <Grid container spacing={2}>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <MaterialSelect
                                                        label="Slab"
                                                        name="SlabID"
                                                        onCustomCallback={(name, value) => handleSlabChange(value)}

                                                    >
                                                        <MenuItem key="0" value="0">
                                                            Select Slab
                                                        </MenuItem>
                                                        {Slabs.map((Item) => (
                                                            <MenuItem
                                                                key={Item.SlabID}
                                                                value={Item.SlabID}
                                                            >
                                                                {Item.Name}
                                                            </MenuItem>
                                                        ))}
                                                    </MaterialSelect>
                                                </Grid>

                                                <Grid container spacing={2} style={{ margin: '20px 0 0 0', justifyContent: 'end', borderTop: '1px solid #ccc', padding: '20px 0 0 0' }}>

                                                    <Grid item
                                                        xs={12}
                                                        sm={12}
                                                        md={3}
                                                        lg={3}>
                                                        <SecondaryButton
                                                            // disabled={
                                                            //     loadingOpen
                                                            // }
                                                            endIcon={
                                                                <ResetIcon />
                                                            }
                                                            title="Back"
                                                            onClick={() => handleChange("", "4")}

                                                        />
                                                    </Grid>
                                                    <Grid item
                                                        xs={12}
                                                        sm={12}
                                                        md={3}
                                                        lg={3}>
                                                        <PrimaryButton
                                                            title="Proceed"
                                                            type="submit"
                                                            // disabled={loadingOpen}
                                                            endIcon={
                                                                <RechargeIcon />
                                                            }
                                                        />
                                                    </Grid>

                                                </Grid>
                                            </Grid>
                                        </Grid>

                                        <Grid>
                                            {/* <SlabCommission SlabDetails={isSlabCommission} /> */}
                                        </Grid>

                                    </TabPanel>
                                </Form>
                            );
                        }}

                    </Formik>
                </CustomContainer>
            </Paper>

        </TabContext>

    );
};

