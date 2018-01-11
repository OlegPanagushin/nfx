# Naming/Coding Standards

**NFX** does not use any 3rd party components but for some DB-access (MongoDB and MySQL are primary targets). 
**NFX** uses very BCL:

* Basic/primitive types: string, ints, doubles, decimal, dates, +Math
* Parallel task library: 25% of features - create, run, wait for completion, Task, Parallel.For/Each
* Collections: List, Dictionary, ConcurrentDictionary, HashSet, Queue
* Threading: Thread, lock()/Monitor, Interlocked*, AutoresetEvent
* Various: Stopwatch, Console, WinForms is used for SOME interactive tests(not needed for operation)
* Some ADO references (Reader/SQLStatement) in segregated data-access components
* Reflection API
* Drawing 2D (Graphics)

**Avoid all non-BCL classes.**

## Naming Conventions

* Instance non-pub fields must begin with m_ i.e. `m_BoxColor`

* Static non-pub fields must begin with s_ i.e. `s_ServerInstance`

* Thread-static non-pub fields must begin with ts_ i.e. `ts_LockList`

* Pub fields: PascalCase and preferably read-only i.e. `BoxColor`

* Pub properties: PascalCase i.e. `BoxColor`

* Pub Methods: PascalCase i.e. `GetPriority()`

* Protected Methods: PascalCase, core virtual overrides Do i.e. : pub `Open()` calls protected virtual `DoOpen()`

* Localizable stirng constants must be moved to StringConsts.cs. **Do not use .NET localization mechanisms**

* Non-localizable string constants must be declared in CONSTS region:

* Numeric Time span values must end with _MS, _SEC, _MIN specifier. NFX does not use TimeSpan type for storing intervals in props/configs

* Configuration Section names: CONFIG_SECT, Configuration attr names: CONFIG_ATTR

* Default values constants should start from DEFAULT_*

* MIN/MAX value bounds should be prefixed with MIN_/MAX_

* Members prefixed with "__" are signifying a hack behaviour which is sometimes needed (see below). Business developers should never call these methods/members

## Code File Structure 

Try to organize code file structure per the spec below, in the particular order:

1. License
2. USINGS - System, NFX, then your namespaces(if any)
3. Region "CONSTS"
4. Region ".ctor" - constructors, and static factories/properties, Destructor() calls
5. Region "Fields/.pvt .flds"
6. Region "Properties" - public properties
7. Region "Pub" - public methods
8. Region "Protected" - protected methods/stubs
9. Region ".pvt/.pvt.impl" - private implementations

Please see EMPTY_CLASS_TEMPLATE.cs under '/Source'

**C# Note:** in C# the access specifiers are not granular enough and in many cases one needs to "un-protect" some otherwise read-only field or property 
(i.e. there is no way to make some member public ONLY to descendants of X). 
While it's true that "internal" is sometimes sufficient, sometimes one needs to create a "hack" setter. 
In these cases please make an internal function that starts with "__" (at least one underscore) and declare it near the field/property of interest: 

```cs
private int m_Magic; 

internal void __setMagic(int val){ m_Magic = val; }

```
This approach (vs just making the whole field internal) allows to signify the "non-normal" case.

## DOs and DONTs

* Never return a null from a string property UNLESS you need to signify the absence of a value (this is a rare case). 
In most cases (98%) return string properties like `get { return m_Name ?? string.Empty; }`

* DO NOT proliferate redundant argument checks that do not reflect the "business logic" - like much of the .NET code does. 
This is because the **NFX** code is a runtime for Aum, in Aum code contracts/args checks are done using aspects. 
No need to write 1000s of IF statements that are not needed. 
For example, do not check for nulls in this method: `mystream.CompressInto(another)`. If the user gets "obj ref" its ok
* DO protect MAJOR methods with arg checks (non null) where the purpose of the argument is not obvious. 
Example: `code.Analyze( lexer, parser)` - if lexer is not null then parser must be supplied as well
* DO NOT raise generic exceptions, derive(directly or indirectly) all exceptions from `NFXException`
* DO NOT use cryptic names
* DO NOT reference 3rd-party (non-clr/system-core) DLLs from **NFX**
* DO NOT bring-in mixed license code
* DO NOT use NUGET or any other package manager. UNISTACK is all about NOT USING packages. 
If you want to use **NFX** in your project which is already "dirty" with 3rd parties - that's fine, but you will have to copy **NFX** DLLs by hand. We will not take NUGET-related contributions into out code base
* DO NOT Rely on the proprietary Microsoft technologies from within **NFX**: WCF, IIS, PowerShell, ASP.NET, MVC, Razor, MS.SQL, Entity, LINQto*, ActiveDirectory, etc.
