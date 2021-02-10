# SlowSqlDataReaderDemo

This repo repros https://github.com/dotnet/SqlClient/issues/593.

I found this bug while investigating https://github.com/github/c2c-actions-platform/issues/2579.

The bug exists for both `System.Data.SqlClient` and `Microsoft.Data.SqlClient`.

To repro, first do `insert into tbl_RegistryItems values (1, 'SqlDataReader', 'Test', 'x')` in SSMS

Then in the console app, try this:

```
write 4000000
read sync
read async
read async sa
```

Note that the `read async` step takes about 10 seconds but the `sync` and `async sa` (SequentialAccess) are much faster.  

Try switching the library of the SqlClient and the result is the same.

Note the `O(N^2)` behavior if you change the number of characters you're writing.
