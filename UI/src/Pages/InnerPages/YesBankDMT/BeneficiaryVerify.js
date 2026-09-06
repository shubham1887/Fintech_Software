import React, { useState } from 'react';
import { Formik, Form } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { LOGOUT } from 'ConstComponents';
import { BeneficiaryOTPValidate as BENE_VERIFY } from 'Components/Api/YesBankDMTAPI';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { Container, Grid, Button, Hidden } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_PROVIDE_VALID,
    MONEY_TRANSFER_CODE,
    MONEY_TRANSFER_OPERATOR_TRANSFER_CODE
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

const BeneficiaryVerify = ({
    BeniName,
    AccountNo,
    IFSC,
    SenderID,
    BeneficiaryID,
    Mobile,
    ActiveDMTName,
    onDismissCallback,
    onSuccessCallback
}) => {
    const classes = commonStyle();
    const history = useHistory();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [formObject, setFormObject] = useState({
        Mobile: Mobile,
        OTP: ''
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var request = {
            BeneID: BeneficiaryID,
            OTP: values.OTP,
            DMTName: ActiveDMTName,
            Mobile: Mobile,
            AccountNo: AccountNo,
            IFSC: IFSC,
            BeneName: BeniName,
            BeneMobile: SenderID,
            OperatorID: MONEY_TRANSFER_OPERATOR_TRANSFER_CODE,
            ServiceID: MONEY_TRANSFER_CODE
        };
        var requestObj = JSON.stringify(request);

        setLoading(true);
        setIsError(false);

        var response = await BENE_VERIFY(requestObj);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setLoading(false);
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        var data = JSON.parse(response.data);
        setResponseMessage(data.Message);
        setSeverity('success');
        setIsError(true);
        onDismissCallback(false);

        var transferRequest = {
            BeneficiaryID: data.BeneficiaryID,
            DMTName: ActiveDMTName,
            Mobile: Mobile,
            AccountNo: AccountNo,
            IFSC: IFSC,
            BeniName: BeniName,
            BeneMobile: SenderID,
            OperatorID: MONEY_TRANSFER_OPERATOR_TRANSFER_CODE,
            ServiceID: MONEY_TRANSFER_CODE
        };
        onSuccessCallback(transferRequest);
    };

    const FormValidationSchema = Yup.object({
        OTP: Yup.string().required(PLEASE_PROVIDE_VALID + 'OTP'),
        Mobile: Yup.string().required(PLEASE_PROVIDE_VALID + 'mobile')
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
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <MaterialTextField
                                        label="OTP"
                                        name="OTP"
                                        length={6}
                                        type="text"
                                        placeholder="OTP"
                                    />

                                    <Hidden xsUp>
                                        <MaterialTextField
                                            label="BeneficiaryID"
                                            name="BeneficiaryID"
                                            type="text"
                                            readOnly={true}
                                            placeholder="BeneficiaryID"
                                        />
                                    </Hidden>
                                    <Hidden xsUp>
                                        <MaterialTextField
                                            label="Sender Mobile"
                                            name="Mobile"
                                            type="text"
                                            placeholder="Sender Mobile"
                                        />
                                    </Hidden>
                                </Grid>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Verify"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            onClick={(e) => {
                                                setFieldValue(
                                                    'isSecondSubmit',
                                                    false
                                                );
                                                handleSubmit(e);
                                            }}
                                            disabled={loading}
                                        >
                                            Submit
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
                                        title="Cancel"
                                        className={
                                            classes.CustomDangerButtonStyle
                                        }
                                        // onClick={(e) => {
                                        //     history.push(
                                        //         '/YBDMT_BeneficiaryAdd'
                                        //     );
                                        // }}
                                    >
                                        Cancel
                                    </Button>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            />
        </CustomContainer>
    );
};
export default BeneficiaryVerify;
