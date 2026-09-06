import React, { useState, useEffect } from 'react';
import { Formik, Form, Field } from 'formik';
import { FETCH_LIST } from 'Config';
import InputLabel from 'Components/Inputes/InputLabel';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { SelectByWalletIDAndReceiverID as WALLET_INFO } from 'Components/Api/WalletApi';
import {
    Select as WALLET_SELECT,
    Insert as DEFAULTAMOUNT_INSERT,
    SelectBalanceCapping as USERWISE_SELECT,
    Update as USERWISE_UPDATE,
    Delete as USERWISE_DELETE
} from 'Components/Api/UserBalanceCapping';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import {
    Container,
    Grid,
    Box,
    Paper,
    Button,
    MenuItem,
    CircularProgress
} from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';

const CustomContainer = HOCLoader(Container);

export const BalanceCappingUserwise = () => {
    const classes = commonStyle();
    const [downline, setDownline] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [IsFetchAllList, setisFetchAllList] = useState(false);
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [wallets, setWallets] = useState([]);
    const [IsUserwise, setIsUserwise] = useState(true);
    const [UserInfo, setUserInfo] = useState('');
    const [walletID, setWalletID] = useState(0);
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        values.UserID = values.User.UserID;

        if (values.IsUserwise === false) {
            var errorValidation = false;
            await DefaultValidationSchema.validate(values).catch(function (
                err
            ) {
                errorValidation = true;
                setSeverity('error');
                setResponseMessage(err.message);
                setIsError(true);
                return;
            });
            if (errorValidation === true) {
                return;
            }
        }

        if (values.IsUserwise === true) {
            await UserwiseValidationSchema.validate(values).catch(function (
                err
            ) {
                errorValidation = true;
                setSeverity('error');
                setResponseMessage(err.message);
                setIsError(true);
                return;
            });
            if (errorValidation === true) {
                return;
            }
        }

        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);
        setSubmitting(false);
        setIsError(false);
        setLoading(true);

        var response = await DEFAULTAMOUNT_INSERT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setSubmitting(true);
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setSeverity('error');
            setIsError(true);
            return;
        }
        selectData();
        resetForm(FormObj);
        reset();
    };
    const reset = () => {
        setUserInfo('');
        setUserDefault({
            UserID: 0,
            FirmName: 'Select User'
        });
        setFormObj({
            WalletID: 0,
            Amount: '0',
            UserwiseAmount: '0',
            IsUserwise: IsUserwise
        });
    };

    const DefaultValidationSchema = Yup.object({
        WalletID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'walllet')
            .required(PLEASE_SELECT_VALID + 'walllet'),
        Amount: Yup.number()
            .min(10, PLEASE_PROVIDE_VALID + 'minimum 10 amount')
            .required(PLEASE_SELECT_VALID + 'Amount')
    });

    const UserwiseValidationSchema = Yup.object({
        WalletID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'walllet')
            .required(PLEASE_SELECT_VALID + 'walllet'),

        UserwiseAmount: Yup.number()
            .min(10, PLEASE_PROVIDE_VALID + 'minimum 10 amount')
            .required(PLEASE_SELECT_VALID + 'Amount'),

        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.string()
                    .notOneOf(['0'], PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            })
    });

    const selectWallets = async () => {
        const response = await WALLET_SELECT();
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) return;
        var data = JSON.parse(response.data);

        setWallets(data);
    };

    const selectData = async () => {
        //  var requestObj = JSON.stringify(value);
        setItemList([]);
        const { isAuthorized, status, data } = await USERWISE_SELECT();

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

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const fetchUserList = async () => {
        let listValue = await FETCH_LIST();

        if (listValue === undefined || listValue === null || listValue === '') {
            return;
        }

        if (listValue !== undefined && listValue === 'All') {
            setisFetchAllList(true);
            SelectUser('');
            return;
        }
    };

    const callSearch = async (text) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            //  text.length <= 0 ||
            text === userDefault.FirmName
        )
            return;

        var jsonObj = {
            FirmName: text
        };
        SelectUser(jsonObj);
    };

    const SelectUser = async (values) => {
        setLoading(true);
        var request = JSON.stringify(values);
        setLoading(false);
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

    const [FormObj, setFormObj] = useState({
        WalletID: 0,
        Amount: '0',
        UserwiseAmount: '0',
        IsUserwise: IsUserwise,
        User: {
            UserID: 0,
            FirmName: 'Select User'
        }
    });

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserBalanceCappingID',
            srno: true
        });

        columns.push({
            label: 'User',
            name: 'UserName'
        });
        columns.push({
            label: 'Wallet',
            name: 'Wallet'
        });

        columns.push({
            label: 'Amount',
            name: 'Amount',
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
            name: 'UserBalanceCappingID',
            delete: true,
            callback: (UserBalanceCappingID) => {
                Delete(UserBalanceCappingID);
            }
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        fetchUserList();
        createColumns();
        selectWallets();
        selectData();
    }, []);
    const updateWalletID = async (value) => {
        setWalletID(value);
    };
    const updateUserID = async (value, setFieldValue) => {
        setFieldValue('UserID', value.UserID);
        setUserDefault({
            UserID: value.UserID,
            FirmName: value.FirmName
        });
        await selectDownlineBalanceInformation(value.UserID, setFieldValue);
    };

    const selectDownlineBalanceInformation = async (
        ReceiverID,
        setFieldValue
    ) => {
        if (ReceiverID <= 0) return;

        var request = {
            WalletID: walletID,
            ReceiverID: ReceiverID
        };

        setLoading(true);
        var json = JSON.stringify(request);

        const response = await WALLET_INFO(json);

        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }

        const { Outstanding, BalanceDetail } = JSON.parse(response.data);
        setUserInfo(BalanceDetail);
        setFieldValue('Info', 'OS:' + Outstanding);
    };
    const WalletList = wallets.map(({ WalletID, Name }) => (
        <MenuItem key={WalletID} value={WalletID}>
            {Name}
        </MenuItem>
    ));
    const UpdateValidation = Yup.object({
        UserBalanceCappingID: Yup.number().required(PLEASE_PROVIDE_VALID + 'ID')
    });
    const UpdateClick = async (row) => {
        setIsError(false);
        let jsonObj = {
            UserBalanceCappingID: row[0],
            Amount: row[3]
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
        const response = await USERWISE_UPDATE(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        selectData();
    };
    const Delete = async (UserBalanceCappingID) => {
        let jsonObj = {
            UserBalanceCappingID: UserBalanceCappingID
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setSeverity('');
        setResponseMessage('');

        setLoading(true);
        const response = await USERWISE_DELETE(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        selectData();
    };
    const UploadForm = ({ setFieldValue }) => {
        return (
            <Form
                style={{
                    marginBottom: '15px'
                }}
            >
                <Grid item xs={12}>
                    <Paper className={classes.PaperStyle}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    name="WalletID"
                                    label="Wallet"
                                    title="Wallet"
                                    onCustomCallback={(name, value) => {
                                        updateWalletID(value);
                                    }}
                                >
                                    <MenuItem key="0" value="0">
                                        Select Wallet
                                    </MenuItem>
                                    {WalletList}
                                </MaterialSelect>
                            </Grid>

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <Field
                                    component={AsyncAutoComplete}
                                    defaultValue={userDefault}
                                    FieldName="User"
                                    name="User"
                                    SubFieldName="UserID"
                                    options={downline}
                                    getOptionLabel={(option) => option.FirmName}
                                    textFieldProps={{
                                        label: 'Select User'
                                    }}
                                    onSearchCallback={(text) => {
                                        callSearch(text);
                                    }}
                                    onCustomCallback={(name, value) => {
                                        updateUserID(value, setFieldValue);
                                    }}
                                />
                                <InputLabel color="green" value={UserInfo} />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialNumberField
                                    label=" Balance Capping Amount"
                                    name="UserwiseAmount"
                                    type="text"
                                    placeholder="Balance Capping Amount"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <PrimaryButton
                                    type="submit"
                                    title="Save Userwise"
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={loading}
                                    onClick={(e) => {
                                        setFieldValue('IsUserwise', true);
                                    }}
                                >
                                    Save Userwise
                                </PrimaryButton>
                                {loading && (
                                    <CircularProgress
                                        size={24}
                                        className={classes.buttonProgress}
                                    />
                                )}
                            </Grid>
                        </Grid>
                    </Paper>
                </Grid>
            </Form>
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
                enableReinitialize={true}
                initialValues={FormObj}
                //validationSchema={FormValidationSchema}
                onSubmit={handleSubmit}
                resetForm={FormObj}
            >
                {UploadForm}
            </Formik>

            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
