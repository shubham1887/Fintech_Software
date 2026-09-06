import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import ReactTable from 'Components/ReactTable/ReactTable';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { Select, SelectDocuments } from 'Components/Api/KYCAPI';
import { SelectPending as SELECT_PENDING } from 'Components/Api/KYCAPI';

import { DocumentList } from 'Pages/InnerPages/KYC/DocumentList';
import { LOGOUT } from 'ConstComponents';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import {
    Paper,
    Container,
    Button,
    Grid,
    Box,
    MenuItem,
    CircularProgress
} from '@material-ui/core';

import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { useHistory } from 'react-router';
import { Approve } from 'Pages/InnerPages/KYC/Approve';
const CustomContainer = HOCLoader(Container);

export const KYCApprove = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [reportData, setReportData] = useState({});
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [severity, setSeverity] = useState('success');
    const [modalOpen, setModalOpen] = useState(false);
    const [isOpenReport, setIsOpenReport] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [currentUserID, setCurrentUserID] = useState(0);
    const [currentName, setCurrentName] = useState('');
    const formRef = useRef();
    const [documents, setDocuments] = useState([]);
    const [statusList] = useState([
        { Name: 'All' },
        { Name: 'Approved' },
        { Name: 'Rejected' },
        { Name: 'Pending' }
    ]);

    var SearchObj = {
        Status: 'Pending',
        IsExcelDownload: false
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });
        columns.push({
            label: 'RequestDate',
            name: 'RequestDate',
            actived: true
        });
        columns.push({
            label: 'Person Name',
            name: 'PersonName',
            actived: true
        });

        columns.push({
            label: 'User',
            name: 'Name',
            actived: true
        });
        columns.push({
            label: 'Parent',
            name: 'Parent',
            actived: true
        });
        columns.push({
            label: 'ApproveDate',
            name: 'ApproveDate',
            actived: true
        });
        columns.push({
            label: 'Approved',
            name: 'ISApproved',
            approval: true,
            actived: true
        });
        columns.push({
            label: 'View',
            name: 'UserID',
            testbutton: true,
            callback: (UserID, row) => {
                selectDocumentsByUserID(UserID, row);
            }
        });

        columns.push({
            label: 'KYCApprove',
            name: 'KYCApproveName',
            actived: true
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        setLoadingOpen(false);
        SelectPending();
        createColumns();
    }, []);

    const selectDocumentsByUserID = async (UserID, row) => {
        setCurrentUserID(UserID);
        setCurrentName(row[2]);
        setReportData({
            UserID: UserID,
            FirmName: row[3],
            Date: row[1]
        });
        // history.push('/KYC/Approve', params);
        setIsOpenReport(true);
    };

    const SearchObjSchema = Yup.object({
        Status: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Status')
            .required(PLEASE_SELECT_VALID + 'Status')
    });

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(false);
        let jSONObj = JSON.stringify(values);
        setItemList([]);
        const response = await Select(jSONObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);

        if (data.IsExcelDownload) {
            downloadFile(data.ReportPath, 'KYC Approve');
        }
        setItemList(data.list);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }

    const SelectPending = async () => {
        let jSONObj = JSON.stringify(SearchObj);
        const response = await SELECT_PENDING(jSONObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            setItemList([]);
            return;
        }

        var data = JSON.parse(response.data);

        setItemList(data);
    };
    const OpenReportModalCallback = (value) => {
        setIsOpenReport(value);
    };
    const handleDismissCallback = () => {
        setModalOpen(false);
        SelectPending();
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
                initialValues={SearchObj}
                validationSchema={SearchObjSchema}
                onSubmit={submithandle}
                resetForm={SearchObj}
            >
                {({ setFieldValue, handleSubmit }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                KYC Approve/Reject
                            </h2>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Status"
                                                name="Status"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Status
                                                </MenuItem>
                                                {statusList.map((Item, key) => (
                                                    <MenuItem
                                                        key={key}
                                                        value={Item.Name}
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
                                            md={1}
                                            lg={1}
                                        >
                                            <Box
                                                style={{
                                                    textAlign: 'center'
                                                }}
                                            >
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
                                                        handleSubmit(e);
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

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Search"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loadingOpen}
                                                onClick={(e) => {
                                                    setFieldValue(
                                                        'IsExcelDownload',
                                                        false
                                                    );
                                                    handleSubmit(e);
                                                }}
                                            >
                                                Search
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
                                            >
                                                Reset
                                            </SecondaryButton>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <SecondaryButton
                                                type="Upload KYC"
                                                title="Upload KYC"
                                                className={
                                                    classes.CustomDangerButtonStyle
                                                }
                                                onClick={(e) => {
                                                    history.push('/KYC/Upload');
                                                }}
                                            >
                                                Upload KYC
                                            </SecondaryButton>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div
                                        className={classes.SubmitWrapper}
                                    ></div>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>

            <h2 className={classes.PaperHead_h2}>User List</h2>

            <ReactTable data={itemList} columns={mainColumns} />

            {isOpenReport && (
                <CustomModalWithScroll
                    isOpen={isOpenReport}
                    title={`KYC Document {Request Date - ${reportData.Date} }, { FirmName -  ${reportData.FirmName}} `}
                    width="100%"
                    onDismissCallback={OpenReportModalCallback}
                >
                    <Approve
                        reportData={reportData}
                        onDismissCallback={OpenReportModalCallback}
                    />
                </CustomModalWithScroll>
            )}
            {modalOpen ? (
                <DocumentList
                    currentName={currentName}
                    documents={documents}
                    UserID={currentUserID}
                    modalOpen={modalOpen}
                    onDismissCallback={handleDismissCallback}
                />
            ) : null}
        </CustomContainer>
    );
};
