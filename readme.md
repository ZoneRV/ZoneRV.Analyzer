### Rules

| Rule ID | Category   | Severity    | Has Code Fix | Notes                                                                                                                |
|---------|------------|-------------|:------------:|----------------------------------------------------------------------------------------------------------------------|
| ZRV0003 | Usage      | Error       |              | Optional property should be expressed as simple property Expressions                                                 |
| ZRV0004 | Usage      | Warning     |              | Expression does not contain any properties with OptionalPropertyAttribute and should be removed                      |
| ZRV0005 | Usage      | Warning     |              | Trailing Properties on expression do not contain any properties with OptionalPropertyAttribute and should be trimmed |
| ZRV0007 | Design     | Error       |              | Prevents optional properties from being used in DebuggerDisplays                                                     |

### HubSpot Rules

| Rule ID | Category | Severity | Has Code Fix | Notes                                                                                                             |
|---------|----------|----------|:------------:|-------------------------------------------------------------------------------------------------------------------|
| ZRVHS01 | Usage    | Error    |              | Parameters with ExpectConstPropertiesFrom or attributes should use constants from the correct type.               |
| ZRVHS02 | Usage    | Warning  |              | Parameters with ExpectConstPropertiesFrom or attributes should use constants over string literals when available. |
| ZRVHS03 | Design   | Error    |              | All HubSpotEntityBase must have an ObjectTpyeAttribute.                                                           |

### Removed Rules

| Rule ID | Notes                                                                             | Reason                                                            |
|---------|-----------------------------------------------------------------------------------|-------------------------------------------------------------------|
| ZRV0001 | Classes should have a Debugger display if it is within a model namespace          | Made redundant by [L.O.C.A.T.](https://github.com/Liamth99/LOCAT) |
| ZRV0002 | Debugger display should have a valid value                                        | Made redundant by [L.O.C.A.T.](https://github.com/Liamth99/LOCAT) |
| ZRV0006 | Variable Names for classes should not contain blacklisted words                   | Made redundant by [L.O.C.A.T.](https://github.com/Liamth99/LOCAT) |
| ZRV0008 | Consider using 'is null' or 'is not null' instead of '==' or '!=' for null checks | Made redundant by [L.O.C.A.T.](https://github.com/Liamth99/LOCAT) |
| ZRV0009 | Async Method names should end with Async                                          | Made redundant by [L.O.C.A.T.](https://github.com/Liamth99/LOCAT) |