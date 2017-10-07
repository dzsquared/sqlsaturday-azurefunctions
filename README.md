# Utilizing Azure Functions with SQL Server/Azure SQL
Ever spend a day building something that in retrospect should have taken you 15 minutes? I did. Now I'm sharing that project within 15 minutes.
![SQL Sat Logo](/images/pass_sqlsaturday862.jpg "Azure Functions Logo")

## Why Learn about Azure Functions as a data professional?
1. This is a lightning talk, and the logo is a lightning bolt.  ![Azure Functions Logo](/images/AzureFunctions.png "Azure Functions Logo") 
2. Unlock business logic in a variety of languages without building a server, maintaining a server, and writing the foundational code.
C#, F#, Node.JS, Python, PHP, etc... (now Java!)
3. The trigger for the code to execute is handled within the function setup and can be a wide range of relevant and useful events - many of them data related.

    i. Azure Storage, Event Hubs, Service Bus

    ii. Timer

    iii. HTTP request or webhook
4. Azure Functions provide simple way for databases to interact with other data sources while giving an enourmous amount of flexibility.

## Initial Setup
1. Logon to your Azure Portal (portal.azure.com) and create a new Function App.

    a. If you want to enable Application Insights, select the same region for both resources to minimize IO costs.

    b. If you don't enable Application Insights, it's quick to do at a later date.
2. In the function app under *Application Settings*, add the 1 or more connection strings for your SQL databases.

    a. An example SQL Server connection string is `Data Source={some_server};Initial Catalog=sqlsaturday;Integrated Security=False;User Id={your_username};Password={your_password};MultipleActiveResultSets=True`

    b. An example Azure SQL connection string is `Server=tcp:sqlsaturday.database.windows.net,1433;Initial Catalog=sqlsaturday;Persist Security Info=False;User ID={your_username};Password={your_password};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;` You can copy this directly from the server settings in the Azure Portal.
3. Under functions, add a new function from the HttpTrigger - C# template. Name your function wisely, it's currently impossible to rename a function.
4. Your function starts with 2 files, run.csx and function.json.

    a. [Function.json](/blob/master/function.json) contains some of the basic definitions for the function, including what the trigger is.

    b. [Run.csx](/blob/master/run.csx) contains the directives and logic for the program.
5. You'll want to add a [project.json](/blob/master/project.json) file, which allows you to add packages to the function via NuGet.  In this example, we need to include Belgrade.Sql.Client.
6. The beauty of Azure functions is the simplicity of getting right to business logic in the run.csx file. There is a sample [run.csx](/blob/master/run.csx) file in this repository, or you can build it yourself from [codesnippets.csx](/blob/master/codesnippets.csx).
Utilizing run.csx, copy the code into run.csx in the Azure portal.
7. You might already have a test/sample database at your fingertips, but if not - create a SQL Database in Azure. Through the Azure portal you can create an Azure SQL database from the sample AdventureWorksLT database or other backup ([Basic Setup](/images/AzureSQLSample.png)). Azure functions can access both Azure SQL and SQL Server databases.  The sample code utilizes 2 tables, JobNotes (sample text) and JobNotes_Sentiment (storage for results).
8. In your Azure Portal, add a Text Analytics API object ([located under Cognitive Services](/images/textapisetup.png)).  You're going to need the API keys to pass with each request.  Good news! There's a free tier with 5,000 transactions per month. The template JSON for text analytics submission is:


```
{
    "documents": [
        {
            "language": "en",
            "id": "1",
            "text": "This product worked exactly like it was supposed to at first. Instead of an annoyance, a rogue fly getting into the house was almost exciting! After using the product every other day, for a month, The handle (black part) and the racket (yellow part) started becoming a little wobbly. Before long the racket stopped working because of the two pieces disconnecting. The design is either flawed, or I received a faulty product."
        }, {
            "language": "en",
            "id": "2",
            "text": "Wasn't sure what to think. I already own a bug zapper that you hang in your yard and it works pretty well to draw the majority of the pests away from our main seating area, but sometimes there are persistent bugs that just won't leave me or my guests alone. That's when this handy product comes in. I like to give it to my guests to use when they are being annoyed by a bug. Most people find vindication in zapping an insistent mosquito or fly with the Elucto Electric Bug Zapper Fly Swatter."
        }
    ]
}
```



## References

**Azure Functions Developers Reference:** https://docs.microsoft.com/en-us/azure/azure-functions/functions-reference

**Azure Functions C# Reference:** https://docs.microsoft.com/en-us/azure/azure-functions/functions-reference-csharp

**AdventureWorks Data Dictionary:** http://www.sqldatadictionary.com/AdventureWorks2012.pdf

**Azure Text Analytics:** https://azure.microsoft.com/en-us/services/cognitive-services/text-analytics/

**C# - SQL Bulk Copy:** https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlbulkcopy(v=vs.110).aspx

**C# - SQL Command:** https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand(v=vs.110).aspx


# Important!  Sponsors!
![sponsors logos](/images/sponsors.png "sponsors logos")