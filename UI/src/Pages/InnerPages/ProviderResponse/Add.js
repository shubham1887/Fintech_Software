import React, { useState, useEffect } from 'react';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { TestResponseModal } from 'Components/Modal/TestResponseModel';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import {
    SelectByProviderID,
    Delete as DeleteRow,
    Activate,
    Update as UpdateRow,
    SelectBySampleID as SELECT_BY_SampleID,
    FetchAllSampleResponseFromSupport
} from 'Components/Api/ProviderResponseAPI';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import { LOGOUT } from 'ConstComponents';
import { useHistory } from 'react-router';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SampleResponseView } from 'Pages/InnerPages/ProviderResponse/ProviderSampleResponse';
import CopyToClipBoardInput from 'Components/CopyToClipBoardInput';
import HOCLoader from 'Components/Loader/HOCLoader';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { useLocation } from 'react-router-dom';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const ProviderResponseAdd = (ProviderResponseMasterSampleID) => {
    const classes = commonStyle();
    const location = useLocation();

    let ID = useState(0);
    if (location.state != null) {
        ID = location.state.ID;
    }

    const [primaryID] = useState(ID);

    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const history = useHistory();

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [providerList, setProviderList] = useState([]);

    const [providerID, setProviderID] = useState(0);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [openTestResponse, setOpenTestResponse] = useState(false);
    const [responseText, setResponseText] = useState('');
    const [TextMustExists, setTextMustExists] = useState('');
    const [modalOpen, setModalOpen] = useState(false);

    const TestClick = async (value, TextMustExists) => {
        setOpenTestResponse(true);
        setResponseText(value);
        setTextMustExists(TextMustExists);
    };

    const statusList = [
        {
            ID: 'SUCCESS',
            Type: 'SUCCESS'
        },
        {
            ID: 'FAILURE',
            Type: 'FAILURE'
        },
        {
            ID: 'PROCESS',
            Type: 'PROCESS'
        },
        {
            ID: 'RESEND',
            Type: 'RESEND'
        }
    ];

    const responseTypes = [
        {
            ID: 'Initial',
            Type: 'Initial'
        },
        {
            ID: 'Callback',
            Type: 'Callback'
        },
        {
            ID: 'StatusCheck',
            Type: 'StatusCheck'
        },
        {
            ID: 'BalanceCheck',
            Type: 'BalanceCheck'
        },
        {
            ID: 'View',
            Type: 'View'
        }
    ];

    useEffect(() => {
        selectProvider();
    }, [primaryID]);

    const [formObject, setFormObject] = useState({
        //   Provider: primaryID,
        ProviderID: primaryID,
        ProviderName: ''
    });

    const Delete = async (request) => {
        var requestObj = {
            ProviderResponseMasterID: request
        };

        var jsonReq = JSON.stringify(requestObj);
        setLoadingOpen(true);
        const response = await DeleteRow(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        SelectProviderResponses(providerID);
    };

    const activate = async (item, value) => {
        var requestObj = {
            ProviderResponseMasterID: item,
            IsActive: value
        };
        var jsonReq = JSON.stringify(requestObj);
        setLoadingOpen(true);
        const response = await Activate(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
    };

    const onDeleteClick = (item) => {
        setIsAlertOpen(true);
        setConfirmAlertRequest(item);
    };

    const CustomValidation = Yup.object({
        Type: Yup.string()
            .notOneOf(['0'], PLEASE_PROVIDE_VALID + 'type')
            .required(PLEASE_PROVIDE_VALID + 'type'),

        TextMustExists: Yup.string().required(
            PLEASE_PROVIDE_VALID + ' TextMustExists'
        ),
        Response: Yup.string().required(PLEASE_PROVIDE_VALID + 'response'),
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_PROVIDE_VALID + 'status')
            .required(PLEASE_PROVIDE_VALID + 'status'),
        ProviderID: Yup.number().required(PLEASE_PROVIDE_VALID + 'provider')
    });

    const onUpdateClick = async (row) => {
        // here type value is an array need to have only selected value
        let requestObj = {
            ProviderID: row[5],
            ProviderResponseMasterID: row[0],
            Type: row[1],
            TextMustExists: row[2],
            Response: row[3],
            Status: row[4],
            IsActive: 1,
            IsRefCode: 0
        };
        setIsError(false);
        setResponseMessage('');

        var errorValidation = false;

        await CustomValidation.validate(requestObj).catch(function (err) {
            errorValidation = true;
            setSeverity('error');
            setResponseMessage(err.message);

            setIsError(true);
            return;
        });

        if (errorValidation === true) return;

        var jsonReq = JSON.stringify(requestObj);
        setLoadingOpen(true);
        const response = await UpdateRow(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        SelectProviderResponses(row[5]);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ProviderResponseMasterID',
            srno: true
        });

        columns.push({
            label: 'Type',
            name: 'Type',
            dropdown: true,
            list: responseTypes
        });

        columns.push({
            label: 'MustExist',
            name: 'TextMustExists',
            editable: true,
            setCellHeaderProps: () => ({
                style: { width: '20%' }
            })
        });

        columns.push({
            label: 'Response',
            name: 'Response',
            editable: true,
            setCellHeaderProps: () => ({
                style: { width: '50%' }
            })
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            dropdown: true,
            list: statusList
        });
        columns.push({
            label: '#',
            name: 'ProviderID',
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
            label: 'Update',
            name: 'ProviderResponseMasterID',
            buttonWithTitle: true,
            buttonTitle: 'Update',
            callback: (row) => {
                onUpdateClick(row);
            }
        });
        columns.push({
            label: 'Test',
            name: 'Response',
            testbutton: true,
            callback: (value, row) => {
                TestClick(value, row[2]);
            }
        });

        columns.push({
            label: 'Delete',
            name: 'ProviderResponseMasterID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });
        setMainColumns(columns);
    };

    function handleChange(name, value, setFieldValue) {
        setFieldValue(name, value);
        SelectProviderResponses(value);
    }

    const selectProvider = async () => {
        setLoadingOpen(true);
        const response = await PROVIDER_DROP_DOWN();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);

        if (parseInt(primaryID) >= 1) {
            SelectProviderResponses(primaryID);
        }
    };

    const SelectProviderResponses = async (providerID) => {
        var requestObj = {
            ProviderID: providerID
        };

        setProviderID(providerID);
        setItemList([]);
        var request = JSON.stringify(requestObj);
        const response = await SelectByProviderID(request);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        createColumns();
        var data = JSON.parse(response.data);

        setItemList(data);
    };

    const SelectSampleResponse = async () => {
        setModalOpen(true);
    };

    const handleDismissCallback = () => {
        setModalOpen(false);
    };

    const handleCopyCallback = (request) => {
        selectSampleResponseByID(request);
        setModalOpen(false);
    };

    const handleSetAllResponseCallback = (providerID) => {
        SelectProviderResponses(providerID);
        setModalOpen(false);
    };

    const FetchAllSampleResponse = async () => {
        const response = await FetchAllSampleResponseFromSupport();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
    };

    const selectSampleResponseByID = async (ProviderResponseMasterSampleID) => {
        let jsonObj = {
            ProviderResponseMasterSampleID: ProviderResponseMasterSampleID
        };
        setItemList([]);
        const response = await SELECT_BY_SampleID(JSON.stringify(jsonObj));

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        let data = JSON.parse(response.data);
        createColumns();
        let newArr = itemList;
        let firstItem = newArr[0];

        firstItem.Response = data.Response;
        firstItem.Status = data.Status;
        firstItem.TextMustExists = data.TextMustexits;
        firstItem.Type = data.Type;

        newArr[0] = firstItem;
        setItemList(newArr);
        itemList[0] = firstItem;
    };
    const OpenSetting = (primaryID) => {
        var param = {
            ID: primaryID
        };
        history.push('/ProviderConfiguration/Index/', param);
    };
    const OpenProvider = (primaryID) => {
        var param = {
            ID: primaryID
        };
        history.push('/ProvidersAdd/Add/', param);
    };
    const SampleResponseModal = modalOpen ? (
        <SampleResponseView
            providerID={providerID}
            modalOpen={modalOpen}
            onDismissCallback={handleDismissCallback}
            onCopyCallback={handleCopyCallback}
            onSetAllResponseCallback={handleSetAllResponseCallback}
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
                initialValues={formObject}
                resetForm={formObject}
            >
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={6} lg={6}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Provider Response
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
                                            OpenSetting(providerID);
                                        }}
                                    >
                                        Setting
                                    </PrimaryButton>
                                </Grid>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <PrimaryButton
                                        variant="contained"
                                        color="primary"
                                        title="URL"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        onClick={() => {
                                            OpenProvider(providerID);
                                        }}
                                    >
                                        URL
                                    </PrimaryButton>
                                </Grid>

                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <SecondaryButton
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
                                    </SecondaryButton>
                                </Grid>
                            </Grid>

                            <Grid item xs={12} style={{ marginTop: '15px' }}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Provider"
                                                name="ProviderID"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    handleChange(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                                title="Provider"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Provider
                                                </MenuItem>
                                                {providerList.map((Item) => (
                                                    <MenuItem
                                                        key={Item.ProviderID}
                                                        value={Item.ProviderID}
                                                    >
                                                        {Item.ProviderName}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <PrimaryButton
                                                variant="contained"
                                                color="primary"
                                                title="Sample Response"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={() => {
                                                    SelectSampleResponse();
                                                }}
                                            >
                                                Sample Response
                                            </PrimaryButton>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <SecondaryButton
                                                variant="contained"
                                                color="primary"
                                                title="Refresh Sample Response"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                onClick={() => {
                                                    FetchAllSampleResponse();
                                                }}
                                            >
                                                Refresh Sample Response
                                            </SecondaryButton>
                                        </Grid>
                                    </Grid>

                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={12}
                                            lg={12}
                                        >
                                            <h6>
                                                Recharge Response Parameters:
                                            </h6>
                                            <p>
                                                <CopyToClipBoardInput text="[RequestID]" />
                                                <CopyToClipBoardInput text="[Amount]" />
                                                <CopyToClipBoardInput text="[Number]" />
                                                <CopyToClipBoardInput text="[TxnID]" />
                                                <CopyToClipBoardInput text="[RefID]" />
                                                <CopyToClipBoardInput text="[Message]" />
                                                <CopyToClipBoardInput text="[ClBal]" />
                                                <CopyToClipBoardInput text="[DMTBalance]" />
                                                <CopyToClipBoardInput text="[LapuNo]" />
                                                <CopyToClipBoardInput text="[ApiComm]" />
                                                <CopyToClipBoardInput text="[ApiCommPer]" />
                                                <CopyToClipBoardInput text="[ROffer]" />
                                                <CopyToClipBoardInput text="[Param1]" />
                                                <CopyToClipBoardInput text="[Param2]" />
                                                <CopyToClipBoardInput text="[Param3]" />
                                                <CopyToClipBoardInput text="[NoUse1]" />
                                                <CopyToClipBoardInput text="[NoUse2]" />
                                            </p>
                                            <h6>View Response Parameters:</h6>
                                            <p>
                                                <CopyToClipBoardInput text="[CustomerName]" />
                                                <CopyToClipBoardInput text="[DueDate]" />

                                                <CopyToClipBoardInput text="[Amount]" />
                                                <CopyToClipBoardInput text="[Message]" />
                                                <CopyToClipBoardInput text="[BBPSViewID]" />
                                                <CopyToClipBoardInput text="[ResponseParam1]" />
                                                <CopyToClipBoardInput text="[ResponseParam2]" />
                                                <CopyToClipBoardInput text="[ResponseParam3]" />
                                                <CopyToClipBoardInput text="[ResponseParam4]" />
                                                <CopyToClipBoardInput text="[ResponseParam5]" />

                                                <CopyToClipBoardInput text="[NoUse1]" />
                                                <CopyToClipBoardInput text="[NoUse2]" />
                                                <CopyToClipBoardInput text="[NoUse3]" />
                                            </p>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>

            <Grid item xs={12} sm={12} md={6} lg={6}>
                <h2 className={classes.PaperHead_h2}>Response </h2>
            </Grid>
            <ReactTable
                data={itemList}
                rowsPerPage={100}
                columns={mainColumns}
            />
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this response"
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
            {openTestResponse ? (
                <TestResponseModal
                    text={responseText}
                    TextMustExists={TextMustExists}
                    isOpen={openTestResponse}
                    title="Test Response"
                    error={''}
                    onDismissCallback={(value) => {
                        setOpenTestResponse(value);
                    }}
                />
            ) : null}

            {SampleResponseModal}
        </CustomContainer>
    );
};
