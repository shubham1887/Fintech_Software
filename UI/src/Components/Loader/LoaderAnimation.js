import React from 'react';
import Lottie from 'react-lottie';

import * as animation from 'Assets/Images/animation.json';

const LottieControl = () => {
    let animationData = animation.default;

    const defaultOptions = {
        loop: true,
        autoplay: true,
        animationData: animationData,
        rendererSettings: {
            preserveAspectRatio: 'xMidYMid slice'
        }
    };

    return (
        <div>
            <Lottie options={defaultOptions} height={'80%'} width={'80%'} />
        </div>
    );
};

export default LottieControl;
