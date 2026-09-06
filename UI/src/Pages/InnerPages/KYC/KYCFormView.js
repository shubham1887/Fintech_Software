import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form } from 'formik';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import * as Yup from 'yup';
import { Icon, IconButton, Tooltip } from '@material-ui/core';
import {
    PLEASE_SELECT_VALID,
    API_USER_CODE,
    RETAILER_CODE
} from 'Components/Helper/Messages';
import { LOGOUT } from 'ConstComponents';
import {
    Container,
    Grid,
    Button,
    TextField,
    CircularProgress,
    Paper,
    Box
} from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { Upload as UploadKYC } from 'Components/Api/KYCAPI';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { ImageView } from 'Pages/InnerPages/KYC/ImageView';
import ZoomInIcon from '@mui/icons-material/ZoomIn';
import { height } from '@material-ui/system';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const KYCFormView = ({
    Label,
    DocumentTextLabel,
    KycDocumentMasterID,
    UserID,
    SampleImageURL,
    DocumentNo,
    Status,
    ApprovedImageURL,
    onDismissCallback
}) => {
    const imageInputRef = useRef();
    const [documentImage, setDocumentImage] = useState('');
    const [DocumentDimension, setDocumentDimension] = useState({
        width: 0,
        height: 0
    });
    const [openFilter, setopenFilter] = useState(true);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [IsDOCApproved, setIsDOCApprove] = useState(false);
    const [IsUpdate, setIsUpdate] = useState(false);
    const [isRetailer, setIsRetailerRole] = useState(false);
    const [IsImageModelOpen, setIsImageModelOpen] = useState(false);
    const [ImageURL, setImageURL] = useState('');
    const [SelectedUserID, setSelectedUserID] = useState(0);

    const [rotation, setrotation] = useState(0);

    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [KYCObj, setKYCObj] = useState({
        KycDocumentMasterID: KycDocumentMasterID,
        DocumentNo: '',
        SelectedUserID: SelectedUserID,
        Document: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        }
    });

    const CheckUserRole = () => {
        if (RETAILER_CODE === parseInt(currentUsertypeid))
            setIsRetailerRole(true);
        if (API_USER_CODE === parseInt(currentUsertypeid))
            setIsRetailerRole(true);
    };

    useEffect(() => {
        SetDocumentDimension(documentImage);
        SetDocumentDimension(SampleImageURL);
    }, [documentImage, SampleImageURL]);

    useEffect(() => {
        setLoadingOpen(false);
        CheckUserRole();
        setSelectedUserID(UserID);
    }, [UserID]);

    useEffect(() => {
        if (Status === 'APPROVE' || Status === 'PROCESS') {
            setDocumentImage(ApprovedImageURL);
            setKYCObj({
                ...KYCObj,
                KycDocumentMasterID: KycDocumentMasterID,
                DocumentNo: DocumentNo,
                ApprovedImageURL: ApprovedImageURL
            });
            SetDocumentDimension(ApprovedImageURL);
            setIsUpdate(true);
        }
        if (Status === 'APPROVE') {
            setIsDOCApprove(true);
        }
    }, [Status, KycDocumentMasterID, DocumentNo, ApprovedImageURL]);

    const KYCObjScheama = Yup.object({
        KycDocumentMasterID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + ' document type')
            .required(PLEASE_SELECT_VALID + ' document type'),
        DocumentNo: Yup.string().required('Required')
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        let jsonObj = {
            KycDocumentMasterID: values.KycDocumentMasterID,
            DocumentNo: values.DocumentNo,
            SelectedUserID: SelectedUserID,

            Document: {
                Name: values.Document.Name,
                Size: values.Document.Size,
                Type: values.Document.Type,
                Data: values.Document.Data
            }
        };

        var requestObj = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);

        var response = await UploadKYC(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setResponseMessage(response.data);
        setIsError(true);

        if (!response.status) {
            setSeverity('error');
            setIsError(true);
            return;
        }

        setSubmitting(true);
        setSeverity('success');
        setDocumentImage('');
        setKYCObj({
            ...KYCObj,
            Note: 'Only jpg/jpeg and png files are allowed under 3MB!',
            KycDocumentMasterID: 0,
            DocumentNo: '',
            Document: {
                Name: '',
                Size: '',
                Type: '',
                Data: ''
            }
        });

        if (!imageInputRef.current) return;

        imageInputRef.current.value = '';

        onDismissCallback();
    };
    const SetDocumentDimension = (documentImage) => {
        if (!documentImage) {
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
        img.src = documentImage;
    };

    const getMax = (imageSize) => {
        if (imageSize > 300) return 300;

        return imageSize;
    };

    const RotateImageRight = () => {
        let newRotation = rotation + 90;
        if (newRotation >= 360) {
            newRotation = -360;
        }
        setrotation(newRotation);
    };

    const RotateImageLeft = () => {
        let newRotation = rotation - 90;
        if (newRotation >= 360) {
            newRotation = -360;
        }
        setrotation(newRotation);
    };

    const ImageZoom = (ApprovedImageURL) => {
        setImageURL(ApprovedImageURL);

        setIsImageModelOpen(true);
    };

    const CustomIMageModal = IsImageModelOpen ? (
        <ImageView
            isOpen={IsImageModelOpen}
            DocumentTextLabel={DocumentTextLabel}
            ImageURL={ImageURL}
            resendCallback={(value) => {}}
            onDismissCallback={(value) => {
                setIsImageModelOpen(value);
            }}
        />
    ) : null;

    const UploadForm = (formProps) => {
        return (
            <Form>
                <Paper className={classes.PaperStyle}>
                    <CustomAccordion
                        title={DocumentTextLabel}
                        open={openFilter}
                        setOpen={(value) => setopenFilter(value)}
                    >
                        <Grid
                            container
                            spacing={2}
                            xs={12}
                            sm={12}
                            md={12}
                            lg={12}
                        >
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label={Label}
                                    disabled={IsDOCApproved}
                                    name="DocumentNo"
                                    placeholder={'Enter ' + Label}
                                    type="text"
                                    length={30}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <span
                                    style={{
                                        horizontalAlign: 'center'
                                    }}
                                >
                                    Sample Image
                                    <br />
                                </span>

                                <img
                                    className="kycImg"
                                    name="SampleImageURL"
                                    src={SampleImageURL}
                                    height={DocumentDimension.height}
                                    width={getMax(DocumentDimension.width)}
                                    alt={SampleImageURL}
                                />
                            </Grid>

                            {IsDOCApproved === true ? (
                                <Grid item xs={12} sm={12} md={4} lg={4}>
                                    {/* <Tooltip title="Rotate Image Right">
                                        <IconButton
                                            className="kycicon"
                                            onClick={RotateImageRight}
                                        >
                                            <Icon>rotate_right</Icon>
                                        </IconButton>
                                    </Tooltip>
                                    <Tooltip title="Rotate Image Left">
                                        <IconButton
                                            className="kycicon"
                                            onClick={RotateImageLeft}
                                        >
                                            <Icon>rotate_left</Icon>
                                        </IconButton>
                                    </Tooltip> */}
                                    <Tooltip title="Zoom In">
                                        <Button
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            onClick={() => {
                                                ImageZoom(ApprovedImageURL);
                                            }}
                                        >
                                            <ZoomInIcon />
                                            <span
                                                style={{
                                                    horizontalAlign: 'left'
                                                }}
                                            >
                                                Zoom
                                                <br />
                                            </span>
                                        </Button>
                                    </Tooltip>

                                    <img
                                        style={{
                                            transform: `rotate(${rotation}deg)`
                                        }}
                                        className="kycImg"
                                        name="ApprovedImageURL"
                                        src={ApprovedImageURL}
                                        height={DocumentDimension.height}
                                        width={getMax(DocumentDimension.width)}
                                        alt={ApprovedImageURL}
                                    />
                                </Grid>
                            ) : null}

                            {IsDOCApproved === false ? (
                                <>
                                    <Grid item xs={12} sm={12} md={4} lg={4}>
                                        <TextField
                                            inputRef={imageInputRef}
                                            id="file"
                                            name="DocumentFile"
                                            type="file"
                                            accept=".png, .jpg, .jpeg"
                                            onChange={(e) => {
                                                var file = e.target.files[0];

                                                setDocumentImage(
                                                    URL.createObjectURL(file)
                                                );

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
                                                reader.onload = function (
                                                    item
                                                ) {
                                                    formProps.setFieldValue(
                                                        'Document.Data',
                                                        item.target.result
                                                            .replace(
                                                                'data:',
                                                                ''
                                                            )
                                                            .replace(/^.+,/, '')
                                                    );
                                                };

                                                reader.readAsDataURL(file);
                                            }}
                                        />

                                        <img
                                            name="documentImage"
                                            src={documentImage}
                                            height={DocumentDimension.height}
                                            width={getMax(
                                                DocumentDimension.width
                                            )}
                                            alt={documentImage}
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={1} lg={1}>
                                        <Grid className={classes.footerGrid}>
                                            <Box
                                                className={classes.flexReverse}
                                            >
                                                <div
                                                    className={
                                                        classes.SubmitWrapper
                                                    }
                                                >
                                                    <Button
                                                        type="submit"
                                                        title="KYC upload"
                                                        className={
                                                            classes.CustomPrimaryButtonStyle
                                                        }
                                                        onClick={(e) => {
                                                            formProps.setFieldValue(
                                                                'Upload',
                                                                true
                                                            );
                                                            formProps.handleSubmit(
                                                                e
                                                            );
                                                        }}
                                                        disabled={loadingOpen}
                                                    >
                                                        {IsUpdate
                                                            ? 'Update'
                                                            : 'Upload'}
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
                                            </Box>
                                        </Grid>
                                    </Grid>
                                </>
                            ) : null}
                        </Grid>
                    </CustomAccordion>
                </Paper>
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
                initialValues={KYCObj}
                validationSchema={KYCObjScheama}
                onSubmit={handleSubmit}
                resetForm={KYCObj}
            >
                {UploadForm}
            </Formik>

            {CustomIMageModal}
        </CustomContainer>
    );
};
