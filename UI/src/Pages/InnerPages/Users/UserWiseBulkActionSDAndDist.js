import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Paper,
    CircularProgress,
    MenuItem
} from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import { BulkUpdate as BULK_UPDATE_USERS } from 'Components/Api/UsersApi';

import { Select as WALLET_SELECT } from 'Components/Api/UserBalanceCapping';

import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { SelectDirectDownlineForUserBulkSelect as SELECT } from 'Components/Api/UsersApi';
import { SelectV1 as USERTYPE_SELECT } from 'Components/Api/UsersApi';
import { SelectDownlineReport as SELECT_DOWNLINE_REPORT } from 'Components/Api/UsersApi';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

const UserWiseBulkActionSDAndDist = () => {
    const classes = commonStyle();

    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState(false);
    const [CriteriaValueLabel, setCriteriaValueLabel] = useState(false);
    const [IsTextValue, setIsTextValue] = useState(false);
    const [userTypeList, setUserTypeList] = useState([]);

    const [IsSwitchValue, setIsSwitchValue] = useState(false);

    const [IsLowBalance, setIsLowBalance] = useState(false);

    const [wallets, setWallets] = useState([]);
    const [CriteriaValues, setCriteriaValues] = useState('');
    const [IsUpdateClicked, setIsUpdateClicked] = useState(false);

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

        setMainColumns(columns);
    };

    const selectWallets = async () => {
        const response = await WALLET_SELECT();
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) return;
        var data = JSON.parse(response.data);

        setWallets(data);
    };

    const WalletList = wallets.map(({ WalletID, Name }) => (
        <MenuItem key={WalletID} value={WalletID}>
            {Name}
        </MenuItem>
    ));
    useEffect(() => {
        createColumns();
        //selectAllUsers();
        selectUserTypes();
    }, []);

    const [formObject, setFormObject] = useState({
        UserTypeID: 0,
        WalletID: 0,
        Criteria: 0,
        CriteriaText: '',
        CriteriaValue: false,
        UserServiceList: [],
        GSTHoldCutting: 'Monthly',
        IsTextValue: false,
        IsSwitchValue: false,
        IsResponseType: false,
        GSTType: '',

        ReferralAmount: 0,
        ReferralBenefitMinAmount: 0
    });

    const FormValidationSchema = Yup.object({
        Criteria: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'criteria')
            .required(PLEASE_SELECT_VALID + 'criteria')
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
    };

    const OnChange = (name, value, setFieldValue) => {
        // setFieldValue(name, value);
    };

    const ProcessUpdatedData = (data) => {
        setShouldGetUpdatedDataFromTable(false);
        if (IsUpdateClicked) {
            UpdateBulkClick(data);
            return;
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
                Criteria: CriteriaValues.Criteria,
                CriteriaText: CriteriaValues.CriteriaText,
                IsTextValue: IsTextValue,
                CriteriaValue: CriteriaValues.CriteriaValue,
                UserServiceList: CriteriaValues.UserServiceList,
                WalletID: CriteriaValues.WalletID,
                ResponseType: CriteriaValues.ResponseType,
                GSTHoldCutting: CriteriaValues.GSTHoldCutting,
                GSTType: CriteriaValues.GSTType,
                ReferralAmount: CriteriaValues.ReferralAmount,
                ReferralBenefitMinAmount:
                    CriteriaValues.ReferralBenefitMinAmount
            });
        });

        setLoadingOpen(true);
        var RequestJson = JSON.stringify(request);
        setIsError(false);

        const Response = await BULK_UPDATE_USERS(RequestJson);

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
        setIsDisplayCriteria(false);
        setFormObject({
            ...formObject,
            UserTypeID: 0,
            Criteria: 0,
            CriteriaText: '',
            CriteriaValue: false
        });

        setItemList([]);
        createColumns();

        //
    };

    const TextDisplay = (setFieldValue) => {
        setFieldValue('CriteriaText', '');
        setIsTextValue(true);
        setIsSwitchValue(false);

        setIsLowBalance(false);
    };

    // const SwitchDisplay = () => {
    //     setIsLowBalance(false);
    //     setIsSwitchValue(true);
    //     setIsTextValue(false);
    // };

    const LowBalanceDisplay = (setFieldValue) => {
        setFieldValue('CriteriaValuet', '');
        setIsLowBalance(true);
        selectWallets();

        setIsSwitchValue(false);
        setIsTextValue(false);
    };
    const selectAllUsers = async () => {
        setItemList([]);

        var inputObj = {
            UserID: 0,
            IsExcelDownload: false
        };

        var requestJSON = JSON.stringify(inputObj);

        setLoadingOpen(true);
        const response = await SELECT_DOWNLINE_REPORT(requestJSON);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setItemList(data.data);
    };
    const Reset = () => {
        setIsLowBalance(false);

        setIsSwitchValue(false);
        setIsTextValue(false);
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
        setItemList([]);
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
                {({ setFieldValue }) => {
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
                                                label="Selection criteria"
                                                name="Criteria"
                                                title="criteria"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    setCriteriaValueLabel(
                                                        value
                                                    );
                                                    setCriteriaTextLabel(value);
                                                    setIsDisplayCriteria(true);
                                                }}
                                            >
                                                <MenuItem key={0} value={0}>
                                                    Selection Criteria
                                                </MenuItem>

                                                {/* <MenuItem
                                                    key={3}
                                                    value={'IsLoginOTP'}
                                                    onClick={() => {
                                                        SwitchDisplay();
                                                    }}
                                                >
                                                    LoginOTP
                                                </MenuItem> */}

                                                <MenuItem
                                                    key={8}
                                                    value={'MaxOutstanding'}
                                                    onClick={() => {
                                                        TextDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    MaxOutstanding
                                                </MenuItem>

                                                <MenuItem
                                                    key={9}
                                                    value={
                                                        'CommissionOnTransfer'
                                                    }
                                                    onClick={() => {
                                                        TextDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    CommissionOnTransfer
                                                </MenuItem>

                                                <MenuItem
                                                    key={16}
                                                    value={'LowBalance'}
                                                    onClick={() => {
                                                        LowBalanceDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    Low Balance Alert
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>

                                        {!IsDisplayCriteria ? null : (
                                            <>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    {IsSwitchValue === true ? (
                                                        <FormikMaterialSwitch
                                                            label={
                                                                CriteriaTextLabel
                                                            }
                                                            name="CriteriaValue"
                                                            onCustomCallback={(
                                                                name,
                                                                value
                                                            ) => {
                                                                OnChange(
                                                                    name,
                                                                    value,
                                                                    setFieldValue
                                                                );
                                                            }}
                                                        />
                                                    ) : null}

                                                    {IsTextValue === true ? (
                                                        <MaterialTextField
                                                            label={
                                                                CriteriaTextLabel
                                                            }
                                                            name="CriteriaText"
                                                            type="text"
                                                            placeholder={
                                                                'Enter ' +
                                                                CriteriaTextLabel
                                                            }
                                                            length={30}
                                                        />
                                                    ) : null}

                                                    {IsLowBalance ? (
                                                        <MaterialSelect
                                                            name="WalletID"
                                                            label="Wallet"
                                                            title="Wallet"
                                                        >
                                                            <MenuItem
                                                                key="0"
                                                                value="0"
                                                            >
                                                                Select Wallet
                                                            </MenuItem>
                                                            {WalletList}
                                                        </MaterialSelect>
                                                    ) : null}
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    {IsLowBalance ? (
                                                        <MaterialNumberField
                                                            label={
                                                                CriteriaValueLabel
                                                            }
                                                            name="CriteriaValue"
                                                            type="text"
                                                            placeholder="Low Balance Amount"
                                                            OnChange={() => {
                                                                Reset();
                                                            }}
                                                        />
                                                    ) : null}
                                                </Grid>
                                            </>
                                        )}

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
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
                                            md={2}
                                            lg={2}
                                        >
                                            <SecondaryButton
                                                type="reset"
                                                title="Reset"
                                                className={
                                                    classes.CustomDangerButtonStyle
                                                }
                                                onClick={() => {
                                                    Reset();
                                                }}
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

export default UserWiseBulkActionSDAndDist;
