import React, { memo, useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { Container, Grid, Paper, CircularProgress } from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import { scrollToTop } from 'Components/utils.js';
import { LOGOUT } from 'ConstComponents';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import {
    Select as SELECT_SETTINGS,
    SelectByID as SELECT_SETTINGS_BY_ID,
    Update as UPDATE_SETTINGS
} from 'Components/Api/AccountOpenSettingAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const Index = memo(() => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [IsUpdate, setIsUpdate] = useState(false);

    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [FormObj, setFormObj] = useState({
        AccountOpenSettingID: 0,
        Name: '',
        DisplayName: '',
        UserName: '',
        Token: '',
        MerchantID: '',
        IsActive: false,
        CallBackURL: '',
        Sequence: 0,
        Mode: ''
    });

    const FormValidationSchema = Yup.object({
        Name: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        UserName: Yup.string().required(PLEASE_PROVIDE_VALID + ' username'),
        Token: Yup.string().required(PLEASE_PROVIDE_VALID + ' token'),
        MerchantID: Yup.string().required(PLEASE_PROVIDE_VALID + ' MerchantID'),
        Mode: Yup.string().required(PLEASE_PROVIDE_VALID + 'Mode')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setResponseMessage('');
        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        var response = await UPDATE_SETTINGS(requestObj);

        if (!response.isAuthorized) LOGOUT();
        setLoadingOpen(false);
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        setLoadingOpen(false);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);
        resetForm(FormObj);

        setFormObj({
            AccountOpenSettingID: 0,
            Name: '',
            DisplayName: '',
            UserName: '',
            Token: '',
            MerchantID: '',
            IsActive: false,
            CallBackURL: '',
            Mode: '',
            Sequence: 0
        });

        selectData();
    };

    useEffect(() => {
        createColumns();
        selectData();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'AccountOpenSettingID',
            srno: true
        });

        columns.push({
            label: 'Edit',
            name: 'AccountOpenSettingID',
            edit: true,
            callback: (AccountOpenSettingID) => {
                setIsUpdate(true);
                SelectByID(AccountOpenSettingID);
            }
        });

        columns.push({
            label: 'Name',
            name: 'Name',
            actived: true
        });

        columns.push({
            label: 'Sequence',
            name: 'Sequence',
            actived: true
        });

        columns.push({
            label: 'Display Name',
            name: 'DisplayName',
            actived: true
        });
        columns.push({
            label: 'UserName',
            name: 'UserName',
            displayAsPassword: true
        });

        columns.push({
            label: 'Token',
            name: 'Token',
            displayAsPassword: true
        });
        columns.push({
            label: 'MerchantID',
            name: 'MerchantID',
            displayAsPassword: true
        });

        columns.push({
            label: 'CallBackURL',
            name: 'CallBackURL',
            actived: true
        });
        columns.push({
            label: 'Mode',
            name: 'Mode',
            actived: true
        });

        columns.push({
            label: 'Active',
            name: 'IsActiveStr',
            actived: true
        });

        setMainColumns(columns);
    };

    const selectData = async () => {
        setLoadingOpen(false);
        const response = await SELECT_SETTINGS();

        if (!response.isAuthorized) LOGOUT();
        setLoadingOpen(false);

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);

        setItemList(data);
    };

    const SelectByID = async (AccountOpenSettingID) => {
        setIsError(false);

        let jsonObj = {
            AccountOpenSettingID: AccountOpenSettingID
        };
        const response = await SELECT_SETTINGS_BY_ID(JSON.stringify(jsonObj));

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        const { data, status } = response;
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);
        }

        let d = JSON.parse(data);

        setFormObj({
            AccountOpenSettingID: d.AccountOpenSettingID,
            Name: d.Name,
            DisplayName: d.DisplayName,
            UserName: d.UserName,
            Token: d.Token,
            IsActive: d.IsActive,
            CallBackURL: d.CallBackURL,
            Mode: d.Mode,
            Sequence: d.Sequence,

            MerchantID: d.MerchantID
        });
        scrollToTop();
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
            <Grid container spacing={3}>
                <Grid item xs={6}>
                    <h2 className={classes.PaperHead_h2}>
                        Account Open Setting
                    </h2>
                </Grid>
                <Grid item xs={6}></Grid>
            </Grid>
            <Formik
                enableReinitialize={true}
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {(formProps) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                {IsUpdate ? (
                                    <Paper className={classes.PaperStyle}>
                                        <Grid container spacing={2}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialTextField
                                                    label="Name"
                                                    name="Name"
                                                    type="text"
                                                    readOnly={true}
                                                    placeholder="Name"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialNumberField
                                                    label="Sequence"
                                                    name="Sequence"
                                                    type="text"
                                                    placeholder="Sequence"
                                                    length={1}
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialTextField
                                                    label="Display Name"
                                                    name="DisplayName"
                                                    type="text"
                                                    placeholder="Display Name"
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialTextField
                                                    label="UserName / UserID"
                                                    name="UserName"
                                                    type="text"
                                                    placeholder="UserName"
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialTextField
                                                    label="Token / secret key"
                                                    name="Token"
                                                    type="text"
                                                    placeholder="Token / secret key"
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialTextField
                                                    label="MerchantID"
                                                    name="MerchantID"
                                                    type="text"
                                                    placeholder="MerchantID"
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialTextField
                                                    label="CallBackURL / ActionURL"
                                                    name="CallBackURL"
                                                    type="text"
                                                    placeholder="CallBackURL / ActionURL"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialTextField
                                                    label="Mode"
                                                    name="Mode"
                                                    type="text"
                                                    placeholder="Mode"
                                                    length={5}
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
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
                                            {/* <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <FormikMaterialSwitch
                                                    label="IsNeft"
                                                    name="IsNeft"
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
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <FormikMaterialSwitch
                                                    label="IsCCF"
                                                    name="IsCCF"
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
                                            </Grid> */}
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <PrimaryButton
                                                    type="submit"
                                                    title="Update"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    disabled={loadingOpen}
                                                >
                                                    Update
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
                                ) : null}
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={100}
            />

            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    <strong> In Paysprint Account Opening : </strong> :
                    <span className={classes.normalTextColor}>
                        use <b>MerchantID</b> as JWTKEY, <b>[UserName]</b>
                        as ParentID, <b>[Token]</b> as Authorize Key{' '}
                        <b>CallBackURL contains(comma separated)</b> as AESKey
                        and AESIV
                    </span>
                </h3>
            </label>
            <br />
            <br />
        </CustomContainer>
    );
});
