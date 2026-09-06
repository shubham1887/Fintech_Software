import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import {
    Container,
    Grid,
    Button,
    Box,
    CircularProgress,
    Paper
} from '@material-ui/core';
import {
    Insert as INSERT_IP,
    Select as SELECT_IPS,
    Delete as DELETE_IP
} from 'Components/Api/IPAddAPI';
import { Send as SEND_OTP } from 'Components/Api/OTPAPI';
import { UpdateIsSecureIP, SelectIsSecureIP } from 'Components/Api/UsersApi';
import { OTPModal } from 'Components/Modal/OTPModal';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import HOCLoader from 'Components/Loader/HOCLoader';
import ReactTable from 'Components/ReactTable/ReactTable';
const CustomContainer = HOCLoader(Container);

export const IPAdd = () => {
    const classes = commonStyle();
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [selectedIP, setSelectedIP] = useState('');
    const [selectedIsIPSecure, setSelectedIsIPSecure] = useState('');
    const [id, setId] = useState(0);
    const [isUpdate, setIsUpdate] = useState(false);
    const [modalErrorMessage, setModalErrorMessage] = useState('');
    const [openOTP, setOpenOTP] = useState(false);
    const [openOTPForSecureIP, setOpenOTPForSecureIP] = useState(false);
    const [SecureIPObj, setSecureIPObj] = useState({
        IsIPSecure: true
    });

    var IPObj = {
        IP: '',
        IPMasterID: id
    };

    const IPValidationSchema = Yup.object({
        IP: Yup.string().required('Please enter IP address')
    });

    const SecureIPSubmitHandle = async (
        values,
        { setSubmitting, setErrors, setStatus, resetForm }
    ) => {
        setSelectedIsIPSecure(values.IsIPSecure);
        setLoadingOpen(true);
        var response;

        if (!values.IsIPSecure) {
            response = await SendOTP('SecureIPOTP');
            setLoadingOpen(false);
            setIsError(false);
            if (!response.isAuthorized) LOGOUT();
            if (response.data === 'True') setOpenOTPForSecureIP(true);
            resetForm(SecureIPObj);
        } else {
            response = await InsertSecureIP({ OTP: '1234' });
            setLoadingOpen(false);
        }
    };

    const InsertSecureIP = async (values) => {
        var jsonObj = {
            OTP: values.OTP,
            IsIPSecure: selectedIsIPSecure
        };
        var requestObj = JSON.stringify(jsonObj);

        setIsError(false);

        var response = await UpdateIsSecureIP(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }
        setOpenOTPForSecureIP(false);

        SelectInitialIsSecureIP();
    };

    useEffect(() => {
        SelectInitialIsSecureIP();
        setIsUpdate(false);
        createColumns();
        selectData();
    }, []);

    const SelectInitialIsSecureIP = async () => {
        setLoadingOpen(true);

        setIsError(false);
        var response = await SelectIsSecureIP('');

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);

        setSecureIPObj({
            IsIPSecure: data.IsIPSecure
        });
    };
    const selectData = async () => {
        setLoadingOpen(true);

        setIsError(false);

        var response = await SELECT_IPS('');

        setLoadingOpen(false);
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

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'IPMasterID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'Date'
        });
        columns.push({
            label: 'UpdatedDate',
            name: 'UpdatedDate'
        });

        columns.push({
            label: 'IP',
            name: 'IPAddress'
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
                Edit(row[3], row[0]);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'IPMasterID',
            delete: true,
            callback: (value) => {
                {
                    onDeleteClick(value);
                }
            }
        });

        setMainColumns(columns);
    };

    const onDeleteClick = (item) => {
        var RequestObj = { IpMasterID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (requestObj) => {
        setLoadingOpen(true);
        var reqObj = JSON.stringify(requestObj);
        var response = await DELETE_IP(reqObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }

        window.location.reload();
    };

    const SendOTP = async (Type) => {
        var jsonObj = {
            Type: Type
        };
        return await SEND_OTP(JSON.stringify(jsonObj));
    };
    const submitHandle = async (
        values,
        { setSubmitting, setErrors, setStatus, resetForm }
    ) => {
        setSelectedIP(values.IP);
        setLoadingOpen(true);
        setIsError(false);
        var response = await SendOTP('IPMaster');
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        if (response.data === 'True') setOpenOTP(true);
        resetForm(IPObj);
    };

    const Edit = (IP, IpMasterID) => {
        setSelectedIP(IP);
        setId(IpMasterID);
        setIsUpdate(true);
        IPObj.IP = IP;
        IPObj.IpMasterID = IpMasterID;
    };

    const Insert = async (values) => {
        var jsonObj = {
            OTP: values.OTP,
            IP: selectedIP
        };
        var requestObj = JSON.stringify(jsonObj);

        setIsError(false);

        var response = await INSERT_IP(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }

        setOpenOTP(false);
        selectData();
    };

    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
        setSelectedIsIPSecure(value);
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
                    <Formik
                        initialValues={IPObj}
                        validationSchema={IPValidationSchema}
                        onSubmit={submitHandle}
                        resetForm={IPObj}
                    >
                        {(formProps) => {
                            return (
                                <Form>
                                    <h2 className={classes.PaperHead_h2}>
                                        IP Add
                                    </h2>

                                    <Grid item xs={12} md={12} lg={12}>
                                        <Grid item xs={12} md={12} lg={12}>
                                            <Paper
                                                className={classes.PaperStyle}
                                            >
                                                <Grid container spacing={1}>
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={6}
                                                        lg={6}
                                                    >
                                                        <MaterialTextField
                                                            label="IP Address"
                                                            name="IP"
                                                            type="text"
                                                            placeholder="IP Address"
                                                            title="IP Address"
                                                        />
                                                    </Grid>
                                                </Grid>
                                            </Paper>
                                        </Grid>
                                        <Grid item xs={12} md={12} lg={12}>
                                            <Grid
                                                className={classes.footerGrid}
                                            >
                                                <Box
                                                    className={
                                                        classes.flexReverse
                                                    }
                                                >
                                                    <div
                                                        className={
                                                            classes.SubmitWrapper
                                                        }
                                                    >
                                                        <Button
                                                            type="submit"
                                                            title="Save"
                                                            className={
                                                                classes.CustomPrimaryButtonStyle
                                                            }
                                                            disabled={
                                                                loadingOpen
                                                            }
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
                                                    >
                                                        Reset
                                                    </Button>
                                                </Box>
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                </Form>
                            );
                        }}
                    </Formik>
                </Grid>
                <Grid item xs={6}>
                    <Formik
                        enableReinitialize={true}
                        initialValues={SecureIPObj}
                        onSubmit={SecureIPSubmitHandle}
                        resetForm={IPObj}
                    >
                        {(formProps) => {
                            return (
                                <Form>
                                    <h2 className={classes.PaperHead_h2}>
                                        Secure IP
                                    </h2>

                                    <Grid item xs={12} md={12} lg={12}>
                                        <Grid item xs={12} md={12} lg={12}>
                                            <Paper
                                                className={classes.PaperStyle}
                                            >
                                                <Grid container spacing={1}>
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={4}
                                                        lg={4}
                                                    >
                                                        <FormikMaterialSwitch
                                                            label="Is IPSecure"
                                                            name="IsIPSecure"
                                                            title="Is IPSecure"
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
                                        </Grid>
                                        <Grid item xs={12} md={12} lg={12}>
                                            <Grid
                                                className={classes.footerGrid}
                                            >
                                                <Box
                                                    className={
                                                        classes.flexReverse
                                                    }
                                                >
                                                    <div
                                                        className={
                                                            classes.SubmitWrapper
                                                        }
                                                    >
                                                        <Button
                                                            type="submit"
                                                            title="Save"
                                                            className={
                                                                classes.CustomPrimaryButtonStyle
                                                            }
                                                            disabled={
                                                                loadingOpen
                                                            }
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
                                                    >
                                                        Reset
                                                    </Button>
                                                </Box>
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                </Form>
                            );
                        }}
                    </Formik>
                </Grid>
            </Grid>
            {openOTP ? (
                <OTPModal
                    isOpen={openOTP}
                    title="OTP"
                    error={modalErrorMessage}
                    submitCallback={(value) => {
                        Insert(value);
                    }}
                    resendCallback={(value) => {}}
                    onDismissCallback={(value) => {
                        setOpenOTP(value);
                    }}
                />
            ) : null}
            {openOTPForSecureIP ? (
                <OTPModal
                    isOpen={openOTPForSecureIP}
                    title="OTP"
                    error={modalErrorMessage}
                    submitCallback={(value) => {
                        InsertSecureIP(value);
                    }}
                    resendCallback={(value) => {}}
                    onDismissCallback={(value) => {
                        setOpenOTPForSecureIP(value);
                    }}
                />
            ) : null}
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this IP."
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Delete(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
