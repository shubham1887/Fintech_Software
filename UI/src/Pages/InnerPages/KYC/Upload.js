import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import { Typography } from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';
import { Container, Paper, Grid } from '@material-ui/core';
import {
    SelectActiveDocUserwise as SELECT_DOC,
    SelectDocuments
} from 'Components/Api/KYCAPI';
import { ADMIN_CODE, SUPPORT_CODE } from 'Components/Helper/Messages';
import { AmountInWordLabel } from 'Components/Inputes/FormikMaterialInput';
import HOCLoader from 'Components/Loader/HOCLoader';
import { useLocation } from 'react-router-dom';
import { KYCFormView } from 'Pages/InnerPages/KYC/KYCFormView';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';

import { FETCH_LIST } from 'Config';
import { RETAILER } from 'Components/Helper/UsertypeCode';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
const CustomContainer = HOCLoader(Container);
export const Upload = () => {
    const formRef = useRef();
    const location = useLocation();
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [isAdminRole, setIsAdminRole] = useState(false);

    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [DocumentTypeList, setDocumentTypeList] = useState([]);
    const UserID = location?.state?.UserID;

    const [IsFetchAllList, setisFetchAllList] = useState(false);
    var currentUserTypeId = localStorage.getItem('UserTypeID');
    const [downline, setDownline] = useState([]);

    const [isDisplayUploadForm, setIsDisplayUploadForm] = useState(false);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });
    const [KYCObj, setKYCObj] = useState({
        Note: 'Only jpg/jpeg and png files are allowed under 3MB!',
        KycDocumentMasterID: '',
        DocumentNo: '',
        Document: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        },
        User: userDefault
    });

    const CheckUserRole = () => {
        if (
            ADMIN_CODE === parseInt(currentUserTypeId) ||
            SUPPORT_CODE === parseInt(currentUserTypeId)
        ) {
            setIsAdminRole(true);
            setIsDisplayUploadForm(false);
        }

        if (
            ADMIN_CODE !== parseInt(currentUserTypeId) &&
            SUPPORT_CODE !== parseInt(currentUserTypeId)
        ) {
            setIsDisplayUploadForm(true);
        }
    };
    useEffect(() => {
        setLoadingOpen(false);
        selectDocType();
        if (userDefault.UserID !== 0)
            selectDocumentsByUserID(userDefault.UserID);

        selectDocumentsByUserID(UserID);
        CheckUserRole();
        fetchUserList();
    }, [userDefault]);

    const DisplayUploadForm = async (value) => {
        if (value.UserID > 0) {
            setIsDisplayUploadForm(true);
            selectDocumentsByUserID(value.UserID);
            selectDocType(value.UserID);
        }
    };

    const callSearch = async (text) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            text.length <= 0 ||
            text === userDefault.FirmName
        )
            return;

        SelectUser(text);
    };
    const fetchUserList = async () => {
        let listValue = await FETCH_LIST();

        if (listValue === undefined || listValue === null || listValue === '') {
            return;
        }

        if (listValue !== undefined && listValue === 'All') {
            setisFetchAllList(true);
            SelectUser('');
            return;
        }
    };
    const SelectUser = async (text) => {
        if (RETAILER === parseInt(currentUserTypeId)) {
            let objWithSelectAll = [userDefault];
            setDownline(objWithSelectAll);
            return;
        }
        var jsonObj = {
            FirmName: text
        };

        if (isValidToRefreshList(downline) === false) {
            return;
        }

        setLoadingOpen(true);
        var request = JSON.stringify(jsonObj);
        const userResponse = await USER_SELECT_FOR_DROPDOWN(request);

        setLoadingOpen(false);
        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(userResponse.data);

        let objWithSelectAll = [userDefault, ...data];

        setDownline(objWithSelectAll);
    };

    const isValidToRefreshList = (list) => {
        if (IsFetchAllList === false) {
            return true;
        }

        if (list === undefined || list === null) {
            return true;
        }

        if (list.length <= 1) {
            return true;
        }

        return false;
    };

    const selectDocType = async (currentUserID) => {
        const response = await SELECT_DOC();
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        if (currentUserID > 0) {
            setIsDisplayUploadForm(true);
        }

        var data = JSON.parse(response.data);

        setDocumentTypeList(data);
    };

    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
    };

    const selectDocumentsByUserID = async (currentUserID) => {
        setLoadingOpen(true);

        if (currentUserID === 0) return;

        let jSONObj = {
            UserID: currentUserID
        };

        const response = await SelectDocuments(JSON.stringify(jSONObj));
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        setIsDisplayUploadForm(true);
        var data = JSON.parse(response.data);
        setDocumentTypeList(data);
    };

    const handleDismissCallback = () => {
        selectDocumentsByUserID();
        refreshForm();
        selectDocType();
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
                initialValues={KYCObj}
                resetForm={KYCObj}
            >
                {() => {
                    return (
                        <Form>
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <AmountInWordLabel
                                    name="Note"
                                    readOnly={true}
                                    title="Only jpg/jpeg and png files are allowed under 3MB!"
                                />
                            </Grid>
                            <Paper className={classes.PaperStyle}>
                                <Typography component={'span'}>
                                    <h6>KYC Upload</h6>
                                </Typography>

                                {isAdminRole ? (
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <Field
                                            component={AsyncAutoComplete}
                                            defaultValue={userDefault}
                                            FieldName="User"
                                            name="User"
                                            SubFieldName="UserID"
                                            options={downline}
                                            getOptionLabel={(option) =>
                                                option.FirmName
                                            }
                                            textFieldProps={{
                                                label: 'Select User'
                                            }}
                                            onSearchCallback={(text) => {
                                                callSearch(text);
                                            }}
                                            onCustomCallback={(name, value) => {
                                                DisplayUploadForm(value);

                                                setUserDefault({
                                                    UserID: value.UserID,
                                                    FirmName: value.FirmName
                                                });
                                            }}
                                        />
                                    </Grid>
                                ) : null}
                            </Paper>
                            {isDisplayUploadForm ? (
                                <>
                                    {DocumentTypeList.map(
                                        (
                                            {
                                                Label,
                                                KycDocumentMasterID,
                                                DocumentType,
                                                SampleImageURL,
                                                DocumentNo,
                                                Status,
                                                ApprovedImageURL
                                            },
                                            index
                                        ) => {
                                            return (
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <KYCFormView
                                                        Label={Label}
                                                        DocumentTextLabel={
                                                            DocumentType
                                                        }
                                                        KycDocumentMasterID={
                                                            KycDocumentMasterID
                                                        }
                                                        UserID={
                                                            userDefault.UserID
                                                        }
                                                        SampleImageURL={
                                                            SampleImageURL
                                                        }
                                                        DocumentNo={DocumentNo}
                                                        Status={Status}
                                                        ApprovedImageURL={
                                                            ApprovedImageURL
                                                        }
                                                        onDismissCallback={
                                                            handleDismissCallback
                                                        }
                                                    />
                                                </Grid>
                                            );
                                        }
                                    )}
                                </>
                            ) : null}
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
