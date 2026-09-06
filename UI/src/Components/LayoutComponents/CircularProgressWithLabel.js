import Box from '@material-ui/core/Box';
import CircularProgress from '@material-ui/core/CircularProgress';
import Typography from '@material-ui/core/Typography';
import PropTypes from 'prop-types';
import { MENU_FORE_COLOR } from 'Config';
import React, { useEffect, useState, memo } from 'react';

const CircularProgressWithLabel = (props) => {
    const [progressValue, setProgressValue] = useState(0);

    const [variant, setVariant] = useState('determinate');

    useEffect(() => {
        let result = (parseInt(props.value) * 100) / 365;
        setProgressValue(result);
        if (result <= 15) {
            setVariant('indeterminate');
            return;
        }
    }, [props.value]);

    return (
        <Box position="relative" display="inline-flex">
            <CircularProgress
                style={{ color: MENU_FORE_COLOR() }}
                variant={variant}
                disableShrink={variant === 'indeterminate' ? true : false}
                value={parseInt(progressValue)}
            />
            <Box
                top={0}
                left={0}
                bottom={0}
                right={0}
                position="absolute"
                display="flex"
                alignItems="center"
                justifyContent="center"
            >
                <Typography
                    style={{ color: MENU_FORE_COLOR() }}
                    variant="caption"
                    component="div"
                >{`${Math.round(props.value)}`}</Typography>
            </Box>
        </Box>
    );
};

CircularProgressWithLabel.propTypes = {
    value: PropTypes.string.isRequired
};

export default memo(CircularProgressWithLabel);
