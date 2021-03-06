---
title: Queryable endpoints
resource: true
---

Saule supports queryable endpoints. These endpoints allow users of
your API to specify constraints on the results. Saule will automatically
apply the query to the `IQueryable<T>` or `IEnumerable<T>` you return from
your action methods.

Saule uses LINQ internally, so queries will be evaluated lazily. If you use
e.g. Entity Framework to generate the `IQueryable<T>`s, the query is executed
on the database, rather than in memory.

To enable queries on an endpoint, simply add the `AllowsQueryAttribute` to
the action method:

```csharp
[HttpGet]
[AllowsQuery]
[Route('api/people')]
public IQueryable<Person> GetPeople()
{
    return Database.People.FindAll();
}
```

> **Note**: Saule only supports the `sort` and `filter` query parameters.
> The same attribute may support other queries in the future.

```
GET mywebsite.com/api/people?sort=last-name,-age
```

```json
{
  "data": [
    {
      "type": "person",
      "id": "0",
      "attributes": {
        "first-name": "Sheba",
        "last-name": "Bockman",
        "age": 20
      },
      "links": {
        "self": "http://example.com/people/0/"
      }
    },
    {
      "type": "person",
      "id": "3",
      "attributes": {
        "first-name": "Eugenie",
        "last-name": "Bockman",
        "age": 6
      },
      "links": {
        "self": "http://example.com/people/3/"
      }
    },
    {
      "type": "person",
      "id": "4",
      "attributes": {
        "first-name": "Larissa",
        "last-name": "Summers",
        "age": 70
      },
      "links": {
        "self": "http://example.com/people/4/"
      }
    },
    {
      "type": "person",
      "id": "1",
      "attributes": {
        "first-name": "Vergie",
        "last-name": "Summers",
        "age": 47
      },
      "links": {
        "self": "http://example.com/people/1/"
      }
    },
    {
      "type": "person",
      "id": "2",
      "attributes": {
        "first-name": "Francisco",
        "last-name": "Summers",
        "age": 41
      },
      "links": {
        "self": "http://example.com/people/2/"
      }
    }
  ],
  "links": {
    "self": "http://example.com/api/people?sort=last-name,-age"
  }
}
```

## Customizing filtering expressions

Sometimes you want to do something specific when a client specifies a filter query parameter.
For example, you might want to do case insensitive filtering for strings, so `/people?filter[name]=smith`
will not return an empty result set.

To do this in Saule, you can set *query filter expressions* for specific types in your JSON API configuration:

```csharp
public static void Register(HttpConfiguration config)
{
    var jsonApiConfig = new JsonApiConfiguration();
    jsonApiConfig.QueryFilterExpressions.Set(new CaseInsensitiveStringQueryFilterExpression());

    config.ConfigureJsonApi(jsonApiConfig);
}
```

If you want to do something more specific, you can also directly specify a lambda expression. For example,
substring search can be implemented easily as follows:

```csharp
public static void Register(HttpConfiguration config)
{
    var jsonApiConfig = new JsonApiConfiguration();
    jsonApiConfig.QueryFilterExpressions.Set<string>((property, filter) => property.Contains(filter));

    config.ConfigureJsonApi(jsonApiConfig);
}
```

If you set the query filter expression for a base class, it will also apply to all child classes. For example,
to overwrite the filter expression for all types, simply set it for `System.Object`:

```csharp
public static void Register(HttpConfiguration config)
{
    var jsonApiConfig = new JsonApiConfiguration();
    jsonApiConfig.QueryFilterExpressions.Set<object>((property, filter) => property != filter);

    config.ConfigureJsonApi(jsonApiConfig);
}
```

This will make filters black list-like, rather than the default white list.

If you want even more control, you can implement the `IQueryFilterExpression` interface. In addition to all
of the above, this also gives you access to the `PropertyInfo` of the property that is being filtered on. This
way, you can even apply specific ways of filtering to specific filters.

Say for example that you want to do substring search for properties called `Name`, and the default equality
comparison otherwise:

```csharp
public class NameSubstringQueryFilterExpression : DefaultQueryFilterExpression<string>
{
    public override Expression<Func<string, string, bool>> GetForProperty(PropertyInfo property)
    {
        if (property.Name == "Name")
        {
            return (prop, filter) => prop.Contains(filter);
        }

        return base.GetForProperty(property);
    }
}
```

You can then use it as before:

```csharp
public static void Register(HttpConfiguration config)
{
    var jsonApiConfig = new JsonApiConfiguration();
    jsonApiConfig.QueryFilterExpressions.Set(new NameSubstringQueryFilterExpression());

    config.ConfigureJsonApi(jsonApiConfig);
}
```