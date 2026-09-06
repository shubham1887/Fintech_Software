import React, { useState, useEffect, useRef } from 'react';
import {
    Container,
    Grid,
    MenuItem,
    TextField,
    Paper,
    CircularProgress
} from '@material-ui/core';

import { Formik, Form } from 'formik';
import { LOGOUT } from 'ConstComponents';
import { Select as SEQUENCE_SELECT_ALL } from 'Components/Api/OperatorSequenceApi';

import {
    SelectForViewAPIDropDown as PROVIDER_VIEW_DROP_DOWN,
    SelectProviderForBulkSequence
} from 'Components/Api/ProviderApi';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { CustomModal } from 'Components/Modal/CustomModal';
import { Edit } from './Edit';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import {
    Insert as SEQUENCE_INSERT,
    BulkUpdate as BULK_UPDATE
} from 'Components/Api/OperatorSequenceApi';
import HOCLoader from 'Components/Loader/HOCLoader';
import { VARIANT } from 'Config';
import Autocomplete from '@material-ui/lab/Autocomplete';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';

const CustomContainer = HOCLoader(Container);

var RealAPIs = [];
var ViewAPIs = [];
export const SequenceAdd = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [Services, setServices] = useState([]);

    const [operatorID] = useState(0);
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState(false);
    const [CriteriaValues, setCriteriaValues] = useState('');
    const [MappingList, setMappingList] = useState([]);
    const [IsSequenceValue, setIsSequenceValue] = useState(false);
    const [IsRealAPIValue, setIsRealAPIValue] = useState(false);
    const [IsViewAPIValue, setIsViewAPIValue] = useState(false);
    const [IsTextValue, setIsTextValue] = useState(false);
    //const [RealAPIList, setRealAPIList] = useState([]);
    const [ViewAPIList, setViewAPIList] = useState([]);
    const [ProviderList, setProviderList] = useState([]);
    const [IsUpdateClicked, setIsUpdateClicked] = useState(false);

    const [ServiceID, setServiceID] = useState(0);
    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    var [FormObj, setFormObj] = useState({
        ServiceID: 11,
        ServiceName: 'Mobile',
        ProviderList: [],
        RealProviderID: 0,
        Criteria: 0,
        ViewProviderID: 0
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }

        setCriteriaValues(values);
        setIsUpdateClicked(true);
        setShouldGetUpdatedDataFromTable(true);
    };

    const ServiceItems = Services.map(({ ServiceID, ServiceName }, index) => (
        <MenuItem key={index} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));

    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const OnServiceChange = async (name, value, setFieldValue) => {
        var request = {
            ServiceID: value
        };
        setItemList([]);
        createColumns();
        setLoadingOpen(true);
        var response = await SEQUENCE_SELECT_ALL(JSON.stringify(request));
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setItemList(data);

        setServiceID(value);
    };

    const selectsequence = async (ServiceID) => {
        var request = {
            ServiceID: ServiceID
        };
        var requestJson = JSON.stringify(request);

        setItemList([]);
        createColumns();
        setLoadingOpen(true);
        const response = await SEQUENCE_SELECT_ALL(requestJson);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        var data = JSON.parse(response.data);

        setItemList(data);
    };

    const EditClick = async (value) => {
        setLoadingOpen(true);
        let OperatorID = parseInt(value[0]);
        let Providers = value[2];

        let ViewProviderID = value[3];

        let RealProviderID = 0; //value[4];

        let ServiceID = parseInt(value[5]);

        let DistinctProviders = Providers.filter(function (p) {
            return p.IsActive === true;
        });

        // if (RealProviderID > 0) {
        //     DistinctProviders.push({
        //         ProviderID: RealProviderID,
        //         OperatorID: OperatorID,
        //         Count: 10000
        //     });
        // }

        var jSONObj = {
            ServiceID: ServiceID,
            ServiceName: '',
            OperatorID: OperatorID,
            OperatorName: '',
            ProviderList: DistinctProviders,
            RealProviderID: RealProviderID,
            ViewProviderID: ViewProviderID
        };

        setLoadingOpen(true);
        var response = await SEQUENCE_INSERT(JSON.stringify(jSONObj));
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }
        selectsequence(ServiceID);
    };

    const SelectProvider = async () => {
        setLoadingOpen(true);
        const response = await SelectProviderForBulkSequence();

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        var data = JSON.parse(response.data);
        setProviderList(data);
    };
    const SeletedProviderIndex = (list) => {
        var arryIndex = [];

        for (var parent in list) {
            for (var child in ProviderList) {
                if (list[parent].ServiceID === ProviderList[child].ServiceID) {
                    arryIndex.push(ProviderList[child]);
                }
            }
        }

        setMappingList(arryIndex);
    };

    const ProviderListItems = ProviderList.map(
        ({ ProviderID, ProviderName }) => (
            <MenuItem key={ProviderID} value={ProviderID}>
                {ProviderName}
            </MenuItem>
        )
    );

    // const SelectRealAPI = async () => {
    //     setLoadingOpen(true);
    //     const response = await SELECT_FOR_REAL_API_DROPDWON();

    //     setLoadingOpen(false);

    //     if (!response.isAuthorized) LOGOUT();

    //     if (!response.status) {
    //         return;
    //     }

    //     var data = JSON.parse(response.data);
    //     const RealAPIoptions = data.map(function (row) {
    //         return {
    //             RealProviderID: row.ProviderID,
    //             ProviderName: row.ProviderName
    //         };
    //     });

    //     setRealAPIList(RealAPIoptions);

    //     const options = data.map(function (row) {
    //         return { ID: row.ProviderID, Type: row.ProviderName };
    //     });

    //     RealAPIs = options;
    // };

    const SelectViewAPI = async () => {
        setLoadingOpen(true);

        const responseAllProvider = await PROVIDER_VIEW_DROP_DOWN();

        setLoadingOpen(false);

        if (!responseAllProvider.isAuthorized) LOGOUT();

        if (!responseAllProvider.status) {
            selectsequence(FormObj.ServiceID);
            return;
        }

        var AllProvider = JSON.parse(responseAllProvider.data);

        const ViewAPIoptions = AllProvider.map(function (row) {
            return {
                ViewProviderID: row.ProviderID,
                ProviderName: row.ProviderName
            };
        });

        setViewAPIList(ViewAPIoptions);

        const optionsProviders = AllProvider.map(function (row) {
            return { ID: row.ProviderID, Type: row.ProviderName };
        });

        ViewAPIs = optionsProviders;

        selectsequence(FormObj.ServiceID);

        createColumns();
    };

    // const RealAPIListItems = RealAPIList.map(
    //     ({ RealProviderID, ProviderName }) => (
    //         <MenuItem key={RealProviderID} value={RealProviderID}>
    //             {ProviderName}
    //         </MenuItem>
    //     )
    // );

    const ViewAPIListItems = ViewAPIList.map(
        ({ ViewProviderID, ProviderName }) => (
            <MenuItem key={ViewProviderID} value={ViewProviderID}>
                {ProviderName}
            </MenuItem>
        )
    );

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'OperatorID',
            srno: true
        });

        columns.push({
            label: 'Detail',
            name: 'Detail'
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderList',
            arrayTextBox: true,
            keyName: 'ProviderName'
        });

        // columns.push({
        //     label: 'RealAPI',
        //     name: 'RealProviderID',
        //     dropdown: true,
        //     list: RealAPIs
        // });

        columns.push({
            label: 'ViewAPI',
            name: 'ViewProviderID',
            dropdown: true,
            list: ViewAPIs
        });
        columns.push({
            label: ' ',
            name: 'OperatorID',
            save: true,
            callback: (value) => {
                EditClick(value);
            }
        });
        columns.push({
            label: 'ServiceID',
            name: 'ServiceID',
            hide: true
        });
        setMainColumns(columns);
    };

    useEffect(() => {
        //SelectRealAPI();
        SelectViewAPI();
        selectServices();
    }, []);

    const DisplayNone = () => {
        setIsSequenceValue(false);
        setIsRealAPIValue(false);
        setIsViewAPIValue(false);
        setIsTextValue(false);
    };

    const SequenceDisplay = () => {
        SelectProvider();
        setIsSequenceValue(true);
        setIsRealAPIValue(false);
        setIsViewAPIValue(false);
        setIsTextValue(true);
    };

    const DropdownDisplayIsViewAPIValue = () => {
        setIsViewAPIValue(true);
        setIsSequenceValue(false);
        setIsRealAPIValue(false);
        setIsTextValue(false);
    };

    // const DropdownDisplayIsRealAPIValue = () => {
    //     setIsSequenceValue(false);
    //     setIsRealAPIValue(true);
    //     setIsViewAPIValue(false);
    //     setIsTextValue(false);
    // };

    const ProcessUpdatedData = (data) => {
        setShouldGetUpdatedDataFromTable(false);
        if (IsUpdateClicked) {
            UpdateBulkClick(data);
            return;
        }
    };

    const UpdateBulkClick = async (requestArr) => {
        setIsUpdateClicked(false);
        if (requestArr.length <= 0) {
            setSeverity('error');
            setResponseMessage('Please select one or more operators');
            setIsError(true);
            return;
        }

        let request = [];
        requestArr.forEach((element) => {
            request.push({
                OperatorID: parseInt(element[0].value),
                Criteria: CriteriaValues.Criteria,
                CriteriaText: CriteriaValues.CriteriaText,
                IsTextValue: IsTextValue,
                CriteriaValue: CriteriaValues.CriteriaValue,
                providerList: CriteriaValues.ProviderListItems,
                Count: CriteriaValues.Count,
                RealProviderID: CriteriaValues.RealProviderID,
                ViewProviderID: CriteriaValues.ViewProviderID,
                ServiceID: CriteriaValues.ServiceID
            });
        });

        setLoadingOpen(true);
        var RequestJson = JSON.stringify(request);
        const Response = await BULK_UPDATE(RequestJson);

        setLoadingOpen(false);
        if (!Response.isAuthorized) LOGOUT();

        if (!Response.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        setSeverity('success');
        setResponseMessage(Response.data);
        setIsError(false);
        setItemList([]);
        DisplayNone();
        selectsequence(formRef.current.values.ServiceID);
        setFormObj({
            //  ...formObject,
            ServiceID: ServiceID,
            ServiceName: '',
            ProviderList: [],
            Criteria: 0,
            CriteriaText: '',
            CriteriaValue: false
        });
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
                initialValues={FormObj}
                onSubmit={handleSubmit}
                resetForm={FormObj}
            >
                {({ values, setFieldValue }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                Provider Sequence
                            </h2>

                            <Grid item xs={12} style={{ marginBottom: '10px' }}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialSelect
                                                label="Service"
                                                name="ServiceID"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnServiceChange(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                                title="Service"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Service
                                                </MenuItem>
                                                {ServiceItems}
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
                                                label="Selection Criteria For Bulk Update"
                                                name="Criteria"
                                                title="criteria"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    setCriteriaTextLabel(value);
                                                    setIsDisplayCriteria(true);
                                                }}
                                            >
                                                <MenuItem key={0} value={0}>
                                                    Selection Criteria For Bulk
                                                    Update
                                                </MenuItem>

                                                <MenuItem
                                                    key={1}
                                                    value={'AddSequence'}
                                                    onClick={() => {
                                                        SequenceDisplay();
                                                    }}
                                                >
                                                    Add Sequence
                                                </MenuItem>

                                                <MenuItem
                                                    key={2}
                                                    value={'RemoveSequence'}
                                                    onClick={() => {
                                                        SequenceDisplay();
                                                    }}
                                                >
                                                    Remove Sequence
                                                </MenuItem>

                                                {/* <MenuItem
                                                    key={3}
                                                    value={'RealAPI'}
                                                    onClick={() => {
                                                        DropdownDisplayIsRealAPIValue();
                                                    }}
                                                >
                                                    Real API
                                                </MenuItem> */}

                                                <MenuItem
                                                    key={4}
                                                    value={'ViewAPI'}
                                                    onClick={() => {
                                                        DropdownDisplayIsViewAPIValue();
                                                    }}
                                                >
                                                    View API
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>

                                        {!IsDisplayCriteria ? null : (
                                            <>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    {IsSequenceValue ===
                                                    true ? (
                                                        <Grid
                                                            container
                                                            spacing={2}
                                                        >
                                                            <Grid
                                                                item
                                                                xs={12}
                                                                sm={12}
                                                                md={8}
                                                                lg={8}
                                                                className={
                                                                    classes.multiSelect
                                                                }
                                                            >
                                                                <Autocomplete
                                                                    multiple
                                                                    name="ProviderListItems"
                                                                    options={
                                                                        ProviderList
                                                                    }
                                                                    value={
                                                                        values.ProviderListItems
                                                                    }
                                                                    getOptionLabel={(
                                                                        option
                                                                    ) =>
                                                                        option.ProviderName
                                                                    }
                                                                    getOptionSelected={(
                                                                        option,
                                                                        value
                                                                    ) =>
                                                                        option.ProviderID ===
                                                                        value.ProviderID
                                                                    }
                                                                    onChange={(
                                                                        event,
                                                                        CallBackValues
                                                                    ) => {
                                                                        values.ProviderListItems = [];

                                                                        CallBackValues.map(
                                                                            (
                                                                                category
                                                                            ) => {
                                                                                values.ProviderListItems.push(
                                                                                    {
                                                                                        ProviderID:
                                                                                            category.ProviderID,
                                                                                        ProviderName:
                                                                                            category.ProviderName
                                                                                    }
                                                                                );
                                                                            }
                                                                        );

                                                                        SeletedProviderIndex(
                                                                            values.ProviderListItems
                                                                        );
                                                                    }}
                                                                    renderInput={(
                                                                        params
                                                                    ) => (
                                                                        <TextField
                                                                            variant={
                                                                                VARIANT
                                                                            }
                                                                            {...params}
                                                                            label="Provider(s)"
                                                                            placeholder="Provider"
                                                                        />
                                                                    )}
                                                                />
                                                            </Grid>
                                                            <Grid
                                                                item
                                                                xs={12}
                                                                sm={12}
                                                                md={4}
                                                                lg={4}
                                                            >
                                                                <MaterialNumberField
                                                                    label="Count"
                                                                    name="Count"
                                                                    typed_length={
                                                                        true
                                                                    }
                                                                    length={10}
                                                                    title="Count"
                                                                />
                                                            </Grid>
                                                        </Grid>
                                                    ) : // <TextCheckboxList
                                                    //     name={
                                                    //         ProviderListItems
                                                    //     }
                                                    //     keyName="ProviderName"
                                                    //     label="Provider"
                                                    // />

                                                    null}

                                                    {/* {IsRealAPIValue === true ? (
                                                        <Grid
                                                            container
                                                            spacing={2}
                                                        >
                                                            <Grid
                                                                item
                                                                xs={12}
                                                                sm={12}
                                                                md={6}
                                                                lg={6}
                                                            >
                                                                <MaterialSelect
                                                                    name="RealProviderID"
                                                                    label="RealAPI"
                                                                    title="RealAPI"
                                                                >
                                                                    <MenuItem
                                                                        key="0"
                                                                        value="0"
                                                                    >
                                                                        Select
                                                                        RealAPI
                                                                    </MenuItem>
                                                                    {
                                                                        RealAPIListItems
                                                                    }
                                                                </MaterialSelect>
                                                            </Grid>
                                                        </Grid>
                                                    ) : null} */}

                                                    {IsViewAPIValue === true ? (
                                                        <Grid
                                                            container
                                                            spacing={2}
                                                        >
                                                            <Grid
                                                                item
                                                                xs={12}
                                                                sm={12}
                                                                md={6}
                                                                lg={6}
                                                            >
                                                                <MaterialSelect
                                                                    name="ViewProviderID"
                                                                    label="ViewAPI"
                                                                    title="ViewAPI"
                                                                >
                                                                    <MenuItem
                                                                        key="0"
                                                                        value="0"
                                                                    >
                                                                        Select
                                                                        ViewAPI
                                                                    </MenuItem>
                                                                    {
                                                                        ViewAPIListItems
                                                                    }
                                                                </MaterialSelect>
                                                            </Grid>
                                                        </Grid>
                                                    ) : null}
                                                </Grid>
                                            </>
                                        )}
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={1}
                                            lg={1}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Update"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                            >
                                                Update
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
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={1}
                                            lg={1}
                                        >
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
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={100}
                headerToolbar={true}
                SuccessFailHeader={true}
                primaryIndex={12}
                setLoadingOpen={(value) => {
                    setLoadingOpen(value);
                }}
                getSearchCallback={({ SearchText }) => {
                    selectsequence(formRef.current.values.ServiceID);
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
                shouldGetUpdatedData={shouldGetUpdatedDataFromTable}
                isSelectableRows={'multiple'}
            />

            {isModalOpen ? (
                <CustomModal
                    isOpen={isModalOpen}
                    title="API Sequence"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsModalOpen(value);
                        selectsequence();
                    }}
                >
                    <Edit
                        OperatorID={operatorID}
                        setIsModalOpen={setIsModalOpen}
                        onDismissCallback={(value) => {
                            setIsModalOpen(value);
                            selectsequence();
                        }}
                    />
                </CustomModal>
            ) : null}
        </CustomContainer>
    );
};
