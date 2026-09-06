import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';

import { LOGOUT } from 'ConstComponents';
import {
    Add as SLAB_ADD,
    SelectByID as SLAB_SELECT_BY_ID,
    Activate as SLAB_ACTIVATE,
    Update as SALB_UPDATE
} from 'Components/Api/SlabApi';

import { Select as SLAB_SELECT } from 'Components/Api/SlabApi';
import { Delete as SLAB_DELETE } from 'Components/Api/SlabApi';
import SettingsIcon from '@material-ui/icons/Settings';
import AssignmentIndIcon from '@material-ui/icons/AssignmentInd';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import ReactTable from 'Components/ReactTable/ReactTable';
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
import { BulkUpdate } from 'Pages/InnerPages/Slabs/BulkUpdate';
import { CopyCommission } from 'Pages/InnerPages/Slabs/CopyCommission';

import {
    PLEASE_PROVIDE_VALID,
    SUPPORT_CODE,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
const CustomContainer = HOCLoader(Container);
export const SlabAdd = () => {
                                 const history = useHistory();
                                 const [Slabs, setSlabs] = useState([]);
                                 const [mainColumns, setMainColumns] = useState(
                                     []
                                 );
                                 const [
                                     loadingOpen,
                                     setLoadingOpen
                                 ] = React.useState(false);
                                 const classes = commonStyle();
                                 const [
                                     responseMessage,
                                     setResponseMessage
                                 ] = useState('');
                                 const [isError, setIsError] = useState(false);
                                 const [severity, setSeverity] = useState(
                                     'success'
                                 );
                                 const [isAlertOpen, setIsAlertOpen] = useState(
                                     false
                                 );
                                 const [
                                     confirmAlertRequest,
                                     setConfirmAlertRequest
                                 ] = useState({});
                                 const [SlabID, setSlabID] = useState(0);
                                 const [isOpen, setIsOpen] = useState(false);
                                 const [
                                     isOpenCopyCommissionModal,
                                     setIsOpenCopyCommissionModal
                                 ] = useState(false);

                                 const [
                                     OpenUpdateButton,
                                     setOpenUpdateButton
                                 ] = useState(false);
                                 const [CopySlabID, setCopySlabID] = useState(
                                     0
                                 );
                                 var currentUsertypeid = localStorage.getItem(
                                     'UserTypeID'
                                 );
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

                                     const response = await SLAB_SELECT_BY_ID(
                                         request
                                     );
                                     setLoadingOpen(false);

                                     if (
                                         SUPPORT_CODE ===
                                         parseInt(currentUsertypeid)
                                     ) {
                                         if (!response.isAuthorized) {
                                             setIsError(true);
                                             setSeverity('error');
                                             setResponseMessage(
                                                 'You dont have access'
                                             );

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
                                     const response = await SLAB_DELETE(
                                         jsonReq
                                     );
                                     setLoadingOpen(false);

                                     if (
                                         SUPPORT_CODE ===
                                         parseInt(currentUsertypeid)
                                     ) {
                                         if (!response.isAuthorized) {
                                             setIsError(true);
                                             setSeverity('error');
                                             setResponseMessage(
                                                 'You dont have access'
                                             );

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
                                     const response = await SLAB_ACTIVATE(
                                         request
                                     );
                                     setLoadingOpen(false);

                                     if (
                                         SUPPORT_CODE ===
                                         parseInt(currentUsertypeid)
                                     ) {
                                         if (!response.isAuthorized) {
                                             setIsError(true);
                                             setSeverity('error');
                                             setResponseMessage(
                                                 "You don't have access"
                                             );

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

                                 const handleSubmit = async (
                                     values,
                                     { setSubmitting, resetForm }
                                 ) => {
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

                                     if (
                                         SUPPORT_CODE ===
                                         parseInt(currentUsertypeid)
                                     ) {
                                         if (!response.isAuthorized) {
                                             setIsError(true);
                                             setSeverity('error');
                                             setResponseMessage(
                                                 "You don't have access"
                                             );

                                             setLoadingOpen(false);
                                             return;
                                         }
                                     }

                                     if (!response.isAuthorized) LOGOUT();

                                     setIsError(true);
                                     setSubmitting(false);

                                     if (!response.status) {
                                         setSeverity('error');
                                         setResponseMessage(response.data);
                                         return;
                                     }

                                     setIsError(false);
                                     setSeverity('success');
                                     setResponseMessage(response.data);
                                     resetForm(SlabObj);
                                     select();
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
                                     SlabName: Yup.string().required(
                                         PLEASE_PROVIDE_VALID + 'Slab Name'
                                     )
                                 });

                                 const select = async () => {
                                     const response = await SLAB_SELECT();

                                     if (
                                         SUPPORT_CODE ===
                                         parseInt(currentUsertypeid)
                                     ) {
                                         if (!response.isAuthorized) {
                                             setIsError(true);
                                             setSeverity('error');
                                             setResponseMessage(
                                                 "You don't have access"
                                             );

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
                                     history.push(
                                         '/Slabs/DMTCommission',
                                         redir
                                     );
                                 };
                                 // const CircleSetup = async (request) => {
                                 //     var redir = { SlabMasterID: request };
                                 //     history.push('/Slabs/CircleWiseCommission', redir);
                                 // };
                                 // const ROfferSetup = async (request) => {
                                 //     var redir = { SlabMasterID: request };
                                 //     history.push('/Slabs/ROfferCommission', redir);
                                 // };
                                 const Assign = async (request) => {
                                     var redir = { SlabMasterID: request };
                                     history.push('/Slabs/Assign', redir);
                                 };
                                 // const OfflineSetup = async (request) => {
                                 //     var redir = { SlabMasterID: request };
                                 //     history.push('/Slabs/OfflineSlabCommission', redir);
                                 // };

                                 const createColumns = () => {
                                     var columns = [];

                                     columns.push({
                                         label: '#',
                                         name: 'SlabID',
                                         srno: true,
                                         deactived: true
                                     });
                                     columns.push({
                                         label: 'Name',
                                         name: 'Name',
                                         deactived: true
                                     });
                                     columns.push({
                                         label: 'Unit',
                                         name: 'Count',
                                         deactived: true
                                     });
                                     columns.push({
                                         label: 'Setup',
                                         name: 'SlabID',

                                         options: {
                                             filter: false,
                                             sort: false,
                                             download: false,

                                             customBodyRender: (
                                                 value,
                                                 tableMeta,
                                                 updateValue
                                             ) => {
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

                                             customBodyRender: (
                                                 value,
                                                 tableMeta,
                                                 updateValue
                                             ) => {
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

                                             customBodyRender: (
                                                 value,
                                                 tableMeta,
                                                 updateValue
                                             ) => {
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

                                                 setIsOpenCopyCommissionModal(
                                                     true
                                                 );
                                             }
                                         }
                                     ];
                                     columns.splice(0, 0, {
                                         label: ' ',
                                         name: 'SlabID',
                                         MoreMenu: true,
                                         options: options
                                     });

                                     columns.push({
                                         label: 'IsActive',
                                         name: 'IsActive',
                                         hide: true
                                     });

                                     setMainColumns(columns);
                                 };
                                 const copyCommissionModal = (
                                     <CustomModalWithScroll
                                         isOpen={isOpenCopyCommissionModal}
                                         title="Copy Commission"
                                         width="75%"
                                         onDismissCallback={(value) => {
                                             setIsOpenCopyCommissionModal(
                                                 value
                                             );
                                             setCopySlabID(0);
                                         }}
                                     >
                                         <CopyCommission
                                             CopySlabID={CopySlabID}
                                         />
                                     </CustomModalWithScroll>
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
                                 const bulkUpdateCommission = (
                                     <CustomModalWithScroll
                                         isOpen={isOpen}
                                         title="Bulk Update Commission"
                                         width="75%"
                                         onDismissCallback={(value) => {
                                             setIsOpen(value);
                                         }}
                                     >
                                         <BulkUpdate
                                             onDismissCallback={(value) => {
                                                 setIsOpen(value);
                                                 setTimeout(() => {},
                                                 MESSAGE_CLOSE_AFTER);
                                             }}
                                         />
                                     </CustomModalWithScroll>
                                 );
                                 const slabForm = (formProps) => {
                                     return (
                                         <Form style={{ marginBottom: '15px' }}>
                                             <Grid container spacing={2}>
                                                 <Grid
                                                     item
                                                     xs={12}
                                                     sm={12}
                                                     md={9}
                                                     lg={9}
                                                 >
                                                     <h2
                                                         className={
                                                             classes.PaperHead_h2
                                                         }
                                                     >
                                                         Slab
                                                     </h2>
                                                 </Grid>
                                                 <Grid
                                                     item
                                                     xs={12}
                                                     sm={12}
                                                     md={3}
                                                     lg={3}
                                                 >
                                                     <Box
                                                         className={
                                                             classes.flexReverse
                                                         }
                                                     >
                                                         <PrimaryButton
                                                             variant="contained"
                                                             color="primary"
                                                             title="Bulk Update Commission"
                                                             className={
                                                                 classes.CustomPrimaryButtonStyle
                                                             }
                                                             onClick={() => {
                                                                 setIsOpen(
                                                                     true
                                                                 );
                                                             }}
                                                         >
                                                             Bulk Update
                                                             Commission
                                                         </PrimaryButton>
                                                     </Box>
                                                 </Grid>
                                             </Grid>

                                             <Grid
                                                 item
                                                 xs={12}
                                                 style={{ marginTop: '15px' }}
                                             >
                                                 <Paper
                                                     className={
                                                         classes.PaperStyle
                                                     }
                                                 >
                                                     <Grid
                                                         container
                                                         spacing={2}
                                                     >
                                                         <Grid
                                                             item
                                                             xs={12}
                                                             sm={12}
                                                             md={2}
                                                             lg={2}
                                                         >
                                                             <MaterialTextField
                                                                 label="Slab Name"
                                                                 name="SlabName"
                                                                 type="text"
                                                                 placeholder="Slab Name"
                                                             />
                                                         </Grid>
                                                         <Grid
                                                             item
                                                             xs={12}
                                                             sm={12}
                                                             md={2}
                                                             lg={2}
                                                         >
                                                             <PrimaryButton
                                                                 type="submit"
                                                                 title={
                                                                     OpenUpdateButton
                                                                         ? 'Update'
                                                                         : 'Add'
                                                                 }
                                                                 className={
                                                                     classes.CustomPrimaryButtonStyle
                                                                 }
                                                                 disabled={
                                                                     loadingOpen
                                                                 }
                                                             >
                                                                 {OpenUpdateButton
                                                                     ? 'Update'
                                                                     : 'Add'}
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
                                                             md={2}
                                                             lg={2}
                                                         >
                                                             <SecondaryButton
                                                                 type="reset"
                                                                 title="Reset"
                                                                 className={
                                                                     classes.CustomDangerButtonStyle
                                                                 }
                                                                 onClick={reset}
                                                             >
                                                                 Reset
                                                             </SecondaryButton>
                                                         </Grid>
                                                     </Grid>
                                                 </Paper>
                                             </Grid>

                                             <Grid
                                                 className={classes.footerGrid}
                                             >
                                                 <Box
                                                     className={
                                                         classes.flexReverse
                                                     }
                                                 >
                                                     <div
                                                         className={
                                                             classes.SubmitWrapper
                                                         }
                                                     ></div>
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
                                             validationSchema={
                                                 SlabValidationSchema
                                             }
                                             onSubmit={handleSubmit}
                                             resetForm={SlabObj}
                                         >
                                             {slabForm}
                                         </Formik>

                                         {isAlertOpen
                                             ? deleteConfirmation
                                             : null}
                                         {isOpen ? bulkUpdateCommission : null}
                                         {isOpenCopyCommissionModal
                                             ? copyCommissionModal
                                             : null}

                                         <h2 className={classes.PaperHead_h2}>
                                             Slab Master
                                         </h2>
                                         <ReactTable
                                             data={Slabs}
                                             columns={mainColumns}
                                         />
                                     </CustomContainer>
                                 );
                             };
