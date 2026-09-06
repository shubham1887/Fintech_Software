import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { LOGOUT } from 'ConstComponents';
import { Container, Grid } from '@material-ui/core';
import { SelectDocuments } from 'Components/Api/KYCAPI';
import { useHistory } from 'react-router';
import { AmountInWordLabel } from 'Components/Inputes/FormikMaterialInput';
import HOCLoader from 'Components/Loader/HOCLoader';
import { useLocation } from 'react-router-dom';
import { KYCFormViewForAdmin } from 'Pages/InnerPages/KYC/KYCFormViewForAdmin';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
const CustomContainer = HOCLoader(Container);
export const Approve = ({ reportData }) => {
    const UserID = reportData.UserID;

    const location = useLocation();
    const history = useHistory();
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [documents, setDocuments] = useState([]);
    const classes = commonStyle();

    const [KYCObj, setKYCObj] = useState({
        Note: 'Only jpg/jpeg and png files are allowed under 3MB!',
        KycDocumentMasterID: '',
        DocumentNo: '',
        Document: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        }
    });

    useEffect(() => {
        setLoadingOpen(false);

        selectDocumentsByUserID(UserID);
    }, []);

    const selectDocumentsByUserID = async (UserID) => {
        setLoadingOpen(true);

        let jSONObj = {
            UserID: UserID
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

        var data = JSON.parse(response.data);

        setDocuments(data);
    };

    const UploadForm = (formProps) => {
        return (
            <Form>
                <Grid container spacing={2}>
                    <Grid item xs={12} sm={12} md={11} lg={11}>
                        <AmountInWordLabel
                            name="Note"
                            readOnly={true}
                            title="Only jpg/jpeg and png files are allowed under 3MB!"
                        />
                    </Grid>
                </Grid>

                {documents.map(
                    (
                        {
                            Label,
                            UserDocumentID,
                            DocumentType,
                            SampleImageURL,
                            DocumentNo,
                            Status,
                            Document,
                            Remark
                        },
                        index
                    ) => {
                        return (
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <KYCFormViewForAdmin
                                    Label={Label}
                                    DocumentTextLabel={DocumentType}
                                    KycDocumentMasterID={UserDocumentID}
                                    SampleImageURL={SampleImageURL}
                                    DocumentNo={DocumentNo}
                                    Status={Status}
                                    ApprovedImageURL={Document}
                                    Remark={Remark}
                                />
                            </Grid>
                        );
                    }
                )}
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
                resetForm={KYCObj}
            >
                {UploadForm}
            </Formik>
        </CustomContainer>
    );
};
