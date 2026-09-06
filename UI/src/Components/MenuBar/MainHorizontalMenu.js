import React, { useState } from 'react';
import menuItems from './menuItems';
import { NavLink } from 'react-router-dom';
import { withStyles } from '@material-ui/core/styles';
import { APP_BAR_COLOR, MENU_FORE_COLOR } from 'Config';
import Logo from 'Components/Logo/Logo';
import LayoutStyles from 'Components/LayoutComponents/LayoutStyle';
import { SUPPORT_USER } from 'Components/Helper/UsertypeCode';
import {
    ADMIN_CODE,
    DIST_CODE,
    SUPERDIST_CODE,
    SUPPORT_CODE
} from 'Components/Helper/Messages';

const styles = (theme) => ({
    root: {
        '& .dropdown-menu': {
            borderRadius: '0px',
            border: '0px',
            margin: '0px',
            background: APP_BAR_COLOR,
            color: MENU_FORE_COLOR() + ' !important',
            boxShadow: '0px 3px 3px 1px ' + MENU_FORE_COLOR()
        },
        '& .dropdown-item': {
            height: '32px',
            color: MENU_FORE_COLOR() + ' !important'
        },
        '& .dropdown-item:focus, & .dropdown-item:hover': {
            background: MENU_FORE_COLOR(),
            color: APP_BAR_COLOR() + ' !important'
        },
        '& .dropdown-item.active, & .dropdown-item:active': {
            background: MENU_FORE_COLOR(),
            color: APP_BAR_COLOR() + ' !important'
        },
        '& .navbar': {
            padding: '0px'
        },
        '& .navbar-nav': {
            color: MENU_FORE_COLOR() + ' !important',
            fontSize: '16px'
        },
        '& .navbar-light, & .nav-link': {
            padding: '20px 8px 18px 8px',
            color: MENU_FORE_COLOR() + ' !important',
            fontSize: '16px',
            borderBottom: '3px solid transparent'
        },
        '.nav-link:focus, & .nav-link:hover, & .nav-link.active': {
            borderBottom: '3px solid ' + MENU_FORE_COLOR()
        }
    }
});

const MainHorizontalMenu = (props) => {
    let { classes } = props;
    const classesLayout = LayoutStyles();
    var IsCashCollection = localStorage.getItem('IsCashCollection') === 'true';
    var IsTargetOnSale = localStorage.getItem('IsTargetOnSale') === 'yes';
    var IsOverDraft = localStorage.getItem('IsOverDraft') === 'yes';
    var IsW2OFundTransfer = localStorage.getItem('IsW2OFundTransfer') === 'yes';
    var IsWhiteLabel = localStorage.getItem('IsWhiteLabel') === 'yes';
    var IsPendingProvider = localStorage.getItem('IsPendingProvider') === 'yes';
    var PlanApiForAPIUser = localStorage.getItem('PlanApiForAPIUser') === 'yes';
    var IsFintechLeadGen = localStorage.getItem('IsFintechLeadGeneration') === 'yes';

    var IsCreditInstitution =
        localStorage.getItem('IsCreditInstitution') === 'yes';


    var FundReversalUpline =
        localStorage.getItem('FundReversalUpline') === 'Yes';

    var currentUsertypeid = parseInt(localStorage.getItem('UserTypeID'));

    var IsChannel = localStorage.getItem('IsChannel') === 'true';

    var IsGSTHold = localStorage.getItem('IsGSTHold') === 'true';
    var IsSignupReferral = localStorage.getItem('IsSignupReferral') === 'true';

    var IsProviderPerformanceGraph = localStorage.getItem('IsProviderPerformanceGraph') === 'true';
    var IsVoucher = localStorage.getItem('Voucher') === 'true';

    var IsServicewiseSubscription =
        localStorage.getItem('ServicewiseSubscription') === 'true';
    var CommissionHold = localStorage.getItem('CommissionHold') === 'true';

    var AutoBalanceTransfer =
        localStorage.getItem('AutoBalanceTransfer') === 'true';
    var IsCashFreeAiUser = localStorage.getItem('IsCashFreeAiUser') === 'true';


    const [MenuJson, setMenuJson] = useState([
        JSON.parse(localStorage.getItem('MenuResponse'))
    ]);

    const isSubMenuVisible = (subOption) => {
        if (
            subOption.name === 'CashCollection Add' &&
            IsCashCollection === false
        )
            return false;
        if (
            subOption.name === 'CashCollection Reverse' &&
            IsCashCollection === false
        )
            return false;
        if (subOption.name === 'Cash Collection' && IsCashCollection === false)
            return false;

        if (subOption.name === 'Sale Target Report' && IsTargetOnSale === false)
            return false;

        if (
            subOption.name === 'Sale Target Setting' &&
            IsTargetOnSale === false
        )
            return false;

        if (subOption.name === 'CashReverse' && IsCashCollection === false)
            return false;

        if (subOption.name === 'Channel Add' && IsChannel === false)
            return false;

        if (subOption.name === 'Channel Assign' && IsChannel === false)
            return false;

        if (subOption.name === 'Overdraft Request' && IsOverDraft === false)
            return false;

        if (
            subOption.name === 'Pending Overdraft Request' &&
            IsOverDraft === false
        )
            return false;

        if (subOption.name === 'Reverse Overdraft' && IsOverDraft === false)
            return false;

        if (subOption.name === 'Transfer Overdraft' && IsOverDraft === false)
            return false;
        if (subOption.name === 'Overdraft Report' && IsOverDraft === false)
            return false;
        if (subOption.name === 'Overdraft Setting' && IsOverDraft === false)
            return false;

        if (
            subOption.name === 'W2O Fund Transfer' &&
            IsW2OFundTransfer === false
        )
            return false;
        if (subOption.name === 'API' && IsWhiteLabel === true) return false;
        if (subOption.name === 'Service' && IsWhiteLabel === true) return false;
        if (
            subOption.name === 'By Pending Provider' &&
            IsPendingProvider === false
        )
            return false;

        if (subOption.name === 'GST Hold' && IsGSTHold === false) return false;
        if (subOption.name === 'User Referral' && IsSignupReferral === false)
            return false;

        if (
            subOption.name === 'Provider Wise Performance Graph' &&
            IsProviderPerformanceGraph === false
        )
            return false;
        if (subOption.name === 'Voucher' && IsVoucher === false) return false;

        if (subOption.name === 'Commission Hold' && CommissionHold === false)
            return false;

        if (subOption.name === 'Plan API' && PlanApiForAPIUser === false)
            return false;

        if (subOption.name === 'Plan View Report' && PlanApiForAPIUser === false)
            return false;

        if (
            subOption.name === 'Service Subscription' &&
            IsServicewiseSubscription === false
        )
            return false;
        if (
            subOption.name === 'Subscription Charge' &&
            IsServicewiseSubscription === false
        )
            return false;
        if (
            subOption.name === 'Subscription Payment' &&
            IsServicewiseSubscription === false
        )
            return false;

        if (
            subOption.name === 'Auto Balance Transfer' &&
            AutoBalanceTransfer === false
        )
            return false;

        if (
            subOption.name === 'Commission Hold Request' &&
            CommissionHold === false
        )
            return false;

        if (subOption.name === 'Plan API' && PlanApiForAPIUser === false)
            return false;

        if (IsCashFreeAiUser === true) {
            if (subOption.name === 'DMT') return false;
            if (subOption.name === 'AEPS') return false;
            if (subOption.name === 'Payout') return false;
            if (subOption.name === 'UPITransfer') return false;
            if (subOption.name === 'Cash Deposit') return false;
            if (subOption.name === 'CMS') return false;
            if (subOption.name === 'Account Opening') return false;
            if (subOption.name === 'CreditCard To Bank') return false;
            if (subOption.name === 'DMT Report') return false;
            if (subOption.name === 'AEPS Report') return false;
            if (subOption.name === 'PayOut Report') return false;
            if (subOption.name === 'VendorPay Report') return false;
            if (subOption.name === 'MicroATM Report') return false;
            if (subOption.name === 'POS Report') return false;
            if (subOption.name === 'CMS Report') return false;
            if (subOption.name === 'UPITransfer Report') return false;
            if (subOption.name === 'Account Opening Report') return false;
            if (subOption.name === 'Cash Deposit Report') return false;
            if (subOption.name === 'Account Statement') return false;
        }


        if (subOption.name === 'Fintech Lead Report' && IsFintechLeadGen === false)
            return false;

        if (IsCreditInstitution === true) {
            if (subOption.name === 'Fund Report') return false;
            if (subOption.name === 'Demat Report') return false;
            if (subOption.name === 'Fund') return false;
            if (subOption.name === 'User Creation Setting') return false;
            if (subOption.name === 'Free ID Transfer') return false;
            if (subOption.name === 'IDPurchase Report') return false;
            if (subOption.name === 'XMPP') return false;
            if (subOption.name === 'Reconciliation') return false;
            if (subOption.name === 'Reconciliation Upload') return false;
            if (subOption.name === 'Provider Purchase') return false;
            if (subOption.name === 'Lapu') return false;
            if (subOption.name === 'Demat Setting') return false;
            if (subOption.name === 'Offline Product') return false;
            if (subOption.name === 'Dth Category-Channel') return false;
            if (subOption.name === 'Dth Package-Channel') return false;
        }
        if (
            subOption.name === 'Bank Account Statement' &&
            IsCreditInstitution === false
        )
            return false;

        if (
            currentUsertypeid === DIST_CODE ||
            currentUsertypeid === SUPERDIST_CODE
        )
            if (
                subOption.name === 'Fund Reverse' &&
                FundReversalUpline === false
            )
                return false;

        return true;
    };

    const isMenuVisible = (subOption) => {
        var arr = subOption.usertypeid;

        var isVisible = false;

        if (arr === undefined) {
            isVisible = isSubMenuVisible(subOption);

            return isVisible;
        }

        arr.map((arr) => {
            if (arr === -99 || arr === currentUsertypeid)
                isVisible = isSubMenuVisible(subOption);
        });

        return isVisible;
    };

    const renderLevels = (children, isSubMenu) => {
        return children.map((subOption, index) => {
            var isVisible = isMenuVisible(subOption);

            if (!isVisible) return null;

            return !subOption.children || subOption.children.length <= 0 ? (
                <li
                    data-target="#sidenav-collapse-main"
                    data-toggle="collapse"
                    className={isSubMenu ? 'nav-item dropdown' : 'nav-item'}
                    key={index}
                >
                    <NavLink
                        data-toggle="collapse"
                        data-target=".navbar-collapse.show"
                        to={subOption.url ? subOption.url : '/#'}
                        className={isSubMenu ? 'dropdown-item' : 'nav-link'}
                    >
                        {subOption.name}
                    </NavLink>
                </li>
            ) : (
                <li key={index} className="nav-item dropdown">
                    <a
                        className="nav-link dropdown-toggle"
                        href="/#"
                        data-toggle="dropdown"
                    >
                        {subOption.name}
                    </a>
                    <ul className="dropdown-menu">
                        {renderLevels(subOption.children, true)}
                    </ul>
                </li>
            );
        });
    };

    return (
        <nav
            className={`navbar navbar-expand-lg navbar-light bg-light mainmenu ${classes.root}`}
        >
            <div className={(classesLayout.title, classesLayout.innerlogo)}>
                <Logo />
            </div>
            <button
                className={`navbar-toggler ${classesLayout.navbarTogglerColor}`}
                type="button"
                data-toggle="collapse"
                data-target="#navbarSupportedContent"
                aria-controls="navbarSupportedContent"
                aria-expanded="false"
                aria-label="Toggle navigation"
            >
                <span>
                    <i className="fad fa-bars" aria-hidden="true"></i>
                </span>
            </button>
            <div
                className="collapse navbar-collapse"
                id="navbarSupportedContent"
            >
                <ul className="navbar-nav ml-auto">
                    {currentUsertypeid === SUPPORT_USER
                        ? renderLevels(MenuJson[0].data, false)
                        : renderLevels(menuItems.data, false)}
                </ul>
            </div>
        </nav>
    );
};

export default withStyles(styles, { withTheme: true })(MainHorizontalMenu);
