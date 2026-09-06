import React, { useState, useEffect, useRef } from 'react';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';
import { ExportTypeItems } from 'Components/ReportHelper';
import { Formik, Form } from 'formik';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { SelectReport as SELECT } from 'Components/Api/OnboardingCallbackAPI';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import { useHistory } from 'react-router';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import { SelectByID as SELECT_BY_ID } from 'Components/Api/OnboardingCallbackAPI';
import { ActionAgentOnboarding } from './ActionAgentOnboarding';
import { ViewDetails } from './ViewDetails';
import {
    ADMIN_CODE,
} from 'Components/Helper/Messages';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const ViewAgentOnboarding = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const history = useHistory();
    const [loading, setLoading] = useState(false);
    const [isOpen, setIsOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [openFilter, setopenFilter] = useState(true);
    const [severity, setSeverity] = useState('success');
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [isViewOpen, setIsViewOpen] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState(false);
    const [isAgentUserDetails, setAgentUserDetails] = useState({});
    const [isOnBoradingUserID, setisOnBoradingUserID] = useState(0);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [currentPage, setCurrentPage] = useState(0);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [isExportType, setIsExportType] = useState(false);
    var currentUserTypeId = parseInt(localStorage.getItem('UserTypeID'));

    const [ModelObj, setModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ProviderID: 0,
        ServiceID: 0,
        MakerStatus: "All",
        CheckerStatus: "PROCESS"
    });

    useEffect(() => {
        CheckUserRole();
    }, [isAdminRole]);
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        callSelectData(TakeCount, SkipCount, '', '', '');
        setSubmitting(false);
    };


    useEffect(() => {
        callSelectData(TakeCount, SkipCount, '', '', '');
        createColumns();
    }, []);

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUserTypeId)) setIsAdminRole(true);
        createColumns();
        callSelectData(TakeCount, SkipCount, '', '', '');
    };


    const callSelectData = async () => {
        setTransactions([]);
        const inputOBJ = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText,
            ExportType: formRef.current.values.ExportType,
            MakerStatus: formRef.current.values.MakerStatus,
            CheckerStatus: formRef.current.values.CheckerStatus
        }
        var response = await SELECT(JSON.stringify(inputOBJ));
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        var data = JSON.parse(response.data);
        if (data.IsDownload) {
            downloadFile(data.ReportPath, 'Onboarding Report');
            return;
        }
        setTransactions(data.list);
        if (SkipCount <= 0) {
            setTotalCount(data.Count);
        }
    };
    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }
    const OnChangeCriteria = (value) => {
        if (value === undefined || value <= 0) {
            setIsDisplayCriteria(false);
            setCriteriaTextLabel('');
            return;
        }
        setCriteriaTextLabel(value);
        setIsDisplayCriteria(true);
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: 'Action',
            name: 'OnboardingUserID',

            statusIndex: (isAdminRole ? 7 : 6),
            button: true,
            disabledOn: ['APPROVE'],
            callback: (row) => {

                HandleAction(row);
            }

        });
        if (!isAdminRole) {

            columns.push({
                label: 'Edit',
                name: 'OnboardingUserID',

                statusIndex: 8,
                edit: true,
                disabledOn: ['APPROVE',],
                callback: (row) => {
                    HandleEdit(row);

                }

            });
        }
        columns.push({
            label: 'Show',
            name: 'OnboardingUserID',

            statusIndex: 1,
            Show: true,
            color: "primary",
            disabledOn: [''],

            callback: (row) => {
                SelectByID(row);
            }

        });
        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });

        columns.push({
            label: 'Entity Name',
            name: 'FullName'
        });
        columns.push({
            label: 'Mobile Number',
            name: 'MobileNumber'
        });
        columns.push({
            label: 'Maker Verified Status',
            name: 'SupportStatus',
            customStatus: true
        });
        columns.push({
            label: 'Maker Remark',
            name: 'SupportRemark'
        });

        columns.push({
            label: 'Checker Verified Status',
            name: 'AdminStatus',
            customStatus: true
        });
        columns.push({
            label: 'Checker Remark',
            name: 'AdminRemark'
        });

        columns.push({
            label: 'Created Date',
            name: 'DOC'
        });

        setMainColumns(columns);
    };
    const HandleAction = async (value) => {
        setisOnBoradingUserID(value);
        setIsOpen(true);
    }

    const HandleEdit = (value) => {
        history.push('/ViewAgentOnboarding/Edit/' + value);
    }
        ;
    const SelectByID = async (value) => {

        let JSONObj = {
            OnboardingUserID: value
        };


        var response = await SELECT_BY_ID(JSON.stringify(JSONObj));
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {

            setSeverity('response');
            setIsError(true);
            setResponseMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);
        setIsViewOpen(true);
        setAgentUserDetails(data);
    }

    const HandleExportTypeChange = (value) => {
        setIsExportType(false);
    }
    const HandleCreateAgent = () => {
        history.push('/ManualAgentOnboarding');
    }
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
                innerRef={formRef}
                initialValues={ModelObj}

                onSubmit={handleSubmit}
                resetForm={ModelObj}
                render={({
                    values,
                    setFieldValue,
                    handleChange,
                    handleSubmit
                }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'View Agent'}
                                        open={openFilter}
                                        setOpen={(value) =>
                                            setopenFilter(value)
                                        }
                                    >
                                        <Grid container spacing={3}>


                                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                                <MaterialSelect
                                                    label="Selection Criteria"
                                                    name="Criteria"
                                                    title="criteria"
                                                    onCustomCallback={(name, value) => {
                                                        OnChangeCriteria(value);
                                                    }}
                                                >
                                                    <MenuItem key={0} value={0}>
                                                        Selection Criteria
                                                    </MenuItem>
                                                    <MenuItem key={1} value={'EntityName'}>
                                                        Entity Name
                                                    </MenuItem>
                                                    <MenuItem key={2} value={'MobileNumber'}>
                                                        Mobile Number
                                                    </MenuItem>
                                                    <MenuItem key={3} value={'PANNo'}>
                                                        PAN Number
                                                    </MenuItem>
                                                </MaterialSelect>
                                            </Grid>

                                            {!IsDisplayCriteria ? null : (
                                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                                    <MaterialTextField
                                                        label={CriteriaTextLabel}
                                                        name="CriteriaText"
                                                        type="text"
                                                        placeholder={
                                                            'Enter ' + CriteriaTextLabel
                                                        }
                                                        length={50}
                                                    />
                                                </Grid>
                                            )}

                                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                                <MaterialSelect
                                                    label="Maker Status"
                                                    name="MakerStatus"
                                                    title="MakerStatus"
                                                >
                                                    <MenuItem key="All" value="All">
                                                        All
                                                    </MenuItem>
                                                    <MenuItem key="PROCESS" value="PROCESS">
                                                        PROCESS
                                                    </MenuItem>
                                                    <MenuItem key="APPROVE" value="APPROVE">
                                                        APPROVE
                                                    </MenuItem>
                                                    <MenuItem key="REJECT" value="REJECT">
                                                        REJECT
                                                    </MenuItem>
                                                </MaterialSelect>
                                            </Grid>
                                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                                <MaterialSelect
                                                    label="Checker Status"
                                                    name="CheckerStatus"
                                                    title="CheckerStatus"

                                                >
                                                    <MenuItem key="All" value="All">
                                                        All
                                                    </MenuItem>
                                                    <MenuItem key="PROCESS" value="PROCESS">
                                                        PROCESS
                                                    </MenuItem>
                                                    <MenuItem key="APPROVE" value="APPROVE">
                                                        APPROVE
                                                    </MenuItem>
                                                    <MenuItem key="REJECT" value="REJECT">
                                                        REJECT
                                                    </MenuItem>
                                                </MaterialSelect>

                                            </Grid>

                                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                                <MaterialSelect
                                                    label="ExportType"
                                                    name="ExportType"
                                                    title="ExportType"

                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {

                                                        HandleExportTypeChange(value)


                                                    }}

                                                >
                                                    {ExportTypeItems}
                                                </MaterialSelect>
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
                                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                                <PrimaryButton
                                                    label="Create Agent"
                                                    name="CreateAgent"
                                                    title="Create Agent"
                                                    onClick={() => HandleCreateAgent()}
                                                >
                                                </PrimaryButton>
                                            </Grid> 

                                        </Grid>
                                    </CustomAccordion>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />
            {isViewOpen && (
                <CustomModalWithScroll
                    isOpen={isViewOpen}
                    title="View Details"
                    width="95%"
                    onDismissCallback={(value) => {
                        setIsViewOpen(value);

                    }}
                >
                    <ViewDetails Details={isAgentUserDetails} />
                </CustomModalWithScroll>
            )}
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
                    callSelectData(TakeCount, currentPage, SearchText, '', '');
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



            {isOpen && (
                <CustomModalWithScroll
                    isOpen={isOpen}
                    title="Action"
                    width="100%"
                    onDismissCallback={(value) => {
                        setIsOpen(value);

                    }}
                >
                    <ActionAgentOnboarding
                        IsEdit={true}
                        OnBoardingUserID={isOnBoradingUserID}
                        onDismissCallback={(value) => {
                            setIsOpen(value);
                            callSelectData(TakeCount, SkipCount, '', '', '');

                        }}
                    />
                </CustomModalWithScroll>
            )}
        </CustomContainer>
    )
}
