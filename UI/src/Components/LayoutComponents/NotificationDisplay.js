import React, { memo, useState, useEffect, useContext } from 'react';
import { Drawer, Card, Button, Icon, IconButton } from '@material-ui/core';
import { getTimeDifference } from 'Components/utils.js';
import {
    SelectByUserID as NEWS_SELECT_BY_USERID,
    UpdateIsView as UPDATE_ISVIEW
} from 'Components/Api/NewsAPI';
import { BalanceContext } from 'Components/BalanceContext';

const NotificationDisplay = ({ IsOpen, handleNotificationPanel }) => {
    const [News, setNews] = useState([]);
    const { NotificationCount } = useContext(BalanceContext);
    const [NewsCount, setNewsCount] = useState(
        parseInt(localStorage.getItem('NewsCount')) || NotificationCount
    );
    useEffect(() => {
        if (IsOpen === true) GetNotifications();
    }, [IsOpen]);

    const GetNotifications = async () => {
        const response = await NEWS_SELECT_BY_USERID();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setNews(data);
        setNewsCount(data.length);
    };

    const deleteNotification = async (NewsID) => {
        let n = News.filter((news) => {
            return news.NewsID !== NewsID;
        });
        setNews(n);

        let jsonObj = {
            NewsID: NewsID
        };

        const response = await UPDATE_ISVIEW(JSON.stringify(jsonObj));

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setNews(data);
        setNewsCount(data.length);

        localStorage.setItem('NewsCount', data.length);
    };

    const NotificationList = News.map(
        ({ NewsID, Date: NotificationDate, Title, Description, ImagePath }) => (
            <div
                key={NewsID}
                className="notification__card position-relative"
                style={{ width: '255px' }}
            >
                <Card className="mx-4 mb-6" elevation={3}>
                    <div className="card__topbar flex items-center justify-between p-2 bg-light-gray">
                        <div className="flex items-center">
                            <div className="card__topbar__button">
                                <Icon
                                    className="card__topbar__icon"
                                    fontSize="small"
                                >
                                    notifications
                                </Icon>
                            </div>
                            <span className="ml-4 font-medium text-muted"></span>
                        </div>
                        <small className="card__topbar__time text-muted">
                            {getTimeDifference(new Date(NotificationDate))}
                            ago
                        </small>
                        <IconButton
                            size="small"
                            className="delete-button bg-light-gray mr-6"
                            onClick={() => deleteNotification(NewsID)}
                        >
                            <Icon className="text-muted" fontSize="small">
                                clear
                            </Icon>
                        </IconButton>
                    </div>
                    <div className="px-4 pt-2 pb-4">
                        <p className="m-0">{Title}</p>
                        <small className="text-muted">{Description}</small>

                        <img
                            name="Image"
                            src={ImagePath}
                            height={46}
                            width={150}
                            alt={ImagePath}
                        />
                    </div>
                </Card>
            </div>
        )
    );

    const handleClearNotifications = () => {
        deleteNotification(99999);
    };
    const ClearNotificationButton = (
        <div className="text-center">
            <Button key="ClearNotifications" onClick={handleClearNotifications}>
                Clear
            </Button>
        </div>
    );
    const NotificationPanelTitle = (
        <div className="notification">
            <div className="notification__topbar flex items-center p-4 mb-4">
                <Icon color="primary">notifications</Icon>
                <h5 className="ml-2 my-0 font-medium">News</h5>
            </div>
        </div>
    );
    return (
        <Drawer
            variant="temporary"
            anchor={'right'}
            open={IsOpen}
            elevation={NewsCount}
            onClose={handleNotificationPanel}
        >
            {NotificationPanelTitle}
            {!IsNewsPresent(News) ? DefaultNotification : NotificationList}

            {!IsNewsPresent(News) ? null : ClearNotificationButton}
        </Drawer>
    );
};

function IsNewsPresent(News) {
    return News === [] ||
        News === null ||
        News === undefined ||
        News.length === 0
        ? false
        : true;
}

const DefaultNotification = (
    <div
        className="notification__card position-relative"
        style={{ width: '255px' }}
    >
        <Card className="mx-4 mb-6" elevation={3}>
            <div className="flex items-center">
                <div className="card__topbar__button">
                    <Icon className="card__topbar__icon" fontSize="small">
                        notifications
                    </Icon>
                </div>
                <span className="ml-4 font-medium text-muted">News</span>
            </div>
            <div className="px-4 pt-2 pb-4">
                <small className="text-muted">No news for you</small>
            </div>
        </Card>
    </div>
);

export default memo(NotificationDisplay);
