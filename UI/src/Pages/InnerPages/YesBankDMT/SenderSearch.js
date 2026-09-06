import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import { Box } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { Select as SENDER_SELECT } from 'Components/Api/YesBankDMTAPI';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { Container, Grid, Paper, Hidden, Typography } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_PROVIDE_VALID,
    MONEY_TRANSFER_CODE
} from 'Components/Helper/Messages';
import YesBankBeneficiaryList from './BeneficiaryList';
import { Last10Report } from 'Pages/InnerPages/Report/Last10Report';
import { useLocation } from 'react-router-dom';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import SearchIcon from 'Components/SvgButtons/SearchIcon';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import HOCLoader from 'Components/Loader/HOCLoader';
import AutoSubmitMobileForSender from 'Components/Helper/AutoSubmitMobileForSender';

const CustomContainer = HOCLoader(Container);

const SenderSearch = ({ ActiveDMTName }) => {
    const formRef = useRef();
    const classes = commonStyle();
    const location = useLocation();
    const history = useHistory();
    const [senderNo, setSenderNo] = useState('');
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [addBeneficiary, setAddBeneficiary] = useState('none');
    const [serviceCode, setServiceCode] = useState(MONEY_TRANSFER_CODE);
    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }
    const Mobile = location?.state?.Mobile;

    const [formObject, setFormObject] = useState({
        Mobile: '',
        Name: '',
        SenderID: '',
        isAddBeneficiary: false,
        RemainingLimit: '',
        Consumed: ''
    });

    useEffect(() => {
        fetchConsumerNumberFontSize();
        select();
        setLoading(false);
    }, []);

    useEffect(() => {
        const timer = setTimeout(() => {
            if (formRef === undefined) return;

            if (!formRef.current) return;

            if (formRef.current.values === undefined) return;

            if (formRef.current.values.Mobile === undefined) return;

            if (formRef.current.values.Mobile.length !== 10) return;

            formRef.current.handleSubmit();
        }, 500);
        return () => clearTimeout(timer);
    }, []);

    const select = async () => {
        let primaryID = Mobile;

        setFormObject({
            ...formObject,
            Mobile: primaryID,
            Name: '',
            SenderID: '',
            RemainingLimit: '',
            Consumed: '',
            DMTName: ActiveDMTName
        });
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setServiceCode('');
        setSenderNo('');

        values.DMTName = ActiveDMTName;
        values.ServiceID = serviceCode;

        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);

        setFormObject({
            ...formObject,
            Mobile: values.Mobile,
            Name: '',
            SenderID: '',
            RemainingLimit: '',
            Consumed: ''
        });
        var response = await SENDER_SELECT(requestObj);

        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        setServiceCode(MONEY_TRANSFER_CODE);
        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            setIsError(true);
            setResponseMessage(response.data);
            if (response.data === 'Customer Does not exist.') {
                let params = {
                    Mobile: values.Mobile,
                    DMTName: ActiveDMTName
                };
                history.push('/YBDMT_SenderAdd', params);
                return;
            }
            if (
                response.data ===
                'Your KYC is not approved, please contact to customer care'
            ) {
                history.push('/KycIndex');
                return;
            }
            setLoading(false);
            return;
        }

        setAddBeneficiary('inline');
        var data = JSON.parse(response.data);

        setFormObject({
            Mobile: data.Mobile,
            Name: data.Name,
            Consumed: data.Consumed,
            RemainingLimit: data.RemainingLimit,
            isAddBeneficiary: false,
            DMTName: ActiveDMTName
        });

        setSenderNo(data.Mobile);
    };

    const FormValidationSchema = Yup.object({
        Mobile: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'mobile')
            .length(10, PLEASE_PROVIDE_VALID + 'mobile')
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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <AutoSubmitMobileForSender />
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialNumberField
                                                font_size={
                                                    consumerNumberFontSize
                                                }
                                                label="Mobile Number"
                                                name="Mobile"
                                                type="Mobile"
                                                length={10}
                                                typed_length={1}
                                                placeholder="Mobile Number"
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
                                                title="Search"
                                                disabled={loading}
                                                //endIcon={<SearchIcon />}
                                                onClick={(e) => {
                                                    setFieldValue(
                                                        'isAddBeneficiary',
                                                        false
                                                    );
                                                    setAddBeneficiary('none');
                                                    handleSubmit(e);
                                                }}
                                            >
                                                Search
                                            </PrimaryButton>
                                        </Grid>
                                        <Grid item xs={12}>
                                            <Box
                                                display="flex"
                                                flexDirection="column"
                                                p={2}
                                            >
                                                <Grid container>
                                                    <Grid item xs={3}>
                                                        <Box
                                                            display="flex"
                                                            justifyContent="flex-start"
                                                        >
                                                            <Typography
                                                                variant="body1"
                                                                fontWeight="bold"
                                                            >
                                                                Name :
                                                            </Typography>
                                                            <Typography
                                                                variant="body1"
                                                                marginLeft="10px"
                                                                style={{
                                                                    fontWeight:
                                                                        'bold',
                                                                    marginLeft:
                                                                        '5px'
                                                                }}
                                                            >
                                                                {
                                                                    formObject.Name
                                                                }
                                                            </Typography>
                                                        </Box>
                                                    </Grid>
                                                    <Grid item xs={3}>
                                                        <Box
                                                            display="flex"
                                                            justifyContent="flex-start"
                                                        >
                                                            <Typography
                                                                variant="body1"
                                                                fontWeight="bold"
                                                            >
                                                                Monthly Limit :
                                                            </Typography>
                                                            <Typography
                                                                variant="body1"
                                                                marginLeft="10px"
                                                                style={{
                                                                    fontWeight:
                                                                        'bold',
                                                                    marginLeft:
                                                                        '5px'
                                                                }}
                                                            >
                                                                {
                                                                    formObject.RemainingLimit
                                                                }
                                                            </Typography>
                                                        </Box>
                                                    </Grid>
                                                    {/* <Grid item xs={3}>
                                                        <Box
                                                            display="flex"
                                                            justifyContent="flex-start"
                                                        >
                                                            <Typography
                                                                variant="body1"
                                                                fontWeight="bold"
                                                            >
                                                                KYC Status :
                                                            </Typography>
                                                            <Typography
                                                                variant="body1"
                                                                marginLeft="10px"
                                                                style={{
                                                                    fontWeight:
                                                                        'bold',
                                                                    marginLeft:
                                                                        '5px'
                                                                }}
                                                            >
                                                                Verified
                                                            </Typography>
                                                        </Box>
                                                    </Grid> */}
                                                    <Grid item xs={3}>
                                                        <Box
                                                            display="flex"
                                                            justifyContent="flex-start"
                                                        >
                                                            <Typography
                                                                variant="body1"
                                                                fontWeight="bold"
                                                            >
                                                                Balance :
                                                            </Typography>
                                                            <Typography
                                                                variant="body1"
                                                                marginLeft="10px"
                                                                style={{
                                                                    fontWeight:
                                                                        'bold',
                                                                    marginLeft:
                                                                        '5px'
                                                                }}
                                                            >
                                                                {
                                                                    formObject.Consumed
                                                                }
                                                            </Typography>
                                                        </Box>
                                                    </Grid>
                                                </Grid>
                                            </Box>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <Hidden xsUp>
                                                <MaterialTextField
                                                    label="SenderID"
                                                    name="SenderID"
                                                    type="text"
                                                    readOnly={true}
                                                    placeholder="SenderID"
                                                />
                                            </Hidden>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>

            <YesBankBeneficiaryList
                senderNo={senderNo}
                ActiveDMTName={ActiveDMTName}
                senderName={formObject.Name}
                setIsError={(value) => {
                    setIsError(value);
                }}
                setSeverity={(value) => {
                    setSeverity(value);
                }}
                setLoading={(value) => {
                    setLoading(value);
                }}
                setResponseMessage={(value) => {
                    setResponseMessage(value);
                }}
                formRef={formRef}
            />

            <Last10Report
                ServiceID={serviceCode}
                setIsError={(value) => {
                    setIsError(value);
                }}
                setSeverity={(value) => {
                    setSeverity(value);
                }}
                setLoading={(value) => {
                    setLoading(value);
                }}
                setResponseMessage={(value) => {
                    setResponseMessage(value);
                }}
                formRef={formRef}
            />
        </CustomContainer>
    );
};
export default SenderSearch;
