import React from 'react';
import Accordion from '@material-ui/core/Accordion';
import AccordionSummary from '@material-ui/core/AccordionSummary';
import AccordionDetails from '@material-ui/core/AccordionDetails';
import Typography from '@material-ui/core/Typography';
import ExpandMoreIcon from '@material-ui/icons/ExpandMore';
const HOCAccordion = (Comp) => ({
    open,
    setOpen,
    title,
    children,
    ...props
}) => {
    const handleChange = (panel) => (event, newExpanded) => {
        setOpen(newExpanded ? panel : false);
    };

    return (
        <Accordion expanded={open} onChange={handleChange(!open)}>
            <AccordionSummary
                expandIcon={<ExpandMoreIcon />}
                aria-controls="panel1a-content"
                id="panel1a-header"
            >
                <Typography component={'span'}>
                    <h6>{title}</h6>
                </Typography>
            </AccordionSummary>
            <AccordionDetails>{children}</AccordionDetails>
        </Accordion>
    );
};

export default HOCAccordion;
