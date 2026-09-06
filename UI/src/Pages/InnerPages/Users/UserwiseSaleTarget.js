import React, { useEffect, useState } from 'react';
import { Formik, Form, Field } from 'formik';

import * as Yup from 'yup';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { SelectDownlinForDropDown as USER_SELECT_DOWNLINE } from 'Components/Api/UsersApi';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';

import { MaterialCurrency } from 'Components/Inputes/MaterialCurrencyField';
import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
import {
    Insert as USERWISE_TARGET_ADD,
    Select as SELECT,
    Delete as Target_DELETE
} from 'Components/Api/UserwiseSaleTargetAPI';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import {
    Container,
    MenuItem,
    Grid,
    Paper,
    Button,
    CircularProgress,
    Box
} from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import InputLabel from 'Components/Inputes/InputLabel';
import HOCLoader from 'Components/Loader/HOCLoader';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';

const CustomContainer = HOCLoader(Container);

export const UserwiseSaleTarget = () => {
    const classes = commonStyle();
    const [mainColumns, setMainColumns] = useState([]);
    const [UserwiseTarget, setUserwiseTarget] = useState([]);

    const [wallets, setWallets] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [loading, setLoading] = useState(false);
    const [UserInfo, setUserInfo] = useState('');
    const [downline, setDownline] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });
    const [UserObj, setUserObj] = useState({
        UserTypeID: 0,
        ParentID: 0,
        TargetAmount: 0,
        Commission: 0,
        CommissionType: 0,
        WalletID: 0,
        User: userDefault
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
        selectDownline(jsonObj);
    };

    const selectDownline = async (values) => {
        setLoadingOpen(true);
        var request = JSON.stringify(values);
        setLoadingOpen(false);
        const response = await USER_SELECT_DOWNLINE(request);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);

        setDownline(data);
    };

    useEffect(() => {
        selectWallets();
        createColumns();
        selectUserwiseTarget();
    }, []);

    const selectUserwiseTarget = async () => {
        const response = await SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            setLoadingOpen(false);
            return;
        }

        var data = JSON.parse(response.data);
        setUserwiseTarget(data);
        setLoadingOpen(false);
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserwiseSaleTargetID',
            srno: true
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
        columns.push({
            label: 'Commission',
            name: 'Commission'
        });
        columns.push({
            label: 'CommissionType',
            name: 'CommissionType'
        });

        columns.push({
            label: 'Delete',
            name: 'UserwiseSaleTargetID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });

        setMainColumns(columns);
    };
    const deleteConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpen}
            title="Delete Confirmation"
            message="Are you sure to delete this target?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpen(false);
                Delete(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpen(false);
            }}
        />
    );

    const onDeleteClick = (item) => {
        var RequestObj = { UserwiseSaleTargetID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };
    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await Target_DELETE(jsonReq);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectUserwiseTarget();
    };
    const selectWallets = async () => {
        setLoadingOpen(true);
        var request = {
            ID: 0
        };
        var json = JSON.stringify(request);
        const response = await WALLET_SELECT(json);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setWallets(data);
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoading(true);
        setIsError(false);

        var inputObj = {
            UserID: values.User.UserID,
            WalletID: values.WalletID,
            TargetAmount: values.TargetAmount,
            Commission: values.Commission,
            CommissionType: values.CommissionType
        };

        const { isAuthorized, status, data } = await USERWISE_TARGET_ADD(
            JSON.stringify(inputObj)
        );
        setLoading(false);
        if (!isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(data);
        setIsError(true);
        if (!status) {
            setSeverity('error');
            return;
        }

        setSubmitting(false);

        resetForm(UserObj);
        setTimeout(() => {}, MESSAGE_CLOSE_AFTER);
        selectUserwiseTarget();
    };

    const validationSchema = Yup.object({
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.string()
                    .notOneOf(['0'], PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            }),
        WalletID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'walllet')
            .required(PLEASE_SELECT_VALID + 'walllet'),
        TargetAmount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'amount')
            .required(PLEASE_PROVIDE_VALID + 'amount'),
        Commission: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'commission')
            .required(PLEASE_PROVIDE_VALID + 'commission'),
        CommissionType: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'commission type')
            .required(PLEASE_SELECT_VALID + 'commission type')
    });

    const OnChangeAmount = (value, setFieldValue) => {
        if (value === undefined || value <= 0) return;

        setFieldValue('InWords', NumberToWord(value));
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
                enableReinitialize={true}
                initialValues={UserObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={UserObj}
                render={({ values, setFieldValue }) => {
                    return (
                        <Form>
                            <Paper
                                style={{ padding: 8 }}
                                className={classes.PaperStyle}
                            >
                                <Grid container spacing={2}>
                                    <Grid item xs={6} sm={6} md={6} lg={6}>
                                        <h2 className={classes.PaperHead_h2}>
                                            Userwise Target
                                        </h2>
                                    </Grid>
                                </Grid>
                                <Grid item xs={12}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
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
                                                    setUserDefault({
                                                        UserID: value.UserID,
                                                        FirmName: value.FirmName
                                                    });
                                                }}
                                            />
                                            <InputLabel
                                                color="green"
                                                value={UserInfo}
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                label="Wallet"
                                                name="WalletID"
                                                title="Select Wallet"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Wallet
                                                </MenuItem>
                                                {wallets.map((item) => (
                                                    <MenuItem
                                                        key={item.WalletID}
                                                        value={item.WalletID}
                                                    >
                                                        {item.Name}
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
                                            <MaterialCurrency
                                                name="TargetAmount"
                                                label="Target Amount"
                                                placeholder="Target Amount"
                                                autoComplete="new-password"
                                                max={100000000}
                                                onCustomCallback={(value) => {
                                                    OnChangeAmount(
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="In words"
                                                name="InWords"
                                                type="text"
                                                readOnly={true}
                                                placeholder="InWords"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialCurrency
                                                name="Commission"
                                                label="Commission"
                                                placeholder="Commission"
                                                autoComplete="new-password"
                                                max={20000000}
                                                onCustomCallback={(value) => {
                                                    OnChangeAmount(
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                label="CommissionType"
                                                name="CommissionType"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Commission Type
                                                </MenuItem>

                                                <MenuItem key="Fix" value="Fix">
                                                    Fix
                                                </MenuItem>
                                                <MenuItem
                                                    key="Flexi"
                                                    value="Flexi"
                                                >
                                                    Per
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>
                                    </Grid>
                                </Grid>
                            </Paper>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Add"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Add
                                        </Button>
                                        {loading && (
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
            />

            <h2 className={classes.PaperHead_h2}>List</h2>
            <ReactTable data={UserwiseTarget} columns={mainColumns} />

            {isAlertOpen ? deleteConfirmation : null}
        </CustomContainer>
    );
};
