# CodeGenAPI
An API approach to my code generation tooling developed over the years

The Default ASP.NET Core project sets up a series of endpoints that can be used to generate code.
Employing SWAGGER to document the API and provide a UI for testing the API.

Example of the API in action
![](Images/ScreenShot1.png)
![](Images/ScreenShot2.png)
![](Images/ScreenShot3.png)

Most of the endpoints look for a parameter that represents the
connection string to the database.  
This can be a connection string or a name of a connection string in the appsettings.json file.  

    "Settings": {  
        "Databases": {
            "DBwSQL_Login":"Data Source=(LOCAL); Initial Catalog=OPENCUDmFULL; User ID=sa; Password=P@ssw0rd",
            "DBwSSPI_Login": "Data Source=(LOCAL); Initial Catalog=OPENCUDmFULL; integrated security = SSPI"
        }
    }

(It looks for the supplied parameter starting with the typical bits of a full connection string. Otherwise it will try to find named connection string in the app settings file)

A Typical usage of the API through the Swagger Interface
After Clicking on the Try It Out button, you can enter the connection string and click Execute.  
![](Images/ScreenShot4.png)
Yielding the following results
![](Images/ScreenShot5.png)

Which as the name of that particular endpoint implies, return a json array of the table and view names in the supplied database connection.  


# Notes
YOU Might have to do
chrome://flags/#allow-insecure-localhost

to allow Insecure localhost crap
