import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import Box from '@material-ui/core/Box';
import * as Yup from 'yup';
import { useLocation } from 'react-router-dom';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { Select as SLAB_SELECT } from 'Components/Api/ChannelSlabAPI';
import { SelectByServiceType as SERVICE_SELECT_BY_TYPE } from 'Components/Api/ServiceApi';
import { SelectByServiceID as SELECT_BY_SERVICEID } from 'Components/Api/ChannelSlabDetailAPI';
import { Add as SLABDETAIL_ADD } from 'Components/Api/ChannelSlabDetailAPI';
import { Delete as SLABDETAIL_DELETE } from 'Components/Api/ChannelSlabDetailAPI';
import { SelectBySlabMasterID as SLAB_DETAIL_SELECT_SLABMASTERID } from 'Components/Api/ChannelSlabDetailAPI';
import { Update as SALBDETAIL_UPDATE } from 'Components/Api/ChannelSlabDetailAPI';

import { CustomModal } from 'Components/Modal/CustomModal';

import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialCommissionField } from 'Components/Inputes/FormikMaterialCommissionInput';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { Container, Grid, MenuItem, Paper, Button } from '@material-ui/core';
import { ServicewiseBulkUpdate } from 'Pages/InnerPages/ChannelSlab/ServicewiseBulkUpdate';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER,
    MONEY_TRANSFER_CODE,
    AEPS_CODE,
    DTH_RECHARGE_CODE,
    ADMIN_CODE
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const ChannelSlabCommission = () => {
    const classes = commonStyle();
    const location = useLocation();
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const SlabID = location.state.ChannelSlabMasterID;
    const formRef = useRef();
    const [Services, setServices] = useState([]);
    const [Slabs, setSlabs] = useState([]);
    const [Operator, setOperator] = useState([]);
    const [SlabDetail, setSlabDetail] = useState([]);

    const [mainColumns, setMainColumns] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [isOpenParameter, setIsOpenParameter] = useState(false);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});

    const typeList = [
        {
            ID: 'Fix',
            Type: 'Fix'
        },
        {
            ID: 'Flexi',
            Type: 'Flexi'
        }
    ];
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
        ChannelSlabMasterID: SlabID,
        ServiceID: 0,
        OperatorID: 0,
        FromAmount: 0,
        ToAmount: 0,
        DownlineCommission: wholeCommissionDefault
    });

    const selectSlabs = async () => {
        const response = await SLAB_SELECT();
        if (!response.isAuthorized) LOGOUT();
        var data = JSON.parse(response.data);
        setSlabs(data);
        setLoadingOpen(false);
        setDefaultSlab();
    };
    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
    };

    useEffect(() => {
        setLoadingOpen(true);
        CheckUserRole();
        selectSlabs();
        selectServices();
        createColumns();
    }, [isAdminRole]);

    const setDefaultSlab = () => {
        let primaryID = parseInt(SlabID);
        primaryID = Number.isInteger(primaryID) ? primaryID : 0;

        setFormObj({
            ...FormObj,
            SlabID: primaryID,
            ServiceID: 0,
            OperatorID: 0,
            FromAmount: 0,
            ToAmount: 0,
            CommissionType: 'Fix',
            Commission: 0,
            RealCommission: 0,
            RealCommissionType: 'Fix'
        });
    };

    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);

        SelectBySlabMasterID(value);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }

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

        SelectByServiceAndSlab(
            formRef.current.values.ServiceID,
            formRef.current.values.ChannelSlabMasterID
        );
        setFormObj({
            ...FormObj,
            ChannelSlabMasterID: values.ChannelSlabMasterID,
            ServiceID: values.ServiceID,
            OperatorID: values.OperatorID,
            FromAmount: 0,
            ToAmount: 0,
            CommissionType: 'Fix',
            Commission: 0,
            RealCommission: 0,
            RealCommissionType: 'Fix'
        });
    };

    const onDeleteClick = (item) => {
        var RequestObj = { ChannelSlabDetailID: item };
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
        SelectBySlabMasterID(SlabID);
    };

    const SelectBySlabMasterID = async (ChannelSlabMasterID, service) => {
        setSlabDetail([]);
        var requestObj = {
            ChannelSlabMasterID: ChannelSlabMasterID,
            ServiceID: service
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

    const selectServices = async () => {
        var requestObj = {
            ServiceType: 'Recharge'
        };
        var request = JSON.stringify(requestObj);
        const response = await SERVICE_SELECT_BY_TYPE(request);

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const OnServiceChange = async (name, value, setFieldValue) => {
        setLoadingOpen(true);

        setFieldValue(name, value);

        if (MONEY_TRANSFER_CODE !== value || AEPS_CODE !== value) {
            //SlabDetailsInsert(request);
            setIsOpenParameter(false);
        }
        if (MONEY_TRANSFER_CODE === value || AEPS_CODE === value)
            setIsOpenParameter(true);

        SelectByServiceAndSlab(
            value,
            formRef.current.values.ChannelSlabMasterID
        );
    };
    const SelectByServiceAndSlab = async (service, ChannelSlabMasterID) => {
        var request = {
            ChannelSlabMasterID: ChannelSlabMasterID,
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
        let ChannelSlabDetailID = row[0];
        let Commissionwrapper;
        if (isAdminRole === true) {
            Commissionwrapper = row[2];
        } else {
            Commissionwrapper = row[3];
        }

        let CommissionType = Commissionwrapper.General.CommissionType;

        let Commission = Commissionwrapper.General.Commission;

        let GeneralCommissionCategory =
            Commissionwrapper.General.CommissionCategory;

        let RealCommissionType = Commissionwrapper.Real.CommissionType;

        let RealCommission = Commissionwrapper.Real.Commission;

        let RealCommissionCategory = Commissionwrapper.Real.CommissionCategory;
        let jsonObj = {
            ChannelSlabDetailID: ChannelSlabDetailID,

            Commission: Commission,
            CommissionType: CommissionType,
            GeneralCommissionCategory: GeneralCommissionCategory,

            RealCommission: RealCommission,
            RealCommissionType: RealCommissionType,
            RealCommissionCategory: RealCommissionCategory
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
                formRef.current.values.ChannelSlabMasterID
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

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ChannelSlabDetailID',
            srno: true
        });
        columns.push({
            label: 'Details',
            name: 'Details'
        });
        if (isAdminRole === false) {
            columns.push({
                label: 'My Commission',
                name: 'OwnCommissionwrapper',
                RealCommission: false,
                disabled: true,
                CustomCommission: true
            });
        }

        columns.push({
            label: 'Commission',
            name: 'Commissionwrapper',
            RealCommission: false,
            CustomCommission: true
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
            label: 'Delete',
            name: 'ChannelSlabDetailID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });
        setMainColumns(columns);
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
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {({ values, setFieldValue, handleChange }) => {
                    return (
                        <Form>
                            <Grid container spacing={2}>
                                <Grid xs={12} sm={12} md={6} lg={6}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Set Channel Commission
                                    </h2>
                                </Grid>
                                <Grid xs={12} sm={12} md={6} lg={6}>
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
                                            Servicewise Bulk Update
                                        </Button>
                                    </Box>
                                </Grid>
                            </Grid>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid xs={12} sm={12} md={2} lg={2}>
                                            <MaterialSelect
                                                label="Slab"
                                                name="ChannelSlabMasterID"
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
                                                {Slabs.map(
                                                    (
                                                        {
                                                            ChannelSlabMasterID,
                                                            Name
                                                        },
                                                        index
                                                    ) => (
                                                        <MenuItem
                                                            key={index}
                                                            value={
                                                                ChannelSlabMasterID
                                                            }
                                                        >
                                                            {Name}
                                                        </MenuItem>
                                                    )
                                                )}
                                            </MaterialSelect>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
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
                                                {Services.map(
                                                    (
                                                        {
                                                            ServiceID,
                                                            ServiceName
                                                        },
                                                        index
                                                    ) => (
                                                        <MenuItem
                                                            key={index}
                                                            value={ServiceID}
                                                        >
                                                            {ServiceName}
                                                        </MenuItem>
                                                    )
                                                )}
                                            </MaterialSelect>
                                        </Grid>
                                        {isOpenParameter ? (
                                            <>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
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
                                                    md={2}
                                                    lg={2}
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
                                                    md={2}
                                                    lg={2}
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
                                                    md={2}
                                                    lg={2}
                                                >
                                                    <label>
                                                        <b>Commission</b>
                                                    </label>

                                                    <Field
                                                        component={
                                                            MaterialCommissionField
                                                        }
                                                        label="DownlineCommission"
                                                        name="DownlineCommission"
                                                    />
                                                </Grid>
                                            </>
                                        ) : null}
                                    </Grid>
                                </Paper>
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
                        ChannelSlabMasterID={SlabID}
                        onDismissCallback={(value) => {
                            setIsOpen(value);
                            SelectByServiceAndSlab(DTH_RECHARGE_CODE, SlabID);
                            setTimeout(() => {}, MESSAGE_CLOSE_AFTER);
                        }}
                    />
                </CustomModal>
            ) : null}

            <Grid container spacing={3}>
                <Grid xs={12} sm={12} md={6} lg={6}>
                    <h2 className={classes.PaperHead_h2}>
                        Channel Commission Details
                    </h2>
                </Grid>
            </Grid>
            <ReactTable
                data={SlabDetail}
                columns={mainColumns}
                rowsPerPage={100}
                footerToolbar={false}
            />
        </CustomContainer>
    );
};
