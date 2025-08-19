### Rules

| Rule ID | Category | Severity | Notes                                                                                                                 |
|---------|----------|----------|-----------------------------------------------------------------------------------------------------------------------|
| ZRV0001 | Design   | Warning  | Classes should have a Debugger display if it is within a model namespace                                              |
| ZRV0002 | Design   | Error    | Debugger display should have a valid value                                                                            |
| ZRV0003 | Usage    | Error    | Optional property should be expressed as simple property Expressions                                                  |
| ZRV0004 | Usage    | Warning  | Expression does not contain any properties with OptionalPropertyAttribute and should be removed                       |
| ZRV0005 | Usage    | Warning  | Trailing Properties on expression do not contain any properties with OptionalPropertyAttribute and should be trimmed  |
| ZRV0006 | Naming   | Warning  | Variable Names for classes should not contain blacklisted words                                                       |
| ZRV0007 | Design   | Error    | Prevents optional properties from being used in DebuggerDisplays                                                      |
| ZRV0008 | Style    | Warning  | Consider using 'is null' or 'is not null' instead of '==' or '!=' for null checks                                     |
