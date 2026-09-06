import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Box,
    Button,
    Paper,
    CircularProgress
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    Select as SELECT,
    Update as UPDATE,
    IsOnOff as ISONOFF
} from 'Components/Api/PlanSettingAPI';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import ReactTable from 'Components/ReactTable/ReactTable';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const PlanSetting = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [IsUpdate, setIsUpdate] = useState(false);
    const [callbackURL, setcallbackURL] = useState('');

    const [FormObj, setFormObj] = useState({
        Name: '',
        UserID: '',
        Token: '',
        Password: '',
        Circle: '',
        IsActive: false
    });
    const FormValidationSchema = Yup.object({
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        UserID: Yup.string().required(PLEASE_PROVIDE_VALID + ' userID'),
        Token: Yup.string().required(PLEASE_PROVIDE_VALID + ' token'),
        Password: Yup.string().required(PLEASE_PROVIDE_VALID + ' password')
    });
    const selectData = async () => {
        const response = await SELECT();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);
        setItemList(data);
    };

    const SelectByID = async (PlanSettingID) => {
        setLoadingOpen(false);

        let jsonObj = {
            PlanSettingID: PlanSettingID
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await ISONOFF(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setIsError(true);
        if (!response.status) setSeverity('error');

        let d = JSON.parse(response.data);
        setFormObj({
            PlanSettingID: d.PlanSettingID,
            Name: d.Name,
            UserID: d.UserID,
            Token: d.Token,
            Password: d.Password,
            Circle: d.Circle,
            IsActive: d.IsActive
        });
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'PlanSettingID',
            srno: true
        });

        columns.push({
            label: 'Name',
            name: 'Name'
        });
        columns.push({
            label: 'UserID',
            name: 'UserID'
        });
        columns.push({
            label: 'Circle',
            name: 'Circle'
        });
        columns.push({
            label: 'Token',
            name: 'Token'
        });

        columns.push({
            label: 'Password',
            name: 'Password'
        });

        columns.push({
            label: 'IsActive',
            name: 'IsActiveStr'
        });
        columns.push({
            label: 'Edit',
            name: 'PlanSettingID',
            edit: true,
            callback: (PlanSettingID) => {
                setIsUpdate(true);
                SelectByID(PlanSettingID);
            }
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        selectData();
    }, []);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = await UPDATE(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        setSubmitting(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }

        resetForm(FormObj);
        setFormObj({
            Name: '',
            UserID: '',
            Token: '',
            Password: '',
            Circle: '',
            IsActive: false
        });

        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);
    };

    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
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
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Plan Setting
                            </h2>

                            <Grid item xs={12}>
                                {IsUpdate ? (
                                    <Paper className={classes.PaperStyle}>
                                        <Grid container spacing={2}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="Name"
                                                    name="Name"
                                                    type="text"
                                                    placeholder="Name"
                                                    readOnly={true}
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="UserID"
                                                    name="UserID"
                                                    type="text"
                                                    placeholder="UserID"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="Circle"
                                                    name="Circle"
                                                    type="text"
                                                    placeholder="Circle"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="Token"
                                                    name="Token"
                                                    type="text"
                                                    placeholder="Token"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="Password"
                                                    name="Password"
                                                    type="text"
                                                    placeholder="Password"
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={3}
                                                sm={3}
                                                md={3}
                                                lg={3}
                                            >
                                                <FormikMaterialSwitch
                                                    label="IsActive"
                                                    name="IsActive"
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        OnChange(
                                                            name,
                                                            value,
                                                            formProps.setFieldValue
                                                        );
                                                    }}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Paper>
                                ) : null}
                            </Grid>
                            {IsUpdate ? (
                                <Grid className={classes.footerGrid}>
                                    <Box className={classes.flexReverse}>
                                        <div className={classes.SubmitWrapper}>
                                            <Button
                                                type="submit"
                                                title="Update"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                Update
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
                                        <Button
                                            type="reset"
                                            title="Reset"
                                            className={
                                                classes.CustomDangerButtonStyle
                                            }
                                        >
                                            Reset
                                        </Button>
                                    </Box>
                                </Grid>
                            ) : null}
                        </Form>
                    );
                }}
            </Formik>
            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
