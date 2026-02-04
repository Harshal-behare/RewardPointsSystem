# AGDATA Design System - UI Specifications

## Overview
This document contains the complete visual design specifications for the AGDATA application. Use this as a reference to understand the exact colors, typography, spacing, and visual style requirements.

---

## Typography

### Font Family
- **Primary Typeface**: Roboto
- Use Roboto for all text elements throughout the application

### Heading Styles

| Heading Level | Size (px/rem) | Line Height (px/rem) | Weight | Token |
|---------------|---------------|----------------------|--------|-------|
| **heading-01** (H1) | 42px / 2.625rem | 50px / 3.125rem | 700 (Bold) | `--ag-heading-01` |
| **heading-02** (H2) | 32px / 2rem | 40px / 2.5rem | 500 (Medium) | `--ag-heading-02` |
| **heading-03** (H3) | 28px / 1.75rem | 36px / 2.25rem | 500 (Medium) | `--ag-heading-03` |
| **heading-04** (H4) | 20px / 1.25rem | 28px / 1.75rem | 500 (Medium) | `--ag-heading-04` |
| **heading-05** (H5) | 16px / 1rem | 24px / 1.5rem | 500 (Medium) | `--ag-heading-05` |
| **heading-06** (H6) | 14px / 0.875rem | 20px / 1.25rem | 500 (Medium) | `--ag-heading-06` |

### Body Text Styles

| Style | Size (px/rem) | Line Height (px/rem) | Weight | Letter Spacing | Token |
|-------|---------------|----------------------|--------|----------------|-------|
| **Body Large - Regular** | 16px / 1rem | 24px / 1.5rem | 400 (Regular) | 0 | `--ag-body-01` |
| **Body Large - Bold** | 16px / 1rem | 24px / 1.5rem | 700 (Bold) | 0 | `--ag-body-02` |
| **Body Large - Italic** | 16px / 1rem | 24px / 1.5rem | 400 (Italic) | 0 | `--ag-body-03` |
| **Body Small - Regular** | 14px / 0.875rem | 18px / 1.125rem | 400 (Regular) | 0.16px | `--ag-body-04` |
| **Body Small - Bold** | 14px / 0.875rem | 18px / 1.125rem | 700 (Bold) | 0.16px | `--ag-body-05` |
| **Body Small - Italic** | 14px / 0.875rem | 18px / 1.125rem | 400 (Italic) | 0.16px | `--ag-body-06` |

---

## Color System

### Text Colors

| Token | Role | Color Value | Usage |
|-------|------|-------------|-------|
| `$text-primary` | Primary body copy, Headers, Hover text for $text-secondary, Selected text color | Gray 100<br>#161616 | Main text throughout the application |
| `$text-secondary` | Secondary text color, Input labels color, Unselected text color | Gray 70<br>#525252 | Supporting text, form labels |
| `$text-placeholder` | Placeholder text | Gray 40<br>#a8a8a8 | Input placeholder text |
| `$text-on-color` | Text on interactive colors | White<br>#ffffff | Text displayed on colored backgrounds |
| `$text-helper` | Tertiary text, Help text | Gray 60<br>#6f6f6f | Helper text, hints, descriptions |
| `$text-error` | Error message text | Red 60<br>#da1e28 | Error messages and validation |
| `$text-inverse` | Text color on $background-inverse | White<br>#ffffff | Text on dark/inverse backgrounds |
| `$text-disabled` | Disabled text color, Disabled label | Gray 100 @ 25%<br>#161616 (opacity: 25%) | Disabled state text |
| `$text-on-color-disabled` | Disabled text color for $text-on-color | Gray 50<br>#8d8d8d | Disabled text on colored backgrounds |

### Background Colors

| Token | Role | Color Value | Usage |
|-------|------|-------------|-------|
| `$background` | Default page background | White<br>#ffffff | Main application background |
| `$background-active` | Active state color for $background | Gray 50 @ 50%<br>#8d8d8d (opacity: 50%) | Active/pressed state backgrounds |
| `$background-hover` | Hover state color for $background | Gray 50 @ 12%<br>#8d8d8d (opacity: 12%) | Hover state backgrounds |
| `$background-selected` | Selected state color for $background | Gray 50 @ 20%<br>#8d8d8d (opacity: 20%) | Selected state backgrounds |
| `$background-selected-hover` | Hover state color for $background-selected | Gray 50 @ 32%<br>#8d8d8d (opacity: 32%) | Selected + hover state |
| `$background-brand` | Feature background color | Cyan 60<br>#007D79 | Brand-colored backgrounds, feature areas |
| `$background-inverse` | High contrast background color | DeepOcean 80<br>#004E60 | Dark/inverse backgrounds for contrast |
| `$background-inverse-hover` | Hover state color for $background-inverse | Gray 80 hover<br>#474747 | Hover on inverse backgrounds |

### Icon Colors

| Token | Role | Color Value | Usage |
|-------|------|-------------|-------|
| `$icon-primary` | Primary icons | Gray 100<br>#161616 | Main icon color |
| `$icon-secondary` | Secondary icons | Gray 70<br>#525252 | Secondary/supporting icons |
| `$icon-on-color` | Icons on interactive colors, Icons on non-UI colors | White<br>#ffffff | Icons on colored backgrounds |
| `$icon-inverse` | Inverse text color, Inverse icon color | White<br>#ffffff | Icons on dark backgrounds |
| `$icon-on-color-disabled` | Disabled state color for $icon-on-color | Gray 50<br>#8d8d8d | Disabled icons on colored backgrounds |
| `$icon-disabled` | Disabled state for icons | Gray 100 @ 25%<br>#161616 (opacity: 25%) | Disabled icon state |
| `$icon-interactive` | Icon color | Blue 60<br>#0f62fe | Interactive/clickable icons |

### Tag/Badge Colors

The design system includes multiple color schemes for tags and badges:

#### Blue Tags
- **Background (Light)**: Blue 20 - #d0e2ff
- **Text/Icon**: Blue 70 - #0043ce
- **Hover Background**: Blue hover - #0a6f3f

#### Cyan Tags
- **Background (Light)**: Cyan 20 - #bae6ff
- **Text/Icon**: Cyan 70 - #00539a
- **Hover Background**: Cyan hover - #99daff

#### Green Tags
- **Background (Light)**: Green 20 - #a7f0ba
- **Text/Icon**: Green 70 - #0e6027
- **Hover Background**: Green hover - #74e792

#### Magenta Tags
- **Background (Light)**: Magenta 20 - #ffd6e8
- **Text/Icon**: Magenta 70 - #9f1853
- **Hover Background**: Magenta hover - #ffbdda

#### Purple Tags
- **Background (Light)**: Purple 20 - #e8daff
- **Text/Icon**: Purple 70 - #6929c4
- **Hover Background**: Purple hover - #d4c7ff

#### Red Tags
- **Background (Light)**: Red 20 - #ffd7d9
- **Text/Icon**: Red 70 - #a2191f
- **Hover Background**: Red hover - #ffc2c5

#### Gray Tags
- **Background (Light)**: Gray 20 - #e0e0e0
- **Text/Icon**: Gray 100 - #161616
- **Hover Background**: Gray hover - #d1d1d1

---

## Button Styles

### Visual Specifications

The design system provides multiple button states and variants. Each button style includes:

**Button States:**
- Default (Normal)
- Hover
- Active/Pressed
- Focus
- Disabled

**Button Color Variants:**
The comprehensive button component library shows buttons in all the tag colors:
- Blue buttons
- Cyan buttons
- Green buttons (matches brand color)
- Magenta buttons
- Purple buttons
- Red buttons
- Gray buttons
- Black buttons

### Button Visual Characteristics
- **Border Radius**: Slightly rounded corners (appears to be ~4px)
- **Padding**: Consistent horizontal and vertical padding
- **Typography**: Roboto font, medium weight for button text
- **Icons**: Some buttons include left or right icons
- **Sizes**: Multiple size variants available (large, medium, small)

---

## Form Elements

### Input Fields

**Visual Style:**
- Clean, minimal border styling
- Label positioned above input field
- Label color: `$text-secondary` (#525252)
- Placeholder color: `$text-placeholder` (#a8a8a8)
- Input text color: `$text-primary` (#161616)

**States:**
- Default
- Focus (with border highlight)
- Hover
- Disabled (using `$text-disabled`)
- Error (using `$text-error` for messages)

**Helper Text:**
- Uses `$text-helper` color (#6f6f6f)
- Positioned below input field
- Small size (14px body text)

---

## Spacing & Layout

### General Spacing Guidelines

Based on the component specifications:
- **Letter spacing** ranges from 0 to 0.16px for different text sizes
- **Line height** follows a consistent rhythm (1.25x to 3.125x the font size)
- Components use consistent padding and margins for visual rhythm

---

## Component UI Guidelines

### Cards & Containers
- Use white background (`$background`: #ffffff)
- Subtle shadows for elevation
- Rounded corners for modern aesthetic
- Adequate padding for content breathing room

### Lists & Tables
- Clear row separation
- Hover states using `$background-hover`
- Selected states using `$background-selected`
- Consistent vertical spacing between rows

### Navigation
- Primary navigation uses brand colors
- Active/selected states clearly indicated
- Icon + text combinations for clarity
- Hover feedback on all interactive elements

### Modals & Overlays
- Inverse background (`$background-inverse`: #004E60) for overlay
- White content area on top
- Clear close/dismiss actions
- Proper focus management

---

## Accessibility Considerations

### Color Contrast
- All text colors meet WCAG 2.1 AA standards against their backgrounds
- `$text-primary` (#161616) provides maximum contrast on white
- `$text-on-color` (white) ensures readability on colored backgrounds
- Error text (#da1e28) is clearly distinguishable

### Interactive States
- All interactive elements have clear hover states
- Focus indicators for keyboard navigation
- Disabled states are visually distinct (reduced opacity)
- Active/pressed states provide clear feedback

### Text Readability
- Minimum font size: 14px
- Adequate line height for readability (1.125x to 1.5x)
- Comfortable letter spacing where needed

---

## Brand Identity

### Primary Brand Color
**Cyan 60 - #007D79**
- Use for primary actions and brand moments
- Feature backgrounds and highlights
- Primary CTAs (Call-to-Action buttons)

### Supporting Colors
The design system provides a full spectrum of colors for:
- Status indicators (success, warning, error)
- Categories and tags
- Data visualization
- User feedback

### Logo Usage
- AGDATA logo should be prominently displayed
- Maintain clear space around logo
- Use on white or dark backgrounds as appropriate

---

## Design Principles

### Visual Hierarchy
1. Use heading levels appropriately (H1 for page titles, H2 for sections, etc.)
2. Primary actions should use brand colors
3. Secondary information uses gray scale
4. Error states use red, success uses green

### Consistency
1. Use design tokens throughout (don't hardcode colors)
2. Maintain consistent spacing between elements
3. Follow the established button and form patterns
4. Use the same iconography style

### Simplicity
1. Clean, minimal interface
2. Adequate white space
3. Clear typography hierarchy
4. Purposeful use of color

### Feedback
1. All interactive elements have hover states
2. Loading states for async actions
3. Clear error and success messages
4. Disabled states for unavailable actions

---

## Example Component Combinations

### Login/Register Form
- White background card
- Heading-02 for form title
- Body-04 for labels
- Primary brand button for submit
- Text-helper for password requirements
- Text-error for validation messages

### Dashboard Stats Card
- White background with subtle shadow
- Heading-03 for stat title
- Heading-01 for large numbers
- Tag colors for category badges
- Icon-primary for supporting icons

### Data Table
- White background
- Heading-05 for column headers
- Body-04 for cell content
- Background-hover for row hover
- Background-selected for selected rows
- Icon-secondary for action icons

### Navigation Header
- Background-brand or white
- Text-inverse or text-primary accordingly
- Icon-inverse or icon-primary for nav icons
- Active state using background-selected

---

## Notes for Implementation

1. **Roboto Font**: Ensure Roboto font family is loaded (via Google Fonts or local hosting)

2. **Design Tokens**: Use CSS custom properties or your framework's theming system to implement these tokens for easy maintenance

3. **Responsive Design**: While not explicitly shown, ensure all components scale appropriately for mobile, tablet, and desktop

4. **Component Library**: Consider building a reusable component library based on these specifications

5. **Dark Mode**: The inverse background tokens suggest consideration for dark mode - this can be expanded if needed

6. **Animations**: Add subtle transitions (150-300ms) for state changes (hover, focus, etc.)

7. **Elevation**: Use subtle shadows to create depth hierarchy:
   - Cards: light shadow
   - Modals: medium shadow  
   - Dropdowns: medium shadow
   - Tooltips: small shadow

---

## Color Palette Quick Reference

### Grays
- Gray 100: #161616 (Primary text)
- Gray 80: #474747
- Gray 70: #525252 (Secondary text)
- Gray 60: #6f6f6f (Helper text)
- Gray 50: #8d8d8d
- Gray 40: #a8a8a8 (Placeholder)
- Gray 20: #e0e0e0

### Brand & Feature
- Cyan 60: #007D79 (Primary brand)
- Cyan 70: #00539a
- Cyan 20: #bae6ff
- DeepOcean 80: #004E60 (Inverse background)

### Status Colors
- Blue 60: #0f62fe (Interactive)
- Blue 70: #0043ce
- Green 70: #0e6027 (Success)
- Green 20: #a7f0ba
- Red 60: #da1e28 (Error)
- Red 70: #a2191f

### Always Available
- White: #ffffff
- Black (in some contexts): Used sparingly

---

## Summary

This design system provides a comprehensive, professional visual language that is:
- **Consistent**: Using design tokens ensures visual consistency
- **Accessible**: Meeting WCAG standards for color contrast
- **Scalable**: Token-based system allows easy updates
- **Professional**: Clean, modern aesthetic suitable for enterprise applications
- **Flexible**: Multiple color variants for different use cases

Use these specifications as the single source of truth for all visual design decisions in the application.
