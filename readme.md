### Rules

| Rule ID | Category | Severity | Notes                                                                                        |
|---------|----------|----------|----------------------------------------------------------------------------------------------|
| ZRV0001 | Design   | Warning  | Classes should have a Debugger display if it is within a model namespace                     |
| ZRV0002 | Design   | Error    | Debugger display should have a valid value                                                   |
| ZRV0003 | Usage    | Error    | Optional Field should be expressed as simple property Expressions                            |
| ZRV0004 | Usage    | Info     | Expression does not contain any properties with OptionalFieldAttribute and should be removed |
| ZRV0006 | Naming   | Warning  | Variable Names for classes should not contain blacklisted words                              |
### Future Ideas

- Code fix for bad optional field expression
- Suggest abstract expression (ie if `RedCard` and `YellowCard` expression is same use `Card` instead)
- Suppress null error and warning for optional field expression
- Warning for page sizes that exceed the max unloaded limit