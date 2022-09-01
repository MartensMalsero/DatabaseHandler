# DatabaseHandler
Simple database handler for connections to mysql database. Easy to use functions for select, update, insert, delete and many more.

# How to use

```
// You can set debug to true or false to get console messages or not
_ = new DatabaseController(Host, Port, DB, Username, Password, Debug);

// and then you can call the functions as follows
DatabaseController.SelectSQL(...)
DatabaseController.UpsertSQL(...)
```
