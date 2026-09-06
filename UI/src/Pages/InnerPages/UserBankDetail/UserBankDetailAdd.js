import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import Accordion from '@material-ui/core/Accordion';
import { LOGOUT } from 'ConstComponents';
import { Insert as BANK_ADD } from 'Components/Api/UserBankDetailApi';
import { Select as BANK_SELECT } from 'Components/Api/UserBankDetailApi';
import { Delete as BANK_DETAILS_DELETE } from 'Components/Api/UserBankDetailApi';
import { AmountInWordLabel } from 'Components/Inputes/FormikMaterialInput';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Container, Grid, Paper, TextField } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { ADMIN_CODE } from 'Components/Helper/Messages';
import { CustomModal } from 'Components/Modal/CustomModal';
import { QRImageView } from 'Pages/InnerPages/UserBankDetail/QRImageView';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);
const CustomAccordion = HOCAccordion(Accordion);

export const BankAdd = (DocumentTextLabel) => {
    const [Bank, setBank] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const classes = commonStyle();
    const [openFilter, setopenFilter] = useState(true);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [isUpdate, setIsUpdate] = useState(false);
    const [imagePath, setImagePath] = useState('');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [isImageModelOpen, setIsImageModelOpen] = useState(false);
    const [ImageURL, setImageURL] = useState('');
    const [BankName, setBankName] = useState('');

    var currentUsertypeid = localStorage.getItem('UserTypeID');

    const formObj = {
        UserBankDetailsID: 0,
        BankName: '',
        HolderName: '',
        AccountNumber: '',
        Branch: '',
        IFSC: '',
        UPINumber: '',
        Document: '',
        QRCodeImageURL: '',
        Document: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        }
    };

    useEffect(() => {
        setLoadingOpen(true);
        select();
        createColumns();
        CheckUserRole();
    }, []);

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
    };
    const onDeleteClick = (item) => {
        var RequestObj = { UserBankDetailsID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await BANK_DETAILS_DELETE(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        select();
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            return;
        }
        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);
        setIsError(false);
        var response = await BANK_ADD(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        setResponseMessage(response.data);
        setIsError(true);
        setSubmitting(false);

        setSeverity('success');
        if (!response.status) {
            setSeverity('error');
            return;
        }
        select();
        resetForm(formObj);
        setImagePath('');
    };

    const formValidationSchema = Yup.object({
        BankName: Yup.string()
            .matches(/^[a-zA-Z ]+$/, 'BankName accept characters only')
            .max(50, 'maximum 50 chars are required')
            .required(PLEASE_PROVIDE_VALID + 'bank name'),
        HolderName: Yup.string()
            .matches(/^[a-zA-Z ]+$/, 'HolderName accept characters only')
            .required(PLEASE_PROVIDE_VALID + 'holder name'),
        AccountNumber: Yup.string()
            .min(8, 'minimum 8 digits are required')
            .max(20, 'maximum 20 digits are required')
            .required(PLEASE_PROVIDE_VALID + 'account number'),

        Branch: Yup.string()
            .matches(/^[a-zA-Z ]+$/, 'BranchName accept characters only')
            .required(PLEASE_PROVIDE_VALID + 'branch name'),
        IFSC: Yup.string()
            .matches(
                /^[A-Za-z]{4}0[a-zA-Z0-9]{6}$/,
                'First 4 characters must be alphabets, 0, and last 6 characters must be numbers'
            )
            .required(PLEASE_PROVIDE_VALID + 'IFSC')
    });

    const select = async () => {
        const response = await BANK_SELECT();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setBank(data);
    };

    const Edit = (
        UserBankDetailsID,
        BankName,
        HolderName,
        AccountNumber,
        Branch,
        IFSC,
        UPINumber,
        QRCodeImageURL
    ) => {
        setIsUpdate(true);
        formObj.UserBankDetailsID = UserBankDetailsID;
        formObj.BankName = BankName;
        formObj.HolderName = HolderName;
        formObj.AccountNumber = AccountNumber;
        formObj.Branch = Branch;
        formObj.IFSC = IFSC;
        formObj.UPINumber = UPINumber;

        setImagePath(QRCodeImageURL);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserBankDetailsID',
            srno: true
        });
        columns.push({
            label: 'Bank',
            name: 'BankName'
        });
        columns.push({
            label: 'Name',
            name: 'HolderName'
        });
        columns.push({
            label: 'AccountNumber',
            name: 'AccountNumber'
        });
        columns.push({
            label: 'Branch',
            name: 'Branch'
        });

        columns.push({
            label: 'IFSC',
            name: 'IFSC'
        });

        if (isAdminRole) {
            columns.push({
                label: 'UPINumber',
                name: 'UPINumber'
            });
        }
        columns.push({
            label: 'QRCodeImageURL',
            name: 'QRCodeImageURL',
            hide: true
        });

        columns.push({
            label: 'Edit',
            name: 'Edit',
            buttonTitle: 'Edit',
            update: true,
            callback: (row) => {
                Edit(
                    row[0],
                    row[1],
                    row[2],
                    row[3],
                    row[4],
                    row[5],
                    row[6],
                    row[7]
                );
            }
        });
        columns.push({
            label: 'Delete',
            name: 'UserBankDetailsID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });

        columns.push({
            label: 'QR Code',
            name: 'QRCodeImageURL',
            buttonTitle: 'QR',
            testbutton: true,
            callback: (value, row) => {
                viewImg(value, row[1]);
            }
        });
        setMainColumns(columns);
    };

    const viewImg = async (request, BankName) => {
        setIsImageModelOpen(true);
        setImageURL(request);
        setBankName(BankName);
    };

    const UploadForm = (formProps) => {
        return (
            <Form>
                <h2 className={classes.PaperHead_h2}>Bank Details</h2>

                <Grid item xs={12}>
                    <Paper>
                        <CustomAccordion
                            title={'Fund Requests'}
                            open={openFilter}
                            setOpen={(value) => setopenFilter(value)}
                        >
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <MaterialTextField
                                        label="Bank Name"
                                        name="BankName"
                                        type="text"
                                        placeholder="Bank Name"
                                    />
                                </Grid>

                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <MaterialTextField
                                        label="Account Holder Name"
                                        name="HolderName"
                                        type="text"
                                        placeholder="Account Holder Name"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <MaterialNumberField
                                        label="Account Number"
                                        name="AccountNumber"
                                        type="text"
                                        placeholder=" Account Number"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <MaterialTextField
                                        label="Branch Name"
                                        name="Branch"
                                        type="text"
                                        placeholder="Branch Name"
                                    />
                                </Grid>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <MaterialTextField
                                        label="IFSC"
                                        name="IFSC"
                                        type="text"
                                        placeholder="IFSC"
                                    />
                                </Grid>
                                {isAdminRole ? (
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialTextField
                                            label="UPINumber"
                                            name="UPINumber"
                                            type="text"
                                            length="40"
                                            placeholder="UPINumber"
                                        />
                                    </Grid>
                                ) : null}

                                {isAdminRole ? (
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <label>QRCode Image</label>
                                        <TextField
                                            id="file"
                                            name="Document"
                                            type="file"
                                            accept=".png, .jpg, .jpeg"
                                            onChange={(e) => {
                                                var file = e.target.files[0];

                                                setImagePath(
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
                                        <AmountInWordLabel
                                            name="Note"
                                            readOnly={true}
                                            title="Only jpg/ jpeg/ pdf and png files are allowed under 3MB!"
                                        />
                                    </Grid>
                                ) : null}

                                {isAdminRole ? (
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <img
                                            name="QRCodeImageURL"
                                            src={imagePath}
                                            height={100}
                                            width={80}
                                            alt={imagePath}
                                        />
                                    </Grid>
                                ) : null}

                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <PrimaryButton
                                        type="submit"
                                        title={isUpdate ? 'Update' : 'Add'}
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        disabled={loadingOpen}
                                    >
                                        {!isUpdate ? 'Add' : 'Update'}
                                    </PrimaryButton>
                                    {loadingOpen && (
                                        <CircularProgress
                                            size={24}
                                            className={classes.buttonProgress}
                                        />
                                    )}
                                </Grid>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
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
                            </Grid>
                        </CustomAccordion>
                    </Paper>
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
                initialValues={formObj}
                validationSchema={formValidationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
            >
                {UploadForm}
            </Formik>
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete bank details."
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Delete(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}

            {isImageModelOpen && (
                <CustomModal
                    isOpen={isImageModelOpen}
                    title={BankName}
                    width="50%"
                    onDismissCallback={(value) => {
                        setIsImageModelOpen(value);
                    }}
                >
                    <QRImageView
                        ImageURL={ImageURL}
                        setLoading={(value) => setLoadingOpen(value)}
                    />
                </CustomModal>
            )}

            <h2 className={classes.PaperHead_h2}>Bank List</h2>
            <ReactTable data={Bank} columns={mainColumns} rowsPerPage={100} />
        </CustomContainer>
    );
};
