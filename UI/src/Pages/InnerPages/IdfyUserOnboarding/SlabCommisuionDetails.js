import React, { useState, useEffect, useRef } from 'react';
import { Tabs, Tab, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, IconButton } from '@material-ui/core';
import ArrowBackIosIcon from '@material-ui/icons/ArrowBackIos';
import ArrowForwardIosIcon from '@material-ui/icons/ArrowForwardIos';

function SlabCommisuionDetails({ SlabDetails }) {
    const [groupedData, setGroupedData] = useState({});
    const [selectedTab, setSelectedTab] = useState("");
    const tabsRef = useRef(null);

    useEffect(() => {
        const grouped = SlabDetails.reduce((acc, item) => {
            if (!acc[item.ServiceName]) {
                acc[item.ServiceName] = [];
            }
            acc[item.ServiceName].push(item);
            return acc;
        }, {});
        setGroupedData(grouped);

        if (Object.keys(grouped).length > 0) {
            setSelectedTab(Object.keys(grouped)[0]);
        }
    }, [SlabDetails]);

    const handleTabChange = (event, newValue) => {
        setSelectedTab(newValue);
    };

    const scrollTabs = (direction) => {
        if (tabsRef.current) {
            const scrollAmount = direction === 'left' ? -200 : 200;
            tabsRef.current.scrollLeft += scrollAmount;
        }
    };

    const tabContainerStyle = {
        display: 'flex',
        alignItems: 'center',
        marginTop: '20px',
    };

    return (
        <div>
            <div style={tabContainerStyle}>
                <IconButton onClick={() => scrollTabs('left')}>
                    <ArrowBackIosIcon />
                </IconButton>
                <div style={{ flex: 1, overflowX: 'auto', whiteSpace: 'nowrap' }} ref={tabsRef}>
                    <Tabs
                        value={selectedTab}
                        onChange={handleTabChange}
                        variant="scrollable"
                        scrollButtons="off"
                        style={{ minWidth: 'max-content', padding: '10px 0' }}
                    >
                        {Object.keys(groupedData).map((serviceName) => (
                            <Tab key={serviceName} label={serviceName} value={serviceName} />
                        ))}
                    </Tabs>
                </div>
                <IconButton onClick={() => scrollTabs('right')}>
                    <ArrowForwardIosIcon />
                </IconButton>
            </div>

            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Slab Name</TableCell>
                            <TableCell>Operator Name</TableCell>
                            <TableCell>From Amount</TableCell>
                            <TableCell>To Amount</TableCell>
                            <TableCell>Retailer</TableCell>
                            <TableCell>Distributor</TableCell>
                            <TableCell>Super Distributor</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {(groupedData[selectedTab] || []).map((item) => (
                            <TableRow key={item.SlabDetailID}>
                                <TableCell>{item.SlabName}</TableCell>
                                <TableCell>{item.OperatorName}</TableCell>
                                <TableCell>{item.FromAmount}</TableCell>
                                <TableCell>{item.ToAmount}</TableCell>
                                <TableCell>{item.RetailerCommissionCategory === "Surcharge" ? "-" : "+"}{item.RetailerComm}{item.RetailerCommType !== "Fix" ? "%" : " Rs"}</TableCell>
                                <TableCell>{item.DistributorCommissionCategory === "Surcharge" ? "-" : "+"}{item.DistributorComm}{item.DistributorCommType !== "Fix" ? "%" : " Rs"}</TableCell>
                                <TableCell>{item.SuperDistributorCommissionCategory === "Surcharge" ? "-" : "+"}{item.SuperDistributorComm}{item.SuperDistributorCommType !== "Fix" ? "%" : " Rs"}</TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    );
}

export default SlabCommisuionDetails;
