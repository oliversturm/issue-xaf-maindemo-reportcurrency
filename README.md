# Issue: Report Currency in PDFs retrieved from Web API service

Steps:

* Clone the repo
* Create a file `.env` in the repo root. Add a line with your DevExpress Nuget Feed key:

```env
DXNUGETKEY=... 
```

* Start the docker setup:

```bash
> docker compose up --build -d
```

* After the build finishes and the containers start, allow some time -- the two containers for the sample app are configured to restore their dependencies and run their processes, this can take a short while after startup. Watch the logs to see when the services are ready.

* Access http://localhost:5010 in a browser for the demo app. 

* Create a test report using the `Paycheck` data type. Include some of the Employee "pay" and "tax" related fields, which contain currency values.

* Examine the report preview in the Blazor app, it will show the correct `$` currency symbol

* Access http://localhost:5010/swagger for the API published by the Blazor app

* Access the test report through the Blazor App API, by calling `/api/Report/DownloadByName` (can use the Swagger UI) -- it will still have the correct `$` symbol

* Access http://localhost:6010/swagger for the API published by the separate Web API project.

* Access the test report through the Web API -- it will show the wrong currency symbol now (`¤`).

## How the demo was created

* I copied the "Main Demo" from my Windows installation and removed the projects I didn't need.
* I also deactivated everything related to authorization, to simplify test access. I also deactivated HTTPS for the same reason.
* I added a Web API project using the wizard in Windows.
* I changed all the projects to target .NET 7.
* At this point I tried to run both the Blazor and the Web API projects on Windows, and test the report functionality. I found that I was able to retrieve correct reports from both the Blazor app and the Web API endpoint.
* I moved the demo out of Windows, set up the Dockerfiles and the docker-compose config.
* I added the `DevExpress.Drawing.Skia` references
* I modified the SQL connection strings and the listening config, so that the docker containers would work together as intended.
* ... I believe that was it. Please let me know in case you find any other relevant changes, I can confirm whether I made them on purpose.

## Bonus bug: GDI dependency of demo report "Employee List Report"

Try to bring up the preview in the the Blazor app for the "Employee List Report" that is automatically created by the Main Demo. You will see errors, apparently due to some GDI dependency of that report.

```text
Error: System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation.
 ---> System.TypeInitializationException: The type initializer for 'Gdip' threw an exception.
 ---> System.DllNotFoundException: Unable to load shared library 'libgdiplus' or one of its dependencies.
 ```
