import React, { useEffect, useState } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';

import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';

import { Select as USER_SLAB_MAPPING_SELECT } from 'Components/Api/UserSlabMappingApi';

import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { Select as SLAB_SELECT } from 'Components/Api/SlabApi';
import { SlabUserAdd as SLAB_USER_ADD } from 'Components/Api/UserSlabMappingApi';
import { Delete as SLABMAPPING_DELETE } from 'Components/Api/UserSlabMappingApi';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, MenuItem, Grid, Paper } from '@material-ui/core';
import { PLEASE_SELECT_VALID, RETAILER_CODE } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { useHistory } from 'react-router';

import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const SlabAssign = () => {
    const history = useHistory();
    const [SlabMapping, setSlabMapping] = useState([]);
    const classes = commonStyle();
    const [downline, setDownline] = useState([]);
    const [Slabs, setSlabs] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const CheckUserRole = () => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
        }
    };

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    var formObj = {
        SlabID: 0,
        User: {
            UserID: 0,
            FirmName: ''
        }
    };

    useEffect(() => {
        setLoadingOpen(true);
        CheckUserRole();
        //  selectUsers();
        selectSlabs();
        SelectUserSlabMapping();
        createColumns();
    }, []);

    const onDeleteClick = (item) => {
        var RequestObj = { UserSlabMappingID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };
    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await SLABMAPPING_DELETE(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        SelectUserSlabMapping();
    };

    const selectSlabs = async () => {
        const response = await SLAB_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);
        setSlabs(data);
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

    const SelectUser = async (values) => {
        if (isRetailerRole === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setDownline(objWithSelectAll);
            return;
        }

        setLoadingOpen(true);
        var request = JSON.stringify(values);

        const userResponse = await USER_SELECT_FOR_DROPDOWN(request);
        setLoadingOpen(false);
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

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            return;
        }
        var jsonReq = { SlabID: values.SlabID, UserID: values.User.UserID };

        var requestObj = JSON.stringify(jsonReq);
        setLoadingOpen(true);
        setIsError(false);
        var response = await SLAB_USER_ADD(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        setResponseMessage(response.data);
        setIsError(true);
        setSubmitting(false);

        setSeverity('success');
        if (!response.status) {
            setSeverity('error');
            return;
        }
        resetForm(formObj);
        SelectUserSlabMapping();
    };
    const SelectUserSlabMapping = async () => {
        const response = await USER_SLAB_MAPPING_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }
        var data = JSON.parse(response.data);
        setSlabMapping(data);
        setLoadingOpen(false);
    };
    const validationSchema = Yup.object({
        SlabID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Slab')
            .required(PLEASE_SELECT_VALID + 'Slab'),

        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.string()
                    .notOneOf(['0'], PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            })
    });

    const Edit = async (request) => {
        history.push('/Slabs/SlabAssignEdit/' + request);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });
        columns.push({
            label: 'User',
            name: 'FirmName'
        });
        columns.push({
            label: 'Role',
            name: 'Role'
        });
        columns.push({
            label: 'Slab',
            name: 'SlabName'
        });

        columns.push({
            label: 'Edit',
            name: 'UserSlabMappingID',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'UserSlabMappingID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
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
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
                render={(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Slab Assign
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
                                                label="Slab"
                                                name="SlabID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Slab
                                                </MenuItem>
                                                {Slabs.map((Item) => (
                                                    <MenuItem
                                                        key={Item.SlabID}
                                                        value={Item.SlabID}
                                                    >
                                                        {Item.Name}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
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
                                                title="Add"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
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
            />
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this Slab."
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

            <h2 className={classes.PaperHead_h2}>Slab and User list</h2>
            <ReactTable
                data={SlabMapping}
                columns={mainColumns}
                rowsPerPage={50}
            />
        </CustomContainer>
    );
};
