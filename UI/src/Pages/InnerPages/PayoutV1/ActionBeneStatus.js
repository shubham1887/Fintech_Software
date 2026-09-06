import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { Container, Grid, Button, MenuItem } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import CircularProgress from '@material-ui/core/CircularProgress';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { HeaderBeneficiary } from 'Components/SummaryCards/HeaderBeneficiary';
import Box from '@material-ui/core/Box';
import { UpdateStatus as UPDATE } from 'Components/Api/PayoutBeneficiaryAPI';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const ActionBeneStatus = ({
    IsEdit,
    Beneficiary,
    onDismissCallback
}) => {
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [formObject, setformObject] = useState({
        BeneficiaryID: Beneficiary[1],
        AdminRemark: '',
        Status: ''
    });
    const [headerobject, setheaderObj] = useState({
        FirmName: Beneficiary[3],
        AccountNumber: Beneficiary[5],
        AccountHolder: Beneficiary[4],
        IFSC: Beneficiary[6],
        Document: Beneficiary[11],
        BankName: Beneficiary[7],
        IsOTPVerified: Beneficiary[8],
        IsNameVerified: Beneficiary[9],
    });

    useEffect(() => {
        HeaderData();
    }, [Beneficiary]);

    const HeaderData = (values) => {
        setheaderObj({
            FirmName: Beneficiary[3],
            AccountNumber: Beneficiary[5],
            AccountHolder: Beneficiary[4],
            IFSC: Beneficiary[6],
            Document: Beneficiary[9],
            BankName: Beneficiary[7],
            IsNameVerified: Beneficiary[8]
        });
    };

    const validationSchema = Yup.object({
        AdminRemark: Yup.string().required(PLEASE_PROVIDE_VALID + 'remark'),
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Status')
            .required(PLEASE_SELECT_VALID + 'Status')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoadingOpen(true);

        var response = await UPDATE(JSON.stringify(values));

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setSeverity('error');
            setLoadingOpen(false);
            return;
        }

        setTimeout(() => {
            onDismissCallback(false);
        }, MESSAGE_CLOSE_AFTER);
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
            <HeaderBeneficiary headerobject={headerobject} />
            <Formik
                enableReinitialize={true}
                initialValues={formObject}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <Grid container spacing={3}>
                                <Grid item xs={12} sm={12} md={8} lg={8}>
                                    <MaterialTextField
                                        label="Admin Remark"
                                        name="AdminRemark"
                                        type="text"
                                        title="Admin Remark"
                                    />
                                </Grid>

                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <MaterialSelect
                                        label="Status"
                                        name="Status"
                                        title="Status"
                                    >
                                        <MenuItem key="0" value="0">
                                            Select Status
                                        </MenuItem>

                                        <MenuItem key="APPROVE" value="APPROVE">
                                            APPROVE
                                        </MenuItem>
                                        <MenuItem key="REJECT" value="REJECT">
                                            REJECT
                                        </MenuItem>
                                    </MaterialSelect>
                                </Grid>
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
