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
import { Select as USERTYPE_SELECT } from 'Components/Api/UserTypeApi';
import {
    SelectDeletedUsers as SELECT_DELETEDUSERS_REPORT,
    UserRevert as USER_REVERT
} from 'Components/Api/UsersApi';

import ReactTable from 'Components/ReactTable/ReactTable';

import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

const DeletedUsers = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const [userTypeList, setUserTypeList] = useState([]);

    const createColumns = () => {
        var columns = [];
        let primaryKeyIndex = 0;
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
            label: 'Balances',
            name: 'Balances'
        });

        columns.push({
            label: 'Outstanding',
            name: 'Outstanding'
        });

        columns.push({
            label: 'Revert',
            name: 'Isdelete',
            activate: true,
            primaryKeyIndex: primaryKeyIndex,
            callback: (id, value) => {
                activate(id, value);
            }
        });

        setMainColumns(columns);
    };

    const activate = async (item, value) => {
        let jsonObj = {
            UserID: item,
            IsDelete: value
        };

        let request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);
        const response = await USER_REVERT(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        if (!response.status) setSeverity('error');
        setIsError(true);
        setItemList([]);
    };

    useEffect(() => {
        createColumns();
        selectUserTypes();
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
    const [formObject] = useState({
        UserTypeID: 0
    });

    const FormValidationSchema = Yup.object({
        UserTypeID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'role')
            .required(PLEASE_SELECT_VALID + 'role')
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }

        setItemList([]);
        var requestObj = JSON.stringify(values);
        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await SELECT_DELETEDUSERS_REPORT(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        setSubmitting(true);
        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        setItemList(JSON.parse(response.data));
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
                            <h2 className={classes.PaperHead_h2}>
                                Deleted User list
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
                                            <MaterialSelect
                                                label="Role"
                                                name="UserTypeID"
                                                title="Role"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Role
                                                </MenuItem>
                                                <MenuItem key="-99" value="-99">
                                                    All Users
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
                                            <PrimaryButton
                                                type="submit"
                                                title="Search"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                Search
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

            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};

export default DeletedUsers;
