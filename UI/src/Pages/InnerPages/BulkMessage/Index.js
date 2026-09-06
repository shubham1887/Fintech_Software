import React, { memo, useState, useEffect } from 'react';
import { Formik, Form, FieldArray } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Paper,
    CircularProgress,
    TextField
} from '@material-ui/core';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import Autocomplete from '@material-ui/lab/Autocomplete';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import {
    SelectBulkMessage as SELECT_BULK_MESSAGES,
    SendBulkMessage as SEND_BULK_MESSAGE
} from 'Components/Api/BulkMessageAPI';
import { Select as USER_TYPE_SELECT } from 'Components/Api/UserTypeApi';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { DeleteAll as DELETE_ALL } from 'Components/Api/BulkMessageAPI';
const CustomContainer = HOCLoader(Container);

export const Index = memo(() => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [userTypeList, setUserTypeList] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const initialValues = {
        Message: '',
        IsSMS: false,
        IsEmail: false,
        IsWhatsUp: false,
        UserTypeList: [],
        UserType: '',
        UserTypeID: 0
    };

    const [FormObj, setFormObj] = useState(initialValues);

    const FormValidationSchema = Yup.object().shape({
        // UserTypeList: Yup.array().min(1).of(Yup.string().trim().required(PLEASE_PROVIDE_VALID + 'user type')),
        Message: Yup.string().required(PLEASE_PROVIDE_VALID + 'message')
        // UserTypeList: Yup.array()
        //     .min(1)
        //     .of(
        //         Yup.object().shape({
        //             UserType: Yup.string().required().trim().min(1),

        //             UserTypeID: Yup.number().required().min(1)
        //         })
        //     )
    });

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

    useEffect(() => {
        createColumns();
        selectUserTypes();
        selectBulkMessages();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'BulkMessageID',
            srno: true
        });
        columns.push({
            label: 'Message',
            name: 'Message'
        });
        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });
        columns.push({
            label: 'MobileNumber',
            name: 'MobileNumber'
        });
        columns.push({
            label: 'EmailID',
            name: 'EmailID'
        });
        columns.push({
            label: 'SMS Sent',
            name: 'BySMS'
        });
        columns.push({
            label: 'Email Sent',
            name: 'ByEmail'
        });
        columns.push({
            label: 'WhatsApp Sent',
            name: 'ByWhatsApp'
        });
        setMainColumns(columns);
    };

    const submitHandle = async (values, { resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        setLoading(true);
        setIsError(false);

        var response = await SEND_BULK_MESSAGE(JSON.stringify(values));
        if (!response.isAuthorized) {
            LOGOUT();
            return;
        }

        setLoading(false);
        setResponseMessage(response.data);
        setSeverity(response.status ? 'success' : 'error');
        setIsError(true);
        resetForm(FormObj);

        if (response.status) {
            setTimeout(() => {
                selectBulkMessages();
            }, MESSAGE_CLOSE_AFTER);
        }
    };

    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection
        };

        var requestJSON = JSON.stringify(requestObj);

        const response = await SELECT_BULK_MESSAGES(requestJSON);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setLoading(false);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);
        if (SkipCount <= 0) {
            setTotalCount(data.Count);
        }
        setItemList(data.list);
    };

    const selectBulkMessages = async () => {
        setItemList([]);
        createColumns();

        await callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const DeleteAll = async () => {
        setLoading(true);
        setIsError(false);
        var response = await DELETE_ALL();
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();
        setResponseMessage(response.data);

        setIsError(true);

        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            return;
        }
        callSelectData();
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
                validationSchema={FormValidationSchema}
                onSubmit={submitHandle}
                resetForm={FormObj}
            >
                {({ setFieldValue }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Send Bulk Message
                                    </h2>
                                </Grid>
                            </Grid>

                            <Paper className={classes.PaperStyle}>
                                <Grid container spacing={2}>
                                    <Grid
                                        item
                                        xs={12}
                                        sm={6}
                                        md={6}
                                        className={classes.multiSelect}
                                    >
                                        <FieldArray
                                            name="UserTypeList"
                                            render={(userTypeIDList) => (
                                                <Autocomplete
                                                    multiple
                                                    id="UserTypeList"
                                                    options={userTypeList}
                                                    getOptionLabel={(option) =>
                                                        option.UserType
                                                    }
                                                    onChange={(
                                                        event,
                                                        values
                                                    ) => {
                                                        values.UserTypeList = [];
                                                        values.map(
                                                            (category) => {
                                                                userTypeIDList.push(
                                                                    {
                                                                        UserTypeID:
                                                                            category.UserTypeID
                                                                    }
                                                                );
                                                                values.UserTypeList.push(
                                                                    {
                                                                        UserTypeID:
                                                                            category.UserTypeID
                                                                    }
                                                                );
                                                            }
                                                        );
                                                    }}
                                                    renderInput={(params) => (
                                                        <TextField
                                                            {...params}
                                                            variant="standard"
                                                            name="UserTypeList"
                                                            label="Select User Type(s)"
                                                            placeholder="Select User Type(s)"
                                                        />
                                                    )}
                                                />
                                            )}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={6} md={6}>
                                        <MaterialTextField
                                            label="Message"
                                            name="Message"
                                            type="text"
                                            placeholder="Message"
                                        />
                                    </Grid>
                                    {/* <Grid item xs={12} sm={2} md={2}>
                                        <FormikMaterialSwitch
                                            label="Send SMS"
                                            name="IsSMS"
                                            onCustomCallback={(name, value) => {
                                                setFieldValue('IsSMS', value);
                                            }}
                                        />
                                    </Grid> */}
                                    <Grid item xs={12} sm={2} md={2}>
                                        <FormikMaterialSwitch
                                            label="Send EMail"
                                            name="IsEmail"
                                            onCustomCallback={(name, value) => {
                                                setFieldValue('IsEmail', value);
                                            }}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={2} md={2}>
                                        <FormikMaterialSwitch
                                            label="Send WhatsApp"
                                            name="IsWhatsUp"
                                            onCustomCallback={(name, value) => {
                                                setFieldValue(
                                                    'IsWhatsUp',
                                                    value
                                                );
                                            }}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={2} md={2}>
                                        <PrimaryButton
                                            type="submit"
                                            title="Send Bulk Message"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Send Bulk Message
                                        </PrimaryButton>
                                        {loading && (
                                            <CircularProgress
                                                size={24}
                                                className={
                                                    classes.buttonProgress
                                                }
                                            />
                                        )}
                                    </Grid>
                                    <Grid item xs={12} sm={2} md={2}>
                                        <SecondaryButton
                                            type="reset"
                                            variant="contained"
                                            color="secondary"
                                            title="Reset"
                                            disabled={loading}
                                            className={
                                                classes.CustomDangerButtonStyle
                                            }
                                        >
                                            Reset
                                        </SecondaryButton>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={1} lg={1}>
                                        <PrimaryButton
                                            variant="contained"
                                            color="primary"
                                            title="Delete All"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            onClick={() => {
                                                DeleteAll();
                                            }}
                                        >
                                            Delete All
                                        </PrimaryButton>
                                    </Grid>
                                </Grid>
                            </Paper>
                        </Form>
                    );
                }}
            </Formik>
            <ReactTable
                data={itemList}
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
                    callSelectData(currentPage, TakeCount, SearchText, '', '');
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
});
