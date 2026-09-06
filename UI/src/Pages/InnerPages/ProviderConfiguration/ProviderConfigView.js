import React, { useEffect, useState } from 'react';
import { Formik } from 'formik';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { Container, MenuItem, Button, Grid } from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';

import { LOGOUT } from 'ConstComponents';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import {
    SelectDeveloperForDropDown as DEVELOPER_DROP_DOWN,
    UpdateSampleConfiguration as ADD_CONFIG,
    SelectSampleConfigurationByDeveloper as SAMPLE_CONFIG_SELECT_BY_DEVELOPER
} from 'Components/Api/ProviderConfigurationApi';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const ProviderConfigView = ({
    providerID,
    modalOpen,
    onDismissCallback,
    onCopyCallback,
    onSetAllCallback
}) => {
    const [DeveloperList, setDeveloperList] = useState([]);
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [sampleProviderConfig, setSampleProviderConfig] = useState([]);
    const [isError, setIsError] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [severity, setSeverity] = useState('success');

    const [open, setOpen] = useState(modalOpen);
    const [mainColumns, setMainColumns] = useState([]);

    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    const [isSetAllClicked, setIsSetAllClicked] = useState(false);
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ConfigurationSampleID',
            srno: true
        });

        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'Operator',
            name: 'Operator',
            ReadOnlyTextArea: true
        });
        columns.push({
            label: 'OpCode',
            name: 'OpCode',
            ReadOnlyTextArea: true
        });
        columns.push({
            label: 'Type',
            name: 'Type',
            ReadOnlyTextArea: true
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        selectDeveloper();
        createColumns();
    }, []);

    const selectDeveloper = async () => {
        setLoadingOpen(true);
        const response = await DEVELOPER_DROP_DOWN();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setDeveloperList(data);
    };

    const handleModalClose = () => {
        setOpen(false);
        onDismissCallback();
    };

    function handleChange(value, name) {
        selectSampleConfigByDeveloper(name);
    }
    const selectSampleConfigByDeveloper = async (Developer) => {
        let jsonObj = {
            Developer: Developer
        };
        setLoadingOpen(true);
        const response = await SAMPLE_CONFIG_SELECT_BY_DEVELOPER(
            JSON.stringify(jsonObj)
        );

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setSampleProviderConfig(data);
        setLoadingOpen(false);
    };
    const handleSetAllConfigClick = (e) => {
        setShouldGetUpdatedDataFromTable(true);
        setIsSetAllClicked(true);
    };

    const ProcessUpdatedData = (data) => {
        setShouldGetUpdatedDataFromTable(false);
        if (isSetAllClicked) {
            UpdateSampleConfig(data);
            return;
        }
    };

    const UpdateSampleConfig = async (requestArr) => {
        if (requestArr.length <= 0) {
            setSeverity('error');
            setResponseMessage('Please select one or more opcodes to set');
            setIsError(true);
            return;
        }

        if (providerID <= 0) {
            setSeverity('error');
            setResponseMessage('Please select Provider');
            setIsError(true);
            return;
        }

        let request = [];
        requestArr.forEach((element) => {
            let { value } = element[0];
            request.push({
                ConfigurationSampleID: value,
                ProviderID: providerID
            });
        });

        setLoadingOpen(true);
        let { isAuthorized, status, data } = await ADD_CONFIG(
            JSON.stringify(request)
        );
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        setResponseMessage(data);

        if (!status) {
            setSeverity('error');
            setIsError(true);
            return;
        }
        setSeverity('success');
        setIsError(true);

        onSetAllCallback(providerID);
    };
    const [formObj, setFormObj] = useState({
        Developer: ''
    });

    const Developers = DeveloperList.map((item) => (
        <MenuItem key={item.Developer} value={item.Developer}>
            {item.Developer}
        </MenuItem>
    ));
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
            <Formik enableReinitialize={true} initialValues={formObj}>
                {() => {
                    return (
                        <CustomModalWithScroll
                            isOpen={open}
                            title="Sample Configuration"
                            width="75%"
                            onDismissCallback={handleModalClose}
                        >
                            <Grid container spacing={2}>
                                <Grid item xs={8} sm={8} md={8} lg={8}>
                                    <MaterialSelect
                                        label="System"
                                        name="Developer"
                                        title="Select System"
                                        onCustomCallback={(value, name) => {
                                            handleChange(value, name);
                                        }}
                                    >
                                        <MenuItem key="0" value="0">
                                            Select System
                                        </MenuItem>
                                        {Developers}
                                    </MaterialSelect>
                                </Grid>

                                <Grid item xs={4} sm={4} md={4} lg={4}>
                                    <Button
                                        variant="contained"
                                        color="primary"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={() => {
                                            handleSetAllConfigClick();
                                        }}
                                    >
                                        Set selected Config
                                    </Button>
                                </Grid>
                            </Grid>

                            <ReactTable
                                data={sampleProviderConfig}
                                columns={mainColumns}
                                rowsPerPage={8}
                                SuccessFailHeader={true}
                                primaryIndex={12}
                                setLoadingOpen={(value) => {
                                    setLoadingOpen(value);
                                }}
                                setIsError={(value) => {
                                    setIsError(value);
                                }}
                                setSeverity={(value) => {
                                    setSeverity(value);
                                }}
                                setResponseMessage={(value) => {
                                    setResponseMessage(value);
                                }}
                                getUpdatedData={(data) => {
                                    ProcessUpdatedData(data);
                                    setShouldGetUpdatedDataFromTable(false);
                                }}
                                shouldGetUpdatedData={
                                    shouldGetUpdatedDataFromTable
                                }
                                isSelectableRows={'multiple'}
                            />
                        </CustomModalWithScroll>
                    );
                }}
            </Formik>
            ;
        </CustomContainer>
    );
};
