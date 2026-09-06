import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { AssignPlan as ASSIGN_PLAN } from 'Components/Api/SubscriptionPlanAPI';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectPlan as SELECT_PLAN } from 'Components/Api/SubscriptionPlanAPI';
import { SelectByDetailID as SELECT_BY_ID } from 'Components/Api/SubscriptionPlanAPI';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import {
    PLEASE_PROVIDE_VALID,
    RETAILER_CODE
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { SelectRetailerAndAPIUserForDropDown as USER_SELECT_LIST } from 'Components/Api/UsersApi';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

const CustomContainer = HOCLoader(Container);

export const AssignPlan = (setFieldValue) => {
    const formRef = useRef();
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);

    const [severity, setSeverity] = useState('success');
    const [UsersList, setUsersList] = useState([]);
    const [plans, setPlans] = useState([]);
    const [userId, setUserId] = useState(0);
    const [amountRecovered, setAmountRecovered] = useState('');
    const [IsDisplayRecoveryMode, setIsDisplayRecoveryMode] = useState(false);
    const [ModeTextLabel, setModeTextLabel] = useState('');
    const [placeholder, setPlaceholder] = useState('');

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });
    const [formObj, setFormObj] = useState({
        ServiceSubscriptionCreatePlanMasterID: '0',
        PlanAmount: '',
        PlanName: '',
        GSTCharges: '',
        AmountToRecovered: '',
        AmountRecovered: '',
        RecoveryMode: '',
        User: userDefault,
        UserID: 0
    });

    const formValidationSchema = Yup.object({
        PlanAmount: Yup.string().required(PLEASE_PROVIDE_VALID + 'amount')
    });

    useEffect(() => {
        select();
    }, []);

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;
        var jsonReq = {
            PlanName: values.PlanName,
            PlanAmount: values.AmountToRecovered,
            ServiceSubscriptionCreatePlanID:
                values.ServiceSubscriptionCreatePlanMasterID,
            UserID: userId
        };
        setIsError(false);
        var requestObj = JSON.stringify(jsonReq);
        setLoadingOpen(true);
        var response = await ASSIGN_PLAN(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            reset();
            return;
        }
        var data = JSON.parse(response.data);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data.Message);
        setSubmitting(false);
        reset();
    };

    const reset = () => {
        setUserDefault({
            UserID: 0,
            FirmName: 'Select User'
        });

        setFormObj({
            ServiceSubscriptionCreatePlanMasterID: '0',
            PlanAmount: '',
            PlanName: '',
            GSTCharges: '',
            AmountToRecovered: '',
            AmountRecovered: '',
            RecoveryMode: '',
            User: userDefault,
            UserID: 0
        });
    };

    const callSearch = async (text) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            text.length <= 0 ||
            text === userDefault.FirmName
        )
            return;

        var jsonObj = {
            FirmName: text
        };
        selectUsers(jsonObj);
    };

    const selectUsers = async () => {
        var request = {
            UserTypeID: RETAILER_CODE
        };
        var json = JSON.stringify(request);
        const response = await USER_SELECT_LIST(json);
        if (!response.isAuthorized) LOGOUT();
        var data = JSON.parse(response.data);

        let objWithSelectAll = [userDefault, ...data];
        setUsersList(objWithSelectAll);
    };

    const select = async () => {
        setLoadingOpen(true);
        const response = await SELECT_PLAN();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;
        var data = JSON.parse(response.data);
        setPlans(data);
    };

    const OnChange = async (name, value, setFieldValue) => {
        var ID = value;
        setFieldValue(name, value);
        SelectByDetailID(ID);
    };

    const handleAmountRecoveredChange = async (name, value, setFieldValue) => {
        var ID = value;
        setFieldValue(name, value);
        setAmountRecovered(ID);
    };

    const handleRecoveryModeChange = (event) => {
        const value = event.target.value;
        setFieldValue('RecoveryMode', value);
        OnChangeRecoveryMode(value);
    };

    const TextDisplay = (setFieldValue) => {
        setFieldValue('RecoveryMode', '');
        setIsDisplayRecoveryMode(true);
    };

    const menuItems = [
        {
            key: 'IMPS/NEFT',
            value: 'IMPS/NEFT'
        },
        {
            key: 'FundingToDistributor',
            value: 'FundingToDistributor'
        }
    ];

    const OnChangeRecoveryMode = (value) => {
        console.log('value', value);
        if (value === undefined || value <= 0) {
            setIsDisplayRecoveryMode(false);
            setModeTextLabel('');
            return;
        }
        setIsDisplayRecoveryMode(true);

        switch (value) {
            case 'IMPS/NEFT':
                setPlaceholder('Enter RefID');
                break;
            case 'FundingToDistributor':
                setPlaceholder('Enter DistID');
                break;
            default:
                setPlaceholder(`Enter ${value} details`);
        }
    };

    const SelectByDetailID = async (ID) => {
        var requestObj = {
            ServiceSubscriptionCreatePlanID: ID
        };
        var request = JSON.stringify(requestObj);
        const response = await SELECT_BY_ID(request);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) return;
        var data = JSON.parse(response.data);
        setFormObj({
            ...formObj,
            ServiceSubscriptionCreatePlanMasterID:
                data.ServiceSubscriptionCreatePlanMasterID,
            PlanName: data.PlanName,
            AmountToRecovered: data.TotalAmount,
            GSTCharges: data.GST,
            PlanAmount: data.Amount
        });
    };

    const createMenuItem = (key, setFieldValue) => (
        <MenuItem
            key={key}
            value={key}
            onClick={() => {
                TextDisplay(setFieldValue);
            }}
        >
            {key}
        </MenuItem>
    );

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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={formObj}
                validationSchema={formValidationSchema}
                onSubmit={handleSubmit}
                resetForm={formObj}
                render={({ values, setFieldValue }) => {
                    return (
                        <Grid
                            item
                            container
                            margin="normal"
                            xs={12}
                            md={12}
                            sm={12}
                            lg={12}
                            spacing={2}
                        >
                            <Grid item xs={12} sm={12}>
                                <Form>
                                    <h2 className={classes.PaperHead_h2}>
                                        Assign Plan
                                    </h2>
                                    <Grid item xs={12}>
                                        <Paper className={classes.PaperStyle}>
                                            <Grid container spacing={2}>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={8}
                                                    lg={8}
                                                >
                                                    <Field
                                                        component={
                                                            AsyncAutoComplete
                                                        }
                                                        defaultValue={
                                                            userDefault
                                                        }
                                                        FieldName="User"
                                                        name="User"
                                                        SubFieldName="UserID"
                                                        options={UsersList}
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.FirmName}
                                                        textFieldProps={{
                                                            label:
                                                                'Select Entity Id'
                                                        }}
                                                        onSearchCallback={(
                                                            text
                                                        ) => {
                                                            callSearch(text);
                                                        }}
                                                        onCustomCallback={(
                                                            name,
                                                            value
                                                        ) => {
                                                            setUserDefault({
                                                                UserID:
                                                                    value.UserID,
                                                                FirmName:
                                                                    value.FirmName
                                                            });
                                                            setUserId(
                                                                value.UserID
                                                            );
                                                        }}
                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={6}
                                                    lg={6}
                                                >
                                                    <MaterialSelect
                                                        label="Select Plan *"
                                                        name="ServiceSubscriptionCreatePlanMasterID"
                                                        title="Select Plan"
                                                        onCustomCallback={(
                                                            name,
                                                            value
                                                        ) => {
                                                            OnChange(
                                                                name,
                                                                value,
                                                                setFieldValue
                                                            );
                                                        }}
                                                    >
                                                        <MenuItem
                                                            key="0"
                                                            value="0"
                                                        >
                                                            Select Plan
                                                        </MenuItem>
                                                        {plans.map((plan) => (
                                                            <MenuItem
                                                                key={
                                                                    plan.ServiceSubscriptionCreatePlanMasterID
                                                                }
                                                                value={
                                                                    plan.ServiceSubscriptionCreatePlanMasterID
                                                                }
                                                            >
                                                                {plan.PlanName}
                                                            </MenuItem>
                                                        ))}
                                                    </MaterialSelect>
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <MaterialNumberField
                                                        label="Plan Amount:"
                                                        name="PlanAmount"
                                                        type="text"
                                                        placeholder="Plan Amount"
                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <MaterialNumberField
                                                        label="GST Charges:"
                                                        name="GSTCharges"
                                                        type="text"
                                                        placeholder="GST Charges"
                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={6}
                                                    lg={6}
                                                >
                                                    <MaterialNumberField
                                                        label="Amount to be recovered"
                                                        name="AmountToRecovered"
                                                        type="text"
                                                        placeholder="Amount to be recovered"
                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={6}
                                                    lg={6}
                                                >
                                                    <MaterialSelect
                                                        label="Amount Recovered"
                                                        name="AmountRecovered"
                                                        onCustomCallback={(
                                                            name,
                                                            value
                                                        ) => {
                                                            handleAmountRecoveredChange(
                                                                name,
                                                                value,
                                                                setFieldValue
                                                            );
                                                        }}
                                                    >
                                                        <MenuItem
                                                            key="0"
                                                            value="0"
                                                        >
                                                            Amount Recovered
                                                        </MenuItem>

                                                        <MenuItem
                                                            key="Yes"
                                                            value="Yes"
                                                        >
                                                            Yes
                                                        </MenuItem>
                                                        <MenuItem
                                                            key="No"
                                                            value="No"
                                                        >
                                                            No
                                                        </MenuItem>
                                                    </MaterialSelect>
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={6}
                                                    lg={6}
                                                >
                                                    <MaterialSelect
                                                        label="Mode of Recovery"
                                                        name="RecoveryMode"
                                                        onChange={
                                                            handleRecoveryModeChange
                                                        }
                                                    >
                                                        <MenuItem
                                                            key={0}
                                                            value={0}
                                                        >
                                                            Mode of Recovery
                                                        </MenuItem>
                                                        {amountRecovered ===
                                                        'Yes' ? (
                                                            menuItems.map(
                                                                (item) =>
                                                                    createMenuItem(
                                                                        item.key,
                                                                        setFieldValue
                                                                    )
                                                            )
                                                        ) : (
                                                            <MenuItem
                                                                key="DebitBalance"
                                                                value="DebitBalance"
                                                            >
                                                                Debit from Agent
                                                                e-Balance
                                                            </MenuItem>
                                                        )}
                                                    </MaterialSelect>
                                                </Grid>
                                                {IsDisplayRecoveryMode && (
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            label={
                                                                ModeTextLabel
                                                            }
                                                            name="RecoveryModeDetails"
                                                            type="text"
                                                            placeholder={
                                                                placeholder
                                                            }
                                                        />
                                                    </Grid>
                                                )}
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <SecondaryButton
                                                        type="reset"
                                                        title="Cancel"
                                                        className={
                                                            classes.CustomDangerButtonStyle
                                                        }
                                                    >
                                                        Cancel
                                                    </SecondaryButton>
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <PrimaryButton
                                                        type="submit"
                                                        title="Proceed"
                                                        className={
                                                            classes.CustomPrimaryButtonStyle
                                                        }
                                                        disabled={loadingOpen}
                                                    >
                                                        Proceed
                                                    </PrimaryButton>
                                                    {loadingOpen && (
                                                        <CircularProgress
                                                            size={24}
                                                            className={
                                                                classes.buttonProgress
                                                            }
                                                        />
                                                    )}
                                                </Grid>
                                            </Grid>
                                        </Paper>
                                    </Grid>
                                </Form>
                            </Grid>
                        </Grid>
                    );
                }}
            />
        </CustomContainer>
    );
};
