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
import {
    MaterialTextField,
    MaterialTextAreaField
} from 'Components/Inputes/FormikMaterialInput';
import Autocomplete from '@material-ui/lab/Autocomplete';
import { LOGOUT } from 'ConstComponents';
import { Select as USER_TYPE_SELECT } from 'Components/Api/UserTypeApi';
import { SendPushNotification as SEND_PUSH_NOTIFICATIONS } from 'Components/Api/PushNotificationAPI';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const Index = memo(() => {
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');

    const [userTypeList, setUserTypeList] = useState([]);

    var FormObj = {
        Title: '',
        Description: '',
        UserTypeList: []
    };

    useEffect(() => {
        selectUserTypes();
    }, []);

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

    const submitHandle = async (values, { resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        setLoading(true);
        setIsError(false);

        var response = await SEND_PUSH_NOTIFICATIONS(JSON.stringify(values));
        if (!response.isAuthorized) {
            LOGOUT();
            return;
        }

        setLoading(false);
        setResponseMessage(response.data);
        setSeverity(response.status ? 'success' : 'error');
        setIsError(true);
        resetForm(FormObj);
    };

    const FormValidationSchema = Yup.object({
        UserTypeList: Yup.array().min(1, PLEASE_SELECT_VALID + 'user type'),
        Title: Yup.string().required(PLEASE_PROVIDE_VALID + 'title'),
        Description: Yup.string().required(PLEASE_PROVIDE_VALID + 'description')
    });

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
                        <Form>
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Send Push Notification
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
                                    <Grid item xs={12} sm={3} md={3}>
                                        <MaterialTextField
                                            label="Title"
                                            name="Title"
                                            type="text"
                                            placeholder="Title"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={3} md={3}>
                                        <MaterialTextAreaField
                                            label="Description"
                                            name="Description"
                                            type="text"
                                            placeholder="Description"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={3} md={3}>
                                        <PrimaryButton
                                            type="submit"
                                            title="Send Push Notification"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Send Push Notification
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
                                </Grid>
                            </Paper>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
});
