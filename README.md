# Kab's Skilled Spells
Daggerfall Unity mod for skill-based caster level.

## Description

Unlike Arena, Daggerfall allows all classes to cast spells. However, it still keeps the same character level scaling for all spells. This means that a level 30 warrior who's never casted a spell in their life casts the same Fireball as a level 30 Battlemage (though with less spell points efficiency). 

This mod introduces a compromise where the caster level used to calculate the strength of spells is relative to both character level and skill level in the spell effect's magic school.

The progression looks like this, where the columns correspond to skill level, and the rows the character level.

| Player Level | 10 | 20 | 30 | 40 | 50 | 60 | 70 | 80 | 90 | 100 |
|--------------|----|----|----|----|----|----|----|----|----|-----|
| 1            | 1  | 1  | 1  | 1  | 1  | 1  | 1  | 1  | 1  | 1   |
| 2            | 1  | 1  | 1  | 1  | 1  | 1  | 1  | 2  | 2  | 2   |
| 3            | 1  | 1  | 1  | 1  | 2  | 2  | 2  | 2  | 3  | 3   |
| 4            | 1  | 1  | 1  | 2  | 2  | 2  | 3  | 3  | 4  | 4   |
| 5            | 1  | 1  | 2  | 2  | 3  | 3  | 4  | 4  | 5  | 5   |
| 6            | 1  | 1  | 2  | 2  | 3  | 4  | 4  | 5  | 5  | 6   |
| 7            | 1  | 1  | 2  | 3  | 4  | 4  | 5  | 6  | 6  | 7   |
| 8            | 1  | 2  | 2  | 3  | 4  | 5  | 6  | 6  | 7  | 8   |
| 9            | 1  | 2  | 3  | 4  | 5  | 5  | 6  | 7  | 8  | 9   |
| 10           | 1  | 2  | 3  | 4  | 5  | 6  | 7  | 8  | 9  | 10  |
| 11           | 1  | 2  | 3  | 4  | 6  | 7  | 8  | 9  | 10 | 11  |
| 12           | 1  | 2  | 4  | 5  | 6  | 7  | 8  | 10 | 11 | 12  |
| 13           | 1  | 3  | 4  | 5  | 7  | 8  | 9  | 10 | 12 | 13  |
| 14           | 1  | 3  | 4  | 6  | 7  | 8  | 10 | 11 | 13 | 14  |
| 15           | 2  | 3  | 5  | 6  | 8  | 9  | 11 | 12 | 14 | 15  |
| 16           | 2  | 3  | 5  | 6  | 8  | 10 | 11 | 13 | 14 | 16  |
| 17           | 2  | 3  | 5  | 7  | 9  | 10 | 12 | 14 | 15 | 17  |
| 18           | 2  | 4  | 5  | 7  | 9  | 11 | 13 | 14 | 16 | 18  |
| 19           | 2  | 4  | 6  | 8  | 10 | 11 | 13 | 15 | 17 | 19  |
| 20           | 2  | 4  | 6  | 8  | 10 | 12 | 14 | 16 | 18 | 20  |
| 21           | 2  | 4  | 6  | 8  | 11 | 13 | 15 | 17 | 19 | 21  |
| 22           | 2  | 4  | 7  | 9  | 11 | 13 | 15 | 18 | 20 | 22  |
| 23           | 2  | 5  | 7  | 9  | 12 | 14 | 16 | 18 | 21 | 23  |
| 24           | 2  | 5  | 7  | 10 | 12 | 14 | 17 | 19 | 22 | 24  |
| 25           | 3  | 5  | 8  | 10 | 13 | 15 | 18 | 20 | 23 | 25  |
| 26           | 3  | 5  | 8  | 10 | 13 | 16 | 18 | 21 | 23 | 26  |
| 27           | 3  | 5  | 8  | 11 | 14 | 16 | 19 | 22 | 24 | 27  |
| 28           | 3  | 6  | 8  | 11 | 14 | 17 | 20 | 22 | 25 | 28  |
| 29           | 3  | 6  | 9  | 12 | 15 | 17 | 20 | 23 | 26 | 29  |
| 30           | 3  | 6  | 9  | 12 | 15 | 18 | 21 | 24 | 27 | 30  |

Note that characters who pick magic schools as their Primary and Major skills also get a slight bonus.

>Doesn't this make spell progression slower overall?

Yes, until you master the skill entirely. I don't think it's a bad thing, spells already scale to ridiculous levels in both Arena and Daggerfall, this just makes them very strong instead.

## Changelog

- 0.1: Initial release
- 0.2: Caster levels are now visible in the Skills lists of the character sheet (F5). Magic items now use the player level rather than the "skilled" caster level (ie: classic behavior)
- 0.3: Potions now use the player level rather than the "skilled" caster level (ie: classic behavior)