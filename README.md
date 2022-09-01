# DatabaseHandler
Simple database handler for connections to mysql database. Easy to use functions for select, update, insert, delete and many more.

# How to use

```
// You can set debug to true or false to get console messages or not

_ = new DatabaseController(Host, Port, DB, Username, Password, Debug);

```

```
// Then you can call the functions as follows

DatabaseController.SelectSQL(...)
DatabaseController.UpsertSQL(...)
```

```
// For DatabaseController.SelectSQL(...) you have to catch the result as a DataTable, for example

using DataTable dt = DatabaseController.SelectSQL("test_table", new object[] {"id"});
if (dt.Rows.Count > 0)
{
    foreach (DataRow dr in dt.Rows)
    {
        Console.WriteLine(dr["ID"]);
    }
}
```
