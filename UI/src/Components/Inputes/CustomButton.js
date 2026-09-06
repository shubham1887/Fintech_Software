import React from 'react';

import { Button, Tooltip } from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';
import PlanIcon from 'Components/SvgButtons/PlanIcon';
import { green } from '@material-ui/core/colors';

import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR, SECONDARY_COLOR } from 'Config';

const useStyles = makeStyles({
    SecondaryButton: (props) => ({
        '&:hover': {
            background: `${props.hoverColor} !important`,
            color: '#fff'
        },
        '&:disabled': {
            background: '#dddddd !important',
            color: '#fff !important'
        }
    }),

    wrapper: {
        position: 'relative',
        margin: '0px 0px 0 8px',
        height: '100%'
    },
    buttonProgress: {
        color: green[500],
        position: 'absolute',
        top: '50%',
        left: '50%',
        marginTop: -12,
        marginLeft: -12
    }
});

export const PrimaryButton = ({
    title,
    disabled,
    endIcon,
    submitType,
    onClick,
    tooltip
}) => {
    const propsInline = { hoverColor: PRIMARY_COLOR() };

    const classesInline = useStyles(propsInline);

    return (
        <>
            <Tooltip title={tooltip ? tooltip : title}>
                <>
                    <Button
                        variant="contained"
                        color="primary"
                        type={submitType ? submitType : 'submit'}
                        size="large"
                        //title={title}
                        onClick={onClick}
                        disabled={disabled}
                        style={{
                            margin: '0px 0px 0 0px',
                            padding: '12px 6px',
                            fontSize: '18px',
                            fontWeight: '600',
                            width: '100%',
                            textTransform: 'none'
                        }}
                        endIcon={endIcon}
                    >
                        {title}
                    </Button>
                </>
            </Tooltip>

            {disabled && (
                <CircularProgress
                    size={24}
                    className={classesInline.buttonProgress}
                />
            )}
        </>
    );
};

export const SecondaryButton = ({ title, disabled, endIcon, onClick }) => {
    const propsInline = { hoverColor: SECONDARY_COLOR() };

    const classesInline = useStyles(propsInline);
    return (
        <>
            <Button
                variant="contained"
                color="secondary"
                type="reset"
                //size="large"
                title={title}
                onClick={onClick}
                disabled={disabled}
                endIcon={endIcon}
                style={{
                    fontWeight: '600',
                    margin: '0px 0px 0 0px',
                    fontSize: '18px',
                    width: '100%',
                    padding: '12px 6px',
                    textTransform: 'none'
                }}
            >
                {title}
            </Button>
            {disabled && (
                <CircularProgress
                    size={24}
                    className={classesInline.buttonProgress}
                />
            )}
        </>
    );
};

export const InfoButton = ({ title, disabled, onClick, submitType }) => {
    const propsInline = { hoverColor: PRIMARY_COLOR() };

    const classesInline = useStyles(propsInline);

    return (
        <>
            <Button
                variant="contained"
                type={submitType ? submitType : 'submit'}
                style={{
                    backgroundColor: '#39b54a',
                    color: '#fff',
                    fontWeight: '600',
                    margin: '0px 0px 0 0px',
                    width: '50%',
                    padding: '12px 6px',
                    fontSize: '18px',
                    textTransform: 'none'
                }}
                title={title}
                className={classesInline.SecondaryButton}
                onClick={onClick}
                disabled={disabled}
                //endIcon={<PlanIcon />}
            >
                {title}
            </Button>

            {disabled && (
                <CircularProgress
                    size={24}
                    className={classesInline.buttonProgress}
                />
            )}
        </>
    );
};
