import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { useLocation } from 'react-router-dom';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { Select as SLAB_SELECT } from 'Components/Api/SlabApi';
import { SelectPackageName as SELECT_PACKAGE_BY_OPERATORID } from 'Components/Api/OfflineProductAPI';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { SelectByServiceID as SELECT_BY_SERVICEID } from 'Components/Api/OfflineSlab';
import { Insert as SLABDETAIL_ADD } from 'Components/Api/OfflineSlab';
import { Delete as SLABDETAIL_DELETE } from 'Components/Api/OfflineSlab';
import { Activate as SLAB_OPERATOR_ACTIVATE } from 'Components/Api/OfflineSlab';
import { Update as SALBDETAIL_UPDATE } from 'Components/Api/OfflineSlab';

import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialCommissionField } from 'Components/Inputes/FormikMaterialCommissionInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
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
    DTH_BOOKING
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const OfflineSlabCommission = () => {
    const classes = commonStyle();
    const location = useLocation();
    const SlabID = location.state.SlabMasterID;
    const formRef = useRef();

    const [Slabs, setSlabs] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [SlabDetail, setSlabDetail] = useState([]);

    const [ServiceID, setServiceID] = useState(DTH_BOOKING);
    const [mainColumns, setMainColumns] = useState([]);
    const [Package, setPackage] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [IsReal, setIsReal] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const commDefault = {
        Commission: 0.0,
        CommissionType: 'Fix',
        CommissionCategory: 'Commission'
    };

    const wholeCommissionDefault = {
        General: commDefault
    };

    var [FormObj, setFormObj] = useState({
        SlabID: SlabID,
        OpType: 0,
        OperatorID: 0,
        OfflineProductMasterID: 0,
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
        selectSlabs();
        selectOperator(DTH_BOOKING);
        createColumns();
        setLoadingOpen(false);
    }, []);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        values.ServiceID = ServiceID;
        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        setIsError(false);

        var response = await SLABDETAIL_ADD(requestObj);
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

        SelectByServiceAndSlab(
            formRef.current.values.ServiceID,
            formRef.current.values.SlabID,
            formRef.current.values.OperatorID
        );
        resetAll(values.SlabID);
    };

    const resetAll = (SlabID) => {
        formRef.current.values.OperatorID = 0;
        formRef.current.values.ToAmount = 0;
        formRef.current.values.FromAmount = 0;
        formRef.current.values.Retailer = wholeCommissionDefault;
        formRef.current.values.Distributer = wholeCommissionDefault;
        formRef.current.values.SuperDistributer = wholeCommissionDefault;
        formRef.current.values.Client = wholeCommissionDefault;
    };
    const onDeleteClick = (item) => {
        var RequestObj = { OfflineSlabDetailID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await SLABDETAIL_DELETE(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        SelectByServiceAndSlab(
            formRef.current.values.ServiceID,
            formRef.current.values.SlabID,
            formRef.current.values.OperatorID
        );
    };
    const selectSlabs = async () => {
        setLoadingOpen(true);
        const response = await SLAB_SELECT();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setSlabs(data);
    };

    const selectPackage = async (OperatorID, OpType) => {
        var request = {
            OperatorID: OperatorID,
            OpType: OpType
        };
        var jsonReq = JSON.stringify(request);

        const response = await SELECT_PACKAGE_BY_OPERATORID(jsonReq);
        if (!response.isAuthorized) LOGOUT();
        var data = JSON.parse(response.data);
        setPackage(data);
    };
    const OnOperatorChange = async (name, value, setFieldValue) => {
        setLoadingOpen(true);

        setFieldValue(name, value);

        selectPackage(value, formRef.current.values.OpType);

        SelectByServiceAndSlab(ServiceID, formRef.current.values.SlabID, value);
    };

    const SelectByServiceAndSlab = async (service, slab, OperatorID) => {
        var request = {
            ServiceID: service,
            SlabMasterID: slab,
            OperatorID: OperatorID
        };

        setSlabDetail([]);
        setLoadingOpen(true);
        var response = await SELECT_BY_SERVICEID(JSON.stringify(request));

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

        let OfflineSlabDetailID = row[0];
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
            OfflineSlabDetailID: OfflineSlabDetailID,

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
        const response = await SALBDETAIL_UPDATE(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        setTimeout(() => {}, MESSAGE_CLOSE_AFTER);
    };

    const FormValidationSchema = Yup.object({
        SlabID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'slab')
            .required(PLEASE_SELECT_VALID + 'slab'),
        OpType: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'OpType')
            .required(PLEASE_SELECT_VALID + 'OpType'),
        OperatorID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'operator')
            .required(PLEASE_SELECT_VALID + 'operator'),

        OfflineProductMasterID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Package')
            .required(PLEASE_SELECT_VALID + 'Package'),

        FromAmount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'from amount')
            .min(0, PLEASE_PROVIDE_VALID + 'from amount')
            .max(300000, PLEASE_PROVIDE_VALID + 'from amount'),

        ToAmount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'to amount')
            .min(0, PLEASE_PROVIDE_VALID + 'to amount')
            .max(300000, PLEASE_PROVIDE_VALID + 'to amount')
    });

    const activate = async (item, value) => {
        var jsonObj = {
            SlabDetailID: item,
            IsActive: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await SLAB_OPERATOR_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        SelectByServiceAndSlab(
            formRef.current.values.ServiceID,
            formRef.current.values.SlabID
        );
    };

    const createColumns = () => {
        var columns = [];
        const RealCommission = false;

        columns.push({
            label: '#',
            name: 'OfflineSlabDetailID',
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
            RealCommission: false
        });
        columns.push({
            label: 'Distributer',
            name: 'Distributer',
            CustomCommission: true,
            RealCommission: false
        });
        columns.push({
            label: 'SuperDistributer',
            name: 'SuperDistributer',
            CustomCommission: true,
            RealCommission: false
        });
        columns.push({
            label: 'Client',
            name: 'Client',
            CustomCommission: true,
            RealCommission: false
        });
        columns.push({
            label: 'Update',
            name: 'OfflineSlabDetailID',
            save: true,
            buttonTitle: 'Update',
            showTitle: true,
            callback: (row) => {
                UpdateClick(row);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'OfflineSlabDetailID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });

        setMainColumns(columns);
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
                            <h2 className={classes.PaperHead_h2}>
                                Set Commission Range
                            </h2>

                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialSelect
                                                label="Slab"
                                                name="SlabID"
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
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialSelect
                                                label="OpType"
                                                name="OpType"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select OpType
                                                </MenuItem>

                                                <MenuItem
                                                    key="SD BOX"
                                                    value="SD BOX"
                                                >
                                                    SD BOX
                                                </MenuItem>
                                                <MenuItem
                                                    key="HD BOX"
                                                    value="HD BOX"
                                                >
                                                    HD BOX
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>

                                        <>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={4}
                                                lg={4}
                                            >
                                                <MaterialSelect
                                                    label="Operator"
                                                    name="OperatorID"
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        OnOperatorChange(
                                                            name,
                                                            value,
                                                            setFieldValue
                                                        );
                                                    }}
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
                                                                value={
                                                                    OperatorID
                                                                }
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
                                                md={4}
                                                lg={4}
                                            >
                                                <MaterialSelect
                                                    label="Package"
                                                    name="OfflineProductMasterID"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Package
                                                    </MenuItem>
                                                    {Package.map((Item) => (
                                                        <MenuItem
                                                            key={
                                                                Item.OfflineProductMasterID
                                                            }
                                                            value={
                                                                Item.OfflineProductMasterID
                                                            }
                                                        >
                                                            {Item.Name}
                                                        </MenuItem>
                                                    ))}
                                                </MaterialSelect>
                                            </Grid>

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={4}
                                                lg={4}
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
                                                md={4}
                                                lg={4}
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
                                        </>
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
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this slab commission?"
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
