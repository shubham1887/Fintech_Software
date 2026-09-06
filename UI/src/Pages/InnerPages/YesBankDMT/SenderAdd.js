import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { LOGOUT } from 'ConstComponents';
import { SenderRegistration as SENDER_ADD } from 'Components/Api/YesBankDMTAPI';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialDatePicker } from 'Components/Inputes/MaterialDatePicker';
import CustomRadioButton from 'Components/Inputes/CustomRadioButton';
import { Container, Grid, Paper, Button, Typography } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import AutoSubmitPincode from 'Components/Helper/AutoSubmitPincode';
import SearchIcon from 'Components/SvgButtons/SearchIcon';
import { FetchDetailsForPincode as FETCH_DETAILS_FOR_PINCODE } from 'Components/Api/UsersApi';
import {
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import { useLocation } from 'react-router-dom';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

const SenderAdd = () => {
    const classes = commonStyle();
    const history = useHistory();
    const location = useLocation();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const Mobile = location?.state?.Mobile;
    const ActiveDMTName = location?.state?.DMTName;
    const [formObject, setFormObject] = useState({
        Mobile: Mobile,
        Name: '',
        Gender: '',
        Address: '',
        CityName: '',
        District: '',
        Pincode: '',
        State: '',
        FromDate: null
    });

    const select = () => {
        setFormObject({
            ...formObject,
            Mobile: Mobile,
            Name: '',
            Gender: '',
            Address: '',
            CityName: '',
            District: '',
            Pincode: '',
            State: ''
        });
    };

    useEffect(() => {
        select();
    }, []);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var request = {
            Mobile: values.Mobile,
            Name: values.Name,
            Address: values.Address,
            CityName: values.CityName,
            State: values.State,
            PinCode: values.Pincode,
            District: values.District,
            Gender: values.Gender,
            FromDate: values.FromDate,
            DMTName: ActiveDMTName
        };
        var requestObj = JSON.stringify(request);

        setLoading(true);
        setIsError(false);
        var response = await SENDER_ADD(requestObj);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            setLoading(false);
            return;
        }
        resetForm(formObject);

        var data = JSON.parse(response.data);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data.Message);

        var param_2 = {
            Mobile: values.Mobile,
            DMTName: ActiveDMTName
        };

        history.push('/YBDMT_SenderSearch', param_2);
    };

    const FetchDetails = async (values, setFieldValue) => {
        var request = {
            Pincode: values.Pincode
        };
        var requestObj = JSON.stringify(request);

        setLoading(true);
        setIsError(false);

        var response = await FETCH_DETAILS_FOR_PINCODE(requestObj);

        if (!response.isAuthorized) LOGOUT();

        setLoading(false);
        setIsError(true);
        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);

        setFieldValue('CityName', data.CityName);
        setFieldValue('District', data.District);
        setFieldValue('State', data.State);
    };

    const FormValidationSchema = Yup.object({
        Mobile: Yup.string().required(PLEASE_PROVIDE_VALID + 'mobile'),
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        Gender: Yup.string().required(PLEASE_PROVIDE_VALID + 'gender'),
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + ' from date')
            .required(PLEASE_SELECT_VALID + ' date')
            .max(new Date(), PLEASE_SELECT_VALID + ' from date')
            .typeError(PLEASE_SELECT_VALID + ' from date'),
        Address: Yup.string().required(PLEASE_PROVIDE_VALID + 'address')
    });

    return (
        <CustomContainer
            isLoading={loading}
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
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
                render={({ formProps, values, setFieldValue }) => {
                    return (
                        <Grid item xs={12} sm={12} md={4} lg={4}>
                            <Form>
                                <AutoSubmitPincode
                                    callback={() => {
                                        FetchDetails(values, setFieldValue);
                                    }}
                                />
                                <h2 className={classes.PaperHead_h2}>
                                    Add Sender
                                </h2>
                                <Grid item xs={12}>
                                    <Paper className={classes.PaperStyle}>
                                        <Grid container spacing={2}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <MaterialNumberField
                                                    label="Mobile Number *"
                                                    name="Mobile"
                                                    type="Mobile"
                                                    length={10}
                                                    readOnly={true}
                                                    placeholder="Mobile Number"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <Typography
                                                    variant="h9"
                                                    component="div"
                                                    gutterBottom
                                                >
                                                    Select Gender *
                                                </Typography>
                                                <CustomRadioButton
                                                    label="Male"
                                                    name="Gender"
                                                    displayValueID="M"
                                                    isSelected={
                                                        values.Gender === 'M'
                                                    }
                                                    onChange={(value) =>
                                                        setFieldValue(
                                                            'Gender',
                                                            value
                                                        )
                                                    }
                                                />
                                                <CustomRadioButton
                                                    label="Female"
                                                    name="Gender"
                                                    displayValueID="F"
                                                    isSelected={
                                                        values.Gender === 'F'
                                                    }
                                                    onChange={(value) =>
                                                        setFieldValue(
                                                            'Gender',
                                                            value
                                                        )
                                                    }
                                                />
                                                <CustomRadioButton
                                                    label="Other"
                                                    name="Gender"
                                                    displayValueID="O"
                                                    isSelected={
                                                        values.Gender === 'O'
                                                    }
                                                    onChange={(value) =>
                                                        setFieldValue(
                                                            'Gender',
                                                            value
                                                        )
                                                    }
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
                                                    label="First Name *"
                                                    name="Name"
                                                    type="text"
                                                    placeholder="First Name"
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
                                                    label="Last Name"
                                                    name="LastName"
                                                    type="text"
                                                    placeholder="Last Name"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <MaterialDatePicker
                                                    name="FromDate"
                                                    label="DOB *"
                                                    maxDate={new Date()}
                                                    value={values.FromDate}
                                                    onChange={(value) =>
                                                        setFieldValue(
                                                            'FromDate',
                                                            value
                                                        )
                                                    }
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={9}
                                                lg={9}
                                            >
                                                <MaterialNumberField
                                                    label="Pin code *"
                                                    name="Pincode"
                                                    type="text"
                                                    length={6}
                                                    title="Pin code"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <Button
                                                    variant="contained"
                                                    color="primary"
                                                    endIcon={<SearchIcon />}
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    onClick={() => {
                                                        FetchDetails(
                                                            values,
                                                            setFieldValue
                                                        );
                                                    }}
                                                    style={{
                                                        marginLeft: '10px'
                                                    }}
                                                ></Button>
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <MaterialTextField
                                                    label="Sender Address *"
                                                    name="Address"
                                                    type="text"
                                                    placeholder="Sender Address"
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
                                                    label="CityName *"
                                                    name="CityName"
                                                    type="text"
                                                    disabled={true}
                                                    readOnly={true}
                                                    placeholder="CityName"
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
                                                    label="District *"
                                                    name="District"
                                                    type="text"
                                                    disabled={true}
                                                    readOnly={true}
                                                    placeholder="District"
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
                                                    label="State *"
                                                    name="State"
                                                    type="text"
                                                    disabled={true}
                                                    readOnly={true}
                                                    placeholder="State"
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
                                                size="medium"
                                                title="Add"
                                                className={
                                                    classes.CustomDMTButtonStyle
                                                }
                                                disabled={loading}
                                            >
                                                Add
                                            </Button>
                                            {loading && (
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
                                                classes.CustomDMTDangerButtonStyle
                                            }
                                        >
                                            Reset
                                        </Button>
                                    </Box>
                                </Grid>
                            </Form>
                        </Grid>
                    );
                }}
            />
        </CustomContainer>
    );
};
export default SenderAdd;
