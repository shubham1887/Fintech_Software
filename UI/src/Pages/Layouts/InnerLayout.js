import {
    AppBar,
    Avatar,
    Badge,
    CssBaseline,
    Grid,
    IconButton,
    Link,
    Menu,
    MenuItem,
    Toolbar,
    Typography,
    Box
} from '@material-ui/core';

import MenuIcon from '@material-ui/icons/Menu';
import MoreIcon from '@material-ui/icons/MoreVert';
import NotificationsIcon from '@material-ui/icons/Notifications';
import CurrencyRupeeIcon from '@mui/icons-material/CurrencyRupee';
import AddmoneyAppBar from 'Components/DashboardIcon/AddmoneyAppBar';
import ReferAndEarnAppBar from 'Components/DashboardIcon/ReferAndEarnAppBar';

import { usePageVisibility } from 'BrowserVisibility';
import clsx from 'clsx';
import { SelectByUserID as NEWS_SELECT_BY_USERID } from 'Components/Api/NewsAPI';

import { BalanceContext } from 'Components/BalanceContext';
import { textToSpeech } from 'Components/Helper/Helpers';
import { REFRESH_BALANCE_FLAG } from 'Components/Helper/Messages';

import {
    ADMIN,
    RETAILER,
    CLIENT_CODE,
    SUPPORT_USER,
    SUPER_DISTRIBUTOR,
    DISTRIBUTOR,
    API_USER
} from 'Components/Helper/UsertypeCode';

import CustomBreadcrumbs from 'Components/LayoutComponents/CustomBreadcrumbs';
import LayoutStyles from 'Components/LayoutComponents/LayoutStyle';
import NotificationDisplay from 'Components/LayoutComponents/NotificationDisplay';

import WalletsDisplay from 'Components/LayoutComponents/WalletsDisplay';
import NewsMarquee from 'Components/LayoutComponents/NewsMarquee';

import MainHorizontalMenu from 'Components/MenuBar/MainHorizontalMenu';
import ShortCutMenu from 'Components/MenuBar/ShortCutMenu';
import { CLIENT_NAME, MENU_FORE_COLOR } from 'Config';
import { FORCELOGOUT } from 'ConstComponents';
import initFireBase from 'init-fcm';

import React, { useContext, useEffect, useState, lazy } from 'react';
import { useHistory } from 'react-router-dom';
import CircularProgressWithLabel from 'Components/LayoutComponents/CircularProgressWithLabel';
import WhatsAppFAB from 'Components/Footer/WhatsAppFAB';
import 'react-toastify/dist/ReactToastify.css';
import { toast } from 'react-toastify';
const FCMAudio = lazy(() => import('Assets/fcm_audio.mp3'));

const InnerLayout = (props) => {
                                   const {
                                       ProfileImagePath,
                                       NotificationCount,
                                       Wallets,
                                       DashboardBalanceUpdate,
                                       SubscriptionDays,
                                       AppbarRefresh,
                                       AppbarRefreshSD
                                   } = useContext(BalanceContext);

                                   const [anchorEl, setAnchorEl] = useState(
                                       null
                                   );
                                   const isMenuOpen = Boolean(anchorEl);

                                   const [
                                       anchorAccount,
                                       setAnchorAccount
                                   ] = useState(null);

                                   const [firmName] = useState(
                                       localStorage.getItem('FirmName')
                                   );

                                   const [mobileNumber] = useState(
                                       localStorage.getItem('MobileNumber')
                                   );
                                   const [userType] = useState(
                                       localStorage.getItem('UserType')
                                   );
                                   const [userTypeID] = useState(
                                       parseInt(
                                           localStorage.getItem('UserTypeID')
                                       )
                                   );
                                   const [UserAvaterImagePath] = useState(
                                       localStorage.getItem(
                                           'ProfileImagePath'
                                       ) || ProfileImagePath
                                   );
                                   const [RemainingSubscriptionDays] = useState(
                                       localStorage.getItem(
                                           'SubscriptionDays'
                                       ) || SubscriptionDays
                                   );
                                   const [CommissionHold] = useState(
                                       localStorage.getItem('CommissionHold')
                                   );

                                   const classes = LayoutStyles();

                                   const [open, setOpen] = useState(true);
                                   const history = useHistory();

                                   const [NewsCount, setNewsCount] = useState(
                                       parseInt(
                                           localStorage.getItem('NewsCount')
                                       ) || NotificationCount
                                   );

                                   const [
                                       FundRequesCount,
                                       setFundRequesCount
                                   ] = useState(
                                       localStorage.getItem(
                                           'FundRequesCount'
                                       ) || 0
                                   );

                                   const [
                                       isAdminRole,
                                       setIsAdminRole
                                   ] = useState(false);
                                   const [
                                       isSupportUser,
                                       setIsSupportUser
                                   ] = useState(false);
                                   const [
                                       isClientAndAdminRole,
                                       setIsClientAndAdminRole
                                   ] = useState(false);
                                   const [
                                       isApiUserRole,
                                       setIsApiUserRole
                                   ] = useState(false);
                                   const [
                                       isRetailerRole,
                                       setIsRetailerRole
                                   ] = useState(false);
                                   const [
                                       UserWallets,
                                       setUserWallets
                                   ] = useState(Wallets);
                                   const [
                                       isSDAndDistRole,
                                       setSDAndDistRole
                                   ] = useState(false);
                                   const [
                                       isCommHold,
                                       setCommissionHold
                                   ] = useState(false);
                                   async function fetchCName() {
                                       document.title = await CLIENT_NAME();
                                   }

                                   const visibilityNoneForSmallScreen = {
                                       xs: 'block',
                                       sm: 'block',
                                       md: 'block',
                                       lg: 'block',
                                       xl: 'block'
                                   };

                                   const [
                                       WhatsappURL,
                                       setWhatsappURL
                                   ] = useState(
                                       localStorage.getItem('WhatsappURL')
                                   );

                                   const isVisible = usePageVisibility();

                                   useEffect(() => {
                                       if (isVisible === true) {
                                           DashboardBalanceUpdate();
                                           if (
                                               userTypeID === ADMIN ||
                                               userTypeID === SUPPORT_USER
                                           )
                                               AppbarRefresh();

                                           if (
                                               userTypeID ===
                                                   SUPER_DISTRIBUTOR ||
                                               userTypeID === DISTRIBUTOR
                                           )
                                               AppbarRefreshSD();
                                       }
                                   }, [isVisible]);

                                   useEffect(() => {
                                       fetchCName();
                                       initFireBase();
                                   }, []);

                                   useEffect(() => {
                                       setUserWallets(
                                           Wallets.length >= 1
                                               ? JSON.parse(Wallets)
                                               : JSON.parse(
                                                     localStorage.getItem(
                                                         'Wallets'
                                                     )
                                                 )
                                       );
                                   }, [Wallets]);

                                   const snackBarOptions = {
                                       anchorOrigin: {
                                           horizontal: 'right',
                                           vertical: 'bottom'
                                       }
                                   };
                                   const CheckUserRole = () => {
                                       if (ADMIN !== parseInt(userTypeID)) {
                                           if (CommissionHold === 'true')
                                               setCommissionHold(true);
                                       }

                                       if (ADMIN === parseInt(userTypeID))
                                           setIsAdminRole(true);
                                       if (
                                           SUPPORT_USER === parseInt(userTypeID)
                                       )
                                           setIsSupportUser(true);
                                       if (RETAILER === parseInt(userTypeID))
                                           setIsRetailerRole(true);
                                       if (ADMIN === parseInt(userTypeID))
                                           setIsClientAndAdminRole(true);
                                       if (API_USER === parseInt(userTypeID))
                                           setIsApiUserRole(true);

                                       if (CLIENT_CODE === parseInt(userTypeID))
                                           setIsClientAndAdminRole(true);

                                       if (
                                           SUPER_DISTRIBUTOR ===
                                           parseInt(userTypeID)
                                       )
                                           setSDAndDistRole(true);
                                       if (DISTRIBUTOR === parseInt(userTypeID))
                                           setSDAndDistRole(true);
                                   };
                                   const includes = (container, value) => {
                                       var returnValue = false;
                                       var pos = container.indexOf(value);
                                       if (pos >= 0) returnValue = true;

                                       return returnValue;
                                   };

                                   const fcmNotificationMessageProcess = (
                                       messageEvent
                                   ) => {
                                       const {
                                           data,
                                           notification
                                       } = messageEvent;

                                       let autoHideDuration =
                                           data[
                                               'gcm.notification.autoHideDuration'
                                           ];
                                       let variant =
                                           data['gcm.notification.variant'];

                                       let body = '';
                                       let icon = '';
                                       let title = '';

                                       if (notification === undefined) {
                                           body = data.body;
                                           autoHideDuration =
                                               data.autoHideDuration;
                                           title = data.title;
                                           icon = data.icon;
                                           variant = data.variant;
                                       } else {
                                           body = notification.body;
                                           autoHideDuration =
                                               notification.autoHideDuration;
                                           title = notification.title;
                                           icon = notification.icon;
                                           variant = notification.variant;
                                       }

                                       if (
                                           body === undefined ||
                                           body === null ||
                                           body === ''
                                       )
                                           return;

                                       DashboardBalanceUpdate();

                                       if (
                                           includes(title, REFRESH_BALANCE_FLAG)
                                       ) {
                                           if (
                                               userTypeID === ADMIN ||
                                               userTypeID === SUPPORT_USER
                                           )
                                               AppbarRefresh();
                                           return;
                                       }

                                       textToSpeech(body);

                                       if (variant === 'error') {
                                           toast.error(body, {
                                               position: 'bottom-right',
                                               pauseOnHover: true,
                                               draggable: false,
                                               progress: undefined
                                           });
                                       }
                                       if (variant === 'success') {
                                           toast.success(body, {
                                               position: 'bottom-right',
                                               pauseOnHover: true,
                                               draggable: false,
                                               progress: undefined
                                           });
                                       }
                                       new Audio(FCMAudio).play();
                                       return;
                                   };

                                   useEffect(() => {
                                       try {
                                           CheckUserRole();

                                           navigator.serviceWorker.addEventListener(
                                               'message',
                                               (messageEvent) => {
                                                   fcmNotificationMessageProcess(
                                                       messageEvent.data
                                                   );
                                               }
                                           );

                                           navigator.serviceWorker.addEventListener(
                                               'backgroundMessage',
                                               (messageEvent) => {
                                                   fcmNotificationMessageProcess(
                                                       messageEvent.data
                                                   );
                                               }
                                           );
                                       } catch (err) {}
                                   }, []);

                                   const handleLogoutButton = () => {
                                       FORCELOGOUT();
                                   };
                                   const [
                                       OpenNotificationPanel,
                                       setOpenNotificationPanel
                                   ] = useState(false);
                                   const [
                                       OpenCommissionHoldPanel,
                                       setOpenCommissionHoldPanel
                                   ] = useState(false);
                                   const GetNotifications = async () => {
                                       const response = await NEWS_SELECT_BY_USERID();

                                       if (!response.status) return;

                                       var data = JSON.parse(response.data);

                                       setNewsCount(data.length);
                                   };

                                   const handleDrawerOpen = () => {
                                       setOpen(true);
                                   };

                                   const handleProfileMenuOpen = (event) => {
                                       setAnchorEl(event.currentTarget);
                                   };
                                   const OpenSubscriptionRenovation = (
                                       event
                                   ) => {
                                       history.push('/SubscriptionRenew');
                                   };

                                   const handleNotificationPanel = () => {
                                       if (!OpenNotificationPanel)
                                           GetNotifications();
                                       setOpenNotificationPanel(
                                           !OpenNotificationPanel
                                       );
                                   };

                                   const handlCommissionHoldPanel = () => {
                                       setOpenCommissionHoldPanel(
                                           !OpenCommissionHoldPanel
                                       );
                                   };

                                   const handleMenuClose = () => {
                                       setAnchorEl(null);
                                   };
                                   const handleAccountMenuClose = () => {
                                       setAnchorAccount(null);
                                   };

                                   const menuId = 'primary-search-account-menu';

                                   const renderProfileSubMenu = (
                                       <Menu
                                           anchorEl={anchorEl}
                                           id={menuId}
                                           keepMounted
                                           getContentAnchorEl={null}
                                           anchorOrigin={{
                                               vertical: 'bottom',
                                               horizontal: 'center'
                                           }}
                                           transformOrigin={{
                                               vertical: 'top',
                                               horizontal: 'center'
                                           }}
                                           open={isMenuOpen}
                                           onClose={handleMenuClose}
                                       >
                                           <MenuItem>
                                               {firmName} - {mobileNumber}{' '}
                                               <br />
                                           </MenuItem>

                                           <MenuItem>{userType}</MenuItem>

                                           <MenuItem
                                               onClick={() => {
                                                   history.push(
                                                       '/UsersProfile'
                                                   );
                                                   handleMenuClose();
                                               }}
                                           >
                                               Profile
                                           </MenuItem>
                                           {isRetailerRole ? (
                                               <MenuItem
                                                   onClick={() => {
                                                       history.push(
                                                           '/Subscription'
                                                       );
                                                       handleMenuClose();
                                                   }}
                                               >
                                                   Subscription
                                               </MenuItem>
                                           ) : null}
                                           <MenuItem
                                               onClick={() => {
                                                   history.push('/TwoFA');
                                                   handleMenuClose();
                                               }}
                                           >
                                               2FA
                                           </MenuItem>
                                           {!isAdminRole ? (
                                               <MenuItem
                                                   onClick={() => {
                                                       handleMenuClose();
                                                       history.push(
                                                           '/YesbankeKYC'
                                                       );
                                                   }}
                                               >
                                                   Agent KYC Update
                                               </MenuItem>
                                           ) : null}
                                           <MenuItem
                                               onClick={() => {
                                                   history.push(
                                                       '/Users/ChangePassword'
                                                   );
                                                   handleMenuClose();
                                               }}
                                           >
                                               Change Password & Pin
                                           </MenuItem>

                                           <MenuItem
                                               onClick={() => {
                                                   history.push(
                                                       '/UserwiseLowBalance'
                                                   );
                                                   handleMenuClose();
                                               }}
                                           >
                                               Low Balance
                                           </MenuItem>

                                           {!isClientAndAdminRole ? (
                                               <MenuItem
                                                   onClick={() => {
                                                       history.push(
                                                           '/DigitalCertificateIndex'
                                                       );
                                                       handleMenuClose();
                                                   }}
                                               >
                                                   Digital Certificate
                                               </MenuItem>
                                           ) : null}
                                           <MenuItem
                                               onClick={handleLogoutButton}
                                           >
                                               <Link>Logout</Link>
                                           </MenuItem>
                                       </Menu>
                                   );

                                   const renderAccountSubMenu = (
                                       <Menu
                                           anchorEl={anchorAccount}
                                           anchorOrigin={{
                                               vertical: 'top',
                                               horizontal: 'right'
                                           }}
                                           getContentAnchorEl={null}
                                           id={menuId}
                                           keepMounted
                                           transformOrigin={{
                                               vertical: 'top',
                                               horizontal: 'right'
                                           }}
                                           open={Boolean(anchorAccount)}
                                           onClose={handleAccountMenuClose}
                                       >
                                           {UserWallets &&
                                               UserWallets.map(
                                                   (item, index) => (
                                                       <MenuItem
                                                           key={item.Name}
                                                           className={
                                                               index % 2
                                                                   ? classes.backColor
                                                                   : null
                                                           }
                                                       >
                                                           <Grid
                                                               container
                                                               spacing={2}
                                                           >
                                                               <Grid
                                                                   item
                                                                   xs={12}
                                                                   sm={12}
                                                                   md={6}
                                                                   lg={6}
                                                               >
                                                                   {item.Name} :
                                                               </Grid>
                                                               <Grid
                                                                   item
                                                                   xs={12}
                                                                   sm={12}
                                                                   md={6}
                                                                   lg={6}
                                                               >
                                                                   <Typography align="right">
                                                                       {
                                                                           item.Balance
                                                                       }
                                                                   </Typography>
                                                               </Grid>
                                                           </Grid>
                                                       </MenuItem>
                                                   )
                                               )}
                                       </Menu>
                                   );
                                   const mobileMenuId =
                                       'primary-search-account-menu-mobile';

                                   const handleFundRequestClick = () => {
                                       history.push('/Fund');
                                   };

                                   // const handleDMTResendClick = () => {
                                   //     history.push('/DMTResendWallet');
                                   // };

                                   return (
                                       <div className={classes.root}>
                                           <CssBaseline />
                                           <AppBar
                                               position="fixed"
                                               className={classes.appBar}
                                           >
                                               <Toolbar disableGutters={true}>
                                                   <IconButton
                                                       color="primary"
                                                       aria-label="open drawer"
                                                       onClick={
                                                           handleDrawerOpen
                                                       }
                                                       edge="start"
                                                       className={clsx(
                                                           classes.menuButton,
                                                           {
                                                               [classes.hide]: open
                                                           }
                                                       )}
                                                   >
                                                       <MenuIcon />
                                                   </IconButton>

                                                   <MainHorizontalMenu />

                                                   <div
                                                       className={
                                                           classes.sectionDesktop
                                                       }
                                                   >
                                                       {/* {isRetailerRole === true ? (
                            <IconButton
                                aria-label="show notifications"
                                color="inherit"
                                title="DMT Resend"
                                onClick={handleDMTResendClick}
                            >
                                <AccountBalanceWalletIcon />
                            </IconButton>
                        ) : null} */}

                                                       {isAdminRole ===
                                                       false ? (
                                                           <IconButton
                                                               aria-label="show notifications"
                                                               color="inherit"
                                                               title="News"
                                                               style={{
                                                                   color: MENU_FORE_COLOR()
                                                               }}
                                                               onClick={
                                                                   handleNotificationPanel
                                                               }
                                                           >
                                                               <Badge
                                                                   badgeContent={
                                                                       NewsCount
                                                                   }
                                                                   classes={{
                                                                       badge:
                                                                           classes.NewsBadge
                                                                   }}
                                                               >
                                                                   <NotificationsIcon />
                                                               </Badge>
                                                           </IconButton>
                                                       ) : null}

                                                       {isCommHold === true ? (
                                                           <IconButton
                                                               aria-label="show hold commisssion "
                                                               color="inherit"
                                                               title="Hold Commission"
                                                               style={{
                                                                   color: MENU_FORE_COLOR()
                                                               }}
                                                               onClick={
                                                                   handlCommissionHoldPanel
                                                               }
                                                           >
                                                               <Badge
                                                                   //  badgeContent={NewsCount}
                                                                   classes={{
                                                                       badge:
                                                                           classes.NewsBadge
                                                                   }}
                                                               >
                                                                   <CurrencyRupeeIcon />
                                                               </Badge>
                                                           </IconButton>
                                                       ) : null}

                                                       {isSDAndDistRole ===
                                                       true ? (
                                                           <IconButton
                                                               aria-label="Fund Request"
                                                               color="inherit"
                                                               title="Fund Request"
                                                               style={{
                                                                   color: MENU_FORE_COLOR()
                                                               }}
                                                               // onClick={() => {
                                                               //     history.push('/Fund');
                                                               //     handleMenuClose();
                                                               // }}
                                                               onClick={
                                                                   handleFundRequestClick
                                                               }
                                                           >
                                                               <Badge
                                                                   badgeContent={
                                                                       FundRequesCount
                                                                   }
                                                                   classes={{
                                                                       badge:
                                                                           classes.NewsBadge
                                                                   }}
                                                               >
                                                                   <CurrencyRupeeIcon />
                                                               </Badge>
                                                           </IconButton>
                                                       ) : null}
                                                       <IconButton>
                                                           {'   '}
                                                       </IconButton>

                                                       <IconButton
                                                           aria-controls={
                                                               menuId
                                                           }
                                                           onClick={
                                                               handleProfileMenuOpen
                                                           }
                                                           color="inherit"
                                                           title="Profile"
                                                       >
                                                           <Avatar
                                                               alt="Profile Image"
                                                               src={
                                                                   UserAvaterImagePath
                                                               }
                                                           />
                                                       </IconButton>
                                                       <IconButton>
                                                           {'  '}
                                                       </IconButton>
                                                   </div>

                                                   <div
                                                       className={
                                                           classes.sectionMobile
                                                       }
                                                   >
                                                       <IconButton
                                                           aria-label="show more"
                                                           aria-controls={
                                                               mobileMenuId
                                                           }
                                                           aria-haspopup="true"
                                                           style={{
                                                               color: MENU_FORE_COLOR()
                                                           }}
                                                           onClick={
                                                               handleProfileMenuOpen
                                                           }
                                                           color="inherit"
                                                       >
                                                           <MoreIcon />
                                                       </IconButton>
                                                   </div>
                                               </Toolbar>

                                               {isAdminRole || isSupportUser ? (
                                                   <ShortCutMenu />
                                               ) : null}
                                           </AppBar>

                                           {OpenNotificationPanel ? (
                                               <NotificationDisplay
                                                   IsOpen={
                                                       OpenNotificationPanel
                                                   }
                                                   handleNotificationPanel={
                                                       handleNotificationPanel
                                                   }
                                               />
                                           ) : null}

                                           {renderProfileSubMenu}
                                           {renderAccountSubMenu}

                                           <Grid
                                               className={classes.content}
                                               style={{
                                                   marginTop: isAdminRole
                                                       ? '85px'
                                                       : '48px'
                                               }}
                                           >
                                               <div className="flex flex-wrap">
                                                   <Box
                                                       component="div"
                                                       display={
                                                           visibilityNoneForSmallScreen
                                                       }
                                                   >
                                                       <CustomBreadcrumbs />
                                                   </Box>
                                                   <Box
                                                       component="div"
                                                       display={
                                                           visibilityNoneForSmallScreen
                                                       }
                                                   >
                                                       <WalletsDisplay
                                                           start={0}
                                                           end={10}
                                                       />
                                                   </Box>
                                               </div>

                                               <Box
                                                   component="div"
                                                   display={
                                                       visibilityNoneForSmallScreen
                                                   }
                                                   className="flex flex-wrap"
                                               >
                                                   <NewsMarquee />
                                               </Box>

                                               {props.children}

                                               <Box
                                                   component="span"
                                                   display={
                                                       visibilityNoneForSmallScreen
                                                   }
                                               >
                                                   <WhatsAppFAB
                                                       WhatsappURL={WhatsappURL}
                                                   />
                                               </Box>
                                           </Grid>
                                       </div>
                                   );
                               };

export default InnerLayout;
