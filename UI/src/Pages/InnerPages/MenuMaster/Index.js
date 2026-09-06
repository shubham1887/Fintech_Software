import React, { memo, useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import ReactTable from 'Components/ReactTable/ReactTable';
import {
    Container,
    Grid,
    Paper,
    Button,
    CircularProgress,
    Box,
    MenuItem
} from '@material-ui/core';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import {
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import {
    Insert as INSERT,
    Update as UPDATE,
    SelectAll as SELECT_MENU,
    SelectByID as SELECT_BY_ID,
    Delete as DELETE,
    SelectParentMenuDropDown as PARENT_MENU_DROPDOWN
} from 'Components/Api/MenuMasterApi';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

const MenuAdd = memo(() => {
    const classes = commonStyle();
    const [MenuMasterID, setMenuMasterID] = useState(0);

    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [itemList, setItemList] = useState([]);
    const [parentMenuList, setParentMenuList] = useState([]);
    const [IsUpdate, setIsUpdate] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);

    const [CustomContainerProps, setCustomContainerProps] = useState({
        loadingOpen: false,
        isError: false,
        severity: 'success',
        responseMessage: ''
    });

    const updateCustomContainerProps = (key, value) => {
        setCustomContainerProps((props) => ({
            ...props,
            [key]: value
        }));
    };

    const [MenuObj, setMenuObj] = useState({
        MenuMasterID: MenuMasterID,
        Name: '',
        Url: '',
        Icon: '',
        Description: '',
        MenuSequence: 0,
        Level: 0,
        ParentMenuID: 0,
        IsAdmin: false,
        IsSuperDistributer: false,
        IsDistributer: false,
        IsRetailer: false,
        IsAPIUser: false,
        IsSupport: false
    });

    const MenuValidationSchema = Yup.object({
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + ' name'),
        Url: Yup.string().required(PLEASE_PROVIDE_VALID + ' url'),
        Icon: Yup.string().required(PLEASE_PROVIDE_VALID + ' icon'),
        Description: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' description'
        )
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (CustomContainerProps.loading) {
            updateCustomContainerProps('loadingOpen', false);
            return;
        }

        var requestObj = JSON.stringify(values);

        updateCustomContainerProps('isError', false);
        updateCustomContainerProps('loadingOpen', true);

        var response = IsUpdate
            ? await UPDATE(requestObj)
            : await INSERT(requestObj);

        if (!response.isAuthorized) LOGOUT();

        setCustomContainerProps({
            loadingOpen: false,
            isError: true,

            responseMessage: response.data
        });

        if (!response.status) {
            updateCustomContainerProps('severity', 'error');
            return;
        }
        updateCustomContainerProps('severity', 'success');

        resetForm(MenuObj);
        reset();

        setTimeout(() => {
            select();
            selectParentMenuDropdown();
        }, MESSAGE_CLOSE_AFTER);
    };
    const reset = () => {
        setIsUpdate(false);
        setMenuObj({
            MenuMasterID: 0,
            Name: '',
            Url: '',
            Icon: '',
            Description: '',
            MenuSequence: 0,
            Level: 0,
            ParentMenuID: 0,
            IsAdmin: false,
            IsSuperDistributer: false,
            IsDistributer: false,
            IsRetailer: false,
            IsAPIUser: false,
            IsSupport: false
        });
    };
    useEffect(() => {
        updateCustomContainerProps('loadingOpen', true);
        selectParentMenuDropdown();
        select();
        createColumns();
    }, []);

    const selectParentMenuDropdown = async () => {
        updateCustomContainerProps('loadingOpen', true);
        const response = await PARENT_MENU_DROPDOWN();
        updateCustomContainerProps('loadingOpen', false);

        const { isAuthorized, status, data } = response;
        if (!isAuthorized) LOGOUT();

        if (!status) {
            setCustomContainerProps({
                isError: true,
                severity: 'error',
                responseMessage: response.data
            });
            return;
        }
        var dataList = JSON.parse(data);

        setParentMenuList(dataList);
    };

    const select = async () => {
        updateCustomContainerProps('loadingOpen', true);
        const response = await SELECT_MENU();
        updateCustomContainerProps('loadingOpen', false);

        const { isAuthorized, status, data } = response;
        if (!isAuthorized) LOGOUT();

        if (!status) {
            setCustomContainerProps({
                isError: true,
                severity: 'error',
                responseMessage: response.data
            });
            return;
        }
        var dataList = JSON.parse(data);

        setItemList(dataList);
    };
    const createColumns = () => {
        var columns = [];
        var primaryKeyIndex = 0;
        columns.push({
            label: '#',
            name: 'MenuMasterID',
            srno: true
        });

        columns.push({
            label: 'Name',
            name: 'Name'
        });

        columns.push({
            label: 'Url',
            name: 'Url'
        });
        columns.push({
            label: 'Description',
            name: 'Description'
        });

        columns.push({
            label: 'Edit',
            name: 'MenuMasterID',
            edit: true,
            callback: (MenuMasterID) => {
                Edit(MenuMasterID);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'MenuMasterID',
            delete: true,
            callback: (row) => {
                onDeleteClick(row[0]);
            }
        });
        setMainColumns(columns);
    };
    const onDeleteClick = (item) => {
        var RequestObj = { MenuMasterID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };
    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);

        updateCustomContainerProps('loadingOpen', true);
        const response = await DELETE(jsonReq);

        updateCustomContainerProps('loadingOpen', false);
        updateCustomContainerProps('isError', true);

        if (!response.isAuthorized) LOGOUT();

        setCustomContainerProps({
            loadingOpen: false,
            isError: true,
            responseMessage: response.data
        });

        setTimeout(() => {
            select();
        }, MESSAGE_CLOSE_AFTER);
    };
    const Edit = (MenuMasterID) => {
        setIsUpdate(true);

        selectByMenuMasterID(MenuMasterID);
    };
    const selectByMenuMasterID = async (MenuMasterID) => {
        setCustomContainerProps({
            isError: false,
            loading: true
        });

        let jsonObj = {
            MenuMasterID: MenuMasterID
        };
        const response = await SELECT_BY_ID(JSON.stringify(jsonObj));
        setCustomContainerProps({
            loading: false
        });
        if (!response.isAuthorized) LOGOUT();

        const { data, status } = response;
        if (!status) {
            setCustomContainerProps({
                ErrorMeeage: data,
                isError: true,
                severity: 'error'
            });
        }

        let d = JSON.parse(data);
        setMenuObj({
            MenuMasterID: d.MenuMasterID,
            Name: d.Name,
            Url: d.Url,
            Icon: d.Icon,
            Description: d.Description,
            IsAdmin: d.IsAdmin,
            IsSuperDistributer: d.IsSuperDistributer,
            IsDistributer: d.IsDistributer,
            IsRetailer: d.IsRetailer,
            IsAPIUser: d.IsAPIUser,
            IsSupport: d.IsSupport,
            MenuSequence: d.MenuSequence,
            Level: d.Level,
            ParentMenuID: d.ParentMenuID
        });
    };

    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
    };

    const MenuDropdownList = parentMenuList.map(
        ({ MenuMasterID, Name }, index) => (
            <MenuItem key={index} value={MenuMasterID}>
                {Name}
            </MenuItem>
        )
    );
    return (
        <CustomContainer
            isLoading={CustomContainerProps.loadingOpen}
            isError={CustomContainerProps.isError}
            alertMessage={CustomContainerProps.responseMessage}
            severity={CustomContainerProps.severity}
            onCloseCallback={(alertState) => {
                updateCustomContainerProps('isError', alertState);
            }}
        >
            <Formik
                enableReinitialize={true}
                initialValues={MenuObj}
                validationSchema={MenuValidationSchema}
                onSubmit={handleSubmit}
                resetForm={MenuObj}
            >
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>Menu </h2>

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
                                            <MaterialTextField
                                                label="Name"
                                                name="Name"
                                                type="text"
                                                placeholder="Name"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="Url"
                                                name="Url"
                                                type="text"
                                                placeholder="Url"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="Icon"
                                                name="Icon"
                                                type="text"
                                                placeholder="Icon"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="Description"
                                                name="Description"
                                                type="text"
                                                placeholder="Description"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="MenuSequence"
                                                name="MenuSequence"
                                                type="text"
                                                placeholder="MenuSequence"
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialTextField
                                                label="Level"
                                                name="Level"
                                                type="text"
                                                placeholder="Level"
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
                                                label="Parent Menu"
                                                name="ParentMenuID"
                                                title="Select Parent Menu"
                                            >
                                                <MenuItem key="0" value={0}>
                                                    Select Menu
                                                </MenuItem>
                                                {MenuDropdownList}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid item xs={6} sm={6} md={2} lg={2}>
                                            <FormikMaterialSwitch
                                                label="IsAdmin"
                                                name="IsAdmin"
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
                                        </Grid>

                                        <Grid item xs={6} sm={6} md={2} lg={2}>
                                            <FormikMaterialSwitch
                                                label="IsSuperDistributer"
                                                name="IsSuperDistributer"
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
                                        </Grid>
                                        <Grid item xs={6} sm={6} md={2} lg={2}>
                                            <FormikMaterialSwitch
                                                label="IsDistributer"
                                                name="IsDistributer"
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
                                        </Grid>

                                        <Grid item xs={6} sm={6} md={2} lg={2}>
                                            <FormikMaterialSwitch
                                                label="IsRetailer"
                                                name="IsRetailer"
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
                                        </Grid>
                                        <Grid item xs={6} sm={6} md={2} lg={2}>
                                            <FormikMaterialSwitch
                                                label="IsAPIUser"
                                                name="IsAPIUser"
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
                                        </Grid>
                                        <Grid item xs={6} sm={6} md={2} lg={2}>
                                            <FormikMaterialSwitch
                                                label="IsSupport"
                                                name="IsSupport"
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
                                        </Grid>
                                        <Grid item xs={6} sm={6} md={2} lg={2}>
                                            <PrimaryButton
                                                type="submit"
                                                title={
                                                    IsUpdate ? 'Update' : 'Save'
                                                }
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={
                                                    CustomContainerProps.loading
                                                }
                                            >
                                                {IsUpdate ? 'Update' : 'Save'}
                                            </PrimaryButton>
                                            {CustomContainerProps.loading && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </Grid>
                                        <Grid item xs={6} sm={6} md={2} lg={2}>
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

            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this Menu?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Delete(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}

            <h2 className={classes.PaperHead_h2}>Menu Master</h2>
            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={100}
            />
        </CustomContainer>
    );
});

export default MenuAdd;
