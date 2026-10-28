# Changelog

## 1.1.2
- Both commands now only work in sandbox saves (the game's own `FeatureEnables.CanEditTrackCondition` gate, which also allows its debug mode). Other saves get "Only available in sandbox mode."

## 1.1.1
- The wear command is now `/settrackwear` (was `/setwear` in 1.1.0).

## 1.1.0
- New `/setwear [percent]` command: sets every completed track segment to a wear percentage (0% = new, 100% = worn out). Unlike `/repairtrack` it can also make track worse, for testing.

## 1.0.0
- First release: `/repairtrack [condition]`.
