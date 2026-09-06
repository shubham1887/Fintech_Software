import {
    Box,
    Container,
    Grid,
    MenuItem,
    TextField,
    Typography
} from '@material-ui/core';
import { Button } from '@material-ui/core';
import { Formik, Form } from 'formik';
import {
    SelectAgentList as USER_SELECT_LIST,
} from 'Components/Api/UsersApi';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { Upload } from 'Components/Api/AgentDetailsAPI';

import {
    ALL_USERS,
    RETAILER,
} from 'Components/Helper/UsertypeCode';
import HOCLoader from 'Components/Loader/HOCLoader';
import { LOGOUT } from 'ConstComponents';
import React, { useEffect, useState, useRef } from 'react';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

const CustomContainer = HOCLoader(Container);

export const AgentDetailReport = () => {
    const formRef = useRef();
    const classes = commonStyle();

    const [loadingOpen, setLoadingOpen] = useState(false);
    const [IsTextValue, setIsTextValue] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [dataMessage, setDataMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [value, setValue] = useState(ALL_USERS);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [UsersList, setUsersList] = useState([]);
    const [sortColumnName, setSortColumnName] = useState('');
    const [sortDirection, setSortDirection] = useState('');
    const [headerSummary, setHeaderSummary] = useState([]);
    const [document, setDocument] = useState('');
    const [IsDMTWallet, setIsDMTWallet] = useState(false);
    const fileInputRef = useRef(null);
    const callSelectData = (
        TakeCount,
        SkipCount,
        UserTypeID,
        Search,
        excelDownload,
        MobileNumber,
        ParentID,
        FirmName,
        SlabName
    ) => {
        var request = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            UserTypeID: RETAILER,
            Search: Search,
            MobileNumber: MobileNumber,
            ParentID: ParentID,
            FirmName: FirmName,
            SlabName: SlabName,
            SortColumnName: sortColumnName,
            SortDirection: sortDirection,
            IsExcelDownload: excelDownload,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText,
            CriteriaNumber: formRef.current.values.CriteriaNumber,
            CriteriaValue: formRef.current.values.CriteriaValue,
            IsTextValue: IsTextValue,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            ServiceType: formRef.current.values.ServiceType
        };

        selectAllUsers(request);
    };

    const selectAllUsers = async (request) => {
        let requestObj = JSON.stringify(request);
        setUsersList([]);
        setHeaderSummary([]);

        setLoadingOpen(true);
        const response = await USER_SELECT_LIST(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        if (data.IsExcelDownload === true) {
            downloadFile(data.ReportPath, 'User Agent Report');
            return;
        }

        setTotalCount(data.Count);
        setUsersList(data.data);
        setHeaderSummary(data.headerlist);
    };
    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }
    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date().addDays(-30),
        Criteria: 0,
        CriteriaText: '',
        CriteriaNumber: '',
        CriteriaValue: false,
        IsTextValue: false,
        IsSwitchValue: false,
        ServiceType: '',
    });

    const handleClear = () => {
        if (fileInputRef.current) {
            fileInputRef.current.value = '';
        }
        formRef.current.resetForm();
        setDocument('');
        setDataMessage('');
    };

    const [activeTab, setActiveTab] = useState('');

    const handleTabChange = (tab) => {
        setActiveTab(tab);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        setIsError(false);

        const { isAuthorized, status, data } = await Upload(requestObj);
        if (!isAuthorized) LOGOUT();
        setLoadingOpen(false);
        setSeverity('success');
        setDataMessage(data);
        if (!status) {
            setResponseMessage(data);

            setSeverity('error');
            setIsError(true);
            return;
        }
    };

    useEffect(() => {
        setLoadingOpen(true);
        callSelectData(TakeCount, SkipCount, RETAILER, '', false);
    }, [TakeCount, IsDMTWallet]);

    const customContainerCallback = (alertState) => {
        setIsError(alertState);
    };

    return (
        <Grid item xs={12}>
            <CustomContainer isLoading={loadingOpen}
                isError={isError}
                alertMessage={responseMessage}
                severity={severity}
                onCloseCallback={customContainerCallback}>
                <Formik
                    enableReinitialize={true}
                    innerRef={formRef}
                    initialValues={ModelObj}
                    onSubmit={submithandle}
                    resetForm={ModelObj}

                >
                    {({ setFieldValue, handleSubmit, formProps }) => (
                        <Form>
                            <Box
                                display="flex"
                                justifyContent="center"
                                mb={2}
                                p={2}
                                boxShadow={3}
                                borderRadius={4}
                                bgcolor="background.paper"
                            >
                                <Grid container spacing={2} alignItems="center">

                                    <Grid item xs={12}>
                                        <Box display="flex" justifyContent="center" mb={2}>
                                            <Button
                                                variant="contained"
                                                className={classes.CustomPrimaryButtonStyle}
                                                style={{ backgroundColor: 'primary', color: '#fff' }}
                                                onClick={() => handleTabChange('upload')}
                                            >
                                                Upload
                                            </Button>
                                            <Button
                                                variant="contained"
                                                className={classes.CustomPrimaryButtonStyle}
                                                style={{
                                                    backgroundColor: 'primary',
                                                    color: '#fff',
                                                    marginLeft: '10px'
                                                }}
                                                onClick={() => handleTabChange('download')}
                                            >
                                                Download
                                            </Button>
                                        </Box>
                                    </Grid>

                                    <Grid item xs={12}>
                                        <Box display="flex" justifyContent="center" mb={2}>
                                            <Grid item xs={12} sm={12} md={3} lg={3}>

                                                <MaterialSelect
                                                    label="Select Service"
                                                    name="ServiceType"
                                                    fullWidth
                                                >
                                                    <MenuItem value="">
                                                        Select Service
                                                    </MenuItem>
                                                    <MenuItem value="BBPS">
                                                        BBPS
                                                    </MenuItem>
                                                    <MenuItem value="MicroATM">
                                                        MicroATM
                                                    </MenuItem>
                                                    <MenuItem value="PinMaster">
                                                        PinMaster
                                                    </MenuItem>
                                                </MaterialSelect>
                                            </Grid>
                                        </Box>
                                    </Grid>

                                    {activeTab === 'upload' && (
                                        <Grid item xs={12}>
                                            <Box display="flex" justifyContent="center" mb={2}>
                                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                                    <TextField
                                                        id="file"
                                                        name="DocumentFile"
                                                        type="file"
                                                        onChange={(e) => {
                                                            var file = e.target.files[0];

                                                            setDocument(URL.createObjectURL(file));

                                                            var reader = new FileReader();
                                                            setFieldValue(
                                                                'ExcelDocument.Name',
                                                                file.name
                                                            );
                                                            setFieldValue(
                                                                'ExcelDocument.Size',
                                                                file.size
                                                            );
                                                            setFieldValue(
                                                                'ExcelDocument.Type',
                                                                file.type
                                                            );
                                                            reader.onload = function (item) {
                                                                setFieldValue(
                                                                    'ExcelDocument.Data',
                                                                    item.target.result
                                                                        .replace('data:', '')
                                                                        .replace(/^.+,/, '')
                                                                );
                                                            };

                                                            reader.readAsDataURL(file);
                                                        }}
                                                    />
                                                </Grid>
                                            </Box>
                                        </Grid>
                                    )}

                                    <Grid item xs={12}>
                                        <Box display="flex" justifyContent="center" mb={2}>
                                            <Button
                                                variant="outlined"
                                                color="primary"
                                                style={{ marginRight: '10px' }}
                                                onClick={handleClear} 
                                            >
                                                Clear
                                            </Button>

                                            {activeTab === 'upload' && (
                                                <Button
                                                    variant="contained"
                                                    className={classes.CustomPrimaryButtonStyle}
                                                    style={{
                                                        backgroundColor: 'primary',
                                                        color: '#fff',
                                                        marginRight: '10px'
                                                    }}
                                                    type="submit"
                                                    onClick={handleSubmit}
                                                >
                                                    Upload
                                                </Button>
                                            )}

                                            {activeTab === 'download' && (

                                                <Button
                                                    variant="contained"
                                                    className={classes.CustomPrimaryButtonStyle}
                                                    style={{
                                                        backgroundColor: 'primary',
                                                        color: '#fff',
                                                        marginRight: '10px'
                                                    }}
                                                    type="submit"
                                                    onClick={() => {
                                                        callSelectData(
                                                            100,
                                                            10,
                                                            value,
                                                            '',
                                                            true
                                                        );
                                                    }}
                                                >
                                                    Download
                                                </Button>
                                            )}
                                            <Typography
                                                variant="subtitle1"
                                                color="primary"
                                                style={{
                                                    backgroundColor: '#f0f0f0',
                                                    padding: '10px',
                                                    borderRadius: '5px',
                                                    fontWeight: 'bold'
                                                }}
                                            >
                                                {dataMessage}
                                            </Typography>
                                        </Box>
                                    </Grid>
                                </Grid>
                            </Box>
                        </Form>
                    )}
                </Formik>
            </CustomContainer>
        </Grid>
    );
};
