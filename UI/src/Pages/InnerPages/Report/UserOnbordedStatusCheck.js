import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { CustomModal } from 'Components/Modal/CustomModal';
import { Container, Grid, MenuItem, Paper } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectUserOnbordingStatusCheckList as SELECT_USERONBORDING_LIST } from 'Components/Api/UsersApi';
import { UserOnbordingStatus as ONBORDING_STATUS } from 'Components/Api/PaysprintAEPSAPI';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { SelectByUserIDLog } from 'Pages/InnerPages/Report/SelectByUserIDLog';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

const UserOnbordedStatusCheck = () => {
    const formRef = useRef();
    const classes = commonStyle();

    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);

    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [currentPage, setCurrentPage] = useState(0);
    const [userList, setUserList] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [providerList, setProviderList] = useState([]);
    const [openFilter, setopenFilter] = useState(true);
    const [Pipe, setPipe] = useState([]);
    const [Amount, setAmount] = useState(0);
    const [TransactionID, setTransactionMasterID] = useState();
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [consumerNumber, setConsumerNumber] = useState(0);
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });
    const [ModelObj, setFormObj] = useState({
        User: userDefault,
        ProviderID: 0,
        Pipe: ''
    });

    // const GetPipeList = async () => {
    //     var request = {
    //         ID: 0
    //     };
    //     var json = JSON.stringify(request);

    //     const response = await BANK_LIST(json);

    //     setLoading(false);
    //     if (!response.isAuthorized) LOGOUT();

    //     if (!response.status) {
    //         setResponseMessage(response.data);
    //         setIsError(true);
    //     }
    //     var data = JSON.parse(response.data);
    //     setPipe(data);
    // };

    // const PipeList2 = Pipe.map((item) => ({
    //     ID: item.BankName, // Change oldName to newName
    //     Type: item.BankName
    // }));

    const PipeList = [
        {
            ID: 'bank1',
            Type: 'bank1'
        },
        {
            ID: 'bank2',
            Type: 'bank2'
        },
        {
            ID: 'bank3',
            Type: 'bank3'
        }
    ];
    const selectProvider = async () => {
        setLoading(true);
        const response = await PROVIDER_DROP_DOWN();
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);
        createColumns();
        await callSelectData(TakeCount, SkipCount, '', '', '');
        setSubmitting(false);
    };
    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            IsExcelDownload: formRef.current.values.IsExcelDownload,
            ProviderID: formRef.current.values.ProviderID,
            UserID: formRef.current.values.User.UserID
        };

        setIsError(false);
        setLoading(true);

        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);
        const { isAuthorized, status, data } = await SELECT_USERONBORDING_LIST(
            requestJSON
        );
        setLoading(false);
        if (!isAuthorized) LOGOUT();
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);

            return;
        }
        var d = JSON.parse(data);
        setTransactions(d);
    };
    const validationSchema = Yup.object({
        ProviderID: Yup.number()
            .nullable(PLEASE_SELECT_VALID + 'Provider')
            .min(0, PLEASE_SELECT_VALID + 'Provider')
            .required(PLEASE_SELECT_VALID + 'Provider')
    });

    const callSearch = async (text) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            text.length <= 0 ||
            text === userDefault.FirmName
        )
            return;

        var jsonObj = {
            FirmName: text
        };
        SelectUser(jsonObj);
    };

    const SelectUser = async (values) => {
        setLoading(true);
        var request = JSON.stringify(values);
        setLoading(false);
        const userResponse = await USER_SELECT_FOR_DROPDOWN(request);

        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(userResponse.data);

        let objWithSelectAll = [userDefault, ...data];

        setUserList(objWithSelectAll);
    };
    const StatusCheck = async (row) => {
        setResponseMessage('');
        var inputObj = {
            UserID: row[0],
            Pipe: row[3]
        };
        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await ONBORDING_STATUS(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        let data = JSON.parse(response.data);

        setIsError(true);
        setSeverity('success');
        setResponseMessage(data.Message);
        callSelectData(TakeCount, SkipCount, '', '', '');
    };
    const APILogModal = (
        <CustomModal
            isOpen={isModalOpen}
            title="API Log"
            width="75%"
            onDismissCallback={(value) => {
                setIsModalOpen(value);
            }}
        >
            <SelectByUserIDLog
                TransactionMasterID={TransactionID}
                ConsumerNumber={consumerNumber}
                Amount={Amount}
                setLoading={(value) => setLoading(value)}
            />
        </CustomModal>
    );

    useEffect(() => {
        createColumns();
        selectProvider();
        //GetPipeList();
    }, []);
    const logClick = async (request, TransactionID) => {
        setIsModalOpen(true);
        setTransactionMasterID(TransactionID);
        setConsumerNumber(request);
        setAmount(0);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });

        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });
        columns.push({
            label: 'MerchantCode',
            name: 'MerchantCode'
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            dropdown: true,
            list: PipeList
        });
        columns.push({
            label: 'StatusCheck',
            name: 'UserID',
            buttonWithTitle: true,
            buttonTitle: 'Status Check',
            callback: (row) => {
                StatusCheck(row);
            }
        });
        columns.push({
            label: 'Log',
            name: 'MobileNumber',
            buttonTitle: 'Log',
            testbutton: true,
            callback: (value, row) => {
                logClick(value, row[0]);
            }
        });

        setMainColumns(columns);
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
                enableReinitialize={true}
                initialValues={ModelObj}
                innerRef={formRef}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={ModelObj}
            >
                {({ setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'User Onboarded'}
                                        open={openFilter}
                                        setOpen={(value) =>
                                            setopenFilter(value)
                                        }
                                    >
                                        <Grid container spacing={3}>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialSelect
                                                    label="Provider"
                                                    name="ProviderID"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Provider
                                                    </MenuItem>
                                                    {providerList.map(
                                                        (
                                                            {
                                                                ProviderID,
                                                                ProviderName
                                                            },
                                                            index
                                                        ) => (
                                                            <MenuItem
                                                                key={index}
                                                                value={
                                                                    ProviderID
                                                                }
                                                            >
                                                                {ProviderName}
                                                            </MenuItem>
                                                        )
                                                    )}
                                                </MaterialSelect>
                                            </Grid>
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <Field
                                                    component={
                                                        AsyncAutoComplete
                                                    }
                                                    defaultValue={userDefault}
                                                    FieldName="User"
                                                    name="User"
                                                    SubFieldName="UserID"
                                                    options={userList}
                                                    getOptionLabel={(option) =>
                                                        option.FirmName
                                                    }
                                                    textFieldProps={{
                                                        label: 'Select User'
                                                    }}
                                                    onSearchCallback={(
                                                        text
                                                    ) => {
                                                        callSearch(text);
                                                    }}
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        setUserDefault({
                                                            UserID:
                                                                value.UserID,
                                                            FirmName:
                                                                value.FirmName
                                                        });
                                                    }}
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
                                                    title="Search"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    disabled={loading}
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
                                                {loading && (
                                                    <CircularProgress
                                                        size={24}
                                                        className={
                                                            classes.buttonProgress
                                                        }
                                                    />
                                                )}
                                            </Grid>
                                        </Grid>
                                    </CustomAccordion>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>

            <ReactTable
                data={transactions}
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                    callSelectData(
                        RowsPerPage,
                        RowsPerPage * currentPage,
                        '',
                        '',
                        ''
                    );
                }}
                getSearchCallback={({ SearchText }) => {
                    callSelectData(TakeCount, SkipCount, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    callSelectData(
                        TakeCount,
                        currentPage,
                        SearchText,
                        column,
                        direction
                    );
                }}
            />
            {isModalOpen ? APILogModal : null}
        </CustomContainer>
    );
};
export default UserOnbordedStatusCheck;
