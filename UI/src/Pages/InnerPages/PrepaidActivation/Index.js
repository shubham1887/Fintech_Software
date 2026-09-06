import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Paper,
    CircularProgress,
    MenuItem
} from '@material-ui/core';
import { CustomModal } from 'Components/Modal/CustomModal';
import { LOGOUT } from 'ConstComponents';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import { SelectFilter as SELECT_FILTER } from 'Components/Api/PrepiadActivationAPI';
import { StatusEdit } from 'Pages/InnerPages/PrepaidActivation/Action';
import ReactTable from 'Components/ReactTable/ReactTable';
import { PLEASE_SELECT_VALID, ADMIN_CODE } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);
export const PrepaidActivationReport = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [itemList, setItemList] = useState([]);
    const [loading, setLoading] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [isModalOpen, setIsModalOpen] = useState(false);

    const [PrepaidActivationID, setPrepaidActivationID] = useState(0);

    const [openFilter, setopenFilter] = useState(true);
    const [isAdminRole, setIsAdminRole] = useState(false);
    var currentUsertypeid = localStorage.getItem('UserTypeID');

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) {
            setIsAdminRole(true);
        }
        createColumns();
    };
    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        Status: 'PROCESS'
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);

        setIsError(false);
        setLoading(true);
        createColumns();
        setItemList([]);
        var response = await SELECT_FILTER(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        setSubmitting(false);
        var data = JSON.parse(response.data);

        setItemList(data);
    };
    const selectData = async () => {
        let jsonObj = {
            ToDate: new Date(),
            FromDate: new Date(),
            Status: 'PROCESS'
        };
        var requestObj = JSON.stringify(jsonObj);
        setItemList([]);
        var response = await SELECT_FILTER(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setResponseMessage(response.data);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(response.data);
        setItemList(data);
    };
    const Edit = async (row) => {
        setIsModalOpen(true);
        setPrepaidActivationID(row[0]);
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'PrepaidActivationID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'Date'
        });

        columns.push({
            label: 'User',
            name: 'UserName'
        });

        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });
        columns.push({
            label: 'Activation Type',
            name: 'ActivationType'
        });
        columns.push({
            label: 'Alternate Number',
            name: 'AlternateNumber'
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        if (isAdminRole === true) {
            columns.push({
                label: 'Action',
                name: 'PrepaidActivationID',
                save: true,
                buttonTitle: 'Update',
                showTitle: true,
                callback: (row) => {
                    Edit(row);
                }
            });
        }

        setMainColumns(columns);
    };
    const validationSchema = Yup.object({
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'from date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'from date')
            .typeError(PLEASE_SELECT_VALID + 'from date'),

        ToDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'to date')
            .typeError(PLEASE_SELECT_VALID + 'to date')
    });

    useEffect(() => {
        setLoading(true);
        CheckUserRole();
        selectData();
    }, [isAdminRole]);

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
                innerRef={formRef}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={ModelObj}
                render={({ values, setFieldValue, handleChange }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'Prepaid Activation Report'}
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
                                                <MaterialSelect
                                                    label="Status"
                                                    name="Status"
                                                    title="Status"
                                                >
                                                    <MenuItem
                                                        key="ALL"
                                                        value="ALL"
                                                    >
                                                        All Status
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="SUCCESS"
                                                        value="SUCCESS"
                                                    >
                                                        SUCCESS
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="FAILURE"
                                                        value="FAILURE"
                                                    >
                                                        FAILURE
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="PROCESS"
                                                        value="PROCESS"
                                                    >
                                                        PROCESS
                                                    </MenuItem>
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
                                                    title="submit"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    disabled={loading}
                                                >
                                                    Search
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
                                        </Grid>
                                    </CustomAccordion>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />
            <ReactTable data={itemList} columns={mainColumns} />
            {isModalOpen ? (
                <CustomModal
                    isOpen={isModalOpen}
                    title="Manual Status update"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsModalOpen(value);
                        selectData();
                    }}
                >
                    <StatusEdit
                        PrepaidActivationID={PrepaidActivationID}
                        onDismissCallback={(value) => {
                            setIsModalOpen(value);
                            selectData();
                        }}
                    />
                </CustomModal>
            ) : null}
        </CustomContainer>
    );
};
