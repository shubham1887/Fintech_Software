import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { useLocation } from 'react-router-dom';

import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { Select as SLAB_SELECT } from 'Components/Api/SlabApi';
import { SelectPrePaidDTHFRCService as PREPAIDDTHFRC_SERVICE } from 'Components/Api/ServiceApi';

import { Activate as ROFFERCOMMISSION_OPERATOR_ACTIVATE } from 'Components/Api/ROfferCommissionAPI';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { Select as ROFFERCOMMISSION_SELECT_SLABMASTERID } from 'Components/Api/ROfferCommissionAPI';
import { Update as ROFFERCOMMISSION_UPDATE } from 'Components/Api/ROfferCommissionAPI';
import { Delete as ROffer_DELETE } from 'Components/Api/ROfferCommissionAPI';
import { Add as ROFFERCOMMISSION_ADD } from 'Components/Api/ROfferCommissionAPI';

import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialROfferCommissionField } from 'Components/Inputes/FormikMaterialROfferCommissionInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { SelectSlab as SLAB_SELECTSLAB } from 'Components/Api/SlabApi';
import excel from 'Assets/Images/excel.svg';
import { Icon } from '@material-ui/core';

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
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const ROfferCommissionAdd = () => {
    const classes = commonStyle();
    const location = useLocation();
    const SlabID = location.state.SlabMasterID;
    const formRef = useRef();
    const [Services, setServices] = useState([]);
    const [Slabs, setSlabs] = useState([]);
    //const [SlabDetail, setSlabDetail] = useState([]);

    const [Operator, setOperator] = useState([]);
    const [RofferCommissionDetail, setRofferCommissionDetail] = useState([]);

    const [mainColumns, setMainColumns] = useState([]);

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
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
        ServiceID: 0,
        OperatorID: 0,
        Amount: 0,
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
        setLoadingOpen(false);
    }, []);

    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);

        SelectByServiceAndSlab(value, FormObj.ServiceID);
    };
    const OnServiceChange = async (name, value, setFieldValue) => {
        setLoadingOpen(true);
        setFieldValue(name, value);

        selectOperator(value);
        SelectByServiceAndSlab(formRef.current.values.SlabID, value);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        setIsError(false);

        var response = await ROFFERCOMMISSION_ADD(requestObj);
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

        setFormObj({
            ...FormObj,
            SlabID: values.SlabID,
            ServiceID: values.ServiceID,
            OperatorID: values.OperatorID,
            Amount: 0
        });
        setTimeout(() => {
            SelectByServiceAndSlab(values.SlabID, values.ServiceID);
        }, MESSAGE_CLOSE_AFTER);
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
        setRofferCommissionDetail(data.list);
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
            ROfferCommissionID: item,
            IsActive: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await ROFFERCOMMISSION_OPERATOR_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        SelectByServiceAndSlab(
            formRef.current.values.SlabID,
            formRef.current.values.ServiceID
        );
    };

    const SelectByServiceAndSlab = async (SlabMasterID, ServiceID) => {
        setRofferCommissionDetail([]);
        var requestObj = {
            SlabMasterID: SlabMasterID,
            ServiceID: ServiceID
        };
        var request = JSON.stringify(requestObj);
        setLoadingOpen(true);

        const response = await ROFFERCOMMISSION_SELECT_SLABMASTERID(request);

        if (!response.isAuthorized) LOGOUT();
        setLoadingOpen(false);
        if (!response.status) return;

        var data = JSON.parse(response.data);

        setRofferCommissionDetail(data);
    };

    const selectSlabs = async () => {
        setLoadingOpen(true);
        const response = await SLAB_SELECT();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setSlabs(data);
    };

    const selectServices = async () => {
        setLoadingOpen(true);
        const response = await PREPAIDDTHFRC_SERVICE();

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
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
        let ROfferCommissionID = row[0];

        let Retailer = row[2];
        let RetailerCommType = Retailer.General.CommissionType;
        let RetailerComm = Retailer.General.Commission;
        let RetailerIsFix = Retailer.General.IsFix;
        let RetailerIsSurcharge = Retailer.General.IsSurcharge;

        let Distributor = row[3];
        let DistributorCommType = Distributor.General.CommissionType;
        let DistributorComm = Distributor.General.Commission;
        let DistributorIsFix = Distributor.General.IsFix;
        let DistributorIsSurcharge = Distributor.General.IsSurcharge;

        let SuperDistributor = row[4];
        let SuperDistributorCommType = SuperDistributor.General.CommissionType;
        let SuperDistributorComm = SuperDistributor.General.Commission;
        let SuperDistributorIsFix = SuperDistributor.General.IsFix;
        let SuperDistributorIsSurcharge = SuperDistributor.General.IsSurcharge;

        let Client = row[5];
        let ClientCommType = Client.General.CommissionType;
        let ClientComm = Client.General.Commission;
        let ClientIsFix = Client.General.IsFix;
        let ClientIsSurcharge = Client.General.IsSurcharge;

        let jsonObj = {
            ROfferCommissionID: ROfferCommissionID,

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
        const response = await ROFFERCOMMISSION_UPDATE(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        setTimeout(() => {
            SelectByServiceAndSlab(
                formRef.current.values.SlabID,
                formRef.current.values.ServiceID
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
        Amount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'amount')
            .min(0, PLEASE_PROVIDE_VALID + 'amount')
            .max(300000, PLEASE_PROVIDE_VALID + 'amount')
    });

    const onDeleteClick = (item) => {
        var RequestObj = { ROfferCommissionID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };
    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await ROffer_DELETE(jsonReq);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        SelectByServiceAndSlab(
            formRef.current.values.SlabID,
            formRef.current.values.ServiceID
        );
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ROfferCommissionID',
            srno: true
        });
        columns.push({
            label: 'Details',
            name: 'Details'
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
            name: 'ROfferCommissionID',
            save: true,
            buttonTitle: 'Update',
            showTitle: true,
            callback: (row) => {
                UpdateClick(row);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'ROfferCommissionID',
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
                                        Set Commission ROffer
                                    </h2>
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
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialNumberField
                                                label="Amount"
                                                name="Amount"
                                                type="text"
                                                length={7}
                                                placeholder="Amount"
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
                                                    MaterialROfferCommissionField
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
                                                    MaterialROfferCommissionField
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
                                                    MaterialROfferCommissionField
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
                                                    MaterialROfferCommissionField
                                                }
                                                label="Client"
                                                name="Client"
                                            />
                                        </Grid>
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

            <h2 className={classes.PaperHead_h2}>Commission Details</h2>

            <ReactTable
                data={RofferCommissionDetail}
                columns={mainColumns}
                rowsPerPage={100}
                footerToolbar={false}
            />
        </CustomContainer>
    );
};
