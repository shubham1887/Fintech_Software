import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import {
    Container,
    Grid,
    Paper,
    Box,
    Button,
    Accordion,
    CircularProgress
} from '@material-ui/core';
import excel from 'Assets/Images/excel.svg';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';
import { Select as USER_ACTOIN_HISTORY_SELECT } from 'Components/Api/UserActionHistoryApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import { SelectSupportUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import Icon from '@material-ui/core/Icon';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const UserActionHistory = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [mainColumns, setMainColumns] = useState([]);
    const [UsersList, setUsers] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [openFilter, setopenFilter] = useState(true);

    const userDefault = {
        UserID: localStorage.getItem('UserID'),
        FirmName: 'Own'
    };

    const [formObject] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        User: userDefault,
        IsExcelDownload: false
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            FromDate: values.FromDate,
            ToDate: values.ToDate,
            UserID: values.User.UserID,
            IsExcelDownload: values.IsExcelDownload
        };

        setItemList([]);

        var request = JSON.stringify(inputObj);
        setLoadingOpen(true);
        const response = await USER_ACTOIN_HISTORY_SELECT(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setLoadingOpen(false);
        setSubmitting(false);

        if (!response.status) {
            setIsError(true);
            setResponseMessage(response.data);
            setSeverity('error');
            return;
        }
        //    setIsError(true);
        //    setResponseMessage(response.data);
        //    setSeverity('success');

        var data = JSON.parse(response.data);

        if (data.IsExcelDownload) {
            formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'UserAction History Report');
        }
        setItemList(data.list);
    };
    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
        document.removeChild(a);
    }

    const SelectUser = async (values) => {
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
        setLoadingOpen(false);
        setUsers(data);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });
        columns.push({
            label: 'DOC',
            name: 'DOC'
        });

        columns.push({
            label: 'User',
            name: 'FirmName'
        });

        columns.push({
            label: 'Action',
            name: 'Action'
        });
        columns.push({
            label: 'Description',
            name: 'Description',
            ReadOnlyTextArea: true
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        SelectUser();
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
                onSubmit={submithandle}
                resetForm={formObject}
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'Support User Action History'}
                                        open={openFilter}
                                        setOpen={(value) =>
                                            setopenFilter(value)
                                        }
                                    >
                                        <Grid container spacing={2}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialDatePickerField
                                                    name="FromDate"
                                                    label="From Date"
                                                    maxDate={new Date()}
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialDatePickerField
                                                    name="ToDate"
                                                    label="To Date"
                                                    maxDate={new Date()}
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
                                                    component={
                                                        AutoCompleteTextView
                                                    }
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
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={1}
                                                lg={1}
                                            >
                                                <Box
                                                    style={{
                                                        textAlign: 'center'
                                                    }}
                                                >
                                                    <Icon
                                                        className={
                                                            classes.CustomImageButtonStyle
                                                        }
                                                        disabled={loadingOpen}
                                                        classes={{
                                                            root:
                                                                classes.iconRoot
                                                        }}
                                                        onClick={(e) => {
                                                            setFieldValue(
                                                                'IsExcelDownload',
                                                                true
                                                            );
                                                            handleSubmit(e);
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
                                                    title="submit"
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
                                        </Grid>
                                    </CustomAccordion>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />

            <h2 className={classes.PaperHead_h2}>Action History</h2>
            <ReactTable
                data={itemList}
                columns={mainColumns}
                headerToolbar={false}
                rowsPerPage={100}
            />
        </CustomContainer>
    );
};
