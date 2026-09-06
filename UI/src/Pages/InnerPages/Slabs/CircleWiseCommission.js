import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { useLocation } from 'react-router-dom';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { Select as SLAB_SELECT } from 'Components/Api/SlabApi';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import {
    Insert as CIRCLE_COMMI_INSERT,
    SelectByStateIDAndSlabID as SELECT_BY_CIRCLE,
    Update as CIRCLE_COMMI_UPDATE,
    Delete as CIRCLE_COMMI_DELETE
} from 'Components/Api/CircleCommissionAPI';
import { SelectBySlabMasterID as SLAB_DETAIL_SELECT_SLABMASTERID } from 'Components/Api/SlabDetailApi';
import { Select as USERTYPE_SELECT } from 'Components/Api/UserTypeApi';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialCommissionField } from 'Components/Inputes/FormikMaterialCommissionInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';
import { CirclewiseBulkUpdate } from 'Pages/InnerPages/Slabs/CircleWiseBulkUpdate';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { CustomModal } from 'Components/Modal/CustomModal';
import { ZoneMasterSelect as Select } from 'Components/Api/StatesApi';
import {
    Container,
    Grid,
    MenuItem,
    Paper,
    Button,
    CircularProgress,
    Box
} from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER,
    PREPAID_RECHARGE_CODE
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const CircleWiseCommission = () => {
    const classes = commonStyle();
    const location = useLocation();
    const SlabID = location.state.SlabMasterID;
    const formRef = useRef();

    const [Slabs, setSlabs] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [SlabDetail, setSlabDetail] = useState([]);
    const [SlabMasterID, setSlabMasterID] = useState(0);
    const [mainColumns, setMainColumns] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [IsReal, setIsReal] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [stateList, setStateList] = useState([]);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [userTypeList, setUserTypeList] = useState([]);

    const commDefault = {
        Commission: 0.0,
        CommissionType: 'Flexi',
        CommissionCategory: 'Commission'
    };

    const wholeCommissionDefault = {
        General: commDefault,
        Real: commDefault
    };
    const selectStates = async () => {
        const response = await Select();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setStateList(data);
    };
    const [stateDefault, setStateDefault] = useState({
        StateID: 0,
        Name: 'Select Circle'
    });

    var [FormObj, setFormObj] = useState({
        SlabID: SlabMasterID,
        OperatorID: 0,
        State: stateDefault,
        FromAmount: 0,
        ToAmount: 0,
        UserTypeID: 0,
        Retailer: wholeCommissionDefault,
        Distributer: wholeCommissionDefault,
        SuperDistributer: wholeCommissionDefault,
        Client: wholeCommissionDefault
    });

    useEffect(() => {
        setLoadingOpen(true);
        selectUserTypes();
        selectSlabs();
        selectStates();
        selectOperator(PREPAID_RECHARGE_CODE);
        createColumns();
        setLoadingOpen(false);
    }, []);

    const setDefaultSlab = () => {
        let primaryID = parseInt(SlabID);
        primaryID = Number.isInteger(primaryID) ? primaryID : 0;

        setSlabMasterID(primaryID);
        setFormObj({
            ...FormObj,
            SlabID: primaryID,
            ServiceID: 0,
            OperatorID: 0,
            FromAmount: 0,
            ToAmount: 0
        });
    };

    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
        setSlabMasterID(value);
        SelectBySlabMasterID(value);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        values.StateID = values.State.StateID;
        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);
        setIsError(false);

        var response = await CIRCLE_COMMI_INSERT(requestObj);
        if (!response.isAuthorized) LOGOUT();

        setLoadingOpen(false);

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) {
            setSeverity('error');
            setIsError(true);
            return;
        }

        setSlabMasterID(values.SlabID);
        resetAll(values.SlabID);

        SelectByStateIDAndSlab(values.StateID, formRef.current.values.SlabID);
    };

    const resetAll = (SlabID) => {
        setStateDefault({
            StateID: 0,
            Name: 'Select Circle'
        });

        formRef.current.values.OperatorID = 0;
        formRef.current.values.ToAmount = 0;
        formRef.current.values.FromAmount = 0;
        formRef.current.values.Retailer = wholeCommissionDefault;
        formRef.current.values.Distributer = wholeCommissionDefault;
        formRef.current.values.SuperDistributer = wholeCommissionDefault;
        formRef.current.values.Client = wholeCommissionDefault;
        formRef.current.values.State = stateDefault;
    };
    const onDeleteClick = (item) => {
        var RequestObj = { CircleWiseCommissionID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await CIRCLE_COMMI_DELETE(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        SelectByStateIDAndSlab(
            formRef.current.values.State.StateID,
            formRef.current.values.SlabID
        );
    };

    const SelectBySlabMasterID = async (SlabMasterID) => {
        setSlabDetail([]);
        var requestObj = {
            SlabMasterID: SlabMasterID,
            ServiceType: 'DMT',
            ServiceID: formRef.current.values.ServiceID
        };
        var request = JSON.stringify(requestObj);
        setLoadingOpen(true);

        const response = await SLAB_DETAIL_SELECT_SLABMASTERID(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setSlabDetail(data);
    };
    const selectSlabs = async () => {
        setLoadingOpen(true);
        const response = await SLAB_SELECT();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setSlabs(data);
        setDefaultSlab();
    };

    const OnStateChange = async (name, value, setFieldValue) => {
        setLoadingOpen(true);
        setFieldValue(name, value);
        setSlabMasterID(FormObj.SlabID);
        SelectByStateIDAndSlab(value.StateID, formRef.current.values.SlabID);
    };

    const SelectByStateIDAndSlab = async (StateID, slab) => {
        var request = {
            StateID: StateID,
            SlabMasterID: slab
        };

        setSlabDetail([]);
        setLoadingOpen(true);
        var response = await SELECT_BY_CIRCLE(JSON.stringify(request));

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) return;
        var data = JSON.parse(response.data);

        setSlabDetail(data);
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

        if (!response.status) setIsError(true);

        if (response.data !== 'Requested operators not found') {
            var data = JSON.parse(response.data);
            setOperator(data);
        }
    };

    const UpdateClick = async (row) => {
        let RetailerIsSurcharge = false;
        let DistributorIsSurcharge = false;
        let SuperDistributorIsSurcharge = false;
        let ClientIsSurcharge = false;

        let CircleWiseCommissionID = row[0];
        let Retailer = row[2];

        if (Retailer.General.CommissionCategory === 'Surcharge')
            RetailerIsSurcharge = true;

        let RetailerCommType = Retailer.General.CommissionType;
        let RetailerComm = Retailer.General.Commission;
        let RetailerIsFix = Retailer.General.IsFix;

        let Distributor = row[3];
        if (Distributor.General.CommissionCategory === 'Surcharge')
            DistributorIsSurcharge = true;

        let DistributorCommType = Distributor.General.CommissionType;
        let DistributorComm = Distributor.General.Commission;
        let DistributorIsFix = Distributor.General.IsFix;

        let SuperDistributor = row[4];
        if (SuperDistributor.General.CommissionCategory === 'Surcharge')
            SuperDistributorIsSurcharge = true;

        let SuperDistributorCommType = SuperDistributor.General.CommissionType;
        let SuperDistributorComm = SuperDistributor.General.Commission;
        let SuperDistributorIsFix = SuperDistributor.General.IsFix;

        let Client = row[5];
        if (Client.General.CommissionCategory === 'Surcharge')
            ClientIsSurcharge = true;

        let ClientCommType = Client.General.CommissionType;
        let ClientComm = Client.General.Commission;
        let ClientIsFix = Client.General.IsFix;

        let jsonObj = {
            CircleWiseCommissionID: CircleWiseCommissionID,

            RetailerComm: RetailerComm,
            RetailerCommType: RetailerCommType,
            RetailerIsFix: RetailerIsFix,
            RetailerIsSurcharge: RetailerIsSurcharge,

            DistributorComm: DistributorComm,
            DistributorCommType: DistributorCommType,
            DistributorIsFix: DistributorIsFix,
            DistributorIsSurcharge: DistributorIsSurcharge,

            SuperDistributorComm: SuperDistributorComm,
            SuperDistributorCommType: SuperDistributorCommType,
            SuperDistributorIsFix: SuperDistributorIsFix,
            SuperDistributorIsSurcharge: SuperDistributorIsSurcharge,

            ClientComm: ClientComm,
            ClientCommType: ClientCommType,
            ClientIsFix: ClientIsFix,
            ClientIsSurcharge: ClientIsSurcharge
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await CIRCLE_COMMI_UPDATE(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        SelectByStateIDAndSlab(
            formRef.current.values.State.StateID,
            formRef.current.values.SlabID
        );
    };

    const FormValidationSchema = Yup.object({
        SlabID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'slab')
            .required(PLEASE_SELECT_VALID + 'slab'),

        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator'),

        FromAmount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'from amount')
            .min(0, PLEASE_PROVIDE_VALID + 'from amount')
            .max(500000, PLEASE_PROVIDE_VALID + 'from amount'),

        ToAmount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'to amount')
            .min(0, PLEASE_PROVIDE_VALID + 'to amount')
            .max(500000, PLEASE_PROVIDE_VALID + 'to amount')
    });

    const createColumns = () => {
        var columns = [];
        const RealCommission = false;

        columns.push({
            label: '#',
            name: 'CircleWiseCommissionID',
            srno: true
        });

        columns.push({
            label: 'Details',
            name: 'Details'
        });

        columns.push({
            label: 'Retailer',
            name: 'Retailer',
            CustomCommission: true,
            RealCommission: RealCommission
        });
        columns.push({
            label: 'Distributer',
            name: 'Distributer',
            CustomCommission: true,
            RealCommission: RealCommission
        });
        columns.push({
            label: 'SuperDistributer',
            name: 'SuperDistributer',
            CustomCommission: true,
            RealCommission: RealCommission
        });
        columns.push({
            label: 'Client',
            name: 'Client',
            CustomCommission: true,
            RealCommission: RealCommission
        });
        columns.push({
            label: 'Update',
            name: 'CircleWiseCommissionID',
            save: true,
            buttonTitle: 'Update',
            showTitle: true,
            callback: (row) => {
                UpdateClick(row);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'CircleWiseCommissionID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });

        setMainColumns(columns);
    };

    const selectUserTypes = async () => {
        const { isAuthorized, status, data } = await USERTYPE_SELECT();

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setSeverity('error');
            setResponseMessage(data);
            setIsError(true);
            return;
        }

        var usertypedata = JSON.parse(data);
        setUserTypeList(usertypedata);
    };
    const SlabItems = Slabs.map(({ SlabID, Name }, index) => (
        <MenuItem key={index} value={SlabID}>
            {Name}
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
            <Formik
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <Grid container spacing={2}>
                                <Grid item xs={6}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Set Circle Wise Commission
                                    </h2>
                                </Grid>
                                <Grid item xs={6}>
                                    <Box className={classes.flexReverse}>
                                        <Button
                                            variant="contained"
                                            color="primary"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            onClick={() => {
                                                setIsOpen(true);
                                            }}
                                        >
                                            Circle-wise Bulk Update
                                        </Button>
                                    </Box>
                                </Grid>
                            </Grid>

                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialSelect
                                                label="Slab"
                                                name="SlabID"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnChange(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                            >
                                                <MenuItem key="0" value={0}>
                                                    Select Slab
                                                </MenuItem>

                                                {SlabItems}
                                            </MaterialSelect>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <Field
                                                component={AutoCompleteTextView}
                                                defaultValue={stateDefault}
                                                FieldName="State"
                                                SubFieldName="StateID"
                                                name="State"
                                                options={stateList}
                                                getOptionLabel={(option) =>
                                                    option.Name
                                                }
                                                textFieldProps={{
                                                    label: 'Select Circle'
                                                }}
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnStateChange(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );
                                                    setStateDefault({
                                                        StateID: value.StateID,
                                                        Name: value.Name
                                                    });
                                                }}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialSelect
                                                label="Operator"
                                                name="OperatorID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Operator
                                                </MenuItem>
                                                {Operator.map(
                                                    (
                                                        {
                                                            OperatorID,
                                                            OperatorName
                                                        },
                                                        index
                                                    ) => (
                                                        <MenuItem
                                                            key={index}
                                                            value={OperatorID}
                                                        >
                                                            {OperatorName}
                                                        </MenuItem>
                                                    )
                                                )}
                                            </MaterialSelect>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <MaterialNumberField
                                                label="From Amount"
                                                name="FromAmount"
                                                type="text"
                                                length={7}
                                                placeholder="From Amount"
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
                                                label="To Amount"
                                                name="ToAmount"
                                                type="text"
                                                length={7}
                                                placeholder="To Amount"
                                            />
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <label>
                                                <b>Retailer</b>
                                            </label>

                                            <Field
                                                component={
                                                    MaterialCommissionField
                                                }
                                                label="Retailer"
                                                name="Retailer"
                                                RealCommission={IsReal}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <label>
                                                <b>Distributer</b>
                                            </label>

                                            <Field
                                                component={
                                                    MaterialCommissionField
                                                }
                                                label="Distributer"
                                                name="Distributer"
                                                RealCommission={IsReal}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <label>
                                                <b>SuperDistributer</b>
                                            </label>

                                            <Field
                                                component={
                                                    MaterialCommissionField
                                                }
                                                label="SuperDistributer"
                                                name="SuperDistributer"
                                                RealCommission={IsReal}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <label>
                                                <b>Client</b>
                                            </label>

                                            <Field
                                                component={
                                                    MaterialCommissionField
                                                }
                                                label="Client"
                                                name="Client"
                                                RealCommission={IsReal}
                                            />
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Save"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loadingOpen}
                                        >
                                            Save
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
                                        variant="contained"
                                        color="secondary"
                                        title="Reset"
                                        className={
                                            classes.CustomDangerButtonStyle
                                        }
                                    >
                                        Reset
                                    </Button>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            ;
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this Circle commission?"
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
            {isOpen ? (
                <CustomModal
                    isOpen={isOpen}
                    title="Circlewise Bulk Update"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsOpen(value);
                    }}
                >
                    <CirclewiseBulkUpdate
                        SlabMasterID={SlabID}
                        onDismissCallback={(value) => {
                            setIsOpen(value);
                            // SelectByServiceAndSlab(
                            //     formRef.current.values.ServiceID,
                            //     formRef.current.values.SlabID
                            // );
                            setTimeout(() => {}, MESSAGE_CLOSE_AFTER);
                        }}
                    />
                </CustomModal>
            ) : null}
            <h2 className={classes.PaperHead_h2}>Commission Details</h2>
            <ReactTable
                data={SlabDetail}
                columns={mainColumns}
                rowsPerPage={100}
                footerToolbar={false}
            />
        </CustomContainer>
    );
};
