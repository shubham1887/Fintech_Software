import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';

import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';

import { Container, Grid, Button } from '@material-ui/core';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';

import { Select as SEARCH } from 'Components/Api/DaybooReportAPI';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const DaybookPopUp = ({ DaybookData }) => {
    var UserID = DaybookData.UserID;
    var FirmName = DaybookData.FirmName;
    const [list, setList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    useEffect(() => {
        select();
        createColumns();
    }, []);

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        UserID: UserID,
        FirmName: FirmName
    });

    const select = async () => {
        var inputObj = {
            FromDate: new Date(),
            ToDate: new Date(),
            UserID: UserID
        };
        setList([]);

        var requestObj = JSON.stringify(inputObj);

        setLoadingOpen(true);
        setIsError(false);

        var response = await SEARCH(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);

        setList(data.list);
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            FromDate: values.FromDate,
            ToDate: values.ToDate,
            UserID: values.UserID
        };
        setList([]);

        var requestObj = JSON.stringify(inputObj);

        setLoadingOpen(true);
        setIsError(false);

        var response = await SEARCH(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);
        setList(data.list);
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

    const createColumns = () => {
        var columns = [];
        var textwrapwidth = 60;

        columns.push({
            label: '#',
            name: 'Operator',
            srno: true
        });
        columns.push({
            label: 'Operator',
            name: 'Operator'
        });

        columns.push({
            label: 'Total',
            name: 'TotalCount',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'T Amt',
            name: 'TotalAmount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'Success',
            name: 'SuccessCount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'S Amt',
            name: 'SuccessAmount',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Failed',
            name: 'FailedCount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'F Amt',
            name: 'FailedAmount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'Pending',
            name: 'PendingCount',
            textwrapwidth,
            numeric: true
        });

        columns.push({
            label: 'P Amt',
            name: 'PendingAmount',
            textwrapwidth,
            numeric: true
        });
        columns.push({
            label: 'Commission',
            name: 'RetailerCommission',
            textwrapwidth,
            numeric: true
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
                enableReinitialize={true}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={ModelObj}
                render={({
                    values,
                    setFieldValue,
                    handleChange,
                    handleSubmit
                }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Grid container spacing={3}>
                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialDatePickerField
                                            name="FromDate"
                                            label="From Date"
                                            maxDate={new Date()}
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialDatePickerField
                                            name="ToDate"
                                            label="To Date"
                                            maxDate={new Date()}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialTextField
                                            label="FirmName"
                                            name="FirmName"
                                            type="text"
                                            readOnly
                                            title="FirmName"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <div className={classes.SubmitWrapper}>
                                            <Button
                                                type="submit"
                                                title="submit"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                Search
                                            </Button>
                                            {loadingOpen && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </div>
                                    </Grid>
                                </Grid>
                            </Grid>

                            <Grid className={classes.footerGrid}></Grid>
                        </Form>
                    );
                }}
            />
            {list !== undefined || list !== null || list.length >= 1 ? (
                <ReactTable
                    data={list}
                    columns={mainColumns}
                    rowsPerPage={10}
                />
            ) : null}
        </CustomContainer>
    );
};
