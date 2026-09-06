import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { useLocation } from 'react-router-dom';

import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { Select as SLAB_SELECT } from 'Components/Api/SlabApi';
import { SelectByServiceType as SERVICE_SELECT_BY_TYPE } from 'Components/Api/ServiceApi';
import { SelectSlab as SLAB_SELECTSLAB } from 'Components/Api/SlabApi';

import { SelectByServiceID as SELECT_BY_SERVICEID } from 'Components/Api/SlabDetailApi';
import { Add as SLABDETAIL_ADD } from 'Components/Api/SlabDetailApi';
import excel from 'Assets/Images/excel.svg';
import { Icon } from '@material-ui/core';

import { Activate as SLAB_OPERATOR_ACTIVATE } from 'Components/Api/SlabDetailApi';

import { SelectBySlabMasterID as SLAB_DETAIL_SELECT_SLABMASTERID } from 'Components/Api/SlabDetailApi';
import { Update as SALBDETAIL_UPDATE } from 'Components/Api/SlabDetailApi';
import { ServicewiseBulkUpdate } from 'Pages/InnerPages/Slabs/ServiceWiseBulkUpdate';
import { UpdateAmount } from 'Pages/InnerPages/Slabs/UpdateAmount';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialCommissionField } from 'Components/Inputes/FormikMaterialCommissionInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { CustomModal } from 'Components/Modal/CustomModal';
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
    MONEY_TRANSFER_CODE,
    AEPS_CODE,
    MESSAGE_CLOSE_AFTER,
    PREPAID_RECHARGE_CODE,
    DTH_RECHARGE_CODE
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const SlabCommission = () => {
    const classes = commonStyle();
    const location = useLocation();
    const SlabID = location.state.SlabMasterID;
    const formRef = useRef();
    const [Services, setServices] = useState([]);
    const [Slabs, setSlabs] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [SlabDetail, setSlabDetail] = useState([]);
    const [SlabMasterID, setSlabMasterID] = useState(0);
    const [selectedSlabDetailID, setSlabDetailID] = useState(0);
    const [isOpen, setIsOpen] = useState(false);
    const [isAmountOpen, setIsAmountOpen] = useState(false);

    const [mainColumns, setMainColumns] = useState([]);

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [isOpenParameter, setIsOpenParameter] = useState(false);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const commDefault = {
        Commission: 0.0,
        CommissionType: 'Fix',
        CommissionCategory: 'Commission'
    };

    const wholeCommissionDefault = {
        General: commDefault,
        Real: commDefault
    };

    var [FormObj, setFormObj] = useState({
        SlabID: SlabMasterID,
        ServiceID: DTH_RECHARGE_CODE,
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

        selectSlabs();
        selectServices();
        createColumns();
        SelectByServiceAndSlab(DTH_RECHARGE_CODE, SlabID);
    }, []);

    const setDefaultSlab = () => {
        let primaryID = parseInt(SlabID);
        primaryID = Number.isInteger(primaryID) ? primaryID : 0;

        setSlabMasterID(primaryID);
        setFormObj({
            ...FormObj,
            SlabID: primaryID
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
        setIsError(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }
        resetForm();
        setSlabMasterID(values.SlabID);
        setFormObj({
            ...FormObj,
            SlabID: values.SlabID,
            ServiceID: values.ServiceID,
            OperatorID: values.OperatorID,
            FromAmount: 0,
            ToAmount: 0
        });
        setTimeout(() => {
            SelectBySlabMasterID(values.SlabID);
        }, MESSAGE_CLOSE_AFTER);
    };

    const callSelectData = async (TakeCount, SkipCount, SearchText) => {
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
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

        if (
            formRef.current.values.ServiceID === undefined ||
            formRef.current.values.ServiceID <= 0
        ) {
            SelectByServiceAndSlab(
                PREPAID_RECHARGE_CODE,
                formRef.current.values.SlabID
            );
            return;
        }

        SelectByServiceAndSlab(
            formRef.current.values.ServiceID,
            formRef.current.values.SlabID
        );
    };

    const SelectBySlabMasterID = async (SlabMasterID) => {
        setSlabDetail([]);
        var requestObj = {
            SlabMasterID: SlabMasterID,
            ServiceType: 'Recharge'
        };
        var request = JSON.stringify(requestObj);
        setLoadingOpen(true);
        const response = await SLAB_DETAIL_SELECT_SLABMASTERID(request);

        if (!response.isAuthorized) LOGOUT();
        setLoadingOpen(false);
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
            ServiceType: 'Recharge'
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
        // setSelectedServiceID(formRef.current.values.ServiceID);
        setFieldValue(name, value);

        setSlabMasterID(FormObj.SlabID);

        if (MONEY_TRANSFER_CODE !== value || AEPS_CODE !== value) {
            setIsOpenParameter(false);
        }
        if (MONEY_TRANSFER_CODE === value || AEPS_CODE === value)
            setIsOpenParameter(true);

        //selectOperator(value);

        SelectByServiceAndSlab(value, formRef.current.values.SlabID);
    };

    const SelectByServiceAndSlab = async (service, slab) => {
        var request = {
            SlabID: slab,
            SlabMasterID: slab,
            ServiceID: service
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

    const UpdateClick = async (row) => {
        let SlabDetailID = row[0];
        let Retailer = row[3];
        let RetailerIsSurcharge = false;
        let RealRetailerIsSurcharge = false;
        let DistributorIsSurcharge = false;
        let RealDistributorIsSurcharge = false;
        let SuperDistributorIsSurcharge = false;
        let RealSuperDistributorIsSurcharge = false;
        let ClientIsSurcharge = false;
        let RealClientIsSurcharge = false;

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

        let Distributor = row[4];

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

        let SuperDistributor = row[5];

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

        let Client = row[6];

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

        setTimeout(() => {
            SelectByServiceAndSlab(
                formRef.current.values.ServiceID,
                formRef.current.values.SlabID
            );
        }, MESSAGE_CLOSE_AFTER);
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
            .max(300000, PLEASE_PROVIDE_VALID + 'from amount'),

        ToAmount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'to amount')
            .min(0, PLEASE_PROVIDE_VALID + 'to amount')
            .max(300000, PLEASE_PROVIDE_VALID + 'to amount')
    });

    const Edit = async (value) => {
        setSlabDetailID(value);
        setIsAmountOpen(true);
    };

    const createColumns = () => {
        var columns = [];

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
            label: 'Amt.Edit',
            name: 'SlabDetailID',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });
        columns.push({
            label: 'Retailer / APIuser',
            name: 'Retailer',
            RealCommission: false,
            CustomCommission: true
        });
        columns.push({
            label: 'Distributer',
            name: 'Distributer',
            RealCommission: false,
            CustomCommission: true
        });
        columns.push({
            label: 'SuperDistributer',
            name: 'SuperDistributer',
            RealCommission: false,
            CustomCommission: true
        });
        columns.push({
            label: 'Client',
            name: 'Client',
            RealCommission: false,
            CustomCommission: true
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
            name: 'SlabDetailID',
            save: true,
            buttonTitle: 'Update',
            showTitle: true,
            callback: (row) => {
                UpdateClick(row);
            }
        });
        // columns.push({
        //     label: 'Delete',
        //     name: 'SlabDetailID',
        //     delete: true,
        //     callback: (value) => {
        //         onDeleteClick(value);
        //     }
        // });

        setMainColumns(columns);
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
            >
                {({ setFieldValue }) => {
                    return (
                        <Form>
                            <Grid container spacing={2}>
                                <Grid xs={12} sm={12} md={12} lg={12}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Set Commission
                                    </h2>
                                </Grid>
                                <Grid xs={12} sm={12} md={12} lg={12}>
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

                            <Grid xs={12} sm={12} md={6} lg={6}>
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
                                                }}
                                                title="Service"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Service
                                                </MenuItem>
                                                {ServiceItems}
                                            </MaterialSelect>
                                        </Grid>
                                        {isOpenParameter ? (
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
                                                        <MenuItem
                                                            key="0"
                                                            value="0"
                                                        >
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
                                                                    {
                                                                        OperatorName
                                                                    }
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
                                                    />
                                                </Grid>
                                            </>
                                        ) : null}
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
                        //Delete(request);
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
            {isAmountOpen ? (
                <CustomModal
                    isOpen={isAmountOpen}
                    title="Update FromAmount ToAmount"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsAmountOpen(value);
                    }}
                >
                    <UpdateAmount
                        SlabDetailID={selectedSlabDetailID}
                        onDismissCallback={(value) => {
                            setIsAmountOpen(value);
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
                rowsPerPage={100}
                footerToolbar={false}
            />
        </CustomContainer>
    );
};
