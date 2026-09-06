import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import { Container, Grid, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import AllProviderWisePerformanceGraph from '../ProviderGraph/AllProviderWisePerformanceGraph';
import SelectProviderWisePerformance from '../ProviderGraph/SelectProviderWisePerformance';
import { ADMIN_CODE } from 'Components/Helper/Messages';

export const ProviderWisePerformanceGraph = () => {
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [providerList, setProviderList] = useState([]);
    const [selectedDate, setSelectedDate] = useState(new Date());
    const [openFilter, setOpenFilter] = useState(true);
    const [selectedProviderID, setSelectedProviderID] = useState(0);
    const CustomAccordion = HOCAccordion(Accordion);
    const CustomContainer = HOCLoader(Container);
    useEffect(() => {
        const currentUsertypeid = localStorage.getItem('UserTypeID');
        const isAdmin = parseInt(currentUsertypeid) === ADMIN_CODE;
        setIsAdminRole(isAdmin);

        const fetchProviderList = async () => {
            const response = await PROVIDER_DROP_DOWN();
            if (!response.isAuthorized) {
                LOGOUT();
                return;
            }
            if (response.status) {
                const data = JSON.parse(response.data);
                setProviderList(data);
            }
        };

        fetchProviderList();
    }, []);

    const handleSubmit = async (values, { setSubmitting }) => {
        setSelectedDate(new Date(values.Date)); 
        setSelectedProviderID(values.ProviderID);
        setSubmitting(false);
    };

    return (
        <CustomContainer>
            <Formik
                initialValues={{
                    Date: selectedDate, 
                    ProviderID: selectedProviderID,
                }}
                onSubmit={handleSubmit}
            >
                {({ values, setFieldValue, handleSubmit }) => (
                    <Form onSubmit={handleSubmit}>
                          <CustomAccordion
                                             title={'Provider Wise Performance Graph'}
                                        open={openFilter}
                                        setOpen={(value) =>
                                            setOpenFilter(value)
                                        }
                                    >
                        <Grid container spacing={2}>
                            <Grid item xs={12} md={2}>
                                <MaterialDatePickerField
                                    name="Date"
                                    label="Date"
                                    value={values.Date}
                                    onChange={(date) =>
                                        setFieldValue('Date', date)
                                    }
                                    maxDate={new Date()}
                                />
                            </Grid>
                            {isAdminRole && (
                                <Grid item xs={12} md={2}>
                                    <MaterialSelect
                                        name="ProviderID"
                                        label="Provider"
                                        value={values.ProviderID}
                                        onChange={(event) =>
                                            setFieldValue(
                                                'ProviderID',
                                                event.target.value
                                            )
                                        }
                                    >
                                        <MenuItem value={0}>All</MenuItem>
                                        {providerList.map(
                                            ({ ProviderID, ProviderName }) => (
                                                <MenuItem
                                                    key={ProviderID}
                                                    value={ProviderID}
                                                >
                                                    {ProviderName}
                                                </MenuItem>
                                            )
                                        )}
                                    </MaterialSelect>
                                </Grid>
                            )}
                            <Grid item xs={12} md={1}>
                                <PrimaryButton type="submit" title="Search">
                                    Search
                                </PrimaryButton>
                            </Grid>
                        </Grid>
                        </CustomAccordion>
                    </Form>
                )}
            </Formik>

            {selectedProviderID > 0 ? (
                <SelectProviderWisePerformance
                    date={selectedDate.toISOString()} 
                    providerID={selectedProviderID}
                />
            ) : (
                <AllProviderWisePerformanceGraph   date={selectedDate.toISOString()}  />
            )}
        </CustomContainer>
    );
};
