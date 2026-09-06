import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    Box,
    Button,
    Paper,
    CircularProgress,
    MenuItem
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    SelectAll as SERVICE_SELECT_ALL,
    ServiceSelectByID as SERVICE_SELECT_BY_ID,
    Update as SERVICE_UPDATE,
    Activate as SERVICE_ACTIVATE,
    Add as SERVICE_ADD,
    Display as SERVICE_Display,
    DisplayDefault as SERVICE_Default,
    DefaultOutside as DEFAULT_OUTSIDE,
    SubscriptionAllowOnOFF as SUBSCRIPTION_ALLOW,
    AllSericesActiveDeactiveGenerateOTP as ALL_SERVICES_ACTIVE_DEACTIVATE_GENERATE_OTP,
    AllSericesActiveDeactiveVeriftyOTP as All_SERVICES_ACTIVE_DEACTIVATE_VERIFTY_OTP
} from 'Components/Api/ServiceApi';
import { OTPModal } from 'Components/Modal/OTPModal';
import ReactTable from 'Components/ReactTable/ReactTable';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const Services = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);

    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [IsUpdate, setIsUpdate] = useState(false);
    const [IsAllServicesActive, setIsAllServicesActive] = useState(false);
    const [IsOTPSent, setIsOTPSent] = useState(false);

    const selectData = async () => {
        const response = await SERVICE_SELECT_ALL();
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
    const allServicesActiveDeactivateGenerateOTP = async (value) => {
        setIsError(false);
        setLoadingOpen(true);
        const response = await ALL_SERVICES_ACTIVE_DEACTIVATE_GENERATE_OTP('');
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
        }

        let result = JSON.parse(response.data);
        setSeverity('success');
        setResponseMessage(response.data);

        if (result.IsOTPSent === true) {
            setIsOTPSent(true);
        }
        return;
    };
    const allServicesActiveDeactivateVerifyOTP = async (values) => {
        const { OTP } = values;
        var jsonObj = {
            IsActive: IsAllServicesActive,
            OTP: OTP
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await All_SERVICES_ACTIVE_DEACTIVATE_VERIFTY_OTP(
            request
        );
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        if (!response.status) {
            setSeverity('error');
            setIsOTPSent(true);
        }
        if (response.status) {
            setSeverity('success');
            setIsOTPSent(false);
        }
        setResponseMessage(response.data);
        selectData();
    };

    const activate = async (item, value) => {
        var jsonObj = {
            ServiceID: item,
            IsActive: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await SERVICE_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };
    const DisplayService = async (item, value) => {
        var jsonObj = {
            ServiceID: item,
            IsDisplay: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await SERVICE_Display(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };

    const DisplayDefault = async (item, value) => {
        var jsonObj = {
            ServiceID: item,
            IsDefault: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await SERVICE_Default(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };

    const SubscriptionAllowOnOff = async (item, value) => {
        var jsonObj = {
            ServiceID: item,
            IsSubscriptionAllow: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await SUBSCRIPTION_ALLOW(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };

    const DefaultOutside = async (item, value) => {
        var jsonObj = {
            ServiceID: item,
            IsOutside: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await DEFAULT_OUTSIDE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };

    const Edit = async (ServiceID) => {
        setIsUpdate(true);
        selectByID(ServiceID);
    };

    const selectByID = async (ServiceID) => {
        let JSONObj = {
            ServiceID: ServiceID
        };

        var request = JSON.stringify(JSONObj);
        setLoadingOpen(true);
        const response = await SERVICE_SELECT_BY_ID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);

        setFormObject({
            ServiceID: data.ServiceID,
            ServiceName: data.ServiceName,
            Charge: data.Charge,
            MinBalance: data.MinBalance,
            ServiceCategory: data.ServiceCategory
        });
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ServiceID',
            srno: true
        });
        // columns.push({
        //     label: 'ServiceID',
        //     name: 'ServiceID'
        // });

        columns.push({
            label: 'Name',
            name: 'ServiceName',
            deactived: true
        });

        columns.push({
            label: 'Charge',
            name: 'Charge',
            numeric: true,
            textwrapwidth: 70,
            deactived: true
        });

        columns.push({
            label: 'MinBalance',
            name: 'MinBalance',
            numeric: true,
            textwrapwidth: 70,
            deactived: true
        });
        columns.push({
            label: 'ServiceCategory',
            name: 'ServiceCategory',
            textwrapwidth: 70,
            deactived: true
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
            label: 'Display',
            name: 'IsDisplay',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                DisplayService(id, value);
            }
        });
        columns.push({
            label: 'Default',
            name: 'IsDefault',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                DisplayDefault(id, value);
            }
        });
        columns.push({
            label: 'DefaultOutside',
            name: 'IsOutside',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                DefaultOutside(id, value);
            }
        });
        // columns.push({
        //     label: 'Subscription',
        //     name: 'IsSubscriptionAllow',
        //     activate: true,
        //     primaryKeyIndex: 0,
        //     callback: (id, value) => {
        //         SubscriptionAllowOnOff(id, value);
        //     }
        // });

        columns.push({
            label: 'Edit',
            name: 'ServiceID',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });
        columns.push({
            label: 'IsActive',
            name: 'IsActive',
            hide: true
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        selectData();
    }, []);

    const [formObject, setFormObject] = useState({
        ServiceID: 0,
        ServiceName: '',
        Charge: 0,
        MinBalance: 0,
        ServiceCategory: ''
    });

    const FormValidationSchema = Yup.object({
        ServiceName: Yup.string().required('Please enter valid service name'),
        Charge: Yup.number()
            .moreThan(-1, 'Please enter valid charge amount')
            .lessThan(1000, 'Please enter valid min. balance amount')
            .required('Please enter valid charge amount'),
        MinBalance: Yup.number()
            .moreThan(-1, 'Please enter valid min. balance amount')
            .lessThan(10000, 'Please enter valid min. balance amount')
            .required('Please enter valid min. balance amount'),
        ServiceCategory: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'service category')
            .required(PLEASE_SELECT_VALID + 'service category')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);
        setSubmitting(false);
        setIsError(false);
        setLoadingOpen(true);
        var response = IsUpdate
            ? await SERVICE_UPDATE(requestObj)
            : await SERVICE_ADD(requestObj);
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
        setIsUpdate(false);
        resetForm(formObject);
        setFormObject({
            ServiceID: 0,
            ServiceName: '',
            Charge: 0,
            MinBalance: 0
        });
        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);
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
                initialValues={formObject}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {() => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>Service </h2>
                            <div style={{ float: 'right' }}>
                                Activate/Deactivate Bulk Services ?{' '}
                                <Button
                                    variant="contained"
                                    color="primary"
                                    className={classes.CustomPrimaryButtonStyle}
                                    onClick={() => {
                                        allServicesActiveDeactivateGenerateOTP();
                                        setIsAllServicesActive(true);
                                    }}
                                >
                                    Active
                                </Button>{' '}
                                <Button
                                    variant="contained"
                                    color="primary"
                                    className={classes.CustomPrimaryButtonStyle}
                                    onClick={() => {
                                        allServicesActiveDeactivateGenerateOTP();
                                        setIsAllServicesActive(false);
                                    }}
                                >
                                    DeActive
                                </Button>
                            </div>

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
                                                    label="Service Name"
                                                    name="ServiceName"
                                                    type="text"
                                                    placeholder="Service Name"
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
                                                <MaterialNumberField
                                                    label="Service Charge"
                                                    name="Charge"
                                                    type="text"
                                                    length={3}
                                                    placeholder="Service Charge"
                                                />
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialNumberField
                                                    label="Min. Balance"
                                                    name="MinBalance"
                                                    type="text"
                                                    length={4}
                                                    placeholder="Min. Balance"
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
                                                    label="ServiceCategory"
                                                    name="ServiceCategory"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Service Category
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="P2P"
                                                        value="P2P"
                                                    >
                                                        P2P
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="P2A Banking RCM"
                                                        value="P2A Banking RCM"
                                                    >
                                                        P2A Banking RCM
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="P2A Banking Non RCM"
                                                        value="P2A Banking Non RCM"
                                                    >
                                                        P2A Banking Non RCM
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="P2A Non Banking"
                                                        value="P2A Non Banking"
                                                    >
                                                        P2A Non Banking
                                                    </MenuItem>
                                                </MaterialSelect>
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
            {IsOTPSent && (
                <OTPModal
                    isOpen={IsOTPSent}
                    title="OTP"
                    submitCallback={(value) => {
                        allServicesActiveDeactivateVerifyOTP(value);
                    }}
                    resendCallback={(value) => {}}
                    onDismissCallback={(value) => {}}
                />
            )}
            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={100}
            />
        </CustomContainer>
    );
};
