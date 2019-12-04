# AzureHealthPage
Azure cloud service health monitoring page.
Application Insights heartbeat checker

![Image](https://raw.githubusercontent.com/crookedbard/AzureHealthPage/master/AzureCloudServiceHealthMonitoringPage.jpg)

## Guide on how to setup the project for your Azure Cloud Services

Before starting to fill appsettings.json with your Azure Cloud details you need to do some additional work:
1. Register Azure Active Directory app with application permission to access your all application insights resources
  1. Azure Active Directory > App registrations > New registration > Enter name, example: "AdAppForAppInsights" & select "Accounts in this organizational directory only" > Register
  2. Click on newly registered app > API permissions > APIs my organization uses > Application Insights API > Application permissions > Data Read.  Note that this permission requires administrators approval. Because this permission will allow your application to access the data from all of your Azure Cloud Application Insights resources. Read statistics, perform queries. Your Azure Cloud administrator should go to this Azure AD app > API permissions and click on "grant concent" button.
  3. Your AD app > Authentication > check 2 checboxes: Access tokens, ID tokens > Save
2. Setup availability tests for your application insights resources and grant "Reader" Access controll for your registered AD app.
  1. Choose your application insights resources that you would like to monitor service availability. If you don't have any then create at least 1.
  2. Select chosen/created application insights resource > Availability > Add test:
    1. Enter a name
    2. Select test type - URL ping test
    3. Enter your applications URL that you want to monitor the availability
    4. Test frequency > 5 minutes
    5. Test locations > select locations that are important to you and be consistent with them (select the same locations for all of your app insights resources). Write down the locations you have selected.
    6. Configure success criteria.
    7. Create
  3. Click on application insight resource > Access control (IAM) > Add:
    1. Role - Reader
    2. Select - the name of the Azure AD app that you registered in 1st step. "AdAppForAppInsights"
    3. Save
  4. Add test and access for each application insight resource that you want to monitor

Open appsettings.json and fill the required fields:
1. Domain - Get this from: portal.azure.com > Azure Active Directory > Overview > First line before you company name
2. ClientId - Get this from: portal.azure.com > Azure Active Directory > App registrations > Your AD APP which has permissions to access all APP Insights > Overview > Application (client) ID
3. ClientSecret - Get this from: portal.azure.com > Azure Active Directory > App registrations > Your AD APP which has permissions to access all APP Insights > Certificates & secrets > Client secrets > Create & copy secret
4. StartCountAvailabilityFromDate - the date you want to start calculating the app availability (the date when you added application insights availability tests)
5. AvgAvailabilityForDays - Default 30, calculates average availability for 30 days (if todays date minus 30 days is lower then StartCountAvailabilityFromDate -  then the value will be lowered. Meaning that you will see the real SLA for 30 days after those 30 days will pass, till then you will see SLA for 1,2,3 ... days).
6. AvgLatencyPerMinutes - default 10. Calculates average per 10 minutes latency from specific location.
7. MinAvailability - default 99.9. Your SLA %. If the value is lower than specified then the percentage will be displayed in red.
8. MaxLatency - default 800. Your maximum allowed latency. If the value is higher than specified then the number will be displayed in red.
9. Applications - enter all the resource names of your application insights resources that you have configured availability tests and granted reader access for Azure AD app.
10. Locations - enter the same locations that you have written down before when you created the availability tests.
11. ApplicationInsights:InstrumentationKey (optional) - if you want to monitor the healths app logs then configure application insights for this application. (Visual studio > Right click on project > Configure application insights )
 
