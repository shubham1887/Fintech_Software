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
import { Select as USERTYPE_SELECT } from 'Components/Api/UserTypeApi';
import { VARIANT } from 'Config';
import {
    SelectByRole as SELECT,
    BulkUpdate as BULK_UPDATE_USERS
} from 'Components/Api/UsersApi';

import { Select as WALLET_SELECT } from 'Components/Api/UserBalanceCapping';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
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

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

const UserWiseBulkAction = () => {
    const classes = commonStyle();

    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState(false);
    const [userTypeList, setUserTypeList] = useState([]);
    const [IsTextValue, setIsTextValue] = useState(false);
    const [IsAutoCompleteValue, setIsAutoCompleteValue] = useState([]);
    const [IsSwitchValue, setIsSwitchValue] = useState(false);
    const [IsIsBalanceCappingValue, setIsBalanceCappingValue] = useState(false);
    const [IsOverdraft, setIsOverdraft] = useState(false);
    const [IsResponseType, setIsResponseType] = useState(false);

    const [IsLowBalance, setIsLowBalance] = useState(false);
    const [IsMpin, setIsMpin] = useState(false);
    const [IsNumberFirst, setIsNumberFirst] = useState(false);

    const [wallets, setWallets] = useState([]);
    const [CriteriaValues, setCriteriaValues] = useState('');
    const [IsUpdateClicked, setIsUpdateClicked] = useState(false);
    const [serviceList, setServiceList] = useState([]);

    var IsGSTHoldButton = localStorage.getItem('IsGSTHold');
    const [IsGSTHoldCutting, setIsGSTHoldCutting] = useState(false);
    const [IsReferralValue, setIsReferralValue] = useState(false);

    const [IsGSTType, setIsGSTType] = useState(false);
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
        selectUserTypes();
        selectService();
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
            .required(PLEASE_SELECT_VALID + 'criteria'),
        ReferralAmount: Yup.number()
            .required(PLEASE_SELECT_VALID + ' amount')
            .min(0, PLEASE_SELECT_VALID + ' amount')
            .max(100, PLEASE_SELECT_VALID + ' amount')
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
    const selectService = async () => {
        const { isAuthorized, status, data } = await SERVICE_SELECT_ALL();

        if (!isAuthorized) LOGOUT();

        if (!status) return;

        var servicedata = JSON.parse(data);

        setServiceList(servicedata);
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
            UserServiceList: [],
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
        setIsMpin(false);
        setIsNumberFirst(false);
        setIsLowBalance(false);
        setIsAutoCompleteValue(false);
        setIsBalanceCappingValue(false);
        setIsOverdraft(false);
        setIsResponseType(false);
        setIsGSTHoldCutting(false);
        setIsGSTType(false);
        setIsReferralValue(false);
    };

    const SwitchDisplay = () => {
        // setFieldValue('CriteriaValue', '');
        setIsDisplayCriteria(true);
        setIsMpin(false);
        setIsNumberFirst(false);
        setIsLowBalance(false);
        setIsSwitchValue(true);
        setIsTextValue(false);
        setIsAutoCompleteValue(false);
        setIsBalanceCappingValue(false);
        setIsOverdraft(false);
        setIsResponseType(false);
        setIsGSTHoldCutting(false);
        setIsGSTType(false);
        setIsReferralValue(false);
    };
    const AutoCompleteDisplay = (setFieldValue) => {
        setFieldValue('UserServiceList', []);

        setIsMpin(false);
        setIsNumberFirst(false);
        setIsLowBalance(false);
        setIsAutoCompleteValue(true);
        setIsSwitchValue(false);
        setIsTextValue(false);
        setIsBalanceCappingValue(false);
        setIsOverdraft(false);
        setIsResponseType(false);
        setIsGSTHoldCutting(false);
        setIsGSTType(false);
        setIsReferralValue(false);
    };
    const BalanceCappingDisplay = () => {
        setIsMpin(false);
        setIsNumberFirst(false);
        setIsLowBalance(false);
        setIsBalanceCappingValue(true);
        setIsAutoCompleteValue(false);
        setIsSwitchValue(false);
        setIsTextValue(false);
        selectWallets();
        setIsOverdraft(false);
        setIsResponseType(false);
        setIsGSTHoldCutting(false);
        setIsGSTType(false);
        setIsReferralValue(false);
    };
    const OverdraftDisplay = (setFieldValue) => {
        setFieldValue('CriteriaText', '');
        setIsMpin(false);
        setIsNumberFirst(false);
        setIsLowBalance(false);
        setIsResponseType(false);
        setIsOverdraft(true);
        setIsBalanceCappingValue(false);
        setIsAutoCompleteValue(false);
        setIsSwitchValue(false);
        setIsTextValue(false);
        setIsGSTHoldCutting(false);
        selectWallets();
        setIsGSTType(false);
        setIsReferralValue(false);
    };

    const ResponseTypeDisplay = () => {
        setIsMpin(false);
        setIsNumberFirst(false);
        setIsLowBalance(false);
        setIsResponseType(true);
        setIsOverdraft(false);
        setIsBalanceCappingValue(false);
        setIsAutoCompleteValue(false);
        setIsSwitchValue(false);
        setIsTextValue(false);
        setIsGSTHoldCutting(false);
        selectWallets();
        setIsGSTType(false);
        setIsReferralValue(false);
    };

    const GSTHoldCuttigDisplay = () => {
        setIsMpin(false);
        setIsNumberFirst(false);
        setIsLowBalance(false);
        setIsGSTHoldCutting(true);
        setIsResponseType(false);
        setIsOverdraft(false);
        setIsBalanceCappingValue(false);
        setIsAutoCompleteValue(false);
        setIsSwitchValue(false);
        setIsTextValue(false);
        setIsGSTType(false);
        setIsReferralValue(false);
    };
    const MPinDisplay = () => {
        setIsMpin(true);
        setIsSwitchValue(true);
        setIsNumberFirst(false);
        setIsLowBalance(false);
        setIsGSTHoldCutting(false);
        setIsResponseType(false);
        setIsOverdraft(false);
        setIsBalanceCappingValue(false);
        setIsAutoCompleteValue(false);
        setIsTextValue(false);
        setIsGSTType(false);
        setIsReferralValue(false);
    };

    const LowBalanceDisplay = () => {
        setIsLowBalance(true);
        selectWallets();
        setIsMpin(false);
        setIsNumberFirst(false);
        setIsGSTHoldCutting(false);
        setIsResponseType(false);
        setIsOverdraft(false);
        setIsBalanceCappingValue(false);
        setIsAutoCompleteValue(false);
        setIsSwitchValue(false);
        setIsTextValue(false);
        setIsGSTType(false);
        setIsReferralValue(false);
    };
    const NumberFirstDisplay = () => {
        setIsNumberFirst(true);
        setIsSwitchValue(true);
        setIsTextValue(false);
        setIsMpin(false);
        setIsLowBalance(false);
        setIsGSTHoldCutting(false);
        setIsResponseType(false);
        setIsOverdraft(false);
        setIsBalanceCappingValue(false);
        setIsAutoCompleteValue(false);
        setIsGSTType(false);
        setIsReferralValue(false);
    };
    const GSTTypeDisplay = () => {
        setIsGSTType(true);
        setIsNumberFirst(false);
        setIsSwitchValue(false);
        setIsTextValue(false);
        setIsMpin(false);
        setIsLowBalance(false);
        setIsGSTHoldCutting(false);
        setIsResponseType(false);
        setIsOverdraft(false);
        setIsBalanceCappingValue(false);
        setIsAutoCompleteValue(false);
        setIsReferralValue(false);
    };

    const ReferralAmountDisplay = () => {
        setIsReferralValue(true);
        setIsMpin(false);
        setIsNumberFirst(false);
        setIsLowBalance(false);
        setIsBalanceCappingValue(false);
        setIsAutoCompleteValue(false);
        setIsSwitchValue(false);
        setIsTextValue(false);

        setIsOverdraft(false);
        setIsResponseType(false);
        setIsGSTHoldCutting(false);
        setIsGSTType(false);
    };
    const Reset = () => {
        setIsReferralValue(false);
        setIsMpin(false);
        setIsNumberFirst(false);
        setIsLowBalance(false);
        setIsBalanceCappingValue(false);
        setIsAutoCompleteValue(false);
        setIsSwitchValue(false);
        setIsTextValue(false);
        setIsOverdraft(false);
        setIsResponseType(false);
        setIsGSTHoldCutting(false);
        setIsGSTType(false);
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
                enableReinitialize={true}
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
                                                    setCriteriaTextLabel(value);
                                                    setIsDisplayCriteria(true);
                                                }}
                                            >
                                                <MenuItem key={0} value={0}>
                                                    Selection Criteria
                                                </MenuItem>
                                                <MenuItem
                                                    key={1}
                                                    value={'Deactivate'}
                                                    onClick={() => {
                                                        SwitchDisplay();
                                                    }}
                                                >
                                                    Deactivate
                                                </MenuItem>
                                                <MenuItem
                                                    key={2}
                                                    value={'IsDelete'}
                                                    onClick={() => {
                                                        SwitchDisplay();
                                                    }}
                                                >
                                                    Delete
                                                </MenuItem>
                                                <MenuItem
                                                    key={3}
                                                    value={'IsLoginOTP'}
                                                    onClick={() => {
                                                        SwitchDisplay();
                                                    }}
                                                >
                                                    LoginOTP
                                                </MenuItem>

                                                <MenuItem
                                                    key={'3IsTwoFA'}
                                                    value={'IsTwoFA'}
                                                    onClick={() => {
                                                        SwitchDisplay();
                                                    }}
                                                >
                                                    TwoFA
                                                </MenuItem>

                                                <MenuItem
                                                    key={4}
                                                    value={
                                                        'IsInvoiceApplicable'
                                                    }
                                                    onClick={() => {
                                                        SwitchDisplay();
                                                    }}
                                                >
                                                    InvoiceApplicable
                                                </MenuItem>

                                                <MenuItem
                                                    key={5}
                                                    value={'AllowRealAPI'}
                                                    onClick={() => {
                                                        SwitchDisplay();
                                                    }}
                                                >
                                                    AllowRealAPI
                                                </MenuItem>
                                                <MenuItem
                                                    key={6}
                                                    value={'IsChannel'}
                                                    onClick={() => {
                                                        SwitchDisplay();
                                                    }}
                                                >
                                                    IsChannel
                                                </MenuItem>
                                                <MenuItem
                                                    key={7}
                                                    value={'PayoutAccountCount'}
                                                    onClick={() => {
                                                        TextDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    PayoutAccountCount
                                                </MenuItem>
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
                                                    key={10}
                                                    value={'IsCommissionHold'}
                                                    onClick={() => {
                                                        SwitchDisplay();
                                                    }}
                                                >
                                                    IsCommissionHold
                                                </MenuItem>

                                                <MenuItem
                                                    key={11}
                                                    value={'Referral'}
                                                    onClick={() => {
                                                        ReferralAmountDisplay();
                                                    }}
                                                >
                                                    Referral
                                                </MenuItem>

                                                <MenuItem
                                                    key={12}
                                                    value={'AddServices'}
                                                    onClick={() => {
                                                        AutoCompleteDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    Add Services
                                                </MenuItem>

                                                <MenuItem
                                                    key={13}
                                                    value={'RemoveServices'}
                                                    onClick={() => {
                                                        AutoCompleteDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    Remove Services
                                                </MenuItem>

                                                <MenuItem
                                                    key={14}
                                                    value={
                                                        'BalanceCappingAmount'
                                                    }
                                                    onClick={() => {
                                                        BalanceCappingDisplay();
                                                    }}
                                                >
                                                    Balance Capping Amount
                                                </MenuItem>

                                                <MenuItem
                                                    key={15}
                                                    value={'Overdraft'}
                                                    onClick={() => {
                                                        OverdraftDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    Overdraft
                                                </MenuItem>
                                                <MenuItem
                                                    key={16}
                                                    value={'ResponseType'}
                                                    onClick={() => {
                                                        ResponseTypeDisplay();
                                                    }}
                                                >
                                                    Response Type
                                                </MenuItem>

                                                <MenuItem
                                                    key={17}
                                                    value={'LowBalance'}
                                                    onClick={() => {
                                                        LowBalanceDisplay();
                                                    }}
                                                >
                                                    Low Balance Alert
                                                </MenuItem>

                                                <MenuItem
                                                    key={18}
                                                    value={'NumberFirst'}
                                                    onClick={() => {
                                                        NumberFirstDisplay();
                                                    }}
                                                >
                                                    NumberFirst
                                                </MenuItem>

                                                <MenuItem
                                                    key={19}
                                                    value={'IsMpin'}
                                                    onClick={() => {
                                                        MPinDisplay();
                                                    }}
                                                >
                                                    Need MPin
                                                </MenuItem>
                                                <MenuItem
                                                    key={20}
                                                    value={'GSTType'}
                                                    onClick={() => {
                                                        GSTTypeDisplay();
                                                    }}
                                                >
                                                    GSTType
                                                </MenuItem>

                                                <MenuItem
                                                    key={'IsReferralPopup'}
                                                    value={'IsReferralPopup'}
                                                    onClick={() => {
                                                        SwitchDisplay();
                                                    }}
                                                >
                                                    IsReferralPopup
                                                </MenuItem>

                                                {IsGSTHoldButton === 'true' ? (
                                                    <MenuItem
                                                        key={21}
                                                        value={'GSTHoldCutting'}
                                                        onClick={() => {
                                                            GSTHoldCuttigDisplay();
                                                        }}
                                                    >
                                                        GSTHold Cutting
                                                    </MenuItem>
                                                ) : null}
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
                                                        />
                                                    ) : null}
                                                    {IsReferralValue ===
                                                    true ? (
                                                        <MaterialNumberField
                                                            label="Referral Amount"
                                                            name="ReferralAmount"
                                                            type="text"
                                                            placeholder="Enter Referral Amount"
                                                        />
                                                    ) : null}

                                                    {IsAutoCompleteValue ===
                                                    true ? (
                                                        <Grid
                                                            item
                                                            className={
                                                                classes.multiSelect
                                                            }
                                                        >
                                                            <FieldArray
                                                                name="UserServiceList"
                                                                render={(
                                                                    serviceIdList
                                                                ) => (
                                                                    <Autocomplete
                                                                        multiple
                                                                        id="ServiceData"
                                                                        options={
                                                                            serviceList
                                                                        }
                                                                        getOptionLabel={(
                                                                            option
                                                                        ) =>
                                                                            option.ServiceName
                                                                        }
                                                                        onChange={(
                                                                            event,
                                                                            values,
                                                                            setFieldValue
                                                                        ) => {
                                                                            values.UserServiceList = [];
                                                                            values.map(
                                                                                (
                                                                                    category
                                                                                ) => {
                                                                                    serviceIdList.push(
                                                                                        {
                                                                                            ServiceID:
                                                                                                category.ServiceID
                                                                                        }
                                                                                    );
                                                                                    values.UserServiceList.push(
                                                                                        {
                                                                                            ServiceID:
                                                                                                category.ServiceID
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
                                                                                name="UserServiceList"
                                                                                label="Service(s)"
                                                                                placeholder="Service"
                                                                            />
                                                                        )}
                                                                    />
                                                                )}
                                                            />
                                                        </Grid>
                                                    ) : null}
                                                    {IsIsBalanceCappingValue ||
                                                    IsLowBalance ? (
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
                                                    {IsResponseType ? (
                                                        <MaterialSelect
                                                            label="ResponseType"
                                                            name="ResponseType"
                                                            title="ResponseType"
                                                        >
                                                            <MenuItem
                                                                key="ALL"
                                                                value="ALL"
                                                            >
                                                                Select Type
                                                            </MenuItem>
                                                            <MenuItem
                                                                key="Initial"
                                                                value="Initial"
                                                            >
                                                                Initial
                                                            </MenuItem>
                                                            <MenuItem
                                                                key="Callback"
                                                                value="Callback"
                                                            >
                                                                Callback
                                                            </MenuItem>
                                                        </MaterialSelect>
                                                    ) : null}
                                                    {IsOverdraft ? (
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

                                                    {IsGSTHoldCutting ? (
                                                        <MaterialSelect
                                                            label="GSTHoldCutting"
                                                            name="GSTHoldCutting"
                                                            title="GSTHoldCutting"
                                                        >
                                                            <MenuItem
                                                                key="Monthly"
                                                                value="Monthly"
                                                            >
                                                                Monthly
                                                            </MenuItem>
                                                            <MenuItem
                                                                key="Daily"
                                                                value="Daily"
                                                            >
                                                                Daily
                                                            </MenuItem>
                                                        </MaterialSelect>
                                                    ) : null}

                                                    {IsGSTType ? (
                                                        <>
                                                            <MaterialSelect
                                                                label="GSTType"
                                                                name="GSTType"
                                                                title="GSTType"
                                                            >
                                                                <MenuItem
                                                                    key="Inclusion"
                                                                    value="Inclusion"
                                                                >
                                                                    Inclusion
                                                                </MenuItem>
                                                                <MenuItem
                                                                    key="Exclusion"
                                                                    value="Exclusion"
                                                                >
                                                                    Exclusion
                                                                </MenuItem>
                                                            </MaterialSelect>
                                                            <label
                                                                style={{
                                                                    margin:
                                                                        '10px'
                                                                }}
                                                            >
                                                                This principles
                                                                are applicable
                                                                for only P2A
                                                                Service.
                                                            </label>
                                                        </>
                                                    ) : null}
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    {IsReferralValue ===
                                                    true ? (
                                                        <MaterialNumberField
                                                            label="Transaction Amount For Referral Benefit"
                                                            name="ReferralBenefitMinAmount"
                                                            type="text"
                                                            placeholder="Transaction Amount For Referral Benefit "
                                                        />
                                                    ) : null}

                                                    {IsLowBalance ? (
                                                        <MaterialNumberField
                                                            label={
                                                                CriteriaTextLabel
                                                            }
                                                            name="CriteriaText"
                                                            type="text"
                                                            placeholder="Low Balance Amount"
                                                        />
                                                    ) : null}

                                                    {IsIsBalanceCappingValue ? (
                                                        <MaterialNumberField
                                                            label={
                                                                CriteriaTextLabel
                                                            }
                                                            name="CriteriaText"
                                                            type="text"
                                                            placeholder="Balance Capping Amount"
                                                        />
                                                    ) : null}
                                                    {IsOverdraft === true ? (
                                                        <MaterialNumberField
                                                            label={
                                                                CriteriaTextLabel
                                                            }
                                                            name="CriteriaText"
                                                            type="text"
                                                            placeholder="Max Overdraft Amount"
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

export default UserWiseBulkAction;
