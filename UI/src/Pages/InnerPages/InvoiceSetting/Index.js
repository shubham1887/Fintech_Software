import React, { memo, useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    MenuItem,
    Grid,
    Box,
    Button,
    Paper,
    CircularProgress,
    TextField
} from '@material-ui/core';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { Select as STATE_SELECT } from 'Components/Api/StatesApi';

import { MaterialTextArea } from 'Components/Inputes/FormikMaterialTextArea';
import { LOGOUT } from 'ConstComponents';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import {
    Select as SELECT_INVOICE_SETTINGS,
    Update as UPDATE_INVOICE_SETTINGS
} from 'Components/Api/InvoiceSettingAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const Index = memo(() => {
    const classes = commonStyle();
    const [Image, setImage] = useState('');
    const [Image1, setImage1] = useState('');

    const [SignatureSourceImage, setSignatureImage] = useState('');

    const [IsUpdate, setIsUpdate] = useState(false);
    const [stateList, setStateList] = useState([]);

    const [CustomContainerProps, setCustomContainerProps] = useState({
        loadingOpen: false,
        isError: false,
        severity: 'success',
        responseMessage: ''
    });

    const [ImageDimension] = useState({
        width: 80,
        height: 60
    });
    const [SignatureImageDimension] = useState({
        width: 80,
        height: 60
    });
    const [FormObj, setFormObj] = useState({
        InvoiceSettingID: 0,
        CompanyName: '',
        LogoURL: '',
        SignatureURL: '',
        CompanyPrefix: '',
        ShortAddress: '',
        FullAddress: '',
        PinCode: '',
        GSTIN: '',
        PAN: '',
        CIN: '',
        District: '',
        State: '',
        Country: '',
        AccountNo: '',
        AccountType: '',
        AccountHolderName: '',
        BankName: '',
        BranchName: '',
        IFSC: '',
        Email: '',
        WebsiteName: '',
        Note: '',
        IsReverseChargeApplicable: false,
        LogoImage: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        },

        SignatureImage: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        }
    });

    const FormValidationSchema = Yup.object({
        CompanyPrefix: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' company prefix'
        ),
        ShortAddress: Yup.string().required(PLEASE_PROVIDE_VALID + ' address'),
        FullAddress: Yup.string().required(PLEASE_PROVIDE_VALID + ' address'),
        PinCode: Yup.string().required(PLEASE_PROVIDE_VALID + ' pincode'),
        GSTIN: Yup.string().required(PLEASE_PROVIDE_VALID + ' GSTIN'),
        PAN: Yup.string().required(PLEASE_PROVIDE_VALID + ' PAN'),
        CIN: Yup.string().required(PLEASE_PROVIDE_VALID + ' CIN'),
        State: Yup.string().required(PLEASE_PROVIDE_VALID + ' state'),
        AccountNo: Yup.string().required(PLEASE_PROVIDE_VALID + ' account no'),
        AccountType: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' account type'
        ),
        AccountHolderName: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' account holder name'
        ),
        BankName: Yup.string().required(PLEASE_PROVIDE_VALID + ' bank name'),
        BranchName: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' branch name'
        ),
        Email: Yup.string().required(PLEASE_PROVIDE_VALID + ' email'),
        WebsiteName: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' website name'
        ),
        Note: Yup.string().required(PLEASE_PROVIDE_VALID + 'note')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);

        setCustomContainerProps({
            isError: false,
            loading: true
        });

        const { isAuthorized, status, data } = await UPDATE_INVOICE_SETTINGS(
            requestObj
        );

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setCustomContainerProps({
                isError: true,
                loading: false,
                severity: 'error',
                responseMessage: data
            });
            return;
        }
        setCustomContainerProps({
            isError: true,
            loading: false,
            severity: 'success',
            responseMessage: data
        });
        resetForm(FormObj);
        setIsUpdate(false);

        setFormObj({
            ...FormObj,
            InvoiceSettingID: 0,
            CompanyName: '',
            LogoURL: '',
            CompanyPrefix: '',
            ShortAddress: '',
            FullAddress: '',
            PinCode: '',
            GSTIN: '',
            PAN: '',
            CIN: '',
            District: '',
            State: '',
            Country: '',
            AccountNo: '',
            AccountType: '',
            AccountHolderName: '',
            BankName: '',
            BranchName: '',
            IFSC: '',
            Email: '',
            WebsiteName: '',
            Note: '',
            IsReverseChargeApplicable: false,
            LogoImage: {
                Name: '',
                Size: '',
                Type: '',
                Data: ''
            },

            SignatureImage: {
                Name: '',
                Size: '',
                Type: '',
                Data: ''
            }
        });

        selectData();
    };

    useEffect(() => {
        selectData();
        selectStates();
    }, []);

    const selectStates = async () => {
        const response = await STATE_SELECT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setCustomContainerProps({
                isError: true,
                severity: 'error',
                responseMessage: response.data
            });
        }

        var data = JSON.parse(response.data);
        setStateList(data);
    };

    const selectData = async () => {
        setCustomContainerProps({
            isError: false,
            loading: true
        });

        const response = await SELECT_INVOICE_SETTINGS();

        if (!response.isAuthorized) LOGOUT();

        setCustomContainerProps({
            isError: false,
            loading: false
        });

        if (!response.status) {
            setCustomContainerProps({
                isError: true,
                severity: 'error',
                responseMessage: response.data
            });
            return;
        }

        let d = JSON.parse(response.data);

        setIsUpdate(true);
        setFormObj({
            ...FormObj,
            InvoiceSettingID: d.InvoiceSettingID,
            CompanyName: d.CompanyName,
            LogoURL: d.LogoURL,
            CompanyPrefix: d.CompanyPrefix,
            ShortAddress: d.ShortAddress,
            FullAddress: d.FullAddress,
            District: d.District,
            State: d.State,
            Country: d.Country,
            GSTIN: d.GSTIN,
            PAN: d.PAN,
            CIN: d.CIN,
            PinCode: d.PinCode,
            AccountNo: d.AccountNo,
            AccountType: d.AccountType,
            AccountHolderName: d.AccountHolderName,
            BankName: d.BankName,
            BranchName: d.BranchName,
            IFSC: d.IFSC,
            Email: d.Email,
            WebsiteName: d.WebsiteName,
            Note: d.Note,
            SignatureURL: d.SignatureURL
        });

        setImage1(d.SignatureURL);
        setImage(d.LogoURL);
    };

    return (
        <CustomContainer
            isLoading={CustomContainerProps.loading}
            isError={CustomContainerProps.isError}
            alertMessage={CustomContainerProps.responseMessage}
            severity={CustomContainerProps.severity}
            onCloseCallback={(alertState) => {
                setCustomContainerProps({
                    isError: alertState
                });
            }}
        >
            <Grid container spacing={3}>
                <Grid item xs={6}>
                    <h2 className={classes.PaperHead_h2}>Invoice Settings </h2>
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
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="Company Name"
                                                name="CompanyName"
                                                type="text"
                                                length="100"
                                                placeholder="Company Name"
                                            />
                                        </Grid>

                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="CompanyPrefix"
                                                name="CompanyPrefix"
                                                type="text"
                                                length="15"
                                                placeholder="CompanyPrefix"
                                            />
                                        </Grid>

                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialSelect
                                                label="State"
                                                name="State"
                                                title="State"
                                            >
                                                <MenuItem value="0">
                                                    Select State
                                                </MenuItem>
                                                {stateList &&
                                                    stateList.map(
                                                        (
                                                            { StateID, Name },
                                                            index
                                                        ) => (
                                                            <MenuItem
                                                                key={index}
                                                                value={Name}
                                                            >
                                                                {Name}
                                                            </MenuItem>
                                                        )
                                                    )}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="District"
                                                name="District"
                                                length="20"
                                                type="text"
                                                placeholder="District"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="Country"
                                                name="Country"
                                                length="20"
                                                type="text"
                                                placeholder="Country"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="PinCode"
                                                name="PinCode"
                                                length="6"
                                                type="text"
                                                placeholder="PinCode"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="GSTIN"
                                                name="GSTIN"
                                                length="20"
                                                type="text"
                                                placeholder="GSTIN"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="PAN"
                                                name="PAN"
                                                length="10"
                                                type="text"
                                                placeholder="PAN"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="CIN"
                                                name="CIN"
                                                length="25"
                                                type="text"
                                                placeholder="CIN"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="AccountHolderName"
                                                name="AccountHolderName"
                                                type="text"
                                                length="100"
                                                placeholder="AccountHolderName"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="AccountNo"
                                                name="AccountNo"
                                                length="20"
                                                type="text"
                                                placeholder="AccountNo"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="AccountType"
                                                name="AccountType"
                                                length="20"
                                                type="text"
                                                placeholder="AccountType"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="BankName"
                                                name="BankName"
                                                length="20"
                                                type="text"
                                                placeholder="BankName"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="BranchName"
                                                name="BranchName"
                                                length="20"
                                                type="text"
                                                placeholder="BranchName"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="IFSC"
                                                name="IFSC"
                                                length="11"
                                                type="text"
                                                placeholder="IFSC"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="Email"
                                                length="80"
                                                name="Email"
                                                type="text"
                                                placeholder="Email"
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <MaterialTextField
                                                label="WebsiteName"
                                                length="80"
                                                name="WebsiteName"
                                                type="text"
                                                placeholder="WebsiteName"
                                            />
                                        </Grid>

                                        <Grid item xs={12} sm={9} md={9} lg={9}>
                                            <MaterialTextField
                                                label="Address"
                                                name="ShortAddress"
                                                length="500"
                                                type="text"
                                                placeholder="Address"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <MaterialTextField
                                                label="Full Address"
                                                name="FullAddress"
                                                length="500"
                                                type="text"
                                                placeholder="Address"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <MaterialTextArea
                                                label="Note"
                                                name="Note"
                                                length="1000"
                                                type="text"
                                                placeholder="Note"
                                                rows={5}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <label>Logo Image</label>
                                            <br />
                                            <TextField
                                                id="file"
                                                name="DocumentFile"
                                                type="file"
                                                inputProps={{
                                                    accept: 'image/*'
                                                }}
                                                onChange={(event) => {
                                                    let {
                                                        files
                                                    } = event.target;
                                                    let file = files[0];

                                                    setImage(
                                                        URL.createObjectURL(
                                                            file
                                                        )
                                                    );

                                                    setFieldValue(
                                                        'LogoImage.Name',
                                                        file.name
                                                    );
                                                    setFieldValue(
                                                        'LogoImage.Size',
                                                        file.size
                                                    );
                                                    setFieldValue(
                                                        'LogoImage.Type',
                                                        file.type
                                                    );

                                                    let reader = new FileReader();

                                                    reader.onload = ({
                                                        target: { result }
                                                    }) => {
                                                        setFieldValue(
                                                            'LogoImage.Data',
                                                            result
                                                                .replace(
                                                                    'data:',
                                                                    ''
                                                                )
                                                                .replace(
                                                                    /^.+,/,
                                                                    ''
                                                                )
                                                        );
                                                    };
                                                    reader.readAsDataURL(file);
                                                }}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <img
                                                name="LogoImage.Data"
                                                src={Image}
                                                height={ImageDimension.height}
                                                width={ImageDimension.width}
                                                alt={Image}
                                            />
                                        </Grid>

                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <label>Signature Image</label>
                                            <br />
                                            <TextField
                                                id="file1"
                                                name="DocumentFile"
                                                type="file"
                                                inputProps={{
                                                    accept: 'image/*'
                                                }}
                                                onChange={(event) => {
                                                    let {
                                                        files
                                                    } = event.target;
                                                    let file = files[0];

                                                    setImage1(
                                                        URL.createObjectURL(
                                                            file
                                                        )
                                                    );

                                                    setFieldValue(
                                                        'SignatureImage.Name',
                                                        file.name
                                                    );
                                                    setFieldValue(
                                                        'SignatureImage.Size',
                                                        file.size
                                                    );
                                                    setFieldValue(
                                                        'SignatureImage.Type',
                                                        file.type
                                                    );

                                                    let reader = new FileReader();

                                                    reader.onload = ({
                                                        target: { result }
                                                    }) => {
                                                        setFieldValue(
                                                            'SignatureImage.Data',
                                                            result
                                                                .replace(
                                                                    'data:',
                                                                    ''
                                                                )
                                                                .replace(
                                                                    /^.+,/,
                                                                    ''
                                                                )
                                                        );
                                                    };
                                                    reader.readAsDataURL(file);
                                                }}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={3} md={3} lg={3}>
                                            <img
                                                name="SignatureImage.Data"
                                                src={Image1}
                                                height={
                                                    SignatureImageDimension.height
                                                }
                                                width={
                                                    SignatureImageDimension.width
                                                }
                                                alt={Image1}
                                            />
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title={IsUpdate ? 'Update' : 'Save'}
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={
                                                CustomContainerProps.loading
                                            }
                                        >
                                            {IsUpdate ? 'Update' : 'Save'}
                                        </Button>
                                        {CustomContainerProps.loading && (
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
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
});
