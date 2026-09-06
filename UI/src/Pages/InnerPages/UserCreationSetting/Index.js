import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

import {
    Select as USER_SELECT,
    Insert as USER_INSERT,
    Update as USER_UPDATE,
    Delete as USER_DELETE
} from 'Components/Api/UserCreationSettingAPI';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import { SelectRetailerAndAPIUserForDropDown as RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectOnlyDistAndSuperDist as USER_TYPE_SELECT } from 'Components/Api/UserTypeApi';

import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';

import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const UserCreationSetting = () => {
    const classes = commonStyle();
    const formRef = useRef();

    const [mainColumns, setMainColumns] = useState([]);

    const [downLine, setDownLine] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [userTypeList, setUserTypeList] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [itemList, setItemList] = useState([]);
    const [openFilter, setopenFilter] = useState(true);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const [ModelObj] = useState({
        MinimumCount: 0,
        Cost: 0,
        UserTypeID: 0,
        User: userDefault
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            UserID: values.User.UserID,
            MinimumCount: values.MinimumCount,
            Cost: values.Cost
        };
        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);
        var response = await USER_INSERT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        selectData();
    };

    const selectUserTypes = async () => {
        const response = await USER_TYPE_SELECT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setResponseMessage(response.data);
        }

        var data = JSON.parse(response.data);
        setUserTypeList(data);
    };

    const UserTypeList = userTypeList.map((item) => (
        <MenuItem key={item.UserTypeID} value={item.UserTypeID}>
            {item.UserType}
        </MenuItem>
    ));

    const validationSchema = Yup.object({
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.number()
                    .nullable(PLEASE_SELECT_VALID + 'User')
                    .min(0, PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            }),
        MinimumCount: Yup.number()
            .min(5, PLEASE_PROVIDE_VALID + 'minimum 5 Count')
            .required(PLEASE_SELECT_VALID + 'Amount'),
        Cost: Yup.number()
            .min(10, PLEASE_PROVIDE_VALID + 'minimum 10 cost')
            .required(PLEASE_SELECT_VALID + 'Amount')
    });

    const callSearch = async (text, UserTypeID) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            text.length <= 0 ||
            text === userDefault.FirmName
        )
            return;

        var jsonObj = {
            FirmName: text,
            UserTypeID: UserTypeID
        };
        selectUsers(jsonObj);
    };

    const selectUsers = async (values) => {
        var request = JSON.stringify(values);
        const {
            isAuthorized,
            status,
            data
        } = await RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN(request);
        if (!isAuthorized) LOGOUT();
        if (!status) return;
        var d = JSON.parse(data);
        let objWithSelectAll = [userDefault, ...d];

        setDownLine(objWithSelectAll);
    };

    useEffect(() => {
        selectUserTypes();
        selectData();
        createColumns();
    }, []);

    const UpdateValidation = Yup.object({
        UserCreationSettingID: Yup.number().required(
            PLEASE_PROVIDE_VALID + 'ID'
        )
    });
    const selectData = async () => {
        //  var requestObj = JSON.stringify(value);
        setItemList([]);
        const { isAuthorized, status, data } = await USER_SELECT();

        setLoading(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);

            return;
        }
        var d = JSON.parse(data);

        setItemList(d);
    };
    const UpdateClick = async (row) => {
        setIsError(false);
        let jsonObj = {
            UserCreationSettingID: row[0],
            Cost: row[3],
            MinimumCount: row[4]
        };
        var errorValidation = false;
        await UpdateValidation.validate(jsonObj).catch(function (err) {
            errorValidation = true;
            setSeverity('error');
            setResponseMessage(err.message);
            setIsError(true);
            return;
        });
        if (errorValidation === true) {
            return;
        }
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoading(true);
        const response = await USER_UPDATE(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        selectData();
    };
    const Delete = async (UserCreationSettingID) => {
        let jsonObj = {
            UserCreationSettingID: UserCreationSettingID
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setSeverity('');
        setResponseMessage('');

        setLoading(true);
        const response = await USER_DELETE(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        selectData();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserCreationSettingID',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        columns.push({
            label: 'User',
            name: 'FirmName'
        });
        columns.push({
            label: 'Cost',
            name: 'Cost',
            editable: true
        });

        columns.push({
            label: 'Minimum Count',
            name: 'MinimumCount',
            editable: true
        });

        columns.push({
            label: 'Update',
            name: 'Update',
            save: true,
            buttonTitle: 'Update',
            callback: (row) => {
                UpdateClick(row);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'UserCreationSettingID',
            delete: true,
            callback: (UserCreationSettingID) => {
                Delete(UserCreationSettingID);
            }
        });
        setMainColumns(columns);
    };

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
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={ModelObj}
                render={({
                    values,
                    setFieldValue,
                    handleChange,
                    handleSubmit
                }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'User Creation Setting'}
                                        open={openFilter}
                                        setOpen={(value) =>
                                            setopenFilter(value)
                                        }
                                    >
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
                                                    title="Select Role"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Role
                                                    </MenuItem>
                                                    {UserTypeList}
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
                                                    component={
                                                        AsyncAutoComplete
                                                    }
                                                    defaultValue={userDefault}
                                                    FieldName="User"
                                                    name="User"
                                                    SubFieldName="UserID"
                                                    options={downLine}
                                                    getOptionLabel={(option) =>
                                                        option.FirmName
                                                    }
                                                    textFieldProps={{
                                                        label: 'Select User'
                                                    }}
                                                    onSearchCallback={(
                                                        text
                                                    ) => {
                                                        callSearch(
                                                            text,
                                                            values.UserTypeID
                                                        );
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
                                                    label="Cost"
                                                    name="Cost"
                                                    type="text"
                                                    placeholder="Cost"
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
                                                    label="Minimum Id Count"
                                                    name="MinimumCount"
                                                    type="text"
                                                    placeholder="Minimum Id Count"
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
                                                    type="submit"
                                                    title="Submit"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    disabled={loading}
                                                >
                                                    Submit
                                                </PrimaryButton>
                                                {loading && (
                                                    <CircularProgress
                                                        size={24}
                                                        className={
                                                            classes.buttonProgress
                                                        }
                                                    />
                                                )}
                                            </Grid>
                                        </Grid>
                                    </CustomAccordion>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />

            <ReactTable
                data={itemList}
                columns={mainColumns}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                }}
                count={totalCount}
                rowsPerPage={10}
            />
        </CustomContainer>
    );
};
