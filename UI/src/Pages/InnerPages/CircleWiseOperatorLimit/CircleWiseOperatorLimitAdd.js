import React, { useState, useEffect } from 'react';
import { Formik, FieldArray, Form, Field } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Paper,
    CircularProgress,
    MenuItem,
    TextField
} from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';
import Autocomplete from '@material-ui/lab/Autocomplete';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { SelectOnlyRetailerAndAPI as USERTYPE_SELECT } from 'Components/Api/UserTypeApi';
import { VARIANT } from 'Config';
import { SelectByRole as SELECT } from 'Components/Api/UsersApi';
import { BulkUpdate as BULK_UPDATE } from 'Components/Api/CircleWiseOperatorLimitAPI';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';
import { ZoneMasterSelect as Select } from 'Components/Api/StatesApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import {
    PLEASE_SELECT_VALID,
    PREPAID_RECHARGE_CODE,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { useHistory } from 'react-router';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const CircleWiseOperatorLimitAdd = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [Operator, setOperator] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [userTypeList, setUserTypeList] = useState([]);
    const [CriteriaValues, setCriteriaValues] = useState('');
    const [IsUpdateClicked, setIsUpdateClicked] = useState(false);
    const [stateList, setStateList] = useState([]);

    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });
        columns.push({
            label: 'Type',
            name: 'UserType'
        });
        columns.push({
            label: 'Name',
            name: 'FirmName'
        });

        columns.push({
            label: 'Mobile',
            name: 'MobileNumber'
        });

        columns.push({
            label: 'Parent',
            name: 'ParentName'
        });
        setMainColumns(columns);
    };

    useEffect(() => {
        createColumns();
        selectUserTypes();
        selectOperator();
        selectStates();
    }, []);
    const selectUserTypes = async () => {
        const { isAuthorized, status, data } = await USERTYPE_SELECT();

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setSeverity('error');
            setResponseMessage(data);
            setIsError(true);
            return;
        }

        var userTypes = JSON.parse(data);
        setUserTypeList(userTypes);
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
        Name: 'Select State'
    });
    const [formObject, setFormObject] = useState({
        UserTypeID: 0,
        OperatorID: '0',
        AmountLimit: 0,
        State: stateDefault
    });

    const FormValidationSchema = Yup.object({
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator'),
        AmountLimit: Yup.number()
            .moreThan(499, PLEASE_PROVIDE_VALID + 'amount more than 499')
            .required(PLEASE_PROVIDE_VALID + 'amount limit')
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        setCriteriaValues(values);
        setIsUpdateClicked(true);
        setShouldGetUpdatedDataFromTable(true);
        resetForm(formObject);
        // history.push('/UserwiseCircleIndex');
    };

    const OnRoleChange = async (name, value) => {
        var inputObj = {
            UserTypeID: value
        };

        var requestJSON = JSON.stringify(inputObj);
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }

        setIsError(false);
        setLoadingOpen(true);
        var response = await SELECT(requestJSON);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setIsError(true);

        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        setItemList(JSON.parse(response.data));
    };

    const ProcessUpdatedData = (data) => {
        setShouldGetUpdatedDataFromTable(false);
        if (IsUpdateClicked) {
            UpdateBulkClick(data);
            return;
        }
    };

    const selectOperator = async () => {
        var requestObj = {
            ServiceID: PREPAID_RECHARGE_CODE
        };
        var request = JSON.stringify(requestObj);

        const response = await OPERATOR_SELECT_BY_SERVICEID(request);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) setIsError(true);

        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);
            setOperator(data);
        }
    };

    const UpdateBulkClick = async (requestArr, resetForm) => {
        setIsUpdateClicked(false);
        if (requestArr.length <= 0) {
            setSeverity('error');
            setIsError(true);
            setResponseMessage('Please select one or more user(s)');
            return;
        }
        let request = [];
        requestArr.forEach((element) => {
            let { value } = element[0];
            request.push({
                UserID: value,
                OperatorID: CriteriaValues.OperatorID,
                StateID: CriteriaValues.State.StateID,
                AmountLimit: CriteriaValues.AmountLimit
            });
        });

        setLoadingOpen(true);
        var RequestJson = JSON.stringify(request);
        setIsError(false);
        const Response = await BULK_UPDATE(RequestJson);

        setLoadingOpen(false);
        if (!Response.isAuthorized) LOGOUT();

        if (!Response.status) {
            setSeverity('error');
            setResponseMessage(Response.data);
            setIsError(true);
            return;
        }
        setSeverity('success');
        setResponseMessage(Response.data);
        setIsError(true);

        setFormObject({
            ...formObject,
            UserTypeID: 0,
            OperatorID: 0,
            AmountLimit: 0,
            State: stateDefault
        });

        setItemList([]);
        createColumns();
        history.push('/OperatorLimitIndex');
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
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={handleSubmit}
                resetForm={formObject}
            >
                {(setFieldValue) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <h2 className={classes.PaperHead_h2}>User list</h2>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Role"
                                                name="UserTypeID"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnRoleChange(name, value);
                                                }}
                                                title="Role"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Role
                                                </MenuItem>
                                                {userTypeList.map(
                                                    (
                                                        {
                                                            UserTypeID,
                                                            UserType
                                                        },
                                                        index
                                                    ) => (
                                                        <MenuItem
                                                            key={index}
                                                            value={UserTypeID}
                                                        >
                                                            {UserType}
                                                        </MenuItem>
                                                    )
                                                )}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Operator"
                                                name="OperatorID"
                                                title="Operator"
                                            >
                                                <MenuItem key="0" value="0">
                                                    All Operator
                                                </MenuItem>
                                                {Operator.map((Item) => (
                                                    <MenuItem
                                                        key={Item.OperatorID}
                                                        value={Item.OperatorID}
                                                    >
                                                        {Item.OperatorName}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
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
                                                    label: 'Select State'
                                                }}
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    setStateDefault({
                                                        StateID: value.StateID,
                                                        Name: value.Name
                                                    });
                                                }}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialNumberField
                                                label="Amount Limit"
                                                name="AmountLimit"
                                                type="text"
                                                length={7}
                                                placeholder="Amount Limit"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={1}
                                            lg={1}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Update"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                Update
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
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={1}
                                            lg={1}
                                        >
                                            <SecondaryButton
                                                type="reset"
                                                title="Reset"
                                                className={
                                                    classes.CustomDangerButtonStyle
                                                }
                                            >
                                                Reset
                                            </SecondaryButton>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                SuccessFailHeader={true}
                primaryIndex={12}
                setLoadingOpen={(value) => {
                    setLoadingOpen(value);
                }}
                setIsError={(value) => {
                    setIsError(value);
                }}
                setSeverity={(value) => {
                    setSeverity(value);
                }}
                setResponseMessage={(value) => {
                    setResponseMessage(value);
                }}
                getUpdatedData={(data) => {
                    ProcessUpdatedData(data);
                    setShouldGetUpdatedDataFromTable(false);
                }}
                shouldGetUpdatedData={shouldGetUpdatedDataFromTable}
                isSelectableRows={'multiple'}
            />
        </CustomContainer>
    );
};
