import React, { memo, useState, useEffect } from 'react';
import { useHistory } from 'react-router';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { useLocation } from 'react-router-dom';
import {
    Container,
    Grid,
    Box,
    Paper,
    CircularProgress,
    MenuItem,
    ListItemIcon,
    Typography
} from '@material-ui/core';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { FormikMaterialSwitch } from 'Components/Inputes/FormikMaterialSwitch';
import ReactTable from 'Components/ReactTable/ReactTable';

import {
    Select as BBPS_CONFIGURATION_SELECT,
    Update as BBPS_CONFIGURATION_UPDATE,
    SelectByID as BBPS_CONFIGURATION_SELECT_BY_ID,
    Add as BBPSCONFIGURATION_ADD,
    Delete as BBPS_CONFIGURATION_DELETE
} from 'Components/Api/BBPSConfigurationAPI';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import { LOGOUT } from 'ConstComponents';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';

import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);
export const BBPSConfiguration = memo(() => {
    const classes = commonStyle();
    const location = useLocation();
    const history = useHistory();
    const [serviceID] = useState(location.state ? location.state.ServiceID : 0);
    const [operatorID] = useState(
        location.state ? location.state.OperatorID : 0
    );
    const [Services, setServices] = useState([]);

    const [CustomContainerProps, setCustomContainerProps] = useState({
        loadingOpen: false,
        isError: false,
        severity: 'success',
        responseMessage: ''
    });

    const [IsUpdate, setIsUpdate] = useState(false);
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [IsBBPSConfigurationList, setIsBBPSConfigurationList] = useState(
        false
    );
    const [Type] = useState([
        { name: 'Text' },
        { name: 'Number' },
        { name: 'Email' },
        { name: 'List' }
    ]);

    const [FormObj, setFormObj] = useState({
        ConfigurationID: 0,
        ServiceID: serviceID,
        OperatorID: operatorID,
        Label: '',
        PlaceHolder: '',
        IsRequired: false,
        Sequence: '',
        ErrorMeeage: '',
        Type: '0',
        ConfigList: '',
        IsDisplay: false
    });

    useEffect(() => {
        if (serviceID > 0) selectOperator(serviceID);
    }, [serviceID]);

    useEffect(() => {
        if (operatorID > 0) setFormObj({ ...FormObj, OperatorID: operatorID });
    }, [operatorID]);

    useEffect(() => {
        createColumns();
        selectData();
        selectServices();
    }, []);

    const selectData = async () => {
        const {
            data,
            isAuthorized,
            status
        } = await BBPS_CONFIGURATION_SELECT();

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setCustomContainerProps({
                isError: true,
                severity: 'error',
                responseMessage: data
            });
            return;
        }

        setItemList(JSON.parse(data));
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'BBPSConfigurationID',
            srno: true
        });

        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });

        columns.push({
            label: 'Label',
            name: 'Label'
        });

        columns.push({
            label: 'Type',
            name: 'Type'
        });
        columns.push({
            label: 'PlaceHolder',
            name: 'PlaceHolder'
        });
        columns.push({
            label: 'IsRerquired',
            name: 'IsRerquiredStr'
        });
        columns.push({
            label: 'IsDisplay',
            name: 'IsDisplayStr'
        });
        columns.push({
            label: 'ErrorMeeage',
            name: 'ErrorMeeage'
        });

        columns.push({
            label: 'Sequence',
            name: 'Sequence'
        });

        columns.push({
            label: 'Edit',
            name: 'BBPSConfigurationID',
            edit: true,
            callback: (BBPSConfigurationID) => {
                Edit(BBPSConfigurationID);
            }
        });

        columns.push({
            label: 'Delete',
            name: 'BBPSConfigurationID',
            delete: true,
            callback: (values) => {
                Delete(values);
            }
        });

        setMainColumns(columns);
    };

    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const OnServiceChange = async (name, value, setFieldValue) => {
        var ServiceID = value; //event.target.value;

        setFieldValue(name, value);

        selectOperator(ServiceID);
    };

    const selectOperator = async (ServiceID) => {
        if (
            ServiceID === null ||
            ServiceID === undefined ||
            ServiceID === 0 ||
            ServiceID === '0'
        )
            return;

        var requestObj = {
            ServiceID: ServiceID
        };
        var request = JSON.stringify(requestObj);

        const response = await OPERATOR_SELECT_BY_SERVICEID(request);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status)
            setCustomContainerProps({
                isError: true
            });

        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);
            setOperator(data);
        }
    };

    const Edit = (BBPSConfigurationID) => {
        setIsUpdate(true);
        selectConfigurationByID(BBPSConfigurationID);
    };

    const selectConfigurationByID = async (BBPSConfigurationID) => {
        setCustomContainerProps({
            isError: false,
            loading: true
        });

        let jsonObj = {
            ConfigurationID: BBPSConfigurationID
        };
        const response = await BBPS_CONFIGURATION_SELECT_BY_ID(
            JSON.stringify(jsonObj)
        );
        setCustomContainerProps({
            loading: false
        });
        if (!response.isAuthorized) LOGOUT();

        const { data, status } = response;
        if (!status) {
            setCustomContainerProps({
                ErrorMeeage: data,
                isError: true,
                severity: 'error'
            });
        }

        let d = JSON.parse(data);

        if (d.Type === 'List') setIsBBPSConfigurationList(true);

        setFormObj({
            ConfigurationID: d.BBPSConfigurationID,
            OperatorID: d.OperatorID,
            ServiceID: d.ServiceID,
            Label: d.Label,
            PlaceHolder: d.PlaceHolder,
            IsRequired: d.IsRequired,
            IsDisplay: d.IsDisplay,
            Sequence: d.Sequence,
            ErrorMeeage: d.ErrorMeeage,
            Type: d.Type,
            ConfigList: d.ConfigList
        });
    };
    useEffect(() => {
        selectOperator(FormObj.ServiceID);
    }, [FormObj.ServiceID]);

    useEffect(() => {
        if (operatorID > 0) setFormObj({ ...FormObj, OperatorID: operatorID });
    }, [Operator]);

    const FormValidationSchema = Yup.object({
        Label: Yup.string().required(PLEASE_PROVIDE_VALID + 'service label'),
        PlaceHolder: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' placeholder'
        ),
        Sequence: Yup.string().required(PLEASE_PROVIDE_VALID + ' sequence')
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (CustomContainerProps.loading) {
            setCustomContainerProps(...CustomContainerProps, {
                loading: false
            });
            return;
        }
        var requestObj = JSON.stringify(values);

        setCustomContainerProps({
            isError: false,
            loading: true
        });

        var response = IsUpdate
            ? await BBPS_CONFIGURATION_UPDATE(requestObj)
            : await BBPSCONFIGURATION_ADD(requestObj);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setCustomContainerProps({
                isError: true,
                loading: false,
                severity: 'error',
                responseMessage: response.data
            });
            return;
        }
        setCustomContainerProps({
            isError: false,
            loading: false,
            severity: 'success',
            responseMessage: response.data
        });
        resetForm(FormObj);
        setIsBBPSConfigurationList(false);
        setFormObj({
            ConfigurationID: 0,
            OperatorID: 0,
            Label: '',
            PlaceHolder: '',
            IsRequired: false,
            IsDisplay: false,
            Sequence: '',
            ErrorMeeage: '',
            Type: '0',
            ConfigList: ''
        });
        setIsUpdate(false);
        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);
    };

    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
    };

    const Delete = async (values) => {
        let JsonObj = {
            ConfigurationID: values[0]
        };
        let JsonData = JSON.stringify(JsonObj);

        const { data, isAuthorized, status } = await BBPS_CONFIGURATION_DELETE(
            JsonData
        );

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setCustomContainerProps({
                isError: true,
                severity: 'error',
                responseMessage: data
            });
            return;
        }
        setCustomContainerProps({
            isError: true,
            severity: 'success',
            responseMessage: data
        });
        setIsUpdate(false);
        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);
    };

    const ServiceItems = Services.map(({ ServiceID, ServiceName }, index) => (
        <MenuItem key={index} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));

    const OperatorItems = Operator.map(
        ({ OperatorID, OperatorName, Path }, index) => (
            <MenuItem key={index} value={OperatorID}>
                <ListItemIcon style={{ verticalAlign: 'middle' }}>
                    <img src={Path} height={30} width={30} alt={OperatorName} />
                </ListItemIcon>
                {OperatorName}
            </MenuItem>
        )
    );

    const TypeItems = Type.map(({ name }, index) => (
        <MenuItem key={index} value={name}>
            {name}
        </MenuItem>
    ));

    const BBPSConfigurationForm = ({ setFieldValue }) => {
        return (
            <Form style={{ marginBottom: '15px' }}>
                <Grid item xs={12}>
                    <Paper className={classes.PaperStyle}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="Service"
                                    name="ServiceID"
                                    onCustomCallback={(name, value) => {
                                        OnServiceChange(
                                            name,
                                            value,
                                            setFieldValue
                                        );
                                    }}
                                    title="Service"
                                >
                                    <MenuItem key="0" value={0}>
                                        Select Service
                                    </MenuItem>
                                    {ServiceItems}
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="Select Operator"
                                    name="OperatorID"
                                    title="Operator"
                                >
                                    <MenuItem key="0" value="0">
                                        Select Operator
                                    </MenuItem>
                                    {OperatorItems}
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="Select Type"
                                    name="Type"
                                    title="Type"
                                    onCustomCallback={(name, value) => {
                                        setIsBBPSConfigurationList(
                                            value === 'List' ? true : false
                                        );
                                    }}
                                >
                                    <MenuItem key="0" value={'0'}>
                                        Select Type
                                    </MenuItem>
                                    {TypeItems}
                                </MaterialSelect>
                            </Grid>
                            {IsBBPSConfigurationList === true ? (
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <MaterialTextField
                                        label="ConfigList"
                                        name="ConfigList"
                                        type="text"
                                        placeholder="ConfigList"
                                    />
                                    <label style={{ color: 'green' }}>
                                        NOTE: Enter data in JSON format
                                    </label>
                                </Grid>
                            ) : null}
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialTextField
                                    label="Label"
                                    name="Label"
                                    type="text"
                                    placeholder="Label"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialTextField
                                    label="PlaceHolder"
                                    name="PlaceHolder"
                                    type="text"
                                    placeholder="PlaceHolder"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <FormikMaterialSwitch
                                    label="IsRequired"
                                    name="IsRequired"
                                    onCustomCallback={(name, value) => {
                                        OnChange(name, value, setFieldValue);
                                    }}
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <FormikMaterialSwitch
                                    label="IsDisplay"
                                    name="IsDisplay"
                                    onCustomCallback={(name, value) => {
                                        OnChange(name, value, setFieldValue);
                                    }}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialTextField
                                    label="ErrorMeeage"
                                    name="ErrorMeeage"
                                    type="text"
                                    placeholder="ErrorMeeage"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialNumberField
                                    label="Sequence"
                                    name="Sequence"
                                    type="text"
                                    length={7}
                                    placeholder="Sequence"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <PrimaryButton
                                    type="submit"
                                    title={IsUpdate ? 'Update' : 'Save'}
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={CustomContainerProps.loading}
                                >
                                    {IsUpdate ? 'Update' : 'Save'}
                                </PrimaryButton>
                                {CustomContainerProps.loading && (
                                    <CircularProgress
                                        size={24}
                                        className={classes.buttonProgress}
                                    />
                                )}
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <SecondaryButton
                                    type="reset"
                                    title="Reset"
                                    className={classes.CustomDangerButtonStyle}
                                >
                                    Reset
                                </SecondaryButton>
                            </Grid>
                        </Grid>
                    </Paper>
                </Grid>
            </Form>
        );
    };

    const BBPSConfigurationReport = (
        <ReactTable data={itemList} columns={mainColumns} />
    );
    return (
        <CustomContainer
            isLoading={CustomContainerProps.loadingOpen}
            isError={CustomContainerProps.isError}
            alertMessage={CustomContainerProps.responseMessage}
            severity={CustomContainerProps.severity}
            onCloseCallback={(alertState) => {
                setCustomContainerProps({
                    isError: alertState
                });
            }}
        >
            <Grid item container xs={12} style={{ marginBottom: '15px' }}>
                <Grid item xs={12} sm={12} md={10} lg={10}>
                    <h2 className={classes.PaperHead_h2}>BBPSConfiguration</h2>
                    <Typography>
                        <span style={{ color: 'green' }}>
                            <b>
                                To take parameter from bill fetch response use
                                ResponseParam1,ResponseParam2,ResponseParam3,ResponseParam4,ResponseParam5
                                in Label Text
                            </b>
                        </span>
                    </Typography>
                </Grid>

                <Grid item xs={12} sm={12} md={2} lg={2}>
                    <Box className={classes.flexReverse}>
                        <PrimaryButton
                            variant="contained"
                            color="primary"
                            title="Operator List"
                            className={classes.CustomPrimaryButtonStyle}
                            onClick={() => {
                                history.push('/Operators/Index');
                            }}
                        >
                            Operator List
                        </PrimaryButton>
                    </Box>
                </Grid>
            </Grid>

            {/* <FormHeader
                shouldDisplayButton={true}
                buttonName="Operator List"
                link="/Operators/Index"
            /> */}

            <Formik
                enableReinitialize={true}
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={handleSubmit}
                resetForm={FormObj}
            >
                {BBPSConfigurationForm}
            </Formik>
            {BBPSConfigurationReport}
        </CustomContainer>
    );
});
