import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { Container, Grid, Button, MenuItem } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import CircularProgress from '@material-ui/core/CircularProgress';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import Box from '@material-ui/core/Box';

import {
    UpdateAction as UPDATE,
    SelectByID as SELECT_BY_ID
} from 'Components/Api/TicketAPI';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const ActionTicket = ({
    TicketMasterID,
    FirmName,
    onDismissCallback
}) => {
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const SearchByID = async () => {
        let JSONObj = {
            TicketMasterID: TicketMasterID
        };

        var request = JSON.stringify(JSONObj);

        const response = await SELECT_BY_ID(request);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);
        setTicketObject({
            TicketMasterID: data.TicketMasterID,
            Body: data.Body,
            Subject: data.Subject,
            FirmName: FirmName
        });
    };

    const [TicketObject, setTicketObject] = useState({
        TicketMasterID: 0,
        Body: '',
        Subject: '',
        FirmName: ''
    });

    const validationSchema = Yup.object({
        Resolution: Yup.string().required(PLEASE_PROVIDE_VALID + 'resolution'),
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + ' status')
            .required(PLEASE_SELECT_VALID + 'status')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setLoadingOpen(true);

        var response = await UPDATE(JSON.stringify(values));

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setSeverity('error');
            setLoadingOpen(false);
            return;
        }

        setTimeout(() => {
            onDismissCallback(false);
        }, MESSAGE_CLOSE_AFTER);
    };

    useEffect(() => {
        SearchByID();
    }, [TicketMasterID]);

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
                initialValues={TicketObject}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={TicketObject}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <Grid container spacing={4}>
                                <Grid item xs={3}>
                                    <MaterialTextField
                                        label="FirmName"
                                        name="FirmName"
                                        type="text"
                                        readOnly={true}
                                        title="FirmName"
                                    />
                                </Grid>
                                <Grid item xs={3}>
                                    <MaterialTextField
                                        label="Subject"
                                        name="Subject"
                                        type="text"
                                        readOnly={true}
                                        title="Subject"
                                    />
                                </Grid>

                                <Grid item xs={6}>
                                    <MaterialTextField
                                        label="Body"
                                        name="Body"
                                        type="text"
                                        readOnly={true}
                                        title="Body"
                                    />
                                </Grid>

                                <Grid item xs={2}>
                                    <MaterialSelect
                                        label="Status"
                                        name="Status"
                                        title="Status"
                                    >
                                        <MenuItem key="0" value="0">
                                            Select Status
                                        </MenuItem>

                                        <MenuItem key="OPEN" value="OPEN">
                                            OPEN
                                        </MenuItem>
                                        <MenuItem key="CLOSED" value="CLOSED">
                                            CLOSED
                                        </MenuItem>
                                    </MaterialSelect>
                                </Grid>

                                <Grid item xs={10}>
                                    <MaterialTextField
                                        label="Resolution"
                                        name="Resolution"
                                        type="text"
                                        title="Resolution"
                                    />
                                </Grid>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Submit"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loadingOpen}
                                        >
                                            Submit
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
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
