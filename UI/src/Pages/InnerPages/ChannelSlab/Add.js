import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { LOGOUT } from 'ConstComponents';
import { Add as SLAB_ADD } from 'Components/Api/ChannelSlabAPI';

import { Select as SLAB_SELECT } from 'Components/Api/ChannelSlabAPI';
import { Delete as SLAB_DELETE } from 'Components/Api/ChannelSlabAPI';
import { SelectByID as SLAB_SELECT_BY_ID } from 'Components/Api/ChannelSlabAPI';

import SettingsIcon from '@material-ui/icons/Settings';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import ReactTable from 'Components/ReactTable/ReactTable';

import {
    Container,
    Grid,
    Paper,
    Button,
    Box,
    CircularProgress
} from '@material-ui/core';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { useHistory } from 'react-router';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import { BulkUpdate } from 'Pages/InnerPages/ChannelSlab/BulkUpdate';
import { CustomModal } from 'Components/Modal/CustomModal';
import {
    PLEASE_PROVIDE_VALID,
    SUPPORT_CODE,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import { ChannelCopyCommission } from 'Pages/InnerPages/ChannelSlab/ChannelCopyCommission';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const Add = () => {
    const history = useHistory();
    const [Slabs, setSlabs] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [isOpen, setIsOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [CopySlabID, setCopySlabID] = useState(0);
    const [ChannelSlabMasterID, setChannelSlabMasterID] = useState(0);
    const [isOpenCopyCommissionModal, setIsOpenCopyCommissionModal] = useState(
        false
    );
    const handleCopyButtonClick = () => {
        setSlabObj({
            ChannelSlabMasterID: 0,
            SlabName: ''
        });
        setOpenUpdateButton(false);
        select();
        setIsOpenCopyCommissionModal(false);
    };
    const copyCommissionModal = (
        <CustomModal
            isOpen={isOpenCopyCommissionModal}
            title="Copy Commission"
            width="75%"
            onDismissCallback={(value) => {
                setIsOpenCopyCommissionModal(value);
                setCopySlabID(0);
                select();
            }}
        >
            <ChannelCopyCommission
                CopySlabID={CopySlabID}
                onCopyButtonClick={handleCopyButtonClick} // Pass the callback function
            />
        </CustomModal>
    );
    const [OpenUpdateButton, setOpenUpdateButton] = useState(false);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    useEffect(() => {
        setLoadingOpen(true);
        select();
        createColumns();
    }, []);

    const onDeleteClick = (item) => {
        var RequestObj = { ChannelSlabMasterID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await SLAB_DELETE(jsonReq);
        setLoadingOpen(false);

        if (SUPPORT_CODE === parseInt(currentUsertypeid)) {
            if (!response.isAuthorized) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage('You dont have access');

                setLoadingOpen(false);
                return;
            }
        }
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        select();
    };

    const [SlabObj, setSlabObj] = useState({
        ChannelSlabMasterID: ChannelSlabMasterID,
        SlabName: ''
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            return;
        }
        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);

        setIsError(false);
        var response = await SLAB_ADD(requestObj);

        setLoadingOpen(false);

        if (SUPPORT_CODE === parseInt(currentUsertypeid)) {
            if (!response.isAuthorized) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage('You dont have access');

                setLoadingOpen(false);
                return;
            }
        }

        if (!response.isAuthorized) {
            LOGOUT();
        }

        setIsError(true);
        setSubmitting(false);
        select();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        setSeverity('success');
        setResponseMessage(response.data);
        resetForm(SlabObj);
        select();
        setOpenUpdateButton(false);
        reset();
    };
    const reset = () => {
        setSlabObj({
            ChannelSlabMasterID: 0,
            SlabName: ''
        });
        setOpenUpdateButton(false);
    };
    const SlabValidationSchema = Yup.object({
        SlabName: Yup.string().required(PLEASE_PROVIDE_VALID + 'Slab Name')
    });

    const select = async () => {
        const response = await SLAB_SELECT();

        if (SUPPORT_CODE === parseInt(currentUsertypeid)) {
            if (!response.isAuthorized) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage('You dont have access');

                setLoadingOpen(false);
                return;
            }
        }

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setSlabs(data);
        setLoadingOpen(false);
    };
    const Edit = async (ChannelSlabMasterID) => {
        setOpenUpdateButton(true);
        setChannelSlabMasterID(ChannelSlabMasterID);
        SelectByID(ChannelSlabMasterID);
    };

    const SelectByID = async (ChannelSlabMasterID) => {
        setLoadingOpen(false);

        let jsonObj = {
            ChannelSlabMasterID: ChannelSlabMasterID
        };

        var request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);
        setResponseMessage('');
        const response = await SLAB_SELECT_BY_ID(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setIsError(true);
        if (!response.status) setSeverity('error');

        let d = JSON.parse(response.data);

        setSlabObj({
            ChannelSlabMasterID: d.ChannelSlabMasterID,
            SlabName: d.Name
        });
    };

    const Setup = async (request) => {
        var ChannelSlabMasterID = { ChannelSlabMasterID: request };
        history.push('/ChannelSlabs/Commission', ChannelSlabMasterID);
    };
    const DMTSetup = async (request) => {
        var ChannelSlabMasterID = { ChannelSlabMasterID: request };
        history.push('/ChannelSlabs/DMTCommission', ChannelSlabMasterID);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ChannelSlabMasterID',
            srno: true
        });
        columns.push({
            label: 'Name',
            name: 'Name'
        });

        columns.push({
            label: 'Setup',
            name: 'ChannelSlabMasterID',

            options: {
                filter: false,
                sort: false,
                download: false,

                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <SettingsIcon
                            variant="contained"
                            color="primary"
                            onClick={(event) => {
                                Setup(value);
                            }}
                        />
                    );
                }
            }
        });
        columns.push({
            label: 'Range Setup',
            name: 'ChannelSlabMasterID',

            options: {
                filter: false,
                sort: false,
                download: false,

                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <SettingsIcon
                            variant="contained"
                            color="primary"
                            onClick={(event) => {
                                DMTSetup(value);
                            }}
                        />
                    );
                }
            }
        });

        columns.push({
            label: 'Edit',
            name: 'ChannelSlabMasterID',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });

        columns.push({
            label: 'Delete',
            name: 'ChannelSlabMasterID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });

        var options = [
            {
                Title: 'Copy Commission',
                Callback: (values) => {
                    setCopySlabID(values[0]);
                    setIsOpenCopyCommissionModal(true);
                }
            }
        ];
        columns.splice(0, 0, {
            label: ' ',
            name: 'ChannelSlabMasterID',
            MoreMenu: true,
            options: options
        });
        setMainColumns(columns);
    };
    const bulkCommissionUpdate = (
        <CustomModal
            isOpen={isOpen}
            title="Bulk Update Channel Commission"
            width="75%"
            onDismissCallback={(value) => {
                setIsOpen(value);
            }}
        >
            <BulkUpdate
                onDismissCallback={(value) => {
                    setIsOpen(value);
                    setTimeout(() => {}, MESSAGE_CLOSE_AFTER);
                }}
            />
        </CustomModal>
    );
    const deleteConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpen}
            title="Delete Confirmation"
            message="Are you sure to delete this Slab?"
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpen(false);
                Delete(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpen(false);
            }}
        />
    );
    const ChannelCommissionReport = (
        <ReactTable data={Slabs} columns={mainColumns} />
    );
    const customLoader = loadingOpen && (
        <CircularProgress size={24} className={classes.buttonProgress} />
    );

    const slabForm = (formProps) => {
        return (
            <Form style={{ marginBottom: '15px' }}>
                <Grid container spacing={2}>
                    <Grid item xs={12} sm={12} md={9} lg={9}>
                        <h2 className={classes.PaperHead_h2}>Channel Slab</h2>
                    </Grid>
                    <Grid item xs={12} sm={12} md={3} lg={3}>
                        <Box className={classes.flexReverse}>
                            <PrimaryButton
                                variant="contained"
                                color="primary"
                                title="Bulk Update Commission"
                                className={classes.CustomPrimaryButtonStyle}
                                onClick={() => {
                                    setIsOpen(true);
                                }}
                            >
                                Bulk Update Commission
                            </PrimaryButton>
                        </Box>
                    </Grid>
                </Grid>

                <Grid item xs={12} style={{ marginTop: '15px' }}>
                    <Paper className={classes.PaperStyle}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialTextField
                                    label="Slab Name"
                                    name="SlabName"
                                    type="text"
                                    placeholder="Slab Name"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <PrimaryButton
                                    type="submit"
                                    title={OpenUpdateButton ? 'Update' : 'Add'}
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={loadingOpen}
                                >
                                    {OpenUpdateButton ? 'Update' : 'Add'}
                                </PrimaryButton>

                                {loadingOpen && (
                                    <CircularProgress
                                        size={24}
                                        className={classes.buttonProgress}
                                    />
                                )}
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <SecondaryButton
                                    type="reset"
                                    title="Reset"
                                    className={classes.CustomDangerButtonStyle}
                                    onClick={reset}
                                >
                                    Reset
                                </SecondaryButton>
                            </Grid>
                        </Grid>
                    </Paper>
                </Grid>

                <Grid className={classes.footerGrid}>
                    <Box className={classes.flexReverse}>
                        <div className={classes.SubmitWrapper}></div>
                    </Box>
                </Grid>
            </Form>
        );
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
                enableReinitialize={true}
                initialValues={SlabObj}
                validationSchema={SlabValidationSchema}
                onSubmit={submithandle}
                resetForm={SlabObj}
            >
                {slabForm}
            </Formik>

            {isAlertOpen ? deleteConfirmation : null}
            {isOpen ? bulkCommissionUpdate : null}
            {isOpenCopyCommissionModal ? copyCommissionModal : null}
            <h2 className={classes.PaperHead_h2}>Slab Detail</h2>
            {ChannelCommissionReport}
        </CustomContainer>
    );
};
