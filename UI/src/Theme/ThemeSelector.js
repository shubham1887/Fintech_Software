import { LightTheme } from 'Theme/LightTheme';
import { DarkTheme } from 'Theme/DarkTheme';

const ThemeSelector = (name) => {
    return name === 'light' ? LightTheme : DarkTheme;
};

export default ThemeSelector;
