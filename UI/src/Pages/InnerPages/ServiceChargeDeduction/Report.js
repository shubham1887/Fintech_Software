import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';

import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { Container, Grid } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { Select as SELECT } from 'Components/Api/ServiceChargeDeductionAPI';

import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import {
    RETAILER,
    API_USER,
    CLIENT_CODE
} from 'Components/Helper/UsertypeCode';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);

const CustomContainer = HOCLoader(Container);

export const ServiceChargeDeductionReport = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [downline, setDownline] = useState([]);
    const [currentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [openFilter, setopenFilter] = useState(true);
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });
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
        SelectUser(jsonObj);
    };

    const SelectUser = async (values) => {
        var request = JSON.stringify(values);

        const userResponse = await USER_SELECT_FOR_DROPDOWN(request);

        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(userResponse.data);
        let objWithSelectAll = [userDefault, ...data];

        setDownline(objWithSelectAll);
    };

    const [ModelObj] = useState({
        FromDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
        ToDate: new Date(),
        User: userDefault
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        values.UserID = values.User.UserID;
        var requestObj = JSON.stringify(values);

        setIsError(false);
        setLoading(true);
        createColumns();
        setTransactions([]);
        var response = await SELECT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        setSubmitting(false);
        var data = JSON.parse(response.data);

        setTransactions(data);
    };

    const validationSchema = Yup.object({
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'from date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'from date')
            .typeError(PLEASE_SELECT_VALID + 'from date'),

        ToDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'to date')
            .typeError(PLEASE_SELECT_VALID + 'to date')
    });
    const CheckUserRole = () => {
        if (RETAILER === parseInt(currentUserTypeID)) {
            setIsRetailerRole(true);
        }
        if (API_USER === parseInt(currentUserTypeID)) {
            setIsRetailerRole(true);
        }
        if (CLIENT_CODE === parseInt(currentUserTypeID)) {
            setIsRetailerRole(true);
        }
    };

    useEffect(() => {
        CheckUserRole();
        createColumns();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'FirmName',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'Date'
        });

        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });

        columns.push({
            label: 'Charge',
            name: 'Charge',
            numeric: true
        });

        columns.push({
            label: 'Remark',
            name: 'Remark',
            ReadOnlyTextArea: true
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
                render={() => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Service Charge'}
                                    open={openFilter}
                                    setOpen={(value) => setopenFilter(value)}
                                >
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialDatePickerField
                                                name="FromDate"
                                                label="From Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialDatePickerField
                                                name="ToDate"
                                                label="To Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>

                                        {!isRetailerRole ? (
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
                                                    options={downline}
                                                    getOptionLabel={(option) =>
                                                        option.FirmName
                                                    }
                                                    textFieldProps={{
                                                        label: 'Select User'
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
                                                    }}
                                                />
                                            </Grid>
                                        ) : null}
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="submit"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loading}
                                            >
                                                Search
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
                            </Grid>
                        </Form>
                    );
                }}
            />

            <ReactTable
                data={transactions}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                }}
                count={totalCount}
                columns={mainColumns}
            />
        </CustomContainer>
    );
};
