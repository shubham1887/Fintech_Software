import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import {
    Add as PROVIDER_ADD,
    Activate as PROVIDER_ACTIVATE,
    Select as PROVIDER_SELECT,
    Balance as PROVIDER_CHECK_BALANCE,
    SelectForDropDown as PROVIDER_DROP_DOWN,
    SelectBySampleID as SELECT_BY_SampleID,
    Delete as PROVIDER_DELETE
} from 'Components/Api/ProviderApi';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';

import ReactTable from 'Components/ReactTable/ReactTable';
import SettingsIcon from '@material-ui/icons/Settings';
import CellTowerIcon from '@mui/icons-material/CellTower';
import DoubleArrowIcon from '@material-ui/icons/DoubleArrow';
import TouchAppIcon from '@material-ui/icons/TouchApp';
import { ProviderURLINFOView } from 'Pages/InnerPages/Providers/ProviderURLInfo';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import {
    Container,
    Grid,
    Paper,
    MenuItem,
    CircularProgress
} from '@material-ui/core';
import { useHistory } from 'react-router';
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

export const ProviderAdd = ({ ProviderDetailsSampleID }) => {
    const history = useHistory();
    const [Providers, setProviders] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [providerList, setProviderList] = useState([]);
    const [modalOpen, setModalOpen] = useState(false);

    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    useEffect(() => {
        setLoadingOpen(true);
        selectProvider();
        createColumns();
        setLoadingOpen(false);
        selectProviderDropDown();
    }, []);

    const selectSampleProvider = async (ProviderDetailsSampleID) => {
        let jsonObj = {
            ProviderDetailsSampleID: ProviderDetailsSampleID
        };

        const response = await SELECT_BY_SampleID(JSON.stringify(jsonObj));

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        let data = JSON.parse(response.data);

        setProviderObj({
            ProviderName: data.ProviderName,
            ProviderUrl: data.ProviderURL
        });
    };

    const checkBalance = async (value) => {
        let jsonObj = {
            ProviderID: value[0]
        };
        let request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);

        const response = await PROVIDER_CHECK_BALANCE(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) setSeverity('error');
        setIsError(true);
        selectProvider();
    };

    const activate = async (item, value) => {
        let jsonObj = {
            ProviderID: item,
            IsActive: value
        };

        let request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);
        const response = await PROVIDER_ACTIVATE(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        if (!response.status) setSeverity('error');
        setIsError(true);
        selectProvider();
    };

    const [ProviderObj, setProviderObj] = useState({
        ProviderName: '',
        ProviderUrl: '',
        IPAddress: '',
        CopyFromProviderID: 0,
        LoginID: '',
        Token: '',
        SupportNumber: '',
        LowBalanceAmount: 0,
        IsRealAPI: false
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;

        let requestObj = JSON.stringify(values);
        setLoadingOpen(true);
        setIsError(false);
        let response = await PROVIDER_ADD(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setResponseMessage(response.data);
        setIsError(true);
        setSubmitting(false);

        setSeverity('success');
        if (!response.status) {
            setSeverity('error');
            return;
        }
        selectProvider();
        resetForm(ProviderObj);
    };

    const ProviderValidationSchema = Yup.object({
        ProviderName: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'Provider Name'
        ),
        ProviderUrl: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'Provider Url'
        ),
        IPAddress: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'API IP address'
        )
    });

    const selectProviderDropDown = async () => {
        setLoadingOpen(true);
        const response = await PROVIDER_DROP_DOWN();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
    };

    const selectProvider = async () => {
        setLoadingOpen(true);
        const response = await PROVIDER_SELECT();
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setProviders(data);
        setLoadingOpen(false);
    };

    const Edit = async (request) => {
        history.push('/Providers/Edit/' + request);
    };

    const Setup = async (request) => {
        var param = {
            ID: request
        };
        history.push('/ProviderConfiguration/Index/', param);

        //  history.push('/ProviderConfiguration/Index/' + request);
    };
    const ProviderCommiSetup = async (request) => {
        var redir = { ProviderID: request };
        history.push('/ProviderCommission', redir);
    };
    const ProviderStateConfigSetup = async (request) => {
        var redir = { ProviderID: request };
        history.push('/ProviderStateConfigSetup', redir);
    };

    const createColumns = () => {
        let primaryKeyIndex = 0;
        let columns = [];

        columns.push({
            label: '#',
            name: 'ProviderID',
            srno: true
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });
        columns.push({
            label: 'CallBack Url',
            name: 'ProviderUrl'
            //ReadOnlyTextArea: true,
        });

        columns.push({
            label: 'Balances',
            name: 'BalanceDetails'
        });
        columns.push({
            label: 'Low Bal',
            name: 'LowBalanceAmount'
        });

        columns.push({
            label: 'Check Bal',
            name: 'ProviderID',
            tableButton: true,
            icon: 'BalanceButton',
            buttonTitle: 'Check Balance',
            callback: (value) => {
                checkBalance(value);
            }
        });

        columns.push({
            label: 'Setup',
            name: 'ProviderID',
            tableButton: true,
            icon: 'SettingButton',
            buttonTitle: 'Setup',
            callback: (value) => {
                Setup(value[0]);
            }
        });
        columns.push({
            label: 'Active',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: primaryKeyIndex,
            callback: (id, value) => {
                activate(id, value);
            }
        });

        columns.push({
            label: 'URL',
            name: 'ProviderID',

            options: {
                filter: false,
                sort: false,
                download: false,

                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <TouchAppIcon
                            variant="contained"
                            color="primary"
                            onClick={(event) => {
                                var param = {
                                    ID: value
                                };
                                history.push('/ProvidersAdd/Add/', param);
                            }}
                        />
                    );
                }
            }
        });

        columns.push({
            label: 'Response',
            name: 'ProviderID',

            options: {
                filter: false,
                sort: false,
                download: false,

                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <DoubleArrowIcon
                            variant="contained"
                            color="primary"
                            onClick={(event) => {
                                var param = {
                                    ID: value
                                };
                                history.push('/ResponseAdd/', param);
                            }}
                        />
                    );
                }
            }
        });
        columns.push({
            label: 'API Comm',
            name: 'ProviderID',

            options: {
                filter: false,
                sort: false,
                download: false,

                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <SettingsIcon
                            variant="contained"
                            color="primary"
                            onClick={(event) => {
                                ProviderCommiSetup(value);
                            }}
                        />
                    );
                }
            }
        });
        columns.push({
            label: 'State Setup',
            name: 'ProviderID',

            options: {
                filter: false,
                sort: false,
                download: false,

                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <CellTowerIcon
                            variant="contained"
                            color="primary"
                            onClick={(event) => {
                                ProviderStateConfigSetup(value);
                            }}
                        />
                    );
                }
            }
        });

        columns.push({
            label: 'Edit',
            name: 'ProviderID',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });

        columns.push({
            label: 'Delete',
            name: 'ProviderID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });
        columns.push({
            label: 'IsActive',
            name: 'IsActive',
            hide: true
        });

        setMainColumns(columns);
    };

    const onDeleteClick = (ProviderID) => {
        var RequestObj = {
            ProviderID: ProviderID
        };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };
    const Delete = async (request) => {
        var requestObj = JSON.stringify(request);

        setIsError(false);

        setLoadingOpen(true);
        const response = await PROVIDER_DELETE(requestObj);

        if (!response.isAuthorized) {
            LOGOUT();
            return;
        }

        setResponseMessage(response.data);
        setSeverity(response.status ? 'success' : 'error');
        setLoadingOpen(false);
        setIsError(true);

        if (response.status) {
            setTimeout(() => {
                selectProvider();
            }, MESSAGE_CLOSE_AFTER);
        }
    };
    const DeleteConfirmationModal = isAlertOpen ? (
        <ConfirmAlert
            isOpen={isAlertOpen}
            title="Delete Confirmation"
            message="Are you sure to delete this provider."
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpen(false);
                Delete(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpen(false);
            }}
        />
    ) : null;

    const handleDismissCallback = () => {
        setModalOpen(false);
    };

    const handleCopyCallback = (ProviderDetailsSampleID) => {
        selectSampleProvider(ProviderDetailsSampleID);
        setModalOpen(false);
    };

    const handleCustomContainerCloseCallback = (alertState) => {
        setIsError(alertState);
    };

    const ProviderAddForm = (formProps) => {
        return (
            <Form>
                <Grid container spacing={2}>
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <h2 className={classes.PaperHead_h2}>Provider</h2>
                    </Grid>
                </Grid>

                <Grid tem xs={12} sm={12} md={12} lg={12}>
                    <h3 className={classes.Provider_Format}>
                        Note : Always add XMPP API as first API.
                    </h3>
                </Grid>

                <Grid item xs={12}>
                    <Paper className={classes.PaperStyle}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialTextField
                                    label="Provider Name"
                                    name="ProviderName"
                                    type="text"
                                    placeholder="Provider Name"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialTextField
                                    label="Provider Url"
                                    name="ProviderUrl"
                                    type="text"
                                    placeholder=" Provider Url"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialTextField
                                    label="IP Address"
                                    name="IPAddress"
                                    type="text"
                                    placeholder="IP Address"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    name="CopyFromProviderID"
                                    label="Response same as"
                                    title="Response same as"
                                >
                                    <MenuItem key="0" value="0">
                                        Select Provider
                                    </MenuItem>
                                    {providerList.map(
                                        (
                                            { ProviderID, ProviderName },
                                            index
                                        ) => (
                                            <MenuItem
                                                key={index}
                                                value={ProviderID}
                                            >
                                                {ProviderName}
                                            </MenuItem>
                                        )
                                    )}
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialTextField
                                    label="LoginID"
                                    name="LoginID"
                                    type="password"
                                    placeholder="LoginID"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialTextField
                                    label="Token"
                                    name="Token"
                                    type="password"
                                    placeholder="Token"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialNumberField
                                    label="Support Number"
                                    name="SupportNumber"
                                    type="text"
                                    placeholder="Support Number"
                                    length={10}
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialNumberField
                                    label="LowBalance Amount"
                                    name="LowBalanceAmount"
                                    type="text"
                                    placeholder="LowBalance Amount"
                                    length={5}
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <PrimaryButton
                                    type="submit"
                                    title="Add"
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={loadingOpen}
                                >
                                    Add
                                </PrimaryButton>
                                {loadingOpen && (
                                    <CircularProgress
                                        size={24}
                                        className={classes.buttonProgress}
                                    />
                                )}
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <SecondaryButton
                                    variant="contained"
                                    color="secondary"
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

    const providerURlInfoModal = modalOpen ? (
        <ProviderURLINFOView
            modalOpen={modalOpen}
            onDismissCallback={handleDismissCallback}
            onCopyCallback={handleCopyCallback}
        />
    ) : null;

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={handleCustomContainerCloseCallback}
        >
            <Formik
                enableReinitialize={true}
                initialValues={ProviderObj}
                validationSchema={ProviderValidationSchema}
                onSubmit={handleSubmit}
                resetForm={ProviderObj}
                render={ProviderAddForm}
            />

            <h2 className={classes.PaperHead_h2}>Provider List</h2>
            <ReactTable
                data={Providers}
                columns={mainColumns}
                rowsPerPage={100}
            />
            {DeleteConfirmationModal}
            {providerURlInfoModal}
        </CustomContainer>
    );
};
