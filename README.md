
![Logo](https://miro.medium.com/v2/resize:fit:640/format:webp/1*rrSe6zHPqZtMgZ2rou_W4w.png)

# Implement Redis Cache in ASP.NET Core Web API

## What is Redis?

Redis (Remote Dictionary Server) is an open-source, in-memory data structure store that serves as a database, cache, and message broker. In the context of ASP.NET Core Web API, Redis is commonly used as a caching layer (distributed caching solution) to improve application performance by storing frequently accessed data in memory, thus reducing the need to access the data from slower storage mediums like Databases or External APIs. 

It is the first-choice caching solution among ASP.NET Core developers due to its high speed, scalability, and support for advanced data structures. 

## What Is Caching?

The cache is the memory storage used to store the frequent access data in the temporary storage; it will improve the performance drastically, avoid unnecessary database hits, and store frequently used data into the buffer whenever we need it.

# Distributed Caching

## What is Distributed Caching?

Distributed caching is a caching mechanism that shares cache data across multiple server instances. This type of caching is crucial in environments where applications are deployed in a load-balanced server farm or across multiple servers. It helps in maintaining data consistency and improves the performance and scalability of applications by providing a fast way of accessing frequently used data from a shared cache, reducing the load on database server.

## Distributed Caching Architecture:

Let us first understand the architecture of Distributed Caching, and then we will see how to implement distributed caching using Redis cache in our ASP.NET Core Web API Application. 

## Screenshots

![App Screenshot](https://miro.medium.com/v2/resize:fit:1120/0*HaRIMwm37jEngfKh.png)

# Setting up Redis on Windows 10/11

Redis is primarily designed to run on Linux, macOS, and BSD systems. Officially, it doesn't support Windows. However, developers can use alternative methods to run Redis on Windows machines. Ideally, developers use a secondary machine (mostly Linux) that takes care of caching, where Redis runs and serves as a cache memory for multiple applications. 
Let us proceed and see how we can set up Redis on a Windows 10/11 machine:

## Download Redis for Windows: 

Please visit the following GitHub URL to download a Windows-compatible version of Redis supported by Microsoft: 
https://github.com/microsoftarchive/redis/releases/tag/win-3.0.504

Once you visit the above URL, it will open the below page. From this page, download the Redis-x64-3.0.504.zip file from the GitHub Repo on your machine 

# Basic Redis Commands: 

For all commands, please refer to this link- https://www.geeksforgeeks.org/complete-guide-to-redis-commands/ 

The following are some of the Basic Commands:
- Set a Value: SET Product "water glass"
- Get a Value: GET Product
- Delete a Key: DEL Product
- EXPIRE a Key: EXPIRE key seconds ( EXPIRE Product 2 )
- PERSIST key: PERSIST key ( Removes the expiration from a key PERSIST Product  )

# Project setup:

So, first, create a new ASP.NET Core Web API project with the name RedisCaching. Once you create the project, we need to add the following Packages, which are for Entity Framework Core to work with SQL Server database. You can install these packages using the following commands in the Package Manager Console.

// communicate with a SQL Server database, allowing you to perform data operations such as CRUD (Create, Read, Update, Delete)
```bash
Install-Package Microsoft.EntityFrameworkCore.SqlServer 
```
// managing EF Core migrations
```bash
Install-Package Microsoft.EntityFrameworkCore.Tools 
```
# Configure Connection String:
Next, we need to add the connection string in appsettings.json file. So, modify the appsettings.json file as follows. Here, the Entity Framework Core will create the database named ProductsDB if it is not already created in the SQL server.
```bash
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=test;Database=ProductsDB;Trusted_Connection=True;TrustServerCertificate=True;"
    },
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*"
}
```

# Create Database Migration:

Open the Package Manager Console and then execute the Add-Migration command to create a new Migration file and then execute the Update-Database to apply the migration to the database as follows. 





