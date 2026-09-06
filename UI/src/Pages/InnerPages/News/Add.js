import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, FieldArray } from 'formik';

import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { LOGOUT } from 'ConstComponents';
import { Insert as NEWS_ADD } from 'Components/Api/NewsAPI';
import { Select as USERTYPE_SELECT } from 'Components/Api/UserTypeApi';
import {
    MaterialTextField,
    MaterialTextAreaField
} from 'Components/Inputes/FormikMaterialInput';
import { Container, Grid, Button } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import Autocomplete from '@material-ui/lab/Autocomplete';
import TextField from '@material-ui/core/TextField';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

const NewsAdd = ({ onDismissCallback }) => {
    const classes = commonStyle();
    const [newsImage, setDocumentImage] = useState('');
    const [DocumentDimension, setDocumentDimension] = useState({
        width: 0,
        height: 0
    });
    const imageInputRef = useRef();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');

    const [userTypeList, setUserTypeList] = useState([]);

    var FormObj = {
        Title: '',
        Description: '',
        HyperLink: '',
        UserTypeList: [],
        Document: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        }
    };

    useEffect(() => {
        selectUserTypes();
    }, []);

    useEffect(() => {
        if (!newsImage) {
            setDocumentDimension({
                width: 0,
                height: 0
            });
            return;
        }
        var img = new Image();

        img.onload = function () {
            let newWidth = (this.width * 200) / this.height;
            setDocumentDimension({
                width: newWidth,
                height: 170
            });
        };
        img.src = newsImage;
    }, [newsImage]);

    const getMax = (imageSize) => {
        if (imageSize > 300) return 300;

        return imageSize;
    };
    const selectUserTypes = async () => {
        const response = await USERTYPE_SELECT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setUserTypeList(data);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        setLoading(true);
        setIsError(false);

        var response = await NEWS_ADD(JSON.stringify(values));
        if (!response.isAuthorized) LOGOUT();
        setResponseMessage(response.data);
        setLoading(false);

        setIsError(true);

        setSeverity('success');

        setSubmitting(false);

        if (!response.status) {
            setSeverity('error');
            return;
        }

        resetForm(FormObj);
        setTimeout(() => {
            onDismissCallback(false);
        }, MESSAGE_CLOSE_AFTER);
    };

    const FormValidationSchema = Yup.object({
        Title: Yup.string().required(PLEASE_PROVIDE_VALID + 'title'),
        Description: Yup.string().required(PLEASE_PROVIDE_VALID + 'description')
    });

    const AddForm = (formProps) => {
        return (
            <Form>
                <Grid item xs={12} lg={12} md={12}>
                    <Grid container spacing={1}>
                        <Grid
                            item
                            xs={12}
                            sm={12}
                            md={12}
                            className={classes.multiSelect}
                        >
                            <FieldArray
                                name="UserTypeList"
                                render={(userTypeIDList) => (
                                    <Autocomplete
                                        multiple
                                        id="UserTypeData"
                                        options={userTypeList}
                                        getOptionLabel={(option) =>
                                            option.UserType
                                        }
                                        onChange={(event, values) => {
                                            values.UserTypeList = [];
                                            values.map((category) => {
                                                userTypeIDList.push({
                                                    UserTypeID:
                                                        category.UserTypeID
                                                });
                                                values.UserTypeList.push({
                                                    UserTypeID:
                                                        category.UserTypeID
                                                });
                                            });
                                        }}
                                        renderInput={(params) => (
                                            <TextField
                                                {...params}
                                                variant="standard"
                                                label="User(s)"
                                                placeholder="User"
                                            />
                                        )}
                                    />
                                )}
                            />
                        </Grid>

                        <Grid item xs={12} sm={12} md={12} lg={12}>
                            <MaterialTextField
                                label="Title"
                                name="Title"
                                type="text"
                                placeholder="Title"
                            />
                        </Grid>

                        <Grid item xs={12} sm={12} md={12} lg={12}>
                            <MaterialTextAreaField
                                label="Description"
                                name="Description"
                                type="text"
                                placeholder="Description"
                            />
                        </Grid>
                        <Grid item xs={12} sm={12} md={12} lg={12}>
                            <MaterialTextAreaField
                                label="HyperLink"
                                name="HyperLink"
                                type="text"
                                placeholder="HyperLink"
                            />
                        </Grid>

                        <Grid item xs={12} sm={12} md={6} lg={6}>
                            <TextField
                                inputRef={imageInputRef}
                                id="file"
                                name="DocumentFile"
                                type="file"
                                accept=".png, .jpg, .jpeg"
                                onChange={(e) => {
                                    var file = e.target.files[0];

                                    setDocumentImage(URL.createObjectURL(file));

                                    var reader = new FileReader();
                                    formProps.setFieldValue(
                                        'Document.Name',
                                        file.name
                                    );
                                    formProps.setFieldValue(
                                        'Document.Size',
                                        file.size
                                    );
                                    formProps.setFieldValue(
                                        'Document.Type',
                                        file.type
                                    );
                                    reader.onload = function (item) {
                                        formProps.setFieldValue(
                                            'Document.Data',
                                            item.target.result
                                                .replace('data:', '')
                                                .replace(/^.+,/, '')
                                        );
                                    };

                                    reader.readAsDataURL(file);
                                }}
                            />
                        </Grid>
                        <Grid item xs={12} sm={12} md={6} lg={6}>
                            <img
                                name="newsImage"
                                src={newsImage}
                                height={DocumentDimension.height}
                                width={getMax(DocumentDimension.width)}
                                alt={newsImage}
                            />
                        </Grid>
                    </Grid>
                </Grid>

                <Grid className={classes.footerGrid}>
                    <Box className={classes.flexReverse}>
                        <div className={classes.SubmitWrapper}>
                            <Button
                                type="submit"
                                title="Save"
                                variant="contained"
                                color="primary"
                                disabled={loading}
                            >
                                Save
                            </Button>
                            {loading && (
                                <CircularProgress
                                    size={24}
                                    className={classes.buttonProgress}
                                />
                            )}
                        </div>
                        <Button
                            type="reset"
                            title="Reset"
                            variant="contained"
                            color="secondary"
                            className={classes.CustomDangerButtonStyle}
                        >
                            Reset
                        </Button>
                    </Box>
                </Grid>
            </Form>
        );
    };
    return (
        <CustomContainer
            isLoading={loading}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Formik
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submithandle}
                resetForm={FormObj}
            >
                {AddForm}
            </Formik>
        </CustomContainer>
    );
};

export default NewsAdd;
