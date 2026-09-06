import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { Select as PROVIDER_SELECT } from 'Components/Api/ProviderApi';
import { Select as DATEFORMAT_SELECT } from 'Components/Api/DateFormatAPI';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import { Add as PROVIDERDETAILS_ADD } from 'Components/Api/ProviderDetailAPI';
import { Select as UrlType_SELECT } from 'Components/Api/UrlTypeApi';

import { Activate as PROVIDER_DETAILS_ACTIVATE } from 'Components/Api/ProviderDetailAPI';
import { Delete as PROVIDER_DETAILS_DELETE } from 'Components/Api/ProviderDetailAPI';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import {
    SelectByProviderID as SELECT_BY_PROVIDERID,
    FetchAllSampleURLFromSupport
} from 'Components/Api/ProviderDetailAPI';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { Container, Grid, MenuItem, Paper } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';
import { ProviderURLINFOView } from 'Pages/InnerPages/Providers/ProviderURLInfo';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { useLocation } from 'react-router-dom';
import { SelectBySampleID as SELECT_BY_SampleID } from 'Components/Api/ProviderApi';

import { MaterialTextArea } from 'Components/Inputes/FormikMaterialTextArea';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);
export const ProviderDetailAdd = () => {
    const location = useLocation();
    const ID = location.state.ID;
    var IsPendingProviderOn =
        localStorage.getItem('IsPendingProvider') === 'yes';
    const [primaryID] = useState(ID);
    const [Services, setServices] = useState([]);
    const [Providers, setProviders] = useState([]);
    const [DateFormats, setDateFormats] = useState([]);

    const [UrlTypes, setUrlType] = useState([]);
    const [ProviderDetails, setProviderDetails] = useState([]);
    const [HeaderJson, setHeaderJson] = useState('none');
    const [transactionIDPrefix, setTransactionIDPrefix] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const classes = commonStyle();
    const history = useHistory();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isUpdate, setIsUpdate] = useState(false);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [modalOpen, setModalOpen] = useState(false);
    const [providerID, setProviderID] = useState(0);
    const [FormObj, setFormObj] = useState({
        ProviderDetailID: 0,
        ProviderID: primaryID,
        ProviderName: '',
        ServiceID: 0,
        ServiceName: '',
        UrlType: '0',
        ProviderURL: '',
        MethodType: 0,
        HeaderJson: '',
        DateFormat: '0',
        IsPendingProvider: false,
        TransactionIDPrefix: ''
    });

    const OnChangeProviderID = (name, value, setFieldValue) => {
        setFieldValue(name, value);
        SelectByProviderID(value);
    };

    const onDeleteClick = (item) => {
        var RequestObj = { ProviderDetailID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const OnChange = (name, value, setFieldValue) => {
        if (value === 'POST') {
            setHeaderJson('inline');
        }
        if (value === 'GET') {
            setHeaderJson('none');
        }
    };
    const OnChangeIsPendingProvider = (name, value, setFieldValue) => {
        if (value === 'true') {
            setTransactionIDPrefix(true);
        }
        if (value === 'false') {
            setTransactionIDPrefix(false);
        }
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);
        setIsError(false);

        var response = await PROVIDERDETAILS_ADD(requestObj);
        if (!response.isAuthorized) LOGOUT();

        setLoadingOpen(false);
        setIsError(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }
        resetForm(FormObj);
        setIsUpdate(false);
        setFormObj({
            ...FormObj,

            ProviderDetailID: 0,
            ProviderID: primaryID,
            ProviderName: '',
            ServiceID: 0,
            ServiceName: '',
            UrlType: '0',
            ProviderURL: '',
            MethodType: 0,
            HeaderJson: '',
            DateFormat: '0',
            IsPendingProvider: false,
            TransactionIDPrefix: ''
        });
        SelectByProviderID(primaryID);
    };

    useEffect(() => {
        setLoadingOpen(true);
        selectServices();
        selectProviders();
        selectDateFormat();
        selectUrlType();

        createColumns();
        setLoadingOpen(false);
        SelectByProviderID(primaryID);
    }, []);

    const SelectByProviderID = async (ProviderID) => {
        setProviderDetails([]);
        var requestObj = {
            ProviderID: ProviderID
        };
        var request = JSON.stringify(requestObj);
        setLoadingOpen(true);
        const response = await SELECT_BY_PROVIDERID(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderDetails(data);
    };

    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const selectProviders = async () => {
        const response = await PROVIDER_SELECT();
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviders(data);
    };

    const selectDateFormat = async () => {
        const response = await DATEFORMAT_SELECT();
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setDateFormats(data);
    };

    const selectUrlType = async () => {
        const response = await UrlType_SELECT();
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setUrlType(data);
    };

    const Activate = async (item, value) => {
        var jsonObj = {
            ProviderDetailID: item,
            IsActive: value
        };
        var request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);
        const response = await PROVIDER_DETAILS_ACTIVATE(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        if (!response.status) setSeverity('error');
        setIsError(true);
        SelectByProviderID(primaryID);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);

        const response = await PROVIDER_DETAILS_DELETE(jsonReq);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        SelectByProviderID(primaryID);
    };

    const FormValidationSchema = Yup.object({
        ProviderID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'providerID')
            .required(PLEASE_SELECT_VALID + 'providerID'),
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'ServiceID')
            .required(PLEASE_SELECT_VALID + 'serviceID'),
        UrlType: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'UrlType')
            .required(PLEASE_SELECT_VALID + 'UrlType'),
        ProviderURL: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'Provider URL'
        ),
        MethodType: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'MethodType')
            .required(PLEASE_SELECT_VALID + 'MethodType'),

        TransactionIDPrefix: Yup.string().when(['IsPendingProvider'], {
            is: (IsPendingProvider) => IsPendingProvider === 'true',
            then: Yup.string().required('Please enter transactionid prefix ')
        })
    });

    const Edit = (
        ProviderDetailID,
        ProviderName,
        ServiceName,
        UrlType,
        ProviderURL,
        MethodType,
        HeaderJson,
        ProviderID,
        ServiceID,
        DateFormat,
        TransactionIDPrefix
    ) => {
        setIsUpdate(true);
        if (TransactionIDPrefix.length > 1) setTransactionIDPrefix(true);
        if (MethodType === 'POST') setHeaderJson('inline');

        setFormObj({
            ...FormObj,
            ProviderID: ProviderID,
            ProviderDetailID: ProviderDetailID,
            ProviderName: ProviderName,
            ServiceID: ServiceID,
            ServiceName: ServiceName,
            UrlType: UrlType,
            ProviderURL: ProviderURL,
            MethodType: MethodType,
            HeaderJson: HeaderJson,
            DateFormat: DateFormat,
            TransactionIDPrefix: TransactionIDPrefix
        });
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ProviderDetailID',
            srno: true
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });

        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'Type',
            name: 'UrlType'
        });
        columns.push({
            label: 'URL',
            name: 'ProviderURL',
            ReadOnlyTextArea: true
        });
        columns.push({
            label: 'Method',
            name: 'MethodType'
        });
        columns.push({
            label: 'Header',
            name: 'HeaderJson'
        });
        columns.push({
            label: 'ProviderID',
            name: 'ProviderID',
            hide: true
        });
        columns.push({
            label: 'ServiceID',
            name: 'ServiceID',
            hide: true
        });
        columns.push({
            label: 'DateFormat',
            name: 'DateFormat'
        });
        columns.push({
            label: 'TransIDPrefix',
            name: 'TransactionIDPrefix'
        });

        columns.push({
            label: 'Active',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                Activate(id, value);
            }
        });

        columns.push({
            label: 'Edit',
            name: 'Edit',
            icon: 'EditIcon',
            iconButtonWithRow: true,

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
                    row[9],
                    row[10]
                );
            }
        });

        columns.push({
            label: 'Delete',
            name: 'ProviderDetailID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });

        setMainColumns(columns);
    };
    const SelectURLInfo = async () => {
        setModalOpen(true);

        setProviderID(primaryID);
    };
    const handleDismissCallback = () => {
        setModalOpen(false);
    };

    const handleCopyCallback = (ProviderDetailsSampleID) => {
        selectSampleProvider(ProviderDetailsSampleID);
        setModalOpen(false);
    };
    const selectSampleProvider = async (ProviderDetailsSampleID) => {
        let jsonObj = {
            ProviderDetailsSampleID: ProviderDetailsSampleID
        };

        const response = await SELECT_BY_SampleID(JSON.stringify(jsonObj));

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        let data = JSON.parse(response.data);

        setFormObj({
            ...FormObj,
            MethodType: data.MethodType,
            ProviderURL: data.ProviderURL,
            UrlType: data.UrlType
        });

        // setUrlType(data);
    };
    const handleSetAllURLCallback = (providerID) => {
        SelectByProviderID(providerID);
        setModalOpen(false);
    };
    const OpenSetting = (providerID) => {
        var param = {
            ID: providerID
        };
        history.push('/ProviderConfiguration/Index/', param);
    };

    const OpenResponse = (providerID) => {
        var param = {
            ID: providerID
        };
        history.push('/ResponseAdd/', param);
    };
    const FetchAllSampleURL = async () => {
        const response = await FetchAllSampleURLFromSupport();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
    };

    const providerURlInfoModal = modalOpen ? (
        <ProviderURLINFOView
            providerID={providerID}
            modalOpen={modalOpen}
            onDismissCallback={handleDismissCallback}
            onCopyCallback={handleCopyCallback}
            onSetAllURLCallback={handleSetAllURLCallback}
        />
    ) : null;
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
                render={({ values, setFieldValue, handleChange }) => {
                    return (
                        <Form>
                            <Grid
                                container
                                spacing={2}
                                style={{ marginBottom: '15px' }}
                            >
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Provider Details
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <PrimaryButton
                                        variant="contained"
                                        color="primary"
                                        title="Setting"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={() => {
                                            OpenSetting(primaryID);
                                        }}
                                    >
                                        Setting
                                    </PrimaryButton>
                                </Grid>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <PrimaryButton
                                        variant="contained"
                                        color="primary"
                                        title="Response"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={() => {
                                            OpenResponse(primaryID);
                                        }}
                                    >
                                        Response
                                    </PrimaryButton>
                                </Grid>

                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <PrimaryButton
                                        variant="contained"
                                        color="primary"
                                        title="Sample URL"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={() => {
                                            SelectURLInfo();
                                        }}
                                    >
                                        Sample URL
                                    </PrimaryButton>
                                </Grid>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <PrimaryButton
                                        variant="contained"
                                        color="primary"
                                        title="New Provider"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={() => {
                                            history.push('/ProvidersAdd');
                                        }}
                                    >
                                        New Provider
                                    </PrimaryButton>
                                </Grid>

                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <PrimaryButton
                                        variant="contained"
                                        color="primary"
                                        title="Refresh Sample URL"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={() => {
                                            FetchAllSampleURL();
                                        }}
                                    >
                                        Refresh Sample URL
                                    </PrimaryButton>
                                </Grid>
                            </Grid>

                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <h3 className={classes.Provider_Format}>
                                    Transaction Parameters : use
                                    Amount=[Amount], Number=[Number],
                                    Operator=[Operator], RequestID=[RequestID],
                                    RefID=[RefID], Type=[Type], Date=[Date],
                                    Circle=[Circle], LoginID=[LoginID],
                                    Token=[Token], IFSC=[IFSC], Remark=[Remark],
                                    StateCode=[StateCode], Lapu=[Lapu] for URL
                                    parameters
                                </h3>
                            </Grid>
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <h3 className={classes.Provider_Format}>
                                    View Parameters: use Amount=[Amount],
                                    Number=[Number], Operator=[Operator],
                                    Param1=[Param1], Param2=[Param2],
                                    Param3=[Param3], Param4=[Param4],
                                    Param5=[Param5], for URL parameters
                                </h3>
                            </Grid>
                            <Paper className={classes.PaperStyle}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialSelect
                                            label="Provider"
                                            name="ProviderID"
                                            onCustomCallback={(name, value) => {
                                                OnChangeProviderID(
                                                    name,
                                                    value,
                                                    setFieldValue
                                                );
                                            }}
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Providers
                                            </MenuItem>
                                            {Providers.map((Item) => (
                                                <MenuItem
                                                    key={Item.ProviderID}
                                                    value={Item.ProviderID}
                                                >
                                                    {Item.ProviderName}
                                                </MenuItem>
                                            ))}
                                        </MaterialSelect>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialSelect
                                            label="Service"
                                            name="ServiceID"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Service
                                            </MenuItem>
                                            {Services.map((Item) => (
                                                <MenuItem
                                                    key={Item.ServiceID}
                                                    value={Item.ServiceID}
                                                >
                                                    {Item.ServiceName}
                                                </MenuItem>
                                            ))}
                                        </MaterialSelect>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialSelect
                                            label="UrlType"
                                            name="UrlType"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select URL Type
                                            </MenuItem>
                                            {UrlTypes.map((Item) => (
                                                <MenuItem
                                                    key={Item.UrlType}
                                                    value={Item.UrlType}
                                                >
                                                    {Item.UrlType}
                                                </MenuItem>
                                            ))}
                                        </MaterialSelect>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialSelect
                                            label="DateFormat"
                                            name="DateFormat"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Date Format
                                            </MenuItem>

                                            {DateFormats.map((Item) => (
                                                <MenuItem
                                                    key={Item.DateFormat}
                                                    value={Item.DateFormat}
                                                >
                                                    {Item.DateFormat}
                                                </MenuItem>
                                            ))}
                                        </MaterialSelect>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <MaterialSelect
                                            label="MethodType"
                                            name="MethodType"
                                            onCustomCallback={(name, value) => {
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

                                            <MenuItem key="GET" value="GET">
                                                GET
                                            </MenuItem>
                                            <MenuItem key="POST" value="POST">
                                                POST
                                            </MenuItem>
                                        </MaterialSelect>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={3} lg={3}>
                                        <Box display={HeaderJson}>
                                            <MaterialTextField
                                                label="HeaderJson"
                                                name="HeaderJson"
                                                type="text"
                                                placeholder="HeaderJson"
                                            />
                                        </Box>
                                    </Grid>
                                    {IsPendingProviderOn ? (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                label="IsPendingProvider"
                                                name="IsPendingProvider"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnChangeIsPendingProvider(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                            >
                                                <MenuItem
                                                    key="false"
                                                    value="false"
                                                >
                                                    NO
                                                </MenuItem>

                                                <MenuItem
                                                    key="true"
                                                    value="true"
                                                >
                                                    YES
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>
                                    ) : null}
                                    {transactionIDPrefix ? (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialTextField
                                                label="TransactionIDPrefix"
                                                name="TransactionIDPrefix"
                                                type="text"
                                                length="5"
                                                placeholder="TransactionIDPrefix"
                                            />
                                        </Grid>
                                    ) : null}
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextArea
                                            label="Provider URL"
                                            name="ProviderURL"
                                            type="text"
                                            length="1000"
                                            placeholder="Provider URL"
                                            rows={3}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={1} lg={1}>
                                        <PrimaryButton
                                            type="submit"
                                            title="Save"
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
                                    <Grid item xs={12} sm={12} md={1} lg={1}>
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
                        </Form>
                    );
                }}
            />
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this Provider details."
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

            <h2 className={classes.PaperHead_h2}>Provider Details</h2>

            <ReactTable
                data={ProviderDetails}
                columns={mainColumns}
                rowsPerPage={100}
            />

            {providerURlInfoModal}
        </CustomContainer>
    );
};
