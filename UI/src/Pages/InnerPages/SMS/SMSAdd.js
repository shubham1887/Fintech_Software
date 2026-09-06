import React, { useEffect, useRef, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { Add as SMS_ADD } from 'Components/Api/SMSMasterApi';
import { Activate as SMS_ACTIVATE } from 'Components/Api/SMSMasterApi';
import TextField from '@material-ui/core/TextField';
import { Select as SMS_SELECT } from 'Components/Api/SMSMasterApi';
import { SelectBalance as SMS_CHECK_BALANCE } from 'Components/Api/SMSMasterApi';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, Grid, Paper, MenuItem, Button } from '@material-ui/core';

import { CheckSMS } from 'Pages/InnerPages/Check/CheckSMS';
import { CheckEmail } from 'Pages/InnerPages/Check/CheckEmail';
import { CheckWhatsAppSMS } from 'Pages/InnerPages/Check/CheckWhatsAppSMS';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import Box from '@material-ui/core/Box';
import { CustomModal } from 'Components/Modal/CustomModal';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import Accordion from '@material-ui/core/Accordion';
const CustomContainer = HOCLoader(Container);
const CustomAccordion = HOCAccordion(Accordion);
export const SMSAdd = () => {
    const [Sms, setSms] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isUpdate, setIsUpdate] = useState(false);
    const [isSmsOpen, setIsSmsOpen] = useState(false);
    const [isEmailOpen, setIsEmailOpen] = useState(false);
    const [isWhatsAppSmsOpen, setIsWhatsAppSmsOpen] = useState('');
    const [balance, setBalance] = useState('');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [HeaderJson, setHeaderJson] = useState('none');
    const [HeaderJsonWhatsApp, setHeaderJsonForWhatsApp] = useState('none');
    const [openFilter, setopenFilter] = useState(true);
    const [mobileType, setmobileType] = useState('No');

    const formRef = useRef();
    useEffect(() => {
        setLoadingOpen(true);
        selectSMS();
    }, []);

    const [formObj, setFormObj] = useState({
        SMSMasterID: 0,
        APIName: '',
        SMSString: '',
        SMSOTPURL: '',
        BalanceURL: '',
        WhatsAppURL: '',
        MethodType: 0,
        HeaderJson: '',
        MethodTypeWhatsApp: 0,
        MobileWithOrWithout91: '',
        HeaderJsonWhatsApp: ''
    });
    const Reset = () => {
        setHeaderJson('none');
        setHeaderJsonForWhatsApp('none');
        setFormObj({
            SMSMasterID: 0,
            APIName: '',
            SMSString: '',
            SMSOTPURL: '',
            BalanceURL: '',
            WhatsAppURL: '',
            MethodType: 0,
            HeaderJson: '',
            MethodTypeWhatsApp: 0,
            MobileWithOrWithout91: '',
            HeaderJsonWhatsApp: ''
        });
    };
    const checkBalance = async (value) => {
        var jsonObj = {
            BalanceURL: value
        };

        var request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);

        const response = await SMS_CHECK_BALANCE(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) setSeverity('error');
        setIsError(true);

        setIsModalOpen(true);
        setBalance(response.data);
        // setopenFilter(false);
    };
    const activate = async (item, value) => {
        var jsonObj = {
            SMSMasterID: item,
            IsActive: value
        };

        var request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);
        const response = await SMS_ACTIVATE(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        if (!response.status) setSeverity('error');
        setIsError(true);
        // setopenFilter(false);
        selectSMS();
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        setIsError(false);
        var response = await SMS_ADD(requestObj);

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
        setIsUpdate(false);
        resetForm(formObj);
        Reset();
        selectSMS();
        // setopenFilter(false);
    };

    const formValidationSchema = Yup.object({
        APIName: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        SMSString: Yup.string().required(PLEASE_PROVIDE_VALID + 'sms url'),
        BalanceURL: Yup.string().required(PLEASE_PROVIDE_VALID + 'balance url'),
        MethodType: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'method type')
            .required(PLEASE_SELECT_VALID + 'method type'),
        MethodTypeWhatsApp: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'method type for whatsApp')
            .required(PLEASE_SELECT_VALID + 'method type for whatsApp')
    });

    const selectSMS = async () => {
        const response = await SMS_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            setLoadingOpen(false);
            return;
        }
        createColumns();
        var data = JSON.parse(response.data);
        setSms(data);
        setLoadingOpen(false);
    };
    const Edit = (
        SMSMasterID,
        APIName,
        MethodType,
        HeaderJson,
        SMSString,
        SMSOTPURL,
        BalanceURL,
        MethodTypeWhatsApp,
        HeaderJsonWhatsApp,
        WhatsAppURL,
        MobileWithOrWithout91
    ) => {
        // setopenFilter(true);

        if (MethodTypeWhatsApp === 'POST') {
            setHeaderJsonForWhatsApp('inline');
        }
        if (MethodTypeWhatsApp === 'GET') {
            setHeaderJsonForWhatsApp('none');
        }
        if (MethodType === 'POST') {
            setHeaderJson('inline');
        }
        if (MethodType === 'GET') {
            setHeaderJson('none');
        }
        setIsUpdate(true);
        setFormObj({
            SMSMasterID: SMSMasterID,
            APIName: APIName,
            SMSString: SMSString,
            SMSOTPURL: SMSOTPURL,
            BalanceURL: BalanceURL,
            WhatsAppURL: WhatsAppURL,
            MethodType: MethodType,
            HeaderJson: HeaderJson,
            MethodTypeWhatsApp: MethodTypeWhatsApp,
            MobileWithOrWithout91: MobileWithOrWithout91,
            HeaderJsonWhatsApp: HeaderJsonWhatsApp
        });
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'SMSMasterID',
            srno: true
        });
        columns.push({
            label: 'API',
            name: 'APIName',
            activedboolean: true
        });

        columns.push({
            label: 'MethodType',
            name: 'MethodType',
            activedboolean: true
        });
        columns.push({
            label: 'HeaderJson',
            name: 'HeaderJson',
            hide: true
        });
        columns.push({
            label: 'SMS Url',
            name: 'SMSString',
            //ReadOnlyTextArea: true
            displayAsPassword: true
        });

        columns.push({
            label: 'SMS Otp Url',
            name: 'SMSOTPURL',
            //ReadOnlyTextArea: true
            displayAsPassword: true
        });

        columns.push({
            label: 'Bal Url',
            name: 'BalanceURL',
            //ReadOnlyTextArea: true
            displayAsPassword: true
        });

        columns.push({
            label: 'MethodTypeWp',
            name: 'MethodTypeWhatsApp',
            activedboolean: true
        });
        columns.push({
            label: 'HeaderJsonWhatsApp',
            name: 'HeaderJsonWhatsApp',
            hide: true
        });
        columns.push({
            label: 'WhatsApp URL',
            name: 'WhatsAppURL',
            //ReadOnlyTextArea: true
            displayAsPassword: true
        });
        columns.push({
            label: 'Balance',
            name: 'BalanceURL',
            button: true,
            callback: (value) => {
                checkBalance(value);
            }
        });
        columns.push({
            label: 'MobileWithOrWithout91',
            name: 'MobileWithOrWithout91',
            hide: true
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
            update: true,
            callback: (row) => {
                Edit(
                    row[0],
                    row[1],
                    row[2],
                    row[3],
                    row[4],
                    row[5],
                    row[6],
                    row[7],
                    row[8],
                    row[10]
                );
            }
        });

        columns.push({
            label: 'IsActive',
            name: 'IsActive',
            hide: true
        });

        setMainColumns(columns);
    };
    const OnChange = (name, value, setFieldValue) => {
        if (value === 'POST') {
            setHeaderJson('inline');
        }
        if (value === 'GET') {
            setHeaderJson('none');
        }
    };

    const OnChangeForWhatsApp = (name, value, setFieldValue) => {
        if (value === 'POST') {
            setHeaderJsonForWhatsApp('inline');
        }
        if (value === 'GET') {
            setHeaderJsonForWhatsApp('none');
        }
    };

    const OnChangeMobileWithOrWithout91 = (name, value, setFieldValue) => {
        if (value === 'YES') {
            setmobileType('YES');
        }
        if (value === 'NO') {
            setmobileType('NO');
        }
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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={formObj}
                validationSchema={formValidationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
                render={(formProps, setFieldValue) => {
                    return (
                        <Form>
                            <Grid
                                container
                                spacing={2}
                                style={{ marginBottom: '15px' }}
                            >
                                <Grid item xs={12} sm={12} md={5} lg={5}>
                                    <h2 className={classes.PaperHead_h2}>
                                        SMS URL
                                    </h2>
                                </Grid>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <PrimaryButton
                                        variant="contained"
                                        color="primary"
                                        title="Sms Test"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={() => {
                                            setIsSmsOpen(true);
                                        }}
                                    >
                                        Sms Test
                                    </PrimaryButton>
                                </Grid>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <PrimaryButton
                                        variant="contained"
                                        color="primary"
                                        title="Email Test"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={() => {
                                            setIsEmailOpen(true);
                                        }}
                                    >
                                        Email Test
                                    </PrimaryButton>
                                </Grid>
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <PrimaryButton
                                        variant="contained"
                                        color="primary"
                                        title="WhatsApp Test"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={() => {
                                            setIsWhatsAppSmsOpen(true);
                                        }}
                                    >
                                        WhatsApp Test
                                    </PrimaryButton>
                                </Grid>
                            </Grid>

                            <Grid tem xs={12} sm={12} md={12} lg={12}>
                                <h3 className={classes.Provider_Format}>
                                    Note : use mobile=[mobile],
                                    message=[message], TemplateID=[TemplateID]
                                    parameters
                                </h3>
                            </Grid>
                            <CustomAccordion
                                title={'SMS URL'}
                                open={openFilter}
                                setOpen={(value) => setopenFilter(value)}
                            >
                                <Grid item xs={12}>
                                    <Paper className={classes.PaperStyle}>
                                        <Grid container spacing={3}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label="API Name"
                                                    name="APIName"
                                                    type="text"
                                                    placeholder="SMS URL"
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialSelect
                                                    label="MethodType"
                                                    name="MethodType"
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        OnChange(
                                                            name,
                                                            value,
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Type
                                                    </MenuItem>

                                                    <MenuItem
                                                        key="GET"
                                                        value="GET"
                                                    >
                                                        GET
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="POST"
                                                        value="POST"
                                                    >
                                                        POST
                                                    </MenuItem>
                                                </MaterialSelect>
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={6}
                                                lg={6}
                                            >
                                                <Box display={HeaderJson}>
                                                    <MaterialTextField
                                                        label="HeaderJson"
                                                        name="HeaderJson"
                                                        type="text"
                                                        placeholder="HeaderJson"
                                                    />
                                                </Box>
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <MaterialTextField
                                                    label="SMS URL"
                                                    name="SMSString"
                                                    type="text"
                                                    placeholder="SMS URL"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <MaterialTextField
                                                    label="SMS OTP URL"
                                                    name="SMSOTPURL"
                                                    type="text"
                                                    placeholder="SMS OTP URL"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialSelect
                                                    label="MethodType for WhatsApp"
                                                    name="MethodTypeWhatsApp"
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        OnChangeForWhatsApp(
                                                            name,
                                                            value,
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Type
                                                    </MenuItem>

                                                    <MenuItem
                                                        key="GET"
                                                        value="GET"
                                                    >
                                                        GET
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="POST"
                                                        value="POST"
                                                    >
                                                        POST
                                                    </MenuItem>
                                                </MaterialSelect>
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialSelect
                                                    label="Number With Or Without 91"
                                                    name="MobileWithOrWithout91"
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        OnChangeMobileWithOrWithout91(
                                                            name,
                                                            value,
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Type
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="NO"
                                                        value="NO"
                                                    >
                                                        NO
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="YES"
                                                        value="YES"
                                                    >
                                                        YES
                                                    </MenuItem>
                                                </MaterialSelect>
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={6}
                                                lg={6}
                                            >
                                                <Box
                                                    display={HeaderJsonWhatsApp}
                                                >
                                                    <MaterialTextField
                                                        label="HeaderJson for WhatsApp"
                                                        name="HeaderJsonWhatsApp"
                                                        type="text"
                                                        placeholder="HeaderJson for WhatsApp"
                                                    />
                                                </Box>
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <MaterialTextField
                                                    label="WhatsAppURL"
                                                    name="WhatsAppURL"
                                                    type="text"
                                                    placeholder="WhatsApp URL"
                                                />
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={12}
                                                lg={12}
                                            >
                                                <MaterialTextField
                                                    label="Balance URL"
                                                    name="BalanceURL"
                                                    type="text"
                                                    placeholder="Balance URL"
                                                />
                                            </Grid>
                                        </Grid>

                                        <Grid className={classes.footerGrid}>
                                            <Box
                                                className={classes.flexReverse}
                                            >
                                                <div
                                                    className={
                                                        classes.SubmitWrapper
                                                    }
                                                >
                                                    <Button
                                                        type="submit"
                                                        title="Add"
                                                        className={
                                                            classes.CustomPrimaryButtonStyle
                                                        }
                                                        disabled={loadingOpen}
                                                    >
                                                        {!isUpdate
                                                            ? 'Save'
                                                            : 'Update'}
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
                                                    onClick={() => {
                                                        Reset();
                                                    }}
                                                >
                                                    Reset
                                                </Button>
                                            </Box>
                                        </Grid>
                                    </Paper>
                                </Grid>
                            </CustomAccordion>
                        </Form>
                    );
                }}
            />

            <h2 className={classes.PaperHead_h2}>URL List</h2>
            <ReactTable data={Sms} columns={mainColumns} />

            {isModalOpen ? (
                <CustomModal
                    isOpen={isModalOpen}
                    title="SMS balance"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsModalOpen(value);
                    }}
                >
                    <TextField
                        value={balance}
                        label="Response"
                        variant="outlined"
                        multiline
                        rowsMax={4}
                        fullWidth
                    />
                </CustomModal>
            ) : null}

            {isSmsOpen ? (
                <CustomModal
                    isOpen={isSmsOpen}
                    title="Test Message"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsSmsOpen(value);
                    }}
                >
                    <CheckSMS
                        onDismissCallback={(value) => {
                            setIsSmsOpen(value);
                        }}
                    />
                </CustomModal>
            ) : null}
            {isEmailOpen ? (
                <CustomModal
                    isOpen={isEmailOpen}
                    title="Test Email"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsEmailOpen(value);
                    }}
                >
                    <CheckEmail
                        onDismissCallback={(value) => {
                            setIsEmailOpen(value);
                        }}
                    />
                </CustomModal>
            ) : null}

            {isWhatsAppSmsOpen ? (
                <CustomModal
                    isOpen={isWhatsAppSmsOpen}
                    title="Test WhatsApp Message"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsWhatsAppSmsOpen(value);
                    }}
                >
                    <CheckWhatsAppSMS
                        onDismissCallback={(value) => {
                            setIsWhatsAppSmsOpen(value);
                        }}
                    />
                </CustomModal>
            ) : null}
        </CustomContainer>
    );
};
