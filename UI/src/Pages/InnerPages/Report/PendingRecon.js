import {
    Box,
    Container,
    Grid,
    TextField,
    Typography
} from '@material-ui/core';
import { Button } from '@material-ui/core';
import { Formik, Form } from 'formik';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { FileUpload } from 'Components/Api/AgentDetailsAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
import { LOGOUT } from 'ConstComponents';
import React, { useEffect, useState, useRef } from 'react';

const CustomContainer = HOCLoader(Container);

export const PendingRecon = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [dataMessage, setDataMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const fileInputRef = useRef(null);
    const [document, setDocument] = useState('');
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

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        setIsError(false);

        const { isAuthorized, status, data } = await FileUpload(requestObj);
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

    const handleClear = () => {
        if (fileInputRef.current) {
            fileInputRef.current.value = '';
        }
        formRef.current.resetForm();
        setDocument('');
        setDataMessage('');
    };

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
                                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                                <Box display="flex" justifyContent="center" mb={2}>
                                                    <TextField
                                                        id="file"
                                                        name="DocumentFile"
                                                        type="file"
                                                        fullWidth
                                                        variant="outlined"
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
                                                </Box>
                                            </Grid>
                                        </Box>
                                    </Grid>

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

                                        </Box>
                                        <Box display="flex" justifyContent="center" mb={2}>
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
                                                {dataMessage.split('.').filter(message => message.trim() !== '').map((message, index) => (
                                                    <span key={index}>
                                                        {message.trim()}.
                                                        <br />
                                                    </span>
                                                ))}
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