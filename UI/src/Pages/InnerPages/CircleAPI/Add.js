import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import {
    Select as SELECT,
    Add as ADD,
    Activate as ACTIVATE_DEACTIVATE
} from 'Components/Api/CircleAPIURLAPI';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, Grid, Paper, Button } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import Box from '@material-ui/core/Box';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const CircleAPIAdd = () => {
    const [circleAPIs, setCircleAPIs] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isUpdate, setIsUpdate] = useState(false);
    useEffect(() => {
        setLoadingOpen(true);
        select();
        createColumns();
    }, []);

    var formObj = {
        CircleAPIMasterID: 0,
        CircleAPIURL: ''
    };
    const activate = async (item, value) => {
        var jsonObj = {
            CircleAPIMasterID: item,
            IsActive: value
        };

        var request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);
        const response = await ACTIVATE_DEACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) setSeverity('error');

        setIsError(true);
        select();
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            return;
        }
        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);
        setIsError(false);
        var response = await ADD(requestObj);

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
        select();
        setIsUpdate(false);
        resetForm(formObj);
    };

    const formValidationSchema = Yup.object({
        CircleAPIURL: Yup.string().required(PLEASE_PROVIDE_VALID + 'url')
    });

    const select = async () => {
        const response = await SELECT();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setCircleAPIs(data);
        setLoadingOpen(false);
    };
    const Edit = (CircleAPIMasterID, CircleAPIURL) => {
        setIsUpdate(true);
        formObj.CircleAPIMasterID = CircleAPIMasterID;
        formObj.CircleAPIURL = CircleAPIURL;
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'CircleAPIMasterID',
            srno: true
        });
        columns.push({
            label: 'Circle API URL',
            name: 'CircleAPIURL',
            ReadOnlyTextArea: true
        });

        columns.push({
            label: 'Active',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                activate(id, value);
            }
        });

        columns.push({
            label: 'Edit',
            name: 'Edit',
            buttonTitle: 'Edit',
            save: true,
            callback: (row) => {
                Edit(row[0], row[1]);
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
                validationSchema={formValidationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
                render={(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Circle API URL
                            </h2>

                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <h3 className={classes.Provider_Format}>
                                    Parameter(s) : use Number=[Number] for URL
                                    parameters
                                </h3>
                            </Grid>

                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialTextField
                                                label="Circle API URL"
                                                name="CircleAPIURL"
                                                type="text"
                                                placeholder="Circle API URL"
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
                                                title="Add"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                {!isUpdate ? 'Save' : 'Update'}
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

            <h2 className={classes.PaperHead_h2}>URL List</h2>
            <ReactTable
                data={circleAPIs}
                columns={mainColumns}
                headerToolbar={false}
            />
        </CustomContainer>
    );
};
