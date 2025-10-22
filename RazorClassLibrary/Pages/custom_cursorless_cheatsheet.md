# Cursorless Reference Guide

This document provides a practical overview of Cursorless, including essential commands, scopes, and usage examples. It is based on the official cheatsheet and annotated screenshots from your workspace.

## What is Cursorless?
Cursorless is a VS Code extension that enables voice-driven, rapid code editing and navigation using spoken commands and visual hats/marks.

---

## Key Concepts
- **Scopes**: Logical code units (e.g., object, item, block, map, value, key, line, comment, etc.)
- **Hats/Marks**: Visual indicators placed on code elements for precise selection and manipulation.
- **Targets**: The code element you want to act on (e.g., "item", "object", "block").

---

## Common Selection Commands
| Command                | Description                                                      |
|------------------------|------------------------------------------------------------------|
| `take item`            | Selects the entire item (e.g., a key-value pair in JSON)          |
| `take object`          | Selects the entire object (e.g., a JSON object)                   |
| `take block`           | Selects a block of code (e.g., function, paragraph)               |
| `take map`             | Selects a map/dictionary structure                                |
| `take value`           | Selects the value part of a key-value pair                        |
| `take key`             | Selects the key part of a key-value pair                          |
| `take line`            | Selects the entire line                                           |
| `take comment`         | Selects a comment                                                 |
| `take curly`           | Selects content inside curly braces `{}`                          |
| `take twin`            | Selects matching pair of quotes                                   |
| `take short block`     | Selects a bounded paragraph                                       |
| `take paint`           | Selects a non-whitespace sequence                                 |

---

## Example: Selecting a JSON Object
Suppose you want to select the entire `workbench.colorCustomizations` object in your settings.json:

- Place your cursor on or before the property name.
- Say: `take item` (to select the key-value pair)
- Say: `take object` (to select the full object, including all nested properties)

### Screenshot Reference
Your screenshot shows hats/marks on each property, and the left panel lists supported scopes. Use these visual cues to target specific elements with Cursorless commands.

---

## Scopes from Screenshot
- **comment**: Comment
- **item**: Collection item (e.g., array or object property)
- **key**: Collection key
- **list**: List
- **map**: Map/dictionary
- **value**: Value in a key-value pair
- **block**: Paragraph or code block
- **curly**: Content inside curly braces
- **identifier**: Variable or property name
- **line**: Line
- **link**: Link
- **paint**: Non-whitespace sequence
- **pair**: Matching pair of any quotes
- **round**: Parentheses
- **sentence**: Sentence
- **short block**: Bounded paragraph
- **short paint**: Bounded non-whitespace sequence
- **skis**: Matching pair of backtick quotes
- **string**: String
- **token**: Token
- **twin**: Matching pair of single/double quotes

---

## Tips
- Use hats/marks for precise selection: e.g., "take item air" (where "air" is the hat on the item).
- Combine actions: e.g., "change item", "delete block", "swap value".
- Use the cheatsheet for more advanced actions and variations.

---

## Resources
- [Cursorless Cheatsheet](file:///C:/Users/MPhil/.cursorless/cheatsheet.html)
- [Cursorless Documentation](https://cursorless.org/)

---

## Updates
Add new commands and examples here as you discover more advanced Cursorless features.