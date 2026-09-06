import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { LOGOUT } from 'ConstComponents';
import {
    Add as SLAB_ADD,
    SelectByID as SLAB_SELECT_BY_ID,
    Activate as SLAB_ACTIVATE,
    Update as SALB_UPDATE
} from 'Components/Api/ROfferCommissionAPI';

import { Select as SLAB_SELECT } from 'Components/Api/ROfferCommissionAPI';
import { Delete as SLAB_DELETE } from 'Components/Api/ROfferCommissionAPI';
import SettingsIcon from '@material-ui/icons/Settings';
import AssignmentIndIcon from '@material-ui/icons/AssignmentInd';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import ReactTable from 'Components/ReactTable/ReactTable';
import FormHeader from 'Components/FormHeader';
import {
    Container,
    Grid,
    Paper,
    Button,
    CircularProgress,
    Box
} from '@material-ui/core';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { useHistory } from 'react-router';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { CustomModal } from 'Components/Modal/CustomModal';
import { BulkUpdate } from 'Pages/InnerPages/Slabs/BulkUpdate';
import { CopyCommission } from 'Pages/InnerPages/Slabs/CopyCommission';

import {
    PLEASE_PROVIDE_VALID,
    SUPPORT_CODE,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);
export const ROfferCommissionAdd = () => {
    const history = useHistory();
    const [Slabs, setSlabs] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [SlabID, setSlabID] = useState(0);
    const [isOpen, setIsOpen] = useState(false);
    const [isOpenCopyCommissionModal, setIsOpenCopyCommissionModal] = useState(
        false
    );

    const [OpenUpdateButton, setOpenUpdateButton] = useState(false);
    const [CopySlabID, setCopySlabID] = useState(0);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    useEffect(() => {
        setLoadingOpen(true);
        select();
        createColumns();
    }, []);

    const SelectSlabByID = async (SlabID) => {
        let JSONObj = {
            SlabID: SlabID
        };
        setLoadingOpen(true);
        var request = JSON.stringify(JSONObj);

        const response = await SLAB_SELECT_BY_ID(request);
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

        var data = JSON.parse(response.data);

        setSlabObj({
            SlabID: data.SlabID,
            SlabName: data.Name
        });
    };

    const onDeleteClick = (item) => {
        var RequestObj = { SlabID: item };
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
                setResponseMessage('You dont have access');
                setSeverity('error');
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
    const activate = async (item, value) => {
        var jsonObj = {
            SlabID: item,
            IsActive: value
        };

        var request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);
        const response = await SLAB_ACTIVATE(request);
        setLoadingOpen(false);

        if (SUPPORT_CODE === parseInt(currentUsertypeid)) {
            if (!response.isAuthorized) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage("You don't have access");

                setLoadingOpen(false);
                return;
            }
        }
        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        if (!response.status) setSeverity('error');
        setIsError(true);
        select();
    };

    const [SlabObj, setSlabObj] = useState({
        SlabID: SlabID,
        SlabName: ''
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;

        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);
        setIsError(false);

        var response;
        response =
            values.SlabID === 0
                ? await SLAB_ADD(requestObj)
                : await SALB_UPDATE(requestObj);

        setLoadingOpen(false);

        if (SUPPORT_CODE === parseInt(currentUsertypeid)) {
            if (!response.isAuthorized) {
                setIsError(true);
                setSeverity('error');
                setResponseMessage("You don't have access");

                setLoadingOpen(false);
                return;
            }
        }

        if (!response.isAuthorized) LOGOUT();

        setResponseMessage(response.data);
        setIsError(true);
        setSubmitting(false);
        select();
        setSeverity('success');
        if (!response.status) {
            setSeverity('error');
            return;
        }
        resetForm(SlabObj);

        setOpenUpdateButton(false);
        reset();
    };
    const reset = () => {
        setSlabObj({
            SlabID: 0,
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
                setResponseMessage("You don't have access");

                setLoadingOpen(false);
                return;
            }
        }

        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setSlabs(data);
        setLoadingOpen(false);
    };

    const Edit = async (SlabID) => {
        setOpenUpdateButton(true);
        setSlabID(SlabID);
        SelectSlabByID(SlabID);
    };

    const Setup = async (request) => {
        var redir = { SlabMasterID: request };
        history.push('/Slabs/Commission', redir);
    };
    const DMTSetup = async (request) => {
        var redir = { SlabMasterID: request };
        history.push('/Slabs/DMTCommission', redir);
    };
    const Assign = async (request) => {
        var redir = { SlabMasterID: request };
        history.push('/Slabs/Assign', redir);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'SlabID',
            srno: true
        });
        columns.push({
            label: 'Name',
            name: 'Name'
        });

        columns.push({
            label: 'Setup',
            name: 'SlabID',

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
            name: 'SlabID',

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
            label: 'Assign',
            name: 'SlabID',

            options: {
                filter: false,
                sort: false,
                download: false,

                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <AssignmentIndIcon
                            variant="contained"
                            color="primary"
                            onClick={(event) => {
                                Assign(value);
                            }}
                        />
                    );
                }
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
            label: 'Edit',
            name: 'SlabID',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'SlabID',
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
            name: 'SlabID',
            MoreMenu: true,
            options: options
        });
        setMainColumns(columns);
    };
    const copyCommissionModal = (
        <CustomModal
            isOpen={isOpenCopyCommissionModal}
            title="Copy Commission"
            width="75%"
            onDismissCallback={(value) => {
                setIsOpenCopyCommissionModal(value);
                setCopySlabID(0);
            }}
        >
            <CopyCommission CopySlabID={CopySlabID} />
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
            {isAlertOpen ? deleteConfirmation : null}

            {isOpenCopyCommissionModal ? copyCommissionModal : null}

            <h2 className={classes.PaperHead_h2}>ROffer Commission</h2>
            <ReactTable data={Slabs} columns={mainColumns} />
        </CustomContainer>
    );
};
