import React, { useState, useEffect } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { Container, Grid, Paper, CircularProgress } from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';
import {
    SelectSDAndDistForDropDown as USER_SD_DIST_FOR_DROPDOWN,
    SelectDownlineReport as USER_DOWNLINE,
    SelectSDAndDistForDropDownByOldUserID as USER_SDDIST_FOR_DROPDOWN_BY_OLDUSERID,
    TransferUser as TRANSFER_USERS
} from 'Components/Api/UsersApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';

const CustomContainer = HOCLoader(Container);

const UserTransfer = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [userList, setUserList] = useState([]);
    const [oldSelectedUserID, setOldSelectedUserID] = useState(0);
    const [NewUserList, setNewUserList] = useState([]);
    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    const [loading, setLoading] = useState(false);

    const [IsTransferClicked, setIsTransferClicked] = useState(false);

    const [NewParentID, setNewParentID] = useState(0);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });
    const [newUserDefault, setNewUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const [formObject] = useState({
        User: {
            User: userDefault,
            UserID: 0
        },
        NewUser: {
            NewUser: newUserDefault,
            NewUserID: 0
        }
    });
    const reset = () => {
        setUserDefault({
            UserID: 0,
            FirmName: 'Select User'
        });
        setNewUserDefault({
            UserID: 0,
            FirmName: 'Select User'
        });
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });
        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });

        columns.push({
            label: 'Role',
            name: 'Role'
        });
        columns.push({
            label: 'Balance',
            name: 'Balance',
            Wallets: true
        });
        // if (IsCashCollection === 'true') {
        //     columns.push({
        //         label: 'Outstanding',
        //         name: 'Outstanding',
        //         numeric: true
        //     });
        // }
        setMainColumns(columns);
    };

    useEffect(() => {
        createColumns();
    }, []);

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
        SelectSDAndDist(jsonObj);
    };

    const SelectSDAndDist = async (values) => {
        setLoading(true);
        var request = JSON.stringify(values);
        setLoading(false);
        const userResponse = await USER_SD_DIST_FOR_DROPDOWN(request);

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
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.number()
                    .nullable(PLEASE_SELECT_VALID + 'User')
                    .min(0, PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            }),

        NewUser: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.number()
                    .nullable(PLEASE_SELECT_VALID + 'User')
                    .min(0, PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            })
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setNewParentID(values.NewUser.UserID);
        setIsTransferClicked(true);
        setShouldGetUpdatedDataFromTable(true);
    };

    const OldUserhandleChange = async (name, value) => {
        SelectDownlineReport(value.UserID);

        setOldSelectedUserID(value.UserID);
    };

    const SelectDownlineReport = async (UserID) => {
        var inputObj = {
            UserID: UserID,
            IsExcelDownload: false
        };
        setLoading(true);
        setItemList([]);
        var requestJSON = JSON.stringify(inputObj);
        const userResponse = await USER_DOWNLINE(requestJSON);
        setLoading(false);
        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(userResponse.data);
        }
        var d = JSON.parse(userResponse.data);
        setItemList(d.data);
    };

    const NewcallSearch = async (text) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            text.length <= 0 ||
            text === newUserDefault.FirmName
        )
            return;

        var jsonObj = {
            FirmName: text,
            UserID: oldSelectedUserID
        };
        SelectSDOrDistByUserID(jsonObj);
    };

    const SelectSDOrDistByUserID = async (values) => {
        setLoading(true);
        var request = JSON.stringify(values);
        setLoading(false);
        const userResponse = await USER_SDDIST_FOR_DROPDOWN_BY_OLDUSERID(
            request
        );

        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(userResponse.data);

        let objWithSelectAll = [newUserDefault, ...data];

        setNewUserList(objWithSelectAll);
    };

    const ProcessUpdatedData = (data) => {
        setShouldGetUpdatedDataFromTable(false);
        if (IsTransferClicked) {
            TransferAllClick(data);
            return;
        }
    };

    const TransferAllClick = async (requestArr) => {
        setIsTransferClicked(false);
        if (requestArr.length <= 0) {
            setSeverity('error');
            setResponseMessage('Please select one or more users to transfer');
            setIsError(true);
            return;
        }
        let request = [];
        requestArr.forEach((element) => {
            let { value } = element[0];
            request.push({
                UserID: value,
                NewParentID: NewParentID
            });
        });
        setLoadingOpen(true);

        var RequestJson = JSON.stringify(request);

        const Response = await TRANSFER_USERS(RequestJson);

        if (!Response.isAuthorized) LOGOUT();

        if (!Response.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        setLoadingOpen(false);

        setSeverity('success');
        setResponseMessage(Response.data);
        setIsError(true);

        setItemList([]);
        createColumns();
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
                                                    label: 'Select  Old SD/Dist'
                                                }}
                                                onSearchCallback={(text) => {
                                                    callSearch(text);
                                                }}
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OldUserhandleChange(
                                                        name,
                                                        value
                                                    );

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
                                            <Field
                                                component={AsyncAutoComplete}
                                                defaultValue={newUserDefault}
                                                FieldName="NewUser"
                                                name="NewUser"
                                                SubFieldName="UserID"
                                                options={NewUserList}
                                                getOptionLabel={(option) =>
                                                    option.FirmName
                                                }
                                                textFieldProps={{
                                                    label: 'Select New SD/Dist'
                                                }}
                                                onSearchCallback={(text) => {
                                                    NewcallSearch(text);
                                                }}
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    setNewUserDefault({
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
                                                title="Transfer"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                Transfer
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
                                                    reset();
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

export default UserTransfer;
