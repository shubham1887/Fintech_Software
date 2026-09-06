import React from 'react';
import {
    FormControl,
    Radio,
    RadioGroup,
    FormControlLabel
} from '@mui/material';

const CustomRadioButton = ({ label, displayValueID, isSelected, onChange }) => {
                                                                                   const handleChange = (
                                                                                       event
                                                                                   ) => {
                                                                                       const newValue =
                                                                                           event
                                                                                               .target
                                                                                               .value;
                                                                                       onChange(
                                                                                           newValue
                                                                                       );
                                                                                   };

                                                                                   return (
                                                                                       <FormControl>
                                                                                           <RadioGroup
                                                                                               aria-labelledby="demo-radio-buttons-group-label"
                                                                                               value={
                                                                                                   isSelected
                                                                                                       ? displayValueID
                                                                                                       : ''
                                                                                               }
                                                                                               onChange={
                                                                                                   handleChange
                                                                                               }
                                                                                               name="radio-buttons-group"
                                                                                           >
                                                                                               <FormControlLabel
                                                                                                   value={
                                                                                                       displayValueID
                                                                                                   }
                                                                                                   control={
                                                                                                       <Radio />
                                                                                                   }
                                                                                                   label={
                                                                                                       label
                                                                                                   }
                                                                                               />
                                                                                           </RadioGroup>
                                                                                       </FormControl>
                                                                                   );
                                                                               };
export default CustomRadioButton;
