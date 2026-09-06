import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { Container, Grid, Button, MenuItem } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import CircularProgress from '@material-ui/core/CircularProgress';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import Box from '@material-ui/core/Box';

import { ApproveOnboarding as Approve } from 'Components/Api/OnboardingCallbackAPI';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import { ADMIN_CODE } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { ViewDetails } from './ViewDetails';
const CustomContainer = HOCLoader(Container);

export const ActionAgentOnboarding = ({
    IsEdit,
    OnBoardingUserID,
    onDismissCallback
}) => {
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    var currentUserTypeId = localStorage.getItem('UserTypeID');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [formObject, setformObject] = useState({
        BeneficiaryID: OnBoardingUserID,
        Remark: '',
        Status: ''
    });

    useEffect(() => {

        CheckUserRole();
    }, []);


    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUserTypeId)) setIsAdminRole(true);
    };

    const validationSchema = Yup.object({
        Remark: Yup.string().required(PLEASE_PROVIDE_VALID + 'remark'),
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Status')
            .required(PLEASE_SELECT_VALID + 'Status')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoadingOpen(true);
        const inputObj = {
            OnboardingUserID: OnBoardingUserID,
            Remark: values.Remark,
            Status: values.Status
        }
        const response = await Approve(JSON.stringify(inputObj));
        setLoadingOpen(false);
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
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data);
        onDismissCallback(false);
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
                                        label={`${isAdminRole === true ? "Checker" : "Maker"} Remark`}
                                        name="Remark"
                                        type="text"
                                        title={`${isAdminRole === true ? "Checker" : "Maker"} Remark`}
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
                                            Accept
                                        </MenuItem>
                                        <MenuItem key="REJECT" value="REJECT">
                                            Reject
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
