import React from 'react';
import InboxIcon from '@material-ui/icons/Inbox';
import DashboardIcon from '@material-ui/icons/Dashboard';
import MoneyIcon from '@material-ui/icons/Money';
import ReportIcon from '@material-ui/icons/Report';
import PeopleIcon from '@material-ui/icons/People';
import PersonAddIcon from '@material-ui/icons/PersonAdd';
import ExitToAppIcon from '@material-ui/icons/ExitToApp';
import LocationOnIcon from '@material-ui/icons/LocationOn';
import AddLocationIcon from '@material-ui/icons/AddLocation';
import PinDropIcon from '@material-ui/icons/PinDrop';
import LiveTvIcon from '@material-ui/icons/LiveTv';
import AddToQueueIcon from '@material-ui/icons/AddToQueue';
import MobileFriendlyIcon from '@material-ui/icons/MobileFriendly';
import PeopleAltIcon from '@material-ui/icons/PeopleAlt';
import AssignmentIndIcon from '@material-ui/icons/AssignmentInd';
import MonetizationOn from '@material-ui/icons/MonetizationOn';
import SettingsInputComposite from '@material-ui/icons/SettingsInputComposite';
import TvIcon from '@material-ui/icons/Tv';
import PersonIcon from '@material-ui/icons/Person';
import ListAltIcon from '@material-ui/icons/ListAlt';
import PaymentIcon from '@material-ui/icons/Payment';
import ChatIcon from '@material-ui/icons/Chat';
import ChatBubbleOutlineIcon from '@material-ui/icons/ChatBubbleOutline';
import FaceIcon from '@material-ui/icons/Face';
import SettingsApplicationsIcon from '@material-ui/icons/SettingsApplications';
import Search from '@material-ui/icons/Search';
import AddCircleIcon from '@material-ui/icons/AddCircle';
import ViewListIcon from '@material-ui/icons/ViewList';
import BusinessIcon from '@material-ui/icons/Business';
import DockIcon from '@material-ui/icons/Dock';
import LocalAtmIcon from '@material-ui/icons/LocalAtm';
import AssessmentIcon from '@material-ui/icons/Assessment';
import AccountBalanceWalletIcon from '@material-ui/icons/AccountBalanceWallet';
import AttachMoneyIcon from '@material-ui/icons/AttachMoney';
import FingerprintIcon from '@material-ui/icons/Fingerprint';
import EditIcon from '@material-ui/icons/Edit';
import PrintIcon from '@material-ui/icons/Print';
//import DeleteIcon from '@material-ui/icons/Delete';
import DeleteIcon from 'Components/SvgButtons/DeleteIcon';
import Pets from '@material-ui/icons/Pets';
import DoneAllIcon from '@material-ui/icons/DoneAll';
import CheckCircleIcon from '@material-ui/icons/CheckCircle';
import LibraryAdd from '@material-ui/icons/LibraryAdd';
import PhoneAndroidIcon from '@material-ui/icons/PhoneAndroid';
import PhoneIphoneIcon from '@material-ui/icons/PhoneIphone';
import HighlightOffIcon from '@material-ui/icons/HighlightOff';
import VpnKey from '@material-ui/icons/VpnKey';
import Message from '@material-ui/icons/Message';
import Settings from '@material-ui/icons/Settings';
import AccountBalance from '@material-ui/icons/AccountBalance';
import Build from '@material-ui/icons/Build';
import Call from '@material-ui/icons/Call';
import ImportContacts from '@material-ui/icons/ImportContacts';
import ReceiptIcon from '@material-ui/icons/Receipt';

import BalanceButton from './SvgButtons/BalanceButton';
import SettingButton from 'Components/SvgButtons/SettingButton';

const iconStyle = { fontSize: '14', margin: '0 4px' };

export const CustomIcon = (props) => {
    const getIcon = (param) => {
        switch (param) {
            case 'ImportContacts':
                return <ImportContacts style={iconStyle} />;

            case 'ReceiptIcon':
                return <ReceiptIcon style={iconStyle} />;

            case 'Pets':
                return <Pets style={iconStyle} />;

            case 'Call':
                return <Call style={iconStyle} />;

            case 'HighlightOffIcon':
                return <HighlightOffIcon style={iconStyle} />;

            case 'Build':
                return <Build style={iconStyle} />;

            case 'VpnKey':
                return <VpnKey style={iconStyle} />;

            case 'AccountBalance':
                return <AccountBalance style={iconStyle} />;

            case 'Message':
                return <Message style={iconStyle} />;

            case 'Settings':
                return <Settings style={iconStyle} />;

            case 'PhoneAndroidIcon':
                return <PhoneAndroidIcon style={iconStyle} />;

            case 'PhoneIphoneIcon':
                return <PhoneIphoneIcon style={iconStyle} />;

            case 'DoneAllIcon':
                return <DoneAllIcon style={iconStyle} />;

            case 'LibraryAdd':
                return <LibraryAdd style={iconStyle} />;

            case 'CheckCircleIcon':
                return <CheckCircleIcon style={iconStyle} />;

            case 'EditIcon':
                return <EditIcon style={iconStyle} />;

            case 'PrintIcon':
                return <PrintIcon style={iconStyle} />;

            case 'DeleteIcon':
                return <DeleteIcon style={iconStyle} />;

            case 'AttachMoneyIcon':
                return <AttachMoneyIcon style={iconStyle} />;

            case 'DashboardIcon':
                return <DashboardIcon style={iconStyle} />;

            case 'AccountBalanceWalletIcon':
                return <AccountBalanceWalletIcon style={iconStyle} />;

            case 'BusinessIcon':
                return <BusinessIcon style={iconStyle} />;

            case 'AssessmentIcon':
                return <AssessmentIcon style={iconStyle} />;

            case 'LocalAtmIcon':
                return <LocalAtmIcon style={iconStyle} />;

            case 'DockIcon':
                return <DockIcon style={iconStyle} />;

            case 'ViewListIcon':
                return <ViewListIcon style={iconStyle} />;

            case 'AddCircleIcon':
                return <AddCircleIcon style={iconStyle} />;

            case 'SettingsApplicationsIcon':
                return <SettingsApplicationsIcon style={iconStyle} />;

            case 'Search':
                return <Search style={iconStyle} />;
            case 'MoneyIcon':
                return <MoneyIcon style={iconStyle} />;

            case 'ReportIcon':
                return <ReportIcon style={iconStyle} />;

            case 'PeopleIcon':
                return <PeopleIcon style={iconStyle} />;

            case 'PeopleAltIcon':
                return <PeopleAltIcon style={iconStyle} />;
            case 'PaymentIcon':
                return <PaymentIcon style={iconStyle} />;
            case 'PersonAddIcon':
                return <PersonAddIcon style={iconStyle} />;
            case 'FaceIcon':
                return <FaceIcon style={iconStyle} />;
            case 'AssignmentIndIcon':
                return <AssignmentIndIcon style={iconStyle} />;
            case 'PersonIcon':
                return <PersonIcon style={iconStyle} />;
            case 'ListAltIcon':
                return <ListAltIcon style={iconStyle} />;
            case 'ExitToAppIcon':
                return <ExitToAppIcon style={iconStyle} />;
            case 'LocationOnIcon':
                return <LocationOnIcon style={iconStyle} />;
            case 'PinDropIcon':
                return <PinDropIcon style={iconStyle} />;
            case 'AddLocationIcon':
                return <AddLocationIcon style={iconStyle} />;
            case 'LiveTvIcon':
                return <LiveTvIcon style={iconStyle} />;
            case 'AddToQueueIcon':
                return <AddToQueueIcon style={iconStyle} />;
            case 'TvIcon':
                return <TvIcon style={iconStyle} />;
            case 'ChatIcon':
                return <ChatIcon style={iconStyle} />;
            case 'ChatBubbleOutlineIcon':
                return <ChatBubbleOutlineIcon style={iconStyle} />;

            case 'MobileFriendlyIcon':
                return <MobileFriendlyIcon style={iconStyle} />;

            case 'SettingsInputComposite':
                return <SettingsInputComposite style={iconStyle} />;

            case 'MonetizationOn':
                return <MonetizationOn style={iconStyle} />;

            case 'BalanceButton':
                return <BalanceButton />;

            case 'SettingButton':
                return <SettingButton />;

            case 'FingerprintIcon':
                return <FingerprintIcon style={iconStyle} />;
            default:
                return <InboxIcon style={iconStyle} />;
        }
    };

    return <>{getIcon(props.icon)}</>;
};
