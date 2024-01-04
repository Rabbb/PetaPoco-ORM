# What is diffrent in this ?

* MSSQL ByPage add "order by peta_rn".
* Delete replace from "@@" to "@".
* Mixed both nature's @params and petapoco's @auto_params.

# patepoco @params use
```cs
// one
db.Fetch<user>($@"select * from user u where u.id = @0 and age > @1", 10, 16);
// list
db.Fetch<user>($@"select * from user u where u.id in (@0) and age > @1", new List<int> {10, 12, 6, 4}, 16);
// dictionary
db.Fetch<user>($@"select * from user u where u.age = @age and u.sex = @sex ", new Dictionary<string, object> { {"age", 18}, {"sex", 0} });

// class, struct
class User {
  public int age;
  public int sex {get;set;}
}

var user = new User();
user.age = 18;
user.sex = 0;
db.Fetch<user>($@"select * from user u where u.age = @age and u.sex = @sex ", user);
```


## error example

1. set params with only a Array.

```cs

// list
db.Fetch<user>($@"select * from user u where u.id in (@0)", new List<int> {10, 12, 6, 4});
// results : users where id in 10, 12, 6, 4.


// array **error example**
db.Fetch<user>($@"select * from user u where u.id in (@0)", new int[] {10, 12, 6, 4});
// results : users where id in 10


// array
db.Fetch<user>($@"select * from user u where u.id in (@0) and age > @1", new int[] {10, 12, 6, 4}, 10);
// results : users where id in 10, 12, 6, 4 and age > 10
```

