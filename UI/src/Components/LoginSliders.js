import React, { useState, useEffect, memo } from 'react';
import { SelectPublicSliders as SELECT_PUBLIC_SLIDERS } from 'Components/Api/SliderMasterAPI';
import { Box } from '@material-ui/core';

import Carousel from 'react-material-ui-carousel';

const LoginSliders = () => {
    const [sliders, setSliders] = useState([]);

    let Request = {
        Type: 'Web'
    };

    let jSONObj = JSON.stringify(Request);

    const selectSliders = async () => {
        const response = await SELECT_PUBLIC_SLIDERS(jSONObj);

        if (response && !response.status) return;

        var data = JSON.parse(response.data);
        setSliders(data);
    };

    useEffect(() => {
        if (navigator.onLine) selectSliders();
    }, []);

    return (
        <Carousel
            autoPlay={true}
            animation="fade"
            indicators={true}
            startAt={0}
            className="centerslider"
        >
            {sliders &&
                sliders.map((item) => (
                    <Box
                        width="1"
                        key={String(item.SliderMasterID)}
                        style={{ textAlign: 'center' }}
                    >
                        <img src={item.ImagePath} alt={'Slider'} />
                    </Box>
                ))}
        </Carousel>
    );
};

export default memo(LoginSliders);
