import React from 'react';
import { MenuItem } from '@material-ui/core';

const TopRecords = [10, 100, 500, 1000, 5000];
const StatusList = ['ALL', 'SUCCESS', 'PROCESS', 'FAILURE'];
const DMTStatusList = ['ALL', 'SUCCESS', 'PROCESS', 'FAILURE', 'REFUNDED'];
const ExportTypeList = ['Select Download', 'EXCEL', 'PDF'];
const StatusLists = ['ALL', 'SUCCESS', 'FAILURE'];
const ComplaintList = ['ALL', 'ACCEPT', 'REJECT', 'PROCESS'];
const PGModeList = ['ALL', 'Debit_card', 'credit_card', 'Net Banking', 'UPI'];
const RecoveredItemList = ['Yes', 'No'];

const RangeList = [
    'ALL',
    '1-1',
    '2-1000',
    '1001-2000',
    '2001-3000',
    '3001-4000',
    '4001-5000',
    '5001-100000'
];

const RechargeRangeList = [
    'ALL',
    '10-50',
    '51-100',
    '101-200',
    '201-300',
    '301-500',
    '501-700',
    '701-1000',
    '1001-2000',
    '2001-10000'
];
const StateList = [
    { Key: 1, Value: 'ANDAMAN AND NICOBAR ISLANDS' },
    { Key: 2, Value: 'ANDHRA PRADESH' },
    { Key: 3, Value: 'ARUNACHAL PRADESH' },
    { Key: 4, Value: 'ASSAM' },
    { Key: 5, Value: 'BIHAR' },
    { Key: 6, Value: 'CHANDIGARH' },
    { Key: 7, Value: 'DADRA AND NAGAR HAVELI' },
    { Key: 8, Value: 'DAMAN AND DIU' },
    { Key: 9, Value: 'DELHI' },
    { Key: 10, Value: 'GOA' },
    { Key: 11, Value: 'GUJARAT' },
    { Key: 12, Value: 'HARYANA' },
    { Key: 13, Value: 'HIMACHAL PRADESH' },
    { Key: 14, Value: 'JAMMU AND KASHMIR' },
    { Key: 15, Value: 'KARNATAKA' },
    { Key: 16, Value: 'KERALA' },
    { Key: 17, Value: 'LAKSHADWEEP' },
    { Key: 18, Value: 'MADHYA PRADESH' },
    { Key: 19, Value: 'MAHARASHTRA' },
    { Key: 20, Value: 'MANIPUR' },
    { Key: 21, Value: 'MEGHALAYA' },
    { Key: 22, Value: 'MIZORAM' },
    { Key: 23, Value: 'NAGALAND' },
    { Key: 24, Value: 'ODISHA' },
    { Key: 25, Value: 'PONDICHERRY' },
    { Key: 26, Value: 'PUNJAB' },
    { Key: 27, Value: 'RAJASTHAN PRADESH' },
    { Key: 28, Value: 'SIKKIM' },
    { Key: 29, Value: 'TAMILNADU' },
    { Key: 30, Value: 'TRIPURA' },
    { Key: 31, Value: 'UTTAR PRADESH' },
    { Key: 32, Value: 'WEST BENGAL' },
    { Key: 33, Value: 'CHHATTISGARH' },
    { Key: 34, Value: 'UTTARAKHAND' },
    { Key: 35, Value: 'JHARKHAND' },
    { Key: 36, Value: 'TELANGANA' },
    { Key: 99, Value: 'OTHER' }
];

const MonthList = [
    { Key: 1, Value: 'January' },
    { Key: 2, Value: 'February' },
    { Key: 3, Value: 'March' },
    { Key: 4, Value: 'April' },
    { Key: 5, Value: 'May' },
    { Key: 6, Value: 'June' },
    { Key: 7, Value: 'July' },
    { Key: 8, Value: 'August' },
    { Key: 9, Value: 'September' },
    { Key: 10, Value: 'October' },
    { Key: 11, Value: 'November' },
    { Key: 12, Value: 'December' }
];

const CardNetworkList = ['VISA', 'MASTERCARD', 'RUPAY'];
export const CardNetworkItems = CardNetworkList.map((item) => {
    return (
        <MenuItem key={item} value={item}>
            {item}
        </MenuItem>
    );
});

export const StatusItem = StatusLists.map((item) => {
    return (
        <MenuItem key={item} value={item}>
            {item}
        </MenuItem>
    );
});

export const StatusItems = StatusList.map((item) => {
    return (
        <MenuItem key={item} value={item}>
            {item}
        </MenuItem>
    );
});

export const DMTStatusItems = DMTStatusList.map((item) => {
    return (
        <MenuItem key={item} value={item}>
            {item}
        </MenuItem>
    );
});

export const RecoveredItems = RecoveredItemList.map((item) => {
    return (
        <MenuItem key={item} value={item}>
            {item}
        </MenuItem>
    );
});

export const ExportTypeItems = ExportTypeList.map((item) => {
    return (
        <MenuItem key={item} value={item}>
            {item}
        </MenuItem>
    );
});
export const PGModeItems = PGModeList.map((item) => {
    return (
        <MenuItem key={item} value={item}>
            {item}
        </MenuItem>
    );
});
export const ComplaintItems = ComplaintList.map((item) => {
    return (
        <MenuItem key={item} value={item}>
            {item}
        </MenuItem>
    );
});

export const TopRecordItems = TopRecords.map((item) => {
    return (
        <MenuItem key={item} value={item}>
            {item}
        </MenuItem>
    );
});

export const RangeItems = RangeList.map((item) => {
    return (
        <MenuItem key={item} value={item}>
            {item}
        </MenuItem>
    );
});
export const RechargeRangeItems = RechargeRangeList.map((item) => {
    return (
        <MenuItem key={item} value={item}>
            {item}
        </MenuItem>
    );
});
export const StateItem = StateList.map(({ Key, Value }, index) => {
    return (
        <MenuItem key={index} value={Key}>
            {Value}
        </MenuItem>
    );
});

export const MonthItem = MonthList.map(({ Key, Value }, index) => {
    return (
        <MenuItem key={index} value={Key}>
            {Value}
        </MenuItem>
    );
});
