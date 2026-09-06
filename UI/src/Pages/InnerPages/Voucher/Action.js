import React, { useEffect, useState, useContext } from 'react';
import { Formik, Form } from 'formik';

import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { Update } from 'Components/Api/VoucherAPI';
import { SelectByIDForVoucherApprove } from 'Components/Api/VoucherAPI';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { Container, MenuItem, Grid, Button } from '@material-ui/core';
import InputLabel from 'Components/Inputes/InputLabel';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import CircularProgress from '@material-ui/core/CircularProgress';
import Box from '@material-ui/core/Box';
import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER,
    IS_CASH_COLLECTION
} from 'Components/Helper/Messages';
import { BalanceContext } from 'Components/BalanceContext';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const Action = ({ VoucherMasterID, onDismissCallback }) => {
    const { DashboardBalanceUpdate } = useContext(BalanceContext);
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [formObject, setFormObject] = useState({
        VoucherMasterID: 0
    });

    const validationSchema = Yup.object({
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'status')
            .required(PLEASE_SELECT_VALID + 'status'),

        VoucherMasterID: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'ID')
            .min(1, PLEASE_PROVIDE_VALID + 'ID')
    });

    const select = async (VoucherMasterID) => {
        let JSONObj = {
            VoucherMasterID: VoucherMasterID
        };
        setLoadingOpen(true);
        var request = JSON.stringify(JSONObj);

        const response = await SelectByIDForVoucherApprove(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        var data = JSON.parse(response.data);
        setFormObject({
            VoucherMasterID: data.VoucherMasterID,
            Amount: data.Amount,
            DOC: data.DOC,
            VoucherType: data.VoucherType,
            CrGroup: data.CrGroup,
            CrName: data.CrName,
            DrGroup: data.DrGroup,
            DrName: data.DrName,
            Remark: data.Remark,
            SubmitedByName: data.SubmitedByName,
            InWords: NumberToWord(data.Amount)
        });
    };

    useEffect(() => {
        select(VoucherMasterID);
    }, []);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoadingOpen(true);

        setResponseMessage('');
        setIsError(false);

        var response = await Update(JSON.stringify(values));

         if (!response.isAuthorized) LOGOUT();

        DashboardBalanceUpdate();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setSeverity('error');
            setLoadingOpen(false);
            return;
        }
        setSubmitting(false);

        resetForm(formObject);
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
            <Formik
                enableReinitialize={true}
                initialValues={formObject}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        VoucherType :
                                        <span className={classes.Success_h3}>
                                            {formObject.VoucherType}
                                        </span>
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        CrGroup :
                                        <span className={classes.Success_h3}>
                                            {formObject.CrGroup}
                                        </span>
                                    </h2>
                                </Grid>
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        CrName :
                                        <span className={classes.Success_h3}>
                                            {'₹ ' + formObject.CrName}
                                        </span>
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        Date :
                                        <span className={classes.Success_h3}>
                                            {formObject.DOC}
                                        </span>
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        Amount :
                                        <span className={classes.Success_h3}>
                                            {'₹ ' + formObject.Amount}
                                        </span>
                                        <span className={classes.Success_h3}>
                                            ({formObject.InWords})
                                        </span>
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        DrGroup :
                                        <span className={classes.Success_h3}>
                                            {formObject.DrGroup}
                                        </span>
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        DrName :
                                        <span className={classes.Success_h3}>
                                            {formObject.DrName}
                                        </span>
                                    </h2>
                                </Grid>
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.recharge_h2}>
                                        Remark :
                                        <span className={classes.Success_h3}>
                                            {formObject.Remark}
                                        </span>
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <h2 className={classes.recharge_h2}>
                                        SubmittedBy :
                                        <span className={classes.Success_h3}>
                                            {formObject.SubmitedByName}
                                        </span>
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <MaterialSelect
                                        label="Status"
                                        name="Status"
                                        title="Select Status"
                                    >
                                        <MenuItem key="0" value="0">
                                            Select Status
                                        </MenuItem>
                                        <MenuItem key="Accept" value="Accept">
                                            Accept
                                        </MenuItem>
                                        <MenuItem key="Reject" value="Reject">
                                            Reject
                                        </MenuItem>
                                    </MaterialSelect>
                                </Grid>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <Box display="none">
                                        <MaterialTextField
                                            label="VoucherMasterID"
                                            name="VoucherMasterID"
                                            type="text"
                                            title="VoucherMasterID"
                                        />
                                    </Box>
                                </Grid>
                            </Grid>

                            <Grid
                                className={classes.footerGrid}
                                style={{ marginTop: '20px' }}
                            >
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
