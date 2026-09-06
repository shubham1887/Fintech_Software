import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import * as Yup from 'yup';
import { Tooltip } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { Container, Grid, Button, Paper } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { ImageView } from 'Pages/InnerPages/KYC/ImageView';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import { Approve, SelectBYID as SELECT_BY_ID } from 'Components/Api/KYCAPI';
import ZoomInIcon from '@mui/icons-material/ZoomIn';

const CustomContainer = HOCLoader(Container);

export const KYCFormViewForAdmin = ({
    Label,
    DocumentTextLabel,
    KycDocumentMasterID,
    SampleImageURL,
    DocumentNo,
    Status,
    ApprovedImageURL,
    Remark
}) => {
    const [documentImage, setDocumentImage] = useState('');
    const [DocumentDimension, setDocumentDimension] = useState({
        width: 0,
        height: 0
    });

    const [loadingOpen, setLoadingOpen] = useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [IsDOCApproved, setIsDOCApprove] = useState(false);
    const [rotation, setrotation] = useState(0);
    const [IsImageModelOpen, setIsImageModelOpen] = useState(false);
    const [ImageURL, setImageURL] = useState('');
    const [KYCObject, setKYCObject] = useState({
        UserDocumentID: KycDocumentMasterID,
        UserID: 0,
        IsApproved: false,
        DocumentType: DocumentTextLabel,
        DocumentNo: DocumentNo,
        Name: '',
        DocumentStatus: '',
        Remark: ''
    });
    const ImageZoom = (ApprovedImageURL) => {
        setImageURL(ApprovedImageURL);

        setIsImageModelOpen(true);
    };
    const SearchByID = async () => {
        let JSONObj = {
            UserDocumentID: KycDocumentMasterID
        };

        var request = JSON.stringify(JSONObj);

        const response = await SELECT_BY_ID(request);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            return;
        }

        var data = JSON.parse(response.data);

        if (data.Status === 'APPROVE') {
            setIsDOCApprove(true);
        }
        if (data.Status === 'REJECT') {
            setIsDOCApprove(true);
        }
        setKYCObject({
            UserDocumentID: data.UserDocumentID,
            DocumentType: data.DocumentType,
            DocumentNo: data.DocumentNo,
            Name: data.Name,
            UserID: data.UserID,
            DocumentStatus: data.Status,
            Remark: data.Remark
        });
    };

    useEffect(() => {
        SetDocumentDimension(documentImage);
        SetDocumentDimension(SampleImageURL);
    }, [documentImage, SampleImageURL]);

    useEffect(() => {
        setLoadingOpen(false);
        SearchByID();
    }, []);

    useEffect(() => {
        if (Status === 'APPROVE' || Status === 'PROCESS') {
            setDocumentImage(ApprovedImageURL);
            SetDocumentDimension(ApprovedImageURL);
        }
        if (Status === 'APPROVE') {
            setIsDOCApprove(true);
        }
        if (Status === 'REJECT') {
            setIsDOCApprove(true);
        }
    }, [Status, KycDocumentMasterID, DocumentNo, ApprovedImageURL, Remark]);

    const KYCObjSchema = Yup.object({
        Remark: Yup.string().required(PLEASE_PROVIDE_VALID + 'Remark')
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);

        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);
        setIsError(false);

        const response = await Approve(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setResponseMessage(response.data);

        setIsError(true);
        setSubmitting(false);
        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            return;
        }

        resetForm(KYCObject);
        SearchByID();
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
                height: 120
            });
        };
        img.src = documentImage;
    };

    const getMax = (imageSize) => {
        if (imageSize > 300) return 300;

        return imageSize;
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
                    <Grid container spacing={2} xs={12}>
                        <Grid item xs={12} sm={12} md={4} lg={4}>
                            <MaterialTextField
                                label={DocumentTextLabel}
                                disabled={IsDOCApproved}
                                name="DocumentNo"
                                placeholder={
                                    'Enter ' + DocumentTextLabel + ' No'
                                }
                                type="text"
                                length={30}
                            />
                        </Grid>

                        <Grid item xs={12} sm={12} md={4} lg={4}>
                            <Tooltip title="Zoom In">
                                <Button
                                    variant="contained"
                                    color="primary"
                                    size="small"
                                    className="kycicon"
                                    // type="submit"
                                    onClick={() => {
                                        ImageZoom(ApprovedImageURL);
                                    }}
                                >
                                    <ZoomInIcon />
                                    <span
                                        style={{
                                            horizontalAlign: 'center'
                                        }}
                                    >
                                        Zoom
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

                        <Grid item xs={12} sm={12} md={4} lg={4}>
                            <Grid container spacing={2} xs={12}>
                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    FirmName : {formProps.values.Name}
                                </Grid>

                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    Doc. Status :
                                    {formProps.values.DocumentStatus}
                                </Grid>
                                {IsDOCApproved && (
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        Remark :{formProps.values.Remark}
                                    </Grid>
                                )}
                            </Grid>

                            {!IsDOCApproved && (
                                <>
                                    <Grid item xs={12} sm={12} md={12} lg={12}>
                                        <MaterialTextField
                                            label="Remark"
                                            name="Remark"
                                            type="text"
                                            placeholder="Remark"
                                        />
                                    </Grid>
                                    <Grid container spacing={2} xs={12}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <Button
                                                style={{
                                                    margin: '10px'
                                                }}
                                                type="submit"
                                                variant="contained"
                                                color="primary"
                                                size="small"
                                                onClick={(e) => {
                                                    formProps.setFieldValue(
                                                        'IsApproved',
                                                        true
                                                    );
                                                    formProps.handleSubmit(e);
                                                }}
                                            >
                                                Approve
                                            </Button>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <Button
                                                style={{
                                                    margin: '10px'
                                                }}
                                                type="submit"
                                                variant="contained"
                                                color="secondary"
                                                size="small"
                                                onClick={(e) => {
                                                    formProps.setFieldValue(
                                                        'IsApproved',
                                                        false
                                                    );
                                                    formProps.handleSubmit(e);
                                                }}
                                            >
                                                Reject
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </>
                            )}
                        </Grid>
                    </Grid>
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
                initialValues={KYCObject}
                validationSchema={KYCObjSchema}
                onSubmit={handleSubmit}
                resetForm={KYCObject}
            >
                {UploadForm}
            </Formik>
            {CustomIMageModal}
        </CustomContainer>
    );
};
