import React, { useState, useEffect, useRef } from 'react';
import {
    Container,
    Grid,
    MenuItem,
    Paper,
    CircularProgress
} from '@material-ui/core';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import {
    Insert,
    Select,
    Delete as DeleteSetting,
    SelectByID,
    Update
} from 'Components/Api/UserwisePaymentGatewayAPI';

import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';

import ReactTable from 'Components/ReactTable/ReactTable';
import {
    PLEASE_SELECT_VALID,
    RETAILER_CODE,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import HOCLoader from 'Components/Loader/HOCLoader';

import { Select as SELECT_PAYMENTGATEWAY } from 'Components/Api/PaymentGatewaySettingAPI';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';

import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
const CustomContainer = HOCLoader(Container);

export const UserwisePaymentGateway = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [downline, setDownline] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [PaymentGatewayList, setPaymentGatewayList] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const CheckUserRole = () => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
        }
    };
    const [
        userWisePayementGatewaySettingID,
        setuserWisePayementGatewaySettingID
    ] = useState(0);

    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [IsUpdate, setIsUpdate] = useState(false);
    var [FormObj, setFormObj] = useState({
        PaymentGatewaySettingID: 0,
        User: {
            UserID: 0,
            FirmName: 'All User'
        },
        MinimumAmount: 0,
        MaximumAmount: 0,
        IsUpdate: false,
        UserWisePayementGatewaySettingID: userWisePayementGatewaySettingID
    });
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const selectPaymentGateway = async () => {
        const response = await SELECT_PAYMENTGATEWAY();
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setPaymentGatewayList(data);
    };

    const SelectData = async () => {
        setItemList([]);
        setLoadingOpen(true);
        var response = await Select(JSON.stringify());

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) return;
        var data = JSON.parse(response.data);
        setItemList(data);
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
        SelectUser(jsonObj);
    };

    const SelectUser = async () => {
        if (isRetailerRole === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setDownline(objWithSelectAll);
            return;
        }
        const response = await USER_SELECT_FOR_DROPDOWN();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);

        let objWithSelectAll = [userDefault, ...data];
        setDownline(objWithSelectAll);
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserWisePayementGatewaySettingID',
            srno: true
        });

        columns.push({
            label: 'User',
            name: 'FirmName'
        });

        columns.push({
            label: 'PaymentGateway',
            name: 'PaymentGateway'
        });
        columns.push({
            label: 'Min.Amount',
            name: 'MinimumAmount'
        });
        columns.push({
            label: 'Max.Amount',
            name: 'MaximumAmount'
        });

        columns.push({
            label: 'Edit',
            name: 'UserWisePayementGatewaySettingID',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });

        columns.push({
            label: 'Delete',
            name: 'UserWisePayementGatewaySettingID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });
        setMainColumns(columns);
    };

    const Edit = async (UserWisePayementGatewaySettingID) => {
        setIsUpdate(true);

        selectByID(UserWisePayementGatewaySettingID);
    };

    const selectByID = async (UserWisePayementGatewaySettingID) => {
        let JSONObj = {
            UserWisePayementGatewaySettingID: UserWisePayementGatewaySettingID
        };
        setuserWisePayementGatewaySettingID(UserWisePayementGatewaySettingID);
        var request = JSON.stringify(JSONObj);
        setLoadingOpen(true);
        const response = await SelectByID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setIsUpdate(true);

        setUserDefault({
            UserID: data.UserID,
            FirmName: data.FirmName
        });

        setFormObj({
            User: {
                UserID: data.UserID,
                FirmName: data.FirmName
            },
            PaymentGatewaySettingID: data.PaymentGatewaySettingID,
            MinimumAmount: data.MinimumAmount,
            MaximumAmount: data.MaximumAmount
        });
    };

    const onDeleteClick = (UserWisePayementGatewaySettingID) => {
        var RequestObj = {
            UserWisePayementGatewaySettingID: UserWisePayementGatewaySettingID
        };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };
    const Delete = async (request) => {
        var requestObj = JSON.stringify(request);

        setIsError(false);

        setLoadingOpen(true);
        const response = await DeleteSetting(requestObj);

        if (!response.isAuthorized) {
            LOGOUT();
            return;
        }

        setResponseMessage(response.data);
        setSeverity(response.status ? 'success' : 'error');
        setLoadingOpen(false);
        setIsError(true);

        SelectData();
    };
    const DeleteConfirmationModal = isAlertOpen ? (
        <ConfirmAlert
            isOpen={isAlertOpen}
            title="Delete Confirmation"
            message="Are you sure to delete userwise gateway."
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpen(false);
                Delete(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpen(false);
            }}
        />
    ) : null;

    useEffect(() => {
        CheckUserRole();
        createColumns();
        selectPaymentGateway();
        SelectData();
    }, []);

    const FormValidationSchema = Yup.object({
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.string()
                    .notOneOf(['0'], PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            }),

        PaymentGatewaySettingID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'PaymentGateway')
            .required(PLEASE_SELECT_VALID + 'PaymentGateway'),

        MinimumAmount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'Minimum 1  amount')
            .required(PLEASE_PROVIDE_VALID + 'minimum amount'),
        MaximumAmount: Yup.number()
            .moreThan(9, PLEASE_PROVIDE_VALID + 'amount more than 9')
            .required(PLEASE_PROVIDE_VALID + 'maximum amount')
    });
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }

        let request = {
            UserID: values.User.UserID,
            PaymentGatewaySettingID: values.PaymentGatewaySettingID,
            UserWisePayementGatewaySettingID: userWisePayementGatewaySettingID,
            MinimumAmount: values.MinimumAmount,
            MaximumAmount: values.MaximumAmount
        };

        var requestObj = JSON.stringify(request);
        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);

        var response = IsUpdate
            ? await Update(requestObj)
            : await Insert(requestObj);

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

        setUserDefault({
            UserID: 0,
            FirmName: 'All User'
        });

        resetForm(FormObj);
        setFormObj({
            PaymentGatewaySettingID: 0,
            MinimumAmount: 0,
            MaximumAmount: 0
            // User: {
            //     UserID: 0,
            //     FirmName: 'All User'
            // }
        });

        SelectData();
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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Userwise Payment Gateway
                            </h2>

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
                                            <Field
                                                component={AsyncAutoComplete}
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
                                                onSearchCallback={(text) => {
                                                    callSearch(text);
                                                }}
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    if (value === undefined) {
                                                        return;
                                                    }

                                                    if (value === null) {
                                                        return;
                                                    }
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
                                            <MaterialSelect
                                                label="PaymentGateway"
                                                name="PaymentGatewaySettingID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    All
                                                </MenuItem>
                                                {PaymentGatewayList.map(
                                                    (
                                                        {
                                                            PaymentGatewaySettingID,
                                                            Name
                                                        },
                                                        index
                                                    ) => (
                                                        <MenuItem
                                                            key={index}
                                                            value={
                                                                PaymentGatewaySettingID
                                                            }
                                                        >
                                                            {Name}
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
                                            <MaterialNumberField
                                                label="Minimum Amount"
                                                name="MinimumAmount"
                                                type="text"
                                                placeholder="Minimum Amount"
                                                length={7}
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
                                                label="Maximum Amount"
                                                name="MaximumAmount"
                                                type="text"
                                                placeholder="Maximum Amount"
                                                length={7}
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
                                                title={
                                                    IsUpdate ? 'Update' : 'Add'
                                                }
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                {IsUpdate ? 'Update' : 'Add'}
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
            <h2 className={classes.PaperHead_h2}>
                Userwise Payment Gateway List
            </h2>
            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={100}
                headerToolbar={false}
            />

            {DeleteConfirmationModal}
        </CustomContainer>
    );
};
