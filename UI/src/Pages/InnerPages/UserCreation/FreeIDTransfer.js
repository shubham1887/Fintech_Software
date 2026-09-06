import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';

import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

import {
    FreeTransfer as FREE_TRANSACTION,
    Select as SELECT
} from 'Components/Api/UserCreationMasterAPI';

import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectRetailerAndAPIUserForDropDown as RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectOnlyDistAndSuperDist as USER_TYPE_SELECT } from 'Components/Api/UserTypeApi';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { CONSUMER_NUMBER_FONT_SIZE } from 'Config';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const FreeIDTransfer = () => {
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
    const [consumerNumberFontSize, setConsumerNumberFontSize] = useState('');
    async function fetchConsumerNumberFontSize() {
        setConsumerNumberFontSize(await CONSUMER_NUMBER_FONT_SIZE());
    }
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const [ModelObj, setModelObj] = useState({
        PurchaseCount: 0,
        User: userDefault
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            UserID: values.User.UserID,
            PurchaseCount: values.PurchaseCount
        };
        var requestObj = JSON.stringify(inputObj);
        setIsError(false);
        setLoading(true);
        var response = await FREE_TRANSACTION(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        reset();
        selectData();
    };
    const validationSchema = Yup.object({
        PurchaseCount: Yup.number().min(
            2,
            PLEASE_PROVIDE_VALID + `minimum 2 Count`
        ),
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.number()
                    .nullable(PLEASE_SELECT_VALID + 'User')
                    .min(0, PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            })
    });

    useEffect(() => {
        fetchConsumerNumberFontSize();
        selectUserTypes();
        selectData();
        createColumns();
    }, []);
    const reset = () => {
        setModelObj({
            PurchaseCount: 0,
            User: userDefault
        });
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

    const selectData = async () => {
        setItemList([]);
        const { isAuthorized, status, data } = await SELECT();

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

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserCreationMasterID',
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
            label: 'Op.Id',
            name: 'OpeningIdCount'
        });
        columns.push({
            label: 'Count',
            name: 'PurchaseCount'
        });

        columns.push({
            label: 'Cl.IdC',
            name: 'ClosingIdCount'
        });

        columns.push({
            label: 'Op.Bal',
            name: 'OpeningBalance'
        });
        columns.push({
            label: 'Amount',
            name: 'Amount'
        });
        columns.push({
            label: 'Cl.Bal',
            name: 'ClosingBalance'
        });

        setMainColumns(columns);
    };

    const TransferForm = ({ values, errors, setFieldValue }) => {
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
                <Grid item xs={12} sm={12} md={6} lg={4}>
                    <Form style={{ marginBottom: '15px' }}>
                        <Grid item xs={12}>
                            <Paper>
                                <CustomAccordion
                                    title={'Transfer ID '}
                                    open={openFilter}
                                    setOpen={(value) => setopenFilter(value)}
                                >
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
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
                                            md={12}
                                            lg={12}
                                        >
                                            <Field
                                                component={AsyncAutoComplete}
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
                                                onSearchCallback={(text) => {
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
                                                        UserID: value.UserID,
                                                        FirmName: value.FirmName
                                                    });
                                                }}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <MaterialNumberField
                                                font_size={
                                                    consumerNumberFontSize
                                                }
                                                label="ID Count"
                                                name="PurchaseCount"
                                                type="text"
                                                placeholder="ID Count"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            spacing={2}
                                            container
                                            className={classes.footerGrid}
                                        >
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={6}
                                                lg={6}
                                            >
                                                <SecondaryButton
                                                    variant="contained"
                                                    color="secondary"
                                                    type="reset"
                                                    title="Reset"
                                                    onClick={() => {
                                                        reset();
                                                    }}
                                                    className={
                                                        classes.CustomDangerButtonStyle
                                                    }
                                                >
                                                    Reset
                                                </SecondaryButton>
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={6}
                                                lg={6}
                                            >
                                                <PrimaryButton
                                                    type="submit"
                                                    title="Transfer"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    disabled={loading}
                                                >
                                                    Purchase
                                                </PrimaryButton>
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                </CustomAccordion>
                            </Paper>
                        </Grid>
                    </Form>
                </Grid>
                <Grid
                    item
                    xs={12}
                    sm={12}
                    md={6}
                    lg={8}
                    className="lastrecharges"
                >
                    <h2 className={classes.PaperHead_h2}>Last 10 Transfer</h2>
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
                </Grid>
            </Grid>
        );
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
                key={'formTransfer'}
                enableReinitialize={true}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={ModelObj}
            >
                {TransferForm}
            </Formik>
        </CustomContainer>
    );
};
