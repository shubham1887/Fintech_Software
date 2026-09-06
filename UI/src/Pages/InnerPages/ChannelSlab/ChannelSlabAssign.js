import React, { useEffect, useState } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';

import { Select as USER_SLAB_MAPPING_SELECT } from 'Components/Api/UserChannelSlabMappingApi';
import { SelectChannelUserForDropDown as USER_SELECT_LIST } from 'Components/Api/UsersApi';
import { Select as SLAB_SELECT } from 'Components/Api/ChannelSlabAPI';
import { SlabUserAdd as SLAB_USER_ADD } from 'Components/Api/UserChannelSlabMappingApi';
import { Delete as SLABMAPPING_DELETE } from 'Components/Api/UserChannelSlabMappingApi';

import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, MenuItem, Grid, Paper, Button } from '@material-ui/core';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { useHistory } from 'react-router';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const ChannelSlabAssign = () => {
    const history = useHistory();
    const [SlabMapping, setSlabMapping] = useState([]);
    const classes = commonStyle();
    const [UsersList, setUsers] = useState([]);

    const [Slabs, setSlabs] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [UserDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    var formObj = {
        ChannelSlabMasterID: 0,
        UserChannelSlabMappingID: 0,
        User: {
            UserID: 0,
            FirmName: ''
        }
    };

    useEffect(() => {
        setLoadingOpen(true);
        selectUsers();
        selectSlabs();
        SelectUserSlabMapping();
        createColumns();
    }, []);

    const onDeleteClick = (item) => {
        var RequestObj = { UserChannelSlabMappingID: item };
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

    const selectUsers = async () => {
        const response = await USER_SELECT_LIST();

        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);

        let objWithSelectAll = [UserDefault, ...data];

        setUsers(objWithSelectAll);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            return;
        }
        var jsonReq = {
            ChannelSlabMasterID: values.ChannelSlabMasterID,
            UserID: values.User.UserID
        };

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
        ChannelSlabMasterID: Yup.string()
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
        history.push('/ChannelSlabs/AssignEdit/' + request);
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
            name: 'UserChannelSlabMappingID',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'UserChannelSlabMappingID',
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
                                                component={AutoCompleteTextView}
                                                defaultValue={UserDefault}
                                                FieldName="User"
                                                name="User"
                                                options={UsersList}
                                                getOptionLabel={(option) =>
                                                    option.FirmName
                                                }
                                                getOptionSelected={(
                                                    option,
                                                    value
                                                ) =>
                                                    option.UserID ===
                                                    value.UserID
                                                }
                                                textFieldProps={{
                                                    label: 'User'
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
                                                name="ChannelSlabMasterID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Slab
                                                </MenuItem>
                                                {Slabs.map((Item) => (
                                                    <MenuItem
                                                        key={
                                                            Item.ChannelSlabMasterID
                                                        }
                                                        value={
                                                            Item.ChannelSlabMasterID
                                                        }
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
