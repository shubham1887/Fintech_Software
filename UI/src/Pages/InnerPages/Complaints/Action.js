import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    Container,
    Grid,
    Button,
    MenuItem,
    Typography
} from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import CircularProgress from '@material-ui/core/CircularProgress';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { HeaderComplaint } from 'Components/SummaryCards/HeaderComplaint';
import BBPS_HORIZONTAL_LOGO from 'Assets/Images/BBPS/BBPS-Horizontal-05.png';
import Box from '@material-ui/core/Box';

import {
    Update as UPDATE,
    SelectByID as SELECT_BY_ID
} from 'Components/Api/ComplaintsAPI';

import { SelectForComplaint as SELECT_FOR_COMPLAINT } from 'Components/Api/ComplaintsAPI';

import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const ActionComplaint = ({
    IsEdit,
    TransactionMasterID,
    ComplaintMasterID,
    onDismissCallback
}) => {
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [isBBPS, setIsBBPS] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [ComplaintObject, setComplaintObject] = useState({
        ComplaintMasterID: 0,
        TransactionMasterID: TransactionMasterID,
        Operator: '',
        DOC: '',
        ConsumerNumber: '',
        Amount: 0,
        Status: '0',
        CustomerRemark: '',
        AdminRemark: '',
        ComplaintType: '',
        OpTxnID: '',
        IsBBPS: false
    });
    const [headerComplaint, setheaderComplaint] = useState({
        ProviderName: '',
        SupportNumber: '',
        ProviderRequestID: ''
    });

    const FormValidationSchema = Yup.object({
        CustomerRemark: Yup.string().required(PLEASE_PROVIDE_VALID + 'remark'),
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'status')
            .required(PLEASE_SELECT_VALID + 'status')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoadingOpen(true);
        setSubmitting(false);
        var response = await UPDATE(JSON.stringify(values));

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setSeverity('error');
            setLoadingOpen(false);
            setSubmitting(false);
            return;
        }

        setTimeout(() => {
            onDismissCallback(false);
        }, MESSAGE_CLOSE_AFTER);
    };

    useEffect(() => {
        GetTransactionByID(TransactionMasterID, ComplaintMasterID);
    }, []);

    const GetTransactionByID = async (
        TransactionMasterID,
        ComplaintMasterID
    ) => {
        setLoadingOpen(true);

        let jsonObj = {
            ID: TransactionMasterID,
            ComplaintMasterID: ComplaintMasterID
        };
        var request = JSON.stringify(jsonObj);
        let response = '';
        if (!IsEdit) response = await SELECT_FOR_COMPLAINT(request);
        else response = await SELECT_BY_ID(request);
        setLoadingOpen(false);

        let data = JSON.parse(response.data);

        setIsBBPS(data.IsBBPS);
        setComplaintObject({
            ...ComplaintObject,
            ComplaintMasterID: data.ComplaintMasterID,
            TransactionMasterID: TransactionMasterID,
            ConsumerNumber: data.ConsumerNumber,
            Amount: data.Amount,
            Operator: data.Operator,
            CustomerRemark: data.CustomerRemark,
            AdminRemark: data.AdminRemark,
            DOC: data.TransactionDate,
            IsBBPS: data.IsBBPS
        });
        setheaderComplaint({
            ProviderName: data.ProviderName,
            SupportNumber: data.SupportNumber,
            ProviderRequestID: data.ProviderRequestID
        });
    };

    const BBPSHorizontalLogoImage = () => {
        return (
            <img
                src={BBPS_HORIZONTAL_LOGO}
                style={{
                    float: 'right',
                    marginRight: '25px',
                    marginBottom: '25px'
                }}
                alt="BBPS"
            />
        );
    };
    const BBPSMnemonic = ({ isBBPS }) => {
        return isBBPS ? <BBPSHorizontalLogoImage /> : null;
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
            <Grid item xs={12} sm={12} md={12} lg={12}>
                <Typography variant="subtitle1" gutterBottom>
                    <BBPSMnemonic isBBPS={isBBPS} />
                </Typography>
            </Grid>

            <HeaderComplaint headerComplaint={headerComplaint} />
            <Formik
                enableReinitialize={true}
                initialValues={ComplaintObject}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={ComplaintObject}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <Grid container spacing={3}>
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <MaterialTextField
                                        name="ConsumerNumber"
                                        label="Number"
                                        placeholder="Number"
                                        readOnly={true}
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <MaterialNumberField
                                        name="Amount"
                                        label="Amount"
                                        readOnly={true}
                                        placeholder="Amount"
                                        autoComplete="new-password"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <MaterialTextField
                                        label="Operator"
                                        name="Operator"
                                        type="text"
                                        readOnly={true}
                                        title="Operator"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <MaterialTextField
                                        label="Rch Date"
                                        name="DOC"
                                        type="text"
                                        readOnly={true}
                                        title="Rch DOC"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <MaterialTextField
                                        label="Type"
                                        name="ComplaintType"
                                        type="text"
                                        readOnly={true}
                                        title="Type"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={9} lg={9}>
                                    <MaterialTextField
                                        label="Remark"
                                        name="CustomerRemark"
                                        type="text"
                                        readOnly={true}
                                        title="Remark"
                                    />
                                </Grid>

                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <MaterialTextField
                                        label="Admin Remark"
                                        name="AdminRemark"
                                        type="text"
                                        title="Admin Remark"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <MaterialTextField
                                        label="OpTxnID"
                                        name="OpTxnID"
                                        type="text"
                                        title="OpTxnID "
                                    />
                                </Grid>

                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <MaterialSelect
                                        label="Status"
                                        name="Status"
                                        title="Status"
                                    >
                                        <MenuItem key="0" value="0">
                                            Select Status
                                        </MenuItem>

                                        <MenuItem key="ACCEPT" value="ACCEPT">
                                            ACCEPT
                                        </MenuItem>
                                        <MenuItem key="REJECT" value="REJECT">
                                            REJECT
                                        </MenuItem>
                                    </MaterialSelect>
                                </Grid>
                            </Grid>
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <div
                                    style={{
                                        color: 'green',
                                        marginBottom: '15px'
                                    }}
                                >
                                    Note : Complaint Status "ACCEPT" means
                                    transaction amount refunded on retailer or
                                    Api user wallet,
                                    <br /> Complaint status "REJECT" means no
                                    refund.
                                </div>
                            </Grid>
                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Submit"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loadingOpen}
                                        >
                                            Submit
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
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
