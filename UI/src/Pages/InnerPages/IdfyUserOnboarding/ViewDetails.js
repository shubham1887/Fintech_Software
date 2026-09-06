import React, { useState, useEffect } from 'react';
import { Grid, Box, Dialog, DialogTitle, DialogContent, DialogActions, Button, Typography } from '@material-ui/core';
import { makeStyles } from '@material-ui/styles';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import PdfLogo from 'Assets/Images/PdfImage.png'
import { SelectByID as SELECT_BY_ID } from 'Components/Api/LastChangeUserDetailsAPI';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
const useStyles = makeStyles((theme) => ({

    image: {
        width: '55px',
        height: '55px',
        transition: 'transform 0.3s ease',
        cursor: 'pointer',
    },
    imageExpanded: {
        position: 'relative',
        top: 0,
        left: 0,
        width: '1000px',
        height: '1000px',
        objectFit: 'contain',
        zIndex: -99,
    },
    overlay: {
        position: 'absolute',
        top: 0,
        left: 0,
        width: '100%',
        height: '100%',
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        zIndex: 999,
    },
    dialogImage: {
        width: '100%',
        height: 'auto',
    },
}));

export const ViewDetails = ({ Details }) => {

    const classes = commonStyle();
    const customClasses = useStyles();
    const [openDialog, setOpenDialog] = useState(false);
    const [previewImage, setPreviewImage] = useState("");
    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [IsDetailsList, setDetailsList] = useState([]);

    const [severity, setSeverity] = useState('success');
    const [isError, setIsError] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');

    const handleClose = () => {
        setOpenDialog(false);
    };

    const handleImageClick = (document) => {
        setPreviewImage(document.NewImage);
        setOpenDialog(true);
    };

    useEffect(() => {
        const intervalId = setInterval(() => {
            if (Details.OnboardingUserID) {
                SelectByID();
                createColumns();
                clearInterval(intervalId);
            }
        }, 2000);

        return () => clearInterval(intervalId);
    }, [Details.OnboardingUserID]);
    const SelectByID = async () => {
        let JSONObj = {
            OnboardingUserID: Details.OnboardingUserID

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

        setDetailsList(data);

    }
    const createColumns = () => {
        var columns = [];


        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        columns.push({
            label: 'Change Column',
            name: 'Change'
        });
        columns.push({
            label: 'Old Changes',
            name: 'OldChanges'
        });

        columns.push({
            label: 'New Changes',
            name: 'NewChanges',
            customStatus: true
        });
        setMainColumns(columns);
    };
    return (
        <Box
            style={{
                marginBottom: 16,
                paddingLeft: 5,
            }}
        >

            <Grid container spacing={2} sx={{ flexGrow: 1 }}>
                <Grid item xs={12}>
                    <Typography style={{
                        borderBottom: '1px solid #ccc',
                        fontSize: '16px',
                        fontWeight: '600',
                    }}>Personal Details</Typography>
                </Grid>
                <Grid item xs={12}>
                    <Box style={{ background: '#f1f1f1', padding: '16px', borderRadius: '10px', }}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    ID : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.ID}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Profile ID : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.ProfileID}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    OrderID : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.OrderID}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Employee ID : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.EmployeeID}
                                    </span>
                                </h2>
                            </Grid>
                            {Details.Employee_Details !== "" && (
                                <Grid item xs={12} sm={12} md={4} lg={4}>
                                    <h2 style={{ fontSize: '16px', }}>
                                        Employee Name : {" "}
                                        <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                            {Details.Employee_Details}
                                        </span>
                                    </h2>
                                </Grid>
                            )}
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Referral Code : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Referral_Code}
                                    </span>
                                </h2>
                            </Grid>
                            {Details.ReferralDetails !== "" && (
                                <Grid item xs={12} sm={12} md={4} lg={4}>
                                    <h2 style={{ fontSize: '16px', }}>
                                        Referral code/Distributor ID : {" "}
                                        <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                            {Details.ReferralDetails}
                                        </span>
                                    </h2>
                                </Grid>
                            )}
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Date of birth : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.DOB}
                                    </span>
                                </h2>
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Full Name : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.FullName}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Pan Number : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.PanNo}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Aadhaar Number : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.AadhaarNo}
                                    </span>
                                </h2>
                            </Grid>


                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Email ID : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.EmailID}
                                    </span>
                                </h2>
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Gender : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Gender}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Mobile Number : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.MobileNumber}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Residential  Address : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Residential_Address}
                                    </span>
                                </h2>
                            </Grid>


                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Residential Locality : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Residential_Locality}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Pin Code : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.PinCode}
                                    </span>
                                </h2>
                            </Grid>






                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Country: {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Country}
                                    </span>
                                </h2>
                            </Grid>


                        </Grid>
                    </Box>
                </Grid>

                <Grid item xs={12}>
                    <Typography style={{
                        borderBottom: '1px solid #ccc',
                        fontSize: '16px',
                        fontWeight: '600',
                    }}>Business Details</Typography>
                </Grid>

                <Grid item xs={12}>
                    <Box style={{ background: '#f1f1f1', padding: '16px', borderRadius: '10px', }}>
                        <Grid container spacing={2}>


                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Firm Name : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.FirmName}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Line Of Business : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.LineOfBusiness}
                                    </span>
                                </h2>
                            </Grid>
                            {Details.Type_of_Shop !== "" && (
                                <Grid item xs={12} sm={12} md={4} lg={4}>
                                    <h2 style={{ fontSize: '16px', }}>
                                        Type of Shop : {" "}
                                        <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                            {Details.Type_of_Shop}
                                        </span>
                                    </h2>
                                </Grid>
                            )}
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    GSTNo : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.GSTNo}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Business Address : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Business_Address}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Pin Code : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Business_Pincode}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Business District : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Business_District}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Business Locality : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Business_Locality}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    State: {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Business_State}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Country: {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Country}
                                    </span>
                                </h2>
                            </Grid>
                        </Grid>
                    </Box>
                </Grid>


                <Grid item xs={12}>
                    <Typography style={{
                        borderBottom: '1px solid #ccc',
                        fontSize: '16px',
                        fontWeight: '600',
                    }}>Banking Details</Typography>
                </Grid>

                <Grid item xs={12}>
                    <Box style={{ background: '#f1f1f1', padding: '16px', borderRadius: '10px', }}>
                        <Grid container spacing={2}>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Account Number : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.AccountNumber}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    IFSC : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.IFSC}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Account Holder Name : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.BeneficiaryName}
                                    </span>
                                </h2>
                            </Grid>


                        </Grid>
                    </Box>
                </Grid>
                <Grid item xs={12}>
                    <Typography style={{
                        borderBottom: '1px solid #ccc',
                        fontSize: '16px',
                        fontWeight: '600',
                    }}>Additional Details</Typography>
                </Grid>
                <Grid item xs={12}>
                    <Box style={{ background: '#f1f1f1', padding: '16px', borderRadius: '10px', }}>
                        <Grid container spacing={2}>


                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Category : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Category}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Qualification : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Highest_Qualification}
                                    </span>
                                </h2>
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Occupation : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Occupation}
                                    </span>
                                </h2>
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Operating Time : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Operating_Time}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Working Days : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Working_Days}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Latitude : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Latitude}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Longitude : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.Longitude}
                                    </span>
                                </h2>
                            </Grid> 
                        </Grid>
                    </Box>
                </Grid>

                <Grid item xs={12}>
                    <Typography style={{
                        borderBottom: '1px solid #ccc',
                        fontSize: '16px',
                        fontWeight: '600',
                    }}>Upload Documents</Typography>
                </Grid>

                <Grid item xs={12}>
                    <Box style={{ background: '#f1f1f1', padding: '16px', borderRadius: '10px', }}>
                        <Grid container spacing={2}>
                            {Details.list && Details.list.length > 0 ? (
                                Details.list.map((document, index) => {
                                    const isPDF = document.NewImage?.endsWith('.pdf');
                                    return (
                                        <Grid item xs={12} sm={12} md={4} lg={4} style={{ textAlign: 'center' }} key={index}>
                                            <span style={{ padding: '16px' }}>
                                                {isPDF ? (
                                                    <a href={document.NewImage} target="_blank" rel="noopener noreferrer">
                                                        <img
                                                            src={PdfLogo}
                                                            style={{ width: '260px' }}
                                                        />
                                                    </a>
                                                ) : (
                                                    <img
                                                        src={document.NewImage}
                                                        style={{ width: '260px' }}
                                                            onClick={() => handleImageClick(document)}
                                                        alt={`Document ${index + 1}`}
                                                    />
                                                )}
                                                <Typography>{document.Ref_ID}</Typography>
                                            </span>
                                        </Grid>
                                    );
                                })
                            ) : (
                                <p>No documents available.</p>
                            )}
                        </Grid>

                    </Box>
                </Grid>
                <Grid item xs={12}>
                    <Typography style={{
                        borderBottom: '1px solid #ccc',
                        fontSize: '16px',
                        fontWeight: '600',
                    }}>Remark</Typography>
                </Grid>

                <Grid item xs={12}>
                    <Box style={{ background: '#f1f1f1', padding: '16px', borderRadius: '10px', }}>
                        <Grid container spacing={2}>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Maker Remark : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.SupportRemark}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Maker Status : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.SupportStatus}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Maker User Name : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.SupportUserName}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Checker Remark : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.AdminRemark}
                                    </span>
                                </h2>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <h2 style={{ fontSize: '16px', }}>
                                    Checker Status : {" "}
                                    <span style={{ fontSize: '16px', fontWeight: '600' }}>
                                        {Details.AdminStatus}
                                    </span>
                                </h2>
                            </Grid>


                        </Grid>
                    </Box>
                </Grid>


                <Grid item xs={12}>
                    <Box style={{ background: '#f1f1f1', padding: '16px', borderRadius: '10px', }}>
                        <Grid container spacing={2}>

                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <ReactTable
                                    title={'User Stock'}
                                    data={IsDetailsList}
                                    columns={mainColumns}
                                    headerToolbar={false}
                                    footerToolbar={false}
                                />
                            </Grid>
                        </Grid>
                    </Box>
                </Grid>



                <Dialog open={openDialog} onClose={handleClose} maxWidth="lg" fullWidth>
                    <DialogTitle>Image Preview</DialogTitle>
                    <DialogContent onMouseLeave={handleClose}>
                        <img
                            src={previewImage}
                            className={customClasses.dialogImage}
                            alt="Expanded Document"
                        />
                    </DialogContent>
                    <DialogActions>
                        <Button onClick={handleClose} color="primary">
                            Close
                        </Button>
                    </DialogActions>
                </Dialog>
            </Grid>
        </Box>
    );
};
