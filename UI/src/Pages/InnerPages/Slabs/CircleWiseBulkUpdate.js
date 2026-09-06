import React, { useState, useEffect } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Box,
    Button,
    MenuItem,
    CircularProgress
} from '@material-ui/core';
import { ToggleButton, ToggleButtonGroup } from '@material-ui/lab';

import { LOGOUT } from 'ConstComponents';
import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { BulkUpdate as CIRCLRWISE_BULK_UPDATE_COMMISSION } from 'Components/Api/CircleCommissionAPI';
import { ZoneMasterSelect as Select } from 'Components/Api/StatesApi';
import { SelectWithoutAPIUserType as USERTYPE_SELECT_WITHOUT_APIUSERTYPE } from 'Components/Api/UserTypeApi';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SUPPORT_USER } from 'Components/Helper/UsertypeCode';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const CirclewiseBulkUpdate = ({ SlabMasterID, onDismissCallback }) => {
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [stateList, setStateList] = useState([]);
    const [userTypeList, setUserTypeList] = useState([]);

    const [CommissionType, setCommissionType] = useState('Flexi');
    const [CommissionCategory, setCommissionCategory] = useState('Commission');

    const selectUserTypes = async () => {
        const {
            isAuthorized,
            status,
            data
        } = await USERTYPE_SELECT_WITHOUT_APIUSERTYPE();

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setSeverity('error');
            setResponseMessage(data);
            setIsError(true);
            return;
        }

        var usertypedata = JSON.parse(data);
        setUserTypeList(usertypedata);
    };
    const selectStates = async () => {
        const response = await Select();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setStateList(data);
    };
    const [stateDefault, setStateDefault] = useState({
        StateID: 0,
        Name: 'Select Circle'
    });
    const [formObject] = useState({
        SlabMasterID: SlabMasterID,
        State: stateDefault,
        UserTypeID: 0,
        SlabType: 'General',
        Commission: 0,
        CommissionType: CommissionType,
        CommissionCategory: CommissionCategory
    });

    const FormValidationSchema = Yup.object({
        State: Yup.object()
            .nullable(PLEASE_SELECT_VALID + ' circle')
            .required(PLEASE_SELECT_VALID + ' circle')
            .shape({
                StateID: Yup.string()
                    .notOneOf(['0'], PLEASE_SELECT_VALID + 'circle')
                    .required(PLEASE_SELECT_VALID + 'circle')
            }),

        UserTypeID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'role')
            .required(PLEASE_SELECT_VALID + 'role')
    });

    useEffect(() => {
        selectStates();
        selectUserTypes();
    }, []);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        values.StateID = values.State.StateID;
        var requestObj = JSON.stringify(values);
        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await CIRCLRWISE_BULK_UPDATE_COMMISSION(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        setSubmitting(true);
        setSeverity('success');

        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }
        setTimeout(() => {
            onDismissCallback(false);
        }, MESSAGE_CLOSE_AFTER);
    };

    const UserTypeItems = userTypeList.map(({ UserTypeID, UserType }, index) =>
        UserTypeID !== SUPPORT_USER ? (
            <MenuItem key={index} value={UserTypeID}>
                {UserType}
            </MenuItem>
        ) : null
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
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <Field
                                            component={AutoCompleteTextView}
                                            defaultValue={stateDefault}
                                            FieldName="State"
                                            SubFieldName="StateID"
                                            name="State"
                                            options={stateList}
                                            getOptionLabel={(option) =>
                                                option.Name
                                            }
                                            textFieldProps={{
                                                label: 'Select Circle'
                                            }}
                                            onCustomCallback={(name, value) => {
                                                setStateDefault({
                                                    StateID: value.StateID,
                                                    Name: value.Name
                                                });
                                            }}
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialSelect
                                            label="Role"
                                            name="UserTypeID"
                                            title="Role"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Role
                                            </MenuItem>
                                            {UserTypeItems}
                                        </MaterialSelect>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <MaterialNumberField
                                            label="Commission"
                                            name="Commission"
                                            type="text"
                                            length={7}
                                            placeholder="Commission Amount"
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <ToggleButtonGroup
                                            exclusive
                                            size="small"
                                            value={CommissionCategory}
                                            onChange={(event, newValue) => {
                                                setCommissionCategory(newValue);
                                                setFieldValue(
                                                    'CommissionCategory',
                                                    newValue
                                                );
                                            }}
                                        >
                                            <ToggleButton value={'Commission'}>
                                                COM
                                            </ToggleButton>
                                            <ToggleButton value={'Surcharge'}>
                                                SUR
                                            </ToggleButton>
                                        </ToggleButtonGroup>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <ToggleButtonGroup
                                            exclusive
                                            size="small"
                                            value={CommissionType}
                                            name={'CommissionType'}
                                            onChange={(event, newValue) => {
                                                setCommissionType(newValue);
                                                setFieldValue(
                                                    'CommissionType',
                                                    newValue
                                                );
                                            }}
                                        >
                                            <ToggleButton value={'Flexi'}>
                                                PER
                                            </ToggleButton>
                                            <ToggleButton value={'Fix'}>
                                                FIX
                                            </ToggleButton>
                                        </ToggleButtonGroup>
                                    </Grid>
                                </Grid>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Update"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loadingOpen}
                                        >
                                            Update
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
                                </Box>
                                <span style={{ color: 'green' }}>
                                    Note : Update all operator commission by
                                    selected "Circle" and "Slab"
                                </span>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
