import React, { useState, useEffect } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Box,
    Button,
    Paper,
    MenuItem,
    CircularProgress
} from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';
import { SelectForSaleTarget as USERTYPE_SELECT_FOR_SaleTarget } from 'Components/Api/UserTypeApi';
import {
    SaleTargetSelect as SALE_TARGET_SELECT,
    TransferCommission as TRANSFER_COMMISSION
} from 'Components/Api/UserwiseSaleTargetAPI';

import { SelectDownlinForDropDown as USER_SELECT_DOWNLINE } from 'Components/Api/UsersApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { GET_Month_FOR_GST } from 'Components/Helper/Messages';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import ResetIcon from 'Components/SvgButtons/ResetIcon';
import { DISTRIBUTOR, SUPER_DISTRIBUTOR } from 'Components/Helper/UsertypeCode';
const CustomContainer = HOCLoader(Container);

export const UserwiseSaleTargetReport = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [userList, setUserList] = useState([]);
    const [userTypeList, setUserTypeList] = useState([]);

    const [MonthValue, setMonthValue] = useState(0);

    const [selectedUserTypeID, setSelectedUserTypeID] = useState(0);
    const [
        IsTransferCommissionClicked,
        setIsTransferCommissionClicked
    ] = useState(false);

    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const [Months] = useState([
        { Key: 1, Value: 'January' },
        { Key: 2, Value: 'February' },
        { Key: 3, Value: 'March' },
        { Key: 4, Value: 'April' },
        { Key: 5, Value: 'May' },
        { Key: 6, Value: 'June' },
        { Key: 7, Value: 'July' },
        { Key: 8, Value: 'August' },
        { Key: 9, Value: 'September' },
        { Key: 10, Value: 'October' },
        { Key: 11, Value: 'November' },
        { Key: 12, Value: 'December' }
    ]);
    const MonthItems = Months.map(({ Key, Value }, index) => (
        <MenuItem key={index} value={Key}>
            {Value}
        </MenuItem>
    ));
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });

        columns.push({
            label: 'WalletID',
            name: 'WalletID',
            hide: true
        });

        columns.push({
            label: 'SaleDOC',
            name: 'SaleMonthDOC'
        });
        columns.push({
            label: 'UserName',
            name: 'UserName'
        });

        columns.push({
            label: 'Wallet',
            name: 'Wallet'
        });
        var textwrapWidth = 60;
        columns.push({
            label: 'TargetAmount',
            name: 'TargetAmount',
            numeric: true,
            textwrapWidth
        });

        if (
            parseInt(selectedUserTypeID) === DISTRIBUTOR ||
            parseInt(selectedUserTypeID) === SUPER_DISTRIBUTOR
        ) {
            columns.push({
                label: 'TransferAmount',
                name: 'SaleAmount',
                numeric: true,
                textwrapWidth
            });
        } else {
            columns.push({
                label: 'SaleAmount',
                name: 'SaleAmount',
                numeric: true,
                textwrapWidth
            });
        }

        columns.push({
            label: 'Diff',
            name: 'Difference',
            numeric: true,
            textwrapWidth
        });

        columns.push({
            label: 'Commission',
            name: 'Commission'
        });
        columns.push({
            label: 'Type',
            name: 'CommissionType'
        });
        columns.push({
            label: 'Status',
            name: 'Status'
        });
        setMainColumns(columns);
    };

    useEffect(() => {
        selectUserTypes();
        createColumns();
    }, []);
    const [formObject] = useState({
        UserTypeID: 0,
        User: {
            UserID: 0,
            FirmName: 'All User'
        },
        Month: GET_Month_FOR_GST()
    });
    const selectUserTypes = async () => {
        const {
            isAuthorized,
            status,
            data
        } = await USERTYPE_SELECT_FOR_SaleTarget();

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
            FirmName: text,
            UserTypeID: selectedUserTypeID
        };
        SelectDownline(jsonObj);
    };

    const SelectDownline = async (values) => {
        setLoadingOpen(true);
        var request = JSON.stringify(values);
        setLoadingOpen(false);
        const userResponse = await USER_SELECT_DOWNLINE(request);

        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(userResponse.data);

        let objWithSelectAll = [userDefault, ...data];

        setUserList(objWithSelectAll);
    };

    const FormValidationSchema = Yup.object({
        UserTypeID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'role')
            .required(PLEASE_SELECT_VALID + 'role'),

        Month: Yup.string()
            .nullable(PLEASE_SELECT_VALID + 'month')
            .required(PLEASE_SELECT_VALID + 'month'),

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

    const handleSubmit = async (Values) => {
        setMonthValue(Values.Month);
        setItemList([]);
        setMainColumns([]);

        var inputObj = {
            Month: Values.Month,
            UserTypeID: Values.UserTypeID
        };
        let requestJSON = JSON.stringify(inputObj);

        const { data, status, isAuthorized } = await SALE_TARGET_SELECT(
            requestJSON
        );

        setLoadingOpen(false);
        if (!isAuthorized) LOGOUT();
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);
            return;
        }

        var d = JSON.parse(data);
        createColumns();

        setItemList(d.list);
    };

    const ProcessUpdatedData = (data) => {
        setShouldGetUpdatedDataFromTable(false);
        if (IsTransferCommissionClicked) {
            TransferAllClick(data);
            return;
        }
    };

    const handleTransferCommissionClick = (e) => {
        setShouldGetUpdatedDataFromTable(true);
        setIsTransferCommissionClicked(true);
    };

    const TransferAllClick = async (requestArr) => {
        setIsTransferCommissionClicked(false);

        if (requestArr.length <= 0) {
            //setIsError(true);
            setResponseMessage('Please select one or more users to transfer');
            // setSeverity('error');

            return;
        }

        let request = [];
        requestArr.forEach((element) => {
            let { value } = element[0];

            request.push({
                UserID: value,
                WalletID: element[1].value,
                SaleAmount: element[6].value,
                SaleMonthDOC: element[2].value,
                Month: MonthValue
            });
        });
        setLoadingOpen(true);

        var RequestJson = JSON.stringify(request);

        const Response = await TRANSFER_COMMISSION(RequestJson);

        if (!Response.isAuthorized) LOGOUT();
        setLoadingOpen(false);
        if (!Response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(Response.data);
            return;
        }

        setIsError(false);
        setSeverity('success');
        setResponseMessage(Response.data);

        setItemList([]);
        createColumns();
    };

    const OnRoleChange = async (name, value) => {
        setSelectedUserTypeID(value);
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
                {() => {
                    return (
                        <Form>
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
                                                label="Month"
                                                name="Month"
                                                title="Month"
                                            >
                                                {MonthItems}
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
                                            <Field
                                                component={AsyncAutoComplete}
                                                defaultValue={userDefault}
                                                FieldName="User"
                                                name="User"
                                                SubFieldName="UserID"
                                                options={userList}
                                                getOptionLabel={(option) =>
                                                    option.FirmName
                                                }
                                                textFieldProps={{
                                                    label: 'Select User'
                                                }}
                                                onSearchCallback={(text) => {
                                                    callSearch(text);
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
                                            md={2}
                                            lg={2}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Search"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                                onClick={(values) => {
                                                    handleSubmit(values);
                                                }}
                                            >
                                                Search
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
                                            md={2}
                                            lg={2}
                                        >
                                            <PrimaryButton
                                                type="button"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                style={{
                                                    background: 'green',
                                                    marginLeft: '32px'
                                                }}
                                                disabled={loadingOpen}
                                                title="Transfer Comm"
                                                onClick={
                                                    handleTransferCommissionClick
                                                }
                                            >
                                                Transfer Comm
                                            </PrimaryButton>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div
                                        className={classes.SubmitWrapper}
                                    ></div>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                headerToolbar={false}
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
