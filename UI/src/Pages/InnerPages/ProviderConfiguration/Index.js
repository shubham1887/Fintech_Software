import { Container, Grid, MenuItem, Paper, Button } from '@material-ui/core';
import {
    Activate as OPERATOR_ACTIVATE,
    SelectByProviderID as PROVIDERCONFIG_SELECT,
    Update as PROVIDER_OPERATOR_UPDATE,
    UpdateAll as PROVIDER_OPERATOR_UPDATE_ALL,
    FetchAllSampleConfigurationFromSupport
} from 'Components/Api/ProviderConfigurationApi';
import Box from '@material-ui/core/Box';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import HOCLoader from 'Components/Loader/HOCLoader';
import { CustomModal } from 'Components/Modal/CustomModal';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { Form, Formik } from 'formik';
import React, { useEffect, useRef, useState } from 'react';
import { CircleEdit } from './CircleEdit';
import { MESSAGE_CLOSE_AFTER } from 'Components/Helper/Messages';
import { useHistory } from 'react-router';
import { ProviderConfigView } from 'Pages/InnerPages/ProviderConfiguration/ProviderConfigView';
import { useLocation } from 'react-router-dom';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);
export const ProviderConfigSetup = () => {
    const formRef = useRef();

    const classes = commonStyle();
    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    const location = useLocation();
    const ID = location.state.ID;

    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [ProviderName, setProviderName] = useState(0);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const history = useHistory();
    const [TakeCount, setTakeCount] = useState(100);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [sortColumnName] = useState('');
    const [sortDirection] = useState('');
    const [PrimaryID, setPrimaryID] = useState(0);
    const [Services, setServices] = useState([]);

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [operatorID, setOperatorID] = useState(0);

    const [operatorName, setOperatorName] = useState('');
    const [modalOpen, setModalOpen] = useState(false);
    const [providerID, setProviderID] = useState(0);
    let [FormObj] = useState({
        ServiceID: 11,
        ServiceName: 'Mobile'
    });
    const ServiceItems = Services.map(({ ServiceID, ServiceName }, index) => (
        <MenuItem key={index} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));

    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();

        if (!response.isAuthorized) LOGOUT();

        let data = JSON.parse(response.data);
        setServices(data);
    };
    const OnServiceChange = async (name, value, setFieldValue) => {
        let JSONObj = {
            ProviderID: PrimaryID,
            ServiceID: value,
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: '',
            SortColumnName: sortColumnName,
            SortDirection: sortDirection
        };

        setLoadingOpen(true);
        setItemList([]);
        let request = JSON.stringify(JSONObj);

        const response = await PROVIDERCONFIG_SELECT(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        let data = JSON.parse(response.data);

        setItemList(data.data);
        setTotalCount(data.Count);

        if (data.data.length <= 0) {
            return;
        }
        setProviderName(data.data[0].ProviderName);
    };

    const selectData = async () => {
        let URL = window.location.pathname;
        let res = URL.split('/');
        let primaryID = ID;
        // let primaryID = res[res.length - 1];
        setPrimaryID(primaryID);
        selectDataByID(primaryID, TakeCount, SkipCount, '');
    };

    const selectDataByID = async (primaryID, TakeCount, SkipCount, Search) => {
        let JSONObj = {
            ProviderID: primaryID,
            ServiceID: formRef.current.values.ServiceID,
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: Search,
            SortColumnName: sortColumnName,
            SortDirection: sortDirection
        };

        setLoadingOpen(true);
        setItemList([]);
        let request = JSON.stringify(JSONObj);

        const response = await PROVIDERCONFIG_SELECT(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setResponseMessage(response.data);
            setSeverity('error');
            return;
        }

        let data = JSON.parse(response.data);

        if (data.data.length <= 0) return;
        setItemList(data.data);
        setTotalCount(data.Count);

        if (data.data.length <= 0) {
            return;
        }

        setProviderName(data.data[0].ProviderName);
    };

    const activate = async (item, value) => {
        let jsonObj = {
            ConfigurationID: item,
            IsActive: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await OPERATOR_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };

    const EditClick = (OpName, OperatorID) => {
        setIsModalOpen(true);
        setOperatorName(OpName);
        setOperatorID(OperatorID);
    };

    const UpdateClick = async (row) => {
        let ConfigurationID = row[0];

        let OpCode = row[3];
        let Type = row[4];
        let Lapu = row[5];
        let PendingCount = row[6];
        let PendingInMinute = row[7];
        let FailCount = row[8];
        let FailInMinute = row[9];

        let jsonObj = {
            ConfigurationID: ConfigurationID,
            OpCode: OpCode,
            Type: Type,
            Lapu: Lapu,
            PendingCount: PendingCount,
            PendingInMinute: PendingInMinute,
            FailCount: FailCount,
            FailInMinute: FailInMinute
        };

        let request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await PROVIDER_OPERATOR_UPDATE(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);
    };

    const createColumns = () => {
        let columns = [];

        columns.push({
            label: '#',
            name: 'ConfigurationID',
            srno: true
        });

        columns.push({
            label: 'Service',
            name: 'ServiceName',
            hide: true
        });

        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });

        columns.push({
            label: 'Code',
            name: 'OpCode',
            editable: true
        });
        columns.push({
            label: 'Type',
            name: 'Type',
            editable: true
        });
        columns.push({
            label: 'Lapu',
            name: 'Lapu',
            editable: true
        });

        columns.push({
            label: 'P.Count',
            name: 'PendingCount',
            editable: true
        });
        columns.push({
            label: 'P.Minute',
            name: 'PendingInMinute',
            editable: true
        });
        columns.push({
            label: 'F.Count',
            name: 'FailCount',
            editable: true
        });
        columns.push({
            label: 'F.Minute',
            name: 'FailInMinute',
            editable: true
        });

        columns.push({
            label: 'Update',
            name: 'Update',
            save: true,
            buttonTitle: 'Update',
            callback: (row) => {
                UpdateClick(row);
            }
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

        setMainColumns(columns);
    };

    useEffect(() => {
        selectServices();
        selectData();
        createColumns();
    }, []);

    const OpenResponse = (providerID) => {
        var param = {
            ID: providerID
        };
        history.push('/ResponseAdd/', param);
    };
    const OpenProvider = (providerID) => {
        var param = {
            ID: providerID
        };
        history.push('/ProvidersAdd/Add/', param);
    };

    const SelectConfigurationInfo = async () => {
        setModalOpen(true);

        setProviderID(PrimaryID);
    };
    const handleDismissCallback = () => {
        setModalOpen(false);
    };

    const handleCopyCallback = (providerID) => {
        selectData();
        setModalOpen(false);
    };

    const handleSetAllCallback = (providerID) => {
        selectData();
        setModalOpen(false);
    };

    const providerConfigModal = modalOpen ? (
        <ProviderConfigView
            providerID={providerID}
            modalOpen={modalOpen}
            onDismissCallback={handleDismissCallback}
            onCopyCallback={handleCopyCallback}
            onSetAllCallback={handleSetAllCallback}
        />
    ) : null;
    const FetchAllConfigurationInfo = async () => {
        const response = await FetchAllSampleConfigurationFromSupport();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
    };

    const ProviderSetupForm = ({ setFieldValue }) => {
        return (
            <Form>
                <Grid container spacing={2} style={{ marginBottom: '15px' }}>
                    <Grid item xs={12} sm={12} md={4} lg={4}>
                        <h2 className={classes.PaperHead_h2}>
                            Provider Configuration
                        </h2>
                    </Grid>
                    <Grid item xs={12} sm={12} md={2} lg={2}>
                        <Box className={classes.flexReverse}>
                            <PrimaryButton
                                variant="contained"
                                color="primary"
                                title="URL"
                                className={classes.CustomPrimaryButtonStyle}
                                onClick={() => {
                                    OpenProvider(PrimaryID);
                                }}
                            >
                                URL
                            </PrimaryButton>
                        </Box>
                    </Grid>
                    <Grid item xs={12} sm={12} md={2} lg={2}>
                        <Grid item xs={12}>
                            <Box className={classes.flexReverse}>
                                <PrimaryButton
                                    variant="contained"
                                    color="primary"
                                    title="Response"
                                    className={classes.CustomPrimaryButtonStyle}
                                    onClick={() => {
                                        OpenResponse(PrimaryID);
                                    }}
                                >
                                    Response
                                </PrimaryButton>
                            </Box>
                        </Grid>
                    </Grid>
                    <Grid item xs={12} sm={12} md={2} lg={2}>
                        <Box className={classes.flexReverse}>
                            <PrimaryButton
                                variant="contained"
                                color="primary"
                                title="Sample Configuration"
                                className={classes.CustomPrimaryButtonStyle}
                                onClick={() => {
                                    SelectConfigurationInfo();
                                }}
                            >
                                Sample Configuration
                            </PrimaryButton>
                        </Box>
                    </Grid>

                    <Grid item xs={12} sm={12} md={2} lg={2}>
                        <Box className={classes.flexReverse}>
                            <PrimaryButton
                                variant="contained"
                                color="primary"
                                title="Refresh Configuration"
                                className={classes.CustomPrimaryButtonStyle}
                                onClick={() => {
                                    FetchAllConfigurationInfo();
                                }}
                            >
                                Refresh Configuration
                            </PrimaryButton>
                        </Box>
                    </Grid>
                </Grid>

                <Paper className={classes.PaperStyle}>
                    <Grid container spacing={2}>
                        <Grid item xs={12} sm={12} md={2} lg={2}>
                            <MaterialSelect
                                label="Service"
                                name="ServiceID"
                                onCustomCallback={(name, value) => {
                                    OnServiceChange(name, value, setFieldValue);
                                }}
                                title="Service"
                            >
                                <MenuItem key="0" value="0">
                                    Select Service
                                </MenuItem>
                                {ServiceItems}
                            </MaterialSelect>
                        </Grid>
                    </Grid>
                </Paper>
            </Form>
        );
    };
    const CustomContainerCloseCallback = (alertState) => {
        setIsError(alertState);
    };
    const CircleMappingModal = isModalOpen ? (
        <CustomModal
            isOpen={isModalOpen}
            title={'Circle Mapping for ' + operatorName}
            width="75%"
            onDismissCallback={(value) => {
                setIsModalOpen(value);
                selectDataByID(PrimaryID, TakeCount, SkipCount, '');
            }}
        >
            <CircleEdit
                operatorName={operatorName}
                OperatorID={operatorID}
                ProviderID={PrimaryID}
                setIsModalOpen={setIsModalOpen}
                onDismissCallback={(value) => {
                    setIsModalOpen(value);
                    selectDataByID(PrimaryID, TakeCount, SkipCount, '');
                }}
            />
        </CustomModal>
    ) : null;

    const processUpdatedData = async (UpdatedList) => {
        setShouldGetUpdatedDataFromTable(false);
        let request = JSON.stringify(UpdatedList);

        setIsError(false);
        setLoadingOpen(true);
        const response = await PROVIDER_OPERATOR_UPDATE_ALL(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);

        // UpdatedList.forEach((row, index) => {
        //     row.forEach((item, index) => {});
        // });
    };

    const handlePageChange = ({ currentPage, RowsPerPage }) => {
        setTakeCount(RowsPerPage);
        setSkipCount(RowsPerPage * currentPage);
        selectDataByID(PrimaryID, RowsPerPage, RowsPerPage * currentPage, '');
    };
    const handleSearchCallback = (SearchText) => {
        selectDataByID(PrimaryID, TakeCount, SkipCount, SearchText.SearchText);
    };
    const handleSortCallback = ({ column, direction }) => {};
    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={CustomContainerCloseCallback}
        >
            <Formik
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={FormObj}
                resetForm={FormObj}
            >
                {ProviderSetupForm}
            </Formik>
            <h3 className={classes.PaperHead_h2}>{ProviderName} Provider</h3>
            <Button
                type="button"
                variant="outlined"
                color="primary"
                size="small"
                title="Update All"
                style={{ float: 'right' }}
                onClick={(e) => {
                    setShouldGetUpdatedDataFromTable(true);
                }}
            >
                Update All
            </Button>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={50}
                count={totalCount}
                getCallback={handlePageChange}
                getSearchCallback={handleSearchCallback}
                getSortCallback={handleSortCallback}
                getUpdatedData={processUpdatedData}
                shouldGetUpdatedData={shouldGetUpdatedDataFromTable}
                isSelectableRows={'none'}
                bulkUpdate={true}
            />

            {CircleMappingModal}

            {providerConfigModal}
        </CustomContainer>
    );
};
