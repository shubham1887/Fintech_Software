import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { useLocation } from 'react-router-dom';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { Select as SLAB_SELECT } from 'Components/Api/SlabApi';
import { SelectByServiceType as SERVICE_SELECT_BY_TYPE } from 'Components/Api/ServiceApi';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { SelectByServiceID as SELECT_BY_SERVICEID } from 'Components/Api/SlabDetailApi';
import { Add as SLABDETAIL_ADD } from 'Components/Api/SlabDetailApi';
import { Delete as SLABDETAIL_DELETE } from 'Components/Api/SlabDetailApi';
import { Activate as SLAB_OPERATOR_ACTIVATE } from 'Components/Api/SlabDetailApi';
import { SelectBySlabMasterID as SLAB_DETAIL_SELECT_SLABMASTERID } from 'Components/Api/SlabDetailApi';
import { Update as SALBDETAIL_UPDATE } from 'Components/Api/SlabDetailApi';
import { Select as USERTYPE_SELECT } from 'Components/Api/UserTypeApi';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialCommissionField } from 'Components/Inputes/FormikMaterialCommissionInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { SelectSlab as SLAB_SELECTSLAB } from 'Components/Api/SlabApi';
import excel from 'Assets/Images/excel.svg';
import { Icon } from '@material-ui/core';

import { ServicewiseBulkUpdate } from 'Pages/InnerPages/Slabs/ServiceWiseBulkUpdate';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { CustomModal } from 'Components/Modal/CustomModal';
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
    MONEY_TRANSFER_CODE
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const DMTSlabCommission = () => {
    const classes = commonStyle();
    const location = useLocation();
    const SlabID = location.state.SlabMasterID;
    const formRef = useRef();
    const [Services, setServices] = useState([]);
    const [Slabs, setSlabs] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [SlabDetail, setSlabDetail] = useState([]);
    const [SlabMasterID, setSlabMasterID] = useState(0);
    const [ServiceID, setServiceID] = useState(MONEY_TRANSFER_CODE);
    const [mainColumns, setMainColumns] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [IsReal, setIsReal] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const [userTypeList, setUserTypeList] = useState([]);
    const [ServiceDefault, setServiceDefault] = useState({
        ServiceID: 0,
        ServiceName: 'Select Service'
    });

    const commDefault = {
        Commission: 0.0,
        CommissionType: 'Fix',
        CommissionCategory: 'Surcharge'
    };

    const wholeCommissionDefault = {
        General: commDefault,
        Real: commDefault
    };

    var [FormObj, setFormObj] = useState({
        SlabID: SlabMasterID,
        ServiceID: MONEY_TRANSFER_CODE,
        OperatorID: 0,
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
        selectServices();
        createColumns();
        setLoadingOpen(false);
        //SelectByServiceAndSlab(MONEY_TRANSFER_CODE, SlabID);
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

        setSlabMasterID(values.SlabID);
        setServiceID(values.ServiceID);
        resetAll(values.SlabID);

        SelectByServiceAndSlab(values.ServiceID, formRef.current.values.SlabID);
    };

    const callSelectData = async (TakeCount, SkipCount, SearchText) => {
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            ServiceID: formRef.current.values.ServiceID,
            SlabMasterID: formRef.current.values.SlabID,
            IsExcelDownload: true,
            ServiceType: ''
        };

        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);

        setLoadingOpen(true);

        var response = await SLAB_SELECTSLAB(requestJSON);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        setSeverity('success');
        var data = JSON.parse(response.data);

        if (data.IsExcelDownload) {
            formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'DMTslab Commission');
        }
        // if (SkipCount <= 0) {
        //     setHeaderSummary(data.HeaderSummary);
        //     setTotalCount(data.Count);
        // }
        setSlabDetail(data.list);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
        document.removeChild(a);
    }

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
        var RequestObj = { SlabDetailID: item };
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

    const selectServices = async () => {
        var requestObj = {
            ServiceType: 'DMT'
        };
        var request = JSON.stringify(requestObj);
        setLoadingOpen(true);
        const response = await SERVICE_SELECT_BY_TYPE(request);

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const OnServiceChange = async (name, value, setFieldValue) => {
        setLoadingOpen(true);

        setFieldValue(name, value);

        setSlabMasterID(FormObj.SlabID);
        selectOperator(value);

        SelectByServiceAndSlab(value, formRef.current.values.SlabID);
    };

    const SelectByServiceAndSlab = async (service, slab) => {
        var request = {
            ServiceID: service,
            SlabMasterID: slab
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
        let RealRetailerIsSurcharge = false;
        let DistributorIsSurcharge = false;
        let RealDistributorIsSurcharge = false;
        let SuperDistributorIsSurcharge = false;
        let RealSuperDistributorIsSurcharge = false;
        let ClientIsSurcharge = false;
        let RealClientIsSurcharge = false;

        let SlabDetailID = row[0];
        let Retailer = row[2];

        if (Retailer.General.CommissionCategory === 'Surcharge')
            RetailerIsSurcharge = true;
        if (Retailer.Real.CommissionCategory === 'Surcharge')
            RealRetailerIsSurcharge = true;

        let RetailerCommType = Retailer.General.CommissionType;
        let RetailerComm = Retailer.General.Commission;
        let RetailerIsFix = Retailer.General.IsFix;

        let RealRetailerCommType = Retailer.Real.CommissionType;
        let RealRetailerComm = Retailer.Real.Commission;
        let RealRetailerIsFix = Retailer.Real.IsFix;

        let Distributor = row[3];
        if (Distributor.General.CommissionCategory === 'Surcharge')
            DistributorIsSurcharge = true;
        if (Distributor.Real.CommissionCategory === 'Surcharge')
            RealDistributorIsSurcharge = true;

        let DistributorCommType = Distributor.General.CommissionType;
        let DistributorComm = Distributor.General.Commission;
        let DistributorIsFix = Distributor.General.IsFix;

        let RealDistributorCommType = Distributor.Real.CommissionType;
        let RealDistributorComm = Distributor.Real.Commission;
        let RealDistributorIsFix = Distributor.Real.IsFix;

        let SuperDistributor = row[4];
        if (SuperDistributor.General.CommissionCategory === 'Surcharge')
            SuperDistributorIsSurcharge = true;
        if (SuperDistributor.Real.CommissionCategory === 'Surcharge')
            RealSuperDistributorIsSurcharge = true;

        let SuperDistributorCommType = SuperDistributor.General.CommissionType;
        let SuperDistributorComm = SuperDistributor.General.Commission;
        let SuperDistributorIsFix = SuperDistributor.General.IsFix;

        let RealSuperDistributorCommType = SuperDistributor.Real.CommissionType;
        let RealSuperDistributorComm = SuperDistributor.Real.Commission;
        let RealSuperDistributorIsFix = SuperDistributor.Real.IsFix;

        let Client = row[5];
        if (Client.General.CommissionCategory === 'Surcharge')
            ClientIsSurcharge = true;
        if (Client.Real.CommissionCategory === 'Surcharge')
            RealClientIsSurcharge = true;

        let ClientCommType = Client.General.CommissionType;
        let ClientComm = Client.General.Commission;
        let ClientIsFix = Client.General.IsFix;

        let RealClientCommType = Client.Real.CommissionType;
        let RealClientComm = Client.Real.Commission;
        let RealClientIsFix = Client.Real.IsFix;

        let jsonObj = {
            SlabDetailID: SlabDetailID,

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
            ClientIsSurcharge: ClientIsSurcharge,

            RealRetailerCommType: RealRetailerCommType,
            RealRetailerComm: RealRetailerComm,
            RealRetailerIsFix: RealRetailerIsFix,
            RealRetailerIsSurcharge: RealRetailerIsSurcharge,

            RealDistributorCommType: RealDistributorCommType,
            RealDistributorComm: RealDistributorComm,
            RealDistributorIsFix: RealDistributorIsFix,
            RealDistributorIsSurcharge: RealDistributorIsSurcharge,

            RealSuperDistributorCommType: RealSuperDistributorCommType,
            RealSuperDistributorComm: RealSuperDistributorComm,
            RealSuperDistributorIsFix: RealSuperDistributorIsFix,
            RealSuperDistributorIsSurcharge: RealSuperDistributorIsSurcharge,

            RealClientComm: RealClientComm,
            RealClientCommType: RealClientCommType,
            RealClientIsFix: RealClientIsFix,
            RealClientIsSurcharge: RealClientIsSurcharge
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

        SelectByServiceAndSlab(formRef.current.values.ServiceID, SlabID);
    };

    const FormValidationSchema = Yup.object({
        SlabID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'slab')
            .required(PLEASE_SELECT_VALID + 'slab'),
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'serviceID')
            .required(PLEASE_SELECT_VALID + 'serviceID'),
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
            name: 'SlabDetailID',
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
            name: 'SlabDetailID',
            save: true,
            buttonTitle: 'Update',
            showTitle: true,
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

        columns.push({
            label: 'Delete',
            name: 'SlabDetailID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value[0]);
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

    const ServiceItems = Services.map(({ ServiceID, ServiceName }, index) => (
        <MenuItem key={index} value={ServiceID}>
            {ServiceName}
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
                                        Set Commission Range
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
                                            Service-wise Bulk Update
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

                                                    // setServiceDefault({
                                                    //     ServiceID:
                                                    //         value.ServiceID,

                                                    //     ServiceName:
                                                    //         value.ServiceName
                                                    // });
                                                }}
                                                title="Service"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Service
                                                </MenuItem>
                                                {ServiceItems}
                                            </MaterialSelect>

                                            {/* <Field
                                                component={AutoCompleteTextView}
                                                defaultValue={ServiceDefault}
                                                FieldName="Service"
                                                SubFieldName="ServiceID"
                                                name="Service"
                                                options={Services}
                                                getOptionLabel={(option) =>
                                                    option.ServiceName
                                                }
                                                textFieldProps={{
                                                    label: 'Select Service'
                                                }}
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnServiceChange(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );

                                                    setServiceDefault({
                                                        ServiceID:
                                                            value.ServiceID,
                                                        ServiceName:
                                                            value.ServiceName
                                                    });
                                                }}
                                            /> */}
                                        </Grid>

                                        <>
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
                                        </>
                                    </Grid>
                                </Paper>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <Grid item xs={12} sm={12} md={1} lg={1}>
                                        <Box style={{ textAlign: 'center' }}>
                                            <Icon
                                                className={
                                                    classes.CustomImageButtonStyle
                                                }
                                                disabled={loadingOpen}
                                                classes={{
                                                    root: classes.iconRoot
                                                }}
                                                onClick={(e) => {
                                                    setFieldValue(
                                                        'IsExcelDownload',
                                                        true
                                                    );
                                                    callSelectData('', '', '');
                                                }}
                                            >
                                                <img
                                                    alt=""
                                                    className={
                                                        classes.imageIcon
                                                    }
                                                    src={excel}
                                                />
                                            </Icon>
                                        </Box>
                                    </Grid>
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
            {isOpen ? (
                <CustomModal
                    isOpen={isOpen}
                    title="Servicewise Bulk Update"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsOpen(value);
                    }}
                >
                    <ServicewiseBulkUpdate
                        SlabMasterID={SlabID}
                        onDismissCallback={(value) => {
                            setIsOpen(value);
                            SelectByServiceAndSlab(
                                formRef.current.values.ServiceID,
                                formRef.current.values.SlabID
                            );
                            setTimeout(() => {}, MESSAGE_CLOSE_AFTER);
                        }}
                    />
                </CustomModal>
            ) : null}

            <h2 className={classes.PaperHead_h2}>Commission Details</h2>

            <ReactTable
                data={SlabDetail}
                columns={mainColumns}
                rowsPerPage={1000}
                footerToolbar={false}
            />
        </CustomContainer>
    );
};
