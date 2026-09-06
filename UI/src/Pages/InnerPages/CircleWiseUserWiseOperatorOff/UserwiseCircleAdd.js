import React, { useState, useEffect } from 'react';
import { Formik, FieldArray, Form } from 'formik';
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
import { SelectOnlyRetailerAndAPI as USERTYPE_SELECT } from 'Components/Api/UserTypeApi';
import { VARIANT } from 'Config';
import { SelectByRoleUserBulk as SELECT } from 'Components/Api/UsersApi';
import { BulkUpdate as BULK_UPDATE } from 'Components/Api/CircleWiseUserWiseOperatorOffAPI';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { ZoneMasterSelect as Select } from 'Components/Api/StatesApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import {
    PLEASE_SELECT_VALID,
    PREPAID_RECHARGE_CODE,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { useHistory } from 'react-router';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const UserWiseCircleAdd = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [Operator, setOperator] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [currentPage, setCurrentPage] = useState(0);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);

    const [userType, setUserType] = useState(0);
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

    const [formObject, setFormObject] = useState({
        UserTypeID: 0,
        OperatorID: '0',
        StateMasterList: []
    });

    const FormValidationSchema = Yup.object({
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator')
    });

    const callSelectData = async (TakeCount, SkipCount, value) => {
        if (value === null || value <= 0 || value === '') {
            value = userType;
        }

        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            UserTypeID: value
        };

        setIsError(false);
        setLoadingOpen(true);

        var requestJSON = JSON.stringify(requestObj);

        var response = await SELECT(requestJSON);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setIsError(true);

        setSeverity('success');

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);
        if (SkipCount <= 0) {
            setTotalCount(data.Count);
        }

        setItemList(data.list);
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        setCriteriaValues(values);
        setIsUpdateClicked(true);
        setShouldGetUpdatedDataFromTable(true);
        resetForm(formObject);
    };

    const OnRoleChange = async (name, value) => {
        setUserType(0);
        setUserType(value);
        callSelectData(TakeCount, SkipCount, value, '', '');
    };

    const ProcessUpdatedData = (data) => {
        setShouldGetUpdatedDataFromTable(false);
        if (IsUpdateClicked) {
            UpdateBulkClick(data);
            return;
        }
    };

    const selectStates = async () => {
        const response = await Select();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }
        var statedata = JSON.parse(response.data);
        setStateList(statedata);
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
            setResponseMessage('Please select one or more state(s)');
            return;
        }
        let request = [];
        requestArr.forEach((element) => {
            let { value } = element[0];
            request.push({
                UserID: value,
                OperatorID: CriteriaValues.OperatorID,
                StateMasterList: CriteriaValues.StateMasterList
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
            OperatorID: 0
        });

        setItemList([]);
        createColumns();
        history.push('/UserwiseCircleIndex');
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
                                            md={6}
                                            lg={6}
                                        >
                                            {/* <Grid
                                                item
                                                className={classes.multiSelect}
                                            > */}
                                            <FieldArray
                                                name="StateMasterList"
                                                render={(stateIdList) => (
                                                    <Autocomplete
                                                        multiple
                                                        id="StateData"
                                                        options={stateList}
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.Name}
                                                        onChange={(
                                                            event,
                                                            values
                                                        ) => {
                                                            values.StateMasterList = [];
                                                            values.map(
                                                                (category) => {
                                                                    stateIdList.push(
                                                                        {
                                                                            ID:
                                                                                category.ID
                                                                        }
                                                                    );
                                                                    values.StateMasterList.push(
                                                                        {
                                                                            ID:
                                                                                category.ID
                                                                        }
                                                                    );
                                                                }
                                                            );
                                                        }}
                                                        renderInput={(
                                                            params
                                                        ) => (
                                                            <TextField
                                                                variant={
                                                                    VARIANT
                                                                }
                                                                {...params}
                                                                label="State(s)"
                                                                placeholder="State"
                                                            />
                                                        )}
                                                    />
                                                )}
                                            />
                                            {/* </Grid> */}
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
                count={totalCount}
                rowsPerPage={TakeCount}
                SuccessFailHeader={true}
                primaryIndex={12}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                    callSelectData(
                        RowsPerPage,
                        RowsPerPage * currentPage,
                        '',
                        '',
                        ''
                    );
                }}
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
