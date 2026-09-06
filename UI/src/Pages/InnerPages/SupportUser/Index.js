import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import { Container, Grid, Paper } from '@material-ui/core';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';

import {
    Activate as SUPPOERT_USER_ACCESS_ACTIVATE,
    SelectMenu as MENU_SELECT
} from 'Components/Api/SupportUserAccessApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import { SelectSupportUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PLEASE_SELECT_VALID,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
const CustomContainer = HOCLoader(Container);

export const SupportAccess = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [mainColumns, setMainColumns] = useState([]);
    const [UsersList, setUsers] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const formRef = useRef();

    const formObject = {
        MenuMasterID: 0,

        User: {
            UserID: 0,
            FirmName: 'Select User'
        }
    };

    function handleChange(value) {
        selectData({ UserID: value });
    }

    const Activate = async (id, value) => {
        var jsonObj = {
            MenuMasterID: id,
            UserID: formRef.current.values.User.UserID,
            IsActive: value
        };

        var request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);
        const response = await SUPPOERT_USER_ACCESS_ACTIVATE(request);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        if (!response.status) setSeverity('error');
        setIsError(true);
        setTimeout(() => {
            selectData({ UserID: formRef.current.values.User.UserID });
        }, MESSAGE_CLOSE_AFTER);
    };

    const selectData = async (request) => {
        const { isAuthorized, status, data } = await MENU_SELECT(
            JSON.stringify(request)
        );
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);

            return;
        }

        setItemList(JSON.parse(data));
    };

    const SelectUser = async (values) => {
        setLoadingOpen(true);
        var request = JSON.stringify(values);

        const { isAuthorized, status, data } = await USER_SELECT_FOR_DROPDOWN(
            request
        );
        setLoadingOpen(false);
        if (!isAuthorized) LOGOUT();

        if (!status) {
            setIsError(true);
            setSeverity('error');
            return;
        }

        setLoadingOpen(false);
        setUsers(JSON.parse(data));
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'MenuMasterID',
            srno: true
        });
        columns.push({
            label: 'Name',
            name: 'name'
        });

        // columns.push({
        //     label: 'Page Name',
        //     name: 'Description'
        // });

        columns.push({
            label: 'Operations',
            name: 'children',
            primaryKeyIndex: 0,
            CustomSwitch: true,
            callback: ({ id, value }) => {
                Activate(id, value);
            }
        });

        // columns.push({
        //     label: 'OnOff',
        //     name: 'IsActive',
        //     activateRow: true,
        //     primaryKeyIndex: 0,
        //     callback: (id, value, row) => {
        //         Activate(row, value);
        //     }
        // });

        setMainColumns(columns);
    };

    useEffect(() => {
        setLoadingOpen(true);
        SelectUser();
        createColumns();
    }, []);

    const validationSchema = Yup.object({
        User: Yup.object()
            .nullable(PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
            .shape({
                UserID: Yup.string()
                    .notOneOf(['0'], PLEASE_SELECT_VALID + 'User')
                    .required(PLEASE_SELECT_VALID + 'User')
            })
    });

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
                initialValues={formObject}
                validationSchema={validationSchema}
                resetForm={formObject}
            >
                {() => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Support User Access
                            </h2>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <Field
                                                component={AutoCompleteTextView}
                                                FieldName="User"
                                                name="User"
                                                SubFieldName="UserID"
                                                options={UsersList}
                                                getOptionLabel={(option) =>
                                                    option.FirmName
                                                }
                                                textFieldProps={{
                                                    label: 'Select User'
                                                }}
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    handleChange(value.UserID);
                                                }}
                                            />
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            <h2 className={classes.PaperHead_h2}>Support Access</h2>
            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
