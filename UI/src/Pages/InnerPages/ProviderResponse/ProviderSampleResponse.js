import React, { useEffect, useState } from 'react';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { Container, MenuItem, Button, Grid } from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';
import { makeStyles } from '@material-ui/core/styles';
import { LOGOUT } from 'ConstComponents';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import {
    SelectForDropDown as DEVELOPER_DROP_DOWN,
    UpdateSampleResponseToProvider as ADD_RESPONSE,
    SelectSampleResponseByDeveloper as SAMPLERESPONSE_SELECT_BYDEVELOPER
} from 'Components/Api/ProviderResponseAPI';

import { Formik } from 'formik';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const SampleResponseView = ({
    providerID,
    modalOpen,
    onDismissCallback,
    onCopyCallback,
    onSetAllResponseCallback
}) => {
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [sampleProviderResponse, setSampleProviderResponse] = useState([]);

    const [DeveloperList, setDeveloperList] = useState([]);

    const [isError, setIsError] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [severity, setSeverity] = useState('success');

    const [open, setOpen] = useState(modalOpen);
    const [mainColumns, setMainColumns] = useState([]);

    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    const [isSetAllResponseClicked, setIsSetAllResponseClicked] = useState(
        false
    );

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ProviderResponseMasterSampleID',
            srno: true
        });

        columns.push({
            label: 'System',
            name: 'Provider'
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            ReadOnlyTextArea: true
        });

        columns.push({
            label: 'Type',
            name: 'Type',
            ReadOnlyTextArea: true
        });
        columns.push({
            label: 'MustExist',
            name: 'TextMustexits',
            CopyButton: true,
            ReadOnlyTextArea: true
        });

        columns.push({
            label: 'Response',
            name: 'Response',
            CopyButton: true,
            ReadOnlyTextArea: true
        });

        // columns.push({
        //     label: 'Copy',
        //     name: 'ProviderResponseMasterSampleID',
        //     copy: true,
        //     callback: (value) => {
        //         CopyToResponsePage(value);
        //     }
        // });

        setMainColumns(columns);
    };

    // const CopyToResponsePage = async (request) => {
    //     setCopy(true);
    //     setProviderResponseMasterSampleID(request);
    //     onCopyCallback(request);
    // };

    useEffect(() => {
        selectDeveloper();
        createColumns();
        if (providerID <= 0) {
            setSeverity('error');
            setResponseMessage('Please select provider');
            setIsError(true);
            return;
        }
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

    const selectSampleResponseByDeveloper = async (Developer) => {
        let jsonObj = {
            Developer: Developer
        };
        setLoadingOpen(true);
        const response = await SAMPLERESPONSE_SELECT_BYDEVELOPER(
            JSON.stringify(jsonObj)
        );

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        if (data.length > 0 && providerID !== 0) {
        }

        setSampleProviderResponse(data);
        setLoadingOpen(false);
    };

    const handleModalClose = () => {
        setOpen(false);
        onDismissCallback();
    };

    const handleSetAllResponseClick = (e) => {
        setShouldGetUpdatedDataFromTable(true);
        setIsSetAllResponseClicked(true);
    };

    const ProcessUpdatedData = (data) => {
        setShouldGetUpdatedDataFromTable(false);
        if (isSetAllResponseClicked) {
            UpdateSampleResponse(data);
            return;
        }
    };
    const UpdateSampleResponse = async (requestArr) => {
        if (requestArr.length <= 0) {
            setSeverity('error');
            setResponseMessage('Please select one or more response to set');
            setIsError(true);
            return;
        }

        if (providerID.length <= 0) {
            setSeverity('error');
            setResponseMessage('Please select provider');
            setIsError(true);
            return;
        }

        let request = [];
        requestArr.forEach((element) => {
            let { value } = element[0];
            request.push({
                ProviderResponseMasterSampleID: value,
                ProviderID: providerID
            });
        });

        setLoadingOpen(true);
        let { isAuthorized, status, data } = await ADD_RESPONSE(
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

        onSetAllResponseCallback(providerID);
    };
    function handleChange(value, name) {
        selectSampleResponseByDeveloper(name);
    }

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
                            title="Sample Response"
                            width="75%"
                            onDismissCallback={handleModalClose}
                        >
                            {/* <Modal open={open} onClose={handleModalClose}> */}

                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={8} lg={8}>
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

                                <Grid item xs={12} sm={12} md={4} lg={4}>
                                    <Button
                                        variant="contained"
                                        color="primary"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={() => {
                                            handleSetAllResponseClick();
                                        }}
                                    >
                                        Add selected Response
                                    </Button>
                                </Grid>
                            </Grid>
                            <ReactTable
                                data={sampleProviderResponse}
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
