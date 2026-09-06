import React, { memo, useState } from 'react';
import Marquee from 'react-fast-marquee';
import { PRIMARY_COLOR } from 'Config';

const NewsMarquee = () => {
    const [NewsMessage] = useState(localStorage.getItem('NewsMessage'));
    const [HyperLink] = useState(localStorage.getItem('HyperLink'));

    if (
        NewsMessage === undefined ||
        NewsMessage === null ||
        NewsMessage.length <= 0
    ) {
        return null;
    }
    let NewsWithTyperLink = '';
    if (HyperLink.length > 5) {
        NewsWithTyperLink = (
            <a href={HyperLink} target="_blank" rel="noopener noreferrer">
                {NewsMessage}
            </a>
        );
    } else {
        NewsWithTyperLink = NewsMessage;
    }

    return (
        <Marquee
            style={{ color: PRIMARY_COLOR() }}
            pauseOnHover
            gradient={false}
            speed={50}
        >
            {NewsWithTyperLink}
        </Marquee>
    );
};

export default memo(NewsMarquee);
