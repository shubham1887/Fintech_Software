import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';

import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { LOGOUT } from 'ConstComponents';
import {
    selectTicketCategory as TICKETCATRGORY_SELECT,
    Insert as TICKET_INSERT,
    Select as TICKET_SELECT,
    Delete as TICKET_DELETE,
    Update as TICKET_UPDATE,
    SelectByID as SELECT_BY_ID
} from 'Components/Api/TicketAPI';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import {
    Container,
    Grid,
    Paper,
    Button,
    MenuItem,
    TextField,
    CircularProgress
} from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const Ticket = () => {
    const classes = commonStyle();

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [categoryList, setCategoryList] = useState([]);
    const [document, setDocument] = useState('');
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [IsUpdate, setIsUpdate] = useState(false);
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);
        setSubmitting(false);
        setIsError(false);
        setLoading(true);

        var response = IsUpdate
            ? await TICKET_UPDATE(requestObj)
            : await TICKET_INSERT(requestObj);

        // var response = await TICKET_INSERT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setSubmitting(true);

        setIsError(true);

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        setSeverity('success');
        setResponseMessage(response.data);
        resetForm(FormObj);
        resetForm(FormData);
        selectData();
        setTimeout(() => {}, MESSAGE_CLOSE_AFTER);
    };

    const FormValidationSchema = Yup.object({
        TicketCategoryID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + ' ticket category')
            .required(PLEASE_SELECT_VALID + 'ticket category'),
        Subject: Yup.string().required(PLEASE_PROVIDE_VALID + 'subject'),
        Body: Yup.string().required(PLEASE_PROVIDE_VALID + 'body')
    });

    const selectTicketCategory = async () => {
        const response = await TICKETCATRGORY_SELECT();
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) return;
        var data = JSON.parse(response.data);
        setCategoryList(data);
    };

    const ResetAll = async (e) => {
        e.target.value = '';
    };
    const selectData = async () => {
        const { isAuthorized, status, data } = await TICKET_SELECT();

        setLoading(false);
        if (!isAuthorized) LOGOUT();

        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);
            return;
        }
        var d = JSON.parse(data);

        setItemList(d);
    };

    const DeleteTicket = async (TicketMasterID) => {
        let jsonObj = {
            TicketMasterID: TicketMasterID
        };
        setLoading(true);
        const response = await TICKET_DELETE(JSON.stringify(jsonObj));
        setLoading(false);

        const { isAuthorized, status, data } = response;
        if (!isAuthorized) LOGOUT();

        setResponseMessage(data);
        setIsError(true);

        if (!status) {
            setSeverity('error');
            return;
        }
        setSeverity('success');

        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);
    };

    const Edit = async (request) => {
        setIsUpdate(true);
        SelectByID(request);
    };

    const SelectByID = async (TicketMasterID) => {
        let jsonObj = {
            TicketMasterID: TicketMasterID
        };
        var requestObj = JSON.stringify(jsonObj);

        const response = await SELECT_BY_ID(requestObj);

        //  if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setFormObj({
            TicketMasterID: data.TicketMasterID,
            TicketCategoryID: data.TicketCategoryID,
            Body: data.Body,
            Subject: data.Subject
        });
    };

    const [FormObj, setFormObj] = useState({
        TicketMasterID: 0,
        TicketCategoryID: 0,
        Body: '',
        Subject: '',
        Document: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        }
    });

    const createColumns = () => {
        var columns = [];

        var enableIndex = 2;
        columns.push({
            label: '#',
            name: 'TicketMasterID',
            srno: true
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            hide: true
        });
        columns.push({
            label: 'Category',
            name: 'Category'
        });

        columns.push({
            label: 'Subject',
            name: 'Subject'
        });

        columns.push({
            label: 'Description',
            name: 'Body'
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        columns.push({
            label: 'Edit',
            name: 'Edit',
            buttonTitle: 'Edit',
            update: true,
            callback: (row) => {
                Edit(row[0]);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'TicketMasterID',
            delete: true,
            callback: (value) => {
                DeleteTicket(value);
            }
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        createColumns();
        selectTicketCategory();
        selectData();
    }, []);

    const TicketCategoryItems = categoryList.map(
        ({ TicketCategoryID, Name }) => (
            <MenuItem key={TicketCategoryID} value={TicketCategoryID}>
                {Name}
            </MenuItem>
        )
    );

    const UploadForm = (formProps) => {
        return (
            <Form>
                <Grid item xs={12}>
                    <Paper className={classes.PaperStyle}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialSelect
                                    name="TicketCategoryID"
                                    label="TicketCategoryID"
                                    title="TicketCategoryID"
                                >
                                    <MenuItem key="0" value="0">
                                        Category
                                    </MenuItem>
                                    {TicketCategoryItems}
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="Subject"
                                    name="Subject"
                                    type="text"
                                    placeholder="Subject"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={6} lg={6}>
                                <MaterialTextField
                                    label="Body"
                                    name="Body"
                                    type="text"
                                    placeholder="Body"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <TextField
                                    id="file"
                                    name="DocumentFile"
                                    type="file"
                                    accept="image/*"
                                    onChange={(e) => {
                                        var file = e.target.files[0];

                                        setDocument(URL.createObjectURL(file));

                                        var reader = new FileReader();
                                        formProps.setFieldValue(
                                            'Document.Name',
                                            file.name
                                        );
                                        formProps.setFieldValue(
                                            'Document.Size',
                                            file.size
                                        );
                                        formProps.setFieldValue(
                                            'Document.Type',
                                            file.type
                                        );
                                        reader.onload = function (item) {
                                            formProps.setFieldValue(
                                                'Document.Data',
                                                item.target.result
                                                    .replace('data:', '')
                                                    .replace(/^.+,/, '')
                                            );
                                        };

                                        reader.readAsDataURL(file);
                                    }}
                                />
                            </Grid>
                        </Grid>
                    </Paper>
                </Grid>
                <Grid className={classes.footerGrid}>
                    <Box className={classes.flexReverse}>
                        <div className={classes.SubmitWrapper}>
                            <Button
                                type="submit"
                                title={IsUpdate ? 'Update' : 'Save'}
                                className={classes.CustomPrimaryButtonStyle}
                                disabled={loading}
                            >
                                {IsUpdate ? 'Update' : 'Save'}
                            </Button>
                            {loading && (
                                <CircularProgress
                                    size={24}
                                    className={classes.buttonProgress}
                                />
                            )}
                        </div>
                        <Button
                            type="reset"
                            variant="contained"
                            color="secondary"
                            title="Reset"
                            className={classes.CustomDangerButtonStyle}
                            onClick={() => window.location.reload()}
                            // onClick={() => {
                            //     reset();
                            // }}
                        >
                            Reset
                        </Button>
                    </Box>
                </Grid>
            </Form>
        );
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
                onSubmit={handleSubmit}
                resetForm={FormObj}
            >
                {UploadForm}
            </Formik>

            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
