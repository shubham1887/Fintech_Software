import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import TabList from '@material-ui/lab/TabList';
import TabContext from '@material-ui/lab/TabContext';
import TabPanel from '@material-ui/lab/TabPanel';
import { makeStyles } from '@material-ui/core/styles';
import ImageUploadComponent from 'Components/ImageUploadComponent';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { Grid, Paper, AppBar, Tab, MenuItem, Container, Button, CircularProgress, Divider } from '@material-ui/core';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { PrimaryButton, SecondaryButton } from 'Components/Inputes/CustomButton';
import ResetIcon from 'Components/SvgButtons/ResetIcon';
import RechargeIcon from 'Components/SvgButtons/RechargeIcon';
import { SelectByID as SELECT_BY_ID } from 'Components/Api/OnboardingCallbackAPI';
import { LOGOUT } from 'ConstComponents';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import SlabCommission from './SlabCommisuionDetails';
import { Select as SLAB_SELECT } from 'Components/Api/SlabApi';
import { SelectBySlabMasterID as SLAB_DETAIL_SELECT_SLABMASTERID } from 'Components/Api/SlabDetailApi';
import { useHistory } from 'react-router';
import { Update as UPDATE } from 'Components/Api/OnboardingCallbackAPI';


import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import ImageNotFound from 'Assets/Images/ImageNotFound.png';
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
        overflowWrap: 'normal',
        whiteSpace: 'nowrap',
        textOverflow: 'ellipsis',
        overflow: 'hidden',         
        paddingLeft: '6%',
        paddingRight: '6%',
    }

}));

export const UpdateAgent = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const classesInner = useStyles();
    const history = useHistory();
    const [tabIndex, setTabIndex] = useState('1');
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [documentImage, setDocumentImage] = useState('');
    const [Slabs, setSlabs] = useState([]);
    const [severity, setSeverity] = useState('success');
    const [isOpen, setOpen] = useState(false);
    const [isGstNo, setGstNo] = useState(false);
    const [isVerify, setVerify] = useState(false);
    const [selectedImage, setSelectedImage] = useState("");
    const [isSlabCommission, setSlabCommission] = useState([]);
    const [isDocumentList, setDocumentList] = useState({});
    const [verifiedPage, setVerifiedPage] = useState(false);
    const [selectedSlabID, setSelectedSlabID] = useState(1);
    useEffect(() => {
        SelectByID();
        selectSlabs();


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
        AadhaarNo: "",
        Residential_Address: "",
        Business_Address: "",
        Residential_Locality: "",

        GstNo: "",
        SlabID: "",
        Residential_State: "",
        Business_Locality: "",
        Country: "",
        PinCode: "",
        SlabID: "",
        Business_State: "",
        Business_Pincode: "",
        BeneficiaryName: "",
        Business_District: "",
        Address_Proof_Document_Number: "",
        Address_Proof: "",
        Residential_District: "",
        Referral_Code: "",
        EmployeeID: "",
        LineOfBusiness: "",
        Type_of_Shop: "",
        ReferralDetails: "",
        Employee_Details: "",
        GST_Address: "",
        Category: "",
        Occupation: "",
        Working_Days: "",
        Operating_Time: "",
        Latitude: "",
        Longitude: "",
        SelfieImage: "",
        PANImage: "",
        AadhaarImage: "",
        GSTImage: "",
        Establishment1Image: "",
        Establishment2Image: "",
        CVR_Report1: "",
        CVR_Report2: "",
    });

    const selectSlabs = async () => {
        const response = await SLAB_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);

        if (formRef.current) {
            formRef.current.values.SlabID = data[0].SlabID;
        }
        setSlabs(data);
    };
    const SelectByID = async () => {
        let URL = window.location.pathname;
        var res = URL.split('/');

        let onboardingUserID = res[res.length - 1];

        let JSONObj = {
            OnboardingUserID: onboardingUserID
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
            BeneficiaryName: data.BeneficiaryName,
            Gender: data.Gender,
            PinCode: data.PinCode,
            BankName: data.BankName,
            Residential_Address: data.Residential_Address,
            Business_Address: data.Business_Address,
            GstNo: data.GSTNo,
            SlabID: data.SlabID,
            Residential_Locality: data.Residential_Locality,
            Residential_State: data.Residential_State,
            Business_Locality: data.Business_Locality,
            Country: data.Country,
            PinCode: data.PinCode,
            AadhaarNo: data.AadhaarNo,
            Business_State: data.Business_State,
            Business_Pincode: data.Business_Pincode,
            Business_District: data.Business_District,
            Address_Proof_Document_Number: data.Address_Proof_Document_Number,
            Address_Proof: data.Address_Proof,
            Trade_name: data.Trade_name,
            Trade_pan: data.Trade_pan,
            Residential_District: data.Residential_District,
            Referral_Code: data.Referral_Code,
            EmployeeID: data.EmployeeID,
            LineOfBusiness: data.LineOfBusiness,
            Type_of_Shop: data.Type_of_Shop,
            ReferralDetails: data.ReferralDetails,
            GST_Address: data.GST_Address,
            Employee_Details: data.Employee_Details,
            Latitude: data.Latitude,
            Longitude: data.Longitude,
            Category: data.Category,
            Occupation: data.Occupation,
            Working_Days: data.Working_Days,
            Operating_Time: data.Operating_Time,
            IIBF_Certification_Number: data.IIBF_Certification_Number,
            IIBF_Certification: data.IIBF_Certification,


        });
        setVerifiedPage(data.VerifiedPage)
        setDocumentList(data.list);

        if (data.GSTNo !== "") {
            setGstNo(true);
        }
        if (data.Account_exists.toLowerCase() === "yes") {
            setVerify(true);
        }
    }

    const SelectBySlabMasterID = async (values) => {

        setSlabCommission([]);
        if (values === "" || values === 0) {
            return;
        }
        setLoading(true);
        const inputObj = {
            SlabMasterID: values
        }
        const response = await SLAB_DETAIL_SELECT_SLABMASTERID(JSON.stringify(inputObj));
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

        setSlabCommission(data);
    };
    const handleSlabChange = (value) => {
        const selectedSlabID = value;

        SelectBySlabMasterID(selectedSlabID);
    };

    const submithandle = async (values) => {
        let URL = window.location.pathname;
        var res = URL.split('/');
        let onboardingUserID = res[res.length - 1];
        const inputObj = {
            OnboardingUserID: onboardingUserID,
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
            BeneficiaryName: values.BeneficiaryName,
            BankName: values.BankName,
            Business_Address: values.Business_Address,
            AadhaarNo: values.AadhaarNo,
            Residential_District: values.Residential_District,
            Residential_Locality: values.Residential_Locality,
            Residential_State: values.Residential_State,
            Business_Locality: values.Business_Locality,
            Business_District: values.Business_District,
            Business_State: values.Business_State,
            Business_Pincode: values.Business_Pincode,
            SelfieImage: values.Selfie,
            PANImage: values.PAN,
            AadhaarImage: values.Aadhaar,
            GSTImage: values.GST,
            Establishment1Image: values.Establishment1,
            Establishment2Image: values.Establishment2,
            CVR_Report1: values.CVR_Report1,
            CVR_Report2: values.CVR_Report2,
            Referral_Code: values.Referral_Code,
            EmployeeID: values.EmployeeID,
            LineOfBusiness: values.LineOfBusiness,
            Type_of_Shop: values.Type_of_Shop,
            ReferralDetails: values.ReferralDetails,
            GST_Address: values.GST_Address,
            Employee_Details: values.Employee_Details,
            Latitude: values.Latitude,
            Longitude: values.Longitude,
            Category: values.Category,
            Occupation: values.Occupation,
            Working_Days: values.Working_Days,
            Operating_Time: values.Operating_Time,
            IIBF_Certification_Number: values.IIBF_Certification_Number,
            IIBF_Certification: values.IIBF_Certification

        }
        const response = await UPDATE(JSON.stringify(inputObj));
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setIsError(true);
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);
        setSeverity('success');
        setResponseMessage(data);
        setIsError(true);
        history.push('/ViewAgentOnboarding');
    }
    const handleClickOpen = (image) => {
        setSelectedImage(image);
        setOpen(true);
    };

    const [isTabEnabled, setIsTabEnabled] = useState({ "1": true, "2": false, "3": false, "4": false, "5": false, "6": false });

    const handleChange = (event, newValue) => {
        if (isTabEnabled[newValue]) {
            setTabIndex(newValue);
        }
    };

    const handleProceed = (nextTab) => {
        const currentTab = tabIndex;
        if (true) {
            setIsTabEnabled((prev) => ({ ...prev, [nextTab]: true }));
            setTabIndex(nextTab);
        }
    };

    return (

        <TabContext value={tabIndex}>
            <AppBar
                position="static"
                color="default"
                className={classesInner.appBarHeight}
            >
                <TabList onChange={handleChange} aria-label="recharge tabs">
                    <Tab label="Personal Details" value="1" className={classesInner.Tab} />
                    <Tab label="Business Details" value="2" className={isTabEnabled["2"] ? classesInner.Tab : `${classesInner.Tab} disabled`} />
                    <Tab label="Bank Details" value="3" className={isTabEnabled["3"] ? classesInner.Tab : `${classesInner.Tab} disabled`} />
                    <Tab label="Commission Profile" value="4" className={isTabEnabled["4"] ? classesInner.Tab : `${classesInner.Tab} disabled`} />
                    <Tab label="Documents Upload" value="5" className={isTabEnabled["5"] ? classesInner.Tab : `${classesInner.Tab} disabled`} />
                    <Tab label="Additional Details" value="6" className={isTabEnabled["6"] ? classesInner.Tab : `${classesInner.Tab} disabled`} />
                    <Tab label="Client Visit" value="7" className={isTabEnabled["7"] ? classesInner.Tab : `${classesInner.Tab} disabled`} />

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
                        innerRef={formRef}
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
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12} md={8} lg={8} >
                                                <Grid container spacing={2}>
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            type={'text'}
                                                            label="Referral code/Distributor ID"
                                                            name="Referral_Code"


                                                        />
                                                        <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                                            {UserObj.ReferralDetails}
                                                        </span>

                                                    </Grid>


                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            type={'text'}
                                                            label="EmployeeID"
                                                            name="EmployeeID"
                                                            placeholder="EmployeeID"
                                                        // readOnly={true}
                                                        />
                                                        <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                                            {UserObj.Employee_Details}
                                                        </span>

                                                    </Grid>
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={6}
                                                        lg={6}
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
                                                        md={6}
                                                        lg={6}
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
                                                        md={6}
                                                        lg={6}
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
                                                        md={6}
                                                        lg={6}
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
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            type={'text'}
                                                            label="Aadhaar Number"
                                                            name="AadhaarNo"


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
                                                        md={6}
                                                        lg={6}
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
                                                        md={6}
                                                        lg={6}
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
                                                        md={6}
                                                        lg={6}
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
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            type={'text'}
                                                            label="Residential District"
                                                            name="Residential_District"

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
                                                            type={'text'}
                                                            label="State"
                                                            name="Residential_State"

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
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            type={'text'}
                                                            label="PinCode"
                                                            name="PinCode"

                                                        />
                                                    </Grid>
                                                </Grid>
                                            </Grid>
                                            <Grid item xs={12} sm={12} md={4} lg={4}  >

                                                {Array.isArray(isDocumentList) && isDocumentList.length > 0 ? (
                                                    isDocumentList
                                                        .filter(data =>
                                                            ['aadhaar', 'selfie', 'pan'].includes(data.Ref_ID.toLowerCase())
                                                        )
                                                        .map((data) => (
                                                            <Grid
                                                                spacing={5}
                                                                container
                                                                style={{
                                                                    justifyContent: 'center'
                                                                }}
                                                            >
                                                                <Grid
                                                                    item
                                                                    xs={12}
                                                                    sm={12}
                                                                    md={4}
                                                                    lg={4}
                                                                    key={data.Ref_ID}
                                                                >
                                                                    <Grid container spacing={2}>
                                                                        {data.NewImage !== "" ? (
                                                                            <>
                                                                                <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                                                                                    <img src={data.NewImage}
                                                                                        style={{ width: '100%', height: '100%' }}
                                                                                        onClick={() => handleClickOpen(data.NewImage)} />
                                                                                </Grid>
                                                                                <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                                                                                    <h1 style={{ fontSize: '18px' }}>{data.Ref_ID}</h1>
                                                                                </Grid></>
                                                                        ) : (
                                                                            <img src={ImageNotFound} width='250px' height='250px' />
                                                                        )}

                                                                    </Grid>
                                                                </Grid>
                                                            </Grid>

                                                        ))
                                                ) : (
                                                    <div>No documents found.</div>
                                                )}

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
                                                        onClick={() => handleProceed("2")}

                                                        endIcon={
                                                            <RechargeIcon />
                                                        }
                                                    />
                                                </Grid>
                                            </Grid>
                                        </Grid>
                                    </TabPanel>
                                    <TabPanel value="2" className={classesInner.marginTab}>
                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12} md={8} lg={8} >
                                                <Grid container spacing={2}>

                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={6}
                                                        lg={6}
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
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            type={'text'}
                                                            label="Line Of Business"
                                                            name="LineOfBusiness"
                                                            placeholder="LineOfBusiness"

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
                                                            type={'text'}
                                                            label="Type of Shop"
                                                            name="Type_of_Shop"
                                                            placeholder="Type_of_Shop"

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
                                                            type={'text'}
                                                            label="Business Address"
                                                            name="Business_Address"

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
                                                            type={'text'}
                                                            label="Business District"
                                                            name="Business_District"

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
                                                            type={'text'}
                                                            label="Business Locality"
                                                            name="Business_Locality"

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
                                                            type={'text'}
                                                            label="Business State"
                                                            name="Business_State"

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
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            type={'text'}
                                                            label="Pin Code"
                                                            name="Business_Pincode"
                                                        />


                                                    </Grid>
                                                    {isGstNo === false && (
                                                        <><Grid
                                                            item
                                                            xs={12}
                                                            sm={12}
                                                            md={6}
                                                            lg={6}
                                                        >
                                                            <MaterialTextField
                                                                type={'text'}
                                                                label="Address Proof"
                                                                name="Address_Proof" />
                                                        </Grid><Grid
                                                            item
                                                            xs={12}
                                                            sm={12}
                                                            md={6}
                                                            lg={6}
                                                        >
                                                                <MaterialTextField
                                                                    type={'text'}
                                                                    label="Document Number"
                                                                    name="Address_Proof_Document_Number" />
                                                            </Grid></>
                                                    )}
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            type={'text'}
                                                            label="GST"
                                                            name="GstNo" />
                                                    </Grid>
                                                    {isGstNo === true && (
                                                        <>
                                                            <Grid
                                                                item
                                                                xs={12}
                                                                sm={12}
                                                                md={6}
                                                                lg={6}
                                                            >
                                                                <MaterialTextField
                                                                    type={'text'}
                                                                    label="Trade name"
                                                                    name="Trade_name" />
                                                            </Grid><Grid
                                                                item
                                                                xs={12}
                                                                sm={12}
                                                                md={6}
                                                                lg={6}
                                                            >
                                                                <MaterialTextField
                                                                    type={'text'}
                                                                    label="Trade PAN"
                                                                    name="Trade_pan" />
                                                            </Grid>
                                                            <Grid
                                                                item
                                                                xs={12}
                                                                sm={12}
                                                                md={12}
                                                                lg={12}
                                                            >
                                                                <MaterialTextField
                                                                    type={'text'}
                                                                    label="GST Address"
                                                                    name="GST_Address"

                                                                />
                                                            </Grid>
                                                        </>
                                                    )}
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            type={'text'}
                                                            label="Latitude"
                                                            name="Latitude" />
                                                    </Grid>
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            type={'text'}
                                                            label="Longitude"
                                                            name="Longitude" />
                                                    </Grid>
                                                </Grid>
                                            </Grid>
                                            <Grid item xs={12} sm={12} md={4} lg={4}  >

                                                {Array.isArray(isDocumentList) && isDocumentList.length > 0 ? (
                                                    isDocumentList
                                                        .filter(data =>
                                                            ['shop_images1', 'shop_images2', 'gst', 'establishment1', 'establishment2'].includes(data.Ref_ID.toLowerCase())
                                                        )
                                                        .map((data) => (
                                                            <Grid
                                                                spacing={5}
                                                                container
                                                                style={{
                                                                    justifyContent: 'center'
                                                                }}
                                                            >
                                                                <Grid
                                                                    item
                                                                    xs={12}
                                                                    sm={12}
                                                                    md={4}
                                                                    lg={4}
                                                                    key={data.Ref_ID}
                                                                >
                                                                    <Grid container spacing={2}>
                                                                        {data.NewImage !== "" ? (
                                                                            <>
                                                                                <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                                                                                    <img src={data.NewImage}
                                                                                        style={{ width: '100%', height: '100%' }}
                                                                                        onClick={() => handleClickOpen(data.NewImage)} />
                                                                                </Grid>
                                                                                <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                                                                                    <h1 style={{ fontSize: '18px' }}>{data.Ref_ID}</h1>
                                                                                </Grid></>
                                                                        ) : (
                                                                            <img src={ImageNotFound} width='250px' height='250px' />
                                                                        )}

                                                                    </Grid>
                                                                </Grid>
                                                            </Grid>

                                                        ))
                                                ) : (
                                                    <div>No documents found.</div>
                                                )}

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
                                                            onClick={() => handleProceed("3")}

                                                            endIcon={
                                                                <RechargeIcon />
                                                            }
                                                        />
                                                    </Grid>
                                                </Grid>
                                            </Grid>
                                        </Grid>
                                    </TabPanel>
                                    <TabPanel value="3" className={classesInner.marginTab}>

                                        <Grid container spacing={2}>
                                            <Grid item xs={12} sm={12} md={8} lg={8} >
                                                <Grid container spacing={2}>
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={6}
                                                        lg={6}
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
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            type={'text'}
                                                            label="Account Number"
                                                            name="AccountNumber"

                                                            placeholder="Account Number"

                                                        />

                                                    </Grid>
                                                    {isVerify && (
                                                        <Grid
                                                            item
                                                            xs={12}
                                                            sm={12}
                                                            md={6}
                                                            lg={6}
                                                        >
                                                            <MaterialTextField
                                                                type={'text'}
                                                                label="Beneficiary Name"
                                                                name="BeneficiaryName"

                                                                placeholder="BeneficiaryName"

                                                            />

                                                        </Grid>

                                                    )}

                                                </Grid>
                                            </Grid>
                                            <Grid item xs={12} sm={12} md={4} lg={4}  >

                                                {Array.isArray(isDocumentList) && isDocumentList.length > 0 ? (
                                                    isDocumentList
                                                        .filter(data =>
                                                            ['cheque'].includes(data.Ref_ID.toLowerCase())
                                                        )
                                                        .map((data) => (
                                                            <Grid
                                                                spacing={5}
                                                                container
                                                                style={{
                                                                    justifyContent: 'center'
                                                                }}
                                                            >
                                                                <Grid
                                                                    item
                                                                    xs={12}
                                                                    sm={12}
                                                                    md={4}
                                                                    lg={4}
                                                                    key={data.Ref_ID}
                                                                >
                                                                    <Grid container spacing={2}>
                                                                        {data.NewImage !== "" ? (
                                                                            <>
                                                                                <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                                                                                    <img src={data.NewImage}
                                                                                        style={{ width: '100%', height: '100%' }}
                                                                                        onClick={() => handleClickOpen(data.NewImage)} />
                                                                                </Grid>
                                                                                <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                                                                                    <h1 style={{ fontSize: '18px' }}>{data.Ref_ID}</h1>
                                                                                </Grid></>
                                                                        ) : (
                                                                            <img src={ImageNotFound} width='250px' height='250px' />
                                                                        )}

                                                                    </Grid>
                                                                </Grid>
                                                            </Grid>

                                                        ))
                                                ) : (
                                                    <div>No documents found.</div>
                                                )}

                                            </Grid>
                                            <Grid item xs={12} sm={12} md={12} lg={12} >
                                                <Grid container spacing={2} style={{ justifyContent: 'end', marginTop: '20px', borderTop: '1px solid #ccc', padding: '20px 0 0 0' }}>


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
                                                            onClick={() => handleProceed("4")}

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
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <MaterialSelect
                                                        label="Slab"
                                                        name="SlabID"
                                                        //value={selectedSlabID} 
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
                                                            onClick={() => handleProceed("5")}

                                                            endIcon={
                                                                <RechargeIcon />
                                                            }
                                                        />
                                                    </Grid>

                                                </Grid>
                                            </Grid>
                                        </Grid>

                                        <Grid>
                                            <SlabCommission SlabDetails={isSlabCommission} />
                                        </Grid>

                                    </TabPanel>
                                    <TabPanel value="5" className={classesInner.marginTab}>
                                        <Grid item xs={12} sm={12} md={12} lg={12} >
                                            <Grid container spacing={2}>

                                                {Array.isArray(isDocumentList) && isDocumentList.length > 0 ? (

                                                    isDocumentList.map((data) => (

                                                        <><Grid
                                                            item
                                                            xs={12}
                                                            sm={12}
                                                            md={4}
                                                            lg={4}
                                                            key={data.Ref_ID}
                                                            style={{ textAlign: 'center', }}
                                                        >
                                                            <ImageUploadComponent
                                                                documentType={data.Ref_ID}
                                                                DocumentImage={data.NewImage}
                                                                setFieldValue={setFieldValue}
                                                                ID={data.OnboardingUserDocumentID}
                                                            /></Grid></>
                                                    ))
                                                ) : (
                                                    <div>No documents found.</div>
                                                )}

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
                                                            // disabled={
                                                            //     loadingOpen
                                                            // }
                                                            onClick={() => handleProceed("6")}

                                                            endIcon={
                                                                <RechargeIcon />
                                                            }
                                                        />

                                                    </Grid>
                                                </Grid>
                                            </Grid>
                                        </Grid>

                                    </TabPanel>
                                    <TabPanel value="6" className={classesInner.marginTab}>

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
                                                        label="Category"
                                                        name="Category"
                                                        placeholder="Category"

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
                                                        label="Occupation"
                                                        name="Occupation"
                                                        placeholder="Occupation"

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
                                                        label="Working_Days"
                                                        name="Working_Days"
                                                        placeholder="Working_Days"

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
                                                        label="Operating Time"
                                                        name="Operating_Time"
                                                        placeholder="Operating_Time"

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
                                                        label="IIBF Certification"
                                                        name="IIBF_Certification"
                                                        placeholder="IIBF_Certification"

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
                                                        label="IIBF Certification Number"
                                                        name="IIBF_Certification_Number"
                                                        placeholder="IIBF_Certification_Number"

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
                                                        onClick={() => handleChange("", "5")}

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
                                                        onClick={() => handleProceed("7")}

                                                        endIcon={
                                                            <RechargeIcon />
                                                        }
                                                    />
                                                </Grid>

                                            </Grid>
                                        </Grid>

                                    </TabPanel>
                                    <TabPanel value="7" className={classesInner.marginTab}>
                                        <Grid item xs={12} sm={12} md={12} lg={12} >
                                            <Grid container spacing={2}>

                                                {Array.isArray(isDocumentList) && isDocumentList.length > 0 ? (
                                                    isDocumentList.filter(data =>
                                                        ["cor_report1", "cor_report2"].includes(data.Ref_ID.toLowerCase())
                                                    ).length > 0 ? (
                                                            isDocumentList
                                                                .filter(data =>
                                                                    ["cor_report1", "cor_report2"].includes(data.Ref_ID.toLowerCase())
                                                                )
                                                                .map((data) => (
                <Grid
                    item
                    xs={12}
                    sm={12}
                    md={4}
                    lg={4}
                    key={data.Ref_ID}
                    style={{ textAlign: 'center' }}
                >
                    <ImageUploadComponent
                        documentType={data.Ref_ID}
                        DocumentImage={data.NewImage}
                        setFieldValue={setFieldValue}
                        ID={data.OnboardingUserDocumentID}
                    />
                </Grid>
            ))
                                                    ) : (
                                                        <>
                                                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                                                <ImageUploadComponent
                                                                        documentType="CVR_Report1"
                                                                        setFieldValue={setFieldValue}
                                                                    />
                                                                </Grid>
                                                                <Grid item xs={12} sm={12} md={4} lg={4}>
                                                                    <ImageUploadComponent
                                                                        documentType="CVR_Report2"
                                                                    setFieldValue={setFieldValue}
                                                                />
                                                            </Grid>
                                                        </>
                                                    )
                                                ) : null}


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
                                                        onClick={() => handleChange("", "6")}

                                                    />
                                                </Grid>
                                                <Grid item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}>
                                                    <PrimaryButton
                                                        title="Submit"
                                                        type="submit"
                                                        // disabled={loadingOpen}
                                                        endIcon={
                                                            <RechargeIcon />
                                                        }
                                                    />
                                                </Grid>

                                            </Grid>
                                        </Grid>

                                    </TabPanel>
                                </Form>
                            );
                        }}

                    </Formik>
                    {isOpen && (
                        <CustomModalWithScroll
                            isOpen={isOpen}
                            title="image"
                            width="75%"
                            onDismissCallback={(value) => {
                                setOpen(value);
                                // refreshForm();
                            }}
                        >
                            <img src={selectedImage}
                                style={{ width: '80%' }}
                            />
                        </CustomModalWithScroll>
                    )}
                </CustomContainer>
            </Paper>

        </TabContext>

    );
};

