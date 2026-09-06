import React, { useContext } from 'react';
import { BalanceContext } from 'Components/BalanceContext';
import { withStyles } from '@material-ui/core/styles';
import { NAV_BAR_COLOR } from 'Config';
import AppbarShortCut from 'Components/LayoutComponents/AppbarShortCut';

const styles = (theme) => ({
    root: {
        '&, & .horizontal-nav ul ul': {
            background: NAV_BAR_COLOR,
            color: '#494949'
        },
        '& .horizontal-nav ul li a': {
            color: '#494949'
        },
        '& .horizontal-nav ul li': {
            padding: '0px'
        },
        '& .horizontal-nav ul li a:hover': {
            color: '#fff'
        },
        '& .horizontal-nav a, & .horizontal-nav label': {
            color: '#fff'
        },
        '& .horizontal-nav ul li ul li:hover, & .horizontal-nav ul li ul li.open': {
            background: '#eb008b',
            color: '#fff'
        },
        '& .horizontal-nav ul li ul': {
            background: '#000',
            color: '#fff'
        },
        '& .horizontal-nav ul li ul li a': {
            color: '#fff'
        },
        '& .horizontal-nav li > a::after': {
            color: '#fff'
        },
        '& .horizontal-nav ul li:hover': {
            background: '#000',
            color: '#fff'
        },
        '& .horizontal-nav': {
            padding: '0 15px'
        }
        // '.navbarShort': {
        //     display: 'inline'
        // }
    }
});

const ShortCutMenu = (props) => {
    let { classes } = props;

    const {
        PendingKYCCount,
        FundRequesCount,
        FundReverseRequestCount,
        ComplaintCount,
        PendingTransactionCount,
        PendingDMTTransactionCount,
        CallMeRequestCount,
        PendingTicketCount,
        PasswordExpiraryDays,
        PendingUPITransactionCount
    } = useContext(BalanceContext);

    const flexContainer = {
        display: 'flex',
        flexDirection: 'row',
        padding: 0
    };

    return (
        <div className={`navbarShort ${classes.root}`}>
            <div className={`horizontal-nav`}>
                <ul className="menu" style={flexContainer}>
                    <AppbarShortCut
                        Title="Live Transaction"
                        Content={''}
                        Link="/LiveTransaction"
                        Icon={'video-camera'}
                    />
                    <AppbarShortCut
                        Title="Pending Recharge"
                        Content={PendingTransactionCount.toString()}
                        Link="/Pending"
                        Icon={'hourglass-end'}
                    />
                    <AppbarShortCut
                        Title="Pending DMT"
                        Content={PendingDMTTransactionCount.toString()}
                        Link="/PendingDmt"
                        Icon={'dollar-sign'}
                    />

                    <AppbarShortCut
                        Title="Fund Requests"
                        Content={FundRequesCount.toString()}
                        Link="/Fund"
                        Icon={'inr'}
                    />
                    <AppbarShortCut
                        Title="Fund Reverse Request"
                        Content={FundReverseRequestCount.toString()}
                        Link="/FundReverseRequestReport"
                        Icon={'inr'}
                    />

                    <AppbarShortCut
                        Title="Complaints"
                        Content={ComplaintCount.toString()}
                        Link="/Complaints"
                        Icon={'times-circle'}
                    />

                    <AppbarShortCut
                        Title="Newly Added User"
                        Content={''}
                        Link="/Users/DatewiseReport"
                        Icon={'user-plus'}
                    />

                    {/* <AppbarShortCut
                        Title="Ticket"
                        Content={PendingTicketCount.toString()}
                        Link="/TicketReport"
                        ClassName="desktopmenu"
                        Icon={'envelope'}
                    /> */}
                    <AppbarShortCut
                        Title="Remaining days for password expiry"
                        Content={PasswordExpiraryDays.toString()}
                        ClassName="desktopmenu"
                        Link="/Users/ChangePassword"
                        Icon={'key'}
                    />
                    <AppbarShortCut
                        Title="Version History"
                        Content={''}
                        ClassName="desktopmenu"
                        Link="/VersionHistory"
                        Icon={'vial'}
                    />
                </ul>
            </div>
        </div>
    );
};

export default withStyles(styles, { withTheme: true })(ShortCutMenu);
