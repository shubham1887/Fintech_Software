import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import {
    Insert as INSERT,
    Select as USER_SELECT,
    Update as USER_UPDATE,
    Delete as USER_DELETE,
    ActiveDeactive as ACTIVATE_DEACTIVE,
    AllActiveDeactive as All_ACTIVE_DEACTIVATE
} from 'Components/Api/UserWisePlanLimitSettingAPI';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, Grid, MenuItem, Button } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import ReactTable from 'Components/ReactTable/ReactTable';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import Box from '@material-ui/core/Box';

import { SelectOnlyRetailerAndAPI as USER_TYPE_SELECT } from 'Components/Api/UserWisePlanLimitSettingAPI';
import { SelectRetailerAndAPIUserForDropDown as RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';

import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const UserWisePlanLimitSetting = () => {
    const formRef = useRef();

    const [userTypeList, setUserTypeList] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [mainColumns, setMainColumns] = useState([]);
    const [openFilter, setOpenFilter] = useState(true);
    const [itemList, setItemList] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(false);
    const [IsAllServicesActive, setIsAllServicesActive] = useState(true);
    const [IsAllServicesDeActive, setIsAllServicesDeActive] = useState(false);
    const [transactions, setTransactions] = useState([]);
    const [currentPage, setCurrentPage] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [downLine, setDownLine] = useState([]);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });

    const [formObj] = useState({
        ServiceAndUserwiseSubscriptionID: 0,
        User: userDefault,
        UserTypeID: 0,
        CircleHitCount: 0,
        PlanHitCount: 0,
        BillFetchHitCount: 0
    });

    const formValidationSchema = Yup.object({
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

    const reset = () => {
        setUserDefault({
            UserID: 0,
            FirmName: 'Select User'
        });
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }

    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let inputObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            UserID: formRef.current.values.User.UserID,

            IsExcelDownload: formRef.current.values.IsExcelDownload
        };
        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await USER_SELECT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        if (data.IsExcelDownload) {
            formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'Service Subscription Charge Report');
        }

        setTransactions(data.list);
        setTotalCount(data.Count);
        reset();
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
        var jsonReq = {
            UserTypeID: values.UserTypeID,
            FirmName: values.FirmName
        };
        var request = JSON.stringify(jsonReq);
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
        createColumns();
        callSelectData(TakeCount, SkipCount, '', '', '');
    }, []);

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;
        var jsonReq = {
            UserID: values.User.UserID,
            UserTypeID: values.UserTypeID,
            CircleHitCount: values.CircleHitCount,
            PlanHitCount: values.PlanHitCount,
            BillFetchHitCount: values.BillFetchHitCount
        };

        setIsError(false);
        var requestObj = JSON.stringify(jsonReq);
        setLoadingOpen(true);

        var response = await INSERT(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const UpdateClick = async (row) => {
        setIsError(false);
        let jsonObj = {
            UserWisePlanLimitID: row[0],
            CircleHitCount: row[2],
            PlanHitCount: row[3],
            BillFetchHitCount: row[4]
        };
        var errorValidation = false;

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
        callSelectData(TakeCount, SkipCount, '', '', '');
        // selectData();
    };

    const Delete = async (UserWisePlanLimitID) => {
        let jsonObj = {
            UserWisePlanLimitID: UserWisePlanLimitID
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
        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const activate = async (item, value) => {
        var jsonObj = {
            UserWisePlanLimitID: item,
            IsActive: value
        };

        var request = JSON.stringify(jsonObj);

        setLoading(true);
        setIsError(false);
        const response = await ACTIVATE_DEACTIVE(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        if (!response.status) setSeverity('error');
        setIsError(true);
        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const allServicesActive = async () => {
        var jsonObj = {
            IsActive: IsAllServicesActive
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await All_ACTIVE_DEACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        if (!response.status) {
            setSeverity('error');
        }
        if (response.status) {
            setSeverity('success');
        }
        setResponseMessage(response.data);
        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const allServicesDeactivate = async () => {
        var jsonObj = {
            IsActive: IsAllServicesDeActive
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await All_ACTIVE_DEACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        if (!response.status) {
            setSeverity('error');
        }
        if (response.status) {
            setSeverity('success');
        }
        setResponseMessage(response.data);
        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const UserTypeList = userTypeList.map((item) => (
        <MenuItem key={item.UserTypeID} value={item.UserTypeID}>
            {item.UserType}
        </MenuItem>
    ));
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserWisePlanLimitID',
            srno: true
        });

        columns.push({
            label: 'User',
            name: 'FirmName'
        });

        columns.push({
            label: 'CircleHitCount',
            name: 'CircleHitCount',
            editable: true
        });

        columns.push({
            label: 'PlanHitCount',
            name: 'PlanHitCount',
            editable: true
        });

        columns.push({
            label: 'BillFetchHitCount',
            name: 'BillFetchHitCount',
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
            label: 'Active',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                activate(id, value);
            }
        });

        columns.push({
            label: 'Delete',
            name: 'UserWisePlanLimitID',
            delete: true,
            callback: (UserWisePlanLimitID) => {
                Delete(UserWisePlanLimitID);
            }
        });
        setMainColumns(columns);
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
                initialValues={formObj}
                validationSchema={formValidationSchema}
                onSubmit={handleSubmit}
                resetForm={formObj}
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'User Wise Plan Limit'}
                                    open={openFilter}
                                    setOpen={(value) => setOpenFilter(value)}
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
                                            xs={10}
                                            sm={10}
                                            md={2}
                                            lg={2}
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
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialNumberField
                                                label="Circle Count"
                                                name="CircleHitCount"
                                                type="text"
                                                placeholder="Circle Count"
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
                                                label="Plan Count"
                                                name="PlanHitCount"
                                                type="text"
                                                placeholder="Plan Count"
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
                                                label="Bill Fetch Count"
                                                name="BillFetchHitCount"
                                                type="text"
                                                placeholder="Bill Fetch Count"
                                            />
                                        </Grid>
                                        <Grid
                                            container
                                            spacing={2}
                                            style={{
                                                justifyContent: 'flex-end',
                                                display: 'flex'
                                            }}
                                        >
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                                style={{
                                                    justifyContent: 'flex-end',
                                                    display: 'flex'
                                                }}
                                            >
                                                <Grid
                                                    container
                                                    spacing={2}
                                                    style={{
                                                        justifyContent: 'end'
                                                    }}
                                                >
                                                    <Grid
                                                        item
                                                        xs={10}
                                                        sm={10}
                                                        md={1}
                                                        lg={1}
                                                    >
                                                        <Box
                                                            style={{
                                                                textAlign:
                                                                    'center'
                                                            }}
                                                        >
                                                            <Icon
                                                                className={
                                                                    classes.CustomImageButtonStyle
                                                                }
                                                                disabled={
                                                                    loading
                                                                }
                                                                classes={{
                                                                    root:
                                                                        classes.iconRoot
                                                                }}
                                                                onClick={(
                                                                    e
                                                                ) => {
                                                                    setFieldValue(
                                                                        'IsExcelDownload',
                                                                        true
                                                                    );
                                                                    callSelectData(
                                                                        TakeCount,
                                                                        SkipCount,
                                                                        '',
                                                                        '',
                                                                        ''
                                                                    );
                                                                    //handleSubmit(e);
                                                                }}
                                                            >
                                                                <img
                                                                    alt=""
                                                                    className={
                                                                        classes.imageIcon
                                                                    }
                                                                    src={excel}
                                                                />
                                                            </Icon>
                                                        </Box>
                                                    </Grid>{' '}
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={1}
                                                        lg={1}
                                                    >
                                                        <PrimaryButton
                                                            type="submit"
                                                            title="Add"
                                                            className={
                                                                classes.CustomPrimaryButtonStyle
                                                            }
                                                            disabled={
                                                                loadingOpen
                                                            }
                                                        >
                                                            Add
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
                                                            variant="contained"
                                                            color="secondary"
                                                            title="Reset"
                                                            className={
                                                                classes.CustomDangerButtonStyle
                                                            }
                                                            onClick={() => {
                                                                reset();
                                                            }}
                                                        >
                                                            Reset
                                                        </SecondaryButton>
                                                    </Grid>
                                                </Grid>
                                            </Grid>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                            style={{
                                                justifyContent: 'flex-end',
                                                display: 'flex'
                                            }}
                                        >
                                            <p style={{ marginTop: '14px' }}>
                                                Activate/Deactivate in Bulk?
                                            </p>
                                            <Button
                                                variant="contained"
                                                color="primary"
                                                style={{ margin: '0 10px' }}
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={() => {
                                                    allServicesActive();
                                                    setIsAllServicesActive(
                                                        true
                                                    );
                                                }}
                                            >
                                                Active
                                            </Button>{' '}
                                            <Button
                                                variant="contained"
                                                color="primary"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={() => {
                                                    allServicesDeactivate();
                                                    setIsAllServicesDeActive(
                                                        false
                                                    );
                                                }}
                                            >
                                                DeActive
                                            </Button>
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
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={TakeCount}
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
                getSearchCallback={({ SearchText }) => {
                    callSelectData(TakeCount, currentPage, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    callSelectData(
                        TakeCount,
                        currentPage,
                        SearchText,
                        column,
                        direction
                    );
                }}
            />
        </CustomContainer>
    );
};
